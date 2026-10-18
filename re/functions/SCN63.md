# SCN63: despachante de estado de cena (opcode 0x63 -> Overworld_BeginUiSceneState) (2026-10-18)

Parcial. Estrutura confirmada no decompile; efeito visual por estado ainda não fechado.

## Cadeia
- Evento 0x63 (primário 0x0804D024): `Overworld_BeginUiSceneState(args0, x, y)`. args0 = estado de cena (id 1 = 86 eventos, id 2 = 112).
- Continuação 0x0804F6E0 espera `Map_ModeDispatch` (0x08050BC8). Só os estados 0xD e 0x16 andam o objeto 0x2F.
- Despachante `Scene_DispatchStateHandler` 0x0805424C [confirmado, Ghidra]:
  - estado = `gOverworldUiState & 0x3F` (struct 0x03005350, +0x1D4); marca +0xF0 = 1, grava +0xF8.
  - sub-tabela = `*(0x083D5F70 + estado*4)`; passo = `sub[+0xFC]` (contador de passo, 4 bytes por entrada).
  - código = `sub[passo]`. Se código > 0x8B chama `Overworld_EnterSceneState` 0x080567F0 (que zera +0xF4/+0xFC/+0xF0, recalcula 32 entradas affine se estado não é 0xD/0x16, e reposiciona o jogador para 0xE/0x12).
  - `Scene_DispatchStateHandlerTail` 0x08054280 faz `mov pc` na tabela 0x080542B0 (140 ponteiros, código 0..0x8B).
- `Overworld_IsUiSceneStateComplete` 0x0804B6E0 = 1 quando o despachante devolve 4 (estado concluído).

## Scripts dos estados 1 e 2 (palavras u32 lidas do ROM)
- Estado 1 (0x083D33F0): `1, 0x400, 0x400, 0x80, 0x18, 0x22, 0, ptr, 2, 0x22, 0, ptr, 0x22, 1, ptr, 2, 4, 0x1b, 0, 0`.
- Estado 2 (0x083D35C0): `1, 0x400, 0x400, 0, 0, 0x22, 0, ptr, 2, 4, 0x1b, 0, 0, 0x8BC4, 0x100, 0, 6, 0, 0, 0`.
- 0x400 = passo que chama `Overworld_EnterSceneState` (valor > 0x8B).
- Código 1 (0x080544E0, sem função no Ghidra; disasm capstone): lê palavras do script (sb + passo) para +0x9C/+0xA0/+0x90/+0x94 (via 0x0807E5B8 sf32_from_i32), e grava +0x214=0x80, +0x218=0, +0x21C=0x400, +0x220=0x800 (campos de escala/viewport; significado GUESS). Chama 0x080567FE 2 vezes.
- Código 2 (0x080545B4 = SceneState_02_UpdateActorLoop, nome do RF7/SC4 = chute): chama 0x08054736, 0x0805476E, 0x080547B2, 0x08054D44/48, 0x08056F24 (Viewport_TweenAdvanceCheckDone: compara float = tween de viewport/câmera), 0x08053464, 0x080545C6.
- Código 0x80 (0x080560BC = SceneState_80_FadeReset, confirmado): chama `Overworld_ResetAndStartScreenTransitionEffect` 0x08049D90 e termina em 0x080567FE. Fade de tela (escurece/clareia) = a parte visual mais próxima já identificada; o estado 1 o usa no passo 3.
- Código 0x8B (0x080567D4): entrada final tratada por Overworld_EnterSceneState.

## O que ainda não está confirmado
- Se o HUD some, se o jogador trava (trava por `Map_ModeDispatch`?), duração em quadros e a música (não visto no trecho).
- Saída: o script chega a 0 (fim) e a cena volta sozinha (estado 4) ou outro opcode desliga; não verificado.
- Cada um dos códigos 0x18/0x22/0x1B/0x04 do script ainda sem leitura própria.

## Confiança
- Cadeia de despacho e tabelas: ALTA (Ghidra + ROM).
- Código 0x80 = fade/transição de tela: ALTA (nome e chamada confirmados).
- Estados 1/2 = sequência de fade + tween de viewport + atores: MÉDIA (chamadas identificadas, semântica dos argumentos GUESS).
- Objeto 0x2F nos estados 1/2: NÃO confirmado (o pedido do A4 de sprite de objeto não se sustenta).

Simbolos: `SCN63_simbolos.csv`. Atualize o nome do 0x0804D024 e do 0x0804F6E0 em HC4.md (Event_Continuation op 0x63 = espera de cena).
