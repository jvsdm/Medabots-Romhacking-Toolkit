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

## 0x0801C660 (A/02) sem função no Ghidra
- Sem função definida no Ghidra; não decompilado. Pendente (criar função e reexecutar). Confiança: nenhuma.

## 0x0801C6C4 (A/03) sem função no Ghidra
- Idem A/02.

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

## 0x0801CBE4 (A/0F) sem função no Ghidra; 0x0801CC24 (A/10) sem função no Ghidra
- Pendentes (mesmo caso de A/02).

## 0x0801CC58 Fight_DispatchOnSubstateAA_CounterThenAdvance (A/11)
- Chama FUN_080353ac a cada quadro; contador do byte +0x84 sobe; ao passar de 0x1D toca sfx 0x3F, zera e avança.
- Confiança: provável.

## 0x0801CCB8 Fight_DispatchOnSubstateAA_SlideFacingX (A/12)
- Move o X do slot (Fight_SetFacingSlotX) em passos de 1 até 0x5A; ao fim copia o X e avança +0xAA.
- Confiança: provável.

## 0x0801CD08 (A/14), 0x0801CD28 (A/15), 0x0801CD40 (A/17), 0x0801CDB4 (A/1B), 0x0801CF78 (A/20),
## 0x0801D1AC (A/25), 0x0801D244 (A/28), 0x0801D264 (A/26), 0x0801D2AC (A/2D), 0x0801D2C4 (A/2E),
## 0x0801D2D8 (A/2F) sem função no Ghidra
- Pendentes. (0x0801CD08 e 0x0801CD28 são alvos de entradas 0x14/0x15; 0x0801CD50 é A/1A, já abaixo.)

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
- Sem função no Ghidra (ver acima; alvos de entrada na tabela). Pendentes.

## Família B (Fight_DispatchOnSubstateAA_B_*), stub B 0x0801D2F0 (pool 0x0801D310, provável)

## 0x0801D318 (B/00) sem função no Ghidra; 0x0801D36C (B/03) sem função no Ghidra. Pendentes.

## Família C (Fight_DispatchOnSubstateAA_C_*), stub C 0x0801D380 (provável)

## 0x0801D3A8 (C/00) e 0x0801D3F8 (C/03): sem função no Ghidra. Pendentes.

## Família D (Fight_DispatchOnSubstateAA_D_*), stub D 0x0801D40C (provável)

## 0x0801D434 (D/00), 0x0801D45C (D/01), 0x0801D524 (D/02): sem função no Ghidra. Pendentes.

## 0x0801D544 Fight_DispatchOnSubstateAA_D_WaitAnimDone (D/03)
- Roda anim 9 do slot; quando termina (-1) limpa slot 9 e grava +0xB5=1 (sinal de fim para o despachante).
- Chama: Anim_RunSlotScript, Tile_ClearSlots. Confiança: confirmado.

## Família E (Fight_DispatchOnSubstateAA_E_*), stub E 0x0801D5A0 (pool), 16 entradas

## 0x0801D5A8 Fight_FindAllyFollowupActionSource (E/00)
- Procura fonte de ação encadeada (follow-up) no lado aliado; grava 1-based em fight_information[0xDE] e
  tipo em [0xDF]; avança ou marca passo completo. Nome: mtbe.csv (Simon/Rokusho). Confiança: provável.

## 0x0801D6A4 (E/01) sem função no Ghidra. Pendente.

## 0x0801D738 Fight_PrepareFollowupSourceDisplay (E/02)
- Copia o bot-fonte para área de exibição temporária, reconstrói peças e cria sprite composto. Nome: mtbe.csv.
  Confiança: provável.

## 0x0801D878 Fight_AnimateFollowupSourceIntro (E/03)
- Anima o sprite temporário até a posição; ao fim semeia o estado temporário e avança. Nome: mtbe.csv.
  Confiança: provável.

## 0x0801D95C Fight_AdvanceFollowupSourceApproach (E/04)
- Avança o sprite temporário para a próxima posição. Nome: mtbe.csv. Confiança: provável.

## 0x0801D9DC (E/05) sem função no Ghidra. Pendente.

## Fora da HA1 (mesma tabela E, índices 06..0B; ver HA2 ou lista seguinte)
- 0x0801DAED (E/06), 0x0801DC75 (E/07), 0x0801DD31 (E/08), 0x0801DD75 (E/09), 0x0801DDDD (E/0B).

## Máquina de estados que estes handlers formam (esboço)
- Estado = +0xAA. Família A cobre apresentação de dano/ROS: 04/22 (setup de pose) → 06/1A/1C (tiles e sprite)
  → 0A (número) → 0C (subtrai armadura) → 0D (backup de stats; 0x1A ou 0x20) → 1D (fim, salta para 0x0E)
  → 21/24 (carga de tiles e flag).
- Família E é a cadeia de follow-up: 00 (achar fonte aliada) → 02 (preparar exibição) → 03 (intro) → 04 (aproximação)
  → 05 (pendente).
- Família D: 03 é o único com sinal de término (+0xB5=1). B e C só têm 2 handlers cada, todos pendentes.
- Estados sem handler em A (ex.: 0x01, 0x0E, 0x16, 0x18/0x19, 0x1E..0x1F, 0x23 aponta para C989, 0x29..0x2C
  são placeholder) ficam para a rodada de funções sem código no Ghidra.
