// Aplica os nomes de re/symbols/mtbe.csv SO nas funções que ainda se chamam FUN_* no Ghidra.
// Seguro: não apaga nem cria funções, não limpa listagem, não mexe em nomes já dados (pelos agentes ou à mão).
// Escolha o mtbe.csv quando pedir. Depois salve o projeto (Ctrl+S).
// @category Medabots
// @menupath Tools.Medabots.Apply mtbe names to FUN_
// @author medabots-re

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.SourceType;
import java.io.File;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.List;

public class ApplyMtbeNamesToFUN extends GhidraScript {
    @Override
    public void run() throws Exception {
        File f = askFile("Escolha re/symbols/mtbe.csv", "Abrir");
        List<String> lines = Files.readAllLines(f.toPath(), StandardCharsets.UTF_8);
        int renamed = 0, kept = 0, missing = 0, failed = 0;
        for (String line : lines) {
            if (line.isEmpty() || line.startsWith("#") || line.startsWith("address,")) continue;
            String[] p = line.split(",", 4);
            if (p.length < 3 || !p[2].startsWith("func")) continue;
            String name = p[1].trim();
            if (name.isEmpty() || name.equals("?") || name.startsWith("FUN_")) continue;
            Address a = toAddr(Long.decode(p[0].trim()) & ~1L);
            Function fn = getFunctionAt(a);
            if (fn == null) { missing++; continue; }
            if (!fn.getName().startsWith("FUN_")) { kept++; continue; }
            try { fn.setName(name, SourceType.USER_DEFINED); renamed++; }
            catch (Exception e) { failed++; println("falhou " + a + " " + name + ": " + e.getMessage()); }
        }
        println("renomeadas " + renamed + ", já tinham nome " + kept + ", sem função no endereço " + missing + ", falharam " + failed);
    }
}
