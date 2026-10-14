# FS4: 38 funções Thumb de batalha (ponteiros nas tabelas 0x083ACEB0..0x083AD094)

Origem: `FS4_lista.txt`. Todas as 38 entradas são início exato de função no Ghidra (`get_function_by_address`). Antes eram `FUN_xxxxxxxx`; agora têm nome (renomeadas no Ghidra, confiança provável; nada salvo). Nenhuma está em `mtbe.csv`.

Quem chama: nenhuma tem BL no Ghidra (`get_function_xrefs` vazio nas 38). São chamadas só por ponteiro em tabelas de passo (0x083ACE14 + 4*índice, base da ST.md, não recalculada aqui). Por isso, "Chamada por" = tabela.

Prólogo comum (quase todas): `Fight_UpdateBotPartEntryMotion` (0x08012098) + `Obj_StepAccumMoveA` (0x08043CD4), às vezes com `Fight_DrawBotPartSprite` (0x080123A0) e `Fight_UpdateIndicatorOscillation` (0x08012468). Depois, um teste de estado e o avanço do passo.

RAM usada nas seções (provável, pelos literais da ROM):
- GS = 0x0300137C: sub-estado da batalha (passo). Quase toda função termina com `GS += 1` ou `GS = const`.
- ctx = 0x03001800: contexto da batalha (ctx+0x80, +0x84, +0x168..+0x187 são campos de bot/peça).
- fight = 0x03000440: estrutura de batalha (fight+0x84 = contador de fade; +0x9C = flag).
- 0x03000490, 0x03000430, 0x0300058C, 0x030004DE: buffers e bytes de entrada (0x0300058C ≈ teclas; ver seções).
- 0x03002B8C = índice de cena (menu_list), confirmado no README da pesquisa.
- 0x03002C30 = entrada de sprite (OAM) inicializada em 2 funções.
- Ponteiros ROM 0x083C6744 (lidos como `*(0x083C6744+off)`): pelo que o decompile mostra, é o argumento do `StateMachine_DispatchStep`. O conteúdo dessa tabela não foi identificado.

## 0x08010B24 Fight_ResetPartStateBlock
- O que faz: zera 0x14 bytes em 0x03000490, grava 2 no byte 0, 3 em 0x03002B8C (cena) e 0 em GS.
- RAM: 0x03000490 (20 B), 0x03002B8C, GS. Chama `Mem_FillBytes`.
- Tabela: 0x083ACEB0. Antes FL3.md deixava "nome mantido"; significado da cena 3 não confirmado.
- Confiança: provável (escritas confirmadas no decompile; sentido do reset não).

## 0x08010BA0 Fight_BranchFlagAdvanceGS
- O que faz: passo de movimento; se fight+0x9C == 1, GS += 1; senão GS = 0x70.
- RAM: fight+0x9C, GS.
- Tabela: 0x083ACEF4.
- Confiança: provável (ramo confirmado; intenção do flag não).

## 0x08010C28 Fight_WaitSubBThenAdvanceGS
- O que faz: roda `StateMachine_DispatchSubB`; quando retorna ≠0, `Obj_ResetStateFields` e GS += 1.
- RAM: GS. Chama `StateMachine_DispatchSubB` (0x08041CD4), `Obj_ResetStateFields` (0x0803FB0C).
- Tabela: 0x083ACEFC.
- Confiança: confirmado (fluxo); provável (o que o sub-despachante faz).

## 0x08010CE8 Fight_CopyPartGfxPtrsThenAdvanceGS
- O que faz: escolhe ponteiros por ctx+0x168 (e ctx+0x169) numa tabela ROM (0x083C6F68 e 0x083BBB4C). Copia 14 B para 0x030004DE e 15 B para 0x030004F2. Depois GS += 1.
- RAM: ctx+0x168/+0x169, 0x030004DE, 0x030004F2, GS. Chama `Fight_DrawBotPartSpriteBig` (0x080124B8).
- Tabelas: 0x083ACF04 e 0x083AD004 (duas entradas, mesmo alvo).
- Confiança: provável (cópias confirmadas; qual gráfico é, não).

## 0x08010D58 Fight_RunSubStep1ACTimer
- O que faz: roda `StateMachine_DispatchStep` sobre o ponteiro ROM 0x083C6744+0x1AC. Quando termina: se ctx+0x16A == 0, GS = 0x3C; senão GS += 1.
- RAM: ctx+0x16A, GS.
- Tabela: 0x083ACF08.
- Confiança: provável.

## 0x08010DAC Fight_InitPartSpriteEntry
- O que faz: `decompress_and_store` de ROM 0x084D923C para VRAM 0x06012400. DMA3 de 8 palavras (0x0850FF78 → 0x05000340). Preenche a entrada de sprite 0x03002C30 (+0x40..+0x68). `snd_sfx_request(0x2A)`. GS += 1.
- RAM: 0x040000D4 (DMA3), 0x05000340, 0x06012400, 0x03002C30, GS.
- Tabela: 0x083ACF0C.
- Confiança: provável (DMA e init confirmados no código; o sprite em si não).

## 0x08010E50 Fight_AdvanceSlotYWaveStep
- O que faz: `Fight_AdvanceSlotYByWaveTable(1,0)`; quando retorna ≠0, GS += 1.
- RAM: GS. Chama `Fight_AdvanceSlotYByWaveTable` (0x08036AC4).
- Tabela: 0x083ACF10.
- Confiança: confirmado (fluxo); provável (o efeito visual).

## 0x08010E7C Fight_InitIndicatorSpriteEntry
- O que faz: como o 0x08010DAC, mas com `Fight_UpdateIndicatorOscillation`. Descompacta ROM 0x084D9350 em VRAM 0x06016C00, DMA3 de 0x084E4538 para 0x05000360. Inicializa a entrada 0x03002C30 (+0x80..+0x9C). GS += 1.
- RAM: 0x03002C30, 0x06016C00, 0x05000360, GS.
- Tabela: 0x083ACF14.
- Confiança: provável.

## 0x08010F08 Fight_WaitAorBOrStartInput
- O que faz: lê o byte 0x0300058C. Se (&3): `ClearObjectAttributeEntries(1,2)`, zera 0x03000430, SFX 0x0D, GS += 1. Se (&8): SFX 4, GS = 0x60.
- RAM: 0x0300058C (entrada de teclas, provável), 0x03000430, GS.
- Tabela: 0x083ACF18.
- Confiança: provável (bits 0/1/3 seguem a convenção GBA; A/B/Start não testados no emulador).

## 0x08010F6C Fight_WaitCtxFlagOrJumpGS40
- O que faz: se ctx+0x01 == 0, GS = 0x40; senão `Obj_ResetStateFields` e GS += 1.
- RAM: ctx+0x01, GS.
- Tabela: 0x083ACF1C.
- Confiança: provável.

## 0x08010FA4 Fight_RunSubStep1D8AddCounter
- O que faz: `StateMachine_DispatchStep` sobre ROM 0x083C6744+0x1D8. Ao terminar, `Map64_AddCountClamp99(0x32,1)` e GS += 1.
- RAM: GS. Chama `Map64_AddCountClamp99` (0x080422B8, confirmado no mtbe).
- Tabela: 0x083ACF20.
- Confiança: provável.

## 0x08010FDC Fight_RefreshDisplayFormatNumStep
- O que faz: `Fight_RefreshBotDisplayState` (0x08012584); formata o número ctx+0x16C (16 bits) em 6 glifos no buffer 0x030004DE (base 0x35). Depois `Obj_ResetStateFields` e GS += 1.
- RAM: ctx+0x16C, 0x030004DE, GS.
- Tabela: 0x083ACF24.
- Confiança: provável (formatação confirmada pelo nome da chamada; o número exibido não).

## 0x08011018 Fight_RunSubStep1B4ThenAdvance
- O que faz: `StateMachine_DispatchStep` sobre ROM 0x083C6744+0x1B4. Ao terminar, GS += 1.
- RAM: GS.
- Tabela: 0x083ACF28.
- Confiança: provável.

## 0x08011048 Fight_AwardPartExpThenAdvance
- O que faz: `Fight_AwardPartExperience` (0x080127DC); ctx+0x186 = 0; GS += 1.
- RAM: ctx+0x186, GS.
- Tabela: 0x083ACF2C.
- Confiança: confirmado (fluxo); provável (XP).

## 0x08011078 Fight_CyclePartFrameTableA
- O que faz: ctx+0x186 é o índice (0..3). Se for 3, zera e GS = 0x47. Senão, lê a tabela de 8 B em ctx+0x16E: se o byte for 0, índice += 1; senão copia 0x0B B de ROM (0x083B6590) para 0x030004DE, toca `snd_song_request(0x10)` e GS += 1.
- RAM: ctx+0x16E, ctx+0x186, 0x030004DE, GS.
- Tabela: 0x083ACF30.
- Confiança: provável.

## 0x080110FC Fight_RunSubStep1DCThenAdvance
- O que faz: `StateMachine_DispatchStep` sobre ROM 0x083C6744+0x1DC. Ao terminar, GS += 1.
- RAM: GS.
- Tabela: 0x083ACF34.
- Confiança: provável.

## 0x0801112C Fight_IncPartIndexSetGS43
- O que faz: ctx+0x186 += 1 e GS = 0x43.
- RAM: ctx+0x186, GS.
- Tabela: 0x083ACF38.
- Confiança: confirmado (escritas); provável (passo).

## 0x08011158 Fight_CyclePartFrameTableB
- O que faz: como o 0x08011078, com tabela de 8 B em ctx+0x174 (bytes em ctx+0x175+idx*8). Quando idx == 3, GS = 0x4B. Copia 0x0B B (ROM 0x083B6590) e 0x11 B (ROM 0x083B74B8) para 0x030004DE/0x030004F2. `snd_song_request(0x10)`. GS += 1.
- RAM: ctx+0x174, ctx+0x175, ctx+0x186, 0x030004DE, 0x030004F2, GS.
- Tabela: 0x083ACF40.
- Confiança: provável.

## 0x0801120C Fight_RunSubStep1E0ThenAdvance
- O que faz: `StateMachine_DispatchStep` sobre ROM 0x083C6744+0x1E0. Ao terminar, GS += 1.
- RAM: GS.
- Tabela: 0x083ACF44.
- Confiança: provável.

## 0x0801123C Fight_IncPartIndexSetGS47
- O que faz: ctx+0x186 += 1 e GS = 0x47.
- RAM: ctx+0x186, GS.
- Tabela: 0x083ACF48.
- Confiança: confirmado (escritas); provável (passo).

## 0x08011268 Fight_CyclePartIndexSetGS58
- O que faz: ctx+0x186 == 3 → zera e GS = 0x58. Senão, se o byte em ctx+0x16F+idx*8 for 0, idx += 1; caso contrário GS += 1.
- RAM: ctx+0x16F, ctx+0x186, GS.
- Tabela: 0x083ACF50.
- Confiança: provável.

## 0x080112C4 Fight_CommitPaletteClearFlag
- O que faz: `CopyPaletteBuffersToHardware` (0x08001380); fight+0x84 = 0; GS += 1.
- RAM: fight+0x84, GS.
- Tabela: 0x083ACF54.
- Confiança: confirmado (fluxo).

## 0x080112F0 Fight_FadeRowsToColorStepA
- O que faz: enquanto fight+0x84 < 9, `Palette_FadeRowsToColorA` e `..B` com cor 0xFFFF, linhas (cnt<<2), e cnt += 1. Ao passar de 8, GS += 1.
- RAM: fight+0x84, GS, buffers de paleta.
- Tabela: 0x083ACF58.
- Confiança: provável (fade pelo nome das funções confirmado no mtbe; cor/linha não confirmadas).

## 0x080114E0 Fight_FadeOutSoundStep
- O que faz: `snd_fade_request(8)` e GS += 1.
- RAM: GS. Chama `snd_fade_request` (0x080750F0).
- Tabela: 0x083ACF84.
- Confiança: provável.

## 0x080115B4 Fight_CommitPaletteOscStep
- O que faz: draw + oscilação + `CopyPaletteBuffersToHardware`; fight+0x84 = 0; GS += 1.
- RAM: fight+0x84, GS.
- Tabela: 0x083ACFA4.
- Confiança: confirmado (fluxo).

## 0x080115E8 Fight_FadeRowsUpStep
- O que faz: como o 0x080112F0 (fade A/B, cor 0xFFFF, até cnt 9), com draw e oscilação. Ao terminar, GS += 1.
- RAM: fight+0x84, GS.
- Tabela: 0x083ACFA8.
- Confiança: provável. Antes, FS.md listava este endereço como "sem função" no Ghidra; agora é FUN_ (criado pelo sync).

## 0x080116C4 Fight_InitPartStateSetGS10
- O que faz: `Slots_ClearFourPairs` e `ClearObjectAttributeEntries(0,0x80)`; fight+0x96 = 0; `Mem_FillBytes` em fight+0x3C (0x14 B); marca fight+0x3D/+0x3E a partir de ctx+0x168. Cena (0x03002B8C) = 5 e GS = 0x10.
- RAM: fight+0x3C..+0x3E, 0x03002B8C, GS.
- Tabela: 0x083ACFB0.
- Confiança: provável.

## 0x08011A58 Fight_FadeRowsDownStep
- O que faz: fade inverso (linhas 0x20 − 4·cnt), cor 0xFFFF, cnt até 9; depois GS += 1. Com draw e oscilação.
- RAM: fight+0x84, GS.
- Tabela: 0x083ACFBC.
- Confiança: provável.

## 0x08011ABC Fight_DrawOscSetGS3D
- O que faz: draw + oscilação e GS = 0x3D.
- RAM: GS.
- Tabela: 0x083ACFC0.
- Confiança: provável.

## 0x08011ADC Fight_RunSubStep1B8ThenAdvance
- O que faz: `StateMachine_DispatchStep` sobre ROM 0x083C6744+0x1B8. Ao terminar: fight+0x1B = 0 e GS += 1.
- RAM: fight+0x1B, GS.
- Tabela: 0x083ACFE4.
- Confiança: provável.

## 0x08011B18 Fight_WaitSubBFlagOrJumpGS77
- O que faz: `StateMachine_DispatchSubB`; ao terminar, se o byte 0x030057E0+0xC2F for 0, GS = 0x77; senão GS += 1.
- RAM: 0x030057E0+0xC2F, GS.
- Tabela: 0x083ACFE8.
- Confiança: provável.

## 0x08011B5C Fight_FadeRowsToWhiteStep
- O que faz: fade A/B com valores 0x7FFF e 0xFFFF; cnt em fight+0x84 até 9; depois GS += 1.
- RAM: fight+0x84, GS, buffers de paleta.
- Tabela: 0x083ACFF4.
- Confiança: provável (a cor 0x7FFF = branco em BGR555 é inferência; argumentos não confirmados).

## 0x08011C88 Fight_AssignRandomPartSlotStep
- O que faz: se (byte 0x030057E0+0xC2E & 0xF) == 0: `Fight_AssignRandomBotPartSlot(0)`, `Tinpet_ActivateMatchingEntry` com id de ctx/bot (0x03000590 + bot·0x100 + 0x47), `Obj_ResetStateFields`, GS += 1. Senão, zera o byte e GS = 0x40.
- RAM: 0x030057E0+0xC2E, 0x03000590, ctx+0x187, GS.
- Tabela: 0x083AD000.
- Confiança: provável (usa a base de bot da pesquisa, 0x03000590 + bot·0x100).

## 0x08011CFC Fight_RunSubStep1B0ThenAdvance
- O que faz: `StateMachine_DispatchStep` sobre ROM 0x083C6744+0x1B0. Quando termina e (0x0300058C & 3) ≠ 0: zera 0x03000430, SFX 0x0D, GS += 1.
- RAM: 0x0300058C, 0x03000430, GS.
- Tabela: 0x083AD008.
- Confiança: provável.

## 0x08011D50 Fight_CheckPartTableEmptyStep
- O que faz: `Count_Entries3InTable9` (0x080440B4). Se 0: `Obj_ResetStateFields` e GS += 1. Senão GS = 0x40.
- RAM: GS. Chama `Count_Entries3InTable9`.
- Tabela: 0x083AD00C.
- Confiança: provável.

## 0x08011D80 Fight_RunSubStep1BCSetGS72
- O que faz: `StateMachine_DispatchStep` sobre ROM 0x083C6744+0x1BC; ao terminar, GS = 0x72.
- RAM: GS.
- Tabela: 0x083AD010.
- Confiança: provável.

## 0x08012BC0 Fight_DrawBotsOverlayStep
- O que faz: `Fight_DrawActiveBotsOverlay` (0x080195C0), `FUN_08016370(ctx+0x80)`, GS += 1.
- RAM: ctx+0x80, GS.
- Tabela: 0x083AD090.
- Confiança: provável.

## 0x08012BE4 Fight_AnimSpriteGroupStep
- O que faz: draw do overlay de bots e `Fight_UpdateSpriteGroupMoveAnim(ctx+0x80)` (0x08016438). Quando retorna ≠0, GS += 1.
- RAM: ctx+0x80, GS.
- Tabela: 0x083AD094.
- Confiança: provável.

## Divergências e pendências
- FS.md marcava 0x08010DAC e 0x080115E8 como "sem função no Ghidra". Hoje são funções (FUN_ criados pelo sync). A lista FS.md de 136 verificados estava defasada.
- FL3.md tinha 0x08010B24 como "nome mantido" (FUN_08010B24). Agora está `Fight_ResetPartStateBlock`.
- As 38 não têm xref de código: o Ghidra não cria xref a partir de ponteiro de tabela. Quem chama é a tabela ROM.
- ROM 0x083C6744 aparece como base do `StateMachine_DispatchStep` em 12 funções. Não identificado; é a principal pendência.
- Índices de tabela (0x083ACE14 + 4·i) herdados da ST.md, não recalculados.
- Alguns alvos aparecem em mais de uma tabela (0x080112C4 em 0x083ACF54/0x083ACF88/0x083ACFF0; 0x080112F0 em 0x083ACF58/0x083ACF8C; 0x080114E0 em 0x083ACF84/0x083ACFEC; 0x08010CE8 em 0x083ACF04/0x083AD004). A FS4 lista só a primeira.
