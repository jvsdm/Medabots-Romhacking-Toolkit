"""Extrai o texto do Medabots (E) para JSON (formato do insert_text_v3.py).

    python dump_text_v3.py ROM.gba game_text.json

Mantem traducoes ja existentes no JSON de saida. Correcoes sobre a v2:
- tabela de caracteres completa 0x00-0x7F (a v2 nao tinha 0x4F e decodificava 0x4F-0x7F deslocado);
- o byte depois de <?> (0xFA) sai cru como <0xNN> (a v2 gerava "<?>ä" = 0x50, que a insercao gravava como 0x51);
- MedaforceDesc (0x3B7580) removido: sao as entradas 50-51 de MedaforceNames.
"""
import argparse
import json
import os
import struct

from medtext import CHARS

GBA_BASE = 0x08000000

TABLES = [
    # --- Nomes ---
    (0x3B6590,  34, "MedalNames",         "Nomes das Medalhas"),
    (0x3BA658, 120, "BotNames",           "Nomes dos Medabots"),
    (0x3BBB4C, 480, "PartNames",          "Nomes das peças"),
    (0x3BCFEC, 480, "PartCodeNames",      "Códigos das peças"),
    (0x3C40B8,  97, "CharacterNames",     "Nomes dos personagens"),
    (0x483ED8,  64, "KeyItems",           "Key Items"),

    # --- Medalhas / Habilidades ---
    (0x3B66EC,  27, "MedalAbilityNames",  "Nomes das habilidades"),
    (0x3B6B48,  27, "MedalAbilityDesc1",  "Compatibilidade peça→medalha"),
    (0x3B6FA4,  26, "MedalAbilityDesc2",  "Compatibilidade medalha→peça"),
    (0x3B7048,   8, "AttackTypeNames",    "Tipos de ataque"),
    (0x3B722C,   8, "AttackTypeDesc",     "Descrições dos ataques"),
    (0x3B74B8,  52, "MedaforceNames",     "Nomes das Medaforças"),
    # 0x3B7580 (antigo "MedaforceDesc") sao as entradas 50-51 de MedaforceNames: removido
    (0x3B80C0,   5, "MedaforceDesc1",      "Descrições das Medaforças1"),
    (0x3B82F0,   8, "SpecialAttackDesc",  "Descrições de ataques"),
    (0x3BE868,  60, "WeaponTypeNames",    "Nomes dos tipos de arma"),

    # --- Pernas / Campos ---
    (0x3BEAFC,   7, "LegTypes",           "Tipos de perna"),
    (0x3BEB68,  11, "BattlefieldNames",   "Campos de batalha"),

    # --- Batalha / Status ---
    (0x3BDA04,  11, "StatDescriptions",   "Descrições de estatísticas"),
    (0x3C2D3C,  96, "PreBattleMessage",   "Diálogos antes das batalhas"),
    (0x3C3B04,  96, "PostBattleMessage",  "Diálogos após as batalhas"),
    (0x3C47EC,   6, "MenuLabels",         "Rótulos do menu"),
    (0x3C4B90, 128, "StatusStrings",      "Textos de status"),
    (0x3C6744, 176, "GameMessages",       "Mensagens do jogo"),
    (0x3CF530,  32, "SystemMessages",     "Mensagens do sistema"),

    # --- Diálogo ---
    (0x3B5578,  23, "Dialogue_3B53D8",    "Diálogo inicial"),
    (0x4144B4, 248, "Dialogue_3F7BD4",    "Diálogo de NPCs (1)"),
    (0x411AE0, 202, "Dialogue_411D34",    "Diálogo de NPCs (2)"),
    (0x4798F4, 389, "Dialogue_479E0C",    "Diálogo de história (1)"),
    (0x479F0C, 237, "Dialogue_47A424",    "Diálogo de história (2)"),
    (0x47A2C4, 303, "Dialogue_47A7DC",    "Diálogo de história (3)"),
    (0x47A784, 323, "Dialogue_47AC9C",    "Diálogo de história (4)"),
    (0x47AC94, 286, "Dialogue_47B1AC",    "Diálogo de história (5)"),
    (0x47B110, 295, "Dialogue_47B628",    "Diálogo de história (6)"),
    (0x47B5B0, 259, "Dialogue_47BAC8",    "Diálogo de história (7)"),
    (0x47B9C0, 263, "Dialogue_47BED8",    "Diálogo de história (8)"),
    (0x47BDE0, 281, "Dialogue_47C2F8",    "Diálogo de história (9)"),
    (0x47C248, 256, "Dialogue_47C760",    "Diálogo de história (10)"),
    (0x47C64C, 277, "Dialogue_47CB64",    "Diálogo de história (11)"),
    (0x47CAA8, 180, "Dialogue_47CFC0",    "Diálogo de história (12)"),
    (0x47CD7C, 233, "Dialogue_47D294",    "Diálogo de história (13)"),
    (0x47D124, 293, "Dialogue_47D63C",    "Diálogo de história (14)"),
    (0x47D5C0, 323, "Dialogue_47DAD8",    "Diálogo de história (15)"),
    (0x47DAD0, 276, "Dialogue_47DFE8",    "Diálogo de história (16)"),

    # --- Loja / Torneio ---
    (0x48521C,  19, "ShopDialogue",       "Diálogo de loja"),
    (0x485F88,  31, "TournamentMessages", "Mensagens de torneio"),
    (0x48698C,  12, "RaceMessages",       "Mensagens de corrida"),

    # --- Sistema ---
    (0x3ACB48,  11, "ProcessStrings",     "Strings de processo"),
    (0x3ACBA0,  11, "ProcessStrings2",     "Strings de processo"),
    (0x3ACBCC,  22, "ProcessStrings3",     "Strings de processo"),
]



def is_valid_ptr(rom, raw):
    return 0 < raw - GBA_BASE < len(rom)


def read_ptr_table(rom, offset, count):
    pointers = []
    for i in range(count):
        raw = struct.unpack_from('<I', rom, offset + 4 * i)[0]
        if not is_valid_ptr(rom, raw):
            break
        pointers.append(raw - GBA_BASE)
    return pointers


def read_string(rom, offset):
    out = bytearray()
    i = offset
    while i < len(rom):
        b = rom[i]
        if b == 0xFE:
            break
        out.append(b)
        if b == 0xFF:
            out.append(rom[i + 1])
            break
        if b in (0xF7, 0xF9):
            out.append(rom[i + 1])
            i += 2
        elif b == 0xFB:
            out.extend(rom[i + 1:i + 4])
            i += 4
        else:
            i += 1
    return bytes(out)


def decode(data):
    out = []
    i = 0
    while i < len(data):
        b = data[i]
        if b < len(CHARS):
            out.append(CHARS[b])
            i += 1
        elif b == 0xF7:
            out.append(f'<SPEED:{data[i + 1]}>')
            i += 2
        elif b == 0xF8:
            out.append('<I>')
            i += 1
        elif b == 0xF9:
            out.append(f'<MEM:{data[i + 1]}>')
            i += 2
        elif b == 0xFA:
            out.append('<?>')
            i += 1
            # Argumento do 0xFA: byte cru
            if i < len(data) and data[i] < 0x80:
                out.append(f'<0x{data[i]:02X}>')
                i += 1
        elif b == 0xFB:
            out.append(f'<PORTRAIT:{data[i + 1]},{data[i + 2]},{data[i + 3]}>')
            i += 4
        elif b == 0xFC:
            out.append('<NB>')
            i += 1
        elif b == 0xFD:
            out.append('<NL>')
            i += 1
        elif b == 0xFF:
            out.append(f'<END:{data[i + 1]}>')
            i += 2
        else:
            out.append(f'<0x{b:02X}>')
            i += 1
    return ''.join(out)


def extract_table(rom, offset, count, description):
    strings = [{"index": idx, "original": decode(read_string(rom, ptr)), "translation": ""}
               for idx, ptr in enumerate(read_ptr_table(rom, offset, count))]
    return {"ptr_offset": f"0x{offset:08X}", "description": description, "strings": strings}


def load_translations(path):
    if not os.path.exists(path):
        return {}
    with open(path, encoding='utf-8') as f:
        data = json.load(f)
    return {(name, e["index"]): e.get("translation", "")
            for name, table in data.get("tables", {}).items() for e in table.get("strings", [])}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('rom')
    ap.add_argument('json')
    args = ap.parse_args()

    print("Lendo ROM...")
    with open(args.rom, 'rb') as f:
        rom = f.read()

    data = {"tables": {}}
    total = 0
    for offset, count, name, desc in TABLES:
        data["tables"][name] = extract_table(rom, offset, count, desc)
        total += len(data["tables"][name]["strings"])

    existing = load_translations(args.json)
    kept = 0
    for name, table in data["tables"].items():
        for e in table["strings"]:
            t = existing.get((name, e["index"]), "")
            if t:
                e["translation"] = t
                kept += 1

    with open(args.json, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    print(f"{total} strings em {len(TABLES)} tabelas, {kept} traducoes mantidas: {args.json}")


if __name__ == '__main__':
    main()
