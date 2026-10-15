// Deixa o Ghidra igual ao repositório: lê medabots_sync.tsv (mesma pasta deste script), cria as funções que
// faltam e aplica os nomes de re/symbols/mtbe.csv. Linhas S separam uma função que o Ghidra engoliu dentro de
// outra (remove a hospedeira, cria a nova, recria a hospedeira com o mesmo nome). Linhas D só desmontam (caso de
// switch sem prólogo). Fora isso não apaga nada.
// Grava o que mudou em medabots_sync_log.txt (mesma pasta). Depois salve o projeto (Ctrl+S).
// @category Medabots
// @menupath Tools.Medabots.Sync
// @author medabots-re

import ghidra.app.cmd.function.CreateFunctionCmd;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.lang.Register;
import ghidra.program.model.listing.Data;
import java.math.BigInteger;
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
    private int repaired = 0;

    // Função sem código na entrada (corpo de 1 byte, "halt_baddata"): os bytes estavam marcados como dado ou o
    // modo ficou ARM. Tira o dado do ponto de entrada, marca Thumb, desmonta e recalcula o corpo.
    private void ensureCode(Function fn) throws Exception {
        Address a = fn.getEntryPoint();
        if (getInstructionAt(a) != null) return;  // já tem código (funções folha de 1 instrução têm corpo de 2 bytes)
        Data d = getDataContaining(a);
        if (d != null) clearListing(d.getMinAddress(), d.getMaxAddress());
        Register t = currentProgram.getRegister("TMode");
        if (t != null && getInstructionAt(a) == null)
            currentProgram.getProgramContext().setValue(t, a, a, BigInteger.ONE);
        disassemble(a);
        CreateFunctionCmd.fixupFunctionBody(currentProgram, fn, monitor);
        if (getInstructionAt(a) != null) { repaired++; log.println("reparada " + a + " corpo " + fn.getBody().getNumAddresses()); }
        else log.println("sem codigo " + a);
    }

    @Override
    public void run() throws Exception {
        File dir = getSourceFile().getParentFile().getFile(false);
        File tsv = new File(dir, "medabots_sync.tsv");
        if (!tsv.exists()) { tsv = askFile("medabots_sync.tsv", "Abrir"); dir = tsv.getParentFile(); }
        List<String> lines = Files.readAllLines(tsv.toPath(), StandardCharsets.UTF_8);
        log = new PrintWriter(new File(dir, "medabots_sync_log.txt"), "UTF-8");

        int created = 0, renamed = 0, same = 0, labels = 0, inside = 0, failed = 0, disasm = 0;
        monitor.initialize(lines.size());
        for (String line : lines) {
            monitor.checkCanceled();
            monitor.incrementProgress(1);
            if (line.isEmpty() || line.startsWith("#")) continue;
            String[] p = line.split("\t");
            Address a = toAddr(Long.decode(p[1]));
            try {
                if (p[0].equals("D")) {
                    // Caso de switch sem prólogo: só desmonta como Thumb para o Ghidra mostrar o código.
                    if (getInstructionAt(a) != null) continue;
                    Data d = getDataContaining(a);
                    if (d != null && !d.isDefined()) clearListing(d.getMinAddress(), d.getMaxAddress());
                    Register t = currentProgram.getRegister("TMode");
                    if (t != null) currentProgram.getProgramContext().setValue(t, a, a, BigInteger.ONE);
                    disassemble(a);
                    if (getInstructionAt(a) != null) { disasm++; log.println("desmontada " + a); }
                    else { failed++; log.println("falhou desmontar " + a); }
                    continue;
                }
                if (p[0].equals("S")) {
                    // Função engolida por outra: tira a hospedeira, cria esta, recria a hospedeira com o mesmo nome.
                    if (getFunctionAt(a) != null) continue;
                    Function host = getFunctionContaining(a);
                    Address hostEntry = null;
                    String hostName = null;
                    if (host != null) {
                        hostEntry = host.getEntryPoint();
                        hostName = host.getName();
                        removeFunction(host);
                    }
                    disassemble(a);
                    Function fn = createFunction(a, null);
                    if (fn == null) { failed++; log.println("falhou separar " + a); }
                    else { created++; ensureCode(fn); log.println("separada " + a + (hostName != null ? " de " + hostName : "")); }
                    if (hostEntry != null && getFunctionAt(hostEntry) == null) {
                        Function h = createFunction(hostEntry, null);
                        if (h == null) { failed++; log.println("falhou recriar " + hostEntry + " " + hostName); }
                        else h.setName(hostName, SourceType.USER_DEFINED);
                    }
                    continue;
                }
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
                ensureCode(fn);
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
            + ", rotulos " + labels + ", miolo de outra funcao " + inside + ", reparadas " + repaired + ", desmontadas " + disasm + ", falharam " + failed;
        log.println(sum);
        log.close();
        println(sum);
        println("detalhes em " + new File(dir, "medabots_sync_log.txt"));
    }
}
