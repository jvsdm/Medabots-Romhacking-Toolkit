# Action script 0x36..0x6A: handler -> despachante -> efeito (GH-AS3c)

Continuação de `AS2.md` (que deixou o efeito e as pendências 0x47/0x4A). Leitura na ROM original
`Medabots - Metabee (Europe).gba` (só leitura) e decompile no Ghidra (handlers já são funções:
`ActionScript_OpXX_...`, 0x08034524..0x08034E78).

## Como ler a tabela

- **handler**: função do opcode no Ghidra (nome confirmado). Wrapper = `push lr; bl Fight_DispatchOnSubstateAA_XX;
  se +0xB5 avança o roteiro`.
- **despachante**: `Fight_DispatchOnSubstateAA_XX` (0x0802xxxx). Decompile = `*(byte*)(fight+0xAA)` indexa uma tabela
  de ponteiros (`CallViaR0`). Cada entrada é um passo (função); o passo é um estado da animação ou efeito.
- **tabela**: endereço do ponteiro de tabela (literal do despachante, lido no ROM). Passos pelos nomes dos
  símbolos (`*_NN_` = índice; `Shared_` = passo comum; `Fight_DispatchOnSubstateAA_` omitido).
- **efeito**: o que o opcode faz na batalha. Confirmado = lido no código; provável = pelo nome/passos.
  Os passos vêm dos nomes no CSV e não foram todos decompilados (ver Pendente).

## Mapa do despachante (tabela de ponteiros pelo nome dos passos)

Passos com nome comum em quase todas as tabelas: `04/05/06/07 S_*` (carregar gráficos, passo de tabela, esperar
slots), `01 TrapFamily_Handler`, `02/13 StatusCheckFamily_Handler`, `Shared_*` (passo comum, p.ex. ajuste de
direção/espera). Os passos de efeito por opcode estão na tabela da seção seguinte.

## Tabela por opcode

| op | handler (Ghidra) | despachante | tabela | efeito na batalha | conf. |
|----|------------------|-------------|--------|-------------------|-------|
| 0x36 | ActionScript_Op36_CallAF 0x08034524 | Fight_DispatchOnSubstateAA_AF 0x08024998 | 0x083AE064 | animação de armadilha/oscilação de slot: acumula peça e checa técnica (`AccumPartAndCheckTech`), `TrapFamily`, `StatusCheckFamily`, carrega gráficos, `SetupOverlay`, slot de oscilação e contagem regressiva | provável (passos pelos nomes) |
| 0x37 | ActionScript_Op37_CallAG 0x08034540 | ..._AG 0x08024E04 | 0x083AE0C4 | igual a AF com deslize de slot: `SetupSlot`, `ScrollSlot`, `PlaySfx3`, `SlideSlot`, `SpawnEffect`, `WaitSlot`, `SetBotPart`, `SetStep3` | provável |
| 0x38 | ActionScript_Op38_CallAH 0x0803455C | ..._AH 0x0802518C | 0x083AE124 | formação/divisão: `AH_Split`, `AH_Draw`, `AH_Form`, esperas e acúmulo (AH_Accum). Ver `HA8.md` | confirmado (HA8) |
| 0x39 | ActionScript_Op39_CallAI 0x08034578 | ..._AI 0x080257C0 | 0x083AE184 | carregamento e setup (`AI_Load`, `AI_0B_S`, `AI_SetA`, `AI_12_S`) + oscilação/contagem; sem dado de batalha identificado | provável (só animação) |
| 0x3A | ActionScript_Op3A_CallAJ 0x08034594 | ..._AJ 0x08025AF8 | 0x083AE1E4 | troca/recuperação: `AJ_Swap`, `AJ_0C/0D_R*`, `AJ_0E_S`, `AJ_Rest` (restaura), `AJ_13/14_S` | provável |
| 0x3B | ActionScript_Op3B_CallAK 0x080345B0 | ..._AK 0x08025F2C | 0x083AE264 | tabela curta (4 passos): `AK_00_C` e passos comuns; efeito pequeno (ajuste de cena) | provável (pouco dado) |
| 0x3C | ActionScript_Op3C_CallAL 0x080345CC | ..._AL 0x08025FA0 | 0x083AE280 | tabela curta (4 passos): `AL_00_S`, `AL_Wait`, `AL_03_C` | provável (pouco dado) |
| 0x3D | ActionScript_Op3D_ClearSupportRecord 0x080345E8 | (inline, sem despachante) | - | zera os bytes +2..+5 do registro de suporte do bot ativo (`fight+0x2A + 8*fight[0x8A]`) e avança | confirmado (disasm, AS2) |
| 0x3E | ActionScript_Op3E_CallAM 0x08034628 | ..._AM 0x08026108 | 0x083AE298 | animação: `AM_00_A`, `AM_04_S`, `AM_Wait`, `AM_Step`, `AM_07/08_S`, `AM_0A_C`, `AM_0B_S` | provável |
| 0x3F | ActionScript_Op3F_CallAN_SceneExit 0x08034644 | ..._AN 0x0802645C | 0x083AE2EC | passo `AN_06` = `Fight_ApplyRandomPartReplacementEffect` (troca aleatória de peça); se +0xB5 e +0xAB != 1 grava cena 8 em 0x03002B8C e 0 em 0x0300137C (saída do combate) | confirmado (AS2 + nome do passo); cena 8 não validada no emulador |
| 0x40 | ActionScript_Op40_SideSpritesAO 0x08034684 | ..._AO 0x08026884 | 0x083AE340 | atualiza sprites laterais e sequência dos dois lados, depois passos `H_*`/`HealChange` (troca/cura de barra) e `Fight_QueueHeadBreakIfDefenderHead` (quebra de cabeça do defensor) | provável (nomes; HA8 para AH..AP) |
| 0x41 | ActionScript_Op41_PickRandomOpponentTargetPart 0x080346C4 | (inline) | - | escolhe alvo aleatório: lado adversário (`Fight_GetOpponentSideBaseIndex`) → bot ativo aleatório → peça aleatória → copia para o registro de alvo | confirmado (chamadas) |
| 0x42 | ActionScript_Op42_RollPartValueAndEffect 0x08034708 | (inline) | - | sorteia valor de peça (`Fight_RollPartValue` 0x0803769C) e calcula efeito pelo ROS (`Fight_ComputeEffectValueFromRos` 0x080376F4) | confirmado (nomes); detalhe provável |
| 0x43 | ActionScript_Op43_SumSideField4 0x08034728 | (inline) | - | laço de 3 bots do lado escolhido: soma o campo +4 (passo 0x18) das peças com +15 != 0 e estado 2/3, acumula em u16 | provável (alvo do acúmulo não identificado) |
| 0x44 | ActionScript_Op44_SideSpritesAP 0x0803476C | ..._AP 0x08026CF4 | 0x083AE3C0 | igual a 0x40 com a tabela AP: `H_00_Lo`, `H_0A_Se`, `H_0B_Ti`, `H_13_Ba`, `H_16_Se`, `H_17_Ru`, `AP_Appl`, `AP_18_L`, `AP_1B_L` (troca de HP/ peça e aplicação) | confirmado (chamadas) / passos provável; HA8 para AH..AP |
| 0x45 | ActionScript_Op45_IncrementUsageCounter 0x080347AC | (sem despachante) | - | `Fight_IncrementActionUsageCounter` 0x08036270 (contador de uso da ação) e avança | confirmado (nome + chamada) |
| 0x46 | ActionScript_Op46_SumPartDamageOfTargetBot 0x080347BC | (inline) | - | soma, sobre as 4 peças do bot indicado, `armadura máx − atual` (`+0` e `+0xD`) e acumula em u16 de fight | provável (disasm; leitura confirmada) |
| 0x47 | ActionScript_Op47_SideSpritesCopyTarget 0x080347F8 | (inline, sem despachante) | - | ver seção **0x47 e 0x4A** abaixo | **decodificado** (confiança provável nos campos) |
| 0x48 | ActionScript_Op48_SideSpritesAQ 0x080348D0 | ..._AQ 0x08026ED0 | 0x083AE480 | sprites laterais + passos `H_3E/3F/41/44/45` (troca de HP/ peça), `AQ_00_P`, `AQ_02_R`, `AQ_13_J` | provável |
| 0x49 | ActionScript_Op49_HalfArmorOfActiveBot 0x08034910 | (inline) | - | para as 4 peças do bot ativo (`fight+0x87`): `fight+0xD6+2k = armadura_k >> 1` e `fight+0xEC += armadura_k >> 1` (metade da armadura e soma) | confirmado (disasm, AS2) |
| 0x4A | ActionScript_Op4A_AllySideLoop 0x0803496C | (inline) | - | ver seção **0x47 e 0x4A** abaixo | **decodificado** (campo acumulado provável) |
| 0x4B | ActionScript_Op4B_FindBotWithPartAndPickPart 0x080349D8 | (inline) | - | como 0x41, mas procura bot que tenha a peça (`Fight_FindTargetBotWithPart` 0x08037798) antes de escolher peça; copia para o alvo | provável |
| 0x4C | ActionScript_Op4C_CopyPartArmorToEC 0x08034A68 | (inline) | - | `fight+0xEC = armadura da peça fight[0x8B] do bot fight[0x88]` (u16) | confirmado (disasm) |
| 0x4D | ActionScript_Op4D_RollDamageFromTwoDice 0x08034AA0 (renomeado; era Fight_RollDamageFromTwoDice) | (inline) | - | sorteia dois dados: `randbyte() & 0xF` indexa tabela de 16 valores ROM (DAT_08034AEC), grava em fight+0x188 e no vizinho (DAT_08034AF0), e `fight+0xEC = (d1 + d2) * 10` (u16). Ver seção 0x4D | confirmado (decompile) |
| 0x4E | ActionScript_Op4E_FindActiveBotWithGroup... 0x08034AF4 | (inline) | - | `Fight_GetOpponentSideBaseIndex` + `Fight_FindActiveBotInSideWithGroup` (0x08037830) + bot e peça aleatórios (como 0x41) | provável (nomes) |
| 0x4F | ActionScript_Op4F_FindActiveBotWithGroup 0x08034B68 | (inline) | - | `Fight_GetOpponentSideBaseIndex` + `Fight_FindActiveBotInSideWithGroup`; escolhe o bot (sem peça) | provável (nomes) |
| 0x50 | ActionScript_Op50_CallAR 0x08034BA0 | ..._AR 0x08026FD0 | 0x083AE500 | dano em peça aleatória dos adversários (`AR_Setup`, `AR_Anim`, `AR_Appl` aplica, `AR_Spaw`) | confirmado (HN2) |
| 0x51 | ActionScript_Op51_CallAS 0x08034BBC | ..._AS 0x0802793C | 0x083AE580 | estado por técnica no próprio bot (4..8): `AS_Sele`, `AS_Spin`, `AS_SetS` | confirmado (HN2) |
| 0x52 | ActionScript_Op52_CallAT 0x08034BD8 | ..._AT 0x08027F5C | 0x083AE5C0 | bônus de suporte do time: `AT_Grow`, `AT_Puls`, `AT_Appl`, `AT_SetS` | confirmado (HN2) |
| 0x53 | ActionScript_Op53_CallAU 0x08034BF4 | ..._AU 0x08028248 | 0x083AE600 | marca de reação no próprio bot: `AU_Slid`, `AU_Blin`, `AU_SetA`, `AU_SetS` | confirmado (HN2) |
| 0x54 | ActionScript_Op54_CallAV 0x08034C10 | ..._AV 0x08028404 | 0x083AE640 | estado em todos os adversários: `AV_Sele`, `AV_Appl`, `AV_09_C` | confirmado (HN2) |
| 0x55 | ActionScript_Op55_CallAW 0x08034C2C | ..._AW 0x08028C40 | 0x083AE680 | time: proteção contra status (passos `AW_00..0A`, `AW_07_F`, `AW_09_A`) | provável (nome e passos) |
| 0x56 | ActionScript_Op56_CallAX 0x08034C48 | ..._AX 0x080294B4 | 0x083AE6C0 | time: curar (`AX_00_S`, `AX_02_F`, `AX_07_S`, passos de espera) | provável |
| 0x57 | ActionScript_Op57_CallAY 0x08034C64 | ..._AY 0x08029840 | 0x083AE700 | time: reparar armadura (`AY_04..09`, `AY_06_A`, `AY_08_A`) | provável |
| 0x58 | ActionScript_Op58_CallAZ 0x08034C80 | ..._AZ 0x08029E0C | 0x083AE740 | time: restaurar peças (`AZ_00_S`, `AZ_02_F`, `AZ_06_R`, `AZ_09_S`) | provável |
| 0x59 | ActionScript_Op59_CallBA 0x08034C9C | ..._BA 0x0802A0E8 | 0x083AE780 | time: gauge (`BA_01_R`, `BA_05_F`, `BA_06_M`, `BA_08_A`, `BA_SetM`) | provável |
| 0x5A | ActionScript_Op5A_CallBB 0x08034CB8 | ..._BB 0x0802A6AC | 0x083AE7C0 | técnica sem dano: escala (`BB_Scal`), marca (`BB_Mark`), animação (`BB_Anim`), `BB_SetM` | provável |
| 0x5B | ActionScript_Op5B_CallBC 0x08034CD4 | ..._BC 0x0802AAD0 | 0x083AE800 | `BC_Load`, `W_05_Mo`, `W_06_Ru` (movimento/ rumble), `BC_AddV` (soma valor), `BC_SetM` | provável |
| 0x5C | ActionScript_Op5C_CallBD 0x08034CF0 | ..._BD 0x0802ACCC | 0x083AE840 | igual a BC com `BD_RunE`/`BD_RunS` e `BD_AddV` | provável |
| 0x5D | ActionScript_Op5D_CallBE 0x08034D0C | ..._BE 0x0802B000 | 0x083AE880 | sorteio de técnica (`BE_Roll`), tiles (`Z_TileC`), `BE_RunH`, `BE_Appl`, `BE_SetR` | provável |
| 0x5E | ActionScript_Op5E_CallBF 0x08034D28 | ..._BF 0x0802B7E8 | 0x083AE8D0 | `BF_Drop` (queda), `BF_Floa` (flutua), `BF_Copy`, `BF_Appl`, `BF_SetS` | provável |
| 0x5F | ActionScript_Op5F_CallBG 0x08034D44 | ..._BG 0x0802BD6C | 0x083AE910 | `BG_Flas` (flash), `BG_Move`, `BG_Squa`, `BG_Tran` (transição), `BG_SetS` | provável |
| 0x60 | ActionScript_Op60_CallBH 0x08034D60 | ..._BH 0x0802C2AC | 0x083AE954 | `BH_Spaw`, `BH_RunS`, `BH_Blin`, `BH_Boos` (impulso), `BH_SetS` | provável |
| 0x61 | ActionScript_Op61_CallBI 0x08034D7C | ..._BI 0x0802C644 | 0x083AE994 | `BH_Init/Grow/Shri/Mark/Spaw/Move`, `Fight_CommitBoostChargeSecondaryEffect` (carga de boost) | provável |
| 0x62 | ActionScript_Op62_CallBJ 0x08034D98 | ..._BJ 0x0802CBE8 | 0x083AE9D4 | `BH_Rais`, `BH_Deta`, `BH_Drop`, `BH_Anim`, `BH_Comm` (commit) | provável |
| 0x63 | ActionScript_Op63_CallBK 0x08034DB4 | ..._BK 0x0802D688 | 0x083AEA30 | `BH_Init/Grow/Shri/Spaw/Move`, `BK_0B..0F_S` (passos finais) | provável |
| 0x64 | ActionScript_Op64_CallBL 0x08034DD0 | ..._BL 0x0802DCB4 | 0x083AEA88 | `BL_00_L`, `Z_05_Fl`, `Z_TickC`, `BL_06/07_B`, `BL_0B_M`, `BL_12..14_D/A` (efeito de tela/ borda) | provável |
| 0x65 | ActionScript_Op65_CallBM 0x08034DEC | ..._BM 0x0802E424 | 0x083AEB1C | `BM_00_M`, `BM_02_G`, `BM_05_R`, `BM_08..0C`, `BM_0F_D`, `AR_Show/Anim/Spaw` | provável |
| 0x66 | ActionScript_Op66_CallBN 0x08034E08 | ..._BN 0x0802EBC0 | 0x083AEB88 | `BN_00_M`, `BN_03_R`, `BN_05_W`, `BN_07..09_S` | provável |
| 0x67 | ActionScript_Op67_CallBO 0x08034E24 | ..._BO 0x0802F030 | 0x083AEBC4 | `BO_00..09` (S/A/T/M/R), `BO_0A_S` | provável |
| 0x68 | ActionScript_Op68_CallBP 0x08034E40 | ..._BP 0x0802F628 | 0x083AEC04 | `BP_00_M`, `BP_04_P`, `BP_06_B`, `BP_08_A`, `AR_Show/Anim/Spaw` | provável |
| 0x69 | ActionScript_Op69_CallBQ 0x08034E5C | ..._BQ 0x0802F990 | 0x083AEC58 | `BQ_00..0A` (L/S/G/S...) | provável |
| 0x6A | ActionScript_Op6A_CallBR 0x08034E78 | ..._BR 0x0802FE50 | 0x083AEC98 | `BR_00..08` (L/S/W/J/B) e `BR_10..14_A` (animação final) | confirmado (wrapper); cauda da tabela truncada na leitura |

## 0x47 e 0x4A (decodificados neste lote)

**0x47 `ActionScript_Op47_SideSpritesCopyTarget` (0x080347F8)**, sem despachante:
1. `FightPreview_UpdateSingleLayerSideSprites`, depois avança os dois cursores de sequência lateral
   (`FightPreview_AdvanceLeftSideSequenceStep` grava em fight+`DAT_08034868`; `...Right...` grava em fight+0x160).
2. `fight+0xF6 = 3`.
3. Soma 1 no byte do bot ativo `fight+0x87` em `bot*0x100 + DAT_0803486C + 0xE0` (contador por bot; campo exato
   não confirmado).
4. `fight+DAT_08034870 = fight+0x88` e `Fight_CopySelectedTargetToFightInfo(fight+0x87)`.
5. Inverte o bit 2 de `fight+0xF8`. Se ficou 0: `fight+0x114 = 0`. Se ficou 1: `fight+0x114 = 0x28` e `fight+0xFF |= 1`.
6. Lado oposto (base `DAT_080348B0`): se o bit 2 de +0xF8 estiver 0, grava 0x28 em `+DAT_080348B4` e `|= 2` em
   +0xFF; senão zera `+DAT_080348CC`.
7. `Fight_AdvanceActionScriptCursor`.
Efeito: um passo da sequência dos dois lados, copia o alvo e marca um timer de 0x28 quadros (provável) por lado,
alternando o bit 2 a cada execução.

**0x4A `ActionScript_Op4A_AllySideLoop` (0x0803496C)**:
1. `fight+0x60 = Fight_GetAllySideBaseIndex(fight+0x87)` (índice do primeiro bot do lado aliado).
2. Laço de 3 bots (`k` = 0..2, bot = base + k). Para cada bot com byte `+0xBA` diferente de 0 (bot ativo), percorre
   as 4 peças (`0x59 + 0x18*j` no bloco do bot, `bloco = 0x03000590 + bot*0x100`): se o byte +0 da peça for 0,
   soma 10 no u16 `fight+0x8C`.
3. `Fight_AdvanceActionScriptCursor`.
Efeito: acumula 10 por peça "vazia" (+0 == 0) dos bots aliados ativos em `fight+0x8C`. Confiança: provável no
significado de +0 (armadura máxima) e no uso de `fight+0x8C` (nome do destino não identificado).

**0x4D `ActionScript_Op4D_RollDamageFromTwoDice` (0x08034AA0)** (renomeado no Ghidra): dois sorteios
(`randbyte() & 0xF`) indexam uma tabela de 16 bytes (`DAT_08034AEC`); o primeiro vai para `fight+0x188`, o segundo
para o vizinho, e `fight+0xEC = (d1 + d2) * 10`. Confirmado no decompile.

## Pendente

- Passos de AF, AG, AM..AQ, AW..BR: só o nome do passo (CSV) foi usado; o corpo de cada passo (efeito exato)
  não foi decompilado neste lote. AH..AP: ver `HA8.md`; AR..AV: ver `HN2.md`.
- Campo exato do contador por bot em 0x47 (`bot*0x100 + DAT_0803486C + 0xE0`) e alvo do acúmulo de 0x43.
- Tabelas de passo: o tamanho real de cada uma não foi confirmado (a leitura pegou entradas até a próxima tabela,
  e algumas trazem passos comuns como `ObjScript_Op00_Stub`).
- Cena 8 em 0x3F: não validada no emulador.
- Despachantes sem passo de dado documentado: AI, AK, AL (poucas entradas).
- Pendências do Ghidra: nenhuma função nova criada; só 0x08034AA0 foi renomeada.
