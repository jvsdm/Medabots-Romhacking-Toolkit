"""
mold_lab.py - Moldes novos (50-63) com animacao de movimento: teste de que o bloqueio era so de dados.

Formato (Metabee E; ver docs secao 15):
- Tabela de moldes 0x3D1D78: 50 ponteiros (0 nulo, 1-2 raizes, 3-6 juntas na RAM) para registros de 0x18 B:
  +0x00 lista de artes, +0x0F/+0x10 moldes filhos, +0x11 paleta, +0x12/+0x13 largura/altura, +0x15 forma do OBJ.
- Roteiros de movimento (59): ponteiros A 0x3D1E40, B 0x3D1F2C, C 0x3D2018. C = duracao de cada quadro-chave
  (bytes, fim 0; N = quantidade). A = s16 [molde 1..49][N] (angulo, 1/65536 volta). B = (s8 x, s8 y) [molde][N].
  O tocador (0x08047428) percorre 63 moldes: para 50-63 lia alem dos 49 registros de cada roteiro.

Uso: python tools/mold_lab.py IN.gba OUT.gba [--sem-movimento]
Faz a cabeca do bot 0 (NOCTOBAT, moldes 7->8) usar clones 50->51; com movimento, as linhas 50/51 copiam 7/8.
"""
import struct
import sys

GBA = 0x08000000
MOLDS, MOLD_COUNT, NEW_MOLDS = 0x3D1D78, 50, 64
MOTION_A, MOTION_B, MOTION_C, SCRIPTS = 0x3D1E40, 0x3D1F2C, 0x3D2018, 59
ROWS_OLD, ROWS_NEW = 49, 63          # moldes 1..49 -> 1..63
PREVIEW_HEAD = 0x7EE70               # 16 B por bot: entradas u32 (molde bits 0-5)
MIRROR_MAP = 0x81490                 # 50 B: molde -> molde no lado espelhado (lido em 0x08046626)


def u32(rom, at):
    return struct.unpack_from("<I", rom, at)[0]


def put32(rom, at, value):
    struct.pack_into("<I", rom, at, value)


class Space:
    def __init__(self, rom):
        self.rom = rom
        end = len(rom)
        while end and rom[end - 1] == 0xFF:
            end -= 1
        self.at = (max(end, 0x800000) + 0x10 + 3) & ~3

    def store(self, data):
        at = self.at
        if at + len(data) > len(self.rom):
            self.rom.extend(b"\xFF" * (((at + len(data) + 0xFFFFF) & ~0xFFFFF) - len(self.rom)))
        self.rom[at:at + len(data)] = data
        self.at = (at + len(data) + 3) & ~3
        return at


def loaded_literals(rom, value):
    """Palavras de literal pool (lidas por ldr rX,[pc]) iguais a value, no codigo."""
    sites = []
    packed = struct.pack("<I", value)
    at = rom.find(packed, 0, 0x80000)
    while at >= 0:
        if at % 4 == 0 and any(
                (u16 >> 11) == 0b01001 and ((pc + 4) & ~3) + (u16 & 0xFF) * 4 == at
                for pc in range(max(0, at - 1020), at, 2)
                for u16 in [struct.unpack_from("<H", rom, pc)[0]]):
            sites.append(at)
        at = rom.find(packed, at + 1, 0x80000)
    return sites


def expand_motion(rom, space, row_source):
    """Cada roteiro ganha linhas para os moldes 50-63; row_source[molde novo] = molde copiado (o resto zera)."""
    done = {}
    for script in range(SCRIPTS):
        c = u32(rom, MOTION_C + 4 * script) - GBA
        keys = 0
        while rom[c + keys]:
            keys += 1
        if keys == 0:
            continue
        for table, width in ((MOTION_A, 2), (MOTION_B, 2)):
            old = u32(rom, table + 4 * script) - GBA
            if (table, old) not in done:
                row = keys * width
                data = bytearray(rom[old:old + ROWS_OLD * row]) + bytearray(row * (ROWS_NEW - ROWS_OLD))
                for new, src in row_source.items():
                    data[(new - 1) * row:new * row] = rom[old + (src - 1) * row:old + src * row]
                done[(table, old)] = space.store(bytes(data))
            put32(rom, table + 4 * script, GBA + done[(table, old)])
    return len(done)


def main():
    src, dst = sys.argv[1], sys.argv[2]
    with_motion = "--sem-movimento" not in sys.argv
    rom = bytearray(open(src, "rb").read())
    space = Space(rom)

    # Tabela de moldes com 64 posicoes (o que segue a original sao os ponteiros A do movimento).
    table = bytearray(rom[MOLDS:MOLDS + 4 * MOLD_COUNT]) + bytearray(4 * (NEW_MOLDS - MOLD_COUNT))
    def record(mold):
        return bytearray(rom[u32(rom, MOLDS + 4 * mold) - GBA:][:0x18])
    clone7, clone8 = record(7), record(8)
    clone7[0x10] = 51                       # filho do 7 era o 8: o clone aponta para o clone
    put32(table, 4 * 50, GBA + space.store(bytes(clone7)))
    put32(table, 4 * 51, GBA + space.store(bytes(clone8)))
    new_table = GBA + space.store(bytes(table))
    sites = loaded_literals(rom, GBA + MOLDS)
    for at in sites:
        put32(rom, at, new_table)
    print(f"moldes: tabela -> {new_table:#010x} ({len(sites)} literais)")

    # Mapa de espelhamento (lado direito da batalha): 50 bytes molde -> molde espelhado (troca os bracos
    # 14-16 <-> 17-19). Os novos moldes viravam 0 (nulo) do lado direito; ganham entrada propria.
    mirror = bytearray(rom[MIRROR_MAP:MIRROR_MAP + MOLD_COUNT]) + bytes(range(MOLD_COUNT, NEW_MOLDS))
    new_mirror = GBA + space.store(bytes(mirror))
    sites = loaded_literals(rom, GBA + MIRROR_MAP)
    for at in sites:
        put32(rom, at, new_mirror)
    print(f"espelho: mapa -> {new_mirror:#010x} ({len(sites)} literal)")

    # Cabeca do bot 0: entradas com molde 7/8 passam a 50/51.
    for k in range(4):
        at = PREVIEW_HEAD + 4 * k
        entry = u32(rom, at)
        if entry & 0x3F in (7, 8):
            put32(rom, at, (entry & ~0x3F) | {7: 50, 8: 51}[entry & 0x3F])
    print("bot 0 cabeca:", [hex(u32(rom, PREVIEW_HEAD + 4 * k)) for k in range(4)])

    if with_motion:
        print("movimento: blocos novos", expand_motion(rom, space, {50: 7, 51: 8}))
    open(dst, "wb").write(rom)
    print("->", dst)


if __name__ == "__main__":
    main()
