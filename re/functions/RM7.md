# RM7 - revisao de nomes de RAM (43 da lista)

## Trocas feitas (rename_data tentado; nao reli no Ghidra)
- 0x030018AB fight_action_result -> gFightActionResult (provavel, TA.md)
- 0x030018D2 fight_slot_applied_flags -> gFightTargetPartFlags (FI_simbolos + HN2.md)
- 0x03000890 gEnemyBattleSideState -> gBattleBotEnemy0 (DIVERGENCIA: e o bloco do bot 3, nao lado de batalha; DF2.md)
- 0x0300059A BattleBot_Name -> gBattleBotHeadRecord (DIVERGENCIA: +0x0A e registro de cabeca de 14 B; decompile 0x080150FC)
- 0x03000591 BattleBot_MedalId -> gBattleBotMedalId (provavel: +0x01 vem do byte 0 do template)
- 0x0300058C keys_pressed_edge -> gJustPressedButtons (provavel, DB.md:31)
- 0x03000536 IWRAM_PlayerSpecialtyBase -> gCtx_ByteF6 (kind func_thumb -> data; e byte ctx+0xF6, nao base)
- 0x03006F74 gKeyItemBagCursorPtr -> gSaveScreenCtxPtr (DIVERGENCIA: FM6.md:4 e ponteiro da tela de salvar)

## Confirmados sem troca (doc)
0x03000579, 0x0300057A, 0x03000588 (BT/MN), 0x03000537 (DM), 0x030063F3 (VA/MP2), 0x03006400, 0x03006408 (VA/HE1),
0x03006D38 (CS2), 0x030004CE (FL3), 0x03000890 ja listado acima.

## Conflitos / abertos (sem doc suficiente, nao renomeados)
- 0x030018D6 fight_slot_roll_halfword: FI_simbolos.csv:40 diz gFight_TargetDamage (conf, BP_08) mas RM2 diz chute e AS2 tem conflito; decidir com AS2.
- 0x030005D3 (+0x43) e 0x03000637 (+0xA7): nome chute; o template 0x080150FC NAO escreve +0x43..+0x5F (DF2 passo 1). Falta o escritor.
- 0x0300049C gCtx_Byte5C, 0x030004D7 gSelBot_Medal, 0x030004D9/DA gSelBot_*ArmPartId: chutes; sem decompile nesta rodada.
- 0x0300381A gPlayerSpeedIndex: OW2.md:51 diz obj+0x2A do slot 0x2F, nao global; nome suspeito.
- 0x030064A1, 0x03006439, 0x030064F4/FC/06/07/0E/0F: sem decompile nesta rodada.
- 0x02024B84, 0x020260E8, 0x020260EA: sem doc nem literal; vem do mtbe (Simon/Rokusho).
- 0x0300182B/2C team_1_trap, 0x03001882 DPAD_press, 0x03006D30: chutes, sem efeito visto.
- 0x030005D0 gBotBlock0Ident: data; HD.md:70 aponta divergencia de base (+0xBB).
- 0x03007F00 sp_usr, 0x03007FA0 sp_irq, 0x03007FFC INTR_VECTOR: mantidos (RM6/MN).

## Achado para o README de pesquisa
- Template 0x080150FC escreve bot+0x01 (byte 0 do registro), +0x04..+0x09, +0x0A..+0x17 (cabeca 14 B), +0x18..+0x27 (especialidade nibbles), +0x20..+0x27 (stats 1..100).
