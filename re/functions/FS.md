# FS: candidatos a início de função que o Ghidra não tem (Metabee EU)

Arquivo gerado: `functions/FS_lista.txt` (endereço TAB origem). Nada renomeado; nada salvo no Ghidra.

## Método
1. ROM `Medabots - Metabee (Europe).gba` lida só para leitura (python -I).
   - Ponteiros: u32 ímpar em 0x08000000..0x08100000, alinhado a 4 (alvo = valor sem o bit 0).
   - Alvos de BL Thumb (par de halfwords F000..F7FF / F800..FFFF; alvo = (inst+4) + offsets).
   - União: 10795 candidatos.
2. Retirados os já conhecidos: linhas F e C de `re/ghidra/medabots_sync.tsv` e linhas `func_thumb`/`func_arm`/`code_pc` de `re/symbols/mtbe.csv` (2510 endereços).
3. Mantidos só os que começam com push: `0xB5xx` (com lr) ou `0xB4xx` (sem lr); o halfword seguinte não pode ser 0000/FFFF. Descartados 7889 sem prólogo e 11 no miolo de função conhecida (heurística: último terminador antes do alvo dentro de 0x1000).
4. Conferência no Ghidra (`get_function_by_address`): 136 candidatos verificados. 9 já são função no Ghidra (nome FUN_xxxx) e saíram da lista. Os outros 127 deram "No function found", ou seja, não estão dentro de nenhum corpo de função (nenhum caso de miolo).

## Contagens
- Candidatos (ponteiro ou BL): 10795
- Já funções (TSV F/C + mtbe): 2510
- Sem prólogo push: 7889
- Miolo de função conhecida (heurística): 11
- Com push: 385 (1 com B4, 384 com B5)
- Ghidra confirmou função (fora do TSV): 9 (todos com prólogo B5, nenhum B4 no grupo verificado)
- **Lista final: 376** (47 BL, 329 só ponteiro)
- Verificados no Ghidra: 136; não verificados (249 da lista final): ainda sem conferência no Ghidra.

## Amostras
- Ghidra tem função (fora do TSV e do mtbe.csv): 0x08000824 (corpo 08000824-08000837), 0x08002EC4 (-08002F5D), 0x0800393C (-08003991), 0x0800994C (-0800999F), 0x0800B79C (-0800B80D), 0x080191E4 (-0801921B), 0x08041B1C (-08041B9F), 0x08048038 (-08048085), 0x0806A860 (-0806AA05). Estes saíram da lista.
- Função desde o sync de 2026-10-14 (exemplos mantidos; nomes: PartSlots_UpdateAndFadeStep, PartEditor_FinishPaletteStep, BattlePreview_FadeSoundStep, PartsGrid_BlendTickStep, PartsGrid_SlideDetailStep, PartDetail_DispatchLegIconStep, Fight_InitPartSpriteEntry, Fight_FadeRowsUpStep, Fight_SelectEnemyMedaforceStep, Fight_StepPaletteFadeToColor, Anim_DrawSegmentThenDispatchLoad, Screen_TransitionEnterAndAdvance, Screen_ClearOamAndAdvance, BotAnim_StepMoveByPhase, Scene_StartSong4AndSlots, snd_update_channel2_if_track_flag, Data_080EC9EE): 0x08007BD8, 0x0800E350, 0x080115E8, 0x08014C84, 0x0803E1A4, 0x08057698, 0x0806B134, 0x080EC9EE, 0x08003D4C, 0x080044F8, 0x08004874, 0x0800801C, 0x08010DAC, 0x0803CECC, 0x0805E450, 0x08071030, 0x08074780.

## Conclusões
- O TSV de sincronização não cobre todas as funções que o Ghidra tem: pelo menos 9 entradas de amostra (cerca de 7% das verificadas) existem no Ghidra e faltam no TSV/mtbe.csv. Antes de tratar um endereço como novo, conferir no Ghidra.
- A lista não é confiável para os 249 candidatos não verificados. Próximo passo: rodar `get_function_by_address` nos 249 restantes (ou listar as funções do Ghidra e fazer a diferença) e remover os que já são função.
- Nenhum candidato foi confirmado como início real de função: isso exige decompilar ou ler o corpo de cada um.
