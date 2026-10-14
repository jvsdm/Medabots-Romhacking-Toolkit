# BA2: estágios de animação de bot (0x0806B0D4) e tabelas 0x08485700 / 0x08485988 (2026-10-14)

Tarefa BA2 (vindo de JT6). Resultado: (1) hospedeiro e tabela de 11 estágios entendidos; (2) base da tabela de
handlers achada (literal em 0x080688BC, dentro de `fx_step_table_a`); (3) consumidor de 0x08485988 NÃO achado
(lead abaixo).

## (1) BotAnim_TickStageCallbacksAll 0x0806B0D4

- Loop de 20 slots (`i*0x40` a partir de 0x03002C30 = gObjectSlots). Byte `+0x20` = estágio.
- Estágio != 0: `call_r1(i, tabela[estágio-1])`, tabela = 0x084857D0 (literal em 0x0806B104; confirmado por scan da
  ROM e xref DATA de 0x0806B0F0).
  - Retorno != 0: `+0x1A++` (contador de fase).
  - Retorno == 0 e estágio != 0: `slot_spawn(i, 0, 0, 0)` = reset (zera +0x20..+0x22 e +0x1A). Retorno 0 = "estágio terminou".
- Chamadores (xrefs BL, confirmado no Ghidra): `intro_scene_step` (0x0806A104), `dispatch_scene_state` (0x0806A3B4),
  `scene_sequence_a` (0x0806A644), `Scene_AnimSeqStepAdvance9` (0x080691E2), `Fight_FrameTickWaitGate10` (0x08069226).
  Roda por quadro nas cenas de intro/sequências e no quadro de batalha. Os 20 objetos são slots de gObjectSlots,
  não só bots de batalha.

| Estágio | Entrada | Alvo | Nome | Confiança / o que faz |
|---|---|---|---|---|
| 1 | 0x084857D0 | 0x0806B134 | BotAnim_StageApproach1 | provável: +0x1A<0x20 move X+1 e Y por tabela 0x08485924; <0x30 Y fixo; senão X+2. Retorno via tail-jump não recuperado. |
| 2 | 0x084857D4 | 0x0806B1A8 | (sem função no Ghidra) | Bytes parecidos com o 1 (variante). Não decompilado. |
| 3 | 0x084857D8 | 0x0806B1D0 | (sem função no Ghidra) | Idem, variante. Não decompilado. |
| 4 | 0x084857DC | 0x0806B1F8 | BotAnim_StageHold4 | confirmado (bytes `mov r0,#1; bx lr`): retorna 1 sempre (espera). Sai por escrita externa no estágio. |
| 5 | 0x084857E0 | 0x0806B1FC | BotAnim_StageStep5 | provável: +0x1A<0x10 → Y+2; faixa 0xD2..0xE1 → Y-2; >0xE1 chama Obj_SetupSlotPtr. |
| 6 | 0x084857E4 | 0x0806B248 | BotAnim_StageBlendAlpha6 | provável: escreve BLDALPHA (0x04000052) com 0x1000 ou (0x20-n)<<8 \| n conforme +0x1A; ímpar: Y-1; >0x3F zera 0x04000050-área; retorno = !(+0x1A>0x3F). |
| 7 | 0x084857E8 | 0x0806B2B8 | (sem função no Ghidra) | Bytes: lê +0x1A, limita, escreve +0x22/+0x23. Não decompilado. |
| 8 | 0x084857EC | 0x0806B2F4 | Obj_UpdateSlotPosition | confirmado (decompile, nome já existente): soma velocidade (tabelas 0x0806B378) em +0xC/+0x10, grava X/Y de tela; se +0x1A>8 grava +0x1A = tabela>>3 e vai p/ estágio 9. Retorna 1. |
| 9 | 0x084857F0 | 0x0806B3D8 | BotAnim_StageWalkStep | confirmado (decompile): anda X ±1 até 0x60, depois fixa 200 e retorna 0. Já nomeado. |
| 10 | 0x084857F4 | 0x0806B484 | (sem função no Ghidra) | Bytes: +0x27++ com limite; usa 0x04000... Não decompilado. |
| 11 | 0x084857F8 | 0x0806B130 | BotAnim_StageDone11 | confirmado (bytes `mov r0,#0; bx lr`): retorna 0 → reset do bot via slot_spawn(i,0). Fim de animação. |

Renomeados no Ghidra (existiam como função): 0x0806B134 → BotAnim_StageApproach1, 0x0806B1FC → BotAnim_StageStep5,
0x0806B248 → BotAnim_StageBlendAlpha6. Não renomeados: 0x0806B1F8 e 0x0806B130 (sem função definida; rename falhou).
Ver BA2_criar.txt.

Pendente: estágios 2, 3, 7 e 10 precisam de função criada no Ghidra para decompile.

## (2) Tabela de handlers de cena (25 entradas, 0x08068924..0x080692EC)

- A base usada NÃO é 0x08485700 e sim **0x08485708**. Literal `0x08485708` em **0x080688BC**, dentro de
  `fx_step_table_a` (0x08068888). Achado por scan de literais na ROM (`python -I`). O FN6 dizia "sem ldr literal": errado.
- Código: `CallViaR0(tabela[*DAT_080688C0])` com índice = byte de estado em RAM. Retorno 0: `*DAT_080688DC++`;
  retorno != 0: zera `*DAT_080688C4` e, se estado==3, faz `REG_DISPCNT &= DAT_080688C8`.
- Chamador: `Scene_FxStepAdvance` (0x08068E88). Literais para o meio da tabela também aparecem em 0x08068904
  (0x0848571C) e 0x08068DF8 (0x08485730), funções sem definição no Ghidra.
- 0x08485700 = 0x080708D9 e 0x08485704 = 0x08000841 não são handlers; a base 0x08485708 vai até 0x08485760.
- Nomes de FN6 para os handlers ficaram como estavam (não revisados aqui).

## (3) Tabela 0x08485988 (11 ptrs: 0x0806DC7C ... 0x0806E074)

- Nenhum literal de base 0x08485988 na ROM (scan de 0x08485980..0x0848598C).
- Literais vizinhos: 0x08485994 em 0x0806DE68 e 0x084859B4 em 0x0806DE64, no pool de uma função não definida no
  Ghidra perto de 0x0806DE20 (cmp/beq, ldrh, bot 0x03002C30). 0x084859BC em 0x0806E148.
- Consumidor provável: função em 0x0806DE20 (sem definição no Ghidra). Não confirmado. Em aberto.

## Pendências

- Criar funções no Ghidra em 0x0806B1A8, 0x0806B1D0, 0x0806B2B8, 0x0806B484, 0x0806B1F8, 0x0806B130 e 0x0806DE20.
- Decompile de 0x0806DE20 para confirmar o consumidor da 0x08485988.
- Decompile de 0x08068904 e 0x08068DF8 (ponteiros para o meio da tabela de handlers).
