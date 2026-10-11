"""Compiles the injected C (inject/patches.json), places it in the ROM's free space and writes the hooks.

    python inject/tools/build.py IN.gba OUT.gba [--gcc DIR]

Free space starts after the last non-0xFF byte (at least 8 MB, + 0x10 gap, 4-aligned), as in the Toolkit's
FreeSpaceAllocator; an 8 MB ROM grows to the next MB. Each hook overwrites the start of a game function with
    [Thumb] bx pc ; (nop if needed)   [ARM] ldr ip, [pc] ; bx ip ; .word target|1
(14-16 bytes; ip is a scratch register at a function entry), so the game's callers reach the C function.
"pointers" replace a word in a ROM table (e.g. a state or handler table) with a C function's address | 1.
Optional sets in patches.json ("optional") are added with --with NAME, or by themselves when the ROM holds their
"auto_marker": the table after it is then passed to the linker as gNaviMoldTable (Navi bots from the Toolkit).
"""
import argparse
import json
import os
import struct
import subprocess
import sys
import tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent
INJECT = HERE.parent
sys.path.insert(0, str(HERE))
import gen_symbols  # noqa: E402

GBA = 0x08000000
# -mlong-calls: the injected code sits more than 4 MB away from the game's code, out of BL range.
CFLAGS = ["-std=gnu11", "-mthumb", "-mthumb-interwork", "-mlong-calls", "-mcpu=arm7tdmi", "-O2", "-ffreestanding", "-fno-builtin",
          "-nostdlib", "-fno-pic", "-ffunction-sections", "-Wall", "-Wextra", "-Werror"]


def run(cmd):
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        sys.exit(f"command failed: {' '.join(map(str, cmd))}\n{result.stdout}{result.stderr}")
    return result.stdout


def free_space_start(rom):
    end = len(rom)
    while end > 0 and rom[end - 1] == 0xFF:
        end -= 1
    return (max(end, 0x800000) + 0x10 + 3) & ~3


def hook_bytes(at, target):
    out = bytearray()
    if at % 4 == 0:
        out += struct.pack("<HH", 0x4778, 0x46C0)       # bx pc ; nop  -> ARM at at+4
    else:
        out += struct.pack("<H", 0x4778)                # bx pc -> ARM at at+2 (aligned)
    out += struct.pack("<III", 0xE59FC000, 0xE12FFF1C, target | 1)   # ldr ip,[pc] ; bx ip ; .word target
    return bytes(out)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("rom_in")
    parser.add_argument("rom_out")
    parser.add_argument("--gcc", default=os.environ.get("ARM_GCC_BIN", ""), help="folder with arm-none-eabi-*")
    parser.add_argument("--with", dest="extra", action="append", default=[], help="optional set from patches.json")
    args = parser.parse_args()
    tool = lambda name: str(Path(args.gcc) / name) if args.gcc else name
    config = json.loads((INJECT / "patches.json").read_text(encoding="utf-8"))
    config.setdefault("pointers", [])
    rom = bytearray(Path(args.rom_in).read_bytes())
    defsyms = []
    for name, extra in config.get("optional", {}).items():
        marker = extra.get("auto_marker")
        at = rom.find(marker.encode()) if marker else -1
        if at >= 0 and name not in args.extra:
            args.extra.append(name)
            defsyms.append(f"-Wl,--defsym=gNaviMoldTable=0x{GBA + at + len(marker):08X}")
            print(f"{marker} found at 0x{GBA + at:08X}: adding {name}")
    for name in args.extra:
        extra = config.get("optional", {}).get(name) or sys.exit(f"no optional set {name}")
        for key in ("sources", "hooks", "pointers"):
            config.setdefault(key, []).extend(extra.get(key, []))

    with tempfile.TemporaryDirectory() as tmp:
        tmp = Path(tmp)
        gen_symbols.main(tmp / "symbols.ld")
        objects = []
        for source in config["sources"]:
            obj = tmp / (Path(source).stem + ".o")
            run([tool("arm-none-eabi-gcc"), *CFLAGS, "-I", str(INJECT / "include"), "-c", str(INJECT / source), "-o", str(obj)])
            objects.append(str(obj))

        base = GBA + free_space_start(rom)
        (tmp / "link.ld").write_text(f"""
INCLUDE "{(tmp / 'symbols.ld').as_posix()}"
SECTIONS {{
  . = 0x{base:08X};
  .text : {{ *(.text*) *(.rodata*) }}
  .data : {{ *(.data*) }}
  .bss : {{ *(.bss*) *(COMMON) }}
  /DISCARD/ : {{ *(.comment) *(.ARM.attributes) }}
}}
ASSERT(SIZEOF(.data) == 0 && SIZEOF(.bss) == 0, "injected code cannot have writable globals: use INJECT_RAM")
""", encoding="utf-8")
        elf = tmp / "inject.elf"
        run([tool("arm-none-eabi-gcc"), *CFLAGS, "-T", str(tmp / "link.ld"), "-Wl,--gc-sections", "-Wl,-e,0", *defsyms,
             *[f"-Wl,--undefined={entry['to']}" for entry in config["hooks"] + config["pointers"]], *objects, "-o", str(elf)])
        binary = tmp / "inject.bin"
        run([tool("arm-none-eabi-objcopy"), "-O", "binary", "-j", ".text", str(elf), str(binary)])
        symbols = {}
        for line in run([tool("arm-none-eabi-nm"), str(elf)]).splitlines():
            parts = line.split()
            if len(parts) == 3:
                symbols[parts[2]] = int(parts[0], 16)
        code = binary.read_bytes()

    offset = base - GBA
    if offset + len(code) > len(rom):
        size = (offset + len(code) + 0xFFFFF) & ~0xFFFFF
        if size > 0x2000000:
            sys.exit("the ROM would pass 32 MB")
        rom += b"\xFF" * (size - len(rom))
    rom[offset:offset + len(code)] = code
    print(f"code: {len(code)} bytes at 0x{base:08X}")

    for hook in config["hooks"]:
        at = int(hook["at"], 16)
        target = symbols.get(hook["to"])
        if target is None:
            sys.exit(f"hook target {hook['to']} not found in the compiled code")
        patch = hook_bytes(at, target)
        if len(patch) > hook.get("max_bytes", 16):
            sys.exit(f"hook at 0x{at:08X} needs {len(patch)} bytes, only {hook['max_bytes']} allowed")
        rom[at - GBA:at - GBA + len(patch)] = patch
        print(f"hook: 0x{at:08X} -> {hook['to']} at 0x{target:08X} ({len(patch)} bytes)")

    for pointer in config["pointers"]:
        at = int(pointer["at"], 16)
        target = symbols.get(pointer["to"])
        if target is None:
            sys.exit(f"pointer target {pointer['to']} not found in the compiled code")
        old = struct.unpack_from("<I", rom, at - GBA)[0]
        struct.pack_into("<I", rom, at - GBA, target | 1)
        print(f"pointer: 0x{at:08X} 0x{old:08X} -> {pointer['to']} 0x{target | 1:08X}")

    Path(args.rom_out).write_bytes(rom)
    print(f"-> {args.rom_out}")


if __name__ == "__main__":
    main()
