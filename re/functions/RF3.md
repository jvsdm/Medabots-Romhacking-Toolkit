# RF3 - revisao dos 34 itens de RF3_lista.txt

Simbolos em RF3_simbolos.csv. Nenhum rename feito (nenhum nome novo melhor que o atual).

## Trocas de tipo (lista dizia dado, bytes/Ghidra mostram codigo)
- 0x080196C0 Fight_DrawSecondaryEffectIcon_AltEntry_80196c0: kind dado -> label (provavel). Cai dentro do corpo de Fight_DrawSecondaryEffectIcon (0x08019600-0x0801970D), nao e funcao propria.
- 0x0801A998 Fight_RunStateDispatch: kind dado -> label (provavel). Alvo de COMPUTED_JUMP em Fight_RunStateDispatch (0x0801A7A8).
- Os demais "dado" com nome de rotulo (Fight_DispatchOnSubstateAA_*, Fight_J02_Step*, PartsGrid_SubState*, Fight_Select*/Copy*) sao rotulos de codigo -> kind label.
- Unico dado real: 0x08012968 medaforce_list_ptr_literal (palavra = 0x083B7F8C).

## Funcoes (3 da lista, todas confirmadas no decompile)
- 0x0800F77C PartDetail_DrawCurrentSelectionAttributeName: confirmado, nome mantido.
- 0x08016E5C Fight_IsPartTechniqueSupportUtilityClass: confirmado, nome mantido. Sem push no inicio (funcao folha).
- 0x08019600 Fight_DrawSecondaryEffectIcon: confirmado, nome mantido (desenha icone, nao "update").

## Achados / avisos
- 0x0801D5E0 Fight_FindAllyFollowupActionSource: o endereco cai na 2a metade de um BL (0x0801D5DE = F015, 0x0801D5E0 = FCF1). Nao e inicio de funcao; o endereco ou o nome do mtbe esta errado. Chute. Precisa achar o inicio real.
- 0x08014F1E Fight_CopyBotSlotRecords e 0x08014F6E Fight_CopyBotIdentityRecords: sem push no inicio e sem xref no Ghidra. Possivelmente blocos de uma funcao maior. Ficam como label provavel, a verificar.
- PartsGrid_SubStateB0-B5 (0x08008D90..0x08008F10): alvos da jump table em 0x08008CA0 (PartsGrid_SubStateSwitchStep). chute -> provavel. Nomes "CellOrTable" nao verificados.
- Tabela Fight_J02_StepTable 0x0801FBEC: 5 x 4 B (ponteiros thumb para os cases 0..4). Ja no mtbe (RD1). Os tres Fight_J02_Step* sao cases 0..2 dela.

## Confianca (contagem)
- confirmado: 11 (3 funcoes, 1 literal, 1 chamada BL, 3 cases de J02, 3 ReturnTail bx-r0)
- provavel: 22
- chute: 1 (0x0801D5E0)
Total 34. O consumidor do timer em 0x0801EBB8 continua chute (nao e do item, so da nota).

Em aberto: achar o inicio real de 0x0801D5E0; confirmar inicio de Fight_CopyBotSlotRecords / Fight_CopyBotIdentityRecords; nomes do bloco PartsGrid_SubState B0-B5.
