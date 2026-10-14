# TQ2: roteiros de action-script das técnicas 0..59 (GH-TQ2, 2026-10-14)

Leitura: ROM original `Medabots - Metabee (Europe).gba` (só leitura, `python -I`), decompile e disasm do Ghidra MCP
(sem salvar, sem fechar). Ponteiros: `gTechniqueActionScriptPtrTable` 0x083C6CA8 (4 B por técnica).
Fluxo completo anotado (todas as linhas, com handler e rótulos): `re/functions/TQ2_fluxo.txt`. Símbolos: `TQ2_simbolos.csv`.

## 1. Resposta ao conflito (20 técnicas no mesmo início 0x083C6A04)

- **Não há desvio no roteiro que separe Sword (0) de Missile (6).** Os ponteiros de 0..9, 13..20, 34 e 35 apontam para
  0x083C6A04, e esse é um ponto do mesmo fluxo linear que vai até 0x083C6CA5; o byte 0x00 em 0x083C6CA6 é o fim
  (confirmado pela caminhada linear: termina exatamente aí). As entradas das outras técnicas (10..12, 21..33, 36..59)
  são pontos dentro desse mesmo fluxo. Logo Sword e Missile executam a mesma sequência de opcodes e os mesmos pontos de escolha.
- **O único desvio é o opcode 0x03 (escolha) e o 0x02 (salto), e o índice da escolha é `fight+0xAB`, não a técnica.** (Significado de 0 = normal: provável, por `HA1.md`.)
  `Fight_WaitForActionScriptChoiceByteMatch` 0x08033974 (decompile): lê 4 argumentos inline, avança o cursor e varre o
  fluxo para a frente até achar o rótulo `args[fight+0xAB]`; o cursor fica depois dele. `fight+0xAB` é o código de
  resultado gravado pelos handlers de fim de ação (0 = normal, 1 e 2 = resultados alternativos; `HA1.md`, `HA1_simbolos.csv`).
- **Consequência:** a diferença entre Sword e Missile NÃO está no roteiro. Ela deve estar em: alvo do setup
  (`gTechniqueActionSetupHandlerTable`, `TQ.md` §3), efeito pela peça/técnica (`DM.md`, peça +1 = id da técnica) e,
  possivelmente, em handlers que leiam `F+0xA8` (id da técnica). **Não verificado:** nenhum handler desta rodada foi
  checado para ler `F+0xA8` (pendente, ver §6).
- Efeitos só da técnica 6 (explosão, míssil, faísca; `pc/research/README.md`) são de animação/motion, não de roteiro.

## 2. Mecanismo do leitor (confirmado hoje no decompile)

- `Fight_AdvanceActionScriptCursor` 0x08034F08: `F+0xBC += 1 + LEN[op]`, com `LEN` = `0x083C6EB8` (literal confirmado
  em 0x08034F40 = 0x083C6EB8). Zera `F+0xAA`.
- `Fight_ReadActionScriptInlineArgs` 0x08034EB8: copia `LEN[op]` bytes após o opcode para `F+0xAC..`.
- Rótulos (opcodes 0x80..0x8A) têm LEN 0: no laço principal são pulados com o avanço (`AS1.md`); só servem de alvo
  de 0x02/0x03.
- **Correção ao `DM.md` §5:** a tabela real dá LEN[0x03] = 4 (não 1) e LEN[0x13] = 0 (não 1). O 0x03 tem 4 rótulos
  inline, e o 0x13 (`Fight_SeedPreDamageFromTotalRos`) não tem argumento. `DM.md` linha 106 deve ser corrigida.
- LEN[0x02] = 1 (um rótulo). O 0x02 não aparece no fluxo de nenhuma técnica 0..59 (só poderia aparecer em medaforça).

## 3. Entradas por técnica (ponteiro da tabela 0x083C6CA8)

Todas as entradas são pontos do mesmo fluxo; "linha de" aponta para o endereço do primeiro opcode no fluxo.

| id | técnica | ptr roteiro | entrada no fluxo | compartilha entrada com |
|---|---|---|---|---|
| 0 | Sword | 0x083C6A04 | 0x083C6A04 | 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 1 | Hammer | 0x083C6A04 | 0x083C6A04 | 0, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 2 | Rifle | 0x083C6A04 | 0x083C6A04 | 0, 1, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 3 | ChainGun | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 4 | Laser | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 5 | Beam | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 6 | Missile | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 7 | Napalm | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 8 | Break | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 9 | Press | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 13, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 10 | GrapTrap | 0x083C6A57 | 0x083C6A57 | - |
| 11 | ShotTrap | 0x083C6A59 | 0x083C6A59 | - |
| 12 | AttackClr | 0x083C6A5B | 0x083C6A5B | - |
| 13 | Bug | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 14, 15, 16, 17, 18, 19, 20, 34, 35 |
| 14 | Virus | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 15, 16, 17, 18, 19, 20, 34, 35 |
| 15 | Thunder | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 16, 17, 18, 19, 20, 34, 35 |
| 16 | Freeze | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 17, 18, 19, 20, 34, 35 |
| 17 | Wave | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 18, 19, 20, 34, 35 |
| 18 | Hold | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 19, 20, 34, 35 |
| 19 | Fire | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 20, 34, 35 |
| 20 | Melt | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 34, 35 |
| 21 | BreakForm | 0x083C6A65 | 0x083C6A65 | - |
| 22 | Stability | 0x083C6A67 | 0x083C6A67 | - |
| 23 | ForceBind | 0x083C6A6B | 0x083C6A6B | - |
| 24 | CancelOptic | 0x083C6A6D | 0x083C6A6D | - |
| 25 | CancelBomb | 0x083C6A6E | 0x083C6A6E | - |
| 26 | CancelGrav | 0x083C6A6F | 0x083C6A6F | - |
| 27 | Defence | 0x083C6A70 | 0x083C6A70 | - |
| 28 | FullBlock | 0x083C6A72 | 0x083C6A72 | - |
| 29 | HalfBlock | 0x083C6A74 | 0x083C6A74 | - |
| 30 | Recovery | 0x083C6A76 | 0x083C6A76 | - |
| 31 | AutoRecover | 0x083C6A87 | 0x083C6A87 | - |
| 32 | Repair | 0x083C6A8B | 0x083C6A8B | - |
| 33 | Reactivate | 0x083C6A9C | 0x083C6A9C | - |
| 34 | AntiAir | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 35 |
| 35 | AntiSea | 0x083C6A04 | 0x083C6A04 | 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 14, 15, 16, 17, 18, 19, 20, 34 |
| 36 | Scout | 0x083C6AA2 | 0x083C6AA2 | - |
| 37 | Conceal | 0x083C6AA4 | 0x083C6AA4 | - |
| 38 | BoostChrg | 0x083C6AA6 | 0x083C6AA6 | - |
| 39 | RapidChrg | 0x083C6AAA | 0x083C6AAA | - |
| 40 | AccelChrg | 0x083C6AAE | 0x083C6AAE | - |
| 41 | DrainChrg | 0x083C6AAF | 0x083C6AAF | - |
| 42 | Pushover | 0x083C6AB3 | 0x083C6AB3 | - |
| 43 | Impair | 0x083C6AB8 | 0x083C6AB8 | - |
| 44 | Confusion | 0x083C6ABC | 0x083C6ABC | - |
| 45 | UseDrain | 0x083C6AC0 | 0x083C6AC0 | - |
| 46 | NoEscape | 0x083C6AC4 | 0x083C6AC4 | - |
| 47 | NoDefense | 0x083C6AC8 | 0x083C6AC8 | - |
| 48 | Destroy | 0x083C6ACC | 0x083C6ACC | - |
| 49 | Sacrifice | 0x083C6AFF | 0x083C6AFF | - |
| 50 | Forcedrain | 0x083C6B2C | 0x083C6B2C | - |
| 51 | TeamForm | 0x083C6B30 | 0x083C6B30 | - |
| 52 | TeamAttack | 0x083C6B32 | 0x083C6B32 | - |
| 53 | Strengthen | 0x083C6B85 | 0x083C6B85 | - |
| 54 | CntrAttk | 0x083C6B86 | 0x083C6B86 | - |
| 55 | Change | 0x083C6B88 | 0x083C6B88 | - |
| 56 | ItrpChange | 0x083C6B8A | 0x083C6B8A | - |
| 57 | AttkChange | 0x083C6B8C | 0x083C6B8C | - |
| 58 | DEFChange | 0x083C6B8E | 0x083C6B8E | - |
| 59 | HealChange | 0x083C6B90 | 0x083C6B90 | - |

## 3b. Pontos de escolha (opcode 0x03): alvos por índice fight+0xAB

| endereço do 03 | args (rótulos) | alvo [0] (+0xAB=0) | alvo [1] | alvo [2] | alvo [3] |
|---|---|---|---|---|---|
| 83C6A05 | 80 81 81 81 | 83C6A0A | 83C6A4B | 83C6A4B | 83C6A4B |
| 83C6A0C | 82 81 81 81 | 83C6A11 | 83C6A4B | 83C6A4B | 83C6A4B |
| 83C6A14 | 83 81 81 81 | 83C6A19 | 83C6A4B | 83C6A4B | 83C6A4B |
| 83C6A1F | 84 85 85 85 | 83C6A24 | 83C6A2E | 83C6A2E | 83C6A2E |
| 83C6A28 | 86 87 87 87 | 83C6A2D | 83C6A37 | 83C6A37 | 83C6A37 |
| 83C6A3D | 88 89 89 89 | 83C6A45 | 83C6A42 | 83C6A42 | 83C6A42 |
| 83C6A79 | 80 81 81 81 | 83C6A7E | 83C6A85 | 83C6A85 | 83C6A85 |
| 83C6A8E | 80 81 81 81 | 83C6A93 | 83C6A9A | 83C6A9A | 83C6A9A |
| 83C6ACD | 82 81 81 81 | 83C6AD2 | 83C6AFC | 83C6AFC | 83C6AFC |
| 83C6AD5 | 83 81 81 81 | 83C6ADA | 83C6AFC | 83C6AFC | 83C6AFC |
| 83C6ADD | 88 81 81 81 | 83C6AE2 | 83C6AFC | 83C6AFC | 83C6AFC |
| 83C6AE8 | 86 87 87 87 | 83C6AED | 83C6AF3 | 83C6AF3 | 83C6AF3 |
| 83C6B00 | 82 81 81 81 | 83C6B05 | 83C6B29 | 83C6B29 | 83C6B29 |
| 83C6B08 | 83 81 81 81 | 83C6B0D | 83C6B29 | 83C6B29 | 83C6B29 |
| 83C6B14 | 86 87 87 87 | 83C6B19 | 83C6B1F | 83C6B1F | 83C6B1F |
| 83C6B33 | 80 81 81 81 | 83C6B38 | 83C6B82 | 83C6B82 | 83C6B82 |
| 83C6B3A | 82 81 81 81 | 83C6B3F | 83C6B82 | 83C6B82 | 83C6B82 |
| 83C6B41 | 8A 81 81 81 | 83C6B46 | 83C6B82 | 83C6B82 | 83C6B82 |
| 83C6B49 | 83 81 81 81 | 83C6B4E | 83C6B82 | 83C6B82 | 83C6B82 |
| 83C6B55 | 84 85 85 85 | 83C6B5A | 83C6B64 | 83C6B64 | 83C6B64 |
| 83C6B5E | 86 87 87 87 | 83C6B63 | 83C6B6D | 83C6B6D | 83C6B6D |
| 83C6B73 | 88 89 89 89 | 83C6B7B | 83C6B78 | 83C6B78 | 83C6B78 |

## 3c. Opcodes usados no fluxo 0..59 (handler e nome)

| op | handler (endereço) | nome |
|---|---|---|
| 01 | 0x080338DC | ActionScript_Op01_pendente |
| 03 | 0x08033974 | Fight_WaitForActionScriptChoiceByteMatch |
| 04 | 0x080339F4 | ActionScript_Op04_pendente |
| 05 | 0x08033A10 | ActionScript_Op05_pendente |
| 06 | 0x08033A2C | Fight_EnsureSelectedActionMetadata |
| 07 | 0x08033A78 | FUN_00000000 |
| 08 | 0x08033A94 | FUN_00000000 |
| 09 | 0x08033AB0 | FUN_00000000 |
| 0A | 0x08033ACC | Fight_CommitSelectedReferencedBotGraphics |
| 0B | 0x08033B7C | Fight_ApplySpecialityDamageFlags |
| 0C | 0x08033C00 | Fight_ComputeTotalRosPhase |
| 0D | 0x08033C10 | FUN_00000000 |
| 0E | 0x08033C20 | Fight_ApplyPendingEffectStatusFlags |
| 0F | 0x08033CD0 | Fight_ResolveDefenderRosPhase |
| 10 | 0x08033CE0 | Fight_UpdateActionArgSelectorPhase |
| 11 | 0x08033CF0 | FUN_00000000 |
| 12 | 0x08033D00 | FUN_00000000 |
| 13 | 0x08033D10 | Fight_SeedPreDamageFromTotalRos |
| 14 | 0x08033D30 | Fight_AccumulateBaseDamage |
| 15 | 0x08033F04 | Fight_HandleActionScriptOpcode1D?(nome Ghidra, tabela=0x15) |
| 16 | 0x08033F64 | Fight_HandleActionScriptOpcode1C?(nome Ghidra, tabela=0x16) |
| 17 | 0x08033F9C | Fight_MarkDefenderFollowupFromDamageThreshold |
| 18 | 0x08034008 | FUN_00000000 |
| 19 | 0x0803402C | FUN_00000000 |
| 1A | 0x08034048 | FightPreview_SetupSceneFromPartTechnique |
| 1B | 0x08034074 | FUN_00000000 |
| 1C | 0x08034120 | FUN_00000000 |
| 1D | 0x08034170 | FUN_00000000 |
| 1E | 0x080341C0 | FUN_00000000 |
| 1F | 0x08034200 | FUN_00000000 |
| 20 | 0x08034240 | FUN_00000000 |
| 21 | 0x0803426C | FUN_00000000 |
| 22 | 0x08034288 | FUN_00000000 |
| 23 | 0x080342C8 | FUN_00000000 |
| 24 | 0x080342E4 | FUN_00000000 |
| 25 | 0x08034300 | FUN_00000000 |
| 26 | 0x0803431C | FUN_00000000 |
| 27 | 0x0803435C | FUN_00000000 |
| 28 | 0x08034378 | FUN_00000000 |
| 29 | 0x080343B8 | FUN_00000000 |
| 2A | 0x080343D4 | FUN_00000000 |
| 2B | 0x080343F0 | FUN_00000000 |
| 2C | 0x0803440C | FUN_00000000 |
| 2D | 0x08034428 | FUN_00000000 |
| 2E | 0x08034444 | FUN_00000000 |
| 2F | 0x08034460 | FUN_00000000 |
| 30 | 0x0803447C | FUN_00000000 |
| 31 | 0x08034498 | FUN_00000000 |
| 32 | 0x080344B4 | FUN_00000000 |
| 33 | 0x080344D0 | FUN_00000000 |
| 34 | 0x080344EC | FUN_00000000 |
| 35 | 0x08034508 | FUN_00000000 |
| 36 | 0x08034524 | ActionScript_Op36_CallAF |
| 37 | 0x08034540 | ActionScript_Op37_CallAG |
| 38 | 0x0803455C | ActionScript_Op38_CallAH |
| 39 | 0x08034578 | ActionScript_Op39_CallAI |
| 3A | 0x08034594 | ActionScript_Op3A_CallAJ |
| 3B | 0x080345B0 | ActionScript_Op3B_CallAK |
| 3C | 0x080345CC | ActionScript_Op3C_CallAL |
| 3D | 0x080345E8 | ActionScript_Op3D_ClearSupportRecord |
| 3E | 0x08034628 | ActionScript_Op3E_CallAM |
| 3F | 0x08034644 | ActionScript_Op3F_CallAN_SceneExit |
| 40 | 0x08034684 | ActionScript_Op40_SideSpritesAO |
| 41 | 0x080346C4 | ActionScript_Op41_PickRandomOpponentTargetPart |
| 42 | 0x08034708 | ActionScript_Op42_RollPartValueAndEffect |
| 43 | 0x08034728 | ActionScript_Op43_SumSideField4 |
| 44 | 0x0803476C | ActionScript_Op44_SideSpritesAP |
| 45 | 0x080347AC | ActionScript_Op45_IncrementUsageCounter |
| 46 | 0x080347BC | ActionScript_Op46_SumPartDamageOfTargetBot |
| 47 | 0x080347F8 | ActionScript_Op47_SideSpritesCopyTarget |
| 48 | 0x080348D0 | ActionScript_Op48_SideSpritesAQ |
| 49 | 0x08034910 | ActionScript_Op49_HalfArmorOfActiveBot |
| 4A | 0x0803496C | ActionScript_Op4A_AllySideLoop |
| 4B | 0x080349D8 | ActionScript_Op4B_FindBotWithPartAndPickPart |
| 4C | 0x08034A68 | ActionScript_Op4C_CopyPartArmorToEC |
| 4D | 0x08034AA0 | ActionScript_Op4D_RandomChance |
| 4E | 0x08034AF4 | ActionScript_Op4E_FindActiveBotWithGroupAndPickPart |
| 4F | 0x08034B68 | ActionScript_Op4F_FindActiveBotWithGroup |
| 50 | 0x08034BA0 | ActionScript_Op50_CallAR |
| 51 | 0x08034BBC | ActionScript_Op51_CallAS |
| 52 | 0x08034BD8 | ActionScript_Op52_CallAT |
| 53 | 0x08034BF4 | ActionScript_Op53_CallAU |
| 54 | 0x08034C10 | ActionScript_Op54_CallAV |
| 55 | 0x08034C2C | ActionScript_Op55_CallAW |
| 56 | 0x08034C48 | ActionScript_Op56_CallAX |
| 57 | 0x08034C64 | ActionScript_Op57_CallAY |
| 58 | 0x08034C80 | ActionScript_Op58_CallAZ |
| 59 | 0x08034C9C | ActionScript_Op59_CallBA |
| 5A | 0x08034CB8 | ActionScript_Op5A_CallBB |
| 5B | 0x08034CD4 | ActionScript_Op5B_CallBC |
| 5C | 0x08034CF0 | ActionScript_Op5C_CallBD |
| 5D | 0x08034D0C | ActionScript_Op5D_CallBE |
| 5E | 0x08034D28 | ActionScript_Op5E_CallBF |
| 5F | 0x08034D44 | ActionScript_Op5F_CallBG |
| 60 | 0x08034D60 | ActionScript_Op60_CallBH |
| 61 | 0x08034D7C | ActionScript_Op61_CallBI |
| 62 | 0x08034D98 | ActionScript_Op62_CallBJ |
| 63 | 0x08034DB4 | ActionScript_Op63_CallBK |
| 64 | 0x08034DD0 | ActionScript_Op64_CallBL |
| 65 | 0x08034DEC | ActionScript_Op65_CallBM |
| 66 | 0x08034E08 | ActionScript_Op66_CallBN |
| 67 | 0x08034E24 | ActionScript_Op67_CallBO |
| 68 | 0x08034E40 | ActionScript_Op68_CallBP |
| 69 | 0x08034E5C | ActionScript_Op69_CallBQ |
| 6A | 0x08034E78 | ActionScript_Op6A_CallBR |

Rótulos (>=0x80) usados: 80 81 82 83 84 85 86 87 88 89 8A


Ponteiros 60..79 (também 0x083C6A04) não são ids de técnica (a faixa 0..59 é a de técnicas; os 80..95 são de medaforça,
ver `MF.md`); seus fluxos também caem neste mesmo fluxo linear.

## 4. Handlers dos opcodes de desvio (Ghidra, agora funções)

| opcode | handler | estado | observação |
|---|---|---|---|
| 0x02 | 0x080338F4 Fight_WaitForActionScriptByteMatch | função, decompile lido (`AS1.md`) | salto para a frente ao rótulo arg[0] |
| 0x03 | 0x08033974 Fight_WaitForActionScriptChoiceByteMatch | função, decompile lido hoje | escolha de 4 rótulos por `fight+0xAB`; LEN 4 |
| (leitor) | 0x08034EB8 Fight_ReadActionScriptInlineArgs | decompile lido hoje | copia LEN[op] bytes para `fight+0xAC` |
| (cursor) | 0x08034F08 Fight_AdvanceActionScriptCursor | decompile lido hoje | `F+0xBC += 1 + LEN[op]` |

Nomes de handler conferidos hoje com `get_function_by_address`: 0x01, 0x04, 0x05 são placeholders
`ActionScript_Op0x_pendente` (função existe). Os handlers 0x06..0x35 e 0x3F..0x69 têm função sem nome útil ou
wrapper (ver tabela §3b/§5). **Conflito de nome:** o Ghidra chama 0x08033F04 de `Fight_HandleActionScriptOpcode1D` e
0x08033F64 de `..._1C`, mas a tabela 0x083AF180 põe esses handlers em 0x15 e 0x16. Conferir antes de usar o nome.

## 5. Pendências

- **Não verificado:** se algum handler de opcode (0x06..0x6A) lê `F+0xA8` (id da técnica). Se nenhum ler, o fluxo
  não separa técnicas; a diferença é de setup e de efeito. Conferir 0x41..0x4F e 0x50..0x6A (wrappers AS2) antes de fechar.
- Execução real dos desvios (qual `fight+0xAB` sai em cada técnica) não foi medida no emulador (fora deste pedido).
- Medaforça (80..95) não foi tabulada aqui.
- Opcodes 0x2A..0x35 e 0x36..0x6A: nomes ainda em AS2 (wrapper), sem decompile nesta rodada.
- Nenhuma função nova foi criada, nenhum nome do Ghidra foi alterado. Ghidra aberto, não salvo.
