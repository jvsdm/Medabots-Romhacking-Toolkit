// Cria os structs do jogo (categoria /Medabots) a partir de medabots_types.tsv (mesma pasta deste script, gerado
// por re/tools/build_types.py) e aplica os tipos nos globais da RAM/ROM. Recria o struct inteiro a cada execução
// (os campos vêm só do repositório) e tipa como ponteiro os literais de código que guardam esses endereços
// (o decompilador passa a mostrar fi->campo). Não mexe em funções nem em nomes.
// Linhas do tsv:
//   T <nome> <tamanho>                         struct
//   M <struct> <offset> <tipo> <nome> <coment> campo; tipo = u8|u16|u32|s8|s16|s32|bytes:N|<Struct>|<Struct>:N
//   A <endereço> <tipo> <qtd> <rótulo>         aplica o tipo (array se qtd > 1) e dá o rótulo
// Grava o resumo em medabots_types_log.txt. Depois salve o projeto (Ctrl+S).
// @category Medabots
// @menupath Tools.Medabots.Types
// @author medabots-re

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.data.*;
import ghidra.program.model.listing.Data;
import ghidra.program.model.mem.MemoryBlock;
import ghidra.program.model.symbol.SourceType;
import java.io.File;
import java.io.PrintWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

public class MedabotsTypes extends GhidraScript {
    private final CategoryPath cat = new CategoryPath("/Medabots");
    private final Map<String, Structure> structs = new LinkedHashMap<>();
    private DataTypeManager dtm;

    private DataType base(String t) {
        switch (t) {
            case "u8": return ByteDataType.dataType;
            case "s8": return SignedByteDataType.dataType;
            case "u16": return WordDataType.dataType;
            case "s16": return ShortDataType.dataType;
            case "u32": return DWordDataType.dataType;
            case "s32": return IntegerDataType.dataType;
        }
        if (t.startsWith("bytes:")) return new ArrayDataType(ByteDataType.dataType, Integer.parseInt(t.substring(6)), 1);
        String[] p = t.split(":");
        DataType s = structs.get(p[0]);
        if (s == null) return null;
        if (p.length == 1) return s;
        return new ArrayDataType(s, Integer.parseInt(p[1]), s.getLength());
    }

    @Override
    public void run() throws Exception {
        File dir = getSourceFile().getParentFile().getFile(false);
        File tsv = new File(dir, "medabots_types.tsv");
        if (!tsv.exists()) { tsv = askFile("medabots_types.tsv", "Abrir"); dir = tsv.getParentFile(); }
        List<String> lines = Files.readAllLines(tsv.toPath(), StandardCharsets.UTF_8);
        PrintWriter log = new PrintWriter(new File(dir, "medabots_types_log.txt"), "UTF-8");
        dtm = currentProgram.getDataTypeManager();
        int fields = 0, applied = 0, failed = 0;

        // 1) structs vazios do tamanho certo (permite struct dentro de struct em qualquer ordem)
        for (String line : lines) {
            String[] p = line.split("\t");
            if (!p[0].equals("T")) continue;
            StructureDataType s = new StructureDataType(cat, p[1], Integer.decode(p[2]), dtm);
            Structure r = (Structure) dtm.addDataType(s, DataTypeConflictHandler.REPLACE_HANDLER);
            r.deleteAll();
            // struct vazio informa getLength() == 1 sem ocupar byte nenhum
            int have = r.isZeroLength() ? 0 : r.getLength();
            r.growStructure(Integer.decode(p[2]) - have);
            structs.put(p[1], r);
        }
        // 2) campos
        for (String line : lines) {
            String[] p = line.split("\t", -1);
            if (!p[0].equals("M")) continue;
            Structure s = structs.get(p[1]);
            DataType t = base(p[3]);
            int off = Integer.decode(p[2]);
            if (s == null || t == null || off + t.getLength() > s.getLength()) {
                failed++; log.println("campo falhou " + line); continue;
            }
            try {
                s.replaceAtOffset(off, t, t.getLength(), p[4], p.length > 5 ? p[5] : null);
                fields++;
            } catch (Exception e) { failed++; log.println("campo falhou " + line + " : " + e.getMessage()); }
        }
        // 3) globais
        for (String line : lines) {
            String[] p = line.split("\t", -1);
            if (!p[0].equals("A")) continue;
            Address a = toAddr(Long.decode(p[1]));
            MemoryBlock b = currentProgram.getMemory().getBlock(a);
            DataType t = base(p[2]);
            int n = Integer.parseInt(p[3]);
            if (b == null || t == null) { failed++; log.println("sem bloco/tipo " + line); continue; }
            if (n > 1) t = new ArrayDataType(t, n, t.getLength());
            try {
                clearListing(a, a.add(t.getLength() - 1));
                createData(a, t);
                // rótulo só onde o mtbe (MedabotsSync) ainda não deu nome
                ghidra.program.model.symbol.Symbol sym = getSymbolAt(a);
                if (p.length > 4 && !p[4].isEmpty() && (sym == null || sym.getSource() == SourceType.DEFAULT))
                    createLabel(a, p[4], true, SourceType.USER_DEFINED);
                applied++;
            } catch (Exception e) { failed++; log.println("aplicar falhou " + line + " : " + e.getMessage()); }
        }
        // 4) literais de código (ldr rX,=0x03001800 etc.) viram ponteiro para o struct; assim o decompilador mostra
        //    fi->campo e bots[i].campo em vez de *(byte *)(DAT_xxx + 0xNN). Só palavras alinhadas fora de instrução.
        int literals = 0;
        Map<Long, DataType> want = new LinkedHashMap<>();
        for (String line : lines) {
            String[] p = line.split("\t", -1);
            if (!p[0].equals("A")) continue;
            long v = Long.decode(p[1]);
            DataType t = base(p[2]);
            if (t != null && (v >>> 24) != 0x08) want.put(v, new PointerDataType(t, 4, dtm));
        }
        ghidra.program.model.mem.Memory mem = currentProgram.getMemory();
        Address lo = toAddr(0x08000000L), hi = toAddr(0x08080000L);
        for (Address a = lo; a.compareTo(hi) < 0; a = a.add(4)) {
            monitor.checkCanceled();
            long v;
            try { v = mem.getInt(a) & 0xFFFFFFFFL; } catch (Exception e) { continue; }
            DataType pt = want.get(v);
            if (pt == null) continue;
            if (getInstructionContaining(a) != null || getInstructionContaining(a.add(3)) != null) continue;
            Data d = getDataAt(a);
            if (d != null && d.getDataType().getName().equals(pt.getName())) continue;
            try {
                clearListing(a, a.add(3));
                createData(a, pt);
                literals++;
            } catch (Exception e) { failed++; log.println("literal falhou " + a + " : " + e.getMessage()); }
        }
        String msg = "structs " + structs.size() + ", campos " + fields + ", globais " + applied + ", literais " + literals + ", falhas " + failed;
        log.println(msg);
        log.close();
        println(msg + " (detalhes em medabots_types_log.txt)");
    }
}
