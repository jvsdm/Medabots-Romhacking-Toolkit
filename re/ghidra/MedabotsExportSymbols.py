# Exporta os nomes dados no Ghidra para re/symbols/mtbe.csv (mescla com o existente, ordenado por endereco).
# @category Medabots
# @menupath Tools.Medabots.Export Symbols
# @author medabots-re

from __future__ import print_function

import csv
import os
import sys

from ghidra.program.model.symbol import SourceType, SymbolType

CSV_RELATIVE = os.path.join("..", "symbols", "mtbe.csv")
FIELDS = ["address", "name", "kind", "count", "comment"]
SKIP_PREFIXES = ("FUN_", "LAB_", "DAT_", "PTR_", "SUB_", "thunk_", "switchD", "caseD", "s_", "u_")


def csv_path():
    path = os.path.normpath(os.path.join(getSourceFile().getParentFile().getAbsolutePath(), CSV_RELATIVE))
    if os.path.exists(path):
        return path
    return askFile("Selecione mtbe.csv", "Salvar").getAbsolutePath()


def load_existing(path):
    rows = {}
    if not os.path.exists(path):
        return rows
    with open(path) as handle:
        for row in csv.DictReader(handle):
            if row.get("address"):
                rows[int(row["address"], 16)] = row
    return rows


def is_thumb(address):
    reg = currentProgram.getProgramContext().getRegister("TMode")
    value = currentProgram.getProgramContext().getValue(reg, address, False)
    return value is not None and value.intValue() == 1


def collect():
    found = {}
    table = currentProgram.getSymbolTable()
    for sym in table.getAllSymbols(True):
        if sym.getSource() != SourceType.USER_DEFINED and sym.getSource() != SourceType.IMPORTED:
            continue
        if not sym.isPrimary() or not sym.isGlobal():
            continue
        name = sym.getName()
        if name.startswith(SKIP_PREFIXES):
            continue
        sym_type = sym.getSymbolType()
        address = sym.getAddress()
        if not address.isMemoryAddress():
            continue
        if sym_type == SymbolType.FUNCTION:
            kind = "func_thumb" if is_thumb(address) else "func_arm"
            func = getFunctionAt(address)
            comment = func.getComment() if func is not None else None
        elif sym_type == SymbolType.LABEL:
            kind = "label"
            comment = getEOLComment(address)
        else:
            continue
        found[address.getOffset()] = {"name": name, "kind": kind, "comment": comment or ""}
    return found


def merge(existing, found):
    merged = dict(existing)
    added = 0
    renamed = 0
    for offset, info in found.items():
        row = merged.get(offset)
        if row is None:
            merged[offset] = {
                "address": "0x%08X" % offset,
                "name": info["name"],
                "kind": info["kind"],
                "count": "",
                "comment": info["comment"].replace("\n", " "),
            }
            added += 1
            continue
        if row["name"] != info["name"]:
            row["name"] = info["name"]
            renamed += 1
        if row["kind"] in ("func_thumb", "func_arm", "label") and info["kind"].startswith("func"):
            row["kind"] = info["kind"]
        if info["comment"] and not row.get("comment"):
            row["comment"] = info["comment"].replace("\n", " ")
    return merged, added, renamed


PY2 = sys.version_info[0] == 2


def text(value):
    if value is None:
        return ""
    if PY2:
        return unicode(value).encode("utf-8")
    return str(value)


def write(path, merged):
    handle = open(path, "wb") if PY2 else open(path, "w", newline="", encoding="utf-8")
    try:
        writer = csv.DictWriter(handle, fieldnames=FIELDS, lineterminator="\n")
        writer.writeheader()
        for offset in sorted(merged):
            row = merged[offset]
            writer.writerow(dict((k, text(row.get(k))) for k in FIELDS))
    finally:
        handle.close()


def run():
    path = csv_path()
    existing = load_existing(path)
    found = collect()
    merged, added, renamed = merge(existing, found)
    write(path, merged)
    msg = "Exportado: %s\nNovos: %d | Renomeados: %d | Total: %d" % (path, added, renamed, len(merged))
    print(msg)
    popup(msg + "\n\nFaca commit do mtbe.csv.")


run()
