# FS2: funções 0x08004DF0..0x08009248 (38 entradas)

Escopo: as 38 entradas de `FS2_lista.txt`. Todas são entrada de função no Ghidra (nenhuma cai no meio de outra).
Quase todas são passos por quadro (um handler por estado) da tela de grade de peças (`PartsGrid_*`). Os endereços
não têm BL direto: cada um é chamado só por ponteiro de tabela. Varredura de BL na ROM inteira: 0 chamadas para
essas 38 entradas.

Tabelas (ROM EU, ponteiros de 4 bytes contíguos com bit thumb):
- `0x083AC86C..0x083ACA28`: 112 entradas (stride 4). Carregada por `FUN_08007584` (literal 0x080075A4, LDR em
  0x0800758E), que faz `CallViaR0(tabela[*0x080075A8])` depois de `Battle_CheckExitShortcut`. Índice = (slot - 0x083AC86C)/4.
  Divergência: `mtbe.csv` põe o label `table_partsgrid_states` em 0x083AC880 com stride 0xC; os dados mostram ponteiros
  de 4 bytes contíguos desde 0x083AC86C. Confiança no stride 4: confirmado (112 ponteiros de código seguidos).
- `0x083AC2C8..0x083AC59C`: 182 entradas. Carregada por `MainState0` (0x08002534, "estado 0"), que indexa com
  `*0x08002550` (gMainState). Provável que seja gMainStateTable (mtbe: 0x0300178C). As entradas 177 e 178 são
  as duas primeiras desta seção (0x08004DF0 e 0x08004E40).

Ghidra: 36 funções renomeadas (provável/confirmado, ver CSV). Ficaram como FUN_ por chute: 0x08007998 e 0x08007DDC.
Helpers comuns vistos nos decompiles (já existem no mtbe/Ghidra): `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`,
`GpuBlend_TickStep`, `Slot_SetPosStepByMode`, `StateMachine_DispatchStep`, `Obj_InitSlotAndLoad`, `Obj_ResetStateFields`,
`snd_sfx_request`, `PartsGrid_*` (mtbe). Os campos `+0x3C..+0x48` são do struct de estado da tela (ponteiro em
literal pool; endereço absoluto não resolvido nesta rodada, por isso a coluna RAM fala em offsets).

## 0x08004DF0 BattlePreview_FadeRowsStep
- O que faz: chama `Pair_TickUpCounterIfFlag` e `BattlePreview_BuildLargeCompositeLayer5`; faz fade de paleta por
  linhas (`Palette_FadeRowsToColorA/B`, linhas 0..contador) e incrementa o contador até 0x21, depois incrementa um contador global.
- RAM: struct +0x84 (contador de linha); ponteiro de literal em 0x08004E24 e cor em 0x08004E28.
- Chamada por: tabela 0x083AC2C8, idx 177 (via MainState0).
- Confiança: provável (fade de paleta, nomes dos helpers confirmados pelas chamadas; significado do estado é chute).

## 0x08004E40 BattlePreview_ResetOamAndState
- O que faz: `ClearObjectAttributeEntries(0,0x80)` (limpa OAM) e zera/seta três campos de estado por ponteiro (0, 7, 0).
- RAM: OAM (via helper `ClearObjectAttributeEntries`; 0x07000000 é o padrão do GBA, não conferido no decompile); ponteiros de literal 0x08004E64..0x08004E70.
- Chamada por: tabela 0x083AC2C8, idx 178.
- Confiança: provável (reset de estado de entrada de tela).

## 0x08007834 PartsGrid_SlideInStep
- O que faz: `Timer_IncPairIfFlag`, `FUN_0800994C`, `Slot_SetPosStepByMode(3, *, 0x18)`, `GpuBlend_TickStep`; ao
  terminar o blend, incrementa o contador de estado.
- RAM: struct da tela (+contador via ponteiro de literal 0x08007864).
- Chamada por: tabela 0x083AC86C, idx 1.
- Confiança: provável (passo de movimento + avanço confirmados; sentido "entrada" é chute).

## 0x08007868 PartsGrid_InputStep
- O que faz: passo de entrada da grade: chama cursor (`RefreshCursorSprite`, `MoveColumnInput`, `MoveRowInput`,
  `ToggleSelection`). Botão A (bit0): índice = row*6 + col (campos +0x3D/+0x3F/+0x40), SFX 6 ou 4; se o slot estiver
  ocupado (bit 0x80) e não for o slot 4, troca de estado (0x60 ou modo 7). Botão B (bit1): SFX 5 e incrementa contador.
- RAM: struct +0x3C, +0x3D, +0x3F, +0x40, +0x42; tabela de bytes de grade (literal 0x08007934).
- Chamada por: tabela 0x083AC86C, idx 2.
- Confiança: confirmado (lido no decompile; índice 6 colunas bate com `PartsGrid_SplitRowCol` do mtbe).

## 0x08007964 PartsGrid_BlendInStep
- O que faz: `SetGpuBlendMode(6,0x3F,0x20)`, `Timer_IncPairIfFlag`, `FUN_0800994C`, `Slot_SetPosStepByMode`, contador++.
- RAM: contador via literal 0x08007994.
- Chamada por: tabela 0x083AC86C, idx 3 e 104.
- Confiança: provável (passo de transição de blend).

## 0x08007998 FUN_08007998 (sem rename)
- O que faz: igual a `PartsGrid_SlideInStep` (Timer, FUN_0800994C, Slot_SetPosStepByMode 0x18, GpuBlend), mas com
  outra variável de contador (literal 0x080079C8).
- Chamada por: tabela 0x083AC86C, idx 4, 7 e 105.
- Confiança: chute quanto ao sentido (provável como passo de saída). Mantido FUN_.

## 0x080079CC PartsGrid_ResetOamAndDispatch
- O que faz: `ClearObjectAttributeEntries(0,0x80)`; se `obj+0x3C == 4` chama `Dispatch_Sub4Mode(0)`; senão seta dois
  contadores para 2 e 0x10.
- RAM: OAM; struct +0x3C; contadores em literal 0x080079FC/0x08007A00.
- Chamada por: tabela 0x083AC86C, idx 5.
- Confiança: provável (despacho de saída da grade).

## 0x08007BD8 PartsGrid_BlendTickStep
- O que faz: `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `GpuBlend_TickStep`; se terminou, contador++.
- RAM: contador em literal 0x08007BF8.
- Chamada por: tabela 0x083AC86C, idx 9, 14 e 21.
- Confiança: provável (padrão blend+contador repetido).

## 0x08007BFC PartsGrid_LoadSlotStep
- O que faz: `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `Obj_InitSlotAndLoad(1,2,0)`, contador++.
- RAM: contador em literal 0x08007C1C.
- Chamada por: tabela 0x083AC86C, idx 10.
- Confiança: provável (carregamento de slot de sprite).

## 0x08007C20 PartsGrid_DispatchSubStep
- O que faz: `StateMachine_DispatchStep(*(obj+0x10))`; se o sub-estado retornar != 0, incrementa contador.
- RAM: struct +0x10 (ponteiro de sub-estado); contador em literal 0x08007C48.
- Chamada por: tabela 0x083AC86C, idx 11.
- Confiança: provável (despacho de sub-máquina, `StateMachine_DispatchStep` pelo nome).

## 0x08007C4C PartsGrid_CursorInputStep
- O que faz: `PartsGrid_StepCursorSkipFilled` + `PartsGrid_RedrawCellDetails`; lê entrada: ±1 coluna/linha com `& 3`
  (SFX 2), B/SEL (SFX 4 ou 5) e troca o estado guardado em ponteiro (0x14, 0x48).
- RAM: bytes de entrada (literal 0x08007C7C); campos +0x42 e +0x43 da struct.
- Chamada por: tabela 0x083AC86C, idx 12.
- Confiança: provável (lógica de cursor; valores 0x14/0x48 são estados sem nome).

## 0x08007D04 PartsGrid_BlendStartStep
- O que faz: `SetGpuBlendMode(6,0x3F,0x20)`, `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, contador++.
- RAM: contador em literal 0x08007D24.
- Chamada por: tabela 0x083AC86C, idx 13 e 20.
- Confiança: provável.

## 0x08007DDC FUN_08007DDC (sem rename)
- O que faz: `ClearObjectAttributeEntries(0,0x80)`; escreve um valor de posição (8, 0x18, 0x28 ou 0x38) conforme o
  byte `+0x42` (0 a 3).
- RAM: OAM; struct +0x42; ponteiro de saída em literal 0x08007E0C..0x08007E34.
- Chamada por: tabela 0x083AC86C, idx 22, 38 e 54.
- Confiança: chute (valores de posição sem prova de significado). Mantido FUN_.

## 0x0800801C PartsGrid_SlideDetailStep
- O que faz: `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `Slot_SetPosStepByMode(3, *, 8)`, `GpuBlend_TickStep`,
  contador ao terminar.
- RAM: contador em literal 0x0800804C.
- Chamada por: tabela 0x083AC86C, idx 25, 31 e 37.
- Confiança: provável (passo de posição do painel de detalhe).

## 0x08008050 PartsGrid_SlideLoadStep
- O que faz: como o anterior, mas em vez do blend chama `Obj_InitSlotAndLoad(1,2,0)` e incrementa o contador.
- Chamada por: tabela 0x083AC86C, idx 26.
- Confiança: provável.

## 0x08008084 PartsGrid_DetailDispatchStep
- O que faz: passo de posição (modo 8), depois `StateMachine_DispatchStep` com o estado escolhido por
  `PartsGrid_GetDetailValueIndex()` (vetor de ponteiros, 4 bytes por índice).
- RAM: vetor de ponteiros em literal 0x080080C0; contador em 0x080080C4.
- Chamada por: tabela 0x083AC86C, idx 27.
- Confiança: provável.

## 0x080080C8 PartsGrid_DetailInputStep
- O que faz: `PartsGrid_CycleDetailMode`; se não trocou, `StepCursorSkipFilled` e `RedrawDetailPanel`; lê entrada
  (SFX 2/4/5) e troca de estado (0x1E, 0x24, 0x48). Se o modo mudou, `Obj_ResetStateFields` e próximo estado.
- RAM: bytes de entrada (literal 0x08008134); campo +0x42.
- Chamada por: tabela 0x083AC86C, idx 28.
- Confiança: provável (ordem de SFX e estados sem nome).

## 0x080081BC PartsGrid_DetailDispatchStepRev
- O que faz: igual ao `PartsGrid_DetailDispatchStep`, com o contador decrementado.
- Chamada por: tabela 0x083AC86C, idx 29.
- Confiança: provável.

## 0x08008200 PartsGrid_BlendSlideStep
- O que faz: `SetGpuBlendMode(6,0x3F,0x20)`, `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `Slot_SetPosStepByMode(3, *, 8)`,
  contador++.
- Chamada por: tabela 0x083AC86C, idx 30 e 36.
- Confiança: provável.

## 0x08008430 PartsGrid_BlendTickStepB
- O que faz: `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `GpuBlend_TickStep`, contador++ ao terminar.
- RAM: contador em literal 0x08008450.
- Chamada por: tabela 0x083AC86C, idx 41, 46 e 53.
- Confiança: provável.

## 0x08008454 PartsGrid_LoadSlotStepB
- O que faz: `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `Obj_InitSlotAndLoad(1,2,0)`, contador++.
- Chamada por: tabela 0x083AC86C, idx 42.
- Confiança: provável.

## 0x08008478 PartsGrid_DispatchStepB
- O que faz: `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `StateMachine_DispatchStep(*(obj+0x30))`, contador++ ao terminar.
- RAM: struct +0x30; contador em literal 0x080084A0.
- Chamada por: tabela 0x083AC86C, idx 43.
- Confiança: provável.

## 0x080084A4 PartsGrid_CellInputStep
- O que faz: `PartsGrid_StepCursorSkipFilled`, `PartsGrid_RedrawCellPanelFull`; lê entrada: bit 0 limpa bits de slot
  (`Slot_ClearAttrMask(1,2)`, SFX 4) e vai para o estado 0x30; bit 1 e bit 2 mudam coluna/linha com SFX 2/5; sem entrada,
  não faz nada. Estados 0x34 e 0x48.
- RAM: bytes de entrada (literal 0x080084E8); campos +0x3F, +0x42, +0x43, +0x46.
- Chamada por: tabela 0x083AC86C, idx 44.
- Confiança: provável (mapa de bits de entrada inferido das chamadas; estados sem nome).

## 0x08008590 PartsGrid_BlendStartStepB
- O que faz: `SetGpuBlendMode(6,0x3F,0x20)`, `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, contador++.
- Chamada por: tabela 0x083AC86C, idx 45 e 52.
- Confiança: provável.

## 0x080085B4 PartsGrid_SetupDetailSprites
- O que faz: se `obj+0x3F == 0` chama `PartsGrid_SetupDetailCursorSprites`, senão `PartsGrid_SetupSlotCountSprite`;
  depois `Obj_ResetStateFields` e contador++.
- Chamada por: tabela 0x083AC86C, idx 48.
- Confiança: provável (nomes dos helpers do mtbe batem com a ação).

## 0x080085E8 PartsGrid_DispatchByCellStep
- O que faz: `Timer_IncPairIfFlag`; se `obj+0x3F == 0`, escolhe estado de uma tabela de ponteiros por celula
  (`+0x3D*0x40 + +0x46` + índice) e `StateMachine_DispatchStep`; senão despacha `obj+0x3C`. Contador++ ao terminar.
- RAM: tabela de estados em literal 0x08008624; campos +0x3C, +0x3D, +0x3F, +0x46.
- Chamada por: tabela 0x083AC86C, idx 49.
- Confiança: provável.

## 0x08008650 PartsGrid_CellMainInputStep
- O que faz: função grande de entrada da celula. Em modo normal (`obj+0x3F == 0`): ciclos de tile (`CycleCellTileIndex`),
  de detalhe (`CycleDetailIndex`) e de contagem (`CycleCellCountIndex` em modo 1), botões com SFX 4/5 que
  trocam para os estados 0x2B, 0x48 e alteram +0x46. Em modo de contagem: `Slot_SetPosYStepByMode(3,*,0x30)` e desenho.
- RAM: campos +0x3F, +0x46; bytes de entrada em literal 0x080086D0 e 0x08008760.
- Chamada por: tabela 0x083AC86C, idx 50.
- Confiança: provável (fluxo confirmado; significado dos estados 0x2B, 0x48 e 0x30 sem nome).

## 0x08008980 PartsGrid_IconStripStep
- O que faz: `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `PartsGrid_AnimateIconStrip`, `GpuBlend_TickStep`, contador++.
- RAM: contador em literal 0x080089A4.
- Chamada por: tabela 0x083AC86C, idx 57, 60 e 69.
- Confiança: provável.

## 0x080089A8 PartsGrid_IconStripInputStep
- O que faz: `AnimateIconStrip`, `StepCursorSkipFilled`, `RedrawCellPanelB`; navega a faixa (+0x42 com `& 3`, SFX 2)
  ou sai (SFX 5, contador++). Estado 0x44.
- RAM: bytes de entrada (literal 0x080089DC); campo +0x42.
- Chamada por: tabela 0x083AC86C, idx 58.
- Confiança: provável.

## 0x08008A34 PartsGrid_IconStripBlendStep
- O que faz: `SetGpuBlendMode`, `Timer_IncPairIfFlag`, `Sprite_ToggleFlag4000PairC`, `AnimateIconStrip`, contador++.
- Chamada por: tabela 0x083AC86C, idx 59 e 68.
- Confiança: provável.

## 0x08008A5C PartsGrid_IconAnimStep
- O que faz: `PartsGrid_SetupIconAnimSprite`; anima o contador de frame +0x48 (0..7) conforme bits 0x40 e 0x80 do byte de
  estado; sem bits, desenha as tiras de bytes (`PartsGrid_DrawSlotBars`, `PartsGrid_DrawCellByteFields8`).
- RAM: campos +0x46, +0x48, +0x3D; OAM/VRAM via helpers.
- Chamada por: tabela 0x083AC86C, idx 64.
- Confiança: provável (animação de frame confirmada no código; uso visual é chute).

## 0x08008B80 PartsGrid_ClearAndInitStep
- O que faz: `Timer_IncPairIfFlag`, `Slot_ClearAttrMask(1,5)`, `bios_cpufastset` (limpa VRAM/paleta), `Sprite_SetupFrameOffsetAndAttr`,
  `Obj_ResetStateFields`, contador++.
- RAM: VRAM/paleta via CpuFastSet (ponteiros em literal 0x08008BAC/0x08008BB0).
- Chamada por: tabela 0x083AC86C, idx 72.
- Confiança: provável.

## 0x08008BB8 PartsGrid_DispatchStepC
- O que faz: `Timer_IncPairIfFlag`, `StateMachine_DispatchStep(*(obj+0x14))`, contador++ ao terminar.
- RAM: struct +0x14; contador em literal 0x08008BDC.
- Chamada por: tabela 0x083AC86C, idx 73.
- Confiança: provável.

## 0x08008BE0 PartsGrid_SlotScriptInputStep
- O que faz: `Timer_IncPairIfFlag`, `Anim_RunSlotScript(*(obj+0x0C), 6)` (roteiro de animação do slot); lê o byte de
  entrada: 0x40 anda para trás no contador +0x44, 0x80 anda para frente (`+0x45 - 1`), SFX 2 e `Sprite_UpdateFrameOffsetXY`;
  sem esses bits, botão A/B com SFX 4/5 troca estado (0x4E, 0x40 e seguintes).
- RAM: struct +0x0C, +0x44, +0x45; bytes de entrada em literal 0x08008C7C.
- Chamada por: tabela 0x083AC86C, idx 74.
- Confiança: provável (roteiro de slot confirmado pelo nome `Anim_RunSlotScript`; estados sem nome).

## 0x08008CA0 PartsGrid_SubStateSwitchStep
- O que faz: `Timer_IncPairIfFlag`; escolhe um handler conforme `+0x42` (0, 1 ou 2) e `+0x44`. Para 0 e 1 chama um
  handler de uma jump table; para 2 chama `StateMachine_DispatchStep` em um de dois ponteiros. Se o retorno != 0, reseta
  campos e vai para o estado 0x49.
- RAM: struct +0x42, +0x44; ponteiros de literal 0x080090A4, 0x080090C8, 0x080090CC.
- Chamada por: tabela 0x083AC86C, idx 75.
- Confiança: provável. Jump table em 0x08008D72 e 0x08008CDA NÃO recuperada pelo Ghidra (Too many branches); os alvos
  ficam em aberto.

## 0x080090D0 PartsGrid_SpritePairStep
- O que faz: `Timer_IncPairIfFlag`, `Slot_ClearAttrMask(6,1)`, `bios_cpufastset`; conforme `+0x42` (0, 1, 2) faz init de
  sprite pair, tamanho C, e vai para o estado 0x0B, 0x1B ou 0x2B; no 2 testa bit 0x80 de +0x46 e vai para 0x30.
- RAM: struct +0x42, +0x46; ponteiros de literal 0x08009100..0x08009108.
- Chamada por: tabela 0x083AC86C, idx 78.
- Confiança: provável.

## 0x080091E0 PartsGrid_OptionRowStep
- O que faz: `Timer_IncPairIfFlag`, `PartsGrid_RefreshCursorSprite`, `Slot_SetPosStepByMode(7, *, 0xA8)`,
  `PartsGrid_CycleOptionRow3` (ciclo de 3 opções); bit 1 dispara SFX 5 e `+0x4A = 2`, bit 0 dispara SFX 4; depois
  `PartsGrid_SetupRowSelectSprite` e contador++.
- RAM: bytes de entrada em literal 0x08009214; campo +0x4A.
- Chamada por: tabela 0x083AC86C, idx 97.
- Confiança: provável (a linha de opções é o único uso de modo 7 nesta faixa).

## 0x08009248 PartsGrid_OptionHandlerStep
- O que faz: `Timer_IncPairIfFlag`, `PartsGrid_RefreshCursorSprite`, `PartsGrid_DispatchOptionHandler`; se o handler
  retornar != 0, `Slot_ClearAttrMask(7,1)` e contador++.
- RAM: contador em literal 0x08009270.
- Chamada por: tabela 0x083AC86C, idx 98.
- Confiança: provável.
