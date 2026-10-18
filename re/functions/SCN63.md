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

## SCN63b (2026-10-18): argumentos do 0x63 e códigos da tabela 0x080542B0
- Argumentos (decompile Ghidra `Event_Op63_Show_Effect` 0x0804D024, confirmado; literais 0x0804D070..80 = E 0x030057E0,
  offsets 0xC94/0xC95/0xC96/0xC48/0xC4A): são 3 bytes lidos em E+0xC94..0xC96.
  - id = E+0xC94 (estado de cena);
  - x = (E+0xC95 & 0x7F)*16 + u16(E+0xC48); y = (E+0xC96 & 0x7F)*16 + u16(E+0xC4A).
  - Ou seja: `tx`/`ty` são deslocamentos em tiles (16 px) a partir de uma origem u16 em pixels, em 0x03006428 (= E+0xC48,
    e 0x0300642A = E+0xC4A) que o extrator não lê. Quem grava essa origem não foi achado (GUESS: posição do evento).
    O porte guarda só `tx`/`ty` no cache.
- Porte: op renomeado para `ui_scene` (extract_events.py), `script.lua` chama `host.uiScene(id)`, `world.lua` só registra
  o id (sem efeito, sem espera). O foco de câmera de 60 quadros que existia era GUESS e foi removido.
- Tabela 0x080542B0 (140 ponteiros, lidos do ROM; Ghidra não tem função nos alvos, então o disasm é capstone via
  Python 3.12 do projeto, sem decompile):
  - 0x00 e 0x04 -> 0x080567FE: fim de passo (zera +0xFC, confere +0xF4 e a fase; GUESS "volta ao despachante").
  - 0x01 -> 0x080544E0: lê 3 palavras do script (estado 1: 0x400, 0x400, 0x80; estado 2: 0x400, 0x400, 0) para campos
    de viewport (+0x9C/+0xA0 como inteiro, +0x90/+0x94 via sf32), e grava +0x214=0x80, +0x218=0, +0x21C=0x400,
    +0x220=0x800. GUESS: enquadramento/escala inicial da cena.
  - 0x02 -> 0x080545B4: laço por quadro (tween de viewport via 0x08056F24). GUESS.
  - 0x18 -> 0x08055618: lê 5 palavras do script e grava no registro de 0x300 bytes do slot em +0x184 (base em
    0x0300xxxx de tabela de ponteiros): +0x178 = palavra 1, +0x180 = f32(palavra 2), +0x184 = palavra 3 e +0x188 =
    palavra 4 (GUESS nos nomes; o campo +0x184 é o índice do slot).
  - 0x22 -> 0x08055ED4: acrescenta 2 palavras a uma lista da cena (ponteiro de contador em +0x140, itens a partir de
    +0x188 + n*4; GUESS: lista de keyframes/posições). Termina chamando 0x080567FE. Não fechado.
  - 0x1B -> 0x08055A28: mesmo prólogo do 0x22 (lê 4 palavras do script depois do código, guardadas na pilha), depois
    indexa uma tabela ROM por uma variável e chama 0x0807CCD8 (GUESS: criar sprite/objeto). Não fechado.
  - 0x80 -> 0x080560BC: fade de tela (`Overworld_ResetAndStartScreenTransitionEffect` 0x08049D90), confirmado antes.
  - 0x8B: entrada final (Overworld_EnterSceneState).
- Estados (scripts lidos do ROM): estado 1 = `1,0x400,0x400,0x80, 0x18,...,0x22,...,0x22,...,0x04,0x1B,...`; estado 2 =
  `1,0x400,0x400,0, 0,0x22,...,0x04,0x1B,..., 0x8BC4,0x100,0, 6,...`. Os códigos usados são 0x01, 0x18, 0x22, 0x1B, 0x04,
  0x00 e 0x02 (o código 0x02 é o laço por quadro e não aparece no script, só no despachante).
- Efeito final por estado: NÃO FECHADO. Sabe-se que o estado mexe no viewport (0x01/0x02) e monta uma lista de itens
  (0x22) com um sprite/objeto (0x1B), mas não se o HUD some, quanto dura, nem se trava o jogador. Não se deve portar um
  efeito por estado sem ver o print do jogo no mapa 24 ev 200 (estado 2) e no ev 26 do mapa 0.

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
