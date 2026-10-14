# RM1 - nomes de variaveis de RAM (parcial)

Trocas feitas (Ghidra rename_data, retorno "attempted", ainda nao reconferido no listing):
- 0x03000440 ctx_team_screen -> gScreenCtx (base de ctx usada por varias telas, nao so time).
- 0x0300047D currently_selected_medal -> gPartsGridCursorIndex (ctx+0x3D, indice da grade /6 e %6).
- 0x0300047F gCtx_TeamScreenState -> gPartsGridCursorRow (ctx+0x3F = linha, decompile de PartsGrid_SplitRowCol).

Achados:
- Conflito: FI2.md (linha 32) chama 0x0300047F de "estado (PS)". O decompile de PartsGrid_SplitRowCol mostra que ele grava index/6 ali. Vale revisar FI2.
- 0x03000486 gCtx_WaitTimer: nome de timer parece errado. Escrito por PartsGrid_CycleDetailIndex/CycleDetailMode, lido por Fight_DrawBotSlotSprite. Sugestao gCtx_PartsDetailIndex, NAO renomeado (falta decompile). Duplicado com 0x030004C6 no nome.
- 0x0300049C e 0x030004DC ambos gCtx_TeamWinner: 0x030004DC = ctx+0x9C (vencedor, DC.md). 0x0300049C ainda sem nome proprio.
- Nomes "selected_tinpet_*" (0x030004D8..DB, 0x03000491, 0x03000493, 0x0300049A): "tinpet" nao descreve nada. Leitores: Parts_*, Battle_InitPartSprites, BattlePartState_Init, SetSelectedMedabotPreviewPartIds (escritor). Candidato provavel: gSelBot_HeadPartId / LeftArmPartId / RightArmPartId / LegsPartId. Nao renomeado nesta rodada.
- Bloco do bot 0x03000590..0x03000659 (battle_robot_1_*): offsets batem com TA/HN4 e mtbe.csv; nomes sao da comunidade ("robot_1"), nao revisados um a um.

Pendentes desta lista (sem xref feito): 0x02000000 a 0x0202FA00 (ROM/EWRAM), 0x03000491-0x03000659 (exceto acima), 0x03000536-0x0300057A, 0x03000588-0x0300058C.
