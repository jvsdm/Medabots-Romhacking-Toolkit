# fight_information 0x03001800 - tabela completa

Legenda: conf = confirmado (decompile ou literal lido na ROM) | provável (texto dos .md, sem decompile) | chute.
"Escritor/leitor" = funções citadas nos .md (nome ou endereço). CONFLITO = documentos divergem.

## Tabela 1 - fight_information 0x03001800

| Offset | Tam | Campo | Quem escreve | Quem lê | Conf | Obs |
|---|---|---|---|---|---|---|
| +0x000 | 1 | índice de nome de personagem (gTextCharacterNames[F+0x00]) | HN10 (0x080310A0) | HN10 | chute | só 1 doc |
| +0x008 | 4 (word) | índice do passo de cena (0..7) | JT2 (0x08071648, 0x08071670, 0x080716FC) | JT2 | provável | fi+0x08 += 1 |
| +0x028 | — | NÃO é campo separado: é o mesmo array de 8 B de +0x02A (fi+0x2A + 8*k) (FI2, Conflito 2) | HN2 (0x08028078 AT_CheckSupport), AS2 | DM, HN2 | resolvido | |
| +0x02A | 8 x N | um só array de registros de 8 B: fi+0x2A + 8*k (FI2, Conflito 2) | HA1 (0x0801D264 A_26), HA5 (0x08021DAC T_11), HA4 (T_0), AS2 | HA4 (T_0) | resolvido | |
| +0x02C | 1 | contador de drenagem do registro (8*89) | HA1 (0x0801CF78 A_20 DrainTarget: decrementa se lado==2) | HA1 | provável | |
| +0x02D | 1 | dano do registro (8*89) | HA1 (0x0801C660 A_02 ApplyDamage) | HA1 | provável | |
| +0x038 | 1 | índice gfx (tabela 0x083B2628, 5 palavras) | não documentado | JT, JT2 (0x080441A8), OW, ST2 | provável | tabela não decodificada |
| +0x03C | 1 | base de paleta do bot ((F+0x3C)+2) | não documentado | HN9 (0x08031704) | chute | só 1 doc |
| +0x051 | 1 | modo de efeito da peça (0..3): 10->0, 11->1, 12->2, outro->3, do byte +0x49 da peça | HA4 S_0 (0x08020D24 e outros S_0/S_1/T_0) | HA4 S_09 (0x080211C8, conf: se ==3 timer=1 senão 0x3C), A6, HA4 S_1, HD, AI2 | conf (decompile S_09) | CONFLITO: BT (0x08030DB4) usa "F+0x51 = 0x28/0x50" como valor de SFX |
| +0x052 | 1 | cópia do byte +0x49 da peça | HA4 S_0 (0x08020D24) | HA4 | provável | |
| +0x053 | 1 | fase do lado B (!=0 -> msg 0x71) | HA4 S_1 (0x08021564) | HA4, A6, HD | provável | HD: +0x51 = lado A |
| +0x055 | 1 | bitmask de fase secundária (bit1->0x60, bit2->0x68, bit3->0x70, bit4->0x78) | BT | BT (Fight_TickSecondaryE...) | provável | |
| +0x057 | 1 | tempo esgotado (==0 -> Fight_EvaluateTeamWinState) | não documentado | BT, HN10 (0x08031120) | provável | |
| +0x058 | 1 | elegibilidade (medaforce; nível 10/30/60) | MF | AI, BT (0x08031614), HN9 (BR_40: !=0 -> F+0xA6=0x79), MF | provável | |
| +0x05B | 1 | nº de bots inimigos (laço i=0..n-1) | Fight_UpdateBotActiveFlagCount (não confirmado) | EN (Fight_InitializeEnemyBotStates) | provável | |
| +0x05C | 1 | bot da ação | AI (0x08033154 Fight_SetPartActionMetadata) | AI, AI2 | provável | |
| +0x05D | 1 | peça-1 da ação | AI | AI, AI2 | provável | |
| +0x05E | 1 | alvo+1 (slot 1-based) | AI (0x0803301C Fight_ValidateTargetSlot) | AI, TQ | provável | |
| +0x05F | 1 | linha de status sorteada | TQ | TQ | provável | |
| +0x060 | 1 | lado base do alvo (0 ou 3) = Fight_GetOpponentSideBaseIndex(fi+0x87) | HN7 (0x0802E010 BL_0B MarkOpp), TQ | HN9 (0x0802F8E4 loop F+0x60+i), AI, HANDOFF | conf | |
| +0x061..+0x063 | 3 | candidatos de slot-alvo (!=0 válido) | TQ | AI (DAT_0803300C) | provável | |
| +0x064..+0x067 | 4 | linha da lista de prioridade (copiada de ROM 0x083AD470 + (rand&0x1F)*4) | AI (0x080156D0, único chamador 0x08015490) | AI (Fight_SelectActionSource), HO1 (0x08039AC8: >=0x78 -> 3) | provável | CONFLITO: AI2 diz que o conteúdo não bate com ids de peça |
| +0x084 | — | REMOVIDO: não é fi+0x84. É o contador de passo do ctx 0x03000440+0x84 (FI2, Conflito 7; literal 0x0801D1E0 = 0x03000440) | - | - | conf | |
| +0x086 | 2 | NÃO é fi. Timer de espera = 0x03000440+0x86 (ctx) | - | - | conf (decompile 0x08030674 e 0x080211C8; literal 0x080306A0 = 0x03000440) | CONFLITO: HA4 (linhas 130, 135) diz "fi+0x86 (timer)" - errado; HANDOFF está certo |
| +0x087 | 1 | bot ativo (atacante, passo 0x100) | vários Fight_DispatchOnSubstateAA_* (HA4 M_0/O_0/Q_0/S_0/T_0, FL4 0x0801C1C0) | FL4 (0x0801C4E0), HA4, HA6, MD, DM, AS2 (~84 refs) | conf (HANDOFF/DM) | eixo principal |
| +0x088 | 1 | bot alvo (F+0x88) | HA4 (N_0, O_0, P_0, Q_0, R_0), HA6 (AC_08, AC_0E, AD_08, AE_08), DM | HA4 (+0xBD do bot alvo), HA6, HA3 | provável | |
| +0x089 | 1 | slot-alvo i (índice dos registros de 8 B) | HA1 (A_25), HA6 (Z_AddGaugeMax 0x080233E0), DM | HA5, HA4, HA1 | provável | |
| +0x08A | 1 | ramo/índice do registro do time | HA4 (S_0C 0x08021300, S_0F 0x08021418, S_23 0x08021774), DM | HA4, DM | provável | ver conflito +0x028/+0x02A |
| +0x08B | 1 | peça escolhida (0..3) | HA4 O_0 (0x08020844: fi+0xD0 = fi+0x8B); HA4 P_01, R_01 (RW) | HA4, HA7 | provável | |
| +0x09C | 1 | índice da tabela C de substates (0x083AD5A4, 10 entradas) | FL4, N1 (0x0801C32C, 0x0801C3F8), SD | FH (0x0801C2F0 RunSubstateDispatchC) | provável | |
| +0x09D | 1 | argumento da tabela C | FH, SD | SD | provável | |
| +0x09E | 1 | flag de saída da tabela C (!=0 -> retorna 1) | SD | FH | provável | |
| +0x0A1 | 1 | índice da tabela A de substates (0x083AD580, 9 entradas; ==3 retorna 1 e zera) | N1 (0x0801C0F8, 0x0801C138: incrementa), SD (0x0801C110) | FH (0x0801C0BC Fight_RunSubstateDispatch), A6 | conf (A6 + FH) | |
| +0x0A2 | 1 | code: argumento da tabela A | FH (grava code) | SD (0x0801C110 chama FUN_0801AC24) | conf | |
| +0x0A6 | 1 | código de passo/mensagem (indexa roteiro de msg) | vários handlers (A_26 "=0x25", HN9, BT, HN6, HO1, JT) | 0x080305E0 (lê e chama RunSubstateDispatch), BT (0x08030E54) | conf (A6 + texto) | |
| +0x0A7 | 1 | índice da peça k: 0 cabeça, 1 braço D, 2 braço E, 3 pernas | HN9 (0x08031614: F+0xA7 = +0xDD-4), HA4 | HA6 (Fight_AccumRosPart), BT, HA4 (+0xA7*0x18) | provável | |
| +0x0A8 | 1 | CONFLITO de sentido: DM = índice de roteiro; HN9 = técnica da peça (+0x49); AS1 = índice em table[fight+0xA8]; MF = B+0xA8+0x50 | HN9, BT | AS1, DM, TQ | provável | resolver |
| +0x0AA | 1 | passo (substate) da ação; handlers fazem +=1 ou =4 para saída | todos os Fight_DispatchOnSubstateAA_* (~70); Fight_Shared_WaitTimerOrSkip 0x08030674 (+=1 quando timer zera) (conf) | 0x080305E0, 0x0802FE50 (tabela 0x083AEC98), FH, A6, DM | conf (0x08030674 e 0x0802F8E4) | 99 refs |
| +0x0AB | 1 | índice variável de comparação de script (+0xAC+0xAB) | HA4 | AS1 | provável | |
| +0x0AC | 1 | byte comparado no script de espera | AS1 | AS1 | provável | |
| +0x0B5 | 1 | handler concluído (=1) | HA3, HN6 (0x0803080C), HO1 (fim do script) | stub do despachante (zera antes) | conf (HANDOFF + HN6) | |
| +0x0B7 | 1 | flag de condição (bit0) | não identificado | HA3, HO1 (FD = (B7==1)) | provável | |
| +0x0B8 | 1 | flag de condição (bits 1, 2) | não identificado | HA3, HO1 (FD = (B8==2) / (B8==4)) | provável | |
| +0x0BC | 2 (u16) | cursor do roteiro de ação (ROM[ptr[F+0xA8] + F+0xBC]) | DM, AS1 (avanço) | TQ (FUN_08034E94), DM | provável | |
| +0x0C8 | 2 (u16) | valor de combate (dano atual; dígitos em HA3) | FH (0x08036BB0), HD, MD (C8=CC=dano), CF (copia armadura +0x59) | HA3, HN7, HN9 | provável | CONFLITO leve: CF diz "armadura", MD/FH dizem "dano" |
| +0x0CA | 1 | flag (0 quando dano < armadura) | MD | HA4 | provável | |
| +0x0CC | 2 (u16) | espelho de +0xC8 | FH, HD, MD | HN7, HN9 | provável | |
| +0x0D0 | 1 | peça do reparo (= fi+0x8B) | HA4 O_0 | HA4 (0x08036CCC Fight_ClampRepairToMissingArmor) | provável | |
| +0x0D1 | 1 | flag "peça quebra" | FH, HD, CF (=1), MD (=0), DM | HN7 (bit7 de D2) | provável | |
| +0x0D2..+0x0D4 | 1 por alvo i (0..2) | bit7 = D1 (quebra); bits0..6 = índice de peça (x0x18) do bot alvo (fi+0x60+i) | HN7 (BK/BL), BP_08 (0x0802F8E4, conf) | BP_08, HN9, HN7, HN5 (DAT_0802CAC0 = fi+0xD2) | conf (decompile BP_08) | CONFLITO: HN7 mapeia 1->2, 2->3; o decompile usa o valor direto |
| +0x0D6..+0x0DA | 2 (u16) por alvo i | dano por alvo i (BP_08 grava C8) | BP_08 (conf), HN7 | BP_08, HN9 | conf (decompile) | CONFLITO: AS2 (0x08034910, inline) diz D6+2k = armadura>>1 |
| +0x0DE | 2 (u16) | bot de follow-up (bloco 0x1640 + 0x100*idx); bit0 de +0xF8 = sentido | JT (literal 0x0801DA20) | HA1, MD | provável | |
| +0x0DF | 1 | tipo de follow-up (0x1B/0x1C = gfx; 0x1B = -DEF) | JT | HA1, MD, DM | provável | |
| +0x0E0..+0x0E2 | 3 | bytes de pendência | HA3 | HA3 | provável | |
| +0x0E8 | 2 (u16) | total de ROS | Fight_AccumRosPart (CF, AI) | CF, MD, DM | provável | |
| +0x0EA | 2 (s16) | pré-dano (Fight_SeedPreDamageFromTotalRos) | Fight_SeedPreDamageFromTotalRos | MD | provável | |
| +0x0EC | 2 (u16) | acumulador de dano/ataque | CF (Fight_ComputePartValue, Fight_ScaleAttackValueByPartScalar), AS2 (0x08034A68) | HA4, DM, MD | provável | |
| +0x0EE | 2 (u16) | redução do defensor (scratch) | MD (0x08038F34) | DM | provável | |
| +0x0F2 | 2 (u16) | ROS resolvido (cópia) | MD | DM | provável | |
| +0x0F6 | 1 | flag bit0 (-> FD) | não identificado | HO1 | provável | |
| +0x0F8 | 1 | bit0 = sentido do movimento; bit1 = usar 0x158 em vez de 0x154 | não identificado | FL4, HA1, HA4, JT, HA2 | provável | |
| +0x0F9 | 1 | banco da técnica (linha de 0x083CE164) | não identificado | HO1 | provável | |
| +0x0FD | 1 | flag de condição (rótulo a[fight+0xFD]) | HO1 | HO1 | conf (texto) | |
| +0x100 | 2 (u16) | cursor do script de objeto | HO1 | HO1 | provável | |
| +0x102.. | 2 cada (halfwords) | argumentos do opcode atual (aN em 0x102+2N) | HO1 (0x0803D450 copia) | HO1 | provável | |
| +0x112 | 1 | código de movimento (0x29) | HO1 | HO1 (MotionPlayStep) | provável | |
| +0x154 / +0x156 | 2 cada | X/Y do bot, lado 0 (F+0xF8 bit1 = 0) | HO1 (X = 300 fora da tela) | HA2, JT | provável | |
| +0x158 / +0x15A | 2 cada | X/Y do bot, lado 1 (F+0xF8 bit1 = 1) | HO1 (X = 0x1C4), JT | HA2, JT | provável | |
| +0x15F / +0x160 | 1 cada | flag do lado (espera) | não identificado | HO1 | provável | |
| +0x164 | 1 | flag "medaforce ok" | HN9, MF | HN9 | provável | |
| +0x165 | 1 | flag de level-up (?0x60:0x4F) | não identificado | BT, HN10 | provável | |
| +0x166 | 1 | índice de habilidade que subiu (tabela 0x083AEE98) | não identificado | HN10 | chute | |
| +0x167 | 1 | novo nível | não identificado | HN10 (Fight_WriteNumberTiles) | chute | |
| +0x16C | 2 (u16) | XP restante | RW (subtrai; zera ao subir) | RW | provável | |
| +0x16E + k*8 | 8 cada | eventos de nível | RW | RW | provável | |
| +0x170 / +0x172 / +0x173 | 1-2 | ids de medaforça (medal_medaforce_list) | MF | MF | provável | |
| +0x174 | 1 | flag de medaforça (nível 10/30/60) | MF | RW | conf (texto) | |
| +0x188 + u | 1 cada (slots 10, 11) | valor de efeito | HO1 | HO1 | provável | |
| +0x1A0 | 1 | barra de valor 0..0x60 | não identificado | HD (0x0803EA38) | provável | |
| +0x1AE | 1 | flag (=1 quando c==9) | HA2 | JT | provável | |
| +0x1B0 | 1 | estado do lado (bit8) | não identificado | HD | provável | |
| +0x840 | 2 (u16) | halfword de gráfico | HA3 | HA3 | provável | |

## Tabela 2 - contexto 0x03000440 (parcial; orçamento de contexto esgotado)

| Offset | Tam | Campo | Quem escreve | Quem lê | Conf | Obs |
|---|---|---|---|---|---|---|
| +0x084 | 1 | contador de animação (FL2/FL3: fade e entrada da luta) | FL2 (0x? passos), FL3 | FL2, FL3 | provável | JT/HA6 zeram; ver conflito com fi+0x84 |
| +0x086 | 2 | timer de espera (decrementa; ao zerar +0xAA da fi += 1) | S_09 0x080211C8 (=1 se ctx+0x51 == 3, senão 0x3C; conf) | 0x08030674 Fight_Shared_WaitTimerOrSkip (conf) | conf | |
| +0x089 | 1 | flag "ctx livre" (==1 libera despacho de UI; A6) | (não identificado) | A6, EN (Fight_SetupBattleBotStates), DT (randbyte se !=0) | provável | |
| +0x087 | 1 | bot ativo do ctx (FL2: anima sprites laterais) | não identificado | FL2 | provável | |
| +0x088 | 1 | bot lateral (Fight_LoadBotSideTiles) | não identificado | FL2 | provável | |
| +0x139 | 1 | seletor de cena (= 0x03000579) | não identificado | BT (Dispatch_ByteStateIndexTable, 0..5) | conf (BT, JT) | |
| +0x13A | 1 | byte de estado/passo (= 0x0300057A) | não identificado | BT (Fight_RunStateDispatch 0x0801A788, 0..8) | conf (literal 0x0801A7AC) | |
| +0x19 / +0x1A / +0x1B | 1 cada | +0x1B = flag de texto (zerado ao terminar a mensagem, A6) | A6 (zera ctx+0x1B) | A6 | provável | |
| +0x9C | 2 (u16) | vencedor de Fight_EvaluateTeamWinState (0x030004DC) | BT, HN9, HN10 (0x03000440+0x9C) | BT, HN10, DF | conf (DF, literal 0x0801B588) | NÃO é fi+0x9C |
| +0x80 | 1 | byte de entrada (Fight_AnimateEntryFrames) | não identificado | FL2, FL3 | provável | |
| +0x8D | 1 | índice do randbyte da tabela 0x083B29EC | DT | DT | provável | |

Nota: as tabelas 2 e 3 estão incompletas (Pendente). Em HA1 e JT, "fi+0x84" pode ser ctx+0x84; ver a seção de conflitos.

## Tabela 3 - bloco E de eventos 0x030057E0..0x030064A0 (parcial)

| Offset | Tam | Campo | Quem escreve | Quem lê | Conf | Obs |
|---|---|---|---|---|---|---|
| E+0xC2A (0x0300640A) | 1 | id de template de encontro (0..245) | EN (Event: gMapEncountersExtra sorteio, ou id direto) | EN (copia 0x28 B de 0x083C1B80 + id*4 para 0x03001800) | provável | |
| E+0xC2D (0x0300640D) | 1 | flag: 0 = sorteia entre 4 ids do mapa | quem põe = 1 na história: NÃO achado | EN | provável | aberto (EN §7) |
| E+0xC5F (0x0300643F) | 1 | repete o despacho de cena se != 0 | não identificado | EV (0x083D212C[0x0300137C]) | provável | |
| E+0xC89 (0x03006469) | 1 | "continuação pendente" do opcode | Event_ReadMessageArgsAndDefer 0x0804B758 (liga); EV | Event_DispatchContinuation 0x0804DC0C; HC1/HC2 | conf (HC1, EV) | zerado por Event_EndOpcode 0x08050290 |
| E+0xC8A | 2 (u16) | cursor da continuação | EV | EV | provável | |
| E+0xC8C | 1 | não documentado (lido em HC?) | não identificado | não identificado | chute | pendente de leitura |
| E+0xC92 | 1 | índice da tabela de continuação (table2) | não identificado | Event_DispatchContinuation (0x0804DC0C, literal 0x0804DC28) | conf | |
| E+0xC93 | 1 | subestado do opcode | Event_EndOpcode (zera) | HC1, HC2 | conf | |
| E+0xC93 (0x03006473) | 1 | zerado em Event_EndOpcode | EV (conf) | EV | conf | |
| E+0xC94.. | n | argumentos do opcode | leitor de opcode | HC1 | provável | |

Pendente: E+0xC8C e E+0xC92 precisam de decompile (Event_DispatchContinuation 0x0804DC0C) e de confirmação da ordem de escrita.

## Conflitos (resumo)
- +0x086: HA4 atribui ao fi; decompile confirma timer em 0x03000440+0x86. Corrigir HA4.
- +0x028 x +0x02A: um só array de registros de 8 B em fi+0x2A + 8*k (FI2, Conflito 2). Resolvido (antes: dois arrays).
- +0x0A8: resolvido (FI2, Conflito 3): é o id da técnica/roteiro (0..59), índice de gTechniqueActionScriptPtrTable 0x083C6CA8.
- +0x0D2: HN7 mapeia peça; decompile usa valor direto.
- +0x0D6: dano (HN7 e decompile) x metade da armadura (AS2).
- +0x051: BT usa como valor de SFX.
- +0x084: HA1 x ctx.
