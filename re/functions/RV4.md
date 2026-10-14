# RV4 - revisao de nomes (60 funcoes, lista RV4_lista.txt)

Resultado: 60 feitas, 58 mantidas (confirmadas no CSV), 2 renomeadas no Ghidra.

## Trocas de nome
- 0x0801F390: Fight_DispatchOnSubstateAA_H_45_DrawPartIconsWaitTimer -> Fight_DispatchOnSubstateAA_H_45_RepairPartTickDrawIcons (chama Fight_RepairPartTick, nao so desenha icones).
- 0x08020B4C: Fight_DispatchOnSubstateAA_R_08 -> Fight_DispatchOnSubstateAA_R_08_RepairTickDrawComposite (repair tick + 2 camadas de composto por lado).

## Achados
- G_06 (0x0801E2B8) "Hold" nao espera de fato: 1 passo e marca feito. Nome mantido como provavel.
- G_00 (0x0801DF0C) preenche duas regioes via CpuFastSet; alvo (paleta/tiles) nao confirmado.
