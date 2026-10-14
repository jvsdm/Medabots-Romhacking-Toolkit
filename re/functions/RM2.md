# RM2: revisao de variaveis de RAM

Trocas (kind=data em todas as linhas; detalhes em RM2_simbolos.csv):
- 0x03000666: battle_robot_1_form -> gFightBot0Flags_D6 (e bot0+0xD6, bit0 em Fight_IsOpticClassAction; nome antigo errado).
- 0x0300066D / 0x0300066E: viraram gFightBot0ChosenPartSlot_DD / gFightBot0ActivePartSlot_DE (confirmado em Fight_SelectBotActivePart e Fight_DrawPartStatusIcon).
- 0x03000BE1..BE7: "first_medal_*" -> gMedalRecord_* (e registro de 0x40 indexado por slot em Fight_AwardPartExperience; "first" era errado).
- 0x03001804 / 0x03001806: gFightBot0PartIds / gFightBot0PartId_Slot2 (4 ids de parte por bot, stride 0xC).
- 0x03001851: gFight_EffectMode -> gTeamPickCounter (escrito por TeamPick e Overlay, nao pela batalha).
- 0x0300196F / 0x03003811: fi_stage_flag -> gFightMedalStageUpFlag; dpad_input_2 -> gPlayerFaceDirection.

Achados:
- gKeysHeld 0x03002B64 e keys_held 0x03001798 sao escritos ambos em get_player_inputs: conceito duplicado, checar.
- gEnemyBattleSideState 0x03000890: sem xref no Ghidra; o literal na ROM esta em 0x08004C4C, fora de funcao.
- Varios nomes ficaram como chute (team_1_trap*, DPAD_press, fight_slot_*, gMapPassTable_Value3, gMapBlockIndex, gPlayerSpeedIndex): precisam de decompile mais fundo.
