# BI2: continuação do BI (quem chama, callbacks de VBlank, ops G/H, H05..H12, genéricos)

Lido no Ghidra (decompile, xrefs e pool da ROM original via python -I). Sem emulador. Não editei mtbe.csv nem HA2/BI.
Renames feitos no Ghidra (em memória, não salvei): ver "Renames" no fim.

## (1) Quem chama 0x0801D0A8: nada chama ele; é miolo de A/22

- 0x0801D0A8 não é entrada de função. Fica no meio de `Fight_DispatchOnSubstateAA_SetupBotAndSprites`, que é
  a família **A, índice 0x22**, entrada **0x0801D06C** (264 bytes). Não há BL nem literal apontando para 0x0801D0A8/A9.
- Quem chama A/22: só a tabela A. Xref DATA em `0x083AD654` (= `0x083AD5CC + 4*0x22`, tabela do stub A 0x0801C4FC).
  Ou seja, é chamado pelo stub A, por `table[+0xAA] == 0x22`. Quem dispara o stub A: NÃO resolvido nesta rodada.
- O que A/22 escreve (decompile de 0x0801D06C; base do bloco `0x03002C30 + bot*0x100`, pool 0x0801D0D8):
  `+0x840` (halfword), `+0x85C = 0`, `+0x84A = 6`, `+0x849 = 0x40`, `+0x848 = 8`, `+0x84B = 0xF0`,
  halfword `+0x844 = 0x3C`, `+0x89` do bloco de luta decide o lado (0x68 ou 0x88), `+0x51` escolhe a posição
  (0xB00/0xC00/0xD00/0xE00), e `+0xAA += 1` no fim.
- Consequência para o BI: os valores 8/0x40/6/0xF0 em 0x03003478..0x0300347B estão certos (0x03002C30+0x848..0x84B),
  mas são escritos pelo A/22, não por um setup da família E. O contador 0x03003478 é inicializado aqui (idx A/22).
- Confiança: confirmado (decompile + pool + xref DATA). Em aberto: quem dispara o stub A (xref de 0x0801C4FC).

## (2) Callback de um quadro em 0x030017F0 (VBlankIrqHandler 0x080007E0)

- VBlank: `VBlankIrqHandler` lê `*0x030017F0` (xref READ em 0x080007E4) e chama. Confirmado (xref).
- Valor 0x08043E49 (Thumb) -> função **0x08043E48**: decompile = wrapper `Obj_StepAccumMoveAWrapper`, que só chama
  `Obj_StepAccumMoveA` e volta. Efeito por quadro: avança o acumulador de movimento do objeto A.
  Confiança: confirmado (decompile do wrapper); provável (o efeito completo fica em Obj_StepAccumMoveA, não lido aqui).
- Valor 0x08044101 (Thumb) -> função **0x08044100** = `Obj_StepAccumMoveBThenSync`: chama `Obj_StepAccumMoveB`
  e depois `FUN_08043fec(byte bloco+0x38)`. Confiança: confirmado (decompile).
- Valor 0x08000841 (Thumb) -> `NoOpActionHandler` (0x08000840): `return`. É o "desarmar" padrão.
  Confiança: confirmado.
- Valor 0x08049CC9 (Thumb) em 0x08049CAC (FUN_08049a6c): o alvo 0x08049CC8 é `Obj_LoadPairFromRecord` (copia 6 bits
  de dois bytes do registro). Provável: é o callback quando o estado do fade chega ao passo final; o destino
  do store (`DAT_08049af8`/`DAT_08049ac8`) não foi confirmado como 0x030017F0.

### Todos os escritores de 0x030017F0 (xref WRITE, 21 pontos) e o valor gravado

| Função (endereço do store) | Valor | O que o callback faz |
|---|---|---|
| InitializeInterruptsAndDefaultHandlers (0x0800047E) | 0x08000841 (nop) | padrão no boot. Provável |
| Menu_TickSlideInPanelFade (0x080030BE) | nop | desarma ao fim do slide. Confirmado |
| FUN_08049A6C (0x08049A80) | nop | desarma. Confirmado |
| FUN_08049A6C (0x08049CAC) | 0x08049CC9 (Obj_LoadPairFromRecord) | ver acima. Provável |
| Overworld_SetTransitionStepAndDispatch (0x080499A2) | não resolvido | xref WRITE, mas o decompile não mostra store nesse slot |
| Gfx_PaletteFadeStep (0x08010B10) | nop | fim do fade de paleta. Confirmado |
| Gfx_ResetBgLayersAndSong (0x08011580) | nop | desarma. Confirmado |
| Fx_InitBlendTableAndTick (0x08011680) | nop | desarma. Confirmado |
| Scene_InitBgAndLoadRecord (0x080113B4) | nop | desarma. Confirmado |
| Gfx_ResetScreenBgAndBlend (0x08011C32) | nop | desarma. Confirmado |
| Dispatch_ByteStateIndexTable (0x08013A3C) | não resolvido | é só dispatcher por tabela; store não visível |
| Fight_PartPanelStep (0x08012B14) | 0x08044101 (Obj_StepAccumMoveBThenSync) | sincroniza objeto B por quadro. Confirmado (pool) |
| Init_CopyTileSetsAndSetBlend (0x080137E0) | nop | desarma. Confirmado |
| G02 Fight_DispatchOnSubstateAA_G_02 (0x0801E070) | nop | desarma ao fim das 3 etapas de BG. Confirmado |
| **G03 Fight_DispatchOnSubstateAA_G_03 (0x0801E1C8)** | **0x08043E49** (Obj_StepAccumMoveA) | cenário de luta: acumulador A por quadro. Confirmado |
| J02 SubDispatch (0x0801FD48) | 0x08044101 (BI) | não vi o store na decompile (fica no passo 4, 0x0801FD3C). Provável |
| J01 Fight_DispatchOnSubstateAA_J_01 (0x0801FB7A) | nop | desarma. Confirmado |
| SceneStep53 Fight_SceneStep53_ExitBattleScene (0x08031340) | nop | desarma ao sair da luta. Confirmado |
| Overworld_RunScreenTransitionStep (0x08049EA4) | nop | desarma ao fim da transição. Confirmado |
| scene_init_b (0x080694F8) | **0x08043E49** (Obj_StepAccumMoveA) | mesma do G03 na cena b. Confirmado (pool) |
| scene_init_c (0x0806962C) | **0x08044101** (Obj_StepAccumMoveBThenSync) | mesma do PartPanel na cena c. Confirmado (pool) |
| Fx_StepTableBThenSetDispcnt (0x08068ECC) | não resolvido | o store visível vai para 0x0300137C, não 0x030017F0 |

Leitura: 0x030017F0 é um "slot de callback por quadro" do VBlank. Quase sempre é nop; durante a luta é 0x08043E49
(G03/scene_init_b) ou 0x08044101 (PartPanel/scene_init_c/J02). Não há evento externo que o libere além do próprio
passo que o troca.

## (3) Ops de script que conduzem G e H (mesmo padrão de E/F)

- **G = `ActionScript_Op1A_DispatchOnSubstateAA_G`** (xref para o stub G 0x0801DEE4 em 0x08034056). Decompile:
  seta `byte [DAT_0803406c + 0x130] = 1`; chama o stub G; `if (byte [DAT_08034070 + 0xB5] != 0)` chama
  `Fight_AdvanceActionScriptCursor`.
- **H = `ActionScript_Op1E_CallSubstateH`** (xref para o stub H 0x0801E2D8 em 0x080341DE). Decompile:
  `FightPreview_UpdateSingleLayerSideSprites()`, depois `FightPreview_AdvanceLeftSideSequenceStep()` e
  `FightPreview_AdvanceRightSideSequenceStep()` gravando os resultados em bytes do bloco (+DAT_080341fc e +0x160);
  chama o stub H; `if (byte [DAT_080341f8 + 0xB5] != 0)` chama `Fight_AdvanceActionScriptCursor`.
- Stubs (confirmados pelos bytes: literal do pool em stub+0x24 aponta para a tabela): G = 0x0801DEE4 (tabela
  0x083AD750), H = 0x0801E2D8 (tabela 0x083AD790). Também E = 0x0801D580 (Op09), F = 0x0801DE1C (Op19), A = 0x0801C4FC.
- Confirmado: o avanço é condicionado a +0xB5 != 0 em G e H (decompile), igual a E e F.
- Provável: os blocos DAT_0803406c/DAT_08034070/DAT_080341f8 são o 0x03001800 (fight_information). Não li o pool.
- Correção ao HA2/BI: F02, G06 e H15 marcam o fim do passo (+B5=1), e o op faz o avanço no mesmo quadro.

## (4) H05, H09, H0C, H0F, H12 e genéricos

Tabela H (0x083AD790) lida na ROM:
- **H05, H09, H0C, H12 = 0x08030625** -> função 0x08030624 = `Fight_DispatchOnSubstateAA_H_19_WaitTimer60ClearSlot10`.
  Ou seja, são o mesmo código que H19: espera o timer `0x030004C6` chegar a 0 e limpa o slot 10, depois `+0xAA += 1`.
  Isso corrige o HA2, que dizia "gen 0x080305E1" para H05 e H09. O timer é armado pelo passo anterior com 0x3C
  (H04 antes de H05, H08 antes de H09, H0B/H0A antes de H0C, H11 antes de H12). Confirmado (tabela + decompile).
- **H0F = 0x080306B9** -> função 0x080306B8, renomeada `Fight_DispatchOnSubstateAA_H_0F_WaitTimerClearSlots`
  (antes `ActionFamily_Change_Handler`). Decompile: se o byte `PTR_DAT_08030724+0x89 == 0` e o bit 0 de
  `*PTR_DAT_08030728` está ligado, limpa os slots 10..29 que têm bit 0x8000 (`ClearObjectAttributeEntries`), `+1` em
  `PTR_DAT_08030730+0xAA`. Depois: se o halfword `+0x86` é 0, repete a limpeza (slots 10..29) e `+1` em
  `DAT_0803077c+0xAA`; senão decrementa `+0x86`. Confirmado (decompile); provável (que é o fim da fase de limpeza da H0E).
- **0x080305E1** (genérico, usado pelas outras famílias): `ActionFamily_StatusCheckFamily_Handler`. Chama
  `Fight_RunSubstateDispatch(byte [DAT_08030604 + 0xA6])`; se retorna != 0 faz `+0xAA += 1`. Confirmado.
- **0x08000841** (genérico nop): `NoOpActionHandler` (retorna). Confirmado.

Não há mais "H05/H09/H0C/H0F/H12 não documentados": agora estão aqui.

## Renames no Ghidra (nesta rodada)

- 0x0801D06C: `Fight_DispatchOnSubstateAA_SetupBotAndSprites` -> `Fight_DispatchOnSubstateAA_A_22_SetupBotAndSprites` (confirmado: xref da tabela A idx 0x22).
- 0x080306B8: `ActionFamily_Change_Handler` -> `Fight_DispatchOnSubstateAA_H_0F_WaitTimerClearSlots` (provável).
- Nomes já existentes e conferidos: 0x08043E48 `Obj_StepAccumMoveAWrapper`, 0x08044100 `Obj_StepAccumMoveBThenSync`,
  0x08030624 `Fight_DispatchOnSubstateAA_H_19_WaitTimer60ClearSlot10`, 0x080305E1 `ActionFamily_StatusCheckFamily_Handler`,
  0x08000841 `NoOpActionHandler`, 0x08049CC8 `Obj_LoadPairFromRecord`.

## Em aberto

- Quem dispara o stub A (0x0801C4FC): xref do stub, para achar a origem do idx 0x22 e da fase do setup.
- Overworld_SetTransitionStepAndDispatch, Dispatch_ByteStateIndexTable e Fx_StepTableBThenSetDispcnt: xref WRITE
  para 0x030017F0 sem store visível na decompile (pode ser store indireto). Não resolvido.
- Store exato de J02 Step4 (0x0801FD3C) e o destino do store em FUN_08049A6C (0x08049CAC).
- Conteúdo de Obj_StepAccumMoveA e FUN_08043FEC (efeito por quadro dos dois callbacks).
- Pool dos ops G/H (blocos DAT_08034070 / DAT_080341f8): confirmar que é 0x03001800.
- Correção pendente no HA2: H05/H09/H0C/H12 não são 0x080305E1.
