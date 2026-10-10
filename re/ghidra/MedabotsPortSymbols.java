// Porta simbolos de re/symbols/rkse.csv (Rokusho E) para o programa atual (Metabee E) por assinatura de bytes.
// @category Medabots
// @menupath Tools.Medabots.Port Rokusho Symbols
// @author medabots-re

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStreamReader;
import java.math.BigInteger;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.lang.Register;
import ghidra.program.model.listing.Data;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.mem.Memory;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;

public class MedabotsPortSymbols extends GhidraScript {

    private static final String CSV_RELATIVE = ".." + File.separator + "symbols" + File.separator + "rkse.csv";
    private static final long CODE_START = 0x08000000L;
    private static final long CODE_END = 0x0807FFFFL;
    private static final long WINDOW = 0x4000L;
    private static final int MIN_UNIQUE_BYTES = 12;

    private static final class Row {
        long address;
        String name;
        String kind;
        String comment;
        byte[] bytes;
        byte[] mask;
        long target = -1;
    }

    private int applied;
    private int skipped;
    private final List<String> unmatched = new ArrayList<>();

    @Override
    protected void run() throws Exception {
        File dir = new File(getSourceFile().getParentFile().getAbsolutePath());
        File csv = new File(dir, CSV_RELATIVE).getCanonicalFile();
        if (!csv.exists()) {
            csv = askFile("Selecione rkse.csv", "Abrir");
        }
        List<Row> rows = read(csv);

        // Passo 1: funcoes com assinatura longa e unica na janela
        TreeMap<Long, Long> deltas = new TreeMap<>();
        for (Row r : rows) {
            monitor.checkCancelled();
            if (!r.kind.startsWith("func_") || r.bytes == null || significant(r) < MIN_UNIQUE_BYTES) {
                continue;
            }
            long hit = findUnique(r);
            if (hit >= 0) {
                r.target = hit;
                deltas.put(r.address, hit - r.address);
            }
        }

        // Passo 2: funcoes curtas ou ambiguas usam o delta da vizinha mais proxima
        for (Row r : rows) {
            monitor.checkCancelled();
            if (!r.kind.startsWith("func_") || r.target >= 0) {
                continue;
            }
            Long delta = nearestDelta(deltas, r.address);
            if (delta == null) {
                continue;
            }
            long candidate = r.address + delta;
            if (r.bytes == null || r.bytes.length == 0 || matchesAt(r, candidate)) {
                r.target = candidate;
            }
        }

        for (Row r : rows) {
            monitor.checkCancelled();
            if (r.kind.startsWith("func_")) {
                if (r.target < 0) {
                    unmatched.add(r.name);
                    continue;
                }
                applyFunction(toAddr(r.target), r.name, r.kind.equals("func_thumb"), r.comment);
            }
            else if (r.address < CODE_START) {
                // RAM e IO: mesmo layout nas versoes E
                applyLabel(toAddr(r.address), r.name, r.comment);
            }
            else {
                skipped++;
            }
        }

        println("Funcoes casadas no passo 1: " + deltas.size());
        println("Aplicados: " + applied + " | Ignorados (dados de ROM, enderecos mudam): " + skipped
            + " | Sem correspondencia: " + unmatched.size());
        for (String n : unmatched) {
            println("  sem correspondencia: " + n);
        }
        popup("Port concluido.\n\nAplicados: " + applied + "\nSem correspondencia: " + unmatched.size()
            + "\n\nDetalhes na aba Console.\nRode Export Symbols e salve (Ctrl+S).");
    }

    private int significant(Row r) {
        int n = 0;
        for (byte m : r.mask) {
            if (m != 0) {
                n++;
            }
        }
        return n;
    }

    private long findUnique(Row r) throws Exception {
        Memory mem = currentProgram.getMemory();
        long lo = Math.max(CODE_START, r.address - WINDOW);
        long hi = Math.min(CODE_END, r.address + WINDOW);
        Address start = toAddr(lo);
        Address end = toAddr(hi);
        long found = -1;
        Address cur = start;
        while (cur != null && cur.compareTo(end) <= 0) {
            Address hit = mem.findBytes(cur, end, r.bytes, r.mask, true, monitor);
            if (hit == null) {
                break;
            }
            if ((hit.getOffset() & 1) == 0) {
                if (found >= 0) {
                    return -1;
                }
                found = hit.getOffset();
            }
            cur = hit.add(1);
        }
        return found;
    }

    private boolean matchesAt(Row r, long at) {
        byte[] buf = new byte[r.bytes.length];
        try {
            currentProgram.getMemory().getBytes(toAddr(at), buf);
        }
        catch (Exception e) {
            return false;
        }
        for (int i = 0; i < buf.length; i++) {
            if (r.mask[i] != 0 && buf[i] != r.bytes[i]) {
                return false;
            }
        }
        return true;
    }

    private static Long nearestDelta(TreeMap<Long, Long> deltas, long address) {
        Long below = deltas.floorKey(address);
        Long above = deltas.ceilingKey(address);
        if (below == null && above == null) {
            return null;
        }
        if (below == null) {
            return deltas.get(above);
        }
        if (above == null) {
            return deltas.get(below);
        }
        return (address - below <= above - address) ? deltas.get(below) : deltas.get(above);
    }

    private boolean isDefaultName(Symbol s) {
        return s == null || s.getSource() == SourceType.DEFAULT || s.getSource() == SourceType.ANALYSIS;
    }

    private void applyFunction(Address a, String name, boolean thumb, String comment) throws Exception {
        Function f = getFunctionAt(a);
        if (f != null && f.getSymbol().getSource() == SourceType.USER_DEFINED) {
            // Nome dado no projeto Metabee tem prioridade
            if (!f.getName().equals(name) && comment != null && !comment.isEmpty() && f.getComment() == null) {
                f.setComment("Rokusho: " + name + ". " + comment);
            }
            return;
        }
        if (f == null) {
            Function container = getFunctionContaining(a);
            if (container != null && !container.getEntryPoint().equals(a)
                && container.getSymbol().getSource() != SourceType.USER_DEFINED) {
                removeFunction(container);
            }
            Instruction ins = getInstructionAt(a);
            if (ins == null) {
                Data d = getDataAt(a);
                if (d != null) {
                    clearListing(d.getMinAddress(), d.getMaxAddress());
                }
                Register tmode = currentProgram.getProgramContext().getRegister("TMode");
                currentProgram.getProgramContext().setValue(tmode, a, a, thumb ? BigInteger.ONE : BigInteger.ZERO);
                disassemble(a);
            }
            f = createFunction(a, name);
            if (f == null) {
                unmatched.add(name + " (falha ao criar em " + a + ")");
                return;
            }
        }
        try {
            f.setName(name, SourceType.USER_DEFINED);
        }
        catch (Exception e) {
            unmatched.add(name + " (" + e.getMessage() + ")");
            return;
        }
        if (comment != null && !comment.isEmpty() && f.getComment() == null) {
            f.setComment(comment);
        }
        applied++;
    }

    private void applyLabel(Address a, String name, String comment) throws Exception {
        if (!currentProgram.getMemory().contains(a)) {
            skipped++;
            return;
        }
        Symbol primary = getSymbolAt(a);
        if (!isDefaultName(primary)) {
            // Ja tem nome do usuario (CSV Metabee): mantem
            skipped++;
            return;
        }
        if (!getSymbols(name, null).isEmpty()) {
            skipped++;
            return;
        }
        Symbol s = createLabel(a, name, true, SourceType.USER_DEFINED);
        if (s != null && comment != null && !comment.isEmpty() && getEOLComment(a) == null) {
            setEOLComment(a, comment);
        }
        applied++;
    }

    private List<Row> read(File file) throws Exception {
        List<Row> rows = new ArrayList<>();
        try (BufferedReader reader = new BufferedReader(
            new InputStreamReader(new FileInputStream(file), StandardCharsets.UTF_8))) {
            String line = reader.readLine();
            while ((line = reader.readLine()) != null) {
                List<String> f = split(line);
                if (f.size() < 3 || f.get(0).trim().isEmpty()) {
                    continue;
                }
                Row r = new Row();
                r.address = Long.decode(f.get(0).trim());
                r.name = f.get(1).trim();
                r.kind = f.get(2).trim();
                r.comment = f.size() > 4 ? f.get(4).trim() : "";
                String sig = f.size() > 5 ? f.get(5).trim() : "";
                if (!sig.isEmpty()) {
                    int n = sig.length() / 2;
                    r.bytes = new byte[n];
                    r.mask = new byte[n];
                    for (int i = 0; i < n; i++) {
                        String h = sig.substring(i * 2, i * 2 + 2);
                        if (!h.equals("..")) {
                            r.bytes[i] = (byte) Integer.parseInt(h, 16);
                            r.mask[i] = (byte) 0xff;
                        }
                    }
                }
                rows.add(r);
            }
        }
        return rows;
    }

    private static List<String> split(String line) {
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
}
