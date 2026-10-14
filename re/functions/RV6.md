# RV6 - revisão de nomes (60 endereços, lista RV6_lista.txt)

Detalhes por endereço em `RV6_simbolos.csv`. Aqui só trocas e achados.

## Trocas de nome (aplicadas no Ghidra)
- 0x08026D98: `Fight_DispatchOnSubstateAA_AP_0x18` -> `Fight_DispatchOnSubstateAA_AP_18_SetupHitEffectSprite` (toca gfx, SFX 0x2A, posiciona sprite de efeito).
- 0x08026F34: `Fight_DispatchOnSubstateAA_AQ_02_RepairActorPart` -> `Fight_DispatchOnSubstateAA_AQ_02_SplitRepairAmountByCapacity` (só divide a quantia pela capacidade, não aplica reparo).

## Achados
- Cinco endereços da família AV (0x080288C8, 0x08028908, 0x08028944, 0x08028980, 0x080289BC) não têm função no Ghidra (`get_function_by_address`: No function found). O decompile devolve a função vizinha `AV_ApplyStatusToOpponents`. Nomes ficaram sem verificação, marcados `pendente`.
- Stubs BI, BJ, BK, BL, BM, BN, BO, BP, BQ, BR são só despacho (`CallViaR0(tabela[passo])`), confirmados.
