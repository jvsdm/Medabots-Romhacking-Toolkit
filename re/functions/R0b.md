# R0b: funções 0x080156F8..0x08016154 (lista R0b_lista.txt)

Nota: a lista tinha 25 endereços (o cabeçalho dizia 12). 0x08015924 já tem seção em R1.md (extra, chamada pelo 0x08019D9C): pulado aqui.
Nomes que já estão no mtbe.csv são reaproveitados. Nomes do Ghidra (LoadTilesWithMalias, FillBgTilemapRectSequential etc.) também.

## 0x080156F8 Fight_IsPartActionBlocked
- Assinatura: `int Fight_IsPartActionBlocked(int bot, int part)` (dois undefined4)
- O que faz: devolve 1 se a ação da parte está bloqueada (efeito pendente, cabeça indisponível, parte indisponível ou FUN_08033318); 0 se nada bloqueia. Ou seja, a lógica é o inverso do que o mtbe.csv diz ("nonzero when available").
- Chama: 0x080154E0, 0x08015508, 0x08015528, 0x08033318. Chamada por: não mapeado.
- Memória: só via subrotinas.
- Para o Lua: sim (regra de bloqueio de ação na luta).
- Confiança: confirmado (código lido). Conflito de nome: o mtbe.csv chama este endereço de Fight_IsPartActionAvailable. Revisar antes de fundir.

## 0x08015738 Fight_LoadHudTileset
- Assinatura: `void Fight_LoadHudTileset(void)`
- O que faz: carrega 0x200 bytes de tiles (bloco 3, base 0xE) a partir de um ponteiro fixo da ROM.
- Chama: LoadTilesWithMalias. Chamada por: não mapeado.
- Para o Lua: sim (carga de asset; o port usa a imagem direto).
- Confiança: provável.

## 0x08015760 Fight_FillPartTilemapRows
- Assinatura: `void Fight_FillPartTilemapRows(void)`
- O que faz: preenche o tilemap das 6 linhas de partes (3 do lado esquerdo em x=2, 3 do direito em x=0x1A, linhas 4/7/10). Para cada slot de bot (0x100 bytes) usa o flag +0x40: se zerado, tile padrão; se ligado, tile com paleta de +0x91.
- Chama: FillBgTilemapRectSequential. Chamada por: não mapeado.
- Memória: RAM de slots de bot (base 0x03000590 + bot*0x100, offsets +0x40, +0x91).
- Para o Lua: sim (desenho do painel de partes).
- Confiança: provável.

## 0x08015838 Fight_GetDisplayedPartIdsOrBrokenGlyphs
- Assinatura: `void Fight_GetDisplayedPartIdsOrBrokenGlyphs(int bot, char *out4)`
- O que faz: escreve 4 bytes com o que aparece para as 4 partes do bot: id da peça (base 0x78 + id, via +0x41) ou, se o flag da parte (+0x59, +0x71, +0x89, +0xA1) estiver ligado, o glifo de peça quebrada (+0x5F, +0x77, +0x8F, +0xA7).
- Chama: nada. Chamada por: 0x080158D0, 0x08015960, 0x08015A00.
- Memória: lê o bloco do bot (RAM).
- Para o Lua: sim.
- Confiança: confirmado. Nome igual ao do mtbe.csv.

## 0x080158D0 Fight_LoadBotCompositeSpriteTiles
- Assinatura: `void Fight_LoadBotCompositeSpriteTiles(int bot)`
- O que faz: chama Fight_GetDisplayedPartIdsOrBrokenGlyphs e depois LoadCompositeBattleSpriteTiles com os 4 índices de parte.
- Chama: 0x08015838, LoadCompositeBattleSpriteTiles. Chamada por: 0x080158F8.
- Para o Lua: sim (carga de tiles de sprite composto).
- Confiança: confirmado.

## 0x080158F8 Fight_RefreshDisplayedPartSpritesForActiveBots
- Assinatura: `void Fight_RefreshDisplayedPartSpritesForActiveBots(void)`
- O que faz: para os 6 slots de bot, se o flag +0x40 estiver ligado, chama Fight_LoadBotCompositeSpriteTiles.
- Chama: 0x080158D0. Chamada por: não mapeado (o mtbe.csv já tem o nome).
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08015960 Fight_CreateBotCompositeSprites
- Assinatura: `void Fight_CreateBotCompositeSprites(int bot, undefined4 x, undefined4 y)`
- O que faz: monta o layout do bot com Fight_GetDisplayedPartIdsOrBrokenGlyphs, Fight_ShouldMirrorReactionIndicatorX e CreateCompositeBattleSpriteObjects (4 objetos, OAM a partir de bot*4+0x5A).
- Chama: 0x08015838, Fight_ShouldMirrorReactionIndicatorX, CreateCompositeBattleSpriteObjects. Chamada por: 0x080159BC.
- Para o Lua: sim (criação dos sprites compostos do bot).
- Confiança: confirmado.

## 0x080159BC Fight_CreateAllBotCompositeSprites
- Assinatura: `void Fight_CreateAllBotCompositeSprites(void)`
- O que faz: para os 6 slots com flag +0x40 ligado, cria os sprites compostos. X = 0x28 nos slots 0-2 e 200 nos slots 3-5; Y = (slot % 3)*0x18 + 0x24.
- Chama: aux_mod_u32, 0x08015960. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08015A00 Fight_RefreshBotDisplayedPartSprites
- Assinatura: `void Fight_RefreshBotDisplayedPartSprites(int bot, undefined4 x, undefined4 y)`
- O que faz: refaz o conjunto de sprites de peça de um bot. Chama 0x08015838 e 0x0804305C, depois limpa bits de atributo de dois slots OAM (bot*4+0x5B e +0x5C) com uma máscara e zera os campos de offset +0x30.
- Chama: 0x08015838, Fight_ShouldMirrorReactionIndicatorX, 0x0804305C. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado. Nome igual ao do mtbe.csv.

## 0x08015A90 Fight_LoadBattleBotTilesSet
- Assinatura: `void Fight_LoadBattleBotTilesSet(void)`
- O que faz: carrega quatro conjuntos de tiles da batalha com LoadTilesWithMalias (blocos 0/0xD, 0/0x11, 0xC/0xF, 0x14/0x10).
- Chama: LoadTilesWithMalias. Chamada por: não mapeado.
- Para o Lua: sim (asset).
- Confiança: provável.

## 0x08015AFC Fight_LoadBattleBotTilesSetExtra
- Assinatura: `void Fight_LoadBattleBotTilesSetExtra(void)`
- O que faz: dois LoadTilesWithMalias (blocos 0xC/0x15 e 0x14/0x15).
- Chama: LoadTilesWithMalias. Chamada por: não mapeado.
- Para o Lua: sim (asset).
- Confiança: provável.

## 0x08015B3C Fight_LayoutBotNameText
- Assinatura: `void Fight_LayoutBotNameText(uint bot)`
- O que faz: limpa atributos de slot e monta o texto (glifos) do nome do bot em posição diferente conforme o lado (bot<3 ou não).
- Chama: Slot_ClearAttrMask, Text_LayoutGlyphSprites. Chamada por: não mapeado.
- Para o Lua: sim (texto do nome do bot).
- Confiança: provável.

## 0x08015BA8 Fight_FillSideLabelBandA (revisto CH1: provável)
- Assinatura: `void Fight_FillSideLabelBandA(uint bot)`
- O que faz: preenche uma faixa de tilemap (3,1) de 1 ou 0x15 de coluna, altura 0x12, conforme o lado do bot.
- Chama: FillBgTilemapRectSequential. Chamada por: não mapeado.
- Para o Lua: sim se a faixa existir no port.
- Confiança: revisto CH1: provável. Lido no decompile: FillBgTilemapRectSequential com x=1 ou x=0x15 conforme param. Que lado e que tiles sao, nao confirmado.

## 0x08015BF4 Fight_FillSideLabelBandB (revisto CH1: provável)
- Assinatura: `void Fight_FillSideLabelBandB(uint bot)`
- O que faz: mesma geometria de Fight_FillSideLabelBandA com outros dados de tile.
- Chama: FillBgTilemapRectSequential. Chamada por: não mapeado.
- Para o Lua: sim se a faixa existir no port.
- Confiança: revisto CH1: provável. Lido no decompile: mesma logica (x=1 ou x=0x15, 0x12) com DAT_08015C14/DAT_08015C38. Tiles nao identificados.

## 0x08015C40 Fight_DrawBotGaugeTilemap
- Assinatura: `void Fight_DrawBotGaugeTilemap(uint bot)`
- O que faz: desenha uma barra de 8 células no tilemap (coluna 1 para bot<3, coluna 0x15 para o outro lado). O número de células cheias e o resto parcial vêm de Fight_ComputeGaugeFillFixed sobre o byte +0xC1 do bot; células vazias usam o tile 0xD200.
- Chama: Fight_ComputeGaugeFillFixed, FillBgTilemapRectSequential. Chamada por: não mapeado.
- Para o Lua: sim (barra do bot).
- Confiança: provável.

## 0x08015DD0 Fight_ComputeGaugeFillFixed
- Assinatura: `u32 Fight_ComputeGaugeFillFixed(int a, int b, int c)`
- O que faz: calcula (a / b) * c em ponto flutuante (soft-float), devolve o inteiro em (x>>3) na parte alta e (x&7) na baixa; força 1 se c != 0 e o resultado for 0.
- Chama: sf32_from_i32_b, sf_add, sf32_div, sf32_mul, sf_to_u32. Chamada por: 0x08015C40.
- Para o Lua: não precisa do soft-float; basta a divisão inteira equivalente (conferir o arredondamento).
- Confiança: provável.

## 0x08015E68 Fight_DrawBotNumericReadout
- Assinatura: `void Fight_DrawBotNumericReadout(uint bot)`
- O que faz: desenha um número de 6 dígitos (Num_DrawDecimal6Tilemap) com o valor do byte +0xC1 do bot, mais o rótulo e o valor fixo; posições diferentes para cada lado.
- Chama: Num_DrawDecimal6Tilemap, FillBgTilemapRectSequential. Chamada por: não mapeado.
- Para o Lua: sim (leitura numérica na tela da luta).
- Confiança: provável.

## 0x08015F24 Fight_DrawBotNameLabel (revisto CH1: provável)
- Assinatura: `void Fight_DrawBotNameLabel(void)`
- O que faz: limpa atributos de slot, copia 12 bytes de uma constante da ROM para um buffer RAM indexado pelo id do bot (+0x81) e monta o texto com Text_LayoutGlyphSprites.
- Chama: Slot_ClearAttrMask, memcpy, Text_LayoutGlyphSprites. Chamada por: não mapeado.
- Para o Lua: sim se o cabeçalho existir no port.
- Confiança: revisto CH1: provável. Lido no decompile: Slot_ClearAttrMask, memcpy de 0xC bytes e Text_LayoutGlyphSprites. Nome de rotulo do bot coerente com o efeito.

## 0x08015F78 Fight_SetupAllIndicatorSlots
- Assinatura: `void Fight_SetupAllIndicatorSlots(void)`
- O que faz: chama Fight_SetupIndicatorSlot para os slots 0 a 5.
- Chama: 0x08015F90. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08015F90 Fight_SetupIndicatorSlot
- Assinatura: `void Fight_SetupIndicatorSlot(uint slot)`
- O que faz: limpa o slot de tile (Tile_ClearSlots) e grava um registro de 0x40 bytes: palavra inicial, x/y via CompositeSprite_ComputeClampedOffsetWord, prioridade pelo x (8/9/10), centralização vertical e uma sequência de 0x20 vinda de tabela ROM.
- Chama: Tile_ClearSlots, CompositeSprite_ComputeClampedOffsetWord. Chamada por: 0x08015F78.
- Memória: RAM de registros de 0x40 bytes (base perto de 0x03000590 + 0x40 + slot*0x40, usando o byte +0x81 do bloco ativo).
- Para o Lua: sim (posicionamento dos indicadores de reação).
- Confiança: provável.

## 0x08016068 Fight_SetupAllIndicatorSlotsAlt
- Assinatura: `void Fight_SetupAllIndicatorSlotsAlt(void)`
- O que faz: chama Fight_SetupIndicatorSlotAlt para os slots 0 a 5.
- Chama: 0x08016080. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08016080 Fight_SetupIndicatorSlotAlt
- Assinatura: `void Fight_SetupIndicatorSlotAlt(uint slot)`
- O que faz: igual a Fight_SetupIndicatorSlot, mas fixa os campos +8 e +9 em 1 e a faixa de prioridade muda (limite 0x7C).
- Chama: Tile_ClearSlots, CompositeSprite_ComputeClampedOffsetWord. Chamada por: 0x08016068.
- Para o Lua: sim.
- Confiança: provável.

## 0x08016138 Fight_StepAllIndicatorSlotAnims
- Assinatura: `void Fight_StepAllIndicatorSlotAnims(undefined4 dir)`
- O que faz: chama Fight_StepIndicatorSlotAnim para os slots 0 a 5 com a mesma direção.
- Chama: 0x08016154. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08016154 Fight_StepIndicatorSlotAnim
- Assinatura: `u32 Fight_StepIndicatorSlotAnim(int slot, int dir)`
- O que faz: ajusta +0x20 em ±6 conforme dir; incrementa o contador +0x21; quando chega a 7 zera o contador e avança o frame +0x20 pela tabela ROM; recalcula posição e tamanho do sprite. Devolve 1 quando o contador deu a volta.
- Chama: CompositeSprite_ComputeClampedOffsetWord. Chamada por: 0x08016138.
- Para o Lua: sim (animação do indicador).
- Confiança: provável.
