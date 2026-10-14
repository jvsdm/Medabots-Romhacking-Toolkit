# HD2: pendências da HUD da batalha (GH-HD2, 2026-10-14)

Complemento de `HD.md` (não substitui). Endereços da ROM EU (Ghidra). Ghidra não foi salvo; 4 funções renomeadas
(ver `HD2_simbolos.csv`). Leitura de literais feita com `python -I` sobre a ROM original.

Correção ao HD.md: o estado do cursor 3x3 e do cursor de escolha fica em `0x03000440` (+0x3C coluna, +0x3D linha),
não em `fi+0x3C`. O mesmo bloco 0x03000440 é o do timer de HD.md seção 2 (+0x86) e do contador +0x84.

## 1. Barras de armadura das 4 peças e descida animada

### 1a. Quem NÃO é a barra
- `Fight_RefreshBotDisplayedPartSprites` 0x08015A00: pega 4 ids (`Fight_GetDisplayedPartIdsOrBrokenGlyphs(bot, fi+0x64)`,
  fi+0x64 = 0x03001864), chama `Sprite_WriteComposite4Objects` (0x0804305C) com slot `bot*4+0x5A` e limpa a máscara de
  atributo dos slots `bot*4+0x5B` e `+0x5C` (`0x03002C30`, mask 0xFDFF) e zera +0x18. É o sprite composto das peças
  (OBJ), não a barra. Chamada por `Fight_BotSelectMenuFrame` 0x08012D2C, 0x08012F80, `Fight_PlaceActiveBotObjects` 0x0801A0F2,
  0x0801443E.
- `Fight_BuildBotPartDisplayStats` 0x0801520C: só monta o bloco de exibição dos bots de lado 2 (count = fi+0x5B; bloco
  `0x030008D0 + i*0x100`, flag +0x40 = 3, ids das 4 peças de fi+4+i*0xC via `GetPartStats`). Chamada por
  `Fight_InitializeEnemyBotStates` 0x080150E0. Não desenha nada.

### 1b. Desenho das 4 linhas de armadura — `Fight_DrawFourPartStatColumns` 0x08017F9C (confirmado no decompile)
- 4 linhas em y = 4, 6, 8, 10 (passo 2). Base do bloco do bot = `0x03000590 + (col*3 + row)*0x100` (col/row em
  0x03000440+0x3C/+0x3D), +0x18 por peça.
- Armadura da peça k: byte +0x59+0x18k, escrito em 3 dígitos com `Num_DrawDecimal6BigTiles` (0x08042950) em x=0x17.
  Tile base 0xA000 se armadura != 0, senão 0xB000, OR 0x390.
- Segundo número: byte +0x4C+0x18k em 3 dígitos, x=0x1B (significado não confirmado; "+0x4C" = pesquisa +0x0C).
- `FillBgTilemapRectSequential(1,2,0x1A,y, mapa, (tile)|0x3A4)`: preenchimento de 1 tile de largura na coluna 0x1A
  (candidato a "barra": preenche o trecho entre armadura e o número; forma visual não verificada no emulador).
- Chamada por: `HandleSelectedMedabotPreviewCursorInput` 0x08018534 (cursor 3x3 da tela de prévia, ver item 2) (QA3), `Fight_BattleSceneInit`
  0x08013CD8, `Fight_SlideSideBackStep` 0x080188EC.
- Confiança: confirmado (código); o que é a "barra" visual = provável.

### 1c. Descida animada — `Fight_AdvanceSlotYByWaveTable` 0x08036AC4 (renomeada; antes `Fight_AdvanceSlotXByWaveTable`)
- Confirmado no decompile. Opera sobre OBJ slot `0x03002C30 + slot*0x40`; o campo de +4 do slot é **Y** (o cursor
  de 0x080183A4 escreve y em +0x204 = slot 0x40 + 4; mesma convenção).
- Se o bit 0x8000 do attr0 está ligado e o contador do slot (+0x23) != 0x10: `Y += onda[contador]` (tabela `u8` em
  0x083AF620, ponteiro em 0x08036B18), contador++; se flag = 1, grava +9 = (contador+1)*4 + 0x40.
  Ao chegar a 0x10 para de descer.
- Chamado por `FightPreview_AdvanceRowSlots` 0x08036B50 (lista de slots terminada em -2, até 5), que é chamada 4 vezes
  (uma por linha, em 0x08026A2C..0x08026A4A) por `Fight_DispatchOnSubstateAA_AO_WaitPartRowsDrawn` 0x08026A24, e por
  `Fight_DispatchOnSubstateAA_H_11_WaitPartIconRows` 0x0801EA0E.
- Ícones das 4 peças: `FightPreview_DrawPartIcon...` 0x08036BB8, chamada por `AO_DrawPartIconsWaitReady` 0x08026A74,
  `H_11_WaitPartIconRows` 0x0801EA0E e `FightPreview_StepSideLoad` 0x0803D358..0803D37E.
- Confiança: confirmado (código); conteúdo da tabela de onda não lido (dado da ROM 0x083AF620).

### 1d. Leitores e escritores de fi+0xC8 / fi+0xCC / fi+0xD1 (0x03001800)
- Escritor de +0xC8/+0xCC/+0xD1: `Fight_CopyPartArmorToDisplayValues` 0x08036B84 (copia armadura +0x59 e marca +0xD1=1).
  Escritor de +0xD1 = 1 quando a peça quebra: `Fight_ApplyDamageToPart` 0x08034F68 (CF.md).
- Leitores (pelos docs HA1, HA2, HA3, HA7; não refeito por xref nesta passagem): HA1 (+0xD1 != 0 vai para 0x1A, senão
  0x20), HA2 (+0xD1 == 0 avança 1 passo, senão 3 passos e `Fight_BackupHalvedStatsIfFlagClear`), HA3 e HA7 (dígitos
  de +0xC8 via `Fight_WriteNumberTiles`).
- Escritor do tilemap do painel: as funções do item 1b e `Overlay_DrawSixDigitRow` usam o mapa vindo de
  `*0x0400000A` (ver item 3).
- Confiança: escritores confirmados; leitores = docs (provável).

## 2. Cursor de seleção 3x3 da tela de prévia (escolha de peça) (QA3)

- `HandleSelectedMedabotPreviewCursorInput` 0x080183A4 (0x080183A4..0x08018597) é o cursor 3x3 da **tela de prévia** do Medabot (QA3; escolha de peça dentro da preparação da luta). Não é o cursor da luta em outro quadro.
  Único chamador: `UpdateSelectedMedabotPreviewScreen` 0x08013E84 (QA3). A leitura "da luta" deste arquivo estava errada.
- Estado: `0x03000440`: +0x3C = coluna, +0x3D = linha; índice = coluna*3 + linha. Teclas lidas de `0x03001798`
  (mascara 0xA0/0x50 para a coluna 0; 0x90/0x60 para as colunas 1 e 2).
  Movimento só vertical nesta função, com volta; só move para célula com flag 0x40 do bloco do bot (`0x03000590 + idx*0x100`)
  ligada. Quando move: sfx 2 e redesenho (itens 1b e abaixo).
  Mapeamento exato de bit a seta = provável (validar no emulador antes de portar).
- **Quem desenha o cursor**: `Fight_DrawSelectCursorObject` 0x08017B24 (renomeada; antes FUN_08017B24). Escreve o OBJ
  slot 0x40 (`0x03002C30+0x200`): attr0 0xC404, y em +0x204, x em +0x202, tile em +0x206 (0x0306 se coluna 0, 0x0307 se
  coluna > 0), +0x20A = 0x0E, +0x20B = 0xC0, +0x21C = 0. x = célula.x + 0x10 (coluna 0) ou célula.x - 0x10 (coluna > 0).
  Posição das células: tabela `0x083AD524` (pares x,y; índice = (linha + coluna*3)*2).
  Chamada por: 0x080183A4 (0x0801851C), `Fight_BattleSceneInit` 0x08013CBC, `Fight_SlideSideBackStep` 0x080188D4.
- Cursor de escolha de **peça** ou **alvo**: NÃO achado nesta passagem. A função 0x08017B24 só posiciona o cursor entre
  bots (3x3). Próximo passo: ver quem escreve o OBJ 0x40 com tile diferente de 0x306/0x307, e a rotina de escolha
  de alvo (fora de 0x080183A4).
- Confiança: confirmado (cursor de bot e OBJ); cursor de peça/alvo = pendente.

## 3. Destino BG do gauge e origem de *0x0400000A

- `0x0400000A` é o registrador de hardware **BG1CNT** (u16). Os desenhadores usam `*(u16*)0x0400000A` como o "mapa"
  passado a `FillBgTilemapRectSequential` 0x08042698 e `Num_DrawDecimal6BigTiles` 0x08042950 (confirmado nos literais
  de `Fight_DrawFourPartStatColumns` 0x08017F9C, `Fight_DrawFourPartRowsWithStats` 0x08017EA8, Overlay_DrawSixDigitRow
  0x0803E8EC e Fight_DrawBotGaugeTilemap 0x08015C40).
- Destino: o screen-base block do BG1 (o valor de BG1CNT dá SBB em bits 8..12). Ou seja, o gauge de 8 células e o
  painel de dígitos vão para **BG1**, não para BG0.
- Origem do ponteiro: não é ponteiro de RAM, é o próprio registrador. Quem grava BG1CNT não foi rastreado (provavelmente
  a inicialização da cena de luta, `FL2`, "registradores de BG"). Pendente: achar a escrita em 0x0400000A (SBB escolhido).
- Confiança: confirmado (o literal é 0x0400000A; BG1CNT pelo mapa de registradores GBA); SBB escolhido = pendente.

## 4. Chamadores de Screen_RevealStepShowNumber 0x080028AC e Overlay_DrawSixDigitRow 0x0803E8EC

- `Screen_RevealStepShowNumber` 0x080028AC (0x080028AC..0x08002A03): NÃO é HUD de batalha. É um **estado principal**
  da máquina de estados: está no índice 0x50 da tabela `0x083AC2C8` (182 ponteiros). Despachada por `MainState0`
  0x08002534 (`CallViaR0(tabela[byte 0x03002B8C])`; literal 0x0800254C = 0x083AC2C8, 0x08002550 = 0x03002B8C).
  Sem xref direto no Ghidra (chamada por tabela). Nome FL10 ("tick de revelação por passos") mantido.
- `Overlay_DrawSixDigitRow` 0x0803E8EC (0x0803E8EC..0x0803E92B): chamada só por `Overlay_InitScreenGraphics` 0x0803DDC6
  (BL direta, uma vez). Desenha a linha de 6 dígitos na inicialização da tela de overlay; não é o dano de batalha.
- Confiança: confirmado (xref e tabela).

## Renomeadas no Ghidra (confirmadas)
- 0x08017B24 `FUN_08017b24` -> `Fight_DrawSelectCursorObject`
- 0x08017D50 `FUN_08017d50` -> `Fight_LoadActiveBotIconTiles`
- 0x08017D90 `FUN_08017d90` -> `Fight_DrawActiveBotNameSprites`
- 0x08036AC4 `Fight_AdvanceSlotXByWaveTable` -> `Fight_AdvanceSlotYByWaveTable`

## Em aberto
- Cursor de escolha de peça/alvo (item 2).
- Forma visual da barra (coluna 0x1A) e significado de +0x4C no item 1b.
- Escrita de BG1CNT (item 3).
- Leitores de +0xC8/+0xD1 confirmados por xref (item 1d).
- Tabela de onda 0x083AF620 (valores) não lida.
