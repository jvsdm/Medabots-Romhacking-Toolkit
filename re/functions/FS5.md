# FS5: passos da cena de batalha (tabela GS 0x083AD080), 38 candidatos

Origem: `FS5_lista.txt`. Os 38 endereços são início de função no Ghidra (`get_function_by_address` bate no endereço exato). 36 foram renomeados no Ghidra (`Fight_*Step`); 0x080140B0 ficou `FUN_` (chute); 0x08015438 é dado, não código.

Achado geral (confirmado na ROM, lido com python -I): cada candidato é a entrada `i` da tabela de ponteiros thumb 0x083AD080, com `i = (endereço_da_entrada - 0x083AD080) / 4`. A tabela é indexada pelo GS (0x0300137C). Isso confirma a entrada 0..3 de BT (080129C9, 08012A21, 08012B41, 08012B95). Os endereços 0x083AD10C (i=35, Fight_BattleSceneInit) e 0x083AD190 (i=68, Fight_StepBattleFrame) caem nesta mesma tabela. Não são tabelas separadas como BT2 supõe.

Padrão dos corpos: quase todos começam com `Fight_DrawActiveBotsOverlay()`, `Fight_AnimateEntryFrames(bot)` e `Sprite_UpdateComposite4Motion(bot*4+0x5a, bot, 0)`. O bot ativo sai do byte fight+0x80. Um contador em bot+0x84 conta os quadros do passo. Ao chegar ao limite, o passo grava o próximo GS num ponteiro de DAT. Base do bloco do bot: 0x03000590 + bot*0x100 (README da pesquisa, R1). Os nomes usam `Fight_` + verbo + objeto + `Step`.

## 0x08012C0C Fight_SetupEntryScreenStep
- O que faz: passo único de montagem da tela de entrada do bot: overlay, tiles, texto de nome, faixa, barra de gauge, número, indicadores; velocidade de animação 8; avança GS.
- RAM: bot+0x84 (contador), DAT de GS. Índice i=6.
- Confiança: provável (nome pelas chamadas; corpo confirmado no decompile).

## 0x08012D98 Fight_EntryMotionWaitStep
- O que faz: anima o bot na entrada até o contador chegar a 0x14; limpa 0x1E..0x2A (ClearObjectAttributeEntries 0x1E,0xB) e avança GS.
- RAM: bot+0x84; OAM via ClearObjectAttributeEntries. i=8.
- Confiança: provável.

## 0x08012DF4 Fight_EntryFieldDrawStep
- O que faz: desenha dois campos conforme o contador (chamadas Fight_DrawTwoFieldsIfCounterLow10/5); ao contador 0x10 chama Fight_CopyTableByteToVar e avança GS.
- RAM: bot+0x84. i=9.
- Confiança: provável.

## 0x08012EA8 Fight_SlideCounterObjectsStep
- O que faz: desliza seis objetos até o alvo e desenha campos de contador; ao contador 10 redesenha o rótulo do nome e fixa GS=7.
- RAM: bot+0x84; ponteiro de GS. i=12.
- Confiança: provável.

## 0x08012F00 Fight_SelectBotActionStep
- O que faz: seleciona a fonte da ação do bot (ciclo, substituição pendente ou Medaforça via Fight_SelectMedaforceActionSource); atualiza sprites das peças; incrementa o índice do bot; escolhe o próximo GS (4, 0x40 ou 0x90 conforme flag e limite fi+0x5a).
- RAM: fight+0x80..0x84, +0x89, +0x5a; bot+0x80 (índice). i=13.
- Confiança: provável (lógica de decisão lida no decompile; nomes dos auxiliares do Ghidra).

## 0x08012FF8 Fight_CountActionUsageStep
- O que faz: incrementa o contador de uso da ação do bot; inicializa os contadores de sprite; GS = 0xA0 (flag fi+0x89=1) ou 0x45.
- RAM: fight+0x87, +0x89. i=14.
- Confiança: provável.

## 0x080130D8 Fight_ShowSelectedActionPanelStep
- O que faz: no fim do contador 0x10, mostra o painel da ação escolhida (ícone, rótulo, descrição, slot de efeito 9) e avança GS.
- RAM: bot+0x84; slot 9 de efeito. i=17.
- Confiança: provável.

## 0x08013228 Fight_CounterFieldPairStep
- O que faz: desenha os pares de campo 4 e 12 pelo contador; ao contador 0x10 faz CpuFastSet e GS=0xB.
- RAM: bot+0x84; VRAM via bios_cpufastset. i=19.
- Confiança: provável.

## 0x080133B8 Fight_ShowAltSelectedActionStep
- O que faz: variante de Fight_ShowSelectedActionPanelStep para o outro lado (kind E/F/G, objeto no quadro 84, FUN_08017268, FUN_080173b4).
- RAM: bot+0x84. i=25.
- Confiança: provável.

## 0x08013508 Fight_CounterFieldPairAltStep
- O que faz: como Fight_CounterFieldPairStep, com o par 12B; ao contador 0x10 faz CpuFastSet e GS=0xB.
- RAM: bot+0x84. i=27.
- Confiança: provável.

## 0x08013614 Fight_LoadObjSlot2Step
- O que faz: anima o bot, carrega o slot de objeto 2 (Obj_InitSlotAndLoad 1,2,0) e avança GS.
- RAM: OBJ. i=40.
- Confiança: provável.

## 0x08013650 Fight_DispatchBotStateMachineStep
- O que faz: anima o bot e roda StateMachine_DispatchStep sobre a tabela indexada pelo bot; quando termina, zera bot+0x1B e avança GS.
- RAM: bot+0x1B, tabela de máquina de estado. i=41.
- Confiança: provável.

## 0x080136B0 Fight_DispatchSubStateMachineStep
- O que faz: anima o bot e roda StateMachine_DispatchSub; ao terminar, GS=0xB.
- RAM: DAT de GS. i=42.
- Confiança: provável.

## 0x080136E8 Fight_CopyPaletteBuffersStep
- O que faz: copia os buffers de paleta para o hardware (CopyPaletteBuffersToHardware), zera o contador e avança GS.
- RAM: bot+0x84; paleta. i=32.
- Confiança: confirmado (chamada explícita; resto provável).

## 0x0801372C Fight_FadeInPaletteStep
- O que faz: fade de linhas de paleta para a cor A e cor B (Palette_FadeRowsToColorA/B) em 9 passos crescentes (contador << 2), depois avança GS.
- RAM: bot+0x84; paleta. i=33.
- Confiança: provável.

## 0x08013A74 Fight_FadeOutPaletteStep
- O que faz: como o anterior, mas o fade vai de 0x20 para 0 (passo -4 por quadro); ao fim, GS=0xB.
- RAM: bot+0x84; paleta. i=61.
- Confiança: provável.

## 0x08013DE4 Fight_PreviewSelectedBotStep
- O que faz: passo da prévia grande do Medabot selecionado: avança o slot de animação, pisca, renderiza o quadro grande, desenha os slots da linha, move o slot (modo 8), faz blend GPU; avança GS quando o blend acaba.
- RAM: contexto da prévia (DAT_08013e28 e 08013e2c). i=36.
- Confiança: provável. A tela de prévia é diferente de UpdateSelectedMedabotPreviewScreen 0x08013E30 (tabela 0x083AD0F0, README).

## 0x08014050 Fight_LoadResultSceneStep
- O que faz: incrementa o uso das ações ativas, marca bot+0x84=1, descompacta gráficos (decompress_and_store), CpuFastSet, pede fade de som (8) e SFX 0x6e; avança GS.
- RAM: bot+0x84; ROM comprimida. i=64.
- Confiança: provável (nome pelo conjunto de chamadas; "resultado" é suposição).

## 0x080140B0 FUN_080140B0 (não renomeado)
- O que faz: máquina de quatro estados (0..3) que move um objeto (0x78/0x48 para 0x40/0x38, +8 e -8 até 0x60/0x40) e checa a flag 0x03.. antes de avançar. Limpa atributos (3,2) e fixa GS ao fim.
- RAM: bot+0x84, bot+0x86, OAM, DAT de flags. i=65.
- Confiança: chute (muitos campos, sem nome claro; deixado como FUN_).

## 0x0801425C Fight_PlayResultCueStep
- O que faz: toca o sinal de resultado pelo estado (Fight_PlayResultCueByState), reinicia contadores de sprite e timer, fixa GS=0x44.
- RAM: DAT de GS. i=66.
- Confiança: provável.

## 0x08014314 Fight_BotEventQueueStep
- O que faz: desenha overlay, tica os bots ativos (exceto tipos de status 0F/10), ordena a fila de eventos do bot; se a fila está vazia GS=0x44, senão remove o evento ou põe os objetos do bot (bit 4 de bot+0xBD).
- RAM: fight+0x45, +0x87; bot+0x40, +0xBD. i=69.
- Confiança: provável.

## 0x080143A0 Fight_ToggleBotFlagAdvanceStep
- O que faz: inverte o bit 4 de bot+0xBD, põe os objetos do bot, tira o evento da fila, avança o ciclo de especialidade; se nenhum bot tem flag, GS=0x45, senão zera fi+0x45 (12 bytes) e GS=0x4C.
- RAM: fight+0x45..0x51; bot+0xBD. i=70.
- Confiança: provável.

## 0x08014404 Fight_PickBotActionStep
- O que faz: tica os bots ativos, inverte o bit 4 de bot+0xBD, atualiza sprites das peças, tira o evento da fila, monta a metadata das ações; escolhe a ação (ciclo ou override pendente) para os bots 0..2 e o estado de espera/troca para os inimigos; GS vai para 0xA0, 0x45 ou 4.
- RAM: fight+0x80..0x87, +0x89; bot+0xC9 (vírgula = ciclo), +0x44. i=71.
- Confiança: provável.

## 0x08014548 Fight_UpdateBotMeterMotionStep
- O que faz: atualiza os seis sprites dos bots; para cada bot com flag ativa desloca a posição X pelo medidor e o objeto de medidor; se o Y chega ao repouso ou fora da tela, limpa o bit 0x40 do bot e GS=0x45.
- RAM: bot+0x40, +0xBB, +0xBF. i=76.
- Confiança: provável.

## 0x080145BC Fight_HighlightBotOamAStep
- O que faz: quando o bot ativo tem bit 1 de bot+0xCE, põe um bit de atributo no OAM (0x1180 + bot*0xC0) e SFX 0x37; avança o contador de fase.
- RAM: OAM 0x07000000+ (cálculo 0x1180 de fight), bot+0xCE. i=96.
- Confiança: provável (par inverso de 0x08014634).

## 0x08014634 Fight_ClearBotHighlightOamAStep
- O que faz: inverso do anterior: para bots com bit 1 de bot+0xCE, reduz 4 por quadro no campo de posição (até 4) e então limpa o bit 1 de bot+0xCE e os atributos OAM; GS=0x44 ao fim.
- RAM: OAM, bot+0xCE. i=97.
- Confiança: provável.

## 0x080146DC Fight_HighlightBotOamBStep
- O que faz: igual ao 0x080145BC, com bit 2 de bot+0xCE (base OAM 0x11C0); SFX 0x37.
- RAM: OAM, bot+0xCE. i=104.
- Confiança: provável.

## 0x08014754 Fight_ClearBotHighlightOamBStep
- O que faz: inverso do 0x080146DC, com bit 2: a condição lê fight+0x55 (bit 4); ao fim limpa o bit 2 de bot+0xCE e um bit de outro ponteiro (0xFB); GS=0x44.
- RAM: OAM, bot+0xCE, fight+0x55. i=105.
- Confiança: provável.

## 0x080147FC Fight_DrawBotNumberOamStep
- O que faz: para bots com bit 4 de bot+0xCE, escreve as tiles do número (valor de bot+0xC8) e monta as entradas OAM; SFX 0x5b; avança GS.
- RAM: bot+0xC8, +0xCE; OAM. i=112.
- Confiança: provável.

## 0x08014870 Fight_StepBotNumberAnimStep
- O que faz: anima as partes de bots com bit 4 a cada quadro; no contador 0x5A limpa o bit 4, apaga atributos OAM (3 entradas), limpa o bit 0x08 de fight+0x55 e GS=0x44.
- RAM: bot+0x84, +0xCE; fight+0x55. i=113.
- Confiança: provável.

## 0x08014920 Fight_DrawBotNumberOamAltStep
- O que faz: igual ao 0x080147FC, para bots com bit 8 de bot+0xCE, com valor em bot+0xCD e modo 1; SFX 0x5b.
- RAM: bot+0xCD, +0xCE; OAM. i=120.
- Confiança: provável.

## 0x08014994 Fight_StepBotNumberAnimAltStep
- O que faz: igual ao 0x08014870, para o bit 8; ao fim limpa 0x10 de fight+0x55 e GS=0x44.
- RAM: bot+0x84, +0xCE; fight+0x55. i=121.
- Confiança: provável.

## 0x08014A44 Fight_LoadObjSlot0Step
- O que faz: carrega o slot de objeto 0 (Obj_InitSlotAndLoad 1,0,0) e avança GS.
- RAM: OBJ. i=144.
- Confiança: provável.

## 0x08014A84 Fight_WaitOverlayTargetStep
- O que faz: manda o overlay para o alvo (Overlay_SetTargetAndCheckReached); quando chega, zera dois contadores e avança GS.
- RAM: bot+0x86, contador de estado. i=146.
- Confiança: provável.

## 0x08014ADC Fight_ResetBattleFieldsStep
- O que faz: zera campos do contexto de batalha (+0x51..+0x54, +0x1A6, e um offset vindo de DAT_08014b28), fixa +0x1B0=1 e zera o contador (bot+0x86); avança GS.
- RAM: contexto de batalha (DAT_08014b24). i=148.
- Confiança: provável.

## 0x08014C84 Fight_SelectEnemyMedaforceStep
- O que faz: para os bots 3..5 (inimigos), se o byte de estado é maior que 3, chama Fight_SelectMedaforceActionSource(idx, estado-4); avança GS.
- RAM: bot+0xBD... (byte do estado de cada bot inimigo); DAT de GS. i=151.
- Confiança: provável.

## 0x08014CC8 Fight_DispatchSubStateMachineAltStep
- O que faz: roda StateMachine_DispatchSub; quando termina, incrementa o contador de GS. Mesma estrutura do 0x080136B0.
- RAM: DAT de GS. i=152.
- Confiança: provável.

## 0x08015438 Data_08015438_BadInstruction (kind=label, não é função)
- O que é: no Ghidra vira função de 3 bytes com "bad instruction data" (halt_baddata). Não é código de passo; o endereço é alvo de BL de 0x08012BF0 (linha da FS5_lista). Não renomeado.
- Confiança: confirmado (é dado/miolo, não função). Tratar como label; checar a ROM antes de citar.
