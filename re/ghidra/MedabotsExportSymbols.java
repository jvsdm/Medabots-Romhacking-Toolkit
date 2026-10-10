// Exporta os nomes dados no Ghidra para re/symbols/mtbe.csv (mescla com o existente, ordenado por endereco).
// @category Medabots
// @menupath Tools.Medabots.Export Symbols
// @author medabots-re

import java.io.BufferedReader;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.OutputStreamWriter;
import java.io.Writer;
import java.math.BigInteger;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;
import ghidra.program.model.symbol.SymbolType;

public class MedabotsExportSymbols extends GhidraScript {

    private static final String CSV_RELATIVE = ".." + File.separator + "symbols" + File.separator + "mtbe.csv";
    private static final String HEADER = "address,name,kind,count,comment";
    private static final String[] SKIP_PREFIXES = {
        "FUN_", "LAB_", "DAT_", "PTR_", "SUB_", "thunk_", "switchD", "caseD", "s_", "u_"
    };

    private static final class Row {
        String name;
        String kind;
        String count = "";
        String comment = "";
    }

    @Override
    protected void run() throws Exception {
        File dir = new File(getSourceFile().getParentFile().getAbsolutePath());
        File csv = new File(dir, CSV_RELATIVE).getCanonicalFile();
        if (!csv.exists()) {
            csv = askFile("Selecione mtbe.csv", "Salvar");
        }

        Map<Long, Row> rows = read(csv);
        int added = 0;
        int renamed = 0;

        for (Symbol sym : currentProgram.getSymbolTable().getAllSymbols(true)) {
            SourceType src = sym.getSource();
            if (src != SourceType.USER_DEFINED) {
                continue;
            }
            if (!sym.isPrimary() || !sym.isGlobal()) {
                continue;
            }
            Address a = sym.getAddress();
            if (!a.isMemoryAddress() || !currentProgram.getMemory().contains(a)) {
                continue;
            }
            String name = sym.getName();
            if (skip(name)) {
                continue;
            }

            String kind;
            String comment;
            if (sym.getSymbolType() == SymbolType.FUNCTION) {
                kind = isThumb(a) ? "func_thumb" : "func_arm";
                Function f = getFunctionAt(a);
                comment = f != null ? f.getComment() : null;
            }
            else if (sym.getSymbolType() == SymbolType.LABEL) {
                kind = "label";
                comment = getEOLComment(a);
            }
            else {
                continue;
            }
            comment = comment == null ? "" : comment.replace('\n', ' ').replace('\r', ' ');

            Row row = rows.get(a.getOffset());
            if (row == null) {
                row = new Row();
                row.name = name;
                row.kind = kind;
                row.comment = comment;
                rows.put(a.getOffset(), row);
                added++;
                continue;
            }
            if (!row.name.equals(name)) {
                row.name = name;
                renamed++;
            }
            if (kind.startsWith("func_") && (row.kind.startsWith("func_") || row.kind.equals("label"))) {
                row.kind = kind;
            }
            if (row.comment.isEmpty() && !comment.isEmpty()) {
                row.comment = comment;
            }
        }

        write(csv, rows);
        String msg = "Exportado: " + csv.getAbsolutePath() + "\nNovos: " + added + " | Renomeados: " + renamed
            + " | Total: " + rows.size();
        println(msg);
        popup(msg + "\n\nFaca commit do mtbe.csv.");
    }

    private static boolean skip(String name) {
        for (String p : SKIP_PREFIXES) {
            if (name.startsWith(p)) {
                return true;
            }
        }
        return false;
    }

    private boolean isThumb(Address a) {
        BigInteger v = currentProgram.getProgramContext()
            .getValue(currentProgram.getProgramContext().getRegister("TMode"), a, false);
        return v != null && v.intValue() == 1;
    }

    private Map<Long, Row> read(File file) throws Exception {
        Map<Long, Row> rows = new TreeMap<>();
        if (!file.exists()) {
            return rows;
        }
        try (BufferedReader reader = new BufferedReader(new FileReader(file))) {
            String line = reader.readLine();
            while ((line = reader.readLine()) != null) {
                if (line.trim().isEmpty()) {
                    continue;
                }
                List<String> f = split(line);
                if (f.size() < 3 || f.get(0).trim().isEmpty()) {
                    continue;
                }
                Row r = new Row();
                r.name = f.get(1).trim();
                r.kind = f.get(2).trim();
                r.count = f.size() > 3 ? f.get(3).trim() : "";
                r.comment = f.size() > 4 ? f.get(4).trim() : "";
                rows.put(Long.decode(f.get(0).trim()), r);
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

    private static String quote(String v) {
        if (v == null) {
            return "";
        }
        if (v.indexOf(',') >= 0 || v.indexOf('"') >= 0) {
            return "\"" + v.replace("\"", "\"\"") + "\"";
        }
        return v;
    }

    private static void write(File file, Map<Long, Row> rows) throws Exception {
        try (Writer w = new OutputStreamWriter(new FileOutputStream(file), StandardCharsets.UTF_8)) {
            w.write(HEADER + "\n");
            for (Map.Entry<Long, Row> e : rows.entrySet()) {
                Row r = e.getValue();
                w.write(String.format("0x%08X", e.getKey()) + "," + quote(r.name) + "," + r.kind + ","
                    + r.count + "," + quote(r.comment) + "\n");
            }
        }
    }
}
