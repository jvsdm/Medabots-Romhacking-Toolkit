# FS8: funções da lista FS8 (38 endereços, faixa 0x080537E4..0x08060A54)

Estado: 16 renomeados no Ghidra (confirmados ou prováveis); 5 já tinham nome (mtbe/FM3/EF/IV); 17 seguem FUN_ ou
sem nome (quase todos chutes). Achado de método: no Ghidra, os endereços 0x080537E4..0x08056F24 (e as entradas
0x08053xxx..0x08054014 e 0x080569E4) aparecem como `halt_baddata` (corpo de 1 instrução, sem disassembly). Os bytes Thumb na ROM são código real
(push/bl/pop válidos). Essas entradas foram lidas direto da ROM com um decodificador mínimo (BL, LDR de literal,
push/pop); não há disassembly no Ghidra. Confiança das entradas de corpo "bad" é provável ou chute.

## 0x080537E4 Viewport_SetFloatBounds
- Monta a estrutura em 0x03005350 com constantes float (±120.5 e ±80.5, em 0xC2F10000/0x42F10000/0xC2A10000/0x42A10000) e chama `0x08053858` e `0x0805390C`.
- RAM: 0x03005350 (escrita), literal 0x03005350.
- Confiança: provável (função de limites; o significado do "viewport" é chute).

## 0x08053858 Viewport_ComputeRectEdgeA
- Helper de float: sf_sub, sf_div, sf_mul sobre 0x03005350; escreve os campos da borda A.
- RAM: 0x03005350.
- Confiança: chute (lido dos bytes; não decompilado).

## 0x0805390C Viewport_ComputeRectEdgeB
- Igual ao 0x08053858 e termina com `sf_trunc_to_i32`; a saída vai para as tabelas 0x083D6F2C / 0x083D732C (ROM, literais).
- RAM: 0x03005350.
- Confiança: chute.

## 0x08053B04 Viewport_UpdateFromCamera
- Chama 6 helpers (0x08053BE0, 0x08053C14, 0x08053C7C, 0x08053C00, 0x08053D78, 0x08053DE0). Lê 0x03006D30, 0x03005520, 0x03005404, 0x03002C30.
- Confiança: chute (função de composição, não decompilada).

## 0x08053BE0 Viewport_ComputeOffsetA
- Usa sf_neg, sf_add, sf_sub, trunc; lê 0x03005404 e 0x03002C30.
- Confiança: chute.

## 0x08053C00 Viewport_ClampAxisA
- Teste de NaN (sf32_cmp_nan), converte inteiro para float e divide. Bytes com `halt_baddata` no Ghidra.
- Confiança: chute.

## 0x08053C14 Viewport_ClampAxisB
- Mesmo padrão do 0x08053C00. Confiança: chute.

## 0x08053C7C Viewport_AxisHelperC
- Helper de float (mul/add/neg/sub/div/trunc). Confiança: chute.

## 0x08053D78 Viewport_AxisHelperD
- Chama `sf_trunc_to_i32` e a si mesmo pelo laço (lido dos bytes). Lê 0x03005404 e 0x03002C30.
- Confiança: chute.

## 0x08053DE0 Viewport_AxisHelperE
- Usa a máscara 0x1FF e 0x03006D30. Confiança: chute.

## 0x08054014 Viewport_UpdateEdgesAndStore
- Chama 0x08053858, 0x0805390C, `sf_to_i32`, `sf32_from_i32`, `sf_add` e 0x08053FBC; lê 0x083D2F30 e 0x030053F0.
- Confiança: chute.

## 0x080569E4 Viewport_ScaleStep
- Float (mul, div, add, trunc), chama 0x080533A4, 0x08053360, 0x08053380. Literal 0x3B800000 (= 1/256).
- Confiança: chute.

## 0x08056F24 Viewport_CompareAndClampFloat
- Compara floats (sf_cmp, sf32_cmp_nan) com constantes de ROM (0x08056F70, 0x080AE8F4). Literais 0x42800000, 0x43400000, 0x43800000.
- Confiança: chute.

## 0x08057688 Screen_StartFlowCmd40
- `GameState_StartFlowByCommand(0x40, 0)` e retorna. Entrada da tabela 0x083D6004.
- RAM: nenhuma direta.
- Confiança: confirmado (decompile: uma chamada). Nome: provável (o que o comando 0x40 é, não confirmado).

## 0x08057698 Screen_TransitionEnterAndAdvance
- `Overworld_ResetAndStartScreenTransitionEffect` com floats 32.0 e 8.0 (0x42000000, 0x41000000), depois `GameState_AdvanceSubroutine`. Entrada 0x083D6008.
- Confiança: provável (lido dos bytes e dos callees).

## 0x080576BC Screen_TransitionRunAndAdvance
- `Overworld_RunScreenTransitionStep`, depois `GameState_AdvanceSubroutine`. Entrada 0x083D600C.
- Confiança: provável.

## 0x080576D0 FUN_080576D0 (sem nome)
- Chama `GameState_StartFlowByCommand`; o argumento não foi lido. Entrada 0x083D6010. Não renomeado.
- Confiança: chute.

## 0x080576E0 Screen_LoadTilesAndMapEntities
- Corpo longo (0x080576E0..0x08057A59). Chama `Map_ClearOamShadowViaDma`, `set_zeros`, `Map_LoadCurrentMapAndEntities`, `decompress_and_store`, `bios_lz77_vram`, `BgMap_CopyRectWithAttrs`, `BgMap_FillRectTilePalette`. Escreve em DMA (0x040000D4), VRAM (0x06001800...) e lê ROM (0x084C2430...).
- RAM: 0x03002C30, 0x030057E0, 0x03006D14; I/O DMA3.
- Confiança: provável (função de carga de tela/tiles, pela lista de callees).

## 0x08057B6C FieldMenu_Sub0B_RowMachine (já nomeado)
- Sub-máquina por linha do menu de campo (ctx+0x65, ctx+0x70). Ver FM3. Nome preexistente, não alterado.
- Confiança: provável (lido em FM3).

## 0x08057D04 FieldMenu_Sub0D_Close (já nomeado)
- Saída do menu de campo: BL 0x08047C00 e BL 0x0804A210. Confiança: confirmado (lido em FM3).

## 0x08057D14 FUN_08057D14 (sem nome)
- Chama 0x08047C24 com 0x084144B4 e 0x0300137C. Entrada 0x083D602C. Sub 0x0E do menu de campo, provavelmente (FM3 "?"). Não renomeado.
- Confiança: chute.

## 0x08057D40 FieldMenu_Sub0F_Close (já nomeado)
- Igual ao 0x08057D04 (saída do menu, códigos 4..6). Confiança: provável (FM3).

## 0x08057D50 FUN_08057D50 (sem nome)
- Mesmo padrão do 0x08057D14 (0x08047C24, 0x084144B4, 0x0300137C). Entrada 0x083D6034. Não renomeado.
- Confiança: chute.

## 0x08059434 Screen_PrepareNibbleRowsAndReset
- Chama `Screen_DrawPackedNibbleRows` (0x08059734, nome do mtbe) e `Map_ResetObjectRenderSlot`; corpo de 73 halfwords. RAM 0x030057A0.
- Confiança: provável (o nome exato do passo é chute).

## 0x0805AC8C Screen_ClearBgVramRegions
- `bios_cpufastset` sobre 0x0202FC00, 0x0202FE00 e 0x05000200 (paleta). Corpo curto.
- Confiança: provável.

## 0x0805C9C4 Screen_InitLoadTilesAndShow
- `bios_cpufastset` (limpa 0x06000000), `ClearObjectAttributeEntries`, `Clear4PairsHalfwords`, REG_DISPCNT=0x60, `decompress_and_store`, `Obj_InitSlotAndLoad`, liga OBJ (DISPCNT |= 0x1000), incrementa o estado.
- RAM: REG_DISPCNT (0x04000000).
- Confiança: confirmado (decompile).

## 0x0805CA74 Screen_DispatchStateTable
- `StateTable_DispatchByIndex`; lê 0x03000440 e 0x0300137C.
- Confiança: provável.

## 0x0805CAA8 Screen_DispatchSubOrRequestMode
- `StateMachine_DispatchSub`; se não houver mais nada, `MainState_RequestModeTransition`.
- Confiança: provável.

## 0x0805DF18 SaveLoad_LoadGame (já nomeado)
- Sem alteração. Confiança: provável (mtbe).

## 0x0805E010 SaveLoad_SaveGame (já nomeado)
- Sem alteração. Confiança: provável (mtbe).

## 0x0805E450 Screen_ClearOamAndAdvance
- `ClearObjectAttributeEntries(0,0x80)`, `Clear4PairsHalfwords`, REG_DISPCNT=0x60, incrementa o estado. Entrada 0x084836D8.
- Confiança: confirmado (decompile).

## 0x0805F984 Screen_DispatchStepAndAdvance
- `StateTable_DispatchByIndex`; se retornou != 0, zera o campo +2, `Obj_ResetStateFields` e escolhe o próximo estado (9 ou +1, conforme byte +0x2A). Entrada 0x0848371C.
- Confiança: confirmado (decompile).

## 0x0805FA00 Screen_DispatchSubByState
- `StateMachine_DispatchSub` com 0x0300137C. Entrada 0x08483734.
- Confiança: provável.

## 0x0805FFF8 FUN_0805FFF8 (sem nome)
- Toca SFX (`snd_sfx_request`), `aux_div_s32`; lê 0x03006F74 e 0x03002C30. Entrada 0x0848374C. Corpo 0x0805FFF8..0x08060167.
- Confiança: chute.

## 0x0806048C Anim_TileFrame64StepSfx
- `Anim_CycleTileFrame64` e `snd_sfx_request`. Lê 0x0300058C e 0x0300137C. Entrada 0x08483768.
- Confiança: provável.

## 0x080604C4 Anim_TileFrame64Cycle
- `Anim_CycleTileFrame64`; escreve REG 0x04000054 (timer). Entrada 0x0848376C.
- Confiança: provável.

## 0x0806096C Screen_EnableBgLayersInit
- REG_DISPCNT |= 0x200, 0x400, 0x800, 0x1000 (BG1..BG3 e OBJ); marca o estado com 8; zera bytes +0x3A e +0x3B. Entrada 0x0848383C.
- Confiança: confirmado (decompile).

## 0x08060A54 Menu_CursorStepSfxAdvance
- `Anim_UpdateSpritePosTable`, `Anim_LoadFrameEvery10`, `Shop_MoveCursorByDpad`; em A/B toca SFX 4 ou 5 e marca +0x48; se 0 (sem mudança), sai com `Obj_ResetStateFields`. Entrada 0x08483848.
- Confiança: provável (o nome Shop_* do callee está como chute em R3).
