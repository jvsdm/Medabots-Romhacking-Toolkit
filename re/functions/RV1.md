# RV1: revisão de nomes (60 endereços de RV1_lista.txt)

Detalhes e confiança por endereço em `RV1_simbolos.csv`. Aqui só trocas de nome e achados.

## Renomeados no Ghidra
- 0x08000964: `is_input_soft_reset` -> `SoftReset_CheckComboAndRun` (executa o reset, não só testa)
- 0x080009BC: `decompress_and_store` -> `Lz_DecodeIfMagic` (só decodifica se o cabeçalho for 'Le')
- 0x08003A38: `InitializeBattleStatusScreen` -> `BattleStatusScreen_InitStep` (é máquina de passos, não init único)
- 0x08005798: `aplica_mascara_slots_b` -> `ObjectSlots_ApplyMaskOverlay4to7`
- 0x08005D1C: `Parts_DrawStatsPanel` -> `PartSlot_DrawStatsPanelKind0` (par de Kind1/2/3)
- 0x08006764: `Parts_CreateIconSprite` -> `Parts_BuildLargeCompositePreview` (não cria sprite de ícone)
- 0x08003394: Ghidra já tinha `Gfx_InitBgLayersAndEnableDisplay`; mtbe.csv ainda diz `MainState0_Menu2_Sub0_InitBg` (desatualizado)

## Achados
- 0x08000000 (`entry`): não há função no Ghidra; é o desvio do cabeçalho para crt0.
- 0x0800656C e 0x0800667C duplicam, respectivamente, BattlePartState_Init (0x0800529C) e BattlePartPreview_Rebuild (0x080053C8). Candidatos a unificar.
- 0x08008CF4..0x08008D54 (5 nomes de PG): não são funções no Ghidra; são casos do switch 0x08008CA0. Não renomeados.
- 0x080023D8 (`SwapDisplayBuffersOnVBlank`, era "chute"): troca pares de ponteiros; o nome fica provável, interpretação aberta.
- 0x08006824 e 0x08006F34: tabelas de salto não recuperadas pelo Ghidra.
