# TR3: escritores de Conceal (bot+0xB9) e de Scout (fi+0x28+8k) (GH-TR3, 2026-10-14)

Parcial. Só leitura: Ghidra (xrefs e decompile) e ROM. Nada salvo, nada renomeado (nenhum nome novo confirmado),
mtbe.csv não editado, sem emulador. Continua o TR2.md.

Confiança: C = confirmado nesta rodada | P = provável | A = aberto.

## Achados desta rodada

1. Roteiro da técnica 37 (C): ponteiro 0x083C6AA4 (`gTechniqueActionScriptPtrTable` 0x083C6CA8[37]). O fluxo linear
   (`TQ2_fluxo.txt` linha 115) é só `0x31` (FUN_08034498, `ActionScript_Op31_CallSubstateAA`) seguido de `0x01`
   (fim de segmento). Ou seja, o roteiro não escreve +0xB9 diretamente. O único efeito dele é agendar o substate AA.
2. Substate AA chamado pelo 0x31 (C): `Fight_DispatchOnSubstateAA_AA_0B_SetStepCode1D` 0x08023718 grava
   fi+0xA6 = 0x1D e fi+0xAA += 1 (decompile). Não escreve bot+0xB9. O passo seguinte (handler do código 0x1D) não foi
   achado: o handler do AA é uma tabela indexada por fi+0xA6, e a entrada de 0x1D não apareceu como xref no Ghidra.
   Hipótese P: o incremento do Conceal fica nesse handler seguinte do AA.
3. Xrefs literais (C, Ghidra):
   - 0x03000649 (bot 0, +0xB9): só `Fight_ResetBotPartState` 0x08036FB8 (zera) e `Fight_ComputeTotalRos` 0x08038D16 (lê).
     Nenhum literal de escrita. Logo o escritor usa ponteiro base + offset (ADD + STRB), não o endereço absoluto.
   - 0x03000590 (base do bloco de bot): ~80 referências em DATA e PARAM. Entre elas, funções de ação e de substate:
     `Fight_DispatchOnSubstateAA_BR_50_CheckLeadersDefeated` 0x08030E9C, `..._BP_08_ApplyTeamPendingDamage` 0x0802F8FE,
     `..._BL_12_DamageRandomPartOfTargets` 0x0802E222, `..._BH_CommitArmorCheckHeads` 0x0802D5D8,
     `..._BA_08_AddGaugeToAllies` 0x0802A554, `..._BB_MarkOpponentsSetGauge` 0x0802A848, `..._AR_ShowDamageNumbers` 0x0802756C,
     `ActionScript_Op4B/4C/4E/41` (0x080349F8, 0x08034A6C, 0x08034B36, 0x080346DC), `Fight_ComputeTotalRos` 0x08038D08/0x08038CB2.
     Ainda não decompilados um por um para procurar STRB em +0xB9.
4. Decompiles sem escrita em +0xB9 (C): `Fight_CommitActiveBotPartFields` 0x0801B3BE (grava +0x48..+0x5F da peça
   ativa) e `Fight_SelectActionSourceStep` 0x08014DA2 (incrementa contador). Descartados como escritores de Conceal.

## Respostas diretas

| Pergunta | Resposta | Conf |
|---|---|---|
| Quem escreve bot+0xB9 (Conceal)? | Não achado. O roteiro da 37 só agenda o substate AA (0x31 → AA_0B 0x08023718 → código 0x1D). | A (escritor), C (roteiro) |
| Acumula? | Não sabido. Se o escritor for o handler 0x1D, é incremento por uso (P). | A |
| Persiste entre turnos? | Pelo que se lê, só zera na derrota (`Fight_ResetBotPartState`). Início de ação não zera (TR2). | P |
| Quem escreve a tabela Scout fi+0x28+8k (0x03001828)? | Não achado. Literais 0x03001828 e 0x0300182A só aparecem em `Fight_ComputeTotalRos`. A escrita deve ser por base+offset na struct 0x03001800. | A |
| Quando é limpo? | Não achado. A tabela é indexada pela fase (fi+0x89). Sem zeragem encontrada. | A |

## Pendente

1. Decompilar o handler do substate AA com código 0x1D (achar a tabela indexada por fi+0xA6; literal stub+0x24 como em HA8.md).
   Se for ele, documentar o incremento de bot+0xB9 e a condição (quem é o alvo).
2. Varrer STRB de +0xB9 nas funções da lista de xrefs de 0x03000590 acima (decompile, procurar `+ 0xb9`).
3. Escritor do Scout: procurar literais 0x03001800 com escrita de fi+0x28..0x67 (base da struct de luta) e
   xrefs de 0x03001800 com STRB em +0x28+8k, ou a fase (fi+0x89) como índice de escrita.
4. Replay (não feito, sem emulador): ler 0x03000649 antes e depois de uma ação 37 e de duas ações seguidas.

Nenhum símbolo novo confirmado nesta rodada. Ver TR3_simbolos.csv (vazio além do cabeçalho).
