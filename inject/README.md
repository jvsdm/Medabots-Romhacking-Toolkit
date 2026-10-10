# Código injetado em C (estilo CFRU) - Medabots Metabee (E)

Funções novas em C, compiladas para Thumb, gravadas no espaço livre da ROM e ligadas ao jogo por desvios
(hooks) no início de funções originais. Os nomes do jogo vêm de `re/symbols/mtbe.csv`; os tipos, de
`re/types/medabots_simon_types.tsv`.

| Caminho | Conteúdo |
|---|---|
| `patches.json` | fontes a compilar e hooks (`at` = função do jogo, `to` = função em C) |
| `src/` | código C injetado |
| `include/gba_types.h` | tipos básicos e `INJECT_RAM` (EWRAM livre) |
| `include/mtbe.h` | protótipos e constantes do jogo usados pelo código injetado (escritos à mão) |
| `include/mtbe_types.h` | structs/enums gerados dos tipos do Simon (`tools/gen_types.py`) |
| `tools/gen_symbols.py` | CSV -> símbolos do linker (funções Thumb com bit 0) |
| `tools/build.py` | compila, liga no espaço livre, grava o código e os hooks |

## Uso

Compilador: Arm GNU Toolchain (`arm-none-eabi-gcc`, testado com 14.3.rel1), no PATH ou via `--gcc`/`ARM_GCC_BIN`.

```
python inject/tools/gen_types.py            # só quando os tipos mudarem
python inject/tools/build.py IN.gba OUT.gba --gcc <pasta bin do toolchain>
```

- O espaço livre começa depois do último byte diferente de 0xFF (mínimo 8 MB), como no `FreeSpaceAllocator` do
  Toolkit; uma ROM de 8 MB cresce para o próximo MB.
- Hook: `bx pc ; nop` (Thumb) + `ldr ip,[pc] ; bx ip ; .word alvo|1` (ARM), 14-16 bytes no início da função
  original. `ip` é registrador de rascunho na entrada de função (AAPCS). O resto da função original fica intacto.
- Código injetado não pode ter globais graváveis (`.data`/`.bss`); estado vai em `INJECT_RAM` (0x0202F700).
  `0x0202E780-0x0202F7FF` ficou zerado em todas as capturas; `0x0202E000-0x0202E77F` é o inventário expandido.

## Menu de debug (2026-10-10) - ativo em `patches.json`

`src/debug_menu.c` troca dois ponteiros de `gMainStateTable` (nenhum codigo do jogo e alterado):
- `[1]` (mapa): com **L + R + SELECT** abre o menu de debug que ja existe na ROM (cena 63 de `menu_list`);
  sem a combinacao roda o estado original.
- `[0]` (cenas): quando o menu de debug e fechado (B), reinicia o jogo (SoftReset da BIOS), porque a volta
  original (cena 0 com o video do mapa) deixa a tela embaralhada.

No menu: ESQUERDA/DIREITA escolhem, A entra, B sai. Opcoes: Process, Medal, Parts, Tinpet, Item, Face, Mdlpic,
Rnd-tk, Battle, Sound, Motion (Motion = visualizador das animacoes de batalha).

## Piloto (2026-10-10) - opcional (`--with pilot_part_stats`)

`src/pilot_part_stats.c` reescreve `GetPartStats` (0x08042B58, 28 chamadas) com o mesmo comportamento e conta as
chamadas em `INJECT_RAM`. A tabela é lida pelo literal original (0x08042B7C), então funciona também na ROM
expandida, onde a tabela foi movida. Testado no mGBA (mgba-mcp, save state na tela de peças): 182 chamadas, tela
igual à da ROM sem o piloto; as diferenças de RAM ficam na faixa 0x03001408-0x0300167F, que também varia entre duas
execuções da ROM original (ruído do emulador sem limite de FPS).

Próximos passos: integrar o build ao Toolkit (patch de projeto), depois a primeira função nova de verdade
(ação de batalha nova pela tabela de handlers, ou o Medachange).
