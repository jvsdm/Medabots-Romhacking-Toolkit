# HA4: handlers dos despachantes Fight_DispatchOnSubstateAA_* (lista HA4_lista.txt, 42 entradas)

Cada entrada é um handler dentro da tabela de um despachante, indexado pelo subestado +0xAA. Subestado = `fight_information`
(0x03001800) +0xAA, que é `action_sequence_state` (+2 de `action_script_state` em +0xA8, types TSV). +0xB5 = `action_handler_complete_flag`
(+0xA8+0xD), zerado pelo despachante antes de cada passo.
Renomeados no Ghidra: 24 (decompile lido). Os outros 18 não têm função criada no Ghidra, mas o código é Thumb válido
(conferido nos bytes da ROM); não criei funções, então não renomeei.
Coluna "Próximo" = valor escrito em +0xAA ao sair (+1 quando não há outro valor).

## 0x08020474 L 0x0F -> Fight_DispatchOnSubstateAA_L_0F_LoadGfxAndSlots
- Passo: entrada do bot: descompacta gráficos, zera e configura o slot 10 (record list), toca SFX 0x2B.
- Lê/escreve: lê +0xF8 do bloco de bot (flag &2 escolhe o offset 0x154/0x158); escreve sprites (+0x280, +0x282, +0x284, +0x29C) e +0xAA.
- Próximo: 0x10. Confiança: confirmado (decompile).

## 0x08020530 L 0x10 -> Fight_DispatchOnSubstateAA_L_10_WaitSlotAnim
- Passo: espera a animação do slot 10 terminar (Anim_RunSlotScript), depois zera tiles (Tile_ClearSlots 10,1).
- Lê/escreve: lê byte de estado do slot; escreve +0x86=0x3C (timer) e +0xAA.
- Próximo: 0x11. Confiança: confirmado (decompile).

## 0x080205A4 M 0x00 -> (sem nome; sem função no Ghidra)
- Passo: carga de gráficos (LZ 0x080009BC, cpufastset 0x0807CCC0) e setup de objetos (0x08036FF8, 0x080370AC), com leitura/escrita em fight_information.
- Lê/escreve: 0x03001800, 0x03002C30 (gObjectSlots).
- Próximo: não resolvido (provável +1). Confiança: provável (bytes e BL; sem decompile).

## 0x08020608 M 0x01 -> (sem nome; sem função no Ghidra)
- Passo: mesma família de M 0x00 (carga de gráficos + BL 0x080370AC, 0x08040B90); escreve em gObjectSlots.
- Próximo: não resolvido (provável +1). Confiança: provável (bytes e BL).

## 0x08020628 M 0x02 -> Fight_DispatchOnSubstateAA_M_02_LoadGfxAndSlotPos
- Passo: carrega gráficos, posiciona o alvo via tabela (índices de +0x87 e +0xA7 do bot), configura slot 0x21 (record list).
- Lê/escreve: lê fi+0x87/+0xA7 e tabela de coordenadas; escreve em gObjectSlots (+0x840, +0x842 region) e +0xAA.
- Próximo: 0x03. Confiança: confirmado (decompile).

## 0x080206E8 M 0x03 -> Fight_DispatchOnSubstateAA_M_03_WaitSlotAnim
- Passo: espera a animação do slot 0x21, depois Tile_ClearSlots(0x21,1).
- Próximo: 0x04. Confiança: confirmado (decompile).

## 0x08020728 M 0x04 -> Fight_DispatchOnSubstateAA_M_04_ApplyPartDamage
- Passo: aplica o dano na peça alvo (Fight_SubtractPartArmorClampZero), atualiza sprites das peças e objetos; se a parte é a cabeça (+0xA7==0), enfileira bot de cabeça quebrada.
- Lê/escreve: fi+0x87 (bot alvo) e fi+0xA7 (parte alvo); +0xAA.
- Próximo: 0x05. Confiança: confirmado (decompile); significado de +0x87/+0xA7 provável.

## 0x08020768 M 0x05 -> Fight_DispatchOnSubstateAA_M_05_PlayEffect
- Passo: toca efeito da peça (FUN_0803F63C com 0x0E), define +0xA6=0x18 (duração).
- Próximo: 0x06. Confiança: confirmado (decompile).

## 0x080207CC N 0x00 -> (sem nome; sem função no Ghidra)
- Passo: função curta; lê 0x03000590 (bloco de bot) e 0x03001800; sem chamadas.
- Próximo: desconhecido. Confiança: chute.

## 0x08020844 O 0x00 -> (sem nome; sem função no Ghidra)
- Passo: penalidade/efeito antes do subhandler: BL 0x0803579C (Fight_GetPartProficiencyBase no mtbe, a revisar), 0x08035814 (Fight_ApplyVirusBugRosPenalty no mtbe), 0x0803725C, 0x08037280, 0x08036270; depois chama subtabela 0x083ADA58 via 0x0807CCF0.
- Próximo: depende da subtabela 0x083ADA58 (não lida). Confiança: provável (BL e literais).

## 0x080208E8 P 0x01 -> (sem nome; sem função no Ghidra)
- Passo: escolhe a peça mais danificada para reparo (Fight_PickMostDamagedRepairablePart1Based 0x08033184).
- Lê/escreve: 0x03000590, 0x03001800.
- Próximo: não resolvido (provável +1). Confiança: provável.

## 0x08020928 P 0x0B -> Fight_DispatchOnSubstateAA_P_0B_RepairPartOrSkip
- Passo: reparo. Se a peça alvo tem flag (bloco 0x03000590 + bot*0x100 + part*0x18 + 0x4F) e +0xCA != 0, escolhe peça a reparar (Fight_PickMostDamagedRepairablePart1Based) e chama FUN_08036CCC; senão pula.
- Lê/escreve: lê 0x03000590 (flag da peça), fi+0xCA, fi+0x88; escreve fi+0xD0 e fi+0x8B (slot-1).
- Próximo: 0x0E se reparou; senão 0x0C. Confiança: confirmado (decompile).

## 0x080209A0 P 0x11 -> (sem nome; sem função no Ghidra)
- Passo: código curto; lê 0x03001800 e escreve um byte de estado; RET.
- Próximo: desconhecido. Confiança: chute.

## 0x080209D8 Q 0x00 -> (sem nome; sem função no Ghidra)
- Passo: mesma sequência de penalidade de O 0x00 (0x0803579C, 0x08035814, 0x0803725C, 0x08037280, 0x08036270) e depois subtabela via 0x0807CCF0.
- Próximo: depende da subtabela (não lida). Confiança: provável.

## 0x08020A90 R 0x01 -> (sem nome; sem função no Ghidra)
- Passo: escolhe peça quebrada ou faltando (Fight_PickRandomBrokenOrMissingPart1Based 0x0803325C) para reparo; BL 0x080351E8, 0x080372D4.
- Próximo: não resolvido (provável 0x0C ou 0x0F). Confiança: provável.

## 0x08020AD0 R 0x0C -> Fight_DispatchOnSubstateAA_R_0C_SplitCountOrSkip
- Passo: copia stats se flag (Fight_CopyStatsIfFlagSet); se não há peça quebrada válida, pula; senão divide a contagem (FightPreview_SplitCountByMax) pela peça escolhida.
- Lê/escreve: lê fi+0x88, fi+0xA7, fi+0xCA; escreve fi+0xD0, fi+0x8B.
- Próximo: 0x0D se pulou; 0x0F se dividiu. Confiança: confirmado (decompile).

## 0x08020B4C R 0x08 -> Fight_DispatchOnSubstateAA_R_08_LoadPartIconsAndJump
- Passo: carrega ícones das peças do lado (FightPreview_LoadDisplayedPartIdsForSide 0 ou 1) e chama FUN_080447EC duas vezes; no fim pula por tabela (jump table não recuperada, ptr 0x08020CE8).
- Lê/escreve: lê flag &2 em PTR_DAT_08020C14+0x30; escreve +0xAA.
- Próximo: 0x09, mais o salto pela tabela. Confiança: confirmado (decompile); destino do salto não resolvido.

## 0x08020CEC R 0x12 -> (sem nome; sem função no Ghidra)
- Passo: só carrega 0x03001800 e retorna (3 instruções).
- Próximo: desconhecido. Confiança: chute.

## 0x08020D24 S 0x00 -> (sem nome; sem função no Ghidra)
- Passo: começo da sequência de hit: penalidade (0x0803579C, 0x08035814, 0x0803725C, 0x08037280, 0x08036270), lê 0x03000590 e 0x03001800.
- Próximo: não resolvido (provável 0x04). Confiança: provável.

## 0x08020DF8 S 0x04 -> Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29
- Passo: carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29.
- Lê/escreve: lê fi+0x87 (bot alvo); escreve sprites em gObjectSlots e +0xAA.
- Próximo: 0x05. Confiança: confirmado (decompile). Esta entrada aparece também nos despachantes AB..AU, T, U, X, Y (coluna extra da lista).

## 0x08020EDC S 0x05 -> (sem nome; sem função no Ghidra)
- Passo: move o sprite do alvo (BL 0x080353CC); escreve em gObjectSlots (+0x858, +0x867).
- Próximo: provável 0x06. Confiança: provável.

## 0x08020FB8 S 0x06 -> Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots
- Passo: carrega gráficos e configura slot 0x21 (record list +0x8C).
- Próximo: 0x07. Confiança: confirmado (decompile).

## 0x08021000 S 0x07 -> Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim
- Passo: espera a animação do slot 0x21 (record +0x8C).
- Próximo: 0x08 quando termina. Confiança: confirmado (decompile).

## 0x08021038 S 0x08 -> Fight_DispatchOnSubstateAA_S_08_LoadGfxByMode
- Passo: carrega gráficos conforme o modo (fi+0x51, valores 0..3); modo 0/1 e 3 configuram slot, modo 2 ajusta BG; timer 0x3C.
- Lê/escreve: lê fi+0x51; escreve BG (0x0840 region), +0x86, +0xAA.
- Próximo: 0x09. Confiança: confirmado (decompile).

## 0x080211C8 S 0x09 -> (sem nome; sem função no Ghidra)
- Passo: escreve atributos de sprite (gObjectSlots +0x848, 0x03000440).
- Próximo: provável +1. Confiança: provável.

## 0x0802121C S 0x0A -> Fight_DispatchOnSubstateAA_S_0A_TimerThenBranchByMode
- Passo: conta o timer (+0x86); quando zera, se modo (fi+0x51) < 3 zera um byte e vai para 0x0B; se modo == 3 toca SFX 0x52 e vai para 0x20.
- Lê/escreve: lê fi+0x51, fi+0x86, fi+0x89; escreve fi+0x86 e +0xAA.
- Próximo: 0x0B ou 0x20. Confiança: confirmado (decompile).

## 0x080212A0 S 0x0B -> (sem nome; sem função no Ghidra)
- Passo: testa byte de estado de sprite (fi+0x842, gObjectSlots+0x867) contra 0x10 e ramifica.
- Próximo: não resolvido. Confiança: chute.

## 0x08021300 S 0x0C -> Fight_DispatchOnSubstateAA_S_0C_CheckPartThenBranch
- Passo: testa a flag da peça (fi+0x8A slot, +0x2A e +0x2B contra fi+0x52); se ok zera byte e vai para 0x0D; senão, modo 2 vai para 0x14, outros tocam SFX 0x3E e vão para 0x0E.
- Próximo: 0x0D, 0x0E ou 0x14. Confiança: confirmado (decompile).

## 0x08021370 S 0x0D -> (sem nome; sem função no Ghidra)
- Passo: chamadas BL 0x08074DE0 e 0x08040CD0; escreve gObjectSlots +0x848/+0x867.
- Próximo: provável 0x0E. Confiança: provável.

## 0x080213E4 S 0x0E -> Fight_DispatchOnSubstateAA_S_0E_CountdownClearSlots
- Passo: contagem regressiva de um byte em -8 até 8; ao chegar, Tile_ClearSlots(0x21,1) e vai para 0x0F.
- Próximo: 0x0F (ou fica em 0x0E). Confiança: confirmado (decompile).

## 0x08021418 S 0x0F -> Fight_DispatchOnSubstateAA_S_0F_RecordDamageAndJump
- Passo: grava o registro de dano do alvo (fi+0x8A seleciona o slot de 8 bytes em +0x2A..+0x2D); usa Fight_AddByteClampToMax com dano e bônus (+0xE8, +0xEC); modo 2 tem variante; no fim pula por tabela (não recuperada).
- Próximo: 0x10 e salto por tabela. Confiança: confirmado (decompile).

## 0x08021564 S 0x10 -> (sem nome; sem função no Ghidra)
- Passo: só carrega 0x03001800 e retorna.
- Próximo: desconhecido. Confiança: chute.

## 0x080215C8 S 0x14 -> Fight_DispatchOnSubstateAA_S_14_SetFlagsSfx40
- Passo: grava estado de 4 em um byte do bloco de estado, zera 0x870, toca SFX 0x40.
- Próximo: 0x15. Confiança: confirmado (decompile).

## 0x08021608 S 0x15 -> (sem nome; sem função no Ghidra)
- Passo: carrega gráfico de efeito (BL 0x080352EC, 0x08040CD0, 0x08040BD8 com dado 0x083B6100); escreve gObjectSlots +0x862.
- Próximo: provável 0x16. Confiança: provável.

## 0x08021638 S 0x20 -> Fight_DispatchOnSubstateAA_S_20_WaitSlotAnimTimer
- Passo: espera a animação do slot 0x21 (record +0xE0); depois timer 0x3C.
- Próximo: 0x21. Confiança: confirmado (decompile).

## 0x0802167C S 0x21 -> Fight_DispatchOnSubstateAA_S_21_SetupSpritesAfterTimer
- Passo: conta o timer (+0x86); ao zerar, copia tiles (FUN_0803F63C) e posiciona sprites (0x58, 0x40, 0x28; x 0x88/0x68 pela flag de tela).
- Próximo: 0x22. Confiança: confirmado (decompile).

## 0x08021734 S 0x22 -> (sem nome; sem função no Ghidra)
- Passo: BL 0x080355F0 três vezes e 0x08074DE0; escreve gObjectSlots +0x867.
- Próximo: provável 0x23. Confiança: provável.

## 0x08021774 S 0x23 -> Fight_DispatchOnSubstateAA_S_23_CheckFlagOrJump25
- Passo: testa flag do slot (fi+0x8A, +0x2A, +0x2B); se falhou, SFX 0x3E e pula direto para 0x25; se ok, zera byte e vai para 0x24.
- Próximo: 0x24 ou 0x25 (salto). Confiança: confirmado (decompile).

## 0x080217D8 S 0x24 -> Fight_DispatchOnSubstateAA_S_24_MoveSpriteStep
- Passo: um passo de movimento de sprite (+8 ou -8 em dois bytes); se o byte chegou a 0x10, toca SFX 0x3E.
- Próximo: sempre 0x25. Confiança: confirmado (decompile).

## 0x0802186C S 0x25 -> Fight_DispatchOnSubstateAA_S_25_ClearSlotsAfterMove
- Passo: move até o byte chegar a 8; então Tile_ClearSlots(0x21,3).
- Próximo: 0x26 quando termina; senão fica em 0x25. Confiança: confirmado (decompile).

## 0x080218E4 T 0x00 -> (sem nome; sem função no Ghidra)
- Passo: penalidade (0x0803579C, 0x08035814, 0x0803725C, 0x08036270); carrega gráfico (LZ 0x084D6A5C para VRAM 0x06014800) e paleta (0x0850FC38); 0x08040B90 com dado 0x083B6100.
- Próximo: provável 0x08 (T 0x08 é o próximo handler). Confiança: provável.

## 0x08021950 T 0x08 -> Fight_DispatchOnSubstateAA_T_08_LoadGfxAndSlot
- Passo: descompacta gráficos, configura slot 0x21 (record +0xE4); se flag de fi bit0, marca +0x840 |= 0x1000.
- Próximo: 0x09. Confiança: confirmado (decompile).

---

## Esboço da máquina de estados (+0xAA), por despachante

- Entrada do bot (L): 0x0F (gráficos, SFX 0x2B) -> 0x10 (espera anim) -> 0x11.
- Efeito de dano (M): 0x00/0x01 (gráficos) -> 0x02 (posiciona) -> 0x03 (espera anim) -> 0x04 (dano na peça) -> 0x05 (efeito, +0xA6=0x18) -> 0x06.
- Reparo (P/R): P 0x0B tenta reparar; se conseguiu vai para 0x0E, senão 0x0C -> R 0x0C divide contagem -> 0x0F, ou pula para 0x0D. R 0x08 carrega ícones e pula por tabela para 0x09.
- Sequência de hit (S): 0x00 (penalidade) -> 0x04 (SFX 0x29) -> 0x05 -> 0x06 -> 0x07 (espera) -> 0x08 (gráficos por modo) -> 0x09 -> 0x0A (timer: modo 3 vai para 0x20, senão 0x0B) -> 0x0C (flag: ok -> 0x0D, senão modo 2 -> 0x14 ou 0x0E) -> 0x0E (contagem) -> 0x0F (registra dano; salto por tabela) -> 0x10 -> 0x14 -> 0x15 -> 0x20 (espera) -> 0x21 (timer, sprites) -> 0x22 -> 0x23 (flag: falhou -> 0x25) -> 0x24 -> 0x25 (move até 8) -> 0x26.
- Pós-batalha (T): 0x00 (penalidade + gráfico) -> 0x08 (gráfico + slot) -> 0x09.

Pendências: as 18 entradas sem função no Ghidra (ver marcação "sem nome"), a subtabela 0x083ADA58 (O/Q), a jump table de R 0x08 e S 0x0F, e os destinos de 0x0B e 0x0D a 0x16 (não lidos). Os nomes de +0x87/+0xA7 (bot/parte alvo) são prováveis.
