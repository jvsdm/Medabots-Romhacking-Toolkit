# GH-CF: decisoes de conflito de nome (mtbe x novo)

Ghidra: renames aplicados com rename_function_by_address (Ghidra nao foi salvo). mtbe.csv NAO editado.
"Ghidra ja" = o nome ja estava assim no Ghidra quando a funcao foi aberta.

| endereco | mtbe | alternativo | decisao | motivo |
|---|---|---|---|---|
| 0x0803579C | Fight_GetPartProficiencyBase | Fight_AccumRosPart | Fight_AccumRosPart | Nao retorna valor: soma no acumulador de ROS (fi+0xE8): byte +0x4D, bonus de medalha, rand&7, especialidade e +0xA5/+0xA6. Renomeado. |
| 0x08034F68 | do_damage_on_part | Fight_ApplyDamageToPart | Fight_ApplyDamageToPart | Nome generico no mtbe; corpo grava dano na armadura (+0x59), flag +0xD1 e devolve excedente. Renomeado. |
| 0x080183A4 | HandleSelectedMedabotPreviewCursorInput | cursor 3x3 da luta (MERGE_NOTAS) | HandleSelectedMedabotPreviewCursorInput (mantido) | Unico chamador: UpdateSelectedMedabotPreviewScreen 0x08013E84. E o cursor da previa, nao o da luta. O corpo usa helpers Fight_* mas o chamador decide. |
| 0x08047BA8 | Overworld_StartMapTransitionEffect | Snd_SfxRequestWrapper | Snd_SfxRequestWrapper | Corpo e so snd_sfx_request (r0 passa adiante). Muitos chamadores (eventos 0x28, 0x44, objetos de mapa) usam como som. Renomeado. |
| 0x08045A40 | IsCompositePreviewPointOnScreen | (pool?) | sem funcao: remover a linha do mtbe | Ghidra nao tem funcao nem contem o endereco; zero xrefs de codigo. Dado/literal pool. Acao: retirar do mtbe (nao editei). |
| 0x0804305C | BuildCompositeBattleSpriteObjects | Sprite_WriteComposite4Objects | Sprite_WriteComposite4Objects | Nao e o mesmo corpo de 0x08042E58: so escreve 4 objetos OAM, sem Tile_ClearSlots e sem gravar +8/+9/+0x18. Variante sem alocacao. Renomeado. |
| 0x08042E58 | CreateCompositeBattleSpriteObjects | (mesmo corpo?) | CreateCompositeBattleSpriteObjects (mantido) | Chama Tile_ClearSlots antes de montar 4 objetos e grava campos extras. Ver 0x0804305C. |
| 0x08037280 | Fight_ScaleAttackValueByPartScalar | Fight_ComputePartValue | Fight_ScaleAttackValueByPartScalar (mantido) | fi+0xEC = (fi+0xE8>>2) + byte +0x4E, depois divide pelo escalar +0x54, minimo 1. Ghidra ja. |
| 0x0803725C | Fight_CheckRosMeetsMinimumThreshold | FUN_0803725C / Fight_SpendRos20ReturnsExhausted | Fight_SpendRos20ReturnsExhausted | Subtrai 0x20 de fi+0xE8 (clamp 0) e devolve 1 se zerou. Nao so checa. Ghidra ja. |
| 0x08040CD0 | ClearObjectAttributeEntries | Tile_ClearSlots | ClearObjectAttributeEntries (mantido) | Zera N blocos de 0x40 bytes a partir de um indice via CpuFastSet; mesmos blocos que o escritor de OAM usa. Confianca provavel. |
| 0x08036B84 | FightPreview_SetPartSpriteId | Fight_CopyPartArmorToDisplayValues | Fight_CopyPartArmorToDisplayValues | Copia armadura atual (+0x59) para fi+0xC8/+0xCC e marca +0xD1=1. Nao copia id de sprite. Renomeado. |
| 0x0801D6A4 | ActionFamily_CancelGravHandler | Fight_DispatchOnSubstateAA_E_01_LoadPhaseGfx | Fight_DispatchOnSubstateAA_E_01_LoadPhaseGfx (Ghidra ja) | Carrega gfx conforme a fase (0x1B/0x1C/0x1D) e avanca substate. Nao e cancelar gravidade. |
| 0x0801D738 | Fight_PrepareFollowupSourceDisplay | Fight_DispatchOnSubstateAA_E_02_CopyBotBuffers | Fight_PrepareFollowupSourceDisplay | Copia 4 blocos de 0x40 do bot, refaz ids exibidos e chama Sprite_WriteComposite4Objects. Bate com a descricao do mtbe. Renomeado. |
| 0x0801D9DC | ActionFamily_Recovery_Handler | Fight_DispatchOnSubstateAA_E_05_SlideBotIntoPlace | Fight_DispatchOnSubstateAA_E_05_SlideBotIntoPlace (Ghidra ja) | Desliza Y do bot ate 0x78 e incrementa substate. Nao e recovery. |
| 0x0801DD30 | ActionFamily_Reactivate_Handler | Fight_DispatchOnSubstateAA_E_08_WaitCounterThenSfx30 | Fight_DispatchOnSubstateAA_E_08_WaitCounterThenSfx30 (Ghidra ja) | Conta byte ate 0x40, toca snd_sfx_request(0x30) e incrementa substate. Nao e reativacao. |
| 0x0804B758 | EventInit_Skip3BytesAndMarkInitialized | Event_ReadMessageArgsAndDefer | Event_ReadMessageArgsAndDefer | Script_ReadBytes(3) e gCurrentEventState=1 (defere a VM). Le 3 bytes de argumento, nao so pula. Provavel: o corpo nao mostra onde os bytes ficam. |
| 0x08036FF8 | Fight_SpawnInlineEffectTextObjects | FightPreview_SetupSpriteRow | Fight_SpawnInlineEffectTextObjects (mantido) | Monta N objetos em linha (tile fixo, posicao, Slot_SetupFromRecordList). Bate com o nome do mtbe. |
| 0x08049440 (duplicado) | Map_TickObjectAnimation | Map_TickObjectAnimation | Map_TickObjectAnimation | Interpretador grande com codigos 0..5 (R3). E o nome principal. |
| 0x080508B4 (duplicado) | Map_TickObjectAnimation | Map_ObjectAnimStep | Map_ObjectAnimStep | Corpo pequeno: estado 0 variante, estado 1 conta ate zero. Chamado por Event_Op1A_Cont (HC1). Renomeado para o nome do R3. |
| 0x08072D68 (duplicado) | snd_track_step | snd_track_step | Snd_TrackStepInterpreter | Interpretador de trilha: le byte do cursor e despacha nota, pausa, comando (SND12). Renomeado. |
| 0x08074118 (duplicado) | snd_track_step | snd_track_step | Snd_ChannelTickStep | Tick por canal: incrementa periodo, calcula freq (aux_div_u32), fim da sequencia chama snd_channel_reset. Renomeado. |

## Pendencias em aberto
- 0x08045A40: remover do mtbe (nao e funcao). Nao editei o mtbe.
- 0x0804B758: decisao provavel; confirmar em Script_ReadBytes onde ficam os 3 bytes.
- 0x08040CD0: confianca provavel (depende de DAT_08040D04 ser o mesmo bloco do OAM).
