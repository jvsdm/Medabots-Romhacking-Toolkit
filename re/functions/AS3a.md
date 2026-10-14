# AS3a: opcodes do roteiro de ação 0x01..0x1A (Medabots Metabee EU) — GH-AS3a

Tabela `gActionScriptOpcodeHandlerTable` 0x083AF180 (4 B por entrada, thumb). Ponteiros lidos da ROM original com `python -I`
(só leitura). Função = ponteiro − 1. Todos os 26 handlers já têm função no Ghidra; os de nome FUN_ e `_pendente` foram
renomeados (ver "Renomeações"). Os nomes `Fight_*` que já eram descritivos foram mantidos; o alias `ActionScript_OpXX` fica no CSV.

## Convenções usadas nas colunas
- **LEN**: byte de `0x083C6EB8[op]` (comprimento de argumento; o cursor avança `1 + LEN`, ver DM.md §5).
- **Passo**: se o handler encerra o passo (+0xB5 = 1, o laço do roteiro para). Handler que só avança o cursor sem setar +0xB5
  não encerra: o laço segue no mesmo quadro lendo o próximo opcode.
- **Cursor**: `Fight_AdvanceActionScriptCursor` 0x08034F08 (chamado por todos os handlers, exceto 0x01).

## Tabela

| op | handler | Ghidra (nome atual) | LEN | encerra passo? | efeito | confiança |
|----|---------|---------------------|-----|----------------|--------|-----------|
| 0x01 | 0x080338DC | ActionScript_Op01_SetSceneExitState | 0 | não (sem +0xB5, sem avanço; o laço repete o mesmo opcode até sair pelo atalho/cena) | grava 8 em 0x03002B8C (cena) e 0x40 em 0x0300137C | provável (mesma cena 8 do 0x3F em AS2) |
| 0x02 | 0x080338F4 | Fight_WaitForActionScriptByteMatch | 1 | sim, ao casar | espera por byte do roteiro (AS1) | provável |
| 0x03 | 0x08033974 | Fight_WaitForActionScriptChoiceByteMatch | 4 | sim, ao casar | igual 0x02, com índice +0xAB | provável |
| 0x04 | 0x080339F4 | ActionScript_Op04_DispatchOnSubstateAA | 0 | só se o despachante setar +0xB5 | chama Fight_DispatchOnSubstateAA; efeito no despachante não lido | confirmado (wrapper), efeito pendente |
| 0x05 | 0x08033A10 | ActionScript_Op05_DispatchOnSubstateAA_B | 0 | só se o despachante setar +0xB5 | chama `..._B`; efeito não lido | confirmado (wrapper), efeito pendente |
| 0x06 | 0x08033A2C | Fight_EnsureSelectedActionMetadata | 0 | não | se o alvo não tem bot, escolhe o de menor prioridade; se não tem peça, sorteia; copia alvo p/ fight; avança | confirmado |
| 0x07 | 0x08033A78 | ActionScript_Op07_DispatchOnSubstateAA_C | 0 | só se o despachante setar +0xB5 | chama `..._C`; efeito não lido | confirmado (wrapper), efeito pendente |
| 0x08 | 0x08033A94 | ActionScript_Op08_DispatchOnSubstateAA_D | 0 | só se o despachante setar +0xB5 | chama `..._D`; efeito não lido | confirmado (wrapper), efeito pendente |
| 0x09 | 0x08033AB0 | ActionScript_Op09_DispatchOnSubstateAA_E | 0 | só se o despachante setar +0xB5 | chama `..._E`; efeito não lido | confirmado (wrapper), efeito pendente |
| 0x0A | 0x08033ACC | Fight_CommitSelectedReferencedBotGraphics | 0 | não | se fight+0xDE != 0: copia 4×64 B de gráficos do bot referenciado (base 0x03000590 + bot×0x100, +0xE40..+0xF00), limpa atributos de objeto, seta slot, refresca sprites do bot ativo, zera +0xDE | confirmado |
| 0x0B | 0x08033B7C | Fight_ApplySpecialityDamageFlags | 0 | não | pelo estado (+0x4A) da peça fight+0xA7 do bot fight+0x87, marca bit 2 ou 1 em +0xBC do defensor | confirmado (leitura), sentido provável |
| 0x0C | 0x08033C00 | Fight_ComputeTotalRosPhase | 0 | não | `Fight_ComputeTotalRos()`; avança | confirmado (wrapper) |
| 0x0D | 0x08033C10 | ActionScript_Op0D_HandleDefenderFollowupFlags | 0 | não | `Fight_HandleDefenderFollowupFlags()`; avança | confirmado (wrapper) |
| 0x0E | 0x08033C20 | Fight_ApplyPendingEffectStatusFlags | 0 | não | efeito 0x2E/0x2F/0x0F/0x10 do alvo → bits 4/8/C em +0xBC (DM.md §6.1) | confirmado |
| 0x0F | 0x08033CD0 | Fight_ResolveDefenderRosPhase | 0 | não | `Fight_ResolveDefenderRosReductions()` (DM.md §6.2); avança | confirmado |
| 0x10 | 0x08033CE0 | Fight_UpdateActionArgSelectorPhase | 0 | não | `Fight_UpdateActionArgSelectorFromDefenderState()`; avança | confirmado (wrapper) |
| 0x11 | 0x08033CF0 | ActionScript_Op11_SetAttackerAffinityResult | 0 | não | `Fight_SetAttackerAffinityResultFromSelectedAction()`; avança | confirmado (wrapper) |
| 0x12 | 0x08033D00 | ActionScript_Op12_SetDefenderAffinityResult | 0 | não | `Fight_SetDefenderAffinityResultFromSelectedAction()`; avança | confirmado (wrapper) |
| 0x13 | 0x08033D10 | Fight_SeedPreDamageFromTotalRos | 0 | não | fight+0xEA = fight+0xE8 >> 2; avança | confirmado |
| 0x14 | 0x08033D30 | Fight_AccumulateBaseDamage | 0 | não | monta dano base em fight+0xEC: +0xEA + poder da peça (+0x4E) + bônus por estado/flags da peça (+0xD0, +0xD3, +0xD4, +0xD6, +0xD8); subtrai +0xD1 do alvo; avança | confirmado (leitura); sentido de cada termo provável |
| 0x15 | 0x08033F04 | ActionScript_Op15_MarkStateFromDamageBelowThreshold | 0 | não | se fight+0xDE != 0, +0xDF == 0x1D, +0xB8 == 0 e fight+0xEC < alvo+0xD8: +0xB8 = 2, +0xBE = 1; avança | confirmado (leitura) |
| 0x16 | 0x08033F64 | ActionScript_Op16_SetStateFromPartFlag1C | 0 | não | se fight+0xDE != 0, +0xDF == 0x1C, +0xB8 == 0: +0xB8 = 2, +0xBE = 5; avança | confirmado (leitura) |
| 0x17 | 0x08033F9C | Fight_MarkDefenderFollowupFromDamageThreshold | 0 | não | se alvo+0xBD tem 0x10 e a peça do alvo está ativa, fight+0xB7 == 0 e alvo+0xD8 < fight+0xEC: fight+0xB7 = 3; avança | confirmado (leitura) |
| 0x18 | 0x08034008 | ActionScript_Op18_AddRandomBonusAndScaleByPart | 0 | não | `Fight_AddRandomBonusUpTo3()`; `Fight_ScaleDamageByPartScalar(bot, peça)`; avança | confirmado (chamadas) |
| 0x19 | 0x0803402C | ActionScript_Op19_DispatchOnSubstateAA_F | 0 | só se o despachante setar +0xB5 | chama `..._F`; efeito não lido | confirmado (wrapper), efeito pendente |
| 0x1A | 0x08034048 | FightPreview_SetupSceneFromPartTechnique | 0 | só se o despachante setar +0xB5 | grava 1 em DAT_0803406C+0x130 (flag de preview), chama `..._G` | confirmado (wrapper), efeito pendente |

Nenhum dos handlers 0x04..0x1A lê byte de argumento inline: os LEN da ROM são 0 para todos eles. Argumentos só em 0x02 (1 B)
e 0x03 (4 B), que leem pelo caminho inline (AS1).

## Divergências com DM.md / AS1
- DM.md §5 diz que LEN[0x03] = 1 e LEN[0x13] = 1. A ROM diz LEN[0x03] = 4 e LEN[0x13] = 0. Bytes em `0x083C6EB8`:
  `00 00 01 04 00 ...` (op 0x00..0x03). Vale corrigir o DM.md.
- AS1 não diz quantos bytes o 0x03 consome (só que lê de forma igual ao 0x02, com índice +0xAB). LEN da ROM = 4: confirmar no disasm do 0x03 se os 4 bytes são lidos pelo handler ou só pulados pelo cursor.

## Pendente (não lido)
- Efeito dos despachantes `Fight_DispatchOnSubstateAA`, `_B`, `_C`, `_D`, `_E`, `_F`, `_G` (usados por 0x04, 0x05, 0x07, 0x08, 0x09, 0x19, 0x1A).
- Confirmar por emulador (não feito aqui) o efeito de 0x01 (escreve cena 8 + 0x40) e o laço: se 0x01 realmente sai do combate.

## Renomeações no Ghidra (confirmadas pelo retorno "renamed successfully")
0x080338DC → ActionScript_Op01_SetSceneExitState; 0x080339F4 → ActionScript_Op04_DispatchOnSubstateAA;
0x08033A10 → ActionScript_Op05_DispatchOnSubstateAA_B; 0x08033A78 → ActionScript_Op07_DispatchOnSubstateAA_C;
0x08033A94 → ActionScript_Op08_DispatchOnSubstateAA_D; 0x08033AB0 → ActionScript_Op09_DispatchOnSubstateAA_E;
0x08033C10 → ActionScript_Op0D_HandleDefenderFollowupFlags; 0x08033CF0 → ActionScript_Op11_SetAttackerAffinityResult;
0x08033D00 → ActionScript_Op12_SetDefenderAffinityResult; 0x08033F04 → ActionScript_Op15_MarkStateFromDamageBelowThreshold
(antigo `Fight_HandleActionScriptOpcode1D`, o "1D" era o valor de +0xDF); 0x08033F64 → ActionScript_Op16_SetStateFromPartFlag1C
(antigo `Fight_HandleActionScriptOpcode1C`); 0x08034008 → ActionScript_Op18_AddRandomBonusAndScaleByPart;
0x0803402C → ActionScript_Op19_DispatchOnSubstateAA_F.
Mantidos (já descritivos): Fight_WaitFor*, Fight_EnsureSelected*, Fight_CommitSelected*, Fight_ApplySpeciality*, etc.
