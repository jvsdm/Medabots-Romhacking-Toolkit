# HA8: handlers da tabela dos despachantes Fight_DispatchOnSubstateAA_AH..AP

Lista: `HA8_lista.txt` (42 handlers). Cada handler é entrada de tabela indexada por +0xAA (subestado) do bloco
0x03001800. Conferido na ROM: todas as 42 linhas batem com `tabela_do_stub[idx]` (ponteiro com bit Thumb).
Tabelas (literal stub+0x24): AH 0x083AE124, AI 0x083AE184, AJ 0x083AE1E4, AK 0x083AE264, AL 0x083AE280,
AM 0x083AE298, AN 0x083AE2EC, AO 0x083AE340, AP 0x083AE3C0. Bloco (literal stub+0x20): 0x03001800.

Resultado: 18 handlers com função no Ghidra (16 renomeados nesta rodada, 2 já tinham nome); 24 sem função no
Ghidra (`No function found`), código não lido.

Convenção usada: "+1" = avança para o próximo subestado; "=0x0D" e "+3" = salto explícito; "timer +0x86 = 0x3C"
é contador de frames, não subestado. O campo +0x87 é o bot ativo e +0x88 o bot alvo (troca no AJ 0x0B).

## 0x08025790 Fight_DispatchOnSubstateAA_AH_FormatCounterDigits (AH, idx 0x12)
- Assinatura: `void f(void)`
- O que faz: formata o valor de +0x51 em 6 glifos (`Num_FormatDecimal6GlyphsBase`), grava +0xA6=0x35, avança +1.
- Memória: lê +0x51; escreve +0xA6, +0xAA.
- Para o Lua: porta junto com a formatação de número.
- Confiança: confirmado (decompile); nome provável.

## 0x080257E8 (AI, idx 0x00)
- Sem função no Ghidra; código não lido. Criar função no Ghidra antes de documentar.
- Confiança: dado (só a entrada da tabela).

## 0x08025858 Fight_DispatchOnSubstateAA_AI_LoadTechniqueGraphics (AI, idx 0x08)
- Assinatura: `void f(void)`
- O que faz: escolhe um de dois gráficos (LZ77 + cpufastset de 16 words) pelo byte de técnica da peça (`== '.'`),
  ajusta registro IO em base+0x840 (|= 0x220), avança +1 e faz salto por outra tabela (não recuperada).
- Memória: lê +0xA7, +0x87, +0x49 do bot; escreve IO, +0xAA.
- Para o Lua: porta a escolha de gráfico; o salto final precisa do próximo despachante.
- Confiança: confirmado (decompile); nome provável.

## 0x08025974 (AI, idx 0x0B)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025A18 Fight_DispatchOnSubstateAA_AI_SetActionResultCodes (AI, idx 0x11)
- Assinatura: `undefined4 f(void)`
- O que faz: zera +0xCA do bot, grava em +0xCB/+0xCC o resultado de `FUN_08036E54` sobre os stats +0xE8/+0xEC,
  grava código 0x2E ou 0x2F em +0xC9 conforme a técnica, avança +1.
- Memória: escreve +0xC9, +0xCA, +0xCB, +0xCC do bot; lê stats +0xE8/+0xEC; lê técnica.
- Para o Lua: porta; falta nome de `FUN_08036E54`.
- Confiança: confirmado (decompile); nome provável.

## 0x08025AAC (AI, idx 0x12)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025B20 (AJ, idx 0x00)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025B9C (AJ, idx 0x08)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025C84 Fight_DispatchOnSubstateAA_AJ_SwapActorAndTarget (AJ, idx 0x0B)
- Assinatura: `undefined4 f(void)`
- O que faz: `FightPreview_SplitValueAgainstLimit` com +0xC1 e stat +0xEC; +0x51 recebe o byte de +0xC8; divide
  contra 0x50 (dobrado se bit 8 de +0xC0); +0x52 = +0xC8; troca bot ativo (+0x87) com alvo (+0x88); chama
  `Fight_ResetBotVisualCounters`; avança +1.
- Memória: lê +0xC0, +0xC1, +0xC8, +0xEC, +0x87, +0x88; escreve +0x51, +0x52, +0x87, +0x88, +0xAA.
- Para o Lua: porta (troca de ator e alvo).
- Confiança: confirmado (decompile); nome provável.

## 0x08025D2C (AJ, idx 0x0C)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025D4C (AJ, idx 0x0D)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025D6C (AJ, idx 0x0E)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025DD4 Fight_DispatchOnSubstateAA_AJ_RestoreActorAndSetupSprite (AJ, idx 0x0F)
- Assinatura: `undefined4 f(void)`
- O que faz: restaura +0x87 ← +0x64, +0x51 ← +0x52, liga bits 0x220 no IO base+0x840, monta sprite do bot
  (registro +0x1742/+0x1744 do bot) e avança +1.
- Memória: lê +0x64, +0x52; escreve +0x87, +0x51, IO, +0xAA.
- Para o Lua: porta (depende da representação de OAM).
- Confiança: confirmado (decompile); nome provável.

## 0x08025E88 (AJ, idx 0x13)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025F14 (AJ, idx 0x14)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025F54 (AK, idx 0x00)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08025FC8 (AL, idx 0x00)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026080 Fight_DispatchOnSubstateAA_AL_WaitSlotAnimations (AL, idx 0x01)
- Assinatura: `undefined4 f(void)`
- O que faz: roda `Anim_RunSlotScript` nos 3 slots (0x21..0x23 no bloco de 0x40); se algum slot tem +0x22 == 0xFF,
  define timer +0x86 = 0x3C e avança +1.
- Memória: lê +0x22 de cada slot; escreve +0x86, +0xAA.
- Para o Lua: porta (espera animação terminar).
- Confiança: confirmado (decompile); nome provável.

## 0x080260EC (AL, idx 0x03)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026130 (AM, idx 0x00)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026184 (AM, idx 0x04)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026284 Fight_DispatchOnSubstateAA_AM_WaitAnimThenPlaceSprite (AM, idx 0x05)
- Assinatura: `undefined4 f(void)`
- O que faz: roda `Anim_RunSlotScript` (slot 0x21); se o byte de controle == -1, grava 0x80 em dois campos,
  ajusta posição (-4 do registro do bot) e avança +1.
- Memória: lê +0x128 (slot), byte de controle; escreve campos do registro de sprite, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x080262FC Fight_DispatchOnSubstateAA_AM_StepDownCounterWithSfx (AM, idx 0x06)
- Assinatura: `undefined4 f(void)`
- O que faz: se o byte de contador == '@' (0x40), toca `snd_sfx_request(0x53)` e avança +1; senão subtrai 8 do
  contador e do campo ligado.
- Memória: lê/escreve contador; escreve +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026340 (AM, idx 0x07)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026378 (AM, idx 0x08)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x080263BC (AM, idx 0x0A)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026444 (AM, idx 0x0B)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026484 (AN, idx 0x00)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x080264E4 Fight_DispatchOnSubstateAA_AN_SetupEffectSpritesAndSfx (AN, idx 0x04)
- Assinatura: `undefined4 f(void)`
- O que faz: LZ77 para gráfico + cpufastset 16 words; IO base+0x840 = const; monta registro de sprite do bot
  (+0x1742/+0x1744) e dois slots (`Slot_SetupFromRecordList` 0x21 e 0x22); toca `snd_sfx_request(0x5E)`; zera
  timer +0x86; avança +1.
- Memória: lê +0x87 do bot e registros; escreve IO, registros de sprite, +0x86, +0xAA.
- Para o Lua: porta (depende de LZ77 e OAM).
- Confiança: confirmado (decompile); nome provável.

## 0x08026628 Fight_DispatchOnSubstateAA_AN_WaitFramesWithAnimations (AN, idx 0x05)
- Assinatura: `undefined4 f(void)`
- O que faz: a cada frame roda `Anim_RunSlotScript` nos slots 0x21 e 0x22 e incrementa timer +0x86 até 0x5A (90);
  ao chegar, avança +1.
- Memória: lê/escreve +0x86; escreve +0xAA.
- Para o Lua: porta (espera de 90 frames).
- Confiança: confirmado (decompile); nome provável.

## 0x08026678 Fight_ApplyRandomPartReplacementEffect (AN, idx 0x06)
- Nome já existia no mtbe.csv (sem renomear). Ver mtbe.csv para a descrição completa.
- O que faz (leitura desta rodada): roda animações 0x21/0x22, marca +0x118=1, troca a peça por uma sorteada
  (`FUN_08037578`, `GetPartStats`, cópia de 0x18 bytes), e chama `Fight_BuildPartActionMetadataForAllParts`,
  `Fight_SelectActionSource`, `Fight_RefreshDisplayedPartSpritesForActiveBots` e `Fight_PlaceActiveBotObjects`; avança +1.
- Confiança: confirmado (decompile).

## 0x08026810 (AN, idx 0x07)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x080268AC (AO, idx 0x01)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x080268D4 Fight_DispatchOnSubstateAA_AO_ClampPartCountsToLimit (AO, idx 0x02)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `FightPreview_ClampPartCountToLimit` para as 4 posições (0..3) do bot com limite do stat +0xEC;
  avança +1.
- Memória: lê +0x88, +0xEC; escreve (via clamp) os contadores de peça; +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026924 Fight_DispatchOnSubstateAA_AO_ShowFourPartValues (AO, idx 0x03)
- Assinatura: `undefined4 f(void)`
- O que faz: LZ77 + cpufastset 8; escolhe linha de stat por bit 2 de +0xF8; escreve 4 números de peça
  (`Fight_WriteNumberTiles`, campos +0xD6/+0xD8/+0xDA/+0xDC) e monta as linhas de sprite; toca `snd_sfx_request(0x5B)`; avança +1.
- Memória: lê +0xD6..+0xDC, +0xF8; escreve tiles/sprites; +0xAA.
- Para o Lua: porta (depende de LZ77 e tiles).
- Confiança: confirmado (decompile); nome provável.

## 0x08026A24 Fight_DispatchOnSubstateAA_AO_WaitPartRowsDrawn (AO, idx 0x04)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `FUN_08036B50` para as 4 linhas de peça (posições 10, 14, 18, 22); se a última retorna != 0,
  grava timer 0x3C e avança +1.
- Memória: escreve timer; +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026A74 Fight_DispatchOnSubstateAA_AO_DrawPartIconsWaitReady (AO, idx 0x07)
- Assinatura: `undefined4 f(void)`
- O que faz: `FUN_08036BB8` nas 4 peças; desenha ícones (`FightPreview_LoadPartIconRows`, `FightPreview_DrawPartIconPairs`);
  se as 4 retornam 1, grava timer 0x3C e avança +1.
- Memória: lê estado de cada peça; escreve timer, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026AF4 Fight_DispatchOnSubstateAA_AO_ShowPartFlagsOrBackup (AO, idx 0x09)
- Assinatura: `undefined4 f(void)`
- O que faz: LZ77 + cpufastset 8; para cada peça com flag +0xD2..+0xD5 != 0 chama `FUN_08036C20` (efeito por peça).
  Se todas as flags são zero, avança +1; senão `Fight_BackupHalvedStatsIfFlagClear` e grava subestado 0x0D.
- Memória: lê +0xD2..+0xD5, +0xF8; escreve +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026BF8 (AO, idx 0x0D)
- Sem função no Ghidra; código não lido. É o destino de 0x09 quando há backup.
- Confiança: dado.

## 0x08026CBC Fight_QueueHeadBreakIfDefenderHeadZero (AO, idx 0x10)
- Nome já existia no mtbe.csv (sem renomear). Leitura desta rodada confere.
- O que faz: se o byte +0x59 do bot alvo é 0, chama `Fight_QueueHeadBrokenBotForResolution`; avança +1.
- Confiança: confirmado (decompile).

## 0x08026D1C (AP, idx 0x01)
- Sem função no Ghidra; código não lido.
- Confiança: dado.

## 0x08026D44 Fight_DispatchOnSubstateAA_AP_ApplyDamageToRandomTargetPart (AP, idx 0x15)
- Assinatura: `void f(void)`
- O que faz: se +0xCA == 0 avança +1; senão sorteia peça alvo (`Fight_SelectRandomTargetPartForSelectedBot`),
  copia alvo para fight_information (`Fight_CopySelectedTargetToFightInfo`), +0xD0 = +0x8B, aplica dano
  (`Fight_ApplyDamageToPart`) e avança +3. Depois, salto por outra tabela (não recuperada).
- Memória: lê +0x87, +0x88, +0x8B, +0xCA; escreve +0xD0, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## Máquina de estados (esboço, por despachante; "→ n" = próximo)

- AH: 0x12 FormatCounterDigits → 0x13.
- AI: 0x00 ? → 0x01; 0x08 LoadTechniqueGraphics → 0x09 + salto; 0x0B ? ; 0x11 SetActionResultCodes → 0x12; 0x12 ?.
- AJ: 0x00 ?; 0x08 ?; 0x0B SwapActorAndTarget → 0x0C; 0x0C..0x0E ?; 0x0F RestoreActorAndSetupSprite → 0x10; 0x13, 0x14 ?.
- AK: 0x00 ?.
- AL: 0x00 ?; 0x01 WaitSlotAnimations → 0x02 (ou timer); 0x03 ?.
- AM: 0x00, 0x04 ?; 0x05 WaitAnimThenPlaceSprite → 0x06; 0x06 StepDownCounterWithSfx → 0x07 quando '@'; 0x07, 0x08, 0x0A, 0x0B ?.
- AN: 0x00 ?; 0x04 SetupEffectSpritesAndSfx → 0x05; 0x05 WaitFramesWithAnimations (90 f) → 0x06; 0x06 ApplyRandomPartReplacementEffect → 0x07; 0x07 ?.
- AO: 0x01 ?; 0x02 ClampPartCountsToLimit → 0x03; 0x03 ShowFourPartValues → 0x04; 0x04 WaitPartRowsDrawn → 0x05; 0x07 DrawPartIconsWaitReady → 0x08; 0x09 ShowPartFlagsOrBackup → 0x0A ou 0x0D; 0x0D ?; 0x10 QueueHeadBreakIfDefenderHeadZero → 0x11.
- AP: 0x01 ?; 0x15 ApplyDamageToRandomTargetPart → 0x16 ou +3.

Pontos em aberto: 24 handlers sem função no Ghidra (criar função e decompilar); os dois saltos por tabela não
recuperados (AI 0x08, AP 0x15); os nomes `FUN_08036E54`, `FUN_08036B50`, `FUN_08036BB8`, `FUN_08036C20`, `FUN_08037578`
ainda sem nome.
