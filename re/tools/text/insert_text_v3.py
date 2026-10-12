"""Insere as traducoes do JSON (dump_text_v3.py) na ROM do Medabots (E).

    python insert_text_v3.py ROM_ENTRADA.gba game_text.json ROM_SAIDA.gba [--reflow]

Use como entrada a ROM ja exportada pelo Toolkit com a fonte estendida (aba Font): as larguras dos acentos e de
o-til/A-til/a-til/O-til vem de la. Este script nao mexe mais na tabela de larguras.

Correcoes sobre a v2:
- caractere fora da fonte vira erro com a lista (a v2 descartava calado);
- texto gravado depois do ultimo byte usado acima de 8 MB, como o Toolkit (a v2 usava 0x850000 fixo);
- strings iguais gravadas uma vez so;
- <0xNN> grava o byte cru (argumento do <?>);
- --reflow: reorganiza <NL>/<NB> das caixas de dialogo pela largura real (208 px, 2 linhas por caixa).
"""
import argparse
import json
import re
import struct
import sys

import medtext

GBA_BASE = 0x08000000
ROM_SIZE = 0x1000000
ORIGINAL_SIZE = 0x800000
GAP = 0x10

# Codigo do jogo -> tabela de larguras da fonte normal
WIDTH_TABLES = {b'A8BP': 0x3B4E08, b'A9BP': 0x3B4E28}

COMMANDS = {'I': [0xF8], '?': [0xFA], 'NB': [0xFC], 'NL': [0xFD], 'ENDLST': [0xFE]}


def encode(text, where):
    out = bytearray()
    for m in re.finditer(r'<([^<>]*)>|(.)', text, re.S):
        cmd, ch = m.group(1), m.group(2)
        if ch is not None:
            if ch not in medtext.CODE:
                raise ValueError(f"{where}: caractere fora da fonte '{ch}'")
            out.append(medtext.CODE[ch])
        elif cmd in COMMANDS:
            out += bytes(COMMANDS[cmd])
        elif cmd.startswith('SPEED:'):
            out += bytes([0xF7, int(cmd[6:])])
        elif cmd.startswith('MEM:'):
            out += bytes([0xF9, int(cmd[4:])])
        elif cmd.startswith('PORTRAIT:'):
            out += bytes([0xFB] + [int(p) for p in cmd[9:].split(',')])
        elif cmd.startswith('END:'):
            out += bytes([0xFF, int(cmd[4:])])
        elif cmd == 'END':
            out += bytes([0xFF, 0])
        elif cmd.lower().startswith('0x'):
            out.append(int(cmd, 16))
        else:
            raise ValueError(f"{where}: tag desconhecida <{cmd}>")
    return bytes(out)


def first_free(rom):
    last = len(rom) - 1
    while last >= ORIGINAL_SIZE and rom[last] == 0xFF:
        last -= 1
    offset = max(last + 1 + GAP, ORIGINAL_SIZE)
    return (offset + 3) & ~3


def is_dialogue(text):
    return '<PORTRAIT' in text or '<NB>' in text


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('rom_in')
    ap.add_argument('json')
    ap.add_argument('rom_out')
    ap.add_argument('--reflow', action='store_true', help='reorganiza linhas/caixas dos dialogos pela largura')
    args = ap.parse_args()

    with open(args.rom_in, 'rb') as f:
        rom = bytearray(f.read())
    if len(rom) < ROM_SIZE:
        rom += b'\xFF' * (ROM_SIZE - len(rom))

    widths = WIDTH_TABLES.get(bytes(rom[0xAC:0xB0]))
    if widths is not None and rom[widths + 0x4F * 2] == 0:
        print("AVISO: larguras 0x4F-0x7F zeradas. Exporte a ROM pelo Toolkit com 'Use extended font' antes.")
    elif widths is not None:
        # reflow com as larguras da propria ROM (fonte estendida ou fonte PT-BR 100%)
        medtext.W_MAIN[:] = rom[widths:widths + 256:2]
        medtext.W_ITAL[:] = rom[widths + 0x200:widths + 0x300:2]

    with open(args.json, encoding='utf-8') as f:
        data = json.load(f)

    jobs, errors, warnings = [], [], 0
    for name, table in data["tables"].items():
        ptr_offset = int(table["ptr_offset"], 16)
        for e in table["strings"]:
            text = medtext.clean(e.get("translation", "").strip())
            if not text:
                continue
            where = f"{name}[{e['index']}]"
            if args.reflow and is_dialogue(text):
                text = medtext.reflow(text)
            original = medtext.clean(e["original"]).replace('<?>ä', '<?><0x50>')
            if [t for t in medtext.tags(text) if t != '<NB>'] != [t for t in medtext.tags(original) if t != '<NB>']:
                print(f"AVISO {where}: tags diferentes do original")
                warnings += 1
            try:
                jobs.append((ptr_offset + 4 * e["index"], encode(text, where) + b'\xFE'))
            except ValueError as ex:
                errors.append(str(ex))

    if errors:
        print("\n".join(errors))
        sys.exit(f"{len(errors)} erro(s); nada gravado.")

    cursor = first_free(rom)
    placed = {}
    for slot, payload in jobs:
        if payload not in placed:
            if cursor + len(payload) > len(rom):
                sys.exit("Sem espaco na ROM.")
            rom[cursor:cursor + len(payload)] = payload
            placed[payload] = cursor
            cursor += len(payload)
        struct.pack_into('<I', rom, slot, placed[payload] + GBA_BASE)

    with open(args.rom_out, 'wb') as f:
        f.write(rom)
    print(f"{len(jobs)} strings ({len(placed)} unicas), {warnings} aviso(s), texto ate 0x{cursor:06X}: {args.rom_out}")


if __name__ == '__main__':
    main()
