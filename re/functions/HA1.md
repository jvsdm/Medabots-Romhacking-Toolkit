# HA1: handlers dos despachantes Fight_DispatchOnSubstateAA_* (lista HA1_lista.txt)

Mecanismo (confirmado no código, stub A 0x0801C4FC e pools dos stubs B..E): o stub zera +0xB5 do bloco
0x03001800 e chama `table[+0xAA]` via trampolim 0x0807CCF0. +0xAA = `action_sequence_state` (FightActionScriptState
em +0xA8, campo +0x2; tipos em `re/types/medabots_simon_types.tsv`). Cada handler roda enquanto o subestado
é o seu índice; no fim ele incrementa +0xAA (passo seguinte) ou grava um valor fixo.
Tabelas (ROM, 4 bytes por entrada, índice = subestado): A 0x083AD5CC (0x30 entradas), B 0x083AD68C (9),
C 0x083AD6B0 (8), D 0x083AD6D0 (8), E 0x083AD6F0 (16). Tamanhos = distância entre pools (confirmado).
Placeholders (não são handler): 0x08000841 (A 18,19,1E,1F,29-2C; B 04-08; C 04-07; D 04-07; E 0C-0F) e
0x080305E1 (B 01, C 01). Provável sentinela/dado; não documentados.

Campos de tela usados pelos handlers (nomes provisórios, pelo código): +0x51 = lado/variante (0..3, usado
para escolher tiles/anim), +0x52 = índice de anim, +0x84..+0x87 = contadores e slot do bot, +0x89 = flag de
lado, +0xA6 = próximo passo de apresentação, +0xD0/+0xD1 = dados do bot/flag. Literais DAT_ não resolvidos
no decompile desta rodada (confiança "provável" nos endereços de RAM).

## Família A (Fight_DispatchOnSubstateAA_*)

## 0x0801C524 Fight_HandleRangeMismatchFailure (A/00)
- O que faz: falha de ação quando a classe da peça não bate com a faixa exigida (proximidade x distância).
- Nome: mtbe.csv (Simon/Rokusho), não reconferido aqui. Confiança: provável (nome de terceiros).
- Estado: passo de escolha/validação; grava falha e avança.

## 0x0801C660 Fight_DispatchOnSubstateAA_A_02_ApplyDamageToHighestAlivePart (A/02)
- O que faz: varre as 4 peças do bot (flags em bot+0x59 + 0x18*i, i = 3..0, do maior para o menor) e grava em
  bot+0xD0 o índice da primeira com flag != 0. Depois chama `Fight_ApplyDamageToPart(slot+0x87, +0xD0,
  fight+0x2D+8*(+0x89))` e avança +0xAA.
- Literais: fight_information 0x03001800, bot 0x03000590. Confiança: provável (fluxo lido; "flag != 0 = peça viva"
  é inferência). Nome mtbe conflitante: `do_damage_on_part` / `Fight_ApplyDamageToPart` (ver MERGE_RELATORIO).

## 0x0801C6C4 Fight_DispatchOnSubstateAA_A_03_LoadSideTilesAndAdvance (A/03)
- O que faz: escolhe a fonte de tiles pelo lado (+0x51): lados 0/1/2 descomprimem um bloco e fazem CpuFastSet
  (8 words) para o destino do lado; lado 3 descomprime e faz CpuFastSet; depois avança +0xAA. Lado 0 e 1 usam
  destinos diferentes (0x0801C700 / 0x0801C718).
- Chama: decompress_and_store, bios_cpufastset. Confiança: confirmado (fluxo); fontes/destinos são ponteiros de
  literal não lidos, então quais arquivos de tile fica "provável".

## 0x0801C774 Fight_DispatchOnSubstateAA_SetupBotPoseSfx (A/04)
- O que faz: monta sprite/pose do bot (+0x840 = (idx+2)*0x400|…), zera +0x85C, grava +0x??=6, 0x40; sfx 0x3E
  (lado +0x51==1 ou 0) ou 0x61 (lado 2/3). Lado 0/1 avança +0xAA; lado 2/3 grava +0xAA=0x10.
- Lê: +0x3C, +0x51, +0x89, +0xAA. Escreve: bloco 0x03001840.., +0x52, +0xAA.
- Confiança: provável (código lido, literais não resolvidos).

## 0x0801C988 Fight_DispatchOnSubstateAA_TickCounterSfx (A/05, e A/23 mesmo código)
- Soma 8 a um contador; ao chegar a 0x40 toca sfx 0x3F se +0xAA < 0x20 e avança +0xAA em 1.
- Confiança: provável.

## 0x0801C9C8 Fight_DispatchOnSubstateAA_RunAnimThenBaseSlot (A/06)
- Posiciona o bot por direção, roda script de anim 0x21 e, quando termina (-1), volta à base e avança +0xAA.
- Chama: Fight_SetBotSlotPosFromDirection, Anim_RunSlotScript, Fight_SetBotBaseSlotAndMarkers. Confiança: confirmado.

## 0x0801CA2C Fight_DispatchOnSubstateAA_ClearTilesWhenCounterDone (A/07, A/13)
- Contador em 0x?? sobe de 8 em 8 (ou desce -8); ao chegar a 8, limpa slots 0x21 e avança +0xAA.
- Chama: Tile_ClearSlots. Confiança: confirmado.

## 0x0801CA60 Fight_DispatchOnSubstateAA_ShowNumberTiles (A/08)
- Escreve número (6 dígitos) na tela com OAM do bot; sfx 0x5B; zera +0xB5-0x6A; avança +0xAA.
- Chama: Fight_WriteNumberTiles, Fight_BuildBotOamEntriesAt. Passo de apresentação de valor (dano/ROS).
- Confiança: provável.

## 0x0801CAA8 Fight_DispatchOnSubstateAA_StepRowAnimation (A/09)
- Anima uma linha de tiles por passo; contador sobe 1 por quadro; passa de 0x32 com flag → força 'x';
  quando o estado 'x' é atingido, limpa 4 slots e avança +0xAA. Passo de animação de barra/linha.
- Confiança: provável.

## 0x0801CB24 Fight_DispatchOnSubstateAA_FormatDecimalValue (A/0A)
- Formata valor de 16 bits em 6 glifos (fonte 0x35) e grava +0xA6=0x12; avança +0xAA.
- Chama: FUN_0803f63c, Num_FormatDecimal6GlyphsBase. Confiança: provável.

## 0x0801CB74 Fight_DispatchOnSubstateAA_SubtractPartArmor (A/0C)
- Dano: subtrai armadura da peça do bot (slot +0x87, lado +0xD0, valor +0xC8), com piso zero; avança +0xAA.
- Chama: Fight_SubtractPartArmorClampZero. Confiança: confirmado (nome e corpo lidos).

## 0x0801CBA4 Fight_DispatchOnSubstateAA_BackupHalvedStats (A/0D)
- Se +0xD1 != 0: sfx 0x3B, faz backup de stats (metade) se flag limpa, +0xA6=0x13, vai para 0x1A.
  Se +0xD1 == 0: vai para 0x20. Confiança: confirmado (fluxo lido).

## 0x0801CBE4 Fight_DispatchOnSubstateAA_A_0F_QueueBrokenBotOrSkip (A/0F)
- O que faz: se o byte bot+0x59 (flag do bot, peça 0) == 0, chama `Fight_QueueHeadBrokenBotForResolution(slot)` e
  grava +0xAA=0x2F; senão grava +0xAA=0x20. Não avança +1: desvia.
- Confiança: provável (fluxo lido; o sentido "cabeça quebrada" vem só do nome do chamado).

## 0x0801CC24 Fight_DispatchOnSubstateAA_A_10_WaitSlotCoordThenSfx (A/10)
- O que faz: `Fight_StepSlotCoordUpInRange(0x21, 0x3C, 0x84)`; quando retorna != 0 toca sfx 0x41, zera o byte
  +0x84 (bloco 0x03000440) e avança +0xAA. Enquanto não retorna, espera.
- Chama: Fight_StepSlotCoordUpInRange, snd_sfx_request. Confiança: provável.

## 0x0801CC58 Fight_DispatchOnSubstateAA_CounterThenAdvance (A/11)
- Chama FUN_080353ac a cada quadro; contador do byte +0x84 sobe; ao passar de 0x1D toca sfx 0x3F, zera e avança.
- Confiança: provável.

## 0x0801CCB8 Fight_DispatchOnSubstateAA_SlideFacingX (A/12)
- Move o X do slot (Fight_SetFacingSlotX) em passos de 1 até 0x5A; ao fim copia o X e avança +0xAA.
- Confiança: provável.

## 0x0801CD08 Fight_DispatchOnSubstateAA_A_14_BranchIfSideTwoElseJump8 (A/14)
- O que faz: se +0x51 (lado) == 2 avança +0xAA em 1; senão grava +0xAA = 8 (salto para o handler 8).
- Confiança: confirmado (corpo de 1 bloco lido).

## 0x0801CD28 Fight_DispatchOnSubstateAA_A_15_SetMsg15Advance (A/15)
- O que faz: grava +0xA6 = 0x15 (código de mensagem) e avança +0xAA. Confiança: confirmado.

## 0x0801CD40 Fight_DispatchOnSubstateAA_A_17_JumpTo20 (A/17)
- O que faz: grava +0xAA = 0x20 (salto de estado). Confiança: confirmado.

## 0x0801CDB4 Fight_DispatchOnSubstateAA_A_1B_WaitSlotCheckAdvance (A/1B)
- O que faz: `FUN_080370AC(0x21, 4)`; quando retorna != 0 avança +0xAA. Enquanto isso, espera.
- Confiança: provável (o chamado FUN_080370AC continua sem nome; ver próxima rodada).

## 0x0801CF78 Fight_DispatchOnSubstateAA_A_20_DrainTargetValueStep (A/20)
- O que faz: se lado == 2, decrementa o byte fight+0x2C+8*(+0x89) e, ao zerar, avança; senão grava +0xAA = 0x2E.
  Se lado != 2, subtrai de fight+0x2C+8*(+0x89) a metade de bot+0xA2 (do bot +0x87) enquanto não passar de zero:
  grava '-' (0x2D) e repete; quando não dá mais, avança +0xAA.
- Confiança: provável (a semântica de +0x2C como "valor por alvo" e de +0xA2 é inferida).

## 0x0801D1AC Fight_DispatchOnSubstateAA_A_25_SlideSlotUntilAnimEnds (A/25)
- O que faz: enquanto `Fight_StepSlotAnimOffsetTable(0x21)` retorna 0 (animação rodando), move a coordenada de 16
  bits do slot 0x21 em +1 (fight+0x89 == 0) ou -1 (senão), contando em fight+0x84 (se bit0 de +0x84 é 0). Quando
  a anim termina, limpa o slot 0x21 (`Tile_ClearSlots(0x21,1)`) e avança +0xAA.
- Confiança: provável (ponteiros de literal não lidos; sentido do bit0 de +0x84 não confirmado).

## 0x0801D244 Fight_DispatchOnSubstateAA_A_28_BranchBySideTwo (A/28)
- O que faz: se +0x51 == 2 grava +0xAA = 0x2E, senão +0xAA = 0x2D. Confiança: confirmado.

## 0x0801D264 Fight_DispatchOnSubstateAA_A_26_ClearTargetValueSetMsg25 (A/26)
- O que faz: zera 4 bytes de fight+0x2A..0x2D no bloco 8 bytes do alvo (+0x89), grava +0xA6 = 0x25 e avança
  +0xAA. Confiança: confirmado.

## 0x0801D2AC Fight_DispatchOnSubstateAA_A_2D_EndResult0 (A/2D)
- O que faz: +0xB5 = 1 (handler concluído) e +0xAB = 0. Confiança: confirmado (escritas lidas); o sentido de
  +0xAB como código de resultado é provável.

## 0x0801D2C4 Fight_DispatchOnSubstateAA_A_2E_EndResult1 (A/2E)
- O que faz: +0xB5 = 1 e +0xAB = 1. Confiança: confirmado (escritas); sentido de +0xAB provável.

## 0x0801D2D8 Fight_DispatchOnSubstateAA_A_2F_EndResult2 (A/2F)
- O que faz: +0xB5 = 1 e +0xAB = 2. Confiança: confirmado (escritas); sentido de +0xAB provável.

## Miolos sem função própria (confirmado no Ghidra)
- 0x0801D245, 0x0801D265, 0x0801D2AD, 0x0801D2C5, 0x0801D2D9: são o bit Thumb das entradas acima (a tabela guarda o
  endereço com bit 0 = 1). Mesmas funções; não são handlers separados.

## 0x0801CD50 Fight_DispatchOnSubstateAA_LoadTilesPlaceSprite (A/1A)
- Descomprime tiles (decompress_and_store), CpuFastSet, posiciona sprite do bot (tabela por lado/slot), avança +0xAA.
- Chama: decompress_and_store, bios_cpufastset, FUN_08036ff8. Confiança: confirmado.

## 0x0801CDD4 Fight_DispatchOnSubstateAA_LoadTilesSetupSlot (A/1C)
- Descomprime tiles, CpuFastSet; grava +0x840=0x?; monta slot com Slot_SetupFromRecordList(…,0x21); avança +0xAA.
- Confiança: provável.

## 0x0801CE94 Fight_DispatchOnSubstateAA_FinishAndLoadSideTiles (A/1D)
- Roda anim 0x21 até -1; então limpa slots, refresca sprites de peça, posiciona bots e carrega tiles do lado
  (+0x51 = 0..3); grava +0xAA = 0x0E (salto de estado, não +1). Confiança: confirmado.

## 0x0801CFFC Fight_DispatchOnSubstateAA_LoadSideTilesThenJump (A/21)
- Descomprime tiles conforme lado (+0x51); avança +0xAA; depois salta por tabela não recuperada no Ghidra
  (0x0801D05C). Confiança: provável (destino do salto não resolvido).

## 0x0801D06C Fight_DispatchOnSubstateAA_SetupBotAndSprites (A/22)
- Parecido com A/04: monta sprite e pose (+0x840, +0x85C, valores 6/0x40/8/0xF0/0x3C); lado define 0x68/0x88
  e posição (0xB00/0xC00/0xD00/0xE00); avança +0xAA. Confiança: provável.

## 0x0801D174 Fight_DispatchOnSubstateAA_ClearFlagsSfx (A/24)
- Zera dois bytes de estado, toca sfx 0x34, avança +0xAA. Confiança: confirmado.

## 0x0801D245 (A/28), 0x0801D265 (A/26), 0x0801D2AD (A/2D), 0x0801D2C5 (A/2E), 0x0801D2D9 (A/2F)
- São as mesmas funções documentadas acima (A/28, A/26, A/2D, A/2E, A/2F); a tabela guarda o endereço com o bit
  Thumb. Ver as seções desta rodada.

## Família B (Fight_DispatchOnSubstateAA_B_*), stub B 0x0801D2F0 (pool 0x0801D310, provável)

## 0x0801D318 Fight_DispatchOnSubstateAA_B_00_CheckSourcePartBroken (B/00)
- O que faz: lê a peça-fonte da ação (índice = bot+0xDD − 1, peça no bloco do bot 0x03000590 + slot*0x100, flag
  em +0x59 + 0x18*índice). Se a flag é 0 (peça "quebrada", provável): grava +0xA6 = 5 (código de resultado) e
  avança +0xAA. Senão: +0xB5 = 1 (concluído) e +0xAB = 0, sem efeito.
- Confiança: confirmado (fluxo e offsets lidos no Ghidra; nome "quebrada" provável). Bate com o nome do mtbe
  (`ActionFamily_SwordHammer_Handler`, Simon/Rokusho).

## 0x0801D36C Fight_DispatchOnSubstateAA_B_03_EndResult1 (B/03)
- O que faz: +0xB5 = 1 e +0xAB = 1 (fim da ação, código 1). Confiança: confirmado (escritas lidas).

## Família C (Fight_DispatchOnSubstateAA_C_*), stub C 0x0801D380 (provável)

## 0x0801D3A8 Fight_DispatchOnSubstateAA_C_00_CheckTargetPartBroken (C/00)
- O que faz: igual ao B/00, mas a peça testada é bot(+0x88)*0x100 + 0x03000590 + 0x59 + 0x18*(+0x8B). Flag 0
  (provável "quebrada"): +0xA6 = 0x0C e avança +0xAA. Senão: +0xB5 = 1 e +0xAB = 0.
- Confiança: confirmado (fluxo lido); semântica da flag provável.

## 0x0801D3F8 Fight_DispatchOnSubstateAA_C_03_EndResult1 (C/03)
- O que faz: +0xB5 = 1 e +0xAB = 1. Confiança: confirmado (escritas lidas).

## Família D (Fight_DispatchOnSubstateAA_D_*), stub D 0x0801D40C (provável)

## 0x0801D434 Fight_DispatchOnSubstateAA_D_00_CpuFastSetAndAdvance (D/00)
- O que faz: `bios_cpufastset(src, dst, 8)` com ponteiros de literal (não lidos) e avança +0xAA.
- Confiança: confirmado (fluxo); conteúdo copiado provável.

## 0x0801D45C Fight_DispatchOnSubstateAA_D_01_ClearSlot9AndPlaceSprites (D/01)
- O que faz: `Tile_ClearSlots(9,1)`; grava dois campos 16 bits de posição (base de bot: +0x1742/+0x1744 do bot
  do lado +0x87 e do alvo +0x88, com −4 de ajuste); +0x25C = 0; +0x24B = 0xF0; +0x264 = 6; monta o slot 9 com
  `Slot_SetupFromRecordList(lista do literal +0x6C, 9)`; avança +0xAA.
- Confiança: provável (fluxo e offsets lidos; base do bloco de RAM não confirmada).

## 0x0801D524 Fight_DispatchOnSubstateAA_D_02_WaitSlot9ReachTarget (D/02)
- O que faz: `Fight_StepSlotTowardTarget(9)`; quando retorna != 0 avança +0xAA. Enquanto isso, espera.
- Confiança: confirmado (corpo de 1 chamada lido); o nome do chamado é o que dá o sentido.

## 0x0801D544 Fight_DispatchOnSubstateAA_D_WaitAnimDone (D/03)
- Roda anim 9 do slot; quando termina (-1) limpa slot 9 e grava +0xB5=1 (sinal de fim para o despachante).
- Chama: Anim_RunSlotScript, Tile_ClearSlots. Confiança: confirmado.

## Família E (Fight_DispatchOnSubstateAA_E_*), stub E 0x0801D5A0 (pool), 16 entradas

## 0x0801D5A8 Fight_FindAllyFollowupActionSource (E/00)
- Procura fonte de ação encadeada (follow-up) no lado aliado; grava 1-based em fight_information[0xDE] e
  tipo em [0xDF]; avança ou marca passo completo. Nome: mtbe.csv (Simon/Rokusho). Confiança: provável.

## 0x0801D6A4 Fight_DispatchOnSubstateAA_E_01_LoadPhaseGfx (E/01)
- O que faz: escolhe a carga de gráfico pelo tipo de follow-up (fight+0xDF): 0x1B e 0x1C descomprimem um bloco
  e fazem CpuFastSet (8 words); 0x1D descomprime e faz CpuFastSet; outros tipos só avançam. Avança +0xAA sempre.
- Chama: decompress_and_store, bios_cpufastset. Confiança: confirmado (fluxo lido); fontes não lidas.
- Nome já estava no Ghidra (criado por outra sessão com o mesmo padrão). Nome mtbe conflitante:
  `ActionFamily_CancelGravHandler` (Simon/Rokusho): conferir na revisão.

## 0x0801D738 Fight_PrepareFollowupSourceDisplay (E/02)
- Copia o bot-fonte para área de exibição temporária, reconstrói peças e cria sprite composto. Nome: mtbe.csv.
  Confiança: provável.

## 0x0801D878 Fight_AnimateFollowupSourceIntro (E/03)
- Anima o sprite temporário até a posição; ao fim semeia o estado temporário e avança. Nome: mtbe.csv.
  Confiança: provável.

## 0x0801D95C Fight_AdvanceFollowupSourceApproach (E/04)
- Avança o sprite temporário para a próxima posição. Nome: mtbe.csv. Confiança: provável.

## 0x0801D9DC Fight_DispatchOnSubstateAA_E_05_SlideBotIntoPlace (E/05)
- O que faz: a cada quadro move a coordenada 16 bits do bot de follow-up (fight+0xDE, bloco 0x1640 + 0x100*idx)
  em −2 (ou +2, se fight+0xF8 bit0 = 1) até cruzar 0x78; chama `Sprite_UpdateComposite4Motion`. Ao chegar, grava
  os dados do sprite (0x78, tiles 8, flag 0) e avança +0xAA. No fim faz retorno normal (`pop {r4-r7}; pop {r0}; bx r0`, sem tabela; o Ghidra leu o `bx r0` como salto por tabela em 0x0801DADE, "Too many branches"). Próximo passo: E06, só ao chegar (JT.md, E/05).
- Confiança: confirmado (fluxo e retorno, JT.md); o salto por tabela não existe.
- Nome já estava no Ghidra (outra sessão). Nome mtbe da linha da tabela pode diferir: conferir.

## Fora da HA1 (mesma tabela E, índices 06..0B; ver HA2 ou lista seguinte)
- 0x0801DAED (E/06), 0x0801DC75 (E/07), 0x0801DD31 (E/08), 0x0801DD75 (E/09), 0x0801DDDD (E/0B).

## Máquina de estados que estes handlers formam (esboço)
- Estado = +0xAA. Família A cobre apresentação de dano/ROS: 04/22 (setup de pose) → 06/1A/1C (tiles e sprite)
  → 0A (número) → 0C (subtrai armadura) → 0D (backup de stats; 0x1A ou 0x20) → 1D (fim, salta para 0x0E)
  → 21/24 (carga de tiles e flag).
- Família E é a cadeia de follow-up: 00 (achar fonte aliada) → 02 (preparar exibição) → 03 (intro) → 04 (aproximação)
  → 05 (desliza o bot até o lugar; salto indireto final não recuperado no Ghidra). Nota: 01 é carga de gráfico
  por tipo de follow-up (fight+0xDF), não "cancelar gravidade" como diz o mtbe; conferir.
- Família D: 03 é o único com sinal de término (+0xB5=1). Família B e C (redo desta rodada): 00 testa a peça-fonte
  (ou alvo) e 03/C03 terminam com código 1; todos com Ghidra. Família A: 2D/2E/2F terminam com código 0/1/2.
- Estados sem handler em A (ex.: 0x01, 0x0E, 0x16, 0x18/0x19, 0x1E..0x1F, 0x23 aponta para C989, 0x29..0x2C
  são placeholder) ficam para a rodada de funções sem código no Ghidra.

Pendente redo: as 24 seções marcadas sem função/pendente foram refeitas (decompile + rename no Ghidra + CSV). Restam: (1) FUN_080370AC e Fight_StepSlotCoordUpInRange sem nome (A/1B, A/10); (2) E/05 (0x0801DADE) é retorno normal, não salto por tabela (JT.md): fechado; (3) Família A já "provável" (A/04..A/22) não reconferida nesta rodada; (4) conflitos com mtbe: A/02 (do_damage_on_part), B/00 (SwordHammer), E/01 (CancelGrav), E/05 (nome do mtbe); (5) placeholders 0x08000841 e 0x080305E1 não são handler, continuam sem seção; (6) A/23 e a entrada 0x23 -> C989 ainda sem conferência.
