# TL: boot até o primeiro passo no mapa (logo, título, menu, New Game, Continue, evento 185)

Status: PARCIAL (2026-10-14, agente medabots; GS 1,3,5,7,8,9,10 e name entry sem função no Ghidra: ver TL_lista.txt). Lido no Ghidra (decompile/disasm/xrefs) + ROM original
(`Medabots - Metabee (Europe).gba`, só leitura, python -I). Fontes de tempo/telas: `pc/research/IN1_intro.md`, `IN3`, `IN4`,
`pc/research/README.md` (New Game, evento 185). Símbolos: `TL_simbolos.csv`. Código sem função: `TL_lista.txt`.

Confiança: **confirmado** (lido no código), **provável**, **pendente**.

## 1. Cadeia de estados (confirmado)

- `AgbMain` 0x0800025C: `gMainState` (0x0300178C) = 3, `gamestate_menu` (0x03002B8C) = 0 (MN.md §1).
- `gMainStateTable` 0x083ABF98[3] = `MainState3` 0x08068440; menu 0 de MainState3 = `Battle_DispatchThenEndTransition` 0x0806E460
  (nome antigo errado; renomeado no Ghidra para `Boot_DispatchSubState`: é só um despachante, ver §2).
- 0x0806E460 (disasm): `CallViaR0(tabela_boot[GS])` seguido de `0x0806E404`; GS = byte **0x0300137C**, tabela
  de boot = literal **0x084859D8** (ponteiros Thumb, 11 entradas + 0x8000841 = NoOp no fim).
- Título/menu (confirmado): `MainState2` menu 3 = `FUN_0805c0d4` (corpo: `CallViaR0(tabela[GS])` e incrementa
  dois contadores em 0x0300057C e 0x03001C04 se 0x03001780 & 1). Tabela de sub-passos = literal **0x0848360C**
  (`CallViaR0` com ponteiro Thumb): entrada 0 = 0x0805C128, **entrada 1 = `Screen_SceneInit` 0x0805C17C**
  (carrega o título do menu: char LZ77 0x0878F7E8 -> VRAM 0x06004000, paleta BG 0x0878F5E8, tilemap 0x0879131C etc.),
  2 = 0x0805C2DC, 3 = 0x0805C410, 4 = 0x0805C454, 5 = 0x0805C574, 6 = 0x0805C7F8, 7 = 0x0805C8EC, 8 = 0x0805C950,
  9 = 0x0805C988, 10 = 0x0805CBFC, 11 = 0x0805CC3C (entradas 2..11 ainda não lidas).
- Por que o menu aparece em f3375: gMainState passa de 3 para 2 em ~f3374 (IN1_intro.md).

## 2. Boot: sub-passos de MainState3 menu 0 (GS 0x0300137C, tabela 0x084859D8)

Entradas lidas (ponteiro Thumb - 1). Confiança do nome: **confirmado** só onde o corpo foi lido.

| GS | função | o que faz (lido) | confiança |
|---|---|---|---|
| 0 | 0x0806E504 `Gfx_ResetVramPaletteOamAndStopSong` | zera VRAM/paleta/OAM por DMA, `snd_song_request(0)` (para a música), DISPCNT=0x60, grava GS-setup, `fx_screen_setup_a(2,4,0)`, grava 4 no contador | confirmado |
| 1 | 0x0806E150 | sem função no Ghidra | pendente (TL_lista.txt) |
| 2 | 0x0806E184 `FUN_0806E184` | `fx_step_table_b()`; se não zero, grava REG_DISPCNT, gMainState e 2 bytes de estado a partir da tabela 0x0806E1E0 (passo seguinte) | provável |
| 3 | 0x0806E4CC | sem função no Ghidra | pendente |
| 4 | 0x0806E5E0 `Gfx_LoadLz77ScreenAndEnableBg0` | LZ77 -> VRAM 0x06000000 (`bios_lz77_vram`), monta 20 linhas de mapa (0x14 x 0x20 hwords) para a tela, paleta BG 0x05000000, DISPCNT |= 0x100 (BG0), passo do contador = 5 | confirmado (código) |
| 5 | 0x0806E90C | sem função no Ghidra | pendente |
| 6 | 0x0806E6E4 `Gfx_LoadLz77RowsTable` | igual ao 4 mas com 0x34 no campo +0x10 e passo = 7 (rótulo do GF3 "logo", 0x0806E6E4 confere) | confirmado (código); papel exato na cena: provável (logo) |
| 7 | 0x0806E93C | sem função no Ghidra | pendente |
| 8 | 0x0806E7EC | a ler | pendente |
| 9 | 0x0806E96C | a ler | pendente |
| 10 | 0x0806E9C0 | a ler | pendente |
| 11 | 0x08000841 | NoOpActionHandler | confirmado |

Cenas por tempo (IN1_intro.md, medido): 0 logo f0-511 (tela branca f0; NATSUME f200-f512), 1 demo f512-2881 (mostruário
de técnicas, NÃO batalha), 2 título f2882-3374 (MEDABOTS, PRESS START pisca ~60 quadros), 3 menu f3375+ (NEW GAME / OPTIONS).
Aparelhagem: Mesma VRAM do título (char 0x087B67B0 etc.). Ligação entre GS e cena: a fazer.

## 3. Menu do título (MainState2 menu 3, `FUN_0805c0d4`)

`FUN_0805c0d4` (confirmado): `CallViaR0(tabela_menu3[GS])` com tabela **0x0848360C**; depois incrementa
0x0300057C (+0) e 0x03001C04 (+2 halfword) e, se 0x03001780 & 1, incrementa de novo. Entradas:
0 = 0x0805C128 (a ler), **1 = `Screen_SceneInit` 0x0805C17C** (confirmado: carrega o título do menu: char LZ77
0x0878F7E8 -> VRAM 0x06004000 via `Asset_UnpackOrCopyBlock`, paleta BG 0x0878F5E8 via CpuFastSet, mapas das telas
28..31, registros de OBJ/BG via ponteiros; no fim incrementa um contador em DAT_0805c2d8, não identificado); 2 = 0x0805C2DC; 3 = 0x0805C410; 4 = 0x0805C454;
5 = 0x0805C574; 6 = 0x0805C7F8; 7 = 0x0805C8EC; 8 = 0x0805C950; 9 = 0x0805C988; 10 = 0x0805CBFC; 11 = 0x0805CC3C.
Xrefs para 0x0805C17C: vazias (chamada só por tabela).

## 4. Menu do título: escolha (confirmado no código; sub-passos de 0x0848360C)

- GS 2 = `Save_InitScreen` 0x0805C2DC: chama `Save_ClearAndRetryLoad` (valida o save, ver SV.md). Retorno 0 (save OK):
  cursor (+5) = 0 e mínimo (+4) = 0, e grava bit 0 de 0x03001784. Senão: cursor = 1 e mínimo = 1. Ou seja, **sem save
  o CONTINUE (índice 0) não é selecionável**. Depois `Palette_FadeBlockUpload` e `Screen_HighlightBitmaskGrid`.
- GS 3 = `Screen_StepCounterAndDrawCell` 0x0805C410: anima um contador (`Anim_StepRecordCounter`) e desenha a célula
  (`Screen_DrawCellAdvance`); ao acabar, avança o passo.
- GS 4 = `Menu_TickCursorInputAndConfirm` 0x0805C454 (confirmado): laço do menu de 3 itens.
  - para baixo (0x80) / cima (0x40): cursor 0..2 com volta pelo mínimo (+4) e pelo 2; som SFX 2.
  - A (0x01), SFX 4: se cursor == 2 (OPTIONS): fade de 0x14 quadros, GS += 1 (vai para GS 5, não lido).
    Senão (cursor 0 ou 1): marca o item (+0x48 = 0x2D) e GS = 6 (fade de saída).
- GS 6 = `Gfx_FadeTransitionStep` 0x0805C7F8 (confirmado): fade por 9 passos (`Palette_FadeRowsToColor*`). Ao acabar:
  - cursor 0 = **CONTINUE**: grava 0x03001784 |= 1 e GS = 8 (`0x0805C950`, sem função no Ghidra).
  - cursor 1 = **NEW GAME**: grava 0x03001784 &= ~2 e GS = 7 (`0x0805C8EC`, sem função no Ghidra).

Notas de nome: Ghidra chama o New Game de `new_game_start` (0x0804A2F0); o nome de README/MU é `site_NewGameClear`.

## 5. NEW GAME: o que zera e inicializa (`new_game_start` 0x0804A2F0, confirmado no código)

Zera (ponteiros da literal pool, tamanhos lidos): 0x1C B do bloco de dinheiro/nome/estatística/tempo (0x03002B70..0x03002B8B,
bate com o layout do save em SV.md: dinheiro, nome, lutas, vitórias, tempo); 0x780 B = medalhas (30 x 0x40); 0x3C0 B =
inventário de peças (0x03004C40, 960 B); 0x48 B = equipe (0x030017A0, 9 registros x 8 B); 0x80 B = itens (0x03002BA0);
0xCC0 B = bloco de estado de eventos E (0x030057E0); 0x13C e 0x600 B; VRAM/OAM por `ClearOamAndVramShadow`; 0x2000 e
0x200 B de buffers de tela. Dinheiro = 0 pelo bloco do 1º zero (sem valor inicial diferente de 0 gravado aqui).

Depois: `Slot_InitIdsSequential`, `Tinpet_InitIds`; grava o mapa **23** (`puVar1[lit]=0x17`), tile inicial (18,13) via
`Map_PixelXToTileX/YToTileY` (posição em px 288/208, o que bate com a memória de README), zera o chapter/flags do
bloco E (`*puVar10 = 0xE`, `puVar1[...]=7` e `=1` em um byte de estado; significado não confirmado), cria o objeto
do jogador no slot 0x2F (`Map_InitializeObjectRenderSlot`), condicional a um byte (slot 0x30, parceiro/seguidor, não
confirmado), `Overworld_StartScreenTransitionEffect` (fade de entrada) e `GameState_AdvanceSubroutine` (avança o
passo). Confiança: confirmado (leitura do decompile); nomes de campos: provável.

## 6. Continue e nome: o que sabemos e o que falta

- Continue (GS 8, `0x0805C950`): sem função no Ghidra. Pelo SV.md, o caminho de leitura é `Site_RunSaveLoadOption(1)` ->
  `Save_ReadVerify` -> `site_LoadGame` 0x0805DF18 (confirmado no SV.md; chamador único: `Site_RunSaveLoadOption` 0x0805DDEC,
  o próprio GS 8 não foi lido). Pendente.
- Nome (GS 7, `0x0805C8EC`): sem função no Ghidra. A tabela de sub-passos da tela de nome é **0x084836E8**:
  [0] = `Menu_TextEntryInput` 0x0805EA44 (confirmado: teclado em grade, até 10 caracteres, `linha*0x1A + coluna + 1`),
  [1] = 0x0805EFD4, [2] = 0x0805EFFC, depois dados (0x23252509 e 0xFE, não ponteiros) e ponteiros 0x0805F144, 0x0805F19C.
  Ligação GS 7 -> essa tabela: a fazer (precisa do corpo de 0x0805C8EC).
- Chamada de `Menu_TextEntryInput`: nenhum xref de BL. Chamador provável = a tabela 0x084836E8 via o estado de nome. Não confirmado.
- Mapa de estado de MainState1 menu 10 = `new_game_start` (confirmado pela posição 0x083D212C = 0x083D2104 + 0x28,
  índice 10). Quem põe `gMainState=1` / `gamestate_menu=10` no caminho GS 7 -> nome -> NEW GAME: pendente.

## 7. Cadeia completa (estado atual)

1. `AgbMain` -> gMainState 3 / menu 0 -> `Boot_DispatchSubState` (GS 0x0300137C, tabela 0x084859D8):
   GS 0 reset (música para), GS 4 e GS 6 carregam o BG (LZ77) do logo/demo, GS 2 e GS 10 não lidos. Logo 0-511 e demo
   512-2881 e título 2882-3374 são GS do boot (ligação exata entre GS e a cena: pendente).
2. Em ~f3374 gMainState = 2 -> `Title_DispatchSubState` (menu 3, tabela 0x0848360C): GS 1 título, GS 2 checa save,
   GS 3 anima, GS 4 menu (CONTINUE / NEW GAME / OPTIONS).
3. NEW GAME: GS 6 fade -> GS 7 (nome, `Menu_TextEntryInput` via 0x084836E8) -> `new_game_start` (MS1 menu 10).
4. CONTINUE: GS 6 fade -> GS 8 -> leitura do save (`site_LoadGame`) -> mapa.
5. Primeira cena: `EventScript_RunCurrent` (MS1 menu 1). Evento de abertura **185** (gatilho walk-over em 18,13 no
   mapa 23, chapters=1; roda sozinho no 1º quadro, depois 187). Medido em `pc/research/README.md` (New Game e ST2_historia.md §6).

## 8. Pendente (resumo)

- Corpo de GS 0x0805C8EC (nome) e GS 0x0805C950 (continue) e demais sub-passos sem função (TL_lista.txt).
- Ligação GS -> cena do boot (logo, demo, título): não feita.
- Quem escreve gMainState=1/gamestate_menu=10 depois do nome: não achado.
- Tela de OPTIONS (GS 5, 0x0805C574): não lida.
