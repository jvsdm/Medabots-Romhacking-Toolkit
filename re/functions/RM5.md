# RM5: revisao dos nomes de RAM (lista de 65)

Feito: 50 renomeados no Ghidra via rename_data (retorno "attempted"; nao reconferido por listagem).
Os demais ficaram sem troca (ver "Mantidos"). CSV: RM5_simbolos.csv.

## Trocas principais
- Bloco do bot (0x03000590 + i*0x100): battle_robot_1_* viraram BattleBot_<Campo>; prefixos Head_/Arm1_/Arm2_/Legs_
  para as pecas (P(k) = B+0x48+0x18k, conforme DM.md s0).
- Ctx 0x03000440: gCtx_Byte46, gCtx_PartGaugePhaseSideA/B, gCtx_Byte5C, gCtx_Byte8C, gCtx_Byte8E, gCtx_NameBuffer.
- Selecionado (ctx+0x98..0x9B): gSelBot_HeadPartId / LeftArm / RightArm / LegsPartId (nomes candidatos de RM1.md).

## Achados / divergencias
- 0x03000486 se chamava gCtx_WaitTimer, mas o timer de espera e ctx+0x86 (0x030004C6). O +0x46 e outro campo.
- 0x0300049C era duplicata de gCtx_TeamWinner; o vencedor e ctx+0x9C (0x030004DC, BT.md). Agora gCtx_Byte5C (chute).
- 0x02000000 nao e gShopItems: HO1.md mostra que e destino de Gfx_LoadDefaultAsset.
- 0x0300052E nao e medaforca: EN.md mostra copia de medalha (0x10 B).
- 0x03000651 (+0xC1): AI.md chama de token 'P'; 'P' = 0x50 = teto do medidor (AI.md). Nomeado como medidor.
- Lado esquerdo/direito: DM.md chama k=1 de "braco D" (direito) e k=2 de "braco E". Os nomes antigos left_arm (k=1)
  e right_arm (k=2) contradizem. Renomeados como Arm1/Arm2 para nao afirmar lado. Pendente decompile.
- ctx+0x5A: FL6.md o descreve como contador incrementado; JT7.md como indice (0x0300049A). Nome escolhido: indice.
- 0x03000536 (IWRAM_PlayerSpecialtyBase) esta com kind=func_thumb na lista, mas e byte de dado (ctx+0xF6). Nao renomeado.

## Mantidos (sem doc ou sem literal)
- Chutes sem doc: 0x030005D3 (+0x43), 0x03000637 (+0xA7), 0x0300059A (+0x0A), 0x030004D7 (ctx+0x97).
- Sem troca: 0x0300058C keys_pressed_edge, 0x03000537, 0x03000579, 0x0300057A, 0x03000588, 0x030005D0 (gBotBlock0Ident),
  0x02024B84 (sem literal na ROM), 0x020260E8 / 0x020260EA (gBattleRootY/X, sem literal nem doc),
  0x0202FA00 (provavel, mantido).

## Abertos
- Confirmar lado dos bracos (Arm1/Arm2) com decompile de AD.md/AI.md.
- Confirmar ctx+0x5A (FL6 vs JT7) e o ctx de FL3 (DAT_08002C50).
- Conferir por listagem no Ghidra se os rename_data pegaram.
