# BT3: tabela de passos por GS (cena da Robattle), 0x083AD080

Entradas lidas por python -I: 192 ponteiros (idx 0..191); idx 192 = 0x01010404 (fora de 0x08000000..0x08800000). Endereço = 0x083AD080 + 4*idx. Nome = mtbe.csv (ou CSV de símbolos do Toolkit). `NOP` = 0x08000840 (NoOpActionHandler, entrada vazia, compartilhada). `SEM NOME` = função sem nome no mtbe.csv.

| idx | endereco tabela | funcao | nome atual |
|---|---|---|---|
| 0 | 0x83ad080 | 0x080129C8 | Fight_SetupBattleBotStates |
| 1 | 0x83ad084 | 0x08012A20 | Fight_PartPanelStep |
| 2 | 0x83ad088 | 0x08012B40 | Gfx_FadeStepPaletteRows |
| 3 | 0x83ad08c | 0x08012B94 | Battle_CpuActionSelectStep |
| 4 | 0x83ad090 | 0x08012BC0 | Fight_DrawBotsOverlayStep |
| 5 | 0x83ad094 | 0x08012BE4 | Fight_AnimSpriteGroupStep |
| 6 | 0x83ad098 | 0x08012C0C | Fight_SetupEntryScreenStep |
| 7 | 0x83ad09c | 0x08012C64 | Fight_BotSelectMenuFrame |
| 8 | 0x83ad0a0 | 0x08012D98 | Fight_EntryMotionWaitStep |
| 9 | 0x83ad0a4 | 0x08012DF4 | Fight_EntryFieldDrawStep |
| 10 | 0x83ad0a8 | 0x08000840 | NOP (NoOpActionHandler) |
| 11 | 0x83ad0ac | 0x08012E40 | Fight_EntryScreenStep |
| 12 | 0x83ad0b0 | 0x08012EA8 | Fight_SlideCounterObjectsStep |
| 13 | 0x83ad0b4 | 0x08012F00 | Fight_SelectBotActionStep |
| 14 | 0x83ad0b8 | 0x08012FF8 | Fight_CountActionUsageStep |
| 15 | 0x83ad0bc | 0x08000840 | NOP (NoOpActionHandler) |
| 16 | 0x83ad0c0 | 0x0801303C | Fight_RunEntryAnimStep |
| 17 | 0x83ad0c4 | 0x080130D8 | Fight_ShowSelectedActionPanelStep |
| 18 | 0x83ad0c8 | 0x08013164 | Fight_PartSelectStep |
| 19 | 0x83ad0cc | 0x08013228 | Fight_CounterFieldPairStep |
| 20 | 0x83ad0d0 | 0x08000840 | NOP (NoOpActionHandler) |
| 21 | 0x83ad0d4 | 0x08013288 | Fight_EntryAnimStep |
| 22 | 0x83ad0d8 | 0x08000840 | NOP (NoOpActionHandler) |
| 23 | 0x83ad0dc | 0x08000840 | NOP (NoOpActionHandler) |
| 24 | 0x83ad0e0 | 0x08013330 | Fight_DrawEntryScene |
| 25 | 0x83ad0e4 | 0x080133B8 | Fight_ShowAltSelectedActionStep |
| 26 | 0x83ad0e8 | 0x08013438 | Fight_UpdateKindSelectorFrame |
| 27 | 0x83ad0ec | 0x08013508 | Fight_CounterFieldPairAltStep |
| 28 | 0x83ad0f0 | 0x08000840 | NOP (NoOpActionHandler) |
| 29 | 0x83ad0f4 | 0x08013568 | Fight_TickEntryAnimationHandler |
| 30 | 0x83ad0f8 | 0x08000840 | NOP (NoOpActionHandler) |
| 31 | 0x83ad0fc | 0x08000840 | NOP (NoOpActionHandler) |
| 32 | 0x83ad100 | 0x080136E8 | Fight_CopyPaletteBuffersStep |
| 33 | 0x83ad104 | 0x0801372C | Fight_FadeInPaletteStep |
| 34 | 0x83ad108 | 0x08013798 | Init_CopyTileSetsAndSetBlend |
| 35 | 0x83ad10c | 0x08013B8C | Fight_BattleSceneInit |
| 36 | 0x83ad110 | 0x08013DE4 | Fight_PreviewSelectedBotStep |
| 37 | 0x83ad114 | 0x08013E30 | UpdateSelectedMedabotPreviewScreen |
| 38 | 0x83ad118 | 0x08013FA8 | Fight_SelectScreenFrame |
| 39 | 0x83ad11c | 0x08000840 | NOP (NoOpActionHandler) |
| 40 | 0x83ad120 | 0x08013614 | Fight_LoadObjSlot2Step |
| 41 | 0x83ad124 | 0x08013650 | Fight_DispatchBotStateMachineStep |
| 42 | 0x83ad128 | 0x080136B0 | Fight_DispatchSubStateMachineStep |
| 43 | 0x83ad12c | 0x08000840 | NOP (NoOpActionHandler) |
| 44 | 0x83ad130 | 0x08000840 | NOP (NoOpActionHandler) |
| 45 | 0x83ad134 | 0x08000840 | NOP (NoOpActionHandler) |
| 46 | 0x83ad138 | 0x08000840 | NOP (NoOpActionHandler) |
| 47 | 0x83ad13c | 0x08000840 | NOP (NoOpActionHandler) |
| 48 | 0x83ad140 | 0x080136E8 | Fight_CopyPaletteBuffersStep |
| 49 | 0x83ad144 | 0x0801372C | Fight_FadeInPaletteStep |
| 50 | 0x83ad148 | 0x08013798 | Init_CopyTileSetsAndSetBlend |
| 51 | 0x83ad14c | 0x08013824 | Fight_InitActionSlotSelect |
| 52 | 0x83ad150 | 0x08000840 | NOP (NoOpActionHandler) |
| 53 | 0x83ad154 | 0x08000840 | NOP (NoOpActionHandler) |
| 54 | 0x83ad158 | 0x08000840 | NOP (NoOpActionHandler) |
| 55 | 0x83ad15c | 0x08000840 | NOP (NoOpActionHandler) |
| 56 | 0x83ad160 | 0x08013AE4 | Fight_MarkSelectedOptionFrame |
| 57 | 0x83ad164 | 0x08000840 | NOP (NoOpActionHandler) |
| 58 | 0x83ad168 | 0x08000840 | NOP (NoOpActionHandler) |
| 59 | 0x83ad16c | 0x08000840 | NOP (NoOpActionHandler) |
| 60 | 0x83ad170 | 0x0801387C | Dispatch_ByteStateIndexTable |
| 61 | 0x83ad174 | 0x08013A74 | Fight_FadeOutPaletteStep |
| 62 | 0x83ad178 | 0x08000840 | NOP (NoOpActionHandler) |
| 63 | 0x83ad17c | 0x08000840 | NOP (NoOpActionHandler) |
| 64 | 0x83ad180 | 0x08014050 | Fight_LoadResultSceneStep |
| 65 | 0x83ad184 | 0x080140B0 | Fight_SceneStep65_TimedSequence |
| 66 | 0x83ad188 | 0x0801425C | Fight_PlayResultCueStep |
| 67 | 0x83ad18c | 0x08000840 | NOP (NoOpActionHandler) |
| 68 | 0x83ad190 | 0x0801427C | Fight_StepBattleFrame |
| 69 | 0x83ad194 | 0x08014314 | Fight_BotEventQueueStep |
| 70 | 0x83ad198 | 0x080143A0 | Fight_ToggleBotFlagAdvanceStep |
| 71 | 0x83ad19c | 0x08014404 | Fight_PickBotActionStep |
| 72 | 0x83ad1a0 | 0x08000840 | NOP (NoOpActionHandler) |
| 73 | 0x83ad1a4 | 0x08000840 | NOP (NoOpActionHandler) |
| 74 | 0x83ad1a8 | 0x08000840 | NOP (NoOpActionHandler) |
| 75 | 0x83ad1ac | 0x08000840 | NOP (NoOpActionHandler) |
| 76 | 0x83ad1b0 | 0x08014548 | Fight_UpdateBotMeterMotionStep |
| 77 | 0x83ad1b4 | 0x08000840 | NOP (NoOpActionHandler) |
| 78 | 0x83ad1b8 | 0x08000840 | NOP (NoOpActionHandler) |
| 79 | 0x83ad1bc | 0x08000840 | NOP (NoOpActionHandler) |
| 80 | 0x83ad1c0 | 0x08000840 | NOP (NoOpActionHandler) |
| 81 | 0x83ad1c4 | 0x08000840 | NOP (NoOpActionHandler) |
| 82 | 0x83ad1c8 | 0x08000840 | NOP (NoOpActionHandler) |
| 83 | 0x83ad1cc | 0x08000840 | NOP (NoOpActionHandler) |
| 84 | 0x83ad1d0 | 0x08000840 | NOP (NoOpActionHandler) |
| 85 | 0x83ad1d4 | 0x08000840 | NOP (NoOpActionHandler) |
| 86 | 0x83ad1d8 | 0x08000840 | NOP (NoOpActionHandler) |
| 87 | 0x83ad1dc | 0x08000840 | NOP (NoOpActionHandler) |
| 88 | 0x83ad1e0 | 0x08000840 | NOP (NoOpActionHandler) |
| 89 | 0x83ad1e4 | 0x08000840 | NOP (NoOpActionHandler) |
| 90 | 0x83ad1e8 | 0x08000840 | NOP (NoOpActionHandler) |
| 91 | 0x83ad1ec | 0x08000840 | NOP (NoOpActionHandler) |
| 92 | 0x83ad1f0 | 0x08000840 | NOP (NoOpActionHandler) |
| 93 | 0x83ad1f4 | 0x08000840 | NOP (NoOpActionHandler) |
| 94 | 0x83ad1f8 | 0x08000840 | NOP (NoOpActionHandler) |
| 95 | 0x83ad1fc | 0x08000840 | NOP (NoOpActionHandler) |
| 96 | 0x83ad200 | 0x080145BC | Fight_HighlightBotOamAStep |
| 97 | 0x83ad204 | 0x08014634 | Fight_ClearBotHighlightOamAStep |
| 98 | 0x83ad208 | 0x08000840 | NOP (NoOpActionHandler) |
| 99 | 0x83ad20c | 0x08000840 | NOP (NoOpActionHandler) |
| 100 | 0x83ad210 | 0x08000840 | NOP (NoOpActionHandler) |
| 101 | 0x83ad214 | 0x08000840 | NOP (NoOpActionHandler) |
| 102 | 0x83ad218 | 0x08000840 | NOP (NoOpActionHandler) |
| 103 | 0x83ad21c | 0x08000840 | NOP (NoOpActionHandler) |
| 104 | 0x83ad220 | 0x080146DC | Fight_HighlightBotOamBStep |
| 105 | 0x83ad224 | 0x08014754 | Fight_ClearBotHighlightOamBStep |
| 106 | 0x83ad228 | 0x08000840 | NOP (NoOpActionHandler) |
| 107 | 0x83ad22c | 0x08000840 | NOP (NoOpActionHandler) |
| 108 | 0x83ad230 | 0x08000840 | NOP (NoOpActionHandler) |
| 109 | 0x83ad234 | 0x08000840 | NOP (NoOpActionHandler) |
| 110 | 0x83ad238 | 0x08000840 | NOP (NoOpActionHandler) |
| 111 | 0x83ad23c | 0x08000840 | NOP (NoOpActionHandler) |
| 112 | 0x83ad240 | 0x080147FC | Fight_DrawBotNumberOamStep |
| 113 | 0x83ad244 | 0x08014870 | Fight_StepBotNumberAnimStep |
| 114 | 0x83ad248 | 0x08000840 | NOP (NoOpActionHandler) |
| 115 | 0x83ad24c | 0x08000840 | NOP (NoOpActionHandler) |
| 116 | 0x83ad250 | 0x08000840 | NOP (NoOpActionHandler) |
| 117 | 0x83ad254 | 0x08000840 | NOP (NoOpActionHandler) |
| 118 | 0x83ad258 | 0x08000840 | NOP (NoOpActionHandler) |
| 119 | 0x83ad25c | 0x08000840 | NOP (NoOpActionHandler) |
| 120 | 0x83ad260 | 0x08014920 | Fight_DrawBotNumberOamAltStep |
| 121 | 0x83ad264 | 0x08014994 | Fight_StepBotNumberAnimAltStep |
| 122 | 0x83ad268 | 0x08000840 | NOP (NoOpActionHandler) |
| 123 | 0x83ad26c | 0x08000840 | NOP (NoOpActionHandler) |
| 124 | 0x83ad270 | 0x08000840 | NOP (NoOpActionHandler) |
| 125 | 0x83ad274 | 0x08000840 | NOP (NoOpActionHandler) |
| 126 | 0x83ad278 | 0x08000840 | NOP (NoOpActionHandler) |
| 127 | 0x83ad27c | 0x08000840 | NOP (NoOpActionHandler) |
| 128 | 0x83ad280 | 0x08000840 | NOP (NoOpActionHandler) |
| 129 | 0x83ad284 | 0x08000840 | NOP (NoOpActionHandler) |
| 130 | 0x83ad288 | 0x08000840 | NOP (NoOpActionHandler) |
| 131 | 0x83ad28c | 0x08000840 | NOP (NoOpActionHandler) |
| 132 | 0x83ad290 | 0x08000840 | NOP (NoOpActionHandler) |
| 133 | 0x83ad294 | 0x08000840 | NOP (NoOpActionHandler) |
| 134 | 0x83ad298 | 0x08000840 | NOP (NoOpActionHandler) |
| 135 | 0x83ad29c | 0x08000840 | NOP (NoOpActionHandler) |
| 136 | 0x83ad2a0 | 0x08000840 | NOP (NoOpActionHandler) |
| 137 | 0x83ad2a4 | 0x08000840 | NOP (NoOpActionHandler) |
| 138 | 0x83ad2a8 | 0x08000840 | NOP (NoOpActionHandler) |
| 139 | 0x83ad2ac | 0x08000840 | NOP (NoOpActionHandler) |
| 140 | 0x83ad2b0 | 0x08000840 | NOP (NoOpActionHandler) |
| 141 | 0x83ad2b4 | 0x08000840 | NOP (NoOpActionHandler) |
| 142 | 0x83ad2b8 | 0x08000840 | NOP (NoOpActionHandler) |
| 143 | 0x83ad2bc | 0x08000840 | NOP (NoOpActionHandler) |
| 144 | 0x83ad2c0 | 0x08014A44 | Fight_LoadObjSlot0Step |
| 145 | 0x83ad2c4 | 0x08014A60 | StateMachine_TickStepCounter |
| 146 | 0x83ad2c8 | 0x08014A84 | Fight_WaitOverlayTargetStep |
| 147 | 0x83ad2cc | 0x08014AB4 | SEM NOME |
| 148 | 0x83ad2d0 | 0x08014ADC | Fight_ResetBattleFieldsStep |
| 149 | 0x83ad2d4 | 0x08014B34 | Fight_TickPartGaugePhases |
| 150 | 0x83ad2d8 | 0x08014C30 | Fight_SelectThreeActionSources |
| 151 | 0x83ad2dc | 0x08014C84 | Fight_SelectEnemyMedaforceStep |
| 152 | 0x83ad2e0 | 0x08014CC8 | Fight_DispatchSubStateMachineAltStep |
| 153 | 0x83ad2e4 | 0x08014CE4 | SEM NOME |
| 154 | 0x83ad2e8 | 0x08000840 | NOP (NoOpActionHandler) |
| 155 | 0x83ad2ec | 0x08000840 | NOP (NoOpActionHandler) |
| 156 | 0x83ad2f0 | 0x08000840 | NOP (NoOpActionHandler) |
| 157 | 0x83ad2f4 | 0x08000840 | NOP (NoOpActionHandler) |
| 158 | 0x83ad2f8 | 0x08000840 | NOP (NoOpActionHandler) |
| 159 | 0x83ad2fc | 0x08000840 | NOP (NoOpActionHandler) |
| 160 | 0x83ad300 | 0x08014A44 | Fight_LoadObjSlot0Step |
| 161 | 0x83ad304 | 0x08014A60 | StateMachine_TickStepCounter |
| 162 | 0x83ad308 | 0x08014A84 | Fight_WaitOverlayTargetStep |
| 163 | 0x83ad30c | 0x08014AB4 | SEM NOME |
| 164 | 0x83ad310 | 0x08014ADC | Fight_ResetBattleFieldsStep |
| 165 | 0x83ad314 | 0x08014CF0 | Fight_SyncBotStatusFromPart |
| 166 | 0x83ad318 | 0x08014AB4 | SEM NOME |
| 167 | 0x83ad31c | 0x08014A84 | Fight_WaitOverlayTargetStep |
| 168 | 0x83ad320 | 0x08014AB4 | SEM NOME |
| 169 | 0x83ad324 | 0x08014D84 | SEM NOME |
| 170 | 0x83ad328 | 0x08014D94 | Fight_SelectActionSourceStep |
| 171 | 0x83ad32c | 0x08014DC8 | Fight_SelectMedaforceSourceStep |
| 172 | 0x83ad330 | 0x08014CC8 | Fight_DispatchSubStateMachineAltStep |
| 173 | 0x83ad334 | 0x08014E20 | Fight_OverlayTimerAdvance |
| 174 | 0x83ad338 | 0x08014E54 | SEM NOME |
| 175 | 0x83ad33c | 0x08014E08 | SEM NOME |
| 176 | 0x83ad340 | 0x08000840 | NOP (NoOpActionHandler) |
| 177 | 0x83ad344 | 0x08000840 | NOP (NoOpActionHandler) |
| 178 | 0x83ad348 | 0x08000840 | NOP (NoOpActionHandler) |
| 179 | 0x83ad34c | 0x08000840 | NOP (NoOpActionHandler) |
| 180 | 0x83ad350 | 0x08000840 | NOP (NoOpActionHandler) |
| 181 | 0x83ad354 | 0x08000840 | NOP (NoOpActionHandler) |
| 182 | 0x83ad358 | 0x08000840 | NOP (NoOpActionHandler) |
| 183 | 0x83ad35c | 0x08000840 | NOP (NoOpActionHandler) |
| 184 | 0x83ad360 | 0x08000840 | NOP (NoOpActionHandler) |
| 185 | 0x83ad364 | 0x08000840 | NOP (NoOpActionHandler) |
| 186 | 0x83ad368 | 0x08000840 | NOP (NoOpActionHandler) |
| 187 | 0x83ad36c | 0x08000840 | NOP (NoOpActionHandler) |
| 188 | 0x83ad370 | 0x08000840 | NOP (NoOpActionHandler) |
| 189 | 0x83ad374 | 0x08000840 | NOP (NoOpActionHandler) |
| 190 | 0x83ad378 | 0x08000840 | NOP (NoOpActionHandler) |
| 191 | 0x83ad37c | 0x08000840 | NOP (NoOpActionHandler) |

Resumo: 192 entradas; 109 NOP; 83 com função; sem nome: 0x8014ab4, 0x8014ce4, 0x8014d84, 0x8014e08, 0x8014e54.

## Fluxo da Robattle em GS (confirmado no decompile + ROM)
- Laço: `Battle_FrameDispatchOrExit` 0x08012970 (menu_list[7]) checa saída, faz CpuFastSet se `*0x03000430 == 2` e chama `table[GS]` (tabela 0x083AD080; DAT 0x080129C4 = 0x0300137C, confirmado no literal pool).
- Init: idx 35 `Fight_BattleSceneInit` 0x08013B8C (decompile): carrega LZ/tiles/paletas, desenha slots e cursores, `SetGpuBlendMode`, e no fim `GS += 1` (literal 0x08013DE0 = 0x0300137C, confirmado). Portanto 35 -> 36.
- Passos: a ordem de índices vem da tabela (idx 36 `Fight_PreviewSelectedBotStep`, 37 `UpdateSelectedMedabotPreviewScreen`, 38 `Fight_SelectScreenFrame`, 40 `Fight_LoadObjSlot2Step`, 64..66 resultado, 68 `Fight_StepBattleFrame`). A transição entre eles é a que FS5.md descreve por passo (grava GS próximo), não verificada uma a uma aqui.
- Frame: idx 68 `Fight_StepBattleFrame` 0x0801427C = passo "frame" (grava GS 0x78/0x70/0x68/0x60 por bits de fi+0x55, ver BT.md).
- Fim: sem leitura nova nesta rodada; a saída de GS para o mapa não foi fechada (ver BT.md, lacunas).

## Lacunas abertas
- Quem põe GS = 35 ao entrar na Robattle: não achado (já na lacuna 1 do BT.md).
- Os 5 endereços sem nome (0x08014AB4, 0x08014CE4, 0x08014D84, 0x08014E08, 0x08014E54) não foram decompilados nesta rodada; nomes não propostos.
- Discrepância de nome: mtbe.csv chama 0x0300137C de `gMainSubStateCounter` (comentário "passo interno da cena"). Na pesquisa e no AGENTE.md é GS. Mesmo endereço; manter GS nos docs e não renomear sem revisão.
- Varredura de stores em GS nas 70 funções: heurística ruidosa (quase toda função lê GS); não usada como prova de escrita. Escritas confirmadas só as que decompile mostrou.
