// Prepara o programa da Metabee (E): memoria, limpeza de codigo falso e simbolos de re/symbols/mtbe.csv.
// @category Medabots
// @menupath Tools.Medabots.Setup
// @author medabots-re

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.math.BigInteger;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Set;

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.address.AddressRange;
import ghidra.program.model.data.PointerDataType;
import ghidra.program.model.lang.Register;
import ghidra.program.model.listing.Bookmark;
import ghidra.program.model.listing.BookmarkManager;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.ProgramContext;
import ghidra.program.model.mem.Memory;
import ghidra.program.model.mem.MemoryBlock;
import ghidra.program.model.symbol.Reference;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;

public class MedabotsSetup extends GhidraScript {

    private static final String EXPECTED_LANGUAGE = "ARM:LE:32:v4t";
    private static final long ROM_START = 0x08000000L;
    private static final long DATA_START = 0x08080000L;
    private static final String CSV_RELATIVE = ".." + File.separator + "symbols" + File.separator + "mtbe.csv";

    // nome, inicio, tamanho, write, execute, volatile
    private static final Object[][] RAM_BLOCKS = {
        { "EWRAM", 0x02000000L, 0x40000L, true, false, false },
        { "IWRAM", 0x03000000L, 0x8000L, true, true, false },
        { "IO", 0x04000000L, 0x400L, true, false, true },
        { "PAL", 0x05000000L, 0x400L, true, false, false },
        { "VRAM", 0x06000000L, 0x18000L, true, false, false },
        { "OAM", 0x07000000L, 0x400L, true, false, false },
    };

    private final List<String> okLog = new ArrayList<>();
    private final List<String> warnLog = new ArrayList<>();

    private static final class Row {
        long address;
        String name;
        String kind;
        int count;
        String comment;
    }

    @Override
    protected void run() throws Exception {
        String lang = currentProgram.getLanguageID().getIdAsString();
        if (!EXPECTED_LANGUAGE.equals(lang)) {
            popup("Idioma do programa e " + lang + ".\nEsperado: " + EXPECTED_LANGUAGE
                + ".\n\nUse Set Language na janela do projeto e rode de novo.");
            return;
        }

        File csv = locateCsv();
        List<Row> rows = readCsv(csv);
        println("CSV: " + csv.getAbsolutePath() + " (" + rows.size() + " linhas)");

        Set<Long> protectedEntries = new HashSet<>();
        for (Row r : rows) {
            if (r.kind.startsWith("func_")) {
                protectedEntries.add(r.address);
            }
        }

        monitor.setMessage("Memoria...");
        ensureMemory();
        monitor.setMessage("Limpeza...");
        cleanup(protectedEntries);
        monitor.setMessage("Simbolos...");
        applySymbols(rows);

        println("OK: " + okLog.size() + " | Avisos: " + warnLog.size());
        for (String w : warnLog) {
            println("  - " + w);
        }
        popup("Medabots setup concluido.\n\nOK: " + okLog.size() + "\nAvisos: " + warnLog.size()
            + "\n\nDetalhes na aba Console.\nSalve com Ctrl+S.");
    }

    private void ok(String msg) {
        okLog.add(msg);
    }

    private void warn(String msg) {
        warnLog.add(msg);
        println("AVISO: " + msg);
    }

    private Address addr(long value) {
        return toAddr(value);
    }

    // CSV

    private File locateCsv() throws Exception {
        File dir = new File(getSourceFile().getParentFile().getAbsolutePath());
        File csv = new File(dir, CSV_RELATIVE).getCanonicalFile();
        if (csv.exists()) {
            return csv;
        }
        return askFile("Selecione mtbe.csv", "Abrir");
    }

    private List<Row> readCsv(File file) throws Exception {
        List<Row> rows = new ArrayList<>();
        try (BufferedReader reader = new BufferedReader(new FileReader(file))) {
            String line = reader.readLine();
            while ((line = reader.readLine()) != null) {
                if (line.trim().isEmpty()) {
                    continue;
                }
                List<String> f = splitCsv(line);
                if (f.size() < 3 || f.get(0).isEmpty()) {
                    continue;
                }
                Row r = new Row();
                r.address = Long.decode(f.get(0).trim());
                r.name = f.get(1).trim();
                r.kind = f.get(2).trim();
                r.count = f.size() > 3 && !f.get(3).trim().isEmpty() ? Integer.parseInt(f.get(3).trim()) : 0;
                r.comment = f.size() > 4 ? f.get(4).trim() : "";
                rows.add(r);
            }
        }
        return rows;
    }

    private static List<String> splitCsv(String line) {
        List<String> out = new ArrayList<>();
        StringBuilder cur = new StringBuilder();
        boolean quoted = false;
        for (int i = 0; i < line.length(); i++) {
            char c = line.charAt(i);
            if (quoted) {
                if (c == '"' && i + 1 < line.length() && line.charAt(i + 1) == '"') {
                    cur.append('"');
                    i++;
                }
                else if (c == '"') {
                    quoted = false;
                }
                else {
                    cur.append(c);
                }
            }
            else if (c == '"') {
                quoted = true;
            }
            else if (c == ',') {
                out.add(cur.toString());
                cur.setLength(0);
            }
            else {
                cur.append(c);
            }
        }
        out.add(cur.toString());
        return out;
    }

    // Memoria

    private void ensureMemory() throws Exception {
        Memory memory = currentProgram.getMemory();
        for (Object[] b : RAM_BLOCKS) {
            String name = (String) b[0];
            Address start = addr((Long) b[1]);
            MemoryBlock block = memory.getBlock(start);
            if (block == null) {
                block = memory.createUninitializedBlock(name, start, (Long) b[2], false);
                ok("Bloco criado: " + name);
            }
            block.setRead(true);
            block.setWrite((Boolean) b[3]);
            block.setExecute((Boolean) b[4]);
            block.setVolatile((Boolean) b[5]);
        }
        MemoryBlock rom = memory.getBlock(addr(ROM_START));
        if (rom != null) {
            rom.setWrite(false);
            rom.setExecute(true);
            if (!"ROM".equals(rom.getName())) {
                rom.setName("ROM");
            }
        }
    }

    // Limpeza

    private boolean hasFlowRefFromCode(Address entry) {
        Address limit = addr(DATA_START);
        for (Reference ref : getReferencesTo(entry)) {
            if (ref.getFromAddress().compareTo(limit) >= 0) {
                continue;
            }
            if (ref.getReferenceType().isCall() || ref.getReferenceType().isJump()) {
                return true;
            }
        }
        return false;
    }

    private void cleanup(Set<Long> protectedEntries) throws Exception {
        Address limit = addr(DATA_START);
        List<Function> bogus = new ArrayList<>();
        for (Function f : currentProgram.getFunctionManager().getFunctions(limit, true)) {
            Address entry = f.getEntryPoint();
            if (protectedEntries.contains(entry.getOffset())) {
                continue;
            }
            if (!hasFlowRefFromCode(entry)) {
                bogus.add(f);
            }
        }

        if (!bogus.isEmpty()) {
            String msg = "Encontradas " + bogus.size() + " funcoes acima de 0x"
                + Long.toHexString(DATA_START).toUpperCase()
                + " sem chamada vinda do codigo.\nSao dados interpretados como codigo.\n\nRemover?";
            if (!askYesNo("Medabots - limpeza", msg)) {
                warn("Limpeza de funcoes falsas pulada");
                return;
            }
            for (Function f : bogus) {
                monitor.checkCancelled();
                List<AddressRange> ranges = new ArrayList<>();
                for (AddressRange r : f.getBody()) {
                    ranges.add(r);
                }
                removeFunction(f);
                for (AddressRange r : ranges) {
                    clearListing(r.getMinAddress(), r.getMaxAddress());
                }
            }
            ok("Funcoes falsas removidas: " + bogus.size());
        }

        MemoryBlock rom = currentProgram.getMemory().getBlock(addr(ROM_START));
        if (rom != null) {
            List<Instruction> orphans = new ArrayList<>();
            for (Instruction ins : currentProgram.getListing().getInstructions(limit, true)) {
                if (ins.getAddress().compareTo(rom.getEnd()) > 0) {
                    break;
                }
                if (getFunctionContaining(ins.getAddress()) == null) {
                    orphans.add(ins);
                }
            }
            for (Instruction ins : orphans) {
                monitor.checkCancelled();
                clearListing(ins.getMinAddress(), ins.getMaxAddress());
            }
            if (!orphans.isEmpty()) {
                ok("Instrucoes soltas removidas na area de dados: " + orphans.size());
            }
        }

        BookmarkManager bm = currentProgram.getBookmarkManager();
        List<Bookmark> doomed = new ArrayList<>();
        Iterator<Bookmark> it = bm.getBookmarksIterator("Error");
        while (it.hasNext()) {
            Bookmark b = it.next();
            if ("Bad Instruction".equals(b.getCategory()) && b.getAddress().compareTo(limit) >= 0) {
                doomed.add(b);
            }
        }
        for (Bookmark b : doomed) {
            bm.removeBookmark(b);
        }
        if (!doomed.isEmpty()) {
            ok("Bookmarks de Bad Instruction removidos: " + doomed.size());
        }
    }

    // Simbolos

    private void applySymbols(List<Row> rows) {
        String[] order = { "ptr", "data", "func_arm", "func_thumb", "label" };
        for (String kind : order) {
            for (Row r : rows) {
                if (!kind.equals(r.kind)) {
                    continue;
                }
                Address a = addr(r.address);
                try {
                    switch (r.kind) {
                        case "func_thumb":
                            applyFunction(a, r.name, true, r.comment);
                            break;
                        case "func_arm":
                            applyFunction(a, r.name, false, r.comment);
                            break;
                        case "ptr":
                            applyPointers(a, r.name, r.count, r.comment);
                            break;
                        case "data":
                            applyData(a, r.name, r.count, r.comment);
                            break;
                        default:
                            setName(a, r.name);
                            setComment(a, r.comment);
                            ok("Label: " + r.name);
                    }
                }
                catch (Exception e) {
                    warn(r.name + " em " + a + ": " + e.getMessage());
                }
            }
        }
        for (Row r : rows) {
            boolean known = false;
            for (String k : order) {
                known |= k.equals(r.kind);
            }
            if (!known) {
                warn("Tipo desconhecido '" + r.kind + "' em " + r.name);
            }
        }
    }

    private Register tmode() {
        return currentProgram.getProgramContext().getRegister("TMode");
    }

    private int currentTMode(Address a) {
        BigInteger v = currentProgram.getProgramContext().getValue(tmode(), a, false);
        return v == null ? -1 : v.intValue();
    }

    private void setTMode(Address a, boolean thumb) throws Exception {
        ProgramContext ctx = currentProgram.getProgramContext();
        ctx.setValue(tmode(), a, a, thumb ? BigInteger.ONE : BigInteger.ZERO);
    }

    private void setName(Address a, String name) throws Exception {
        for (Symbol s : getSymbols(name, null)) {
            if (!s.getAddress().equals(a)) {
                warn("Nome " + name + " ja usado em " + s.getAddress() + "; mantido la");
                return;
            }
        }
        Symbol s = createLabel(a, name, true, SourceType.USER_DEFINED);
        if (s != null) {
            s.setPrimary();
        }
    }

    private void setComment(Address a, String comment) {
        if (comment != null && !comment.isEmpty()) {
            setEOLComment(a, comment);
        }
    }

    private void applyFunction(Address a, String name, boolean thumb, String comment) throws Exception {
        Function func = getFunctionAt(a);
        if (func == null) {
            Function container = getFunctionContaining(a);
            if (container != null && !container.getEntryPoint().equals(a)) {
                ok("Funcao " + container.getName() + " removida (engolia " + name + ")");
                removeFunction(container);
            }
            Instruction ins = getInstructionAt(a);
            int wanted = thumb ? 1 : 0;
            if (ins != null && currentTMode(a) != wanted) {
                clearListing(ins.getMinAddress(), ins.getMaxAddress());
                ins = null;
            }
            if (ins == null) {
                Data data = getDataAt(a);
                if (data != null) {
                    clearListing(data.getMinAddress(), data.getMaxAddress());
                }
                setTMode(a, thumb);
                if (!disassemble(a)) {
                    warn("Falha ao desmontar " + name + " em " + a);
                }
            }
            func = createFunction(a, name);
            if (func == null) {
                warn("Falha ao criar funcao " + name + " em " + a);
                return;
            }
        }
        func.setName(name, SourceType.USER_DEFINED);
        if (comment != null && !comment.isEmpty()) {
            func.setComment(comment);
        }
        ok("Funcao: " + name);
    }

    private void removeFunctionsIn(Address start, Address end) {
        List<Function> doomed = new ArrayList<>();
        for (Function f : currentProgram.getFunctionManager().getFunctions(start, true)) {
            if (f.getEntryPoint().compareTo(end) > 0) {
                break;
            }
            doomed.add(f);
        }
        Function container = getFunctionContaining(start);
        if (container != null && !doomed.contains(container)) {
            doomed.add(container);
        }
        for (Function f : doomed) {
            removeFunction(f);
        }
    }

    private void applyData(Address a, String name, int size, String comment) throws Exception {
        if (size > 0) {
            Address end = a.add(size - 1);
            removeFunctionsIn(a, end);
            clearListing(a, end);
        }
        setName(a, name);
        setComment(a, comment);
        ok("Dados: " + name);
    }

    private void applyPointers(Address a, String name, int count, String comment) throws Exception {
        Address end = a.add((long) count * 4 - 1);
        removeFunctionsIn(a, end);
        clearListing(a, end);
        PointerDataType ptr = new PointerDataType();
        for (int i = 0; i < count; i++) {
            try {
                createData(a.add((long) i * 4), ptr);
            }
            catch (Exception e) {
                warn("Ponteiro " + i + " de " + name + ": " + e.getMessage());
                break;
            }
        }
        setName(a, name);
        setComment(a, comment);
        ok("Tabela: " + name + " (" + count + " ponteiros)");
    }
}
