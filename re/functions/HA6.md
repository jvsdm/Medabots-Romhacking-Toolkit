# HA6: handlers dos despachantes Fight_DispatchOnSubstateAA_Z, _AA, _AB, _AC, _AD, _AE

Lista: `HA6_lista.txt` (42 linhas: endereço, despachante, índice = valor de +0xAA, e na coluna 4 outros
despachantes que aparecem com o mesmo endereço na lista; essa coluna NÃO foi verificada na ROM).
Bloco de estado: `0x03001800` (literal do pool). Subestado = byte +0xAA. Cada despachante chama
`tabela[+0xAA]` (trampolim `0x0807CCF0`); cada handler faz +0xAA += 1 ao terminar (passo seguinte),
ou fica no mesmo índice enquanto espera.
Literais usados pelos handlers: `0x03000440` (+0x84 = contador de passo, +0x86 = temporizador halfword),
`0x03000590` (bloco do bot, base + bot*0x100), `0x03002C30` (gObjectSlots; slot 0x21 = objeto da animação).
Dado = endereço ou valor de tabela, não código.

Ghidra: 23 handlers são funções definidas e foram renomeadas (`Fight_DispatchOnSubstateAA_<D>_<Passo>`).
19 endereços NÃO têm função no Ghidra (código Thumb real na ROM, mas sem análise). Foram lidos à mão
com um disassembler Thumb de rascunho; sem renomear. Ver "Não definidos" abaixo.

## Máquina de estados (esboço, por despachante)

- Z (0x06..0x0F): contador de passo 0x84 sobe/desce até 0 ou 0x10; efeitos de tile/scroll; ganho de
  gauge com teto 0xA0; ajuste de slots. Z/0F-0E-0D-0B: espera e decaimento de slots.
- AA (0x00..0x0B): 00 = cálculo de ROS da peça; 04 = carga de gráficos + init de slot; 05-0A = animação do
  slot 0x21 com espera de fim de script, sfx 0x4D, contador 0x84 e preview; 0B = seta A6=0x1D.
- AB (0x00..0x0F): 00 = cálculo de ROS; 08 = carga de gráficos + slot; 09-0C = espera (animação, timer,
  flag); 0A = move slot até o alvo; 0B = slot + sfx 0x4E; 0D/0E = passos de fade e cópia de valores;
  0F = seta A6=0x2C.
- AC (0x00..0x0F): 00 = ROS; 08 = gráficos + init; 0B = slot + sfx 0x4E (cor 0x46); 0E = lê registro do
  bot (não concluído); 0F = seta A6=0x2B.
- AD (0x00..0x16): 00 = ROS; 08 = gráficos + init; 0B = slot + sfx (0x44); 0D/0E/0F = fade, carga
  de gráficos e escrita de linhas do bot; 12/15/16 = slot + sfx (0x43), escrita de linhas, seta A6=0x2D.
- AE (0x00, 0x08): 00 = ROS; 08 = gráficos + init.

## Despachante Z (stub 0x0801C4FC-like, tabela de 0x06..0x0F)

## 0x080230C8 Fight_DispatchOnSubstateAA_Z_TickCounterUp (índice 0x06)
- Passo: incrementa contador 0x84 de 0x03000440 com `FUN_0803f684`/`FUN_0803f998` (efeito por contador) até 0x10; então +0xAA.
- Lê/escreve: 0x03000440+0x84 (escreve); 0x03001800+0xAA (escreve). Confiança: confirmado (decompile); funções chamadas sem nome.

## 0x08023118 (índice 0x07) NÃO DEFINIDO
- Passo: init de 3 entradas em gObjectSlots (0x03002C30+0x840) com halfword 0xC604 e cópia de dados do bot (0x03000590 + 0x100*bot, +0x1742). Lê código Thumb à mão; parcial. Confiança: chute.

## 0x080231D4 Fight_DispatchOnSubstateAA_Z_TileClearOrScroll (índice 0x08)
- Passo: se o byte em (base+0x...) == '@' limpa slots 0x21 (2) e +0xAA; senão soma 0x10 a um halfword (scroll) e avança o byte.
- Lê/escreve: 0x03001800+0xAA; halfword do bloco de base (0x031D?) via `DAT_0802321c`. Confiança: confirmado.

## 0x08023224 Fight_DispatchOnSubstateAA_Z_TickCounterDown (índice 0x09)
- Passo: contador 0x03000440+0x84 desce até 0 (efeito por contador com `FUN_0803f684`/`FUN_0803f998`), então +0xAA.
- Lê/escreve: 0x03000440+0x84; 0x03001800+0xAA. Confiança: confirmado.

## 0x08023274 NÃO DEFINIDO (índice 0x0A)
- Passo: chama `Fight_GetAllySideBaseIndex` (0x08032FC4) com o lado do ator; laço de 3 bots copiando halfwords de 0x03000590 para gObjectSlots+0x840. Parcial. Confiança: chute.

## 0x08023328 Fight_DispatchOnSubstateAA_Z_WaitAndDecaySlots (índice 0x0B)
- Passo: temporizador halfword 0x03000440+0x86: se 0x14 seta 0x3C e +0xAA; senão sobe e decrementa +4 dos slots com bit 0x8000 (gObjectSlots+0x840, passo 0x40).
- Lê/escreve: 0x03000440+0x86; gObjectSlots+0x840..; 0x03001800+0xAA. Confiança: confirmado.

## 0x08023384 NÃO DEFINIDO (índice 0x0D)
- Passo: laço de 3 slots (gObjectSlots+0x840, passo 0x40): se o halfword tem bit 0x0080 ajusta bytes +8/-8; ao fim, se byte 0x848 == 8 chama `ClearObjectAttributeEntries` (0x08040CD0, args 0x21, 3) e +0xAA. Parcial. Confiança: provável.

## 0x080233E0 Fight_DispatchOnSubstateAA_Z_AddGaugeMax160 (índice 0x0E)
- Passo: `Fight_AddByteClampToMax` soma (short bot+0xEC) ao byte de gauge (bot+0x89*8+0x28), teto 0xA0; +0xAA.
- Lê/escreve: 0x03001800+0x89, +0xEC, +0x28..; +0xAA. Confiança: confirmado.

## 0x08023414 NÃO DEFINIDO (índice 0x0F)
- Passo: escreve byte 0x1C em 0x03001800+0xA6 e +0xAA += 1. Confiança: confirmado (leitura à mão, curto).

## Despachante AA (índices 0x00..0x0B)

## 0x08023454 NÃO DEFINIDO (AA 0x00)
- Passo: cálculo de ROS da peça: chama `Fight_GetPartProficiencyBase` (0x0803579C), `Fight_ApplyVirusBugRosPenalty` (0x08035814), `Fight_CheckRosMeetsMinimumThreshold` (0x0803725C); grava resultado em 0x03001800. Confiança: provável.

## 0x080234A8 NÃO DEFINIDO (AA 0x04)
- Passo: carga de gráficos (`decompress_and_store` 0x080009BC, `CPUFastSet` 0x0807CCC0) e init de slot (gObjectSlots+0x840 = 0xC404; +0x85C = 0). Confiança: provável.

## 0x08023564 Fight_DispatchOnSubstateAA_AA_WaitScriptEnd (AA 0x05)
- Passo: `Anim_RunSlotScript` (slot 0x21); se o byte de fim == -1: timer 0x86=0x3C e +0xAA.
- Lê/escreve: 0x03000440+0x86; 0x03001800+0xAA. Confiança: confirmado.

## 0x080235AC Fight_DispatchOnSubstateAA_AA_SetupSlotSfx4D (AA 0x07)
- Passo: `Slot_SetupFromRecordList` (slot 0x21, registro +0x104), sfx 0x4D (`snd_sfx_request`), zera +0x84; +0xAA.
- Confiança: confirmado.

## 0x080235E8 Fight_DispatchOnSubstateAA_AA_AnimCounterStep (AA 0x08)
- Passo: contador 0x84 += 1; `FightPreview_SetGroupActive4`; `Anim_RunSlotScript`; no fim do script: `Tile_ClearSlots(0x21,1)`, timer 0x86=0x3C, +0xAA.
- Confiança: confirmado.

## 0x08023658 Fight_DispatchOnSubstateAA_AA_WaitPreviewShowGroup (AA 0x09)
- Passo: contador += 1; `FightPreview_SetGroupActive4`; se bot+0x89==0 e flag 0x03001798 bit 0: `FightPreview_ShowGroup4` e +0xAA; senão espera o timer 0x86 zerar.
- Confiança: confirmado.

## 0x080236E0 Fight_DispatchOnSubstateAA_AA_AddGaugeMax120 (AA 0x0A)
- Passo: `Fight_AddByteClampToMax` com teto 0x78 sobre o byte do bot (+0x87*0x100 + base); +0xAA.
- Confiança: confirmado.

## 0x08023718 NÃO DEFINIDO (AA 0x0B)
- Passo: escreve byte 0x1D em 0x03001800+0xA6; +0xAA += 1. Confiança: confirmado (leitura à mão, curto).

## Despachante AB (índices 0x00..0x0F)

## 0x08023758 NÃO DEFINIDO (AB 0x00)
- Passo: cálculo de ROS como AA 0x00, com `Fight_HasNoTechnique16Part` (0x080331F8) e `FUN_08036270`. Parcial. Confiança: provável.

## 0x080237C8 Fight_DispatchOnSubstateAA_AB_LoadGfxSetupSlot (AB 0x08)
- Passo: `decompress_and_store` + `bios_cpufastset`; seta flags em gObjectSlots+0x840 (|0x220); bytes 0x40 e halfword; copia valores de display do bot (+0x1742/+0x1744) e 0x864=6; +0x84=0x42; `Slot_SetupFromRecordList`; timer 0x86=0x3C; +0xAA.
- Confiança: confirmado.

## 0x080238A0 Fight_DispatchOnSubstateAA_AB_WaitAnimOrTimer (AB 0x09)
- Passo: `Anim_RunSlotScript` (índice do byte +0x84*4); se flag de bot ou bit 0 de 0x03001798 não ativos, espera timer 0x86 zerar; senão +0xAA.
- Confiança: confirmado.

## 0x08023900 Fight_DispatchOnSubstateAA_AB_MoveSlotTowardTarget (AB 0x0A)
- Passo: `Anim_RunSlotScript`; desloca o X do slot ±0x40 conforme bit 0 de +0xF8; `Fight_StepSlotTowardTarget` (0x21); ao chegar zera o campo e +0xAA.
- Confiança: confirmado.

## 0x08023988 Fight_DispatchOnSubstateAA_AB_SetupSlot43Sfx4E (AB 0x0B)
- Passo: +0x84=0x43; `Slot_SetupFromRecordList` (registro +0x10C); sfx 0x4E; +0xAA.
- Confiança: confirmado.

## 0x080239C0 Fight_DispatchOnSubstateAA_AB_WaitScriptFlag (AB 0x0C)
- Passo: `Anim_RunSlotScript`; se o byte de fim == -1 então +0xAA.
- Confiança: confirmado.

## 0x08023A04 Fight_DispatchOnSubstateAA_AB_StepFadeBytes8 (AB 0x0D)
- Passo: byte de fade decrementa 8; se já era 8: `Tile_ClearSlots(0x21,1)` e +0xAA; senão soma 8 ao byte vizinho.
- Confiança: confirmado.

## 0x08023A48 Fight_DispatchOnSubstateAA_AB_CopyDisplayValues (AB 0x0E)
- Passo: grava 0x26 e 0 no registro do bot (0x03000590 + bot*0x100 + 0xC4/0xC5) e os valores de `Fight_ClampDisplayValue1To99` (0x08036E54) de +0xE8 e +0xEC em +0xC6 e +0xC7; +0xAA.
- Confiança: confirmado.

## 0x08023AB0 NÃO DEFINIDO (AB 0x0F)
- Passo: escreve byte 0x2C em 0x03001800+0xA6; +0xAA. Confiança: confirmado (leitura à mão).

## Despachante AC (índices 0x00, 0x08, 0x0B, 0x0E, 0x0F)

## 0x08023AF0 NÃO DEFINIDO (AC 0x00)
- Passo: cálculo de ROS como AA 0x00 (chama 0x0803579C, 0x08035814, 0x0803725C, 0x08036270, 0x08037280). Confiança: provável.

## 0x08023B44 NÃO DEFINIDO (AC 0x08)
- Passo: carga de gráficos (0x080009BC, CPUFastSet 0x0807CCC0) e init de slot (como AA 0x04/AB 0x08). Confiança: provável.

## 0x08023C18 Fight_DispatchOnSubstateAA_AC_SetupSlot46Sfx4E (AC 0x0B)
- Passo: +0x84=0x46; `Slot_SetupFromRecordList` (registro +0x118); sfx 0x4E; +0xAA.
- Confiança: confirmado.

## 0x08023C50 NÃO DEFINIDO (AC 0x0E)
- Passo: lê byte do bot (0x03000590 + (byte+0x88)*0x100 + 0xBF) e escreve em 0x03001800+0xAA. Parcial. Confiança: chute.

## 0x08023C78 NÃO DEFINIDO (AC 0x0F)
- Passo: escreve byte 0x2B em 0x03001800+0xA6; +0xAA. Confiança: confirmado (leitura à mão).

## Despachante AD (índices 0x00, 0x08, 0x0B, 0x0D, 0x0E, 0x0F, 0x12, 0x15, 0x16)

## 0x08023CB8 NÃO DEFINIDO (AD 0x00)
- Passo: cálculo de ROS como AA 0x00 (0x0803579C, 0x08035814, 0x0803725C, 0x080331F8). Confiança: provável.

## 0x08023D44 NÃO DEFINIDO (AD 0x08)
- Passo: carga de gráficos e init de slot (como AA 0x04). Confiança: provável.

## 0x08023E18 Fight_DispatchOnSubstateAA_AD_SetupSlot44Sfx4E (AD 0x0B)
- Passo: +0x84=0x44; `Slot_SetupFromRecordList` (registro +0x110); sfx 0x4E; +0xAA.
- Confiança: confirmado.

## 0x08023E50 Fight_DispatchOnSubstateAA_AD_StepFadeBytes8Clear (AD 0x0D)
- Passo: como AB 0x0D, mas `Slot_ClearAttrMask(0x21,1)` no fim e o segundo byte desce 8.
- Confiança: confirmado.

## 0x08023E94 Fight_DispatchOnSubstateAA_AD_LoadGfxSetupSlot (AD 0x0E)
- Passo: como AB 0x08 (gráficos, flags |0x8000, valores de display, +0x84=0x42, slot registro +0x108, timer 0x3C).
- Confiança: confirmado.

## 0x08023F48 NÃO DEFINIDO (AD 0x0F)
- Passo: escreve halfwords em gObjectSlots (0x03002C30) com 0x848/0x849 e +0xAA. Parcial. Confiança: chute.

## 0x08023F80 Fight_DispatchOnSubstateAA_AD_SetupSlot43Sfx4E (AD 0x12)
- Passo: igual AB 0x0B (+0x84=0x43; slot registro +0x10C; sfx 0x4E).
- Confiança: confirmado.

## 0x08023FB8 Fight_DispatchOnSubstateAA_AD_WriteBotDisplayRows (AD 0x15)
- Passo: escreve 0x29 e 0 nos bytes +0xC9..0xCC e +0xC4..0xC7 do registro do bot (dois bots, base 0x03000590) com `Fight_ClampDisplayValue1To99`; +0xAA.
- Confiança: confirmado.

## 0x08024078 NÃO DEFINIDO (AD 0x16)
- Passo: escreve byte 0x2D em 0x03001800+0xA6; +0xAA. Confiança: confirmado (leitura à mão).

## Despachante AE (índices 0x00, 0x08)

## 0x080240B8 NÃO DEFINIDO (AE 0x00)
- Passo: cálculo de ROS como AA 0x00. Confiança: provável.

## 0x0802410C NÃO DEFINIDO (AE 0x08)
- Passo: carga de gráficos e init de slot (como AA 0x04). Confiança: provável.

## Nota sobre o byte +0xA6
Os handlers 0x1C (AA/Z 0x0F), 0x1D (AA 0x0B), 0x2B (AC 0x0F), 0x2C (AB 0x0F) e 0x2D (AD 0x16) escrevem
o byte +0xA6 do bloco 0x03001800 e avançam +0xAA. Provável: são o código de passo/efeito que o resto do
combate lê. Ainda não conferido quem lê +0xA6.
