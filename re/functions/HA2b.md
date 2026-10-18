# HA2b: redo das 12 seções de HA2 nunca relidas (E02, E03, E08, F02, G00, G01, G03, G06, H00, H0D, H15, H18)

Fonte: decompile no Ghidra MCP (aberto, não salvo). Cada entrada foi conferida com `get_function_by_address` antes.
Os endereços de entrada batem com `HA2_lista.txt` (despachante E/F/G/H, índice = byte de `+0xAA`). Os literais
do pool aparecem como `DAT_0801xxxx` no decompile; onde o endereço RAM não foi resolvido, a seção diz isso.
Convenções: `gFightInfo` = `0x03001800`; `+0xAA` = subestado; `+0xB5` = handler feito; "+1" = `+0xAA += 1`.

Nomes: o Ghidra já tem nomes para estes 12 (renomeados em rodada anterior). Uso o padrão do AGENTE
(`Fight_DispatchOnSubstateAA_<Desp>_<Passo>_<Verbo>`). Divergências com o mtbe.csv no fim.

## 0x0801D738 Fight_DispatchOnSubstateAA_E_02_PrepareFollowupSourceDisplay
- Assinatura: `void f(void)` (sem argumentos; lê `gFightInfo`)
- O que faz: copia 4 blocos de 0x40 bytes do bot seguinte (`followUpBot - 1`) para o buffer do bot; limpa a
  máscara de atributos OBJ; monta a lista de partes exibidas e escreve o composto de 4 objetos (sprites da fonte
  de follow-up); marca o grupo composto com o campo 0x24 = 8; zera `DAT_0801d874+0x84`; `+1` no subestado.
- Chama: `Mem_CopyBytes` (x4), `Obj_ClearAttr0Mask`, `Fight_GetDisplayedPartIdsOrBrokenGlyphs`,
  `Sprite_WriteComposite4Objects`, `Obj_SetCompositeGroupField24`.
- Chamada por: stub E (tabela `0x083AD6F0`, índice 0x02).
- Memória: RAM lida: `gFightInfo` (followUpBot, priorityListLine, field_0x65..0x67, moveDirectionFlags,
  field_0x3c); bloco do bot (buffers, 0x1642/0x1644 via literal). RAM escrita: buffers do bot, `+0xAA`, byte +0x84
  do bloco de objeto (literal).
- Para o Lua: não precisa copiar bytes de OBJ; é preparo de sprite. Porte = efeito visual (lista de partes de follow-up).
- Confiança: confirmado (lido no decompile). Endereço RAM dos buffers: provável.

## 0x0801D878 Fight_DispatchOnSubstateAA_E_03_AnimateBotEntry
- Assinatura: `void f(void)`
- O que faz: a cada chamada atualiza o movimento do composto de 4 objetos do bot seguinte (`Sprite_UpdateComposite4Motion`),
  liga/desliga o bit de visibilidade (bit 4 do byte +0x84 de um contador de objeto) e incrementa esse contador.
  Quando o contador chega a 0x3C (`'<'`): liga o bit no bot seguinte, toca SFX 0x40, zera o campo do bot
  (byte 0x100*bot+literal = 4, byte +literal = 0, halfword +0xB = 0), mostra o slot do objeto (`bot+0x71`) e `+1` no subestado.
- Chama: `Sprite_UpdateComposite4Motion`, `Slot_SetFlagBit15`, `Obj_ClearAttr0Mask`, `snd_sfx_request(0x40)`,
  `Fight_ShowObjectSlot`.
- Chamada por: stub E, índice 0x03.
- Memória: RAM lida/escrita: byte +0x84 do contador (literal `DAT_0801d948`); `gFightInfo` (followUpBot, moveDirectionFlags,
  actionSubstate); bloco do bot (bytes em `0x100*bot` + literal). Endereços exatos dos literais: não resolvidos aqui.
- Para o Lua: porte = animação de entrada do bot (60 frames). Pode virar contador simples.
- Confiança: confirmado (ramo do contador 0x3C e `+1` lidos). Efeito visual exato: provável.

## 0x0801DD30 Fight_DispatchOnSubstateAA_E_08_WaitCounterThenSfx30
- Assinatura: `void f(void)`
- O que faz: espera um contador de bytes (literal `DAT_0801dd50+DAT_0801dd54`): se vale `'@'` (0x40), toca SFX 0x30 e
  `+1` no subestado; senão subtrai 4 desse contador e subtrai 4 também de um byte do bloco de objeto (`base+DAT_0801dd70`).
  Ou seja: contagem regressiva em passos de 4 até 0x40, depois o som.
- Chama: `snd_sfx_request(0x30)`.
- Chamada por: stub E, índice 0x08.
- Memória: RAM lida/escrita: contador de byte (literal `DAT_0801dd50+DAT_0801dd54`), byte de `DAT_0801dd70`,
  `gFightInfo+0xAA` (via `PTR_gFightInfo_0801dd58`, literal).
- Para o Lua: espera de tempo + som. Porte simples.
- Confiança: confirmado (valor 0x40 e SFX 0x30 lidos). Alvo do decremento -4: confirmado.

## 0x0801DED4 Fight_DispatchOnSubstateAA_F_02_HoldFlag
- Assinatura: `void f(void)`
- O que faz: só põe `gFightInfo+0xB5 = 1` (handler feito). Não muda o subestado: é "hold".
- Chama: nada. Chamada por: stub F (`0x083AD730`), índice 0x02.
- Memória: escreve `gFightInfo+0xB5`.
- Para o Lua: não precisa (é só o sinal de fim de passo).
- Confiança: confirmado.

## 0x0801DF0C Fight_DispatchOnSubstateAA_G_00_FillPaletteAndTiles
- Assinatura: `void f(void)`
- O que faz: zera o byte +0x84 de um objeto (literal) e preenche duas áreas via `bios_cpufastset`: a primeira com 0
  (fill), a segunda com um valor do literal; depois `+1` no subestado.
- Chama: `bios_cpufastset` (x2, wrapper SWI 0x0C, ver `FUN_0807CCC0`).
- Chamada por: stub G (`0x083AD750`), índice 0x00.
- Memória: escreve byte +0x84 (literal `DAT_0801df48`); fill de VRAM/paleta (endereço no literal `DAT_0801df4c/df58`:
  não resolvido aqui). `gFightInfo+0xAA`.
- Para o Lua: não precisa (fill de memória/VRAM; porte = limpar o fundo).
- Confiança: confirmado (2 chamadas CpuFastSet e `+1`). Destino exato: provável.

## 0x0801DF60 Fight_DispatchOnSubstateAA_G_01_SlideBgSpritesTen
- Assinatura: `void f(void)`
- O que faz: a cada frame copia duas faixas do mapa de fundo (`BgMap_CopyRectWithOffset`, 0x1E x 1, offsets
  `contador` e `0x13 - contador`) usando o contador (byte +0x84, literal). Ao chegar em 9 (10 passos) põe
  `flag1AE = 1` e `+1` no subestado; senão incrementa o contador.
- Chama: `BgMap_CopyRectWithOffset` (x2).
- Chamada por: stub G, índice 0x01.
- Memória: escreve byte +0x84 (contador); `gFightInfo` (flag1AE, actionSubstate); lê o deslocamento em 0x1A4 de outra
  struct (literal `DAT_0801dfc8`).
- Para o Lua: porte = deslizar o fundo em 10 passos (efeito visual).
- Confiança: confirmado (limite 9, flag1AE e `+1`). Nome "Ten": é o contador 0..9 (10 passos).

## 0x0801E09C Fight_DispatchOnSubstateAA_G_03_BuildBattleBackground
- Assinatura: `void f(void)`
- O que faz: máquina de 3 passos guardada pelo byte de estado (literal `DAT_0801e0b4+DAT_0801e0b8`):
  - passo 0: limpa 4 pares de slots (`Slots_ClearFourPairs`), ajusta `REG_DISPCNT` (máscara), define BG, descompacta
    o mapa (`Lz_DecodeIfMagic`), preenche o mapa de BG com 0 (`BgMap_FillRect16`), carrega tiles e paleta do fundo
    (`GfxBankA_LoadTilesAndPalette`, dados de `gFightInfo+0x38`), zera uma palavra via CpuFastSet; estado 0 -> 1.
  - passo 1: liga o bit 0x8000 em `REG_DISPCNT`, aplica o blend (`FxBlend_ApplyFromObjA`), carrega os gráficos
    da técnica do bot ativo (`Fight_LoadGfxListToObjVram`, com `medaforceId + 0x50` se a fonte for >= 4), carrega os
    gráficos fixos (`Fight_LoadFixedGfxListToObjVram`), `Obj_SetWordFromBit0OfF8`, zera o byte +0x84 e o outro
    contador; `+1` no subestado.
  - passo 2+: nada.
- Chama: `Slots_ClearFourPairs`, `Lz_DecodeIfMagic`, `BgMap_FillRect16`, `GfxBankA_LoadTilesAndPalette`,
  `bios_cpufastset`, `FxBlend_ApplyFromObjA`, `Fight_LoadGfxListToObjVram`, `Fight_LoadFixedGfxListToObjVram`,
  `Obj_SetWordFromBit0OfF8`.
- Chamada por: stub G, índice 0x03.
- Memória: escreve `REG_DISPCNT` (0x04000000), VRAM/paleta de fundo, byte de estado (literal), `gFightInfo+0xAA`.
  Lê `gBattleBots[bot]` (selectedActionSource, parts[partIndex].techniqueId, medaforceId) e `gFightInfo` (activeBot,
  partIndex). Usa o array de bots `PTR_gBattleBots_0801e1a8` (ver `0x03000590 + bot*0x100`).
- Para o Lua: porte = montar o cenário de fundo e carregar gráficos da técnica (a lógica de passo 0/1 é a parte útil).
- Confiança: confirmado (3 passos, chamadas, `selectedActionSource < 4`). Ordem de `REG_DISPCNT` e valor 0x8000: confirmado.

## 0x0801E2B8 Fight_DispatchOnSubstateAA_G_06_StepSideSequencesHold
- Assinatura: `void f(void)`
- O que faz: atualiza os sprites de uma camada lateral, avança a sequência do lado esquerdo e do direito (um passo
  de cada) e põe `handlerDoneFlag = 1` (hold: não muda o subestado).
- Chama: `FightPreview_UpdateSingleLayerSideSprites`, `FightPreview_AdvanceLeftSideSequenceStep`,
  `FightPreview_AdvanceRightSideSequenceStep`.
- Chamada por: stub G, índice 0x06.
- Memória: escreve `gFightInfo+0xB5`. O resto está nas rotinas `FightPreview_*` (não lidas aqui).
- Para o Lua: porte = passo de animação das sequências laterais (ver preview de batalha).
- Confiança: confirmado (sequência de 3 chamadas e `+0xB5 = 1`).

## 0x0801E300 Fight_DispatchOnSubstateAA_H_00_LoadSpritesTiles
- Assinatura: `void f(void)`
- O que faz: descompacta 4 blocos (`Lz_DecodeIfMagic`), copia 3 blocos de 8 palavras (`bios_cpufastset`), depois
  monta o lado do preview (bool = bit 1 de `moveDirectionFlags` == 0): carrega os slots laterais, os tiles laterais,
  as linhas de ícones de partes (com `targetBot`) e desenha os pares de ícones; `+1` no subestado.
- Chama: `Lz_DecodeIfMagic` (x4), `bios_cpufastset` (x3), `FightPreview_LoadSideTileSlots`, `FightPreview_LoadSideTiles`,
  `FightPreview_LoadPartIconRows`, `FightPreview_DrawPartIconPairs`.
- Chamada por: stub H (`0x083AD790`), índice 0x00.
- Memória: lê `gFightInfo` (moveDirectionFlags, targetBot, actionSubstate); escreve VRAM/OAM via as rotinas (literais
  `DAT_0801e384..e3b8`, não resolvidos aqui).
- Para o Lua: carregar gráficos do preview. Porte = dados prontos (não é lógica de jogo).
- Confiança: confirmado (chamadas e `+1`). Destinos VRAM: provável.

## 0x0801E918 Fight_DispatchOnSubstateAA_H_0D_LoadNumberSpriteRow
- Assinatura: `void f(void)`
- O que faz: descompacta gráficos (`Lz_DecodeIfMagic`) e copia 8 palavras (`bios_cpufastset`); escolhe um offset
  (0x158 se bit 1 de `moveDirectionFlags` = 0, senão 0x154) e lê dois halfwords dessa área de `sideRecords`; escreve
  o número do `combatValue` do bot ativo (`Fight_WriteNumberTiles`, 1 dígito), monta a linha de sprites
  (`FightPreview_BuildSpriteListRow`, 10 itens, `chosenPart`), toca SFX 0x5B e `+1` no subestado.
- Chama: `Lz_DecodeIfMagic`, `bios_cpufastset`, `Fight_WriteNumberTiles`, `FightPreview_BuildSpriteListRow`,
  `snd_sfx_request(0x5b)`.
- Chamada por: stub H, índice 0x0D.
- Memória: lê `gFightInfo` (moveDirectionFlags, combatValue, chosenPart, sideRecords); escreve tiles/linha de sprites.
- Para o Lua: mostrar o valor numérico (combatValue) na tela de preview; a lógica do número já está em `calc.lua`.
- Confiança: confirmado (chamadas, 10 itens, SFX 0x5B). Offsets 0x154/0x158: lidos, significado provável (preview por lado).

## 0x0801EAC4 Fight_DispatchOnSubstateAA_H_15_HoldFlag
- Assinatura: `void f(void)`
- O que faz: só põe `gFightInfo+0xB5 = 1` (hold, sem mudar o subestado).
- Chama: nada. Chamada por: stub H, índice 0x15.
- Memória: escreve `gFightInfo+0xB5`.
- Para o Lua: não precisa.
- Confiança: confirmado.

## 0x0801EBB8 Fight_DispatchOnSubstateAA_H_18_SetTimer60Advance
- Assinatura: `void f(void)`
- O que faz: põe o halfword de contador de frames `+0x86` = 0x3C (60 frames) no bloco literal e `+1` no subestado.
  Quem consome o contador: ver HA2 "Pendente" (não lido aqui).
- Chama: nada. Chamada por: stub H, índice 0x18.
- Memória: escreve halfword `+0x86` do bloco de `DAT_0801ebcc` (provavelmente bloco do bot; literal não resolvido) e
  `gFightInfo+0xAA`.
- Para o Lua: espera de 60 frames (um `timer = 60` basta).
- Confiança: confirmado (valor 0x3C e `+1`). Bloco exato: provável.

## Conflitos de nome com mtbe.csv (para o Opus resolver; mtbe.csv não foi editado)

Verificação: `python -I` sobre `functions/HA*_simbolos.csv` contra `symbols/mtbe.csv`, mesmo endereço e nome diferente.
Não há endereço com dois nomes dentro dos HA*. Total: 41 divergências.

Casos desta rodada (os 12 desta seção):
- 0x0801D738: mtbe `Fight_PrepareFollowupSourceDisplay`; HA2 antigo `Fight_DispatchOnSubstateAA_E_02_CopyBotBuffers`; proposta desta seção `..._E_02_PrepareFollowupSourceDisplay`.
- 0x0801D878: mtbe é o nome de despachante; HA1 antigo `Fight_AnimateFollowupSourceIntro` (divergente de mtbe e de HA2).

Demais (mtbe = nome atual; HA antigo entre parênteses):
- HA1 0x0801D95C: `Fight_AdvanceFollowupSourceApproach` (HA1); mtbe `Fight_DispatchOnSubstateAA_E_04_StepBotSlot`.
- HA2 0x0801E9FC: `..._H_11_WaitPartIconRows` vs mtbe `..._H_11_DrainPartArmorAndDrawIcons`.
- HA2 0x0801EC3C: `..._H_1D_WaitOrAdvance` vs mtbe `..._H_1D_ChainReactionOrAdvance`.
- HA3 0x0801F390: `..._H_45_DrawPartIconsWaitTimer` vs mtbe `..._H_45_RepairPartTickDrawIcons`.
- HA4 0x08020728: `..._M_04_ApplyPartDamage` vs mtbe `..._M_04_BreakActorUsedPart`.
- HA4 0x08020B4C: `..._R_08_LoadPartIconsAndJump` vs mtbe `..._R_08_RepairTickDrawComposite`.
- HA6 0x080233E0: `..._Z_AddGaugeMax160` vs mtbe `..._Z_0E_AddScoutBonus`.
- HA6 0x080236E0: `..._AA_AddGaugeMax120` vs mtbe `..._AA_0A_AddConceal`.
- HA6 0x08023A48: `..._AB_CopyDisplayValues` vs mtbe `..._AB_SetStatus26CopyDisplayValues`.
- HA7 0x08024A30: `..._AF_SetupOverlay` vs mtbe `..._AF_08_SetupOverlay`.
- HA7 0x08024C44: `..._AF_CountDownToMark` vs mtbe `..._AF_0F_CountDownToMark`.
- HA7 0x08024C94: `..._AF_OscillateSlot` vs mtbe `..._AF_10_OscillateSlot`.
- HA7 0x08024D18: `..._AF_SetSlotParams` vs mtbe `..._AF_11_SetSlotParams`.
- HA7 0x08024DA8: `..._AF_CopyTileData` vs mtbe `..._AF_12_CopyTileData`.
- HA7 0x08024E9C: `..._AG_SetupSlots` vs mtbe `..._AG_08_SetupSlots`.
- HA7 0x08024FC4: `..._AG_SlideSlotsIn` vs mtbe `..._AG_0C_SlideSlotsIn`.
- HA7 0x0802503C: `..._AG_SpawnEffectSlot` vs mtbe `..._AG_0D_SpawnEffectSlot`.
- HA7 0x080249C0: `..._AF_AccumPartAndCheckTech` vs mtbe `..._AF_00_AccumPartAndCheckTech`.
- HA7 0x08024AF4: `..._AF_InitSlotAttrs` vs mtbe `..._AF_0B_InitSlotAttrs`.
- HA7 0x08024BCC: `..._AF_WaitSlot22` vs mtbe `..._AF_0C_WaitSlot22`.
- HA7 0x08024BF8: `..._AF_SetSlotFlagsPos` vs mtbe `..._AF_0E_SetSlotFlagsPos`.
- HA7 0x08024E2C: `..._AG_AccumPartAndCheckTech` vs mtbe `..._AG_00_AccumPartAndCheckTech`.
- HA7 0x08024F90: `..._AG_PlaySfx33SetFlags` vs mtbe `..._AG_0B_PlaySfx33SetFlags`.
- HA7 0x08025104: `..._AG_SetBotParamsFromE8EC` vs mtbe `..._AG_0F_SetBotParamsFromE8EC`.
- HA7 0x08025174: `..._AG_SetStep33` vs mtbe `..._AG_10_SetStep33`.
- HA8 0x08025B20: `..._AJ_00_AccumPartCheckTargetCost` vs mtbe `..._AJ_00_CheckRosAndTargetMedaforceZero`.
- HA8 0x08026284: `..._AM_WaitAnimThenPlaceSprite` vs mtbe `..._AM_05_WaitAnimThenPlaceSprite`.
- HA8 0x080262FC: `..._AM_StepDownCounterWithSfx` vs mtbe `..._AM_06_StepDownCounterWithSfx`.
- HA8 0x080263BC: `..._AM_0A_ClearSlotSetPartDisplayValue` vs mtbe `..._AM_0A_ClearSlotSetFlag10AddPartValueToCounterD8`.
- HA8 0x080264E4: `..._AN_SetupEffectSpritesAndSfx` vs mtbe `..._AN_04_SetupEffectSpritesAndSfx`.
- HA8 0x08026628: `..._AN_WaitFramesWithAnimations` vs mtbe `..._AN_05_WaitFramesWithAnimations`.
- HA8 0x08026678: `Fight_ApplyRandomPartReplacementEffect` (sem prefixo) vs mtbe `..._AN_06_ApplyRandomPartReplacementEffect`.
- HA8 0x080268D4: `..._AO_ClampPartCountsToLimit` vs mtbe `..._AO_02_ClampPartCountsToLimit`.
- HA8 0x08026924: `..._AO_ShowFourPartValues` vs mtbe `..._AO_03_ShowFourPartValues`.
- HA8 0x08026A24: `..._AO_WaitPartRowsDrawn` vs mtbe `..._AO_04_WaitPartRowsDrawn`.
- HA8 0x08026A74: `..._AO_DrawPartIconsWaitReady` vs mtbe `..._AO_07_DrawPartIconsWaitReady`.
- HA8 0x08026AF4: `..._AO_ShowPartFlagsOrBackup` vs mtbe `..._AO_09_ShowPartFlagsOrBackup`.
- HA8 0x08026CBC: `Fight_QueueHeadBreakIfDefenderHeadZero` (sem prefixo) vs mtbe `..._AO_10_QueueHeadBreakIfDefenderHeadZero`.

Padrão: quase todos os conflitos são a mesma função com o índice (`_AF_08_`) acrescentado no mtbe, ou nome antigo
sem prefixo. Sugestão ao Opus: adotar o nome do mtbe (com índice) em todos. Nos casos de nome semanticamente
diferente (HA2 0x801E9FC, 0x801EC3C, HA3 0x801F390, HA4 0x8020728/0x8020B4C, HA6 0x80233E0/0x80236E0/0x8023A48,
HA8 0x8025B20, 0x80263BC), a leitura do decompile precisa ser conferida antes de decidir.

Divergência de endereço (não conflito de nome): E02 em `HA2R_simbolos.csv` usa o mesmo endereço 0x0801D738.
