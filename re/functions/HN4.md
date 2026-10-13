# HN4: handlers dos despachantes Fight_DispatchOnSubstateAA_BA..BG

Lista: `HN4_lista.txt` (40 entradas). Cada handler é entrada da tabela indexada por +0xAA do bloco `0x03001800`
(entrada = tabela + 4*índice). Tabelas lidas na ROM EU (literal stub+0x24): BA `0x083AE780`, BB `0x083AE7C0`,
BC `0x083AE800`, BD `0x083AE840`, BE `0x083AE880`, BF `0x083AE8D0`, BG `0x083AE910`, BH `0x083AE954`.
Resultado: 40 de 40 decompilados (todos já são funções depois do CreateHandlerFunctions) e renomeados no Ghidra.

## Contexto: que passo da Robattle é
Estes despachantes são sequências de ANIMAÇÃO + EFEITO de técnicas sem dano direto (status, buff de time, efeito
no medidor), rodadas na fase de ação depois que o ataque é escolhido. Padrão de todas:
gráficos (LZ77 + CpuFastSet + slots OAM 0x21..0x23 em `0x03002C30+0x840`) -> animação/espera -> aplica o efeito
nos blocos dos bots (`0x03000590 + bot*0x100`) -> grava `+0xA6` (código da mensagem/passo seguinte) -> handler
comum `0x080305E0` roda `Fight_RunSubstateDispatch(+0xA6)` e avança +0xAA.
Campos do bloco 0x03001800 usados: +0x87 bot ativo, +0x60 base do lado (0 ou 3, via
`Fight_GetAllySideBaseIndex`/`Fight_GetOpponentSideBaseIndex`), +0xD2..+0xD4 flag "bot afetado" (3 bytes),
+0xD6..+0xDA valores por bot (3 halfwords), +0xEC valor do efeito (mesmo campo do retorno do dano), +0xE8 valor
secundário, +0x51 tipo de stat (BF), +0x86 timer de frames, +0x84 contador.
Handlers comuns (fora da lista): `0x080305E0` = roda passo +0xA6; `0x08030674` = espera +0x86 frames (A pula);
`0x0803080C` = grava +0xB5=1 (sequência concluída, sem efeito); `0x08022648`/`0x080231D4`/`0x08023224` = HA6/W/Z.

## Cadeias (índice +0xAA)
- BA: ... 09 ShowValueNumbers -> 0A (0x08029D28) -> 0B SetMessage54 -> 0C run A6 -> 0D fim.
- BB: 00 LoadEffectGfx -> 01 ScaleEffectUp -> 02 espera -> 03 MarkOpponentsSetGauge -> 04 AnimOpponentGauges -> 05 msg 0x65 -> 06 run A6.
- BC: 00 LoadTwinEffectGfx -> 01/02 (W 0x08022648/0x08022708) -> 03 AddValueToBotD1 -> 04 msg 0x45 -> 05 run A6.
- BD: 00 LoadTwinEffectGfx -> 01 (W) -> 02 RunEffectAnim -> 03 LoadSecondEffect -> 04 RunSecondEffectAnim -> 05 AddValueToBotD0 -> 06 msg 0x44 -> 07 run A6.
- BE: 00/01 (Z, escurece) -> 02 LoadEffectGfx -> 03/04 (Z) -> 05 SelectTargets (nenhum: 06 = fim B5=1; algum: +2) -> 07 SetupTargetBeams -> 08 StepBeamsToTargets -> 09 LoadHitAnim -> 0A RunHitAnim -> 0B RollRandomStatus -> 0C (0x08028B58) -> 0D espera -> 0E ApplyRandomStatus -> 0F msg 0x4D/0x4E -> 10 run A6.
- BF: 00 SelectAlliesAndStat -> 01 LoadEffectAboveAllies -> 02 DropEffectOntoAllies -> 03 LoadSecondAnim -> 04 RunSecondAnim -> 05 ShowStatIcon -> 06 FloatIconUp -> 07 espera -> 08 CopyValueToNumbers -> 09 (=BA ShowValueNumbers) -> 0A (0x08029D28) -> 0B ApplyStatBoost -> 0C msg 0x3D/0x3E/0x40/0x41 -> 0D run A6.
- BG: 00..02 (Z, escurece) -> 03 SetupCenterEffect -> 04 FlashCenterEffect -> 05 (Z TickCounterDown) -> 06 MarkBothSides -> 07 SetupBothSideSprites -> 08 MoveBothSideSprites -> 09 espera -> 0A..0C (outra lista) -> 0D run A6. BH 00 reusa BG 06.

## 0x0802A5C0 Fight_DispatchOnSubstateAA_BA_ShowValueNumbers (BA 0x09; também BF 0x09)
- Para cada bot afetado (+0xD2+i): `Fight_WriteNumberTiles` com +0xD6/+0xD8/+0xDA e monta OAM (`Fight_BuildBotOamEntriesAltTiles`, slot 0x21).
- Antes descomprime um gráfico de números. sfx 0x5B; zera `0x03000440+0x84`; +0xAA +1 (-> 0x0A).
- Passo: mostrar o valor aplicado (ex.: aumento de stat) sobre cada bot. Confiança: confirmado (decompile); nome provável.

## 0x0802A694 Fight_DispatchOnSubstateAA_BA_SetMessage54 (BA 0x0B)
- Grava +0xA6 = 0x54 e +0xAA +1 (-> 0x0C, run A6). Confiança: confirmado.

## 0x0802A6D4 Fight_DispatchOnSubstateAA_BB_LoadEffectGfx (BB 0x00)
- LZ77 + 2x CpuFastSet(8); objeto 0x840 na posição do bot ativo (x, y-4), espelha (|0x1000, x 0x80/0x70) se +0xF8 bit0.
- `Slot_SetupFromRecordList(rec+0x188, 0x21)`; sfx 0x6A; +0xAA +1 (-> 0x01). Confiança: confirmado; nome provável.

## 0x0802A7E0 Fight_DispatchOnSubstateAA_BB_ScaleEffectUp (BB 0x01)
- Soma 4 aos bytes `0x03002C30+0x848/+0x849` (escala do slot) até 0x40; então timer +0x86 = 0x3C e +0xAA +1 (-> 0x02 espera).
- Confiança: confirmado.

## 0x0802A824 Fight_DispatchOnSubstateAA_BB_MarkOpponentsSetGauge (BB 0x03)
- `FightPreview_ClearPartCounters3`; +0x60 = lado oponente. Para cada oponente vivo (+0x40) com +0x59 != 0:
  bot+0xBD |= 4, +0xD2+i = 1, `FightPreview_ClearPairsForBot`, bot+0xD7 = 0, bot+0xBB (medidor) = 0x14, bot+0xDA = 0; cria sprite 0x22+i.
- `Fight_PlaceActiveBotObjects`, `Fight_SortBotEventQueue`; +0x86 = 0; +0xAA +1 (-> 0x04).
- Passo: efeito no medidor/ação de todo o time inimigo (empurra o medidor). Confiança: confirmado (código); efeito de jogo provável.

## 0x0802A9B8 Fight_DispatchOnSubstateAA_BB_AnimOpponentGauges (BB 0x04)
- Para cada afetado: `Fight_UpdateBotMeterDisplayObject`, `Fight_UpdateBotScreenXFromMeterInverted`, anima o sprite 0x22+i
  seguindo o bot. sfx 0x33 a cada 64 frames; quando todos chegaram e +0x86 > 0x3C: limpa slots 0x21..0x3B, +0xAA +1.
- Confiança: confirmado; nome provável.

## 0x0802AAB8 Fight_DispatchOnSubstateAA_BB_SetMessage65 (BB 0x05)
- +0xA6 = 0x65; +0xAA +1 (-> 0x06 run A6). Confiança: confirmado.

## 0x0802AAF8 Fight_DispatchOnSubstateAA_BC_LoadTwinEffectGfx (BC 0x00)
- LZ77 + CpuFastSet; dois objetos (slots 0x21/0x22, rec+0x70) em x-0x20 e x+0x20 do bot ativo; sfx 0x33; +0xAA +1.
- Confiança: confirmado; nome provável.

## 0x0802AC7C Fight_DispatchOnSubstateAA_BC_AddValueToBotD1 (BC 0x03)
- `Fight_AddByteClampToMax(0x03000590 + ativo*0x100 + 0xD1, +0xEC, 0xFF)`; +0xAA +1 (-> 0x04).
- Passo: aplica o efeito (soma no byte +0xD1 do bot ativo; significado do campo não identificado). Confiança: confirmado.

## 0x0802ACB4 Fight_DispatchOnSubstateAA_BC_SetMessage45 (BC 0x04)
- +0xA6 = 0x45; +0xAA +1 (-> 0x05 run A6). Confiança: confirmado.

## 0x0802ACF4 Fight_DispatchOnSubstateAA_BD_LoadTwinEffectGfx (BD 0x00)
- Igual ao BC 0x00 (dois objetos x±0x20 do bot ativo), registro rec+0x190; sfx 0x33; +0xAA +1. Confiança: confirmado.

## 0x0802AE88 Fight_DispatchOnSubstateAA_BD_RunEffectAnim (BD 0x02)
- `Anim_RunSlotScript(rec+0x190, 0x21)` até `0x03002C30+0x862 == 0xFF`; limpa slot 0x21; +0xAA +1. Confiança: confirmado.

## 0x0802AECC Fight_DispatchOnSubstateAA_BD_LoadSecondEffect (BD 0x03)
- Objeto 0x840 no bot ativo (x, y-4), rec+0x194, slot 0x21; sfx 0x71; +0xAA +1. Confiança: confirmado.

## 0x0802AF6C Fight_DispatchOnSubstateAA_BD_RunSecondEffectAnim (BD 0x04)
- Como BD 0x02 com rec+0x194; ao fim limpa slot 0x21; +0xAA +1. Confiança: confirmado.

## 0x0802AFB0 Fight_DispatchOnSubstateAA_BD_AddValueToBotD0 (BD 0x05)
- `Fight_AddByteClampToMax(0x03000590 + ativo*0x100 + 0xD0, +0xEC, 0xFF)`; +0xAA +1. Par do BC (BC usa +0xD1).
- Confiança: confirmado.

## 0x0802AFE8 Fight_DispatchOnSubstateAA_BD_SetMessage44 (BD 0x06)
- +0xA6 = 0x44; +0xAA +1 (-> 0x07 run A6). Confiança: confirmado.

## 0x0802B028 Fight_DispatchOnSubstateAA_BE_LoadEffectGfx (BE 0x02)
- LZ77 + CpuFastSet(16); dois objetos no bot ativo (escala 0x40, tile 0x7A00). Variante de tile escolhida por
  bot ativo +0xA8 == 0x1E (técnica). sfx 0x4C; +0xAA +1. Confiança: confirmado; nome provável.

## 0x0802B184 Fight_DispatchOnSubstateAA_BE_SelectTargets (BE 0x05)
- Oponentes vivos, +0x59 != 0 e `Fight_HasNoTechnique16Part`: se ativo +0xA8 == 0x1E marca (+0xD2+i = 1) quem tem +0x91 == 0x64;
  senão marca (=2) quem tem +0x91 == 0x6A.
- Algum marcado: +0xAA += 2 (-> 0x07); nenhum: +1 (-> 0x06 = `0x0803080C`, +0xB5 = 1, fim sem efeito). Confiança: confirmado.

## 0x0802B258 Fight_DispatchOnSubstateAA_BE_SetupTargetBeams (BE 0x07)
- Para cada alvo: objeto 0x21+i sai do bot ativo (x, y-4) com destino no alvo (+0x30/+0x32 do slot); tile 0x7B00 (flag 1) ou outro (flag 2).
  sfx 0x24; +0xAA +1. Confiança: confirmado; nome provável.

## 0x0802B388 Fight_DispatchOnSubstateAA_BE_StepBeamsToTargets (BE 0x08)
- `Fight_StepSlotTowardTarget(0x21+i)` para cada alvo; quando todos chegaram, +0xAA +1. Confiança: confirmado.

## 0x0802B3CC Fight_DispatchOnSubstateAA_BE_LoadHitAnim (BE 0x09)
- LZ77 + CpuFastSet(8); para cada alvo: `Slot_SetupFromRecordList(rec, 0x21+i)`; +0xAA +1. Confiança: confirmado.

## 0x0802B460 Fight_DispatchOnSubstateAA_BE_RunHitAnim (BE 0x0A)
- `Anim_RunSlotScript` nos slots dos alvos; quando algum termina (byte de estado 0xFF), +0xAA +1. Confiança: confirmado.

## 0x0802B4D8 Fight_DispatchOnSubstateAA_BE_RollRandomStatus (BE 0x0B)
- Para cada alvo: +0xD6+2i = `randbyte() & 7`. Se 0..3, testa `FUN_08036d64(bot, 0x0D/0x11/0x13/0x10)`; se != 0, troca para 5.
  Mostra o ícone (tile 0x6F00 + valor) no slot 0x21+i; sfx 0x5B; +0xAA +1. Confiança: confirmado; leitura do teste provável.

## 0x0802B5AC Fight_DispatchOnSubstateAA_BE_ApplyRandomStatus (BE 0x0E)
- Limpa slots 0x21..0x23. Por alvo, conforme +0xD6+2i: 0..3 -> `FUN_08036e64(bot, 0x0D/0x11/0x13/0x10, +0xE8, +0xEC)`;
  4/5/6/7 -> bot+0xC9 = 0x2B/0x2C/0x2E/0x2F, +0xCA (1 só no 4), +0xCB/+0xCC = `FUN_08036e54(+0xE8/+0xEC)`. +0xAA +1.
- Passo: aplica um status aleatório (sorteado em 0x0B) em cada alvo. Confiança: confirmado (código); "status" provável.

## 0x0802B7AC Fight_DispatchOnSubstateAA_BE_SetResultMessage (BE 0x0F)
- +0xA6 = 0x4D se ativo +0xA8 == 0x1E, senão 0x4E; +0xAA +1 (-> 0x10 run A6). Confiança: confirmado.

## 0x0802B810 Fight_DispatchOnSubstateAA_BF_SelectAlliesAndStat (BF 0x00)
- +0x60 = lado aliado; marca +0xD2+i nos aliados vivos com +0x59 != 0. Técnica do ativo (+0xA8): 0x1F/0x21/0x23/0x25 ->
  +0x51 = 0/1/2/3 (qual stat sobe). +0xAA +1. Confiança: confirmado.

## 0x0802B8BC Fight_DispatchOnSubstateAA_BF_LoadEffectAboveAllies (BF 0x01)
- LZ77 + CpuFastSet(16); para cada aliado marcado: objeto 0x21+i em x do bot, y = 0xF0 (fora da tela), rec+0x19C; sfx 0x2C; +0xAA +1.
- Confiança: confirmado.

## 0x0802B980 Fight_DispatchOnSubstateAA_BF_DropEffectOntoAllies (BF 0x02)
- Anima e desce y do objeto +1 por frame até y do bot - 4; quando todos chegam, +0xAA +1. Confiança: confirmado.

## 0x0802BA2C Fight_DispatchOnSubstateAA_BF_LoadSecondAnim (BF 0x03)
- Reconfigura os objetos dos aliados marcados e `Slot_SetupFromRecordList`; sfx 0x30; +0xAA +1. Confiança: confirmado.

## 0x0802BAAC Fight_DispatchOnSubstateAA_BF_RunSecondAnim (BF 0x04)
- `Anim_RunSlotScript` nos slots marcados até um terminar (0xFF); +0xAA +1. Confiança: confirmado.

## 0x0802BB24 Fight_DispatchOnSubstateAA_BF_ShowStatIcon (BF 0x05)
- Objetos marcados: y += 8, tile = base + +0x51 (ícone do stat); +0x86 = 0; sfx 0x2E; +0xAA +1. Confiança: confirmado; nome provável.

## 0x0802BBA8 Fight_DispatchOnSubstateAA_BF_FloatIconUp (BF 0x06)
- y dos ícones -1 a cada 2 frames, por 0x20 frames; então +0x86 = 0x3C e +0xAA +1 (-> 0x07 espera). Confiança: confirmado.

## 0x0802BC1C Fight_DispatchOnSubstateAA_BF_CopyValueToNumbers (BF 0x08)
- Limpa slots 0x21..0x23; copia +0xEC para +0xD6+2i dos marcados (valor exibido pelo 0x09); +0xAA +1. Confiança: confirmado.

## 0x0802BC60 Fight_DispatchOnSubstateAA_BF_ApplyStatBoost (BF 0x0B)
- Por aliado marcado: `Fight_AddByteClampToMax(0x030005D8 + bot*0x100 + {0x4D,0x4E,0x50,0x51}[+0x51], +0xEC, 0xFF)`
  (= bloco 0x03000590 +0x95/+0x96/+0x98/+0x99); depois `Fight_BackupHalvedStatsIfFlagClear` e `Fight_CopyStatsIfFlagSet`. +0xAA +1.
- Passo: aplica o buff de stat ao time aliado. Confiança: confirmado (código); quais stats são, não identificado.

## 0x0802BD1C Fight_DispatchOnSubstateAA_BF_SetStatMessage (BF 0x0C)
- +0xA6 = 0x3D/0x3E/0x40/0x41 conforme +0x51 = 0/1/2/3; +0xAA +1 (-> 0x0D run A6). Confiança: confirmado.

## 0x0802BD94 Fight_DispatchOnSubstateAA_BG_SetupCenterEffect (BG 0x03)
- Um objeto fixo no centro (x 0x78, y 0x40, escala 0x40, tile 0x4800); `0x03000440+0x86` = 0 e +0x84 = 0; +0xAA +1.
- Confiança: confirmado; nome provável.

## 0x0802BE44 Fight_DispatchOnSubstateAA_BG_FlashCenterEffect (BG 0x04)
- 3 pulsos (contadores `0x03000440+0x84` < 3 e +0x86, duração por tabela de bytes; sfx 0x4C em cada pulso) e um contador de fase que vai de 0 a 5
  em passos de 0x10; ao fechar a fase 5 limpa slot 0x21 e +0xAA +1. Confiança: confirmado (código); nome provável.

## 0x0802BEEC Fight_DispatchOnSubstateAA_BG_MarkBothSides (BG 0x06; também BH 0x00)
- `FightPreview_ClearPartCounters3`; marca aliados vivos (+0xD2+i = 1) e oponentes vivos (+0xD6+2i = 1), ambos com +0x59 != 0.
  +0xAA +1. Passo: efeito de campo que atinge os dois times. Confiança: confirmado.

## 0x0802BF9C Fight_DispatchOnSubstateAA_BG_SetupBothSideSprites (BG 0x07)
- Sprites 0x840.. nos aliados marcados (no bot) e 0x900.. nos oponentes marcados (y-0x10), escala 0x40, tiles diferentes por lado.
  `0x03000440+0x84` = 0; sfx 0x2E; +0xAA +1. Confiança: confirmado.

## 0x0802C0DC Fight_DispatchOnSubstateAA_BG_MoveBothSideSprites (BG 0x08)
- A cada 2 frames: sprites aliados y-1, oponentes y+1. `0x03000440+0x84` conta até 0x20; então +0x86 = 0x3C e +0xAA +1 (-> 0x09 espera).
- Confiança: confirmado.
