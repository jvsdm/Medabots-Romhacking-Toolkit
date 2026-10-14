// Deixa o Ghidra igual ao repositório: lê medabots_sync.tsv (mesma pasta deste script), cria as funções que
// faltam e aplica os nomes de re/symbols/mtbe.csv. Não apaga função nem limpa listagem.
// Grava o que mudou em medabots_sync_log.txt (mesma pasta). Depois salve o projeto (Ctrl+S).
// @category Medabots
// @menupath Tools.Medabots.Sync
// @author medabots-re

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.SourceType;
import ghidra.program.model.symbol.Symbol;
import java.io.File;
import java.io.PrintWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.List;

public class MedabotsSync extends GhidraScript {
    private PrintWriter log;

    @Override
    public void run() throws Exception {
        File dir = getSourceFile().getParentFile().getFile(false);
        File tsv = new File(dir, "medabots_sync.tsv");
        if (!tsv.exists()) { tsv = askFile("medabots_sync.tsv", "Abrir"); dir = tsv.getParentFile(); }
        List<String> lines = Files.readAllLines(tsv.toPath(), StandardCharsets.UTF_8);
        log = new PrintWriter(new File(dir, "medabots_sync_log.txt"), "UTF-8");

        int created = 0, renamed = 0, same = 0, labels = 0, inside = 0, failed = 0;
        monitor.initialize(lines.size());
        for (String line : lines) {
            monitor.checkCanceled();
            monitor.incrementProgress(1);
            if (line.isEmpty() || line.startsWith("#")) continue;
            String[] p = line.split("\t");
            Address a = toAddr(Long.decode(p[1]));
            try {
                if (p[0].equals("L")) {
                    Symbol s = getSymbolAt(a);
                    if (s == null || !s.getName().equals(p[2])) {
                        createLabel(a, p[2], true, SourceType.USER_DEFINED);
                        labels++;
                        log.println("rotulo " + a + " " + p[2]);
                    }
                    continue;
                }
                Function fn = getFunctionAt(a);
                if (fn == null) {
                    Function host = getFunctionContaining(a);
                    if (host != null) {
                        inside++;
                        log.println("miolo " + a + " dentro de " + host.getName() + (p.length > 2 ? " (" + p[2] + ")" : ""));
                        continue;
                    }
                    disassemble(a);
                    fn = createFunction(a, null);
                    if (fn == null) { failed++; log.println("falhou criar " + a); continue; }
                    created++;
                    log.println("criada " + a);
                }
                if (p[0].equals("F")) {
                    if (fn.getName().equals(p[2])) { same++; continue; }
                    log.println("nome " + a + " " + fn.getName() + " -> " + p[2]);
                    fn.setName(p[2], SourceType.USER_DEFINED);
                    renamed++;
                }
            } catch (Exception e) {
                failed++;
                log.println("erro " + line + ": " + e.getMessage());
            }
        }
        String sum = "criadas " + created + ", renomeadas " + renamed + ", ja certas " + same
            + ", rotulos " + labels + ", miolo de outra funcao " + inside + ", falharam " + failed;
        log.println(sum);
        log.close();
        println(sum);
        println("detalhes em " + new File(dir, "medabots_sync_log.txt"));
    }
}
