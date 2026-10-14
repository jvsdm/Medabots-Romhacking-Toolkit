# NM2: limpeza de nomes em português (GH-NM2, continuação do NM)

Renomeações feitas no Ghidra (decompile confirmado). `mtbe.csv` não foi editado; a fusão fica para o sync.
Símbolos em `NM2_simbolos.csv`.

| Endereço | Antes | Depois | Motivo |
|---|---|---|---|
| 0x080039BC | tick_par_contador | Pair_TickDownCounterIfFlag | Decompile: se bit0 de *flag, decrementa contador e incrementa o seguinte (par de contadores). |
| 0x08004E74 | tick_par_contador_b | Pair_TickUpCounterIfFlag | Decompile: se bit0 de *flag, incrementa contador e o seguinte. Gêmeo de sinal oposto. |
| 0x08004E98 | init_4_slots | ObjectSlots_Init4 | Decompile: zera 4 slots de 0x40 B (campos +0x00, +0x23, +0x40...) e chama atualiza_slot(0..3). |
| 0x08004EF8 | atualiza_slot | ObjectSlot_UpdatePosition | Decompile: calcula posição/tile do slot por tabela e avança contador +0x23 (&0x3F). |
| 0x08004FD0 | desenha_9_slots_a | BattlePreview_LoadSlotTiles9 | Decompile: LoadCompositeBattleSpriteTiles para cada um dos 9 registros de 8 B. Só carrega tiles. |
| 0x08005000 | desenha_9_slots_b | BattlePreview_CreateSlotSprites9 | Decompile: CreateCompositeBattleSpriteObjects para 9 entradas; apaga máscara do slot vazio. |
| 0x08005118 | desenha_icone_a | BattlePreview_DrawMedalLabel | Decompile: apaga máscara de sprites, monta texto do rótulo da medalha (slot selecionado) e layout de glifos. |
| 0x0800519C | desenha_icone_b | BattlePreview_DrawNameLabel | Decompile: igual ao anterior, mas copia 0xB bytes do nome do bot (ou repete glifo). |
| 0x0800522C | carrega_paleta_icone | BattlePreview_LoadEntryPalette | Decompile: CpuFastSet de 0x20 B de paleta (ROM ou entrada) e preenche retângulo do tilemap BG. |
| 0x0800529C | init_estado_parte_batalha | BattlePartState_Init | Decompile: zera buffers (0xD0, 0x800, 0x60 B), inicializa contexto de preview e monta composto grande de 4 partes. |
| 0x080053C8 | desenha_parte_batalha | BattlePartPreview_Rebuild | Decompile: refaz composto grande de 4 partes (lado 0/1 por flag) e limpa flags. |
| 0x080054C0 | desenha_slot_batalha_5 | BattlePreview_BuildLargeCompositeLayer5 | Decompile: BuildFourPartLargePreviewComposite com camada 5 (tamanho 0x3C x 0x32). |
| 0x0800598C | checa_parte_sinal_b | Parts_CheckSpriteIdsUpdateMotion | Decompile: AreBattleSpritePartIdsValid sobre 5 bytes do registro; se válido, Sprite_UpdateComposite4Motion. Variante de Parts_CheckSpriteIdsSetSpeed. |
| 0x080059D0 | prepara_4_partes | Parts_PrepareFourPartSlots | Decompile: grava 4 ids de peça no registro do bot, limpa flag 0x40 do slot e ajusta contagem de inventário (AdjustInventoryPartCount). |
| 0x08005AD0 | desenha_icone_c | PartSlot_DrawNameGlyphs | Decompile: apaga máscara e monta glifos do nome da peça (ou repete glifo se vazio). Não desenha ícone. |
| 0x08005B44 | carrega_paleta_icone_b | PartSlot_LoadIconPalette | Decompile: CpuFastSet de 0x20 B da paleta do ícone e preenche retângulo do tilemap BG. |
| 0x08005BB4 | desenha_texto_parte_a | PartSlot_DrawNumber3 | Decompile: Num_DrawUpTo3DigitsOrDash na coluna x=0x1A (valor de atributo da peça). |
| 0x08005C10 | desenha_icone_d | PartSlot_DrawNameLabel | Decompile: copia 0xB B do nome da peça e monta glifos (retângulo 0x14/0x12). Não é ícone. |
| 0x08005C9C | desenha_icone_e | PartSlot_DrawNameLabelKind0 | Decompile: copia 0xF B do nome da peça (índice +0x98, registro de 0x10) e monta glifos. Não é ícone. |
| 0x08005E00 | desenha_icone_f | PartSlot_DrawNameLabelKind1 | Decompile: idem, para a peça +0x99 (ponteiro de 4 B). |
| 0x08005E84 | desenha_stats_parte_b | PartSlot_DrawStatsPanelKind1 | Decompile: GetPartStats modo 1 da peça +0x99, copia 0x10 B e preenche retângulo 0x10x0xD do BG. |
| 0x08005F68 | desenha_icone_g | PartSlot_DrawNameLabelKind2 | Decompile: idem, para a peça +0x9A. |
| 0x08005FEC | desenha_stats_parte_c | PartSlot_DrawStatsPanelKind2 | Decompile: GetPartStats modo 2 da peça +0x9A; retângulo 0x10x0xF. |
| 0x080060D0 | desenha_icone_h | PartSlot_DrawNameLabelKind3 | Decompile: idem, para a peça +0x9B. |
| 0x08006154 | desenha_stats_parte_d | PartSlot_DrawStatsPanelKind3 | Decompile: GetPartStats modo 3 da peça +0x9B; retângulo 0x10x0x11. |
| 0x08006238 | estilo_linha_stats_9 | PartStats_FillBandH9 | Decompile: FillBgTilemapRectSequential 0xE x 9; param 0/1 escolhe a cor da faixa. |
| 0x08006284 | estilo_linha_stats_B | PartStats_FillBandHB | Idem, altura 0xB. |
| 0x080062D0 | estilo_linha_stats_D | PartStats_FillBandHD | Idem, altura 0xD. |
| 0x0800631C | estilo_linha_stats_F | PartStats_FillBandHF | Idem, altura 0xF. |
| 0x08006368 | estilo_linha_stats_11 | PartStats_FillBandH11 | Idem, altura 0x11. |
| 0x08005588 | troca_selecao_parte | PartSelect_HandleDirectionInput | Decompile: lê bits de direção (0x10..0x80), troca índice pela tabela de vizinhos, SFX 2 e redesenha slots/preview. |
| 0x08005628 | init_4_slots_b | ObjectSlots_Init4Overlay | Decompile: inicializa 4 slots do segundo grupo (+0x100..+0x1C0) via atualiza_slot_b. |
| 0x080056A4 | atualiza_slot_b | ObjectSlot_UpdatePositionOverlay | Decompile: calcula posição/tile do slot do grupo overlay e avança contador +0x23. |
| 0x08044B58 | site_StarterBot (função) | Bot_InitStarterRecord | Decompile: escreve registro do bot inicial (5 bytes de id, 0x2B nos membros) e marca flags. Era label `site_` no mtbe. |
| 0x0805DBC4 | Site_SaveLoadPromptStep | SaveLoad_PromptStep | Decompile: após 0x1D quadros e SFX livre, roda SaveLoad_RunOption até 4 vezes, toca música 6 e troca de estado. |
| 0x0805DDEC | Site_RunSaveLoadOption | SaveLoad_RunOption | Decompile: opção 0 = salvar, 1 = carregar (com verificação), 2 = limpa 0x10 B e grava; depois Save_WriteBlocksEeprom. |
| 0x0805DF18 | site_LoadGame | SaveLoad_LoadGame | Função confirmada (get_function). Carrega save. |
| 0x0805E010 | site_SaveGame | SaveLoad_SaveGame | Função confirmada (get_function). Grava save. |
| 0x08070108 | init_fila_4 | ListRing_InitFour | Decompile: liga 4 nós numa lista circular a partir de um sentinela. |
| 0x0807028C | init_tela_bg0 | Screen_InitBg0Tilemap | Decompile: LZ77 para VRAM, copia 20 linhas de tilemap, grava paleta 0xFC0 e liga BG0 em REG_DISPCNT. |
| 0x08070464 | init_tela_bg0_v2 | Screen_InitBg0TilemapAlt | Idem, variante com paleta 0xF80 e outros blocos. |

## Não renomeado (proposta, sem função no Ghidra)
- `site_BotCountValidator` (0x08043820, kind label no mtbe.csv). O get_function_by_address diz "No function found", mas o decompile do mesmo endereço devolve `AreBattleSpritePartIdsValid` (5 bytes, rejeita 0x1E e >0x77). Conflito: o nome `site_BotCountValidator` parece errado. Não renomeado; mantido o nome já existente `AreBattleSpritePartIdsValid`. Revisar no sync.

## Fora do escopo desta leva
- `scene_init_*`, `init_hide_sprites`, `Init_CopyStartupGraphicsToVram`: prefixo/palavras em inglês, não renomeados.
- Labels `battle_robot_*`, `debug_menu_*` e tabelas: são dados, não funções (fora do NM2).
