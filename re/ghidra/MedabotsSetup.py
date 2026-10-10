# Prepara o programa da Metabee (E): memoria, limpeza de codigo falso e simbolos de re/symbols/mtbe.csv.
# @category Medabots
# @menupath Tools.Medabots.Setup
# @author medabots-re

from __future__ import print_function

import csv
import os

from java.math import BigInteger
from ghidra.program.model.data import PointerDataType
from ghidra.program.model.symbol import SourceType

EXPECTED_LANGUAGE = "ARM:LE:32:v4t"
ROM_START = 0x08000000
DATA_START = 0x08080000
CSV_RELATIVE = os.path.join("..", "symbols", "mtbe.csv")

RAM_BLOCKS = [
    # nome, inicio, tamanho, write, execute, volatile
    ("EWRAM", 0x02000000, 0x40000, True, False, False),
    ("IWRAM", 0x03000000, 0x8000, True, True, False),
    ("IO", 0x04000000, 0x400, True, False, True),
    ("PAL", 0x05000000, 0x400, True, False, False),
    ("VRAM", 0x06000000, 0x18000, True, False, False),
    ("OAM", 0x07000000, 0x400, True, False, False),
]

log_ok = []
log_warn = []


def ok(msg):
    log_ok.append(msg)


def warn(msg):
    log_warn.append(msg)
    print("AVISO: " + msg)


def addr(value):
    return toAddr(value)


# Idioma e memoria

def check_language():
    lang = str(currentProgram.getLanguageID())
    if lang != EXPECTED_LANGUAGE:
        popup("Idioma do programa e %s.\nEsperado: %s.\n\nUse Set Language na janela do projeto e rode de novo." % (lang, EXPECTED_LANGUAGE))
        return False
    return True


def ensure_memory():
    memory = currentProgram.getMemory()
    for name, start, size, write, execute, volatile in RAM_BLOCKS:
        start_addr = addr(start)
        existing = memory.getBlock(start_addr)
        if existing is None:
            block = memory.createUninitializedBlock(name, start_addr, size, False)
            ok("Bloco criado: %s" % name)
        else:
            block = existing
        block.setRead(True)
        block.setWrite(write)
        block.setExecute(execute)
        block.setVolatile(volatile)

    rom = memory.getBlock(addr(ROM_START))
    if rom is not None:
        rom.setWrite(False)
        rom.setExecute(True)
        if rom.getName() != "ROM":
            rom.setName("ROM")


# Limpeza de funcoes falsas na area de dados

def has_flow_ref_from_code(entry):
    limit = addr(DATA_START)
    for ref in getReferencesTo(entry):
        if ref.getFromAddress().compareTo(limit) >= 0:
            continue
        ref_type = ref.getReferenceType()
        if ref_type.isCall() or ref_type.isJump():
            return True
    return False


def find_bogus_functions(protected):
    limit = addr(DATA_START)
    bogus = []
    for func in currentProgram.getFunctionManager().getFunctions(limit, True):
        entry = func.getEntryPoint()
        if entry.getOffset() in protected:
            continue
        if not entry.getAddressSpace().equals(limit.getAddressSpace()):
            continue
        if has_flow_ref_from_code(entry):
            continue
        bogus.append(func)
    return bogus


def clear_orphan_instructions():
    listing = currentProgram.getListing()
    memory = currentProgram.getMemory()
    rom = memory.getBlock(addr(ROM_START))
    if rom is None:
        return 0
    start = addr(DATA_START)
    end = rom.getEnd()
    to_clear = []
    it = listing.getInstructions(start, True)
    while it.hasNext():
        ins = it.next()
        if ins.getAddress().compareTo(end) > 0:
            break
        if getFunctionContaining(ins.getAddress()) is None:
            to_clear.append(ins)
    for ins in to_clear:
        clearListing(ins.getMinAddress(), ins.getMaxAddress())
    return len(to_clear)


def clear_bad_bookmarks():
    bm = currentProgram.getBookmarkManager()
    removed = 0
    it = bm.getBookmarksIterator("Error")
    doomed = []
    while it.hasNext():
        b = it.next()
        if b.getCategory() == "Bad Instruction":
            doomed.append(b)
    for b in doomed:
        bm.removeBookmark(b)
        removed += 1
    return removed


def cleanup(protected):
    bogus = find_bogus_functions(protected)
    if bogus:
        msg = ("Encontradas %d funcoes acima de 0x%08X sem chamada vinda do codigo.\n"
               "Sao dados interpretados como codigo.\n\nRemover?") % (len(bogus), DATA_START)
        if askYesNo("Medabots - limpeza", msg):
            for func in bogus:
                body = func.getBody()
                removeFunction(func)
                for rng in body:
                    clearListing(rng.getMinAddress(), rng.getMaxAddress())
            ok("Funcoes falsas removidas: %d" % len(bogus))
        else:
            warn("Limpeza de funcoes falsas pulada")
            return
    cleared = clear_orphan_instructions()
    if cleared:
        ok("Instrucoes soltas removidas na area de dados: %d" % cleared)
    removed = clear_bad_bookmarks()
    if removed:
        ok("Bookmarks de Bad Instruction removidos: %d" % removed)


# Simbolos

def tmode_register():
    return currentProgram.getProgramContext().getRegister("TMode")


def current_tmode(address):
    reg = tmode_register()
    value = currentProgram.getProgramContext().getValue(reg, address, False)
    if value is None:
        return None
    return int(value.intValue())


def set_tmode(address, thumb):
    reg = tmode_register()
    value = BigInteger.ONE if thumb else BigInteger.ZERO
    currentProgram.getProgramContext().setValue(reg, address, address, value)


def set_name(address, name):
    for sym in getSymbols(name, None):
        if not sym.getAddress().equals(address):
            warn("Nome %s ja usado em %s; mantido la" % (name, sym.getAddress()))
            return
    sym = createLabel(address, name, True, SourceType.USER_DEFINED)
    if sym is not None:
        sym.setPrimary()


def apply_function(address, name, thumb, comment):
    func = getFunctionAt(address)
    if func is None:
        container = getFunctionContaining(address)
        if container is not None and not container.getEntryPoint().equals(address):
            removeFunction(container)
            ok("Funcao %s removida (engolia %s)" % (container.getName(), name))

        ins = getInstructionAt(address)
        wanted = 1 if thumb else 0
        if ins is not None and current_tmode(address) != wanted:
            clearListing(ins.getMinAddress(), ins.getMaxAddress())
            ins = None
        if ins is None:
            data = getDataAt(address)
            if data is not None:
                clearListing(data.getMinAddress(), data.getMaxAddress())
            set_tmode(address, thumb)
            if not disassemble(address):
                warn("Falha ao desmontar %s em %s" % (name, address))
        func = createFunction(address, name)
        if func is None:
            warn("Falha ao criar funcao %s em %s" % (name, address))
            return
    func.setName(name, SourceType.USER_DEFINED)
    if comment:
        func.setComment(comment)
    ok("Funcao: %s" % name)


def remove_functions_in(start, end):
    fm = currentProgram.getFunctionManager()
    doomed = []
    for func in fm.getFunctions(start, True):
        if func.getEntryPoint().compareTo(end) > 0:
            break
        doomed.append(func)
    container = getFunctionContaining(start)
    if container is not None and container not in doomed:
        doomed.append(container)
    for func in doomed:
        removeFunction(func)


def apply_data(address, name, size, comment):
    if size > 0:
        end = address.add(size - 1)
        remove_functions_in(address, end)
        clearListing(address, end)
    set_name(address, name)
    if comment:
        setEOLComment(address, comment)
    ok("Dados: %s" % name)


def apply_pointers(address, name, count, comment):
    end = address.add(count * 4 - 1)
    remove_functions_in(address, end)
    clearListing(address, end)
    ptr = PointerDataType()
    for i in range(count):
        try:
            createData(address.add(i * 4), ptr)
        except Exception as exc:
            warn("Ponteiro %d de %s: %s" % (i, name, exc))
            break
    set_name(address, name)
    if comment:
        setEOLComment(address, comment)
    ok("Tabela: %s (%d ponteiros)" % (name, count))


def apply_label(address, name, comment):
    set_name(address, name)
    if comment:
        setEOLComment(address, comment)
    ok("Label: %s" % name)


def load_rows():
    path = os.path.normpath(os.path.join(getSourceFile().getParentFile().getAbsolutePath(), CSV_RELATIVE))
    if not os.path.exists(path):
        chosen = askFile("Selecione mtbe.csv", "Abrir")
        path = chosen.getAbsolutePath()
    rows = []
    with open(path) as handle:
        for row in csv.DictReader(handle):
            if not row.get("address"):
                continue
            rows.append(row)
    return path, rows


def apply_symbols(rows):
    order = {"ptr": 0, "data": 1, "func_arm": 2, "func_thumb": 2, "label": 3}
    rows = sorted(rows, key=lambda r: order.get(r["kind"].strip(), 9))
    for row in rows:
        address = addr(int(row["address"], 16))
        name = row["name"].strip()
        kind = row["kind"].strip()
        count = int(row["count"]) if row.get("count") else 0
        comment = (row.get("comment") or "").strip()
        try:
            if kind == "func_thumb":
                apply_function(address, name, True, comment)
            elif kind == "func_arm":
                apply_function(address, name, False, comment)
            elif kind == "ptr":
                apply_pointers(address, name, count, comment)
            elif kind == "data":
                apply_data(address, name, count, comment)
            elif kind == "label":
                apply_label(address, name, comment)
            else:
                warn("Tipo desconhecido '%s' em %s" % (kind, name))
        except Exception as exc:
            warn("%s em %s: %s" % (name, address, exc))


def run():
    if not check_language():
        return
    path, rows = load_rows()
    protected = set()
    for row in rows:
        if row["kind"].strip() in ("func_thumb", "func_arm"):
            protected.add(int(row["address"], 16))

    monitor.setMessage("Memoria...")
    ensure_memory()
    monitor.setMessage("Limpeza...")
    cleanup(protected)
    monitor.setMessage("Simbolos...")
    apply_symbols(rows)

    print("")
    print("CSV: " + path)
    print("OK: %d | Avisos: %d" % (len(log_ok), len(log_warn)))
    for line in log_warn:
        print("  - " + line)
    popup("Medabots setup concluido.\n\nOK: %d\nAvisos: %d\n\nDetalhes no Console (Window > Console).\nAgora rode Analysis > Auto Analyze e salve (Ctrl+S)."
          % (len(log_ok), len(log_warn)))


run()
