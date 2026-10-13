# HA3: handlers dos despachantes Fight_DispatchOnSubstateAA_* (42 handlers, lista HA3_lista.txt)

Método (rodada 1): bytes da ROM original + decodificador Thumb. Redo (2026-10-13): as 42 funções existem no Ghidra e foram decompiladas e renomeadas (ver CSV).
ramos seguidos, pools ignorados). O Ghidra NÃO tem função definida nestes endereços (get_function_by_address e
disassemble_function falham). Nada foi criado nem renomeado no Ghidra. Nomes sugeridos só no CSV.
Seções "resumo": a leitura veio de lista de constantes e chamadas, não do corpo completo; leia o corpo antes de usar.

## Mecanismo (confirmado na ROM)
- Cada despachante (A..Z, AA..BR) é um stub com literal `0x03001800` (bloco de combate) e ponteiro de tabela
  no pool. Stub: zera +0xB5 e chama `tabela[+0xAA]` (ver H.md e R1a.md).
- Conferido nesta rodada: para as 42 linhas de HA3_lista.txt, `tabela_do_despachante[índice] == endereço` (42/42).
- Quase todo handler termina com `+0xAA += 1` (leitura/escrita de 0x03001800+0xAA). Alguns gravam valor fixo
  (H:0x24 grava 0x0A) ou também mexem em +0xAB.
- Template comum (passo de apresentação): carga de gráfico LZ77 para VRAM (0x06010000, 0x06010800, 0x06014800,
  0x06004120), cópia de paleta OBJ (0x050003C0, 0x050003A0, 0x050002E0), OBJs em gObjectSlots (0x03002C30,
  offsets 0x280..0x2E2), e som/efeito. Não decifrei qual ataque/texto cada passo mostra.
- Helpers sem nome no mtbe.csv: 0x0800FB9C, 0x0800FBC0, 0x0800FC55, 0x0800FCEF, 0x0800FD5B (carga de gráfico);
  0x0804DD23, 0x0804DD47, 0x0804DDDC, 0x0804DE76, 0x0804DEE2 (cópia de paleta); 0x08049DBC, 0x08049DE0, 0x08049EC7,
  0x08049F5C (som/efeito); 0x0802A0FB, 0x0802A14C, 0x08029E51, 0x0802AC46 (por bot/lado). Dado 0x083B6100 aparece
  em vários handlers (tabela/texto, não confirmado).
- Chamador conhecido: H (0x0801E2D8) é chamado de 0x080341DE (UNCONDITIONAL_CALL); a função que contém 0x080341DE
  não existe no Ghidra. Quem escolhe o despachante (a fase da Robattle) continua em aberto.

## Handlers (despachante:índice; "também" = outros despachantes que apontam para o mesmo código)

## 0x0801ECC0 Fight_DispatchOnSubstateAA_H_20_ApplyPartDamage (H:0x20)
- Assinatura: `void H_20(void)` (sem argumentos; usa o bloco de combate 0x03001800 e o bloco do bot).
- O que faz: sorteia a peça-alvo do bot selecionado (`Fight_SelectRandomTargetPartForSelectedBot`, índice em +0x87),
  copia o alvo para o bloco de combate (`Fight_CopySelectedTargetToFightInfo`) e grava +0xD0 = +0x8B. Se o byte de
  estado da peça (bloco do bot, `+0xA7*0x18 + +0x49`) é '0' (0x30), chama `FUN_08036B84(+0x88, +0x8B)`; senão soma 10
  em +0xCA quando o bot tem bit 2 de +0xD6 e chama `Fight_ApplyDamageToPart(+0x88, +0x8B, +0xCA)`. Depois +0xAA += 1.
- Chama: Fight_SelectRandomTargetPartForSelectedBot, Fight_CopySelectedTargetToFightInfo, FUN_08036B84,
  Fight_ApplyDamageToPart. Chamada por: tabela H (índice 0x20).
- Memória: lê 0x03001800 (+0x87, +0x88, +0x8B, +0xA7, +0xCA); bloco do bot 0x03000590 + bot*0x100 (+0x49, +0xD6);
  escreve +0xD0, +0xCA, +0xAA.
- Para o Lua: porta (é a aplicação de dano à peça; depende de RNG e das funções de peça).
- Confiança: confirmado (corpo lido). Passo: confirmado como dano à peça-alvo; significado do ramo '0' (peça
  intacta?) provável.

## 0x0801ED50 Fight_DispatchOnSubstateAA_H_21_LoadBotSpriteAndPlace (H:0x21; também H:0x4A, P, R)
- Assinatura: `void H_21(void)`.
- O que faz: o mesmo byte de estado da peça decide o ramo. Ramo '0': `decompress_and_store` (gráfico LZ77),
  `bios_cpufastset` com 8 palavras (paleta), grava 0x2100 num registrador de OBJ e pede SFX 0x54. Outro ramo: mesmo
  esquema com outro gráfico, valor 0x1500 e SFX 0x2A. Depois monta o OBJ em gObjectSlots (+0x280 = posição fixa,
  +0x29C = 0, mais os campos de OBJ 0x0E, 0x80 e 0) e posiciona com X/Y do bot (+0x154/+0x158 ou +0x156/+0x15A, conforme
  +0xF8 bit 1), ±0x10 conforme +0x8B. Grava gObjectSlots+0x284 (Y). +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, snd_sfx_request. Chamada por: tabela H (índice 0x21) e os outros
  índices listados no CSV.
- Memória: 0x03001800 (+0x87, +0xA7, +0xAA); bloco do bot selecionado (+0x49, +0x8B, +0xF8, +0x154..+0x15A; literais
  não resolvidos nesta leitura); gObjectSlots (+0x280..+0x29C, +0x284); SFX 0x54 ou 0x2A.
- Para o Lua: porta (sprite e posicionamento de OBJ). Gráficos e paletas: ver os literais, não resolvidos aqui.
- Confiança: confirmado (corpo lido). Passo "carrega sprite do bot que age e posiciona": provável.

## 0x0801EED4 Fight_DispatchOnSubstateAA_H_24_JumpToStep0A (H:0x24)
- Assinatura: `void H_24(void)`.
- O que faz: só grava +0xAA = 0x0A (salto direto para o passo 10). Não toca em gráfico nem em som.
- Memória: 0x03001800+0xAA (escrita).
- Para o Lua: porta trivial.
- Confiança: confirmado.

## 0x0801EEE4 Fight_DispatchOnSubstateAA_H_28_LoadEffectSpriteAtBot (H:0x28)
- Assinatura: `void H_28(void)`.
- O que faz: descomprime gráfico (`decompress_and_store`) e paleta (`bios_cpufastset` 8 palavras); monta um OBJ em
  gObjectSlots (+0x280 = posição; atributo 0x1400 em +iVar-4); escolhe X/Y do bot (+0x154/+0x158 ou +0x156/+0x15A,
  conforme +0xF8 bit 1). Grava gObjectSlots+0x284 = posição inicial e +0x2B0 = posição-alvo (usada por H:0x29 como
  fim do movimento); +0x..+2 = alvo-0x10. Toca SFX 0x2F. +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, snd_sfx_request. Chamada por: tabela H (índice 0x28).
- Memória: gObjectSlots (+0x280, +0x284, +0x2B0, +0x29C); bloco do bot (+0xF8, +0x154..+0x15A); 0x03001800+0xAA.
- Para o Lua: porta (sprite e movimento de OBJ).
- Confiança: confirmado (corpo lido). Passo "efeito que desliza até o bot": provável.

## 0x0801EFC8 Fight_DispatchOnSubstateAA_H_29_WaitSpriteMoveThenTimer60 (H:0x29)
- Assinatura: `void H_29(void)`.
- O que faz: a cada frame ímpar (gFrameCount & 1) incrementa gObjectSlots+0x284. Quando ele iguala gObjectSlots+0x2B0
  (valor gravado por H:0x28), grava 0x03000440+0x86 = 60 (timer de 60 frames) e +0xAA += 1. Senão só espera.
- Chama: nada. Chamada por: tabela H (índice 0x29).
- Memória: gObjectSlots+0x284 (lê/escreve), +0x2B0 (lê); 0x03000440+0x86 (escreve); flag de frame (gFrameCount).
- Para o Lua: porta (espera por contador).
- Confiança: confirmado (corpo lido). Passo "espera o movimento do OBJ acabar": provável.

## 0x0801F018 Fight_DispatchOnSubstateAA_H_30_LoadEffectSpriteAtBot (H:0x30; também AP)
- Assinatura: `void H_30(void)`.
- O que faz: descomprime gráfico e paleta (mesmos helpers de H:0x28); OBJ em gObjectSlots+0x280 com atributo 0x2000;
  posiciona com X/Y do bot (mesmo padrão de +0xF8 bit 1); grava +0x284 = posição; toca SFX 0x2A; +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, snd_sfx_request. Chamada por: tabela H (índice 0x30) e AP.
- Memória: gObjectSlots (+0x280, +0x284, +0x29C); bloco do bot (+0xF8, +0x154..+0x15A); 0x03001800+0xAA.
- Para o Lua: porta (sprite de efeito).
- Confiança: confirmado (corpo lido). Passo: efeito de ataque no alvo, provável.

## 0x0801F0F0 Fight_DispatchOnSubstateAA_H_38_LoadEffectSpriteAtBot (H:0x38)
- Assinatura: `void H_38(void)`.
- O que faz: mesmo padrão de H:0x30 (gráfico e paleta via helpers, OBJ em gObjectSlots+0x280 com atributo 0x2E00,
  posição X/Y do bot em +0x284, SFX 0x2A), +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, snd_sfx_request. Chamada por: tabela H (índice 0x38).
- Memória: gObjectSlots (+0x280, +0x284); bloco do bot (+0xF8, +0x154..+0x15A); 0x03001800+0xAA.
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo: efeito de ataque, provável (diferente de H:0x30 só no atributo do OBJ).

## 0x0801F1C8 Fight_DispatchOnSubstateAA_H_3B_PickRepairablePartOrSkip (H:0x3B)
- Assinatura: `void H_3B(void)`.
- O que faz: escolhe a peça mais danificada e reparável do lado (`Fight_PickMostDamagedRepairablePart1Based` sobre +0x88).
  Se não há peça (retorno 0): +0xAA += 1 (pula). Se há: +0xD0 = +0x8B = índice-1, chama `FUN_08036CCC(+0x88, +0xD0,
  +0xEC)` e grava +0xAA = 0x3E (salta para o passo H:0x3E, que tem gráfico próprio). Correção: a versão anterior dizia
  "só +0xAA += 1", que estava errada.
- Chama: Fight_PickMostDamagedRepairablePart1Based, FUN_08036CCC. Chamada por: tabela H (índice 0x3B).
- Memória: 0x03001800 (+0x88, +0x8B, +0xD0, +0xEC, +0xAA).
- Para o Lua: porta (escolha de peça para reparo).
- Confiança: confirmado (corpo lido, lógica de salto clara). Significado "reparo de peça": provável (nome do helper).

## 0x0801F214 Fight_DispatchOnSubstateAA_H_3E_LoadGfxSpawnEffectAtBot (H:0x3E; também AQ, P, R)
- Assinatura: `void H_3E(void)`.
- O que faz: descomprime gráfico e paleta (`decompress_and_store` + `bios_cpufastset` 8 palavras). Pega X/Y do bot
  (+0x158/+0x15A se +0xF8 bit 1 == 0; senão +0x154/+0x156) e chama `FUN_08036C20(10, +0x8B, x, y, 0x20)`, que cria o
  efeito no bot. Depois +0xAA += 1 e faz um salto indireto por ponteiro (`UNRECOVERED_JUMPTABLE` em 0x0801F28A):
  o Ghidra não recuperou o alvo. Pendência: descobrir esse alvo (pode mudar +0xAA de novo).
- Chama: decompress_and_store, bios_cpufastset, FUN_08036C20, ponteiro não recuperado. Chamada por: tabela H (0x3E),
  AQ, P, R; e o H:0x4D/H:0x3B saltam para cá (+0xAA = 0x3E).
- Memória: 0x03001800 (+0x8B, +0xAA); bloco do bot (+0xF8, +0x154..+0x15A).
- Para o Lua: porta a parte de efeito; o salto indireto precisa do alvo.
- Confiança: confirmado (corpo lido). Passo "efeito no bot e continuação": provável.

## 0x0801F290 Fight_DispatchOnSubstateAA_H_3F_WaitSlotAnimEndThenClear (H:0x3F; também AQ, P, R)
- Assinatura: `void H_3F(void)`.
- O que faz: roda o script de animação do slot 10 (`Anim_RunSlotScript`, ponteiro em +0x80 de DAT_f2c8). Se o byte de
  estado do slot é -1 (animação acabou): `Tile_ClearSlots(10, 1)`, grava timer 60 em 0x03000440+0x86 e +0xAA += 1.
  Senão só espera (repete o passo).
- Chama: Anim_RunSlotScript, Tile_ClearSlots. Chamada por: tabela H (0x3F), AQ, P, R.
- Memória: slot de animação 10 (estado e script); 0x03000440+0x86 (escrita); 0x03001800+0xAA.
- Para o Lua: porta (espera por fim de animação).
- Confiança: confirmado (corpo lido). Passo "espera o fim da animação do slot 10 e limpa": provável. Anterior
  (dado 0x083B6100 e +0x2A2) não se confirma nesta leitura.

## 0x0801F2DC Fight_DispatchOnSubstateAA_H_41_ShowNumberSpriteRowAtBot (H:0x41; também AQ, P, R)
- Assinatura: `void H_41(void)`.
- O que faz: gráfico e paleta (helpers de H:0x28). Pega X/Y do bot (mesmo par de H:0x3E). `Fight_WriteNumberTiles`
  escreve dígitos a partir do halfword em 0x03001800+0xC8 (valor de combate, provavelmente dano). Depois
  `FightPreview_BuildSpriteListRow(..., slot 10, +0x8B, x, y)` monta a linha de sprites do número e toca SFX 0x5B.
  +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, Fight_WriteNumberTiles, FightPreview_BuildSpriteListRow,
  snd_sfx_request. Chamada por: tabela H (0x41), AQ, P, R.
- Memória: 0x03001800 (+0x8B, +0xAA, +0xC8); bloco do bot (+0xF8, +0x154..+0x15A); tiles/OBJ do número (não mapeados).
- Para o Lua: porta (mostrar número sobre o bot).
- Confiança: confirmado (corpo lido). Passo "mostra valor numérico (dano) sobre o bot": provável.

## 0x0801F374 Fight_DispatchOnSubstateAA_H_44_PlaySfx2EAndAdvance (H:0x44; também AQ, P, R)
- Assinatura: `void H_44(void)`.
- O que faz: toca SFX 0x2E e +0xAA += 1. Nada mais (passo de pausa com som).
- Memória: 0x03001800+0xAA.
- Para o Lua: porta trivial (ou nada, se o som não for portado).
- Confiança: confirmado (corpo curto).

## 0x0801F390 Fight_DispatchOnSubstateAA_H_45_DrawPartIconsWaitTimer (H:0x45; também AQ, P, R)
- Assinatura: `void H_45(void)`.
- O que faz: chama `FUN_08036D24(0x030018C8, +0x88, +0x8B)` (o primeiro argumento é o endereço 0x030018C8; resultado em iVar2). Desenha os ícones de peça do
  lado: `FightPreview_LoadPartIconRows` e `FightPreview_DrawPartIconPairs`, com o flag de lado vindo de +0xF8 bit 1.
  Se iVar2 != 0: grava timer 60 em 0x03000440+0x86 e +0xAA += 1. Senão repete o passo.
- Chama: FUN_08036D24, FightPreview_LoadPartIconRows, FightPreview_DrawPartIconPairs. Chamada por: tabela H (0x45),
  AQ, P, R.
- Memória: 0x03001800 (+0x88, +0x8B, +0xAA, +0xC8); bloco do bot (+0xF8); 0x03000440+0x86.
- Para o Lua: porta (tela de ícones de peça; a parte de desenho pode ser simplificada).
- Confiança: confirmado (corpo lido). Correção: a versão anterior dizia "não achei escrita em +0xAA"; o passo avança
  quando FUN_08036D24 devolve não zero. Significado (espera a barra de peças): provável.

## 0x0801F3F0 Fight_DispatchOnSubstateAA_H_47_RepairPartOrJump4A (H:0x47)
- Assinatura: `void H_47(void)`.
- O que faz: entra no ramo de reparo só se (bloco do bot +0xD6 bit 1 OU byte +0x4F da peça ≠ 0) E o halfword +0xCA ≠ 0.
  Então pega a peça mais danificada reparável (`Fight_PickMostDamagedRepairablePart1Based` sobre +0x88). Se achou:
  +0xD0 = +0x8B = índice-1, `FUN_08036CCC(+0x88, +0xD0, +0xCA)` e +0xAA = 0x4A (salta para H:0x4A). Se não: +0xAA += 1.
  É a variante de H:0x3B com o valor de +0xCA em vez de +0xEC.
- Chama: Fight_PickMostDamagedRepairablePart1Based, FUN_08036CCC. Chamada por: tabela H (0x47).
- Memória: 0x03001800 (+0x87, +0x88, +0x8B, +0xA7, +0xAA, +0xCA, +0xD0); bloco do bot (+0xD6, +0x4F da peça).
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo "reparo da peça de maior dano": provável. Antes dizia só "+0xAA += 1":
  incompleto, o salto para 0x4A existe.

## 0x0801F474 Fight_DispatchOnSubstateAA_H_4D_JumpTo3E (H:0x4D)
- Assinatura: `void H_4D(void)`.
- O que faz: grava +0xAA = 0x3E (salto para H:0x3E). Não é "só +0xAA += 1" como estava antes.
- Memória: 0x03001800+0xAA (escrita).
- Para o Lua: porta trivial.
- Confiança: confirmado (corpo curto).

## 0x0801F4AC Fight_DispatchOnSubstateAA_I_00_ClassifyPartStateJump2 (I:0x00)
- Assinatura: `void I_00(void)`.
- O que faz: só age se fight +0xB7 bit 0 == 0 e +0xB8 bits 1 e 2 == 0. Pega o bot de +0x88; se o bloco do bot +0x59 ≠ 0
  e `Fight_HasNoTechnique16Part(bot)` ≠ 0, chama `FUN_08036D64(bot, byte de estado da peça +0x49)`. Se der 0,
  classifica o byte de estado: 0x0D/0x0E grava +0x51 = 0; 0x0F/0x10 grava +0x51 = 1; 0x11/0x12 grava +0x51 = 2;
  0x13/0x14 grava +0x51 = 3. Nos quatro casos +0xAA = 2 (salto). Fora disso, +0xAA += 1.
- Chama: Fight_HasNoTechnique16Part, FUN_08036D64. Chamada por: tabela I (0x00).
- Memória: 0x03001800 (+0x51 do bloco de combate, +0x87, +0x88, +0xA7, +0xAA, +0xB7, +0xB8); bloco do bot (+0x59, +0x49 da peça).
- Para o Lua: porta a classificação; o significado dos valores 0x0D..0x14 não foi confirmado.
- Confiança: confirmado (corpo lido). Passo "classifica estado da peça em 4 categorias e salta para 2": provável.
  Significado das categorias: chute.

## 0x0801F5AC Fight_DispatchOnSubstateAA_I_01_EndHandlerNoEffect (I:0x01; também I)
- Assinatura: `void I_01(void)`.
- O que faz: grava +0xB5 = 1 (handler done). Não toca em +0xAA nem em gráfico: é a mesma finalização de 0x0803080C
  (termina sem efeito).
- Memória: 0x03001800+0xB5 (escrita).
- Para o Lua: porta trivial.
- Confiança: confirmado (corpo de 1 instrução). Correção: a versão anterior falava em "só lê 0x03001800"; errado.

## 0x0801F5BC Fight_DispatchOnSubstateAA_I_02_LoadPartStateSpriteSetupSlot11 (I:0x02)
- Assinatura: `void I_02(void)`.
- O que faz: carrega um gráfico e paleta (`decompress_and_store` + `bios_cpufastset` 8) cuja escolha depende da
  categoria de estado da peça em +0x51 (valores 0..3, definidos por I:0x00): cat 1 e 0 e 2 usam gráficos
  diferentes; cat 3 usa um gráfico e uma paleta extra. Monta um OBJ em gObjectSlots (+0x280, x em +0x282, y em +0x284,
  +0x288 = 0x40, +0x29C = 0; atributos 0x1600) com X/Y do bot (+0x158/+0x15A ou +0x154/+0x156 conforme +0xF8 bit 1).
  Monta um segundo bloco (+0x2C0 = atributo, +0x2C2/+0x2C4 = X/Y, +0x2DC = 0) e chama
  `Slot_SetupFromRecordList(bloco+0x84, 0xB)` (prepara o slot 11). SFX 0x2F. +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, Slot_SetupFromRecordList, snd_sfx_request. Chamada por: tabela I (0x02).
- Memória: 0x03001800 (+0x51, +0xAA, +0xF8 do bot); gObjectSlots (+0x280..+0x2DC); slot 11.
- Para o Lua: porta (sprite de peça por categoria).
- Confiança: confirmado (corpo lido, longo). Passo "mostra sprite da peça conforme categoria e prepara o slot 11":
  provável. Antes: OBJ 0xC624 e 0x289/0x28A/0x28B/0x286 eram chute; agora os valores vêm do corpo.

## 0x0801F780 Fight_DispatchOnSubstateAA_I_03_AnimateSlot11ThenClear (I:0x03)
- Assinatura: `void I_03(void)`.
- O que faz: usa o byte de estado em +0x2A0 do bloco de combate. Se é 0: soma 4 a um byte de deslocamento (+DAT_7AC);
  quando ele chega a -0x80 põe +0x2A0 = 1. Se é 1: subtrai 4 de outro byte (+DAT_7EC), sem passar de 0x40.
  Depois roda o script de animação do slot 11 (`Anim_RunSlotScript`, ponteiro em +0x84) e, quando o byte de estado
  do slot é -1, chama `Tile_ClearSlots(10, 2)` (limpa os slots 10 e 11) e +0xAA += 1.
- Chama: Anim_RunSlotScript, Tile_ClearSlots. Chamada por: tabela I (0x03).
- Memória: 0x03001800 (+0x2A0, bytes de deslocamento, +0xAA); slot 11 (script) e slots 10-11 (limpeza).
- Para o Lua: porta (deslocamento por frame e espera de animação).
- Confiança: confirmado (corpo lido). Passo "desliza/anima o sprite do slot 11 e limpa ao fim": provável. Os dois
  bytes de deslocamento não têm endereço confirmado (DAT_7AC e DAT_7EC não resolvidos).

## 0x0801F800 Fight_DispatchOnSubstateAA_I_04_PlaceTwoPartStateSprites (I:0x04)
- Assinatura: `void I_04(void)`.
- O que faz: calcula dois pontos a partir do X/Y do bot (esquerda: X-4 e Y com +0x158/+0x15A; direita: X+4 com
  +0x154/+0x156; a escolha segue +0xF8 bit 1). Carrega o gráfico e a paleta conforme a categoria da peça (+0x51 de
  0x03001800, mesma tabela de I:0x02). Cria dois OBJs em gObjectSlots: um em X-0x10/Y-8 (atributo 0x1700) e outro em
  X+0x10/Y+8 (atributo com 0xE e 0x80 em +0x28B). Grava +0x2C0 e +0x2DC. +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset. Chamada por: tabela I (0x04).
- Memória: 0x03001800 (+0x51, +0xAA); bloco do bot (+0xF8, +0x154..+0x15A); gObjectSlots (+0x280..+0x2DC).
- Para o Lua: porta (dois sprites de peça, um de cada lado).
- Confiança: confirmado (corpo lido, longo). Passo "dois sprites da peça (esq./dir.)": provável. Antes era "maior
  handler, 39 linhas, parcial"; agora o corpo está lido e o tamanho bate.

## 0x0801F9B0 Fight_DispatchOnSubstateAA_I_05_WaitSlotsThenTimer90 (I:0x05)
- Assinatura: `void I_05(void)`.
- O que faz: chama `FUN_08036DE8(10, 0)` e depois `FUN_08036DE8(11, 1)`. Se o segundo devolve ≠ 0: toca SFX 0x2A, grava
  timer 90 (0x5A) em 0x03000440+0x86 e +0xAA += 1. Senão espera.
- Chama: FUN_08036DE8 (duas vezes), snd_sfx_request. Chamada por: tabela I (0x05).
- Memória: 0x03000440+0x86 (escrita); 0x03001800+0xAA.
- Para o Lua: porta (espera de slots).
- Confiança: confirmado (corpo curto). Passo "espera os slots 10 e 11 e dá pausa de 90 quadros": provável.

## 0x0801F9EC Fight_DispatchOnSubstateAA_I_07_ApplyPartStateEffectAtXY (I:0x07)
- Assinatura: `void I_07(void)`.
- O que faz: chama `FUN_08036E64(+0x88 do lado, byte de estado da peça (+0xA7*0x18 + +0x87*0x100 + bloco do bot + 0x49),
  halfword +0xE8, halfword +0xEC)` e +0xAA += 1. Aplica o efeito de estado da peça ativa na posição (+0xE8, +0xEC).
- Chama: FUN_08036E64. Chamada por: tabela I (0x07).
- Memória: 0x03001800 (+0x87, +0x88, +0xA7, +0xAA, +0xE8, +0xEC); bloco do bot (+0x49 da peça).
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo "efeito de estado de peça na posição": provável.

## 0x0801FA68 Fight_DispatchOnSubstateAA_J_00_PreviewAdvanceSideSequences (J:0x00)
- Assinatura: `void J_00(void)`.
- O que faz: chama `FightPreview_UpdateSingleLayerSideSprites`, avança a sequência lateral esquerda
  (`FightPreview_AdvanceLeftSideSequenceStep`, resultado em +DAT_fac4) e a direita (`FightPreview_AdvanceRightSideSequenceStep`,
  resultado em +0x160). Zera um byte em +0x84 de um bloco auxiliar e limpa duas áreas com `bios_cpufastset` (fonte
  zero e um valor local). +0xAA += 1.
- Chama: FightPreview_UpdateSingleLayerSideSprites, FightPreview_AdvanceLeftSideSequenceStep,
  FightPreview_AdvanceRightSideSequenceStep, bios_cpufastset. Chamada por: tabela J (0x00).
- Memória: 0x03001800 (+0x160, +0xAA, +DAT_fac4); bloco auxiliar (+0x84); áreas limpas por DMA-fill.
- Para o Lua: porta (prévia de sprites laterais).
- Confiança: confirmado (corpo lido). Passo "avança a prévia dos dois lados": provável.
  Correção: a versão antiga falava em carga de tiles de BG (VRAM 0x06004120); não aparece no corpo.

## 0x0801FADC Fight_DispatchOnSubstateAA_J_01_PreviewSideStepSprites (J:0x01)
- Assinatura: `void J_01(void)`.
- O que faz: mesmo começo de J:0x00 (atualiza sprites laterais e avança as duas sequências). Depois chama duas vezes
  `FUN_0803F15C(0x1E, 1, 0, pos, *ptr, *ptr2, 0)` com posições espelhadas (pos e 0x13-pos). O contador de passo em
  +0x84 de um bloco auxiliar vai de 0 a 9: se é 9, limpa 0x200 bytes com `bios_cpufastset`, zera dois bytes e +0xAA += 1;
  senão incrementa o contador.
- Chama: FightPreview_UpdateSingleLayerSideSprites, FightPreview_Advance*SideSequenceStep, FUN_0803F15C, bios_cpufastset.
  Chamada por: tabela J (0x01).
- Memória: 0x03001800 (+0xAA, +0x160); bloco auxiliar (+0x84, +0x1A4); área de 0x200 bytes limpa.
- Para o Lua: porta (sequência de 10 passos da prévia).
- Confiança: confirmado (corpo lido). Passo "sequência de 10 quadros da prévia dos dois lados": provável.
  Correção: a versão antiga dizia que chama NoOpActionHandler (0x08000841); isso não aparece no corpo.

## 0x0801FBC4 Fight_DispatchOnSubstateAA_J_02_SubDispatchByStep (J:0x02)
- Assinatura: `void J_02(void)`.
- O que faz: é um despachante secundário, não um handler. Lê um byte de estado (DAT_fbe0 + DAT_fbe4). Se é > 4, salta
  por um ponteiro que o Ghidra não recuperou (0x0801FD70). Senão salta pela tabela de 5 entradas (0..4) em
  `PTR_switchdata_0801FBE8`. Não grava +0xAA: quem faz isso são os alvos.
- Chama: alvos não recuperados (5 + 1). Chamada por: tabela J (0x02).
- Memória: byte de estado (DAT_fbe0 + DAT_fbe4, não mapeado).
- Para o Lua: porta quando os alvos forem conhecidos.
- Confiança: confirmado (despachante). Alvos: não resolvidos (pendência). Correção: a versão antiga dizia que grava
  0x1901/0x1A0A/0x1B0B (tilemap); isso estava nos alvos, não no despachante.

## 0x0801FD88 Fight_DispatchOnSubstateAA_J_03_PreviewSideStepCounter (J:0x03)
- Assinatura: `void J_03(void)`.
- O que faz: chama duas vezes `FUN_0803EF8C(0x1E, 1, 0, pos, *ptr, 0)`, com posições 9-passo e passo+10 (espelhadas).
  O contador em +0x84 de um bloco auxiliar: se é 9, +0xAA += 1; senão +1.
- Chama: FUN_0803EF8C. Chamada por: tabela J (0x03).
- Memória: bloco auxiliar (+0x84); 0x03001800+0xAA.
- Para o Lua: porta (mesma sequência de J:0x01, com outra rotina de desenho).
- Confiança: confirmado (corpo lido). Passo "parte da sequência da prévia": provável.
  Correção: a versão antiga dizia que mexia em registrador 0x04000008; isso não aparece aqui.

## 0x0801FE14 Fight_DispatchOnSubstateAA_K_00_ConsumePendingBotRefresh (K:0x00)
- Assinatura: `void K_00(void)`.
- O que faz: percorre 3 bytes de pendência em 0x03001800+0xE0..0xE2. No primeiro com valor ≠ 0: zera o byte, grava
  +0x51 = valor-1 (índice do bot ativo), chama `Fight_RefreshDisplayedPartSpritesForActiveBots`,
  `Fight_PlaceActiveBotObjects` e `FUN_08036FA0(+0x51)`, e põe +0xAA = 2. Se nenhum tem pendência: +0xAA += 1.
- Chama: Fight_RefreshDisplayedPartSpritesForActiveBots, Fight_PlaceActiveBotObjects, FUN_08036FA0.
  Chamada por: tabela K (0x00).
- Memória: 0x03001800 (+0x51, +0xAA, +0xE0..+0xE2).
- Para o Lua: porta (fila de bots pendentes e atualização de sprites).
- Confiança: confirmado (corpo lido). Passo "consome o próximo bot pendente e atualiza os sprites": provável.

## 0x0801FE64 Fight_DispatchOnSubstateAA_K_02_LoadGfxPlaceSprite21 (K:0x02)
- Assinatura: `void K_02(void)`.
- O que faz: carrega gráfico e paleta (`decompress_and_store` + `bios_cpufastset` 8). Lê um par de coordenadas de uma
  tabela em 0x1740 + (+0x51)*0x100 do bloco de combate e chama `FUN_08036FF8(0x21, x, y-4, 8)`, que põe o sprite 0x21
  na tela. +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, FUN_08036FF8. Chamada por: tabela K (0x02).
- Memória: 0x03001800 (+0x51, +0xAA) e tabela de posições em 0x03002F40 + cat*0x100 (provável; não confirmado pelo literal).
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo "mostra o sprite 0x21 na posição da tabela": provável.
  Correção: a versão antiga dizia gráfico 0x084CF178 e VRAM 0x06014800; os literais não foram rechecados aqui.

## 0x0801FEC8 Fight_DispatchOnSubstateAA_K_03_WaitSprite21Ready (K:0x03)
- Assinatura: `void K_03(void)`.
- O que faz: chama `FUN_080370AC(0x21, 8)`. Se devolve ≠ 0, +0xAA += 1. Senão espera.
- Chama: FUN_080370AC. Chamada por: tabela K (0x03).
- Memória: 0x03001800+0xAA.
- Para o Lua: porta (espera).
- Confiança: confirmado (corpo curto). Passo "espera o sprite 0x21 ficar pronto": provável.

## 0x0801FEE8 Fight_DispatchOnSubstateAA_K_04_LoadGfxSetupSlot21 (K:0x04)
- Assinatura: `void K_04(void)`.
- O que faz: carrega gráfico e paleta (helpers de K:0x02). Grava um halfword em 0x03001800+0x840 e um byte de
  estado. Lê X/Y de uma tabela (+0x1742 e +0x1744 com cat*0x100, mesma família de K:0x02, com -4 em Y). Grava
  byte +8 = 7 e +0xF0 = 0xF0. Chama `Slot_SetupFromRecordList(bloco+0x88, 0x21)` (prepara o slot 0x21). +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, Slot_SetupFromRecordList. Chamada por: tabela K (0x04).
- Memória: 0x03001800 (+0x51, +0x840, +0xAA, +0xF0); tabela de posições (cat*0x100 + 0x1742/0x1744).
- Para o Lua: porta (prepara sprite de peça).
- Confiança: confirmado (corpo lido). Passo "prepara o slot 0x21 com o sprite da peça": provável.

## 0x0801FF9C Fight_DispatchOnSubstateAA_K_05_WaitSlot21ClearAttrMasks (K:0x05)
- Assinatura: `void K_05(void)`.
- O que faz: roda o script de animação do slot 0x21 (`Anim_RunSlotScript`, ponteiro em +0x88 de DAT_FFF4). Quando o
  byte de estado do slot é -1: `Tile_ClearSlots(0x21, 1)`, depois `Slot_ClearAttrMask(0x5A + 4*cat, 4)` e
  `Slot_ClearAttrMask(0x72 + cat, 1)` (cat = +0x51 do bloco de combate). +0xAA += 1. Senão espera.
- Chama: Anim_RunSlotScript, Tile_ClearSlots, Slot_ClearAttrMask (2x). Chamada por: tabela K (0x05).
- Memória: slot 0x21 (script/estado); máscaras de atributo de slots (0x5A+4*cat e 0x72+cat); 0x03001800 (+0x51, +0xAA).
- Para o Lua: porta (limpa sprites da peça).
- Confiança: confirmado (corpo lido; a conta de índice é 0x5A + 4*cat, vista como (cat<<26 + 0x5A000000)>>24).
  Passo "fim da animação do slot 0x21 e limpeza": provável.

## 0x08020004 Fight_DispatchOnSubstateAA_K_06_QueueMessageCode14 (K:0x06)
- Assinatura: `void K_06(void)`.
- O que faz: chama `FUN_0803F63C(0x03001800 + cat*0x100 + 0x..., DAT_0802003C, 0xE)` (entrada de tabela por lado, cat =
  +0x51) e grava +0xA6 = 0x14 (código de passo/mensagem, ver HANDOFF: +0xA6 = step/message code). +0xAA += 1.
- Chama: FUN_0803F63C. Chamada por: tabela K (0x06).
- Memória: 0x03001800 (+0x51, +0xA6, +0xAA); tabela por lado (cat*0x100 + DAT_08020038, não resolvido).
- Para o Lua: porta (disparo de mensagem).
- Confiança: confirmado (corpo lido). Passo "dispara a mensagem de código 0x14": provável.
  Correção: a versão antiga dizia que lê 0x0300059A e 0x030004DE; isso não aparece no corpo.

## 0x08020040 Fight_DispatchOnSubstateAA_K_08_ResetStepToZero (K:0x08)
- Assinatura: `void K_08(void)`.
- O que faz: grava +0xAA = 0 (volta ao passo 0 da fase K).
- Memória: 0x03001800+0xAA (escrita).
- Para o Lua: porta trivial.
- Confiança: confirmado (corpo curto).

## 0x08020078 Fight_DispatchOnSubstateAA_L_00_CheckFlagF6JumpTo2 (L:0x00)
- Assinatura: `void L_00(void)`.
- O que faz: se o bit 0 de +0xF6 (bloco de combate) é 0: grava +0xAB = (+0xF6 & 1) = 0 e +0xAA += 1. Se o bit é 1:
  +0xAA = 2 (salto). Não toca em gráfico.
- Memória: 0x03001800 (+0xF6, +0xAA, +0xAB).
- Para o Lua: porta trivial.
- Confiança: confirmado (corpo curto). Correção: a versão anterior dizia "+0xAA += 1 e +0xAB += 1"; o +0xAB recebe 0 e o
  salto para 2 existe.

## 0x080200A8 Fight_DispatchOnSubstateAA_L_02_LoadTwoGfxSetupSlot11 (L:0x02)
- Assinatura: `void L_02(void)`.
- O que faz: carrega dois pares gráfico+paleta (`decompress_and_store` + `bios_cpufastset` 8, duas vezes). Pega X/Y do
  bot (+0x158/+0x15A ou +0x154/+0x156 conforme +0xF8 bit 1). Monta três blocos em gObjectSlots (+0x280 com atributo
  0x1300; +0x282/+0x284 = X/Y; atributos 0x0E e 0x80; um segundo OBJ com 0x2C00 em +DAT_201D0; e campos auxiliares
  +0x15/+0x17/+0x19/+0x31). Chama `Slot_SetupFromRecordList(bloco+0xDC, 0x0B)` (slot 11), SFX 0x53, +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, Slot_SetupFromRecordList, snd_sfx_request. Chamada por: tabela L (0x02).
- Memória: gObjectSlots (+0x280..+0x2DC aprox.); bloco do bot (+0xF8, +0x154..+0x15A); slot 11; 0x03001800+0xAA.
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo "prepara o sprite do slot 11 (dois gráficos)": provável.

## 0x080201DC Fight_DispatchOnSubstateAA_L_03_SlideSlot11ThenClear (L:0x03)
- Assinatura: `void L_03(void)`.
- O que faz: roda o script do slot 11 (`Anim_RunSlotScript`, ponteiro em +0xDC). Lê um byte de marca (base + DAT_08020214).
  Se é '(' (0x28): `Tile_ClearSlots(11, 1)` e +0xAA += 1. Senão: +0x2AC += 1 e, em contador ímpar, o sprite sobe 1 px
  (+0x284 -= 1) e a marca avança 1. Ou seja, desliza devagar até a marca.
- Chama: Anim_RunSlotScript, Tile_ClearSlots. Chamada por: tabela L (0x03).
- Memória: slot 11; byte de marca (não resolvido); 0x03001800 (+0x2AC, +0xAA); gObjectSlots+0x284.
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo "desliza o sprite do slot 11 até a marca e limpa": provável.

## 0x08020248 Fight_DispatchOnSubstateAA_L_04_CommitActionIfState3 (L:0x04)
- Assinatura: `void L_04(void)`.
- O que faz: se +0xB7 == 3: chama `Fight_CommitActionState()`, `Tile_ClearSlots(10, 1)`, grava +0xAB = 1 e +0xAA += 1.
  Senão: +0xAA = 6 (salto).
- Chama: Fight_CommitActionState, Tile_ClearSlots. Chamada por: tabela L (0x04).
- Memória: 0x03001800 (+0xB7, +0xAA, +0xAB).
- Para o Lua: porta (confirma a ação quando o estado é 3).
- Confiança: confirmado (corpo curto). Passo "confirma a ação se +0xB7 == 3": provável. Antes: "escreve +0xAB e +0xAA, parcial".

## 0x08020288 Fight_DispatchOnSubstateAA_L_06_LoadGfxResetCounterSfx (L:0x06)
- Assinatura: `void L_06(void)`.
- O que faz: carrega gráfico e paleta (`decompress_and_store` + `bios_cpufastset` 8). Zera o byte de estado em
  0x03001800+DAT_080202D8 e +0x2AC. Toca SFX 0x2F. +0xAA += 1.
- Chama: decompress_and_store, bios_cpufastset, snd_sfx_request. Chamada por: tabela L (0x06).
- Memória: 0x03001800 (+0x2AC, +0xAA, byte de estado); gráfico/paleta por literal (não resolvido aqui).
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo "carrega o gráfico e reinicia o contador": provável. Antes: gráfico
  0x084CB9C8 e OBJ 0x2AB; o gráfico pode coincidir com H:0x28, mas não foi checado.

## 0x080202E0 Fight_DispatchOnSubstateAA_L_07_SlideUntilMarker0x28ThenTimer60 (L:0x07)
- Assinatura: `void L_07(void)`.
- O que faz: lê o byte de marca (base + DAT_08020310). Se é '(' (0x28): timer 60 em 0x03000440+0x86, +0xAB = 0 e +0xAA += 1.
  Senão: +0x2AC += 1, sprite (+0x284) += 1 e a marca avança 1. Desliza para baixo até a marca.
- Chama: nada. Chamada por: tabela L (0x07).
- Memória: byte de marca (não resolvido); gObjectSlots+0x284; 0x03001800 (+0x2AC, +0xAA, +0xAB); 0x03000440+0x86.
- Para o Lua: porta.
- Confiança: confirmado (corpo lido). Passo "desliza até a marca e espera 60": provável. Par de L:0x03 (sobe, e
  aqui desce).

## 0x08020340 Fight_DispatchOnSubstateAA_L_09_BranchOnB8Value (L:0x09)
- Assinatura: `void L_09(void)`.
- O que faz: desvio pelo valor de +0xB8 (bloco de combate): se é 2, +0xAA = 0x0B; se é 4, +0xAA = 0x0F; senão +0xAA += 1.
- Chama: nada. Chamada por: tabela L (0x09).
- Memória: 0x03001800 (+0xAA, +0xB8).
- Para o Lua: porta trivial.
- Confiança: confirmado (corpo curto). Significado dos valores 2 e 4 de +0xB8: chute.

## 0x0802036C Fight_DispatchOnSubstateAA_L_0B_LoadGfxSetupSlot10ThenJump (L:0x0B)
- Assinatura: `void L_0B(void)`.
- O que faz: carrega gráfico e paleta (dois helpers). Pega X/Y do bot (mesmo par de L:0x02). Grava um OBJ em
  gObjectSlots+0x280 (atributo em DAT_08020414) com X/Y, atributos 0x0E e 0x80. Chama
  `Slot_SetupFromRecordList(bloco+0x94, 10)` (slot 10), toca SFX 0x37 e +0xAA += 1. Depois salta por um ponteiro que o
  Ghidra não recuperou (0x0802040C). Pendência: alvo desse salto.
- Chama: decompress_and_store, bios_cpufastset, Slot_SetupFromRecordList, snd_sfx_request, ponteiro não recuperado.
  Chamada por: tabela L (0x0B).
- Memória: gObjectSlots+0x280..; bloco do bot (+0xF8, +0x154..+0x15A); slot 10; 0x03001800+0xAA.
- Para o Lua: porta, depois de achar o alvo do salto.
- Confiança: confirmado (corpo lido). Passo "prepara o sprite no slot 10": provável. Antes: "OBJ 0xC404 e dado
  0x083B6100" não se confirma aqui (0x083B6100 não aparece).

## 0x08020428 Fight_DispatchOnSubstateAA_L_0C_WaitSlot10AnimThenTimer60 (L:0x0C)
- Assinatura: `void L_0C(void)`.
- O que faz: roda o script do slot 10 (`Anim_RunSlotScript`, ponteiro em +0x94). Quando o byte de estado (DAT_08020464 +
  DAT_08020468) é -1: grava timer 60 em 0x03000440+0x86, `Tile_ClearSlots(10, 1)` e +0xAA += 1. Senão espera.
- Chama: Anim_RunSlotScript, Tile_ClearSlots. Chamada por: tabela L (0x0C).
- Memória: slot 10 (script e estado); 0x03000440+0x86; 0x03001800+0xAA.
- Para o Lua: porta.
- Confiança: confirmado (corpo curto). Passo "espera a animação do slot 10 e dá 60 quadros": provável. Antes:
  "dado 0x083B6100; OBJ 0x2A2" não se confirma aqui.

## Esboço da máquina de estados (parcial)
- Despachante = fase (letra). `+0xAA` = passo dentro da fase. Cada handler avança +1, ou grava valor fixo.
- H: apresentação com gráfico e OBJ, e uma espera (H:0x29). I, K, L: mistura de gráfico, OBJ e passos vazios.
  J: BG/tilemap (0x04000008). Nas fases H..L, passos vazios (só +0xAA += 1) são comuns.
- Só H tem chamador conhecido (0x080341DE). Qual evento escolhe a fase e o passo fica em aberto.

## Em aberto
- Feito no redo: 42 funções decompiladas e renomeadas. Saltos indiretos não recuperados: H:0x3E (0x0801F28A), J:0x02 (0x0801FBC4, alvos 0..4 e fallback 0x0801FD70), L:0x0B (0x0802040C).
- Nomes dos helpers listados acima: não estão no mtbe.csv.
- Passo real da Robattle (ataque, dano, troca, fim): não decifrado. Ver pc/research/R3_battle_state.md.
