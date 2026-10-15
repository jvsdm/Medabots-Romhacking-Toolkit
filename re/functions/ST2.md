# GH-ST2 (passos de cena, continuação de ST.md)

Leitura só da ROM original (`Medabots - Metabee (Europe).gba`), disassembly Thumb com capstone (python -I), só leitura.
Ghidra: 0x0800EB34 e 0x0803C0CC não têm função; 0x0801013C, 0x0803BE44, 0x0803C33C e 0x0803C680 têm (nomes do
Ghidra conferidos com o corpo). Nada foi salvo no Ghidra.

## 1. Caller 0x0800EB34 e cena (menu 5)

Corpo (push {lr}; sem função no Ghidra; confirmado no disasm):

    bl 0x0800EBFC            ; Sprite_IncrementXYIfFlag
    bl 0x0800F440            ; PartsGrid_DrawLargeDisplayIfOwned
    bl 0x0801013C            ; PartDetail_DispatchModeHandler (ACE14), retorna 1 se modo == 3
    if r0 != 0:
        Slot_ClearAttrMask(4, 1)      ; 0x08040D0C
        [0x0300137C] += 1             ; avança o passo da cena
    pop {pc}

Como a função é alcançada (confirmado no código):
- Não há BL para 0x0800EB34. Ela é a entrada 66 da tabela de passos **0x083ACCD4** (72 ponteiros, fim 0x083ACDF0):
  0x083ACCD4 + 4*66 = 0x083ACDDC = 0x0800EB35. A entrada 67 (0x083ACDE0) é 0x0800EB61 (outra função).
- O despachante é **0x0800D9DC** (menu_list[5]: `menu_list` 0x083AC2C8 +5 = 0x0800D9DD, literal em 0x083AC2DC).
  Ele roda `Battle_CheckExitShortcut` (0x0803EED8); se não sair, se `[0x03000430] == 2` faz DMA de limpeza, e depois
  `CallViaR0(0x083ACCD4[ [0x0300137C] ])` (literais 0x0800DA2C = 0x083ACCD4 e 0x0800DA30 = 0x0300137C).
- Vizinhos na mesma tabela (confirmados por nome ou corpo): entrada 0 = 0x0800DA35 `MedabotGrid_InitScreen`
  (tela de grade dos Medabots); entrada 64 = 0x0800EA2D `PartDetail_SceneStep` (tela de detalhe de peça). A 66 chama
  as mesmas rotinas de sprite/grade do passo de detalhe.
- Conclusão: **menu 5 = Medawatch/MEDABOTS/peças; passo 66 = passo da tela de detalhe de peça (PartDetail)**, que
  roda o modo `0x083ACE14[ byte ]` até o modo 3. Confiança: provável (nome e vizinhança; o texto da tela não foi
  comparado com o emulador).

## 2. Byte 0x03002D59 e leitor 0x0801013C (PartDetail_DispatchModeHandler)

    push {r4,lr}
    r1 = 0x083ACE14 ; r4 = 0x03002C30 + 0x129 = 0x03002D59
    CallViaR0( table[ *(u8*)0x03002D59 ] )      ; LSL 2, table = ACE14 (148 ponteiros)
    return ( *(u8*)0x03002D59 == 3 )            ; 3 = modo concluído

- 0x03002D59 = `gObjectSlots` (0x03002C30) + 0x129 = **slot 4 (0x03002D30), campo +0x29**. Uma linha de
  `re/symbols/mtbe.csv` diz que +0x29 é o byte do passo de script de objeto (0x080393EC, handler por 0x083AF8C0);
  então o byte é o modo (0..3) do passo da tela de detalhe. Provável: o byte é escrito pelos handlers do próprio
  passo (não achei escritor por literal; é acesso base+offset). Confiança: provável.
- Tabela ACE14: índices 0..2 = 0x08010170, 0x080101E8, 0x0801022C (passos); índice 3 = 0x08000841 (vazio: o valor 3
  é só o fim). 0x083ACE24 (= ACE14+0x10) é usada por menu 6 (ver seção 3).

## 3. 0x08010264 = menu 6 (subvetor ACE24)

    push {lr}
    if [0x03000430] == 2: DMA3 de 0x200 halfwords (fill) (0x040000D4; valor em [sp])
    CallViaR0( 0x083ACE24[ [0x0300137C] ] )    ; literais 0x080102B8 = 0x083ACE24, 0x080102BC = 0x0300137C
    pop {pc}

- Confirmado no código. Não tem função no Ghidra (menu 6 = encontro, MN.md). A tabela ACE24 começa em ACE14[4]
  (0x080102C0 = passo 0 da cena, que faz `orr` 0x40 em 3 halfwords de tilemap). Ver BT.md (menu 6).
- O mesmo bloco ACE14 é usado pela tela de detalhe (menu 5, seção 2) e pelo menu 6 (ACE24): um só vetor com
  sub-tabelas em offsets diferentes. Confiança: confirmado (leitura); a cena (encontro x detalhe) é provável.

## 4. Dono do bloco AFC78 e do bloco AFC2C (leitores)

Literais conferidos (valor na ROM): 0x0803C734 = 0x083AFC38 (NÃO é AFC78; é o leitor de AFC38); 0x0803D59C = 0x083AFC78;
0x0803D744 = 0x083AFDF8 (fim do bloco, sem leitor); 0x0803BEC8, 0x0803C170, 0x0803C3E0 = 0x083AFC2C.

- **0x083AFC78 (96 palavras)** = tabela de passos do menu 10 (Link): função **0x0803D538** (`Scene_GsDispatch_Menu10_Link`,
  menu_list[10] = 0x0803D539). Ldr em 0x0803D570: `CallViaR0( table[ *(0x0300137C) ] )` (r4 = 0x0300137C, literal
  em 0x0803D588). Ghidra: função desde o sync de 2026-10-14 em 0x0803D538 (Scene_GsDispatch_Menu10_Link) (corpo: 0x0803D538..0x0803D584). Confirmado.
- **0x083AFC2C (12 bytes, u8)** = tabela de bytes lida por três funções de combate/scroll, com índice
  `*(u8*)(0x03001800 + 0x38)` (gFightInformation +0x38). O resultado vira o slot de objeto usado (gObjectSlots +
  gCurrentObjectSlot*0x40, 0x0300056F). Donos:
  - 0x0803BE44 **Fight_BotSlideScroll** (Ghidra, corpo 0x0803BE44..0x0803C041; ldr em 0x0803BE4E e literal 0x0803BEC8).
  - 0x0803C0CC: função sem nome no Ghidra (início 0x0803C0CC, prólogo igual ao de 0x0803C33C; literal em 0x0803C170).
    Nome sugerido `Fight_ScrollSlotPhase_C0CC`. Provável.
  - 0x0803C33C **Fight_ScrollEntryPhase** (Ghidra, corpo 0x0803C33C..0x0803C5EB; literal em 0x0803C3E0).
  Confirmado: leitura de AFC2C com fight+0x38 nos três; significado do valor (qual bot/slot) provável.

## 5. Tabela AFC38 (lida por 0x0803C680) — bônus

- 0x083AFC38 = 32 halfwords (0x40 bytes, termina em AFC78): `Anim_SlotScriptTick` (0x0803C680, Ghidra) lê
  `wave = AFC38[ slot+0x23 & 0x1F ]` e soma em `slot+0x04` (X do slot, FH.md). Ou seja: onda de deslocamento por
  slot (balanço). Confirmado no código; nome 'onda' é provável.

## 6. Pendências

- Quem grava 0x03002D59 (slot 4 +0x29) e qual evento põe 0x0300137C = 66 (entrada da tela de detalhe). Não lido;
  precisaria do script de objetos ou de um watch no emulador (fora deste passo).
- 0x0800EB34 (SceneStep_Caller_ACE14) e 0x0803C0CC (Fight_ScrollSlotPhase_C0CC) são funções desde o sync de 2026-10-14: criar função no Ghidra fica para o MedabotsSync (ver MN.md).
- Confirmar no emulador (rec de tela de detalhe de peça) que 0x0300137C = 66 corresponde à tela de detalhe.

Símbolos: `ST2_simbolos.csv`. Lista de endereços sem função: `ST2_lista.txt`.
