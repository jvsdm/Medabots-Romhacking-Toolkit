# PS: telas de peças / montagem / grade de Tinpets (Medabots Metabee EU)

Status: PARCIAL (2026-10-14, GH-PS). Confirmado: grade 3x3 (estados e cursor), montagem (painel e editor), troca no time, inventário de peças. Pendente: grade de peças (PartsGrid_*, 4 InitScreen candidatas) e PartDetail_*. Ghidra não salvo.

## Mapa de telas (rascunho, confirmado no Ghidra onde marcado)

- Tela do Medabot selecionado (preview 3x3): `UpdateSelectedMedabotPreviewScreen` 0x08013E30
  (NÃO 0x08013E84: não há função nesse endereço no Ghidra; mtbe.csv tinha 0x08013E30). Confirmado.

## 0x08013E30 UpdateSelectedMedabotPreviewScreen (confirmado no decompile)
- Assinatura: `void UpdateSelectedMedabotPreviewScreen(void)`
- Por quadro: anima o preview (`AdvanceSelectedMedabotPreviewAnimationSlot`), desenha o frame grande
  (`RenderSelectedMedabotLargePreviewFrame`), `Preview_NudgeOffsetIfFlag`.
- Estado = byte ctx+0x3F (ctx = DAT_08013e58, literal pool; endereço absoluto a confirmar):
  - 0 (entrada/cursor): tick de piscar (`Ui_TickBlinkCounter`), desenha slots exceto o selecionado
    (`Fight_DrawRowBotSlotsExceptSelected`), `Slot_SetPosStepByMode(8,...)`, depois
    `HandleSelectedMedabotPreviewCursorInput` 0x080183A4. A ou START-like (bit 1 de DAT_08013eb8+1 com
    ctx+0x3C != 0) -> SFX 2, `Fight_SlotConfirmStartSlide`, ctx+0x40=0, estado=1. B (bit 2 em ctx de entrada
    com cursor = 1) -> SFX 2 + `Fight_SlotConfirmStartSlide` (mesma saída). Bit 2 sem o outro -> SFX 5 +
    `SetGpuBlendMode(6,0x3F,0x20)` e incrementa contador em DAT_08013f1c (sem mudar estado).
  - 1: `Fight_SlideSideTransitionStep` até 0; ao terminar ctx+0x40=0, estado=2.
  - 2: `Fight_SlideSideBackStep` até 0; estado=3.
  - 3 (carga): `Fight_GetDisplayedPartIdsOrBrokenGlyphs(idx, buf)` com idx = ctx+0x3C*3 + ctx+0x3D (índice 0..8
    da grade 3x3 de Tinpets, coluna-major ou linha-major: a confirmar com a ordem das bandeiras), depois
    `SetSelectedMedabotPreviewPartIds` (4 ids: cabeça/braço D/braço E/pernas), `InitializeSelectedMedabotPreviewRenderer`,
    `SetCompositeBattleSpriteAnimationSpeed(0,8)`, `FillBgTilemapRectSequential(2,2,11,2,...)`,
    `RenderSelectedMedabotPreviewFrame`; estado=0.
- Confiança: confirmado (leitura do decompile). Nomes "Fight_*" são herdados da batalha: a função é reusada
  pela tela de Medabots (confirmar por xref de quem chama, pendente).

## Literais e contexto (lidos na ROM original, `Medabots - Metabee (Europe).gba`)
- Contexto da tela de Medabots/grade: ctx = IWRAM **0x03000440** (estado em +0x3F, cursor coluna +0x3C, linha +0x3D).
- Teclas segurando: byte **0x03001798** (bits: A=1 B=2 SELECT=4 START=8 RIGHT=0x10 LEFT=0x20 UP=0x40 DOWN=0x80).
- Lista de 4 ids da prévia (cabeça, braço D, braço E, pernas): **0x03001864** (4 bytes).
- Bloco de bot da célula: **0x03000590 + k*0x100** (k = 0..8 da grade); flag de célula +0x40 do bloco (provável: "ocupada").
  Obs.: Ghidra +0x40 = pesquisa +0x00 (mesma regra de README).
- Tabela de posição de cursor: 0x083AD524 (9 células x 2 bytes, x/y).

## 0x080183A4 HandleSelectedMedabotPreviewCursorInput (confirmado no decompile, 0x080183A4..0x08018597)
- Move o cursor (3 colunas x 3 linhas) com DPAD e só para em célula ocupada (flag +0x40 do bloco de bot de destino).
  Movimento vertical na coluna 0 (UP/DOWN); horizontal entre colunas (LEFT/RIGHT); a coluna 0 e 2 têm regras
  de borda (de linha 0 pode saltar para linha 2 se ocupada). Tocar o cursor: SFX 2, redesenha nome, painel de
  peças (`Fight_DrawFourPartIcons`, `Fight_DrawFourPartRowsWithStats`, `Fight_DrawFourPartStatColumns`),
  recarrega ids com `Fight_GetDisplayedPartIdsOrBrokenGlyphs(col*3+row, 0x03001864)` e `SetSelectedMedabotPreviewPartIds`.
- O índice da célula é `coluna*3 + linha` (ordem coluna-major, igual à regra de pods do README).
- Confiança: confirmado (decompile); faixas exatas das regras de borda: provável.

## 0x0800C6F8 PartsBuild_UpdateStatusPanel (confirmado, 0x0800C6F8..0x0800C7E1)
- Painel de status da MONTAGEM (chamado pela tabela 0x083ACAE0, coluna do meio, linha 0 = `PartsBuild`).
- Se flag `*0x0800C714 & 4` = 0: copia 8 B do registro (`*0800C7AC`) para o buffer do painel (0800C7B0) e
  desenha 8 glifos (`Text_LayoutGlyphSprites`); senão limpa sprites (`ClearObjectAttributeEntries(8,8)`).
- Byte em `*0800C7B4 & 0x30` (LEFT/RIGHT): inverte o campo de 1 bit em ctx+0x35+ctx[0x34] (seleção A/B).
- Mostra número de 6 dígitos de `buffer-0x69` (`Num_FormatDecimal6Glyphs`).
- Confirmar (byte `*0800C7BC`): bit 1 = fim/cancelar (limpa sprites, marca `*0800C7C0 = 1`); bit 0 =
  `Tinpet_CreateRecord(numero)` se `Tinpet_IsPoolFull()==0`.
- Confiança: confirmado no corpo; nomes do painel = provável.

## 0x0800D3E4 PartsBuild_UpdateEditPanel (confirmado no decompile, 0x0800D3E4..)
- Editor da montagem. Tabela 0x083ACB2C (linha de `PartsBuild_UpdateEditPanel`).
- Alterna a linha ativa ctx+0x34 (0/1) com UP/DOWN (bits 0x40/0x80 de `*0800D420`).
- Linha 0: mostra o registro de 8 B `*0800D49C` (painel esquerdo, 2 linhas de glifos) e `*0800D49C[1]` no
  painel direito. Linha 1: lado oposto (`*0800D538`). Cada lado usa a flag `*0800D4BC & 4` (vazio: limpa).
- RIGHT (0x10 em `*0800D540`) e LEFT (0x20 em `*0800D584`) incrementam/decrementam o dígito em ctx+0x35+sel:
  sel 0 = faixa 0..0x7F (wrap), sel 1 = faixa 0..5 (wrap em '5' -> 0).
- Mostra 2 números de 6 dígitos (`ctx-0x69` e `ctx-0x68`).
- Confirmar (`*0800D628`): bit 1 = fim (música 0, limpa sprites, `*0800D62C = 1`); bit 0 = SFX ou música
  conforme o byte `ctx-0x6A`.
- Confiança: confirmado no corpo; o significado de "dígito sel 0 (0..127) / sel 1 (0..5)" = provável (parece
  seleção de gênero/medalha/peça, não confirmado).

## 0x080058A4 Team_HandleSwapSelectInput (renomeada de Parts_HandleSlotSwapInput; confirmada no decompile)
- Troca de posição do Tinpet no TIME: `Mem_SwapBytes(0x030017A0 + sel*8, 0x030017A0 + cursor*8, 8)`
  (literal 0x08005924 = 0x030017A0; ctx 0x0800591C = 0x03000440; tecla pressionada 0x08005920 = 0x0300058C).
- Entrada: ctx+0x50 == 0 e SELECT (bit 0x04 de 0x0300058C, borda). SFX 2.
  - 1ª vez: grava `cursor+0x80` em ctx+0x52 e chama `init_4_slots_b` (marca a origem).
  - 2ª vez: `sel = ctx+0x52 & 0x7F`; se sel != cursor (ctx+0x51) troca os 8 bytes e redesenha (grade de
    ícones `Menu_DrawPartIconGrid`, medalha, nome, paleta, prévia `BattlePartPreview_Rebuild`,
    `Parts_CheckSpriteIdsSetSpeed`); zera ctx+0x52; `aplica_mascara_slots_b`. Retorna 1 (consumiu a tecla).
- Grava no time = só a troca (sem contagem de peça); o registro 8 B é o da tabela 0x030017A0
  ([estado, gênero, medalha, cabeça, braço D, braço E, pernas, índice]), Tinpet_InitIds 0x080421A0 grava ids 0..8.
- Confiança: confirmado (decompile + literal lido na ROM).

## 0x08043560 AdjustInventoryPartCount (confirmado; nome já existente)
- Inventário de PEÇAS (IWRAM base **0x03004C40**, literal 0x080435AC): endereço = `base + tipo*0xF0 + bot*2 + campo`
  (4 tipos x 120 bots x 2 B). campo 0 = quantidade, campo 1 = equipado/uso. (tipo 0 cabeça, 1 braço D, 2 braço E, 3 perna:
  ordem de BT2 em `pc/research`.)
- `param_4 == 0`: soma `param_5` na quantidade, teto **99**. `param_4 != 0`: subtrai `param_5` no campo do bot (sem piso).
- Confiança: confirmado no decompile; ordem dos tipos = provável (BT2).

## 0x080436AC PartSelect_CycleNextValid (confirmado no corpo; nome já aplicado no Ghidra)
- Anda pela lista de bots (0..0x77, sentinela 0x78 = gabarito/"sem peça") procurando a próxima peça válida do
  tipo `param_4`: `gPartStats` (0x08042B58 GetPartStats) byte +3 == tipo, quantidade != 0 e quantidade != campo 1.
  Sentido `param_3 == 0` avança (+1, pula 0x78 e 0x79 conforme o caso), `!= 0` volta.
- Se nada é achado retorna `param_4 + 0x78`.
- Confiança: confirmado a estrutura; condição "quantidade != equipado" = provável (pode ser "quantidade > equipado").

## 0x08013E30 + 0x080183A4: grade 3x3 do Medabot selecionado (resumo)
- Tela de ESTADO de batalha/menu: tabela 0x083AD0F0 (passo 4), entrada 9 = esta função; entrada 10 = `Fight_SelectScreenFrame` 0x08013FA8.
- Estados (ctx+0x3F): 0 cursor (o botão de confirmar, bit a confirmar no byte 0x0300058D, leva ao estado 1 = slide; o outro bit do mesmo byte = SFX 5 + blend, sem mudar estado);
  1 slide-out; 2 slide-back; 3 carga de peças (0x03001864 + `SetSelectedMedabotPreviewPartIds`).
- Cursor (0x080183A4): grade 3 colunas x 3 linhas, índice = coluna*3+linha; só para em célula ocupada.
- Célula ocupada = flag +0x40 do bloco de bot 0x03000590 + k*0x100 (provável; confirmar lendo o Tinpet escrito por Tinpet_InitIds).

## 0x08009174 PartsGrid_ReloadRowTilesAndSetup (confirmado no corpo)
- Tick de estado da grade de peças: `Timer_IncPairIfFlag`, `PartsGrid_RefreshCursorSprite`, limpa bits em +0x40 e
  +0x80 (`& mask`), recarrega 0x350 bytes de tiles (`LoadTilesWithMalias` 0x12x0x0C), zera +0x4A, chama
  `PartsGrid_SetupRowSelectSprite`, incrementa o contador de estado.

## Estado de telas de montagem e grade (o que faltou)
- Tabela de estados da grade de peças: 0x083AC880 (registros de 0xC B, colunas init/tick/…): entradas
  PartsGrid_InitScreen 0x08007A04 (linha 1, coluna 0). A tabela não tem alinhamento consistente nas colunas: pendente.
- Existem 4 funções chamadas `PartsGrid_InitScreen` (0x08007A04, 0x08007E38, 0x08008234, 0x0800DA34) em tabelas
  diferentes (ponteiros nos literais 0x083AC88C, 0x083AC8CC, 0x083AC90C, 0x083ACCD4). Nenhum confirmado como o da grade
  do Medabot selecionado; PENDENTE: decompilar 0x08007A04 e 0x0800DA34 e cruzar com a tabela.
- `PartsGrid_EnterScreenInit` 0x080087CC (entrada 17 da tabela 0x083AC880): pendente.
- `PartDetailScreen_Init` 0x0800E020 e `PartDetail_SceneStep` 0x0800EA2C: não lidos. Pendente.
- Tinpet_IsPoolFull 0x08042218 e site_TinpetCreate 0x08042238: já confirmados em R2 (pool de 9 entradas).

## Pendente
- Mapa exato coluna/linha da grade 3x3 (regras de borda em 0x080183A4) e o significado dos dígitos de
  PartsBuild_UpdateEditPanel (sel 0 0..127 / sel 1 0..5).
- Quem chama a tabela 0x083AD0F0 (estado da tela de Medabots) e a entrada que abre a tela a partir do menu.
- Tela de detalhe de peça (PartDetail_*, 0x0800E020..0x08010xxx) e a grade de peças (PartsGrid_*).
- Os blocos ROM de origem de PartsBuild (0x083ACC38, 0x083ACCB0): não está confirmado se são gabaritos vazios.
