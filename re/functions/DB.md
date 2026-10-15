# Menus de debug (DB) — Medabots Metabee (EU)

GH-DB, 2026-10-14. Método: decompile/disassemble/xrefs no Ghidra, literais e tabelas lidas da ROM original
(python -I, só leitura). Nada foi validado no emulador ainda: tudo abaixo é leitura de código, "confirmado" =
lido no decompile + literal, "provável" = inferência de nome ou de fluxo.

## Como se entra

- **Não há combinação de teclas na ROM final.** `menu_list[63]` (0x083AC2C8 + 63*4) aponta para `DebugMenu_Dispatch`
  (0x0800B890), mas nenhum caminho de jogo escreve 63 em `gamestate_menu` (0x03002B8C) para abrir o menu.
  Xrefs de escrita em 0x03002B8C: `Debug_ProcessJumpScreen` (0x0800BE32) e `DebugMenu_Exit` (0x0800BA8E) são as
  únicas dentro do próprio menu de debug; o resto são cenas do jogo (confirmado por xref, não por caminho de uso).
- Entrada de fora: via debug_poke (research/README: escrever 0x03002B8C=63, 0x0300178C=0, 0x0300137C=0 num savestate).
- Dentro do menu, a cena 63 pode ser reescrita pela opção Process (ver abaixo).

## Estrutura

- `DebugMenu_Dispatch` 0x0800B890: `CallViaR0(0x083ACA5C[gamestate_subroutine])`. Subrotinas (confirmado):
  - 0: `DebugMenu_Init` 0x0800B8B0 — liga sprites (REG_DISPCNT |= 0x1000), copia OAM de 0x03002C30 para 0x02020800
    (provável backup), limpa OAM, carrega tiles LZ77 0x084C8034 → VRAM 0x06017400, paleta 0x084E43D8 → OBJ pal 15,
    avança passo.
  - 1: `controls_in_debug_menu` 0x0800B938 — lê input e troca opção ou entra nela.
  - 2: `DebugMenu_Exit` 0x0800BA74 — restaura OAM (0x02020800 → 0x03002C30) e volta para cena/passo salvos
    (0x03000470 gDebugReturnScene → gamestate_menu; 0x03000471 gDebugReturnStep → gamestate_subroutine).
  - 3: `DebugMenu_RunOption` 0x0800BAB8 — `CallViaR0(0x083ACA80[gDebugMenuOption])`.
- Estado do menu: `gamestate_subroutine` 0x0300137C (passo: 0 init, 1 controles, 2 sai, 3 executa opção).
  `gDebugMenuOption` 0x03000472 (0..10). Entrada 11 da tabela = NoOp (0x08000840).

## Controles no menu principal (`controls_in_debug_menu`, confirmado)

- `just_pressed_buttons` 0x0300058C. Direita (0x10) soma 1 à opção (wrap 10→0); esquerda (0x20) subtrai (wrap 0→10).
- Nome da opção: 8 bytes copiados de 0x083ACBA0[opção] e desenhados; piscam com `gFrameCount` bit 2 (0x03001780).
- A (0x01): passo 3 (executa a opção). B (0x02): passo 2 (sai).

## Opções (tabela 0x083ACA80, confirmado; nomes do debug_menu_text 0x083ACBA0)

| # | Nome | Despachante (ROM) | Base na tabela de estados 0x083ACAC0 | Observação |
|---|---|---|---|---|
| 0 | Process | Dispatch_StateHandlerTable 0x0800BAD8 | 0 | estado 0 = `Debug_ProcessJumpScreen` |
| 1 | Medal | BE6C 0x0800BE6C | 3 | estado 0 = `Debug_PartsSpawnScreen` |
| 2 | Parts | C29C 0x0800C29C | 6 | estado 0 = `Debug_ItemCountGrantScreen` |
| 3 | Tinpet | C6D8 0x0800C6D8 | 9 | não decompilado ainda |
| 4 | Item | 0x0800C7E4 | — | **pendente**: função desde o sync de 2026-10-14 (nome: DebugMenu_Option4_Dispatch) (miolo de 0x0800C6F8?) |
| 5 | Face | CBA8 0x0800CBA8 | 15 | não decompilado ainda |
| 6 | Mdlpic | 0x0800CE70 | — | **pendente**: função desde o sync de 2026-10-14 (nome: DebugMenu_Option6_Dispatch) (miolo de 0x0800CBC8?) |
| 7 | Rnd-tk | D168 0x0800D168 | 21 | não decompilado ainda |
| 8 | Battle | D25C 0x0800D25C | 24 | estado 0 = `Menu_TickSingleCountSelector` (0x0800D27C) |
| 9 | Sound | D3C4 0x0800D3C4 | 27 | não decompilado ainda |
| 10 | Motion | `Debug_MotionDispatch` D664 0x0800D664 | 30 | k0 = limpa OAM, k1 = monta camadas, k2 = Main, k3 = PlayKind |

Os despachantes usam o mesmo byte de estado `0x03000473` (ctx 0x03000440 + 0x33) sobre a mesma tabela de 34
ponteiros 0x083ACAC0 (`StateHandlerTable_BAD8`): cada opção começa em outro deslocamento. Nomes do menu (Process,
Medal, Parts...) não coincidem com o que as telas fazem na leitura atual (Medal abre o spawn de peças, Parts abre o
contador de itens): o rótulo pode estar deslocado; **confirmar no emulador**.

## Telas de debug (padrão comum, confirmado nos decompiles)

- Cursor de linha: `0x03000474` (ctx+0x34), 3 linhas, 0x40 (cima) / 0x80 (baixo), wrap 0..2.
- Valores das linhas: `0x03000475`, `0x03000476`, `0x03000477` (ctx+0x35..0x37). Direita/esquerda com repeat
  (`0x03001798`) mudam o valor da linha do cursor.
- Texto: 8 bytes de nome copiados para 0x030004DE, piscando por `gFrameCount` bit 2; números 6 dígitos
  (`Num_FormatDecimal6Glyphs`).
- A (0x01) executa a linha; B (0x02) limpa OAM e grava `gamestate_subroutine` = 1 (volta aos controles).

### Process (`Debug_ProcessJumpScreen` 0x0800BAF8, renomeada) — confirmado
- A: `gMainState` 0x0300178C ← v0 (0x03000475); `gamestate_menu` 0x03002B8C ← v1 (0x03000476);
  `gamestate_subroutine` 0x0300137C ← v2 (0x03000477). Ou seja, pula para outra cena/passo do jogo.
  Pode escrever 63 em gamestate_menu e voltar ao menu de debug.

### Medal → Spawn de peças (`Debug_PartsSpawnScreen` 0x0800BE8C) — confirmado
- Valores 0..0x1D (29). Linha 0 (A): se v0 < 0x1E e não há objeto com aquele id, `Medabot_SpawnFromRecord(v0)`
  e copia 11 bytes do registro (0x083B6590[v0]) para 0x03000BEA + slot*0x40.
- Linha 1 (A): `Slot_ReleaseById(v1)` se o objeto existir (remove).
- Linha 2 (A): laço id = v2..0x1D: spawna os que faltam.
- Nomes da linha: 0x083ACC08.

### Parts → Contador/concessão de itens (`Debug_ItemCountGrantScreen` 0x0800C2BC, renomeada) — confirmado
- Valores 0..0x77 (119 itens). Linha 0 (A): `Inv_AddCountClamp99(v0, campo 0..3, +1)` em 4 campos.
- Linha 1 (A): `Inv_SubCountClampZero(v1, campo 0..3, -1)`.
- Linha 2 (A): laço id = v2..0x77 com +1 em 4 campos (concede em massa).
- Nomes: 0x083ACC2C. Sem checagem de cheat/flag: é o que o código faz; **que o menu de jogo nunca chama isso,
  confirmar**.

### Battle → `Menu_TickSingleCountSelector` 0x0800D27C — provável
- Linha única; bits 0x10/0x20 mudam o índice; A grava o valor e (xref) escreve gamestate_menu (0x0800D3A4).
- Não decodificado por completo: **pendente**.

### Motion (Debug_MotionDispatch 0x0800D664 → 4 estados) — confirmado no código
- k0 = `Menu_ClearObjectAttrsAndAdvance` 0x0800D684 (limpa OAM e avança).
- k1 = `FightPreview_BuildLayers` 0x0800D6A8 (monta 2 camadas de sprite do bot).
- k2 = `DebugMotion_Main` 0x0800D710:
  - bot 0..121 no ctx+0x39 (0x03000479); START/SELECT troca o bot (vai para k1, reconstrói as camadas);
  - Direita/Esquerda (repeat 0x03001798) altera `gDebugMotionKinds[cursor]` (0x03000475 + cursor), sem limite;
  - A avança para k3; B limpa OAM, zera 0x03000430 e grava 0x0300137C = 1 (sai).
- k3 = `DebugMotion_PlayKind` 0x0800D90C: toca `MotionPlayStep(lado, kind)` nos dois bots a cada frame; B recua 2 estados.
- Cursor 0x03000474 (gDebugMotionCursor) — nesta tela não é alterado (sem cima/baixo em Main); **confirmar de onde
  vem a mudança de linha**.

## Pendente

- Opções Item (0x0800C7E4), Mdlpic (0x0800CE70): entradas Thumb na tabela, função desde o sync de 2026-10-14 (nomes: Item = DebugMenu_Option4_Dispatch, Mdlpic = DebugMenu_Option6_Dispatch). Decodificar a mão
  ou testar no emulador.
- Tinpet, Face, Rnd-tk, Sound: despachantes conhecidos (tabela de base acima), telas não decompiladas.
- Fluxo entre estados da tabela 0x083ACAC0 (quem incrementa 0x03000473 e com qual passo, e o que é NoOp).
- `FUN_0805db20`, entradas sem função em JT3 (ver JT3.md): fora do escopo deste DB.
- Nenhum caminho de uso do jogo abre o menu de debug: confirmado só por xrefs e pela ausência de combinação.
