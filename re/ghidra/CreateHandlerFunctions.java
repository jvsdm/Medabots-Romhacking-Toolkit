// Cria funções no Ghidra para os handlers chamados por ponteiro (re/functions/H_lista.txt).
// Muitos handlers de tabela (opcodes de evento, ObjectScript, estados da batalha) não são funções no
// Ghidra, então o decompile não funciona neles. Rodar uma vez: Script Manager > CreateHandlerFunctions.
// Escolha o arquivo H_lista.txt quando pedir. Depois salve o projeto (Ctrl+S).
//@category Medabots
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import java.io.File;
import java.nio.file.Files;
import java.util.List;

public class CreateHandlerFunctions extends GhidraScript {
    @Override
    public void run() throws Exception {
        File f = askFile("H_lista.txt", "Abrir");
        List<String> lines = Files.readAllLines(f.toPath());
        int created = 0, existed = 0, failed = 0;
        for (String line : lines) {
            line = line.trim();
            if (line.isEmpty() || line.startsWith("#")) continue;
            String hex = line.split("\s+")[0];
            long v = Long.decode(hex) & ~1L;  // ponteiro Thumb: tira o bit 0
            Address a = toAddr(v);
            if (getFunctionAt(a) != null) { existed++; continue; }
            if (getFunctionContaining(a) != null) { failed++; println("miolo de outra função: " + a); continue; }
            disassemble(a);
            Function fn = createFunction(a, null);
            if (fn != null) created++; else { failed++; println("falhou: " + a); }
        }
        println("criadas " + created + ", já existiam " + existed + ", falharam " + failed);
    }
}
