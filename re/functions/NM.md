# NM: limpeza de nomes (GH-NM)

Renomeações feitas no Ghidra (confirmadas por decompile). `mtbe.csv` não foi editado; a fusão fica para o sync.

| Endereço | Antes | Depois | Motivo |
|---|---|---|---|
| 0x08011FEC | Fight_DrawBotPartEntrySprites | Fight_LoadBotPartEntryTiles | Decompile: só chama LoadCompositeBattleSpriteTiles (carrega tiles). |
| 0x08012024 | Fight_DrawBotPartEntryIcons | Fight_CreateBotPartEntrySprites | Decompile: CreateCompositeBattleSpriteObjects + velocidade de animação 8. Cria sprites. |
| 0x08012098 | Fight_DrawBotPartEntryTiles | Fight_UpdateBotPartEntryMotion | Decompile: Sprite_UpdateComposite4Motion por entrada. Atualiza movimento. |
| 0x080121A8 | Fight_FormatBotLabelText | Fight_CopyBotLabelRecord | Decompile: Mem_CopyBytes de 8 bytes de um registro para o buffer do rótulo. Só copia, não formata texto. |
| 0x080058A4 | processa_troca_slots | Parts_HandleSlotSwapInput | Decompile: lê entrada (bit 4), toca SFX, troca dois registros de 8 bytes (Mem_SwapBytes) e redesenha. Confirmado. |
| 0x08005948 | checa_parte_sinal | Parts_CheckSpriteIdsSetSpeed | Decompile: AreBattleSpritePartIdsValid; se válido, velocidade 8. Checa e ajusta, não "sinal". |
| 0x08005D1C | desenha_stats_parte | Parts_DrawStatsPanel | Decompile: pega stats (GetPartStats), copia texto e preenche retângulo 2,2,16,11 do BG. |
| 0x0807DA50 | SoftFloat64_Compare | SoftF64_Compare | Decompile: sf_unpack x2; classe < 2 retorna 1; senão sf64_cmp_core. Referência do grupo. |
| 0x0807DBCC | SoftFloat64_CompareSafe | SoftF64_CompareAlt1 | Decompile: corpo idêntico ao 0x0807DA50. Duplicata. |
| 0x0807DB34 | Math_Sf64CompareSafe | SoftF64_CompareAlt2 | Mesmo corpo de comparação (classe < 2 -> 1). Duplicata. |
| 0x0807DAE8 | Math_Sf64CompareSafeAlt | SoftF64_CompareAlt3 | Corpo igual ao 0x0807DB34 (ver nota da FR1). Duplicata. |
| 0x0807D48C | SoftFloat64_AddWrapper | SoftF64_AddAlt1 | Decompile: sf_unpack x2 + sf64_add_core + sf64_pack, sem o xor de sinal. |
| 0x0807D4BC | SoftFloat_Add64 | SoftF64_Add | Decompile: mesma sequência; tem xor de sinal em variável não usada (possível resto de subtração). Nome base do grupo. |
| 0x08036270 | Fight_IncrementActionUsageCounter_08036270 | Fight_IncrementActionUsageCounter | Sufixo de endereço removido (gerado pelo sync). |
| 0x08007E38 | PartsGrid_InitScreen_08007E38 | PartsGrid_InitScreen | Sufixo removido; nome já existia no mtbe.csv. |
| 0x0805EFFC | Gfx_PaletteFadeStep_0805EFFC | Gfx_PaletteFadeStep | Sufixo removido; nome já existia no mtbe.csv. |
| 0x08037830 | Fight_FindActiveBotInSideWithGroup (FR3) x Fight_FindLiveSlotMatchingField91 (CH2) | Fight_FindActiveBotInSideWithGroup (mantido) | Decompile: recebe lado (param_1) e bot de referência (param_2); sorteia slot da tabela 16, percorre 3 slots e devolve o primeiro vivo com +0x91 igual ao do bot de referência; senão 0xFF. O nome CH2 não descreve os parâmetros de lado. |
| 0x0801A998 | Fight_RunStateDispatch_0801A998 | Fight_RunStateDispatch | Sufixo removido (nome já no mtbe.csv); jump table pelo byte de estado. |
| 0x08042238 | site_TinpetCreate | Tinpet_CreateRecord | Decompile: acha slot livre em tabela de 9 registros de 8 bytes e grava registro de tinpet (flag, 0x1E e stats). |
| 0x080436AC | site_PartCycle | PartSelect_CycleNextValid | Decompile: avança id de peça (0..0x78) pulando inválidos, usando GetPartStats; devolve o próximo válido. |

## Pendente (não feito neste lote)
- Restante dos nomes em português no R0 (desenha_*, init_*, atualiza_*, carrega_*, troca_*, prepara_4_partes, tick_par_contador*, estilo_linha_stats_*, etc.).
- Labels `site_BotCountValidator` (0x08043820) e `site_StarterBot` (0x08044B58): kind label, não func; ver se é função antes de renomear.
- Não renomeado: `Fight_RandomSlotFromTable16`, `Fight_DispatchOnSubstateAA_*` (nomes com `Slot` são do jogo, fora do escopo de palavras-chave).
- Soft-float: 0x0807DA50, 0x0807DBCC, 0x0807D48C, 0x0807D4BC foram decompilados; 0x0807DB34 e 0x0807DAE8 só pela nota da FR1 (corpo igual), falta confirmar no decompile.
