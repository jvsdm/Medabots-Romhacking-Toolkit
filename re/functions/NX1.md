# NX1: funções novas do sync (endereços de EXTRA_criar.txt)

Origem de cada endereço: ver a linha correspondente em `EXTRA_criar.txt`. Cada seção segue o formato do README
(assinatura, o que faz, chama/chamada por, memória, para o Lua, confiança).

## 0x0801013C PartDetail_DispatchModeHandler
- Assinatura: `int PartDetail_DispatchModeHandler(void)`
- O que faz: lê o byte de modo em 0x03002D59 (ponteiro no literal pool 0x08010160/0x08010164), despacha pela tabela
  de handlers em [DAT_0801015C] via `CallViaR0` e retorna 1 se o modo for 3, senão 0.
- Chama: handler indexado pelo modo (tabela no pool 0x0801015C). Chamada por: 0x0800EB34 (passo 66 do menu 5).
- Memória: lê RAM 0x03002D59 (modo). Para o Lua: precisa ser portado (lógica da tela de detalhe da peça).
- Confiança: confirmado (leitura do decompile + ST2.md §2).
- Nome Ghidra: já era bom, não renomeado.

## 0x08033CD0 Fight_ResolveDefenderRosPhase
- Assinatura: `void Fight_ResolveDefenderRosPhase(void)`
- O que faz: fase de resolução do defensor: chama `Fight_ResolveDefenderRosReductions` e depois
  `Fight_AdvanceActionScriptCursor` (avança o cursor do action-script).
- Chama: Fight_ResolveDefenderRosReductions, Fight_AdvanceActionScriptCursor. Chamada por: tabela de ações (DM.md §1).
- Memória: não lida diretamente (só via subfunções). Para o Lua: precisa ser portado (fase do combate).
- Confiança: confirmado (bytes `bl 0x08038E54; bl 0x08034F08` batem com DM.md).
- Nome Ghidra: já era bom, não renomeado.

## 0x08010264 Scene_GsDispatch_Menu6_Encounter
- Assinatura: `void Scene_GsDispatch_Menu6_Encounter(void)`
- O que faz: se `GS == 2` (byte em 0x03000430) monta um bloco de parâmetros de DMA (valor 0x200, fonte calculada,
  destino fixo) e depois despacha por `GS` pela tabela de handlers do menu 6 (subvetor 0x083ACE24).
- Chama: handler indexado pelo GS (tabela 0x083ACE24). Chamada por: menu_list[6] (BT2.md, MN.md menu 6).
- Memória: lê 0x03000430 (GS); escreve o bloco de DMA apontado por DAT_080102A8. Para o Lua: não precisa
  (DMA/hardware), a lógica do encontro é portada pelos handlers.
- Confiança: confirmado (decompile; condição GS==2 bate com BT2.md).
- Nome Ghidra: já era bom, não renomeado.

## 0x0803D538 Scene_GsDispatch_Menu10_Link
- Assinatura: `int Scene_GsDispatch_Menu10_Link(void)`
- O que faz: se `GS-0x10` está entre 0x10 e 0x1F, ou se `Battle_CheckExitShortcut` não devolve 1 (ver literal
  0x0803D588), monta DMA de CpuFastSet quando GS==2, chama `Overlay_ClearTimerIfFlag` e despacha por GS pela tabela
  de 96 palavras 0x083AFC78 (base do literal 0x0803D59C). É o menu 10 (tela de link/troca).
- Chama: Battle_CheckExitShortcut, bios_cpufastset, Overlay_ClearTimerIfFlag, handler indexado por GS.
  Chamada por: menu_list[10] (BT2.md, MN.md menu 10).
- Memória: lê GS (via literal 0x0803D588/0x0803D58C). Para o Lua: a lógica do link é portada pelos handlers; DMA e
  CpuFastSet não precisam.
- Confiança: confirmado no decompile; a faixa 0x10..0x1F e a base da tabela (GS-0x10) batem com BT2.md.
- Nome Ghidra: já era bom, não renomeado.

## 0x0800C7E4 debug_option4_item_entry
- Assinatura: `void debug_option4_item_entry(void)`
- O que faz: opção "Item" (4ª) do menu de debug. Lê o byte em `[DAT_0800C800 + 0x33]` e despacha pela tabela de
  handlers no literal DAT_0800C7FC via `CallViaR0`.
- Chama: handler indexado pelo byte (tabela no pool 0x0800C7FC). Chamada por: tabela do menu de debug (DB.md).
- Memória: lê RAM na estrutura apontada por DAT_0800C800 (campo +0x33). Para o Lua: não precisa (ferramenta de debug).
- Confiança: confirmado (decompile). O handler final (o que o submenu Item faz) não foi lido.
- Nome Ghidra: já era bom, não renomeado.

## 0x0800CE70 debug_option6_mdlpic_entry
- Assinatura: `void debug_option6_mdlpic_entry(void)`
- O que faz: opção "Mdlpic" (6ª) do menu de debug. Mesmo padrão da opção Item: lê o byte em
  `[DAT_0800CE8C + 0x33]` e despacha por DAT_0800CE88.
- Chama: handler indexado pelo byte. Chamada por: tabela do menu de debug (DB.md).
- Memória: lê a estrutura apontada por DAT_0800CE8C (+0x33). Para o Lua: não precisa (debug).
- Confiança: confirmado (decompile); o handler final não foi lido.
- Nome Ghidra: já era bom, não renomeado.

## 0x08012B94 Battle_CpuActionSelectStep
- Assinatura: `void Battle_CpuActionSelectStep(void)`
- O que faz: driver da escolha da CPU. Chama `0x080195C0` (desenha o overlay dos bots ativos); se o byte de flag em
  `[0x08012BB8] + 0x89` não for 1, chama `Fight_SelectActionsForBotGroup(3)` (BL 0x08012BA6 -> 0x0801531C); depois
  incrementa o contador em `[0x08012BBC]`.
- Chama: 0x080195C0 (Fight_DrawActiveBotsOverlay), 0x0801531C (Fight_SelectActionsForBotGroup). Chamada por: tabela
  0x083AD080 (passo GS=3, ponteiro, sem BL).
- Memória: lê/escreve RAM 0x03000xxx dos ponteiros 0x08012BB8 (flag +0x89) e 0x08012BBC (contador). Para o Lua:
  precisa ser portado (é o passo de escolha de ações da CPU).
- Confiança: confirmado (decompile + disassemble).
- CORREÇÃO ao EXTRA_criar.txt: este driver NÃO chama 0x080196C0. Sua primeira chamada é 0x080195C0.
- Nome Ghidra: já era bom, não renomeado.

## 0x080196C0 (miolo de Fight_DrawSecondaryEffectIcon)
- Não é função própria: o endereço cai dentro de `Fight_DrawSecondaryEffectIcon` (0x08019600..0x0801970D).
  É a instrução `ldr r0,[r0,#0x20]` logo antes do `bl 0x08040bd8`, no ramo em que o byte de estado do bot (+0xC4)
  é 0x26 ou 0x29.
- O que faz (pelo disassemble da função inteira): a função checa o byte de estado do bot (+0xC4 do bloco
  0x03000590 + bot*0x100) para 0x1F, 0x26 ou 0x29; nos dois casos busca um registro de 0x40 bytes numa tabela e chama
  0x08040BD8 ou 0x08040B90, copia 4 bytes com 0x08041D04 (memcpy) e chama 0x08019D9C (ícone/efeito).
- Chama: 0x08040BD8, 0x08040B90, 0x08041D04, 0x08019D9C. Chamada por: não rastreado nesta rodada.
- Memória: lê o estado do bot (0x03000590 + bot*0x100, byte +0xC4) e um byte +0xB de outra struct (base + 0x1740 + bot*0x100). Para o Lua: precisa ser portado (efeito
  visual de status secundário) só se a tela de batalha for portada.
- Confiança: confirmado (disassemble lido por inteiro). A hipótese de "ícone" é provável, não confirmada.
- Nome Ghidra: não renomeado (a função já tem nome do sync).

## 0x08073AB8 snd_dma_channel_setup -> Sound_DmaFifo_SetupChannel
- Assinatura: `void Sound_DmaFifo_SetupChannel(int channel, u32 src)`
- O que faz: escreve 3 palavras no registro DMA do hardware em `0x040000B0 + 12*channel` (base do literal
  0x08073AD8 = 0x040000B0): SAD = `src` (param_2), DAD = palavra da tabela ROM 0x08486E54 no índice `channel-1`
  (literal 0x08073ADC), CNT = 0xF6400004 (literal 0x08073AE0: enable, timing especial/FIFO, repeat, 32 bits).
- Chama: nada. Chamada por: não rastreado nesta rodada (provável o código de SN2 que monta o PCM).
- Memória: escreve IO 0x040000B0..0x040000DF (DMA0..DMA3). Para o Lua: não precisa (hardware).
- Confiança: confirmado nos literais (lidos da ROM original). O índice (channel vs channel-1) bate com SN2 só se o
  parâmetro for o canal 1-based: a SN2 diz 0x040000B0+12*(n-1); o decompile diz +12*param. Conferir o chamador.
- Nome Ghidra: renomeado de `snd_dma_channel_setup` para `Sound_DmaFifo_SetupChannel`.

## 0x080332EC Fight_ValidatePart_ClearPartByte0F
- Assinatura: `void Fight_ValidatePart_ClearPartByte0F(void)`
- O que faz: zera um byte: endereço = `DAT_08033310 + 0x57 + [DAT_08033314+0x5C]*0x100 + [DAT_08033314+0x5D]*0x18`.
  Ou seja, zera o campo +0x0F do registro de peça (0x48 + 0x0F = 0x57), indexado por bot (*0x100) e slot (*0x18).
- Chama: nada. Chamada por: handler de validação de técnica (tabela `gTechniqueActionValidationHandlerTable`, 11
  técnicas, TQ.md).
- Memória: escreve RAM (ponteiros em 0x08033310/0x08033314; campos de bot/slot em +0x5C/+0x5D). Para o Lua: precisa
  ser portado (efeito de validação da técnica), se a lógica de técnica for portada.
- Confiança: confirmado (decompile de 9 instruções; batendo com TQ.md). A leitura "+0x48 + 0x0F" é provável.
- Nome Ghidra: já era bom, não renomeado.

## 0x0800EB34 SceneStep_Caller_ACE14
- Assinatura: `void SceneStep_Caller_ACE14(void)`
- O que faz: passo 66 do menu 5 (entrada da tabela 0x083ACCD4). Chama `Sprite_IncrementXYIfFlag` e
  `PartsGrid_DrawLargeDisplayIfOwned`; se `PartDetail_DispatchModeHandler()` devolve 1, chama
  `Slot_ClearAttrMask(4,1)` e incrementa o contador em [0x0800EB5C].
- Chama: Sprite_IncrementXYIfFlag, PartsGrid_DrawLargeDisplayIfOwned, PartDetail_DispatchModeHandler,
  Slot_ClearAttrMask. Chamada por: tabela 0x083ACCD4 (ponteiro; sem BL, ST2.md §1).
- Memória: contador RAM em [0x0800EB5C]. Para o Lua: precisa ser portado (tela de detalhe da peça).
- Confiança: confirmado (decompile).
- Nome Ghidra: já era bom, não renomeado. Mantém o nome do sync.

## 0x0800D9DC Menu5_Dispatch_PartsScreens
- Assinatura: `void Menu5_Dispatch_PartsScreens(void)`
- O que faz: despachante do menu 5 (peças). Se `Battle_CheckExitShortcut()` devolve 0: se GS==2 monta DMA de
  CpuFastSet; depois despacha pelo passo `*DAT_0800DA30` na tabela de handlers (DAT_0800DA2C).
- Chama: Battle_CheckExitShortcut, bios_cpufastset, handler indexado pelo passo. Chamada por: menu_list[5]
  (ST2.md §1; MN.md).
- Memória: lê GS (literal 0x0800DA1C) e o passo (0x0800DA30). Para o Lua: a lógica do menu é portada pelos
  handlers; DMA não precisa.
- Confiança: confirmado (decompile; ST2.md já dizia menu_list[5]).
- Obs.: o nome "Battle_CheckExitShortcut" é chamado também no menu de peças; o nome pode ser enganoso, conferir.
- Nome Ghidra: já era bom, não renomeado.

## 0x0803C0CC Fight_ScrollSlotPhase_C0CC
- Assinatura: `void Fight_ScrollSlotPhase_C0CC(void)`
- O que faz: fase de rolagem dos slots de bot. Pega o slot atual (byte em DAT_0803C100 + DAT_0803C0FC) e a
  struct de 0x40 bytes dele. Se o contador em +0x2B é diferente de 0, só decrementa e sai. Senão, conforme o estado
  em +0x25: faixa 1..15 rola num sentido e faixa 0x11..0x1F rola no outro; em ambos avança o offset de tile
  (`Anim_AdvanceSlotTileOffset`), atualiza a posição (+0x30) e, passado o limiar, muda os registros de scroll
  (0x154/0x158 da struct de fundo) e chama `Fight_ScrollEntryCoords(1|0, delta)`.
- Chama: Anim_AdvanceSlotTileOffset, Fight_ScrollEntryCoords. Chamada por: não rastreado nesta rodada (o
  sync diz que é a escolha de passo do AFC2C; ver ST2.md §3 / DM.md).
- Memória: lê e escreve a struct de slot (base por DAT_0803C0F8) e os registros de scroll (DAT_0803C174). Para o Lua:
  precisa ser portado se a tela de slots for portada.
- Confiança: estrutura confirmada no decompile (ramos e limiares). O sentido "rolagem" é provável.
- Nome Ghidra: já tinha nome com sufixo do endereço; mantido.

## 0x0807C92C (miolo de bios_store_idx -> Sound_TimerSlot_Select)
- Não é função própria: o endereço cai dentro de `bios_store_idx` (0x0807C918..0x0807C94F), renomeada para
  `Sound_TimerSlot_Select`. É a instrução `ldr r3,[0x0807C944]`, que carrega o literal 0x04000100 (TM0CNT_L).
- Assinatura da função: `int Sound_TimerSlot_Select(u8 idx, u32 *out_reg, u32 *out_cb)`
- O que faz: se `idx <= 3`, grava `idx` no byte 0x030003F4, grava em 0x030003FC o endereço do registro de timer
  `0x04000100 + idx*4` (TM0CNT_L..TM3CNT_L) e grava em `*out_cb` a constante 0x0807C8F5 (ponteiro Thumb, provavelmente
  o callback do timer). Retorna 0; se `idx > 3`, retorna 1 sem gravar.
- Chama: nada. Chamada por: não rastreado nesta rodada (SN2.md diz que é o único uso de TM0CNT).
- Memória: escreve RAM 0x030003F4 e 0x030003FC; ponteiros de IO. Para o Lua: não precisa (timer de hardware).
- Confiança: confirmado nos literais lidos da ROM original (0x030003F4, 0x030003FC, 0x04000100, 0x0807C8F5).
  O papel de "timer PCM" e de 0x0807C8F5 como callback é provável.
- Nome Ghidra: renomeado `bios_store_idx` -> `Sound_TimerSlot_Select` (nome antigo era enganoso, não é BIOS).
