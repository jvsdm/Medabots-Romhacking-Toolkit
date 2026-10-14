# FI2 - tabelas 2 e 3 do fight_information (contexto 0x03000440 e bloco E 0x030057E0)

Complemento de FI.md. Legenda de confiança: confirmado (decompile do Ghidra ou literal lido na ROM) | provável (só texto dos .md) | chute | aberto.

Achado geral (vale para toda a tabela 2): 0x03000440 é um bloco de scratch reutilizado por cena. O mesmo offset
tem significado diferente conforme a tela (ex.: +0x3C..+0x3F na tela de time = coluna/linha/estado; na tela de
mensagem +0x1B é despacho; na luta +0x84/+0x86/+0x9C são contador, timer e vencedor). Antes de documentar um campo,
confirmar a cena (gamestate_subroutine 0x0300137C / gamestate_menu). Endereço absoluto = 0x03000440 + offset.

## Tabela 2 - contexto 0x03000440 (ctx)

| Offset | Abs | Tam | Campo | Quem escreve | Quem lê | Conf | Obs |
|---|---|---|---|---|---|---|---|
| +0x004 | 0x03000444 | 1 | estado do motor de mensagem (indexa gMessageStateHandlerTable 0x083B291C, 0..8) | StateMachine_DispatchStep 0x0803FB3C, MS2 (ctx+4++) | A6, MS, MS2 | confirmado (literal 0x0803FB5C; tabela GH-MS) | 0 = Gfx_PaletteFlushStep, 5 = ScriptVM_RunOpcodes |
| +0x008 | 0x03000448 | 4 (ptr) | ponteiro do roteiro da mensagem | StateMachine_DispatchStep (A6) | ScriptVM_RunOpcodes (A6, MS) | provável (A6, MS) | |
| +0x00C | 0x0300044C | 2 (u16) | pc do roteiro da mensagem | ScriptVM_RunOpcodes | ScriptVM_RunOpcodes | provável (A6, MS) | |
| +0x013 | 0x03000453 | 1 | parada da mensagem | não identificado | MS | provável (MS) | |
| +0x014..+0x017 | 0x03000454.. | 1 cada | caneta X, largura restante, subcoluna, banco | motor de texto (MS) | MS | provável (MS) | |
| +0x018 | 0x03000458 | 1 | palavra pronta | motor de texto (MS) | MS | provável (MS) | |
| +0x019 | 0x03000459 | 1 | modo da mensagem (1 = nova linha, 2 = espera tecla) | MS | MS | provável (MS) | |
| +0x01A | 0x0300045A | 1 | contador de espera (0xFF = sem espera) | MS | MS | provável (MS) | |
| +0x01B | 0x0300045B | 1 | byte de despacho (índice de Dispatch_CallHandlerByByteAt1B 0x08040974, tabela 0x083B2990, 23 handlers) e zerado ao terminar a mensagem | A6 (zera ao fim), MS | JT2 (0x08040974), A6 | confirmado (literal 0x08040990 = 0x03000440; GH-MS) | Conflito de nome resolvido: é ao mesmo tempo despacho e flag de fim (mesmo byte). Ver FI.md "+0x1B = flag de texto" |
| +0x01C | 0x0300045C | 1 | fade da mensagem | MS | MS | provável (MS) | |
| +0x025 | 0x03000465 | 1 | página da mensagem | MS | MS | provável (MS) | |
| +0x026 | 0x03000466 | 1 | passo de estado 3 | MS | MS | confirmado (GH-MS) | |
| +0x027 | 0x03000467 | 1 | flag itálico | MS | MS | confirmado (GH-MS) | |
| +0x02A | 0x0300046A | 1 | byte lido na checagem de StateTable (N2 "ctx+0x2A"; se != 0 ...) | não identificado | N2 | provável (N2) | sem decompile; ver N2.md |
| +0x033 | 0x03000473 | 1 | byte de estado dos despachantes de opção do debug (tabela 0x083ACAC0, 34 entradas) | menus de debug (DB, JT3) | Dispatch_StateHandlerTable 0x0800BAD8 e irmãs | confirmado (GH-DB, literais 0x0800BAF4/0x0800BE88/0x0800D184) | Cena de debug; não é usado na luta |
| +0x034 | 0x03000474 | 1 | cursor de linha do debug (gDebugRowCursor) | DB | DB | confirmado (GH-DB) | 3 linhas, wrap 0..2 |
| +0x035..+0x037 | 0x03000475..77 | 1 cada | valores das 3 linhas do debug (gDebugRowValue0..2); no Motion = kinds | DB | DB, Debug_ProcessJumpScreen | confirmado (GH-DB) | A: v0 -> gMainState, v1 -> gamestate_menu, v2 -> gamestate_subroutine |
| +0x039 | 0x03000479 | 1 | índice de bot do visualizador Motion (0..121) (gDebugBotIndex) | DebugMotion_Main 0x0800D710 | DebugMotion_Main | confirmado (GH-DB) | |
| +0x03F | 0x0300047F | 1 | estado da tela de time (Medabots/peças) | PS (ctx_team_screen) | PS | aberto: PS e PS2 divergem | PS: +0x3F estado; PS2: +0x3F slot (+1 = número). Pendente decompile da tela de time |
| +0x03C / +0x03D | 0x0300047C / 0x0300047D | 1 cada | tela de time: PS = coluna / linha; PS2 = modo (4 = música 7) / índice | PS, PS2 | PS, PS2 | aberto (PS x PS2) | Ver nota de conflito |
| +0x042 / +0x044 / +0x046 | 0x03000482/84/86 | 1-2 | flags da tela de time (dono/grande; PS2) | PS2 | PS2 | provável (PS2) | |
| +0x050 / +0x051 / +0x052 | 0x03000490/91/92 | 1 cada | lock, cursor, origem do SELECT (tela de time, PS) | PS | PS | provável (PS) | |
| +0x065 | 0x030004A5 | 1 | estado de saída de mapa (FL1 ctx+0x65 != 2) | FL1 | FL1 | provável (FL1) | ctx de FL1 não confirmado como 0x03000440 |
| +0x080 | 0x030004C0 | 1 | byte de entrada da luta (Fight_AnimateEntryFrames) | não identificado | FL2, FL3 | aberto | FI.md indicava ctx 0x03000440; FL2/FL3 não dizem o ctx. Pendente |
| +0x084 | 0x030004C4 | 1 | contador de passo do ctx (conta até 0x03, 0x10, 0x20, 0x3C, 0x41...) | A_25 0x0801D1AC (decompile: incrementa ctx+0x84 quando o passo do slot 0x21 retorna 0; bit 0 gateia o avanço do slot); HA6 Z_06/Z_09; HA2 E_03/G_00 (zera); N2 (zera); JT | HA2, HA4, HA6, HN2, HN4, FL2 (fades), JT | confirmado (A_25 decompile + literal 0x0801D1E0 = 0x03000440) | Resolve conflito 7 do FI.md: é do ctx, NÃO de fi+0x84 |
| +0x085 | 0x030004C5 | 1 | zerado no fim do despachante genérico GS (BT2) | BT2 0x08010264 | - | provável (BT2) | |
| +0x086 | 0x030004C6 | 2 (u16) | timer de espera (decrementa; ao zerar +0xAA da fi += 1) | 0x3C ou 60 em HA2 H18, HA3, HA5 T_0x0A, HA6, HN2, HN4, HN9 BQ_06, HN6 AG_09 | Fight_Shared_WaitTimerOrSkip 0x08030674 (decompile); HA5, HN6, HA6 | confirmado (decompile 0x08030674, 0x080211C8; literal 0x080306A0 = 0x03000440) | HA4 (linhas 130, 135) e FI.md tabela 1 atribuem a fi+0x86: ERRADO |
| +0x089 | 0x030004C9 | 1 | flag do ctx: == 1 libera o despacho de UI (A6), != 0 habilita randbyte (DT), == 0 permite pular espera (HA5, HN6, SD) | não identificado | A6, EN, DT, HA5, HN6, SD | provável (usos confirmados; quem grava não) | Nome conflitante: AI2 chama 0x030004C9 de gDeterministicRandom e FI_simbolos de gCtx_FreeFlag. Não resolvido |
| +0x08D | 0x030004CD | 1 | índice do randbyte (lê ROM 0x083B29EC + idx++) | DT | DT | provável (DT) | |
| +0x090 / +0x092 / +0x094 | 0x030004D0/D2/D4 | 2 cada | estado do efeito de blend (mode, step, 0) | R2 | R2 | provável (R2, cena de blend) | Uso de outra cena |
| +0x096 | 0x030004D6 | 1 | byte zerado no passo 0 do despachante JT (0x0801FC00) | JT | JT | provável (JT) | |
| +0x09C | 0x030004DC | 2 (u16) | vencedor de Fight_EvaluateTeamWinState (1 ou 2) (também BR_50 grava 2/1) | Fight_EvaluateTeamWinState 0x0801B53C (decompile), BR_50 0x08030E78 | BT, HN9, HN10, DF | confirmado (decompile 0x0801B53C; literal 0x0801B588 = 0x03000440) | Não é fi+0x9C. SD.md fala em fi+0x9C: provavelmente é este campo, não verificado |
| +0x0F7..+0x0FD | 0x03000537..0x0300053D | 1 cada (7 bytes) | especialidade do jogador (IWRAM_PlayerSpecialtyBase) | não achado | Medabot_SpawnFromRecord (DM) | aberto (DM) | |
| +0x12F | 0x0300056F | 1 | índice de slot de gObjectSlots (slot = 0x03002C30 + k*0x40) | literais 0x0803C6A4/A8 (JT2, JT3) | JT2, JT3 | provável (JT2/JT3) | seleção em dois níveis |
| +0x139 | 0x03000579 | 1 | seletor de cena (0..5) | HA2 G (zera; 0x0801E09C), JT J_02 (zera), HA3 | Dispatch_ByteStateIndexTable 0x08013904 (decompile), HA3 (0x0801FBC4: > 4 sai) | confirmado (decompile BT/JT2) | |
| +0x13A | 0x0300057A | 1 | byte de estado/passo (0..8) | JT (J_xx avança), HA2 (zera) | Fight_RunStateDispatch 0x0801A788 (decompile; tabela 0x0801A7B4) | confirmado (decompile GH-BT; literal 0x0801A7AC) | JT4 zera +0x84 e +1 em 0x0801FD3C |

Notas da tabela 2:
- Os campos +0x086, +0x089, +0x09C e +0x084 são usados nas duas leituras (ctx e fi) em docs antigos. Só ctx está confirmado.
- +0x02A e +0x033 em N2/DB: não confundir com o bloco fi (0x03001800).
- Pendente nesta tabela: +0x080 (escritor e ctx), +0x03C..+0x03F (tela de time: PS x PS2), +0x089 (escritor), e os nomes gCtx_FreeFlag x gDeterministicRandom (mesmo byte 0x030004C9).

## Conflitos resolvidos

### Conflito 2: fi+0x28 x fi+0x2A (registros de 8 bytes)
Resolvido. Não são dois arrays: é um só array de registros de 8 bytes em fi+0x2A + 8*k.
- Decompile de Fight_DispatchOnSubstateAA_AT_CheckSupportAlreadyActive 0x08028078: lê [fi + 8*(fi+0x8A) + 0x2A] (== 0 -> ação) e [fi + 8*(fi+0x8A) + 0x2B] (!= 'Z' 0x5A -> ação). Ou seja, o marcador "byte +3 == 0x5A" de DM/HN2 é o byte fi+0x2B, o mesmo endereço do "+0x2A base, byte 1".
- Decompile de Fight_DispatchOnSubstateAA_T_11_ClearRecordSlot 0x08021DAC: zera fi + 8*(fi+0x89) + {0x2A, 0x2B, 0x2C, 0x2D}. Confirma base +0x2A e índice +0x89.
- Índices diferentes para o mesmo array: +0x8A (checagem de suporte do time; AT_CheckSupport) e +0x89 (slot-alvo; T_11 e HA4/HA5). Doc de DM/HN2 com "+0x28 + 8*(+0x8A)" deve ser lida como +0x2A + 8*(+0x8A). Bytes: +0x2A flag/contador, +0x2B marcador 'Z' (0x5A) = ativo.
- Status: confirmado (decompile x2).

### Conflito 3: fi+0xA8 (sentido do campo)
Resolvido. fi+0xA8 = índice do roteiro de ação = id da técnica (0..59) na tabela gTechniqueActionScriptPtrTable 0x083C6CA8 (TQ.md, 4 bytes por entrada). Medaforça usa o mesmo índice deslocado: B+0xA8 + 0x50 (BR_49 0x08031918, MF.md).
- Decompile de Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart 0x08030A24: no ok grava fi+0xA8 = byte [bot*0x100 + part_rec + (fi+0xDD-1)*0x18 + 0x49] (técnica da peça, +0x49). Ou seja: HN9 ("técnica da peça +0x49") está certo como fonte; DM/AS1 ("índice de roteiro") estão certos como uso.
- "MF = B+0xA8+0x50": correto (offset 0x50 para medaforça).
- Status: confirmado (decompile BR_14 + BR_49 documentado).

### Conflito 7: fi+0x84 x ctx+0x84
Resolvido. Não existe contador de passo em fi+0x84; o contador é ctx+0x84 = 0x030004C4.
- Decompile de Fight_DispatchOnSubstateAA_A_25_SlideSlotUntilAnimEnds 0x0801D1AC: o literal 0x0801D1E0 = 0x03000440 (lido na ROM), e o código faz (base 0x03000440)+0x84 & 1 e incrementa (base)+0x84. O +0x89 do mesmo handler é do literal 0x0801D1E4 = 0x03001800 (fi). Ou seja, o doc HA1 ("+0x84 bloco 0x03000440") está certo e a linha de fi+0x84 da tabela 1 (contador de slots) deve ser removida.
- Status: confirmado (decompile + literal lido da ROM).

## Tabela 3 - bloco E de eventos 0x030057E0 (E + offset)

| Offset | Abs | Tam | Campo | Quem escreve | Quem lê | Conf | Obs |
|---|---|---|---|---|---|---|---|
| E+0xC2A | 0x0300640A | 1 | id de template de encontro (0..245) (gEvt_EncounterTemplateId / gEncounterTemplateId) | EN (sorteio em gMapEncountersExtra[mapa] quando E+0xC2D == 0); Event_Op33_StartBattle 0x0804C53C (byte 1 direto) | EN (copia 0x28 B de 0x083C1B80 + id*4 para 0x03001800) | confirmado (EN_simbolos, HE2) | |
| E+0xC2B | 0x0300640B | 1 | gStepCounter: passos no mapa; > 16 começa o sorteio de encontro | passos do mapa (OW) | EN | confirmado (OW.md) | Faltava em FI.md |
| E+0xC2D | 0x0300640D | 1 | gEncounterRollSuppressed: 0 = sorteia entre os 4 ids do mapa; != 0 = usa E+0xC2A direto | não achado na história | EN (Fight_SelectTemplateForCurrentEncounter) | confirmado no código; quem põe 1 = aberto | aberto (EN §7) |
| E+0xC2E | 0x0300640E | 1 | pós-batalha (ação após a luta, byte 3 do Op33) | Event_Op33_StartBattle (lê 3 bytes) | Continuação 0x0804E8C4 | provável (HE2) | |
| E+0xC2F | 0x0300640F | 1 | modo da batalha (byte 2 do Op33) | Event_Op33_StartBattle | não lido (HE2: "modo ignorado") | provável (HE2) | |
| E+0xC5F | 0x0300643F | 1 | repete o passo da cena no mesmo quadro se != 0 (laço: despacho 0x083D212C[0x0300137C] de novo) | não identificado (zerado no End 0x0804B7C8) | EV (0x08052330 laço) | confirmado (EV §laço; zerado no End) | Resolve "não identificado" parcial; quem liga = aberto |
| E+0xC60 | 0x03006440 | 1 | índice guardado pelo Op4C (parte faltante) e lido pelo Op51 (compara com args) | Event_Op4C_Jump_If_Missing_Part 0x0804CA84 | Event_Op51 0x0804CC5C | confirmado (HE3_simbolos) | EV.md também cita C60 como flag de cena: uso duplo, anotar |
| E+0xC88 | 0x03006468 | 1 | gCurrentEventPendingFlag: != 0 = evento rodando | Map_CheckWalkOverTrigger 0x08052400, Map_CheckFacingEventTriggerAhead 0x08052330 | laço de evento (EV) | confirmado (EV §1 e decompile) | NÃO zerado por Event_EndOpcode |
| E+0xC89 | 0x03006469 | 1 | gCurrentEventStateByte: 0 = fase primária; 1 = continuação pendente | Op31/Op32/Op33 (decompile Op31 0x0804C4B4: grava 1 em C89); Event_ReadMessageArgsAndDefer 0x0804B758 | Script_DispatchHandler 0x0804B6F4 (zera); Event_DispatchContinuation 0x0804DC0C; HC1/HC2 | confirmado (decompile Op31) | zerado por Event_EndOpcode 0x08050290 e Script_DispatchHandler |
| E+0xC8A | 0x0300646A | 2 (u16) | cursor do script (gCurrentEventScriptCursor); lido byte a byte por EventScript_ReadNextByte 0x0804D83C | EventScript_ReadNextByte; zerado por gatilhos e Event_Op19_GotoEvent 0x0804BD70 | EventScript_ReadNextByte | confirmado (EV, HE1) | Divergência de EV.md: o zero escrito em 0x0300646B é a segunda metade do mesmo u16 |
| E+0xC8C | 0x0300646C | 2 (u16) | cursor salvo: cópia de E+0xC8A no início do handler (base dos saltos: alvo = C8C + desloc) | handlers de opcode com salto (Op31 0x0804C4B4: u16 C8C <- C8A, confirmado no decompile; Op32, Op4C, HE4, HN6, HN7) | handlers de continuação (HC3: E+0xC8C + E+0xC94) | confirmado (decompile Op31; HC3, HE2) | Resolve "não documentado" do FI.md |
| E+0xC8E | 0x0300646E | 2 (u16) | gCurrentEventId (BE na leitura do Goto) | Map_CheckWalkOverTrigger, Map_CheckFacingEventTriggerAhead, Event_Op19_GotoEvent | EventScript_ReadNextByte, HE1 | confirmado (EV_simbolos) | |
| E+0xC92 | 0x03006472 | 1 | gCurrentEventOpcodeByte: opcode atual; índice de 0x083D222C (Script_DispatchHandler) e de 0x083D2630 (Event_DispatchContinuation 0x0804DC0C, literal 0x0804DC28) | Script_DispatchHandler 0x0804B6F4 | Script_DispatchHandler, Event_DispatchContinuation | confirmado (EV, EV_simbolos) | FI.md dizia "não identificado" como escritor: é Script_DispatchHandler |
| E+0xC93 | 0x03006473 | 1 | gCurrentEventSubstate: subestado do opcode (fases de continuação) | Script_DispatchHandler e Event_EndOpcode 0x08050290 (zeram) | HC1, HC2, HC3 | confirmado (EV) | linha duplicada no FI.md removida |
| E+0xC94.. | 0x03006474.. | n | gEventScriptArguments: argumentos do opcode (lidos por Script_ReadBytes 0x0804D8CC) | Script_ReadBytes; Op31/Op32 gravam o byte de salto em C94 | HC1 (Op04 usa C94 como contador de frames), HC3 | confirmado (EV, HC1 decompile) | |
| E+0xCB4 | 0x03006494 | 1 | índice da entrada ativa do objeto de mapa do evento (-1 se não há objeto) | Map_CheckWalkOverTrigger, Event_Op19 | HE1, HE3, HN7 | confirmado (EV) | |
| E+0xCB5 | 0x03006495 | 1 | flag Sim/Não da caixa de escolha: 0 = Op31 (Sim/Não), 1 = Op32 (Não/Sim) | Event_Op31_YesOrNoBox 0x0804C4B4 (decompile: grava 0), Event_Op32_NoOrYesBox 0x0804C4F8 (grava 1) | leitor não achado; a continuação 0x0804E860 (HC3) NÃO lê | confirmado (escrita, decompile Op31); leitor = aberto | Resolve o conflito de FIX.md (ordem vem do literal FUN_08048000(1), não de C8B5) |
| E+0xCBA | 0x0300649A | 1 | modo do evento de mapa: 0 e 1 aceitam o mesmo conjunto; 2 só regrava entradas antigas com bit 7 | HC3 (grava 1 em 0x0300649A ao carregar mapa), EX | EX, Map_LoadEntitySpawnsForCurrentMap (HE3) | provável (EX, HC3) | aberto o significado exato de 2 |
| E+0xCBC | 0x0300649C | 1 (s8) | contador de evento (sel = 0): comparado no Op50 Jump_If_Event_Counter_Below | Event (contadores do evento, não achado) | Event_Op50 0x0804CBDC (decompile GH) | confirmado (leitura, decompile); escritor = aberto | |
| E+0xCBD | 0x0300649D | 1 (s8) | contador de evento (sel = 1): o outro operando do Op50 | Event (não achado) | Event_Op50 0x0804CBDC | confirmado (leitura); escritor = aberto | |

Notas da tabela 3:
- Os pontos "Pendente" do FI.md (E+0xC8C e E+0xC92) estão resolvidos: C8C = cursor salvo (decompile Op31), C92 = opcode (Script_DispatchHandler).
- E+0xCB5 ainda sem leitor. Procurar literal 0x03006495 ou ldr com offset 0xCB5 em Event_DispatchContinuation (0x0804DC0C) antes de dar como fechado.
- Escritor de E+0xC2D = 1 continua aberto (EN §7).

## Escritos nesta rodada (verificação)
- Decompile Ghidra (somente leitura, nada salvo): 0x0801D1AC, 0x08028078, 0x08021DAC, 0x08030A24, 0x0804C4B4.
- Literais lidos da ROM: 0x0801D1E0 = 0x03000440; 0x0801D1E4 = 0x03001800; 0x080280B0 = 0x03001800.

## Pendente
- Tabela 2: escritor de +0x089 (flag) e +0x080 (entrada da luta); campos +0x03C..+0x03F (tela de time, PS x PS2).
- Tabela 3: leitor de E+0xCB5; escritor de E+0xC2D = 1; significado exato de E+0xCBA = 2; escritores de E+0xCBC/CBD.
- Nome: 0x030004C9 (gDeterministicRandom em AI2 x gCtx_FreeFlag em FI_simbolos) não resolvido.
