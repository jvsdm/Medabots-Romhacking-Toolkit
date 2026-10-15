# SD: tabelas dos despachantes de sub-estado de batalha (GH-SD, 2026-10-13)

Dois despachantes de sub-estado usados pela luta. Ambos fazem `CallViaR0(tabela[byte de estado])` e não mexem em
`+0xA6` (esse é o despachante do FH/HA). Bloco de estado = `fi` 0x03001800 (literal confirmado em 0x0801C0E4,
0x0801C31C, 0x0801C134, 0x0801C1BC, 0x0801C234).

| Despachante | Endereço | Tabela | Índice (byte) | Entradas | Escreve arg | Retorno |
|---|---|---|---|---|---|---|
| A | Fight_RunSubstateDispatch 0x0801C0BC | 0x083AD580 | fi+0xA1 | 9 | fi+0xA2 = code | 1 se `fi+0xA1 == 3` (e zera) |
| C | Fight_RunSubstateDispatchC 0x0801C2F0 | 0x083AD5A4 | fi+0x9C | 10 (ver abaixo) | fi+0x9D = arg | 1 se `fi+0x9E != 0` |

Literais: A usa 0x0801C0E8 = 0x083AD580 (e 0x0801C0E4 = fi). C usa 0x0801C320 = 0x083AD5A4 (e 0x0801C31C = fi).

## Tamanho da tabela C (0x083AD5A4): 10 entradas (índices 0..9), 0x28 bytes

Nenhum limite no código: o despachante só lê `byte[fi+0x9C] * 4 + base` (sem checagem). O tamanho sai do layout:
- A tabela A (9 entradas, 0x24 bytes) termina exatamente em 0x083AD5A4, onde começa a C: as duas são contíguas.
- A rotina `Fight_DispatchOnSubstateAA` (0x0801C4FC) usa `0x083AD5CC` como base da própria tabela (índice `fi+0xAA`,
  literal em 0x0801C520, decodificado). Logo a C termina em 0x083AD5CC: (0x083AD5CC - 0x083AD5A4) / 4 = 10.
- A ROM contém uma sequência longa de ponteiros Thumb depois de 0x083AD5CC, mas são várias tabelas vizinhas (há
  literais apontando para dentro dela em 0x083AD68C, 0x083AD6B0, 0x083AD6D0 etc.). Não é a tabela C.
- Confiança: provável (layout contíguo + base da AA). Se um handler escrever índice >= 10 em `fi+0x9C`, a leitura
  cai na tabela AA: vale conferir com os 10 chamadores (AJ_0C..AJ_13, BR_24..BR_46) ao fechar.

## Tabela A (0x083AD580): entradas (índice = fi+0xA1)

| Índice | Ponteiro | Função (endereço) | Nome |
|---|---|---|---|
| 0 | 0x0801C0F9 | 0x0801C0F8 (função desde o sync de 2026-10-14 (nome: Fight_SubstateA_0_StartUiActionAndAdvance)) | Fight_SubstateA_0_StartUiActionAndAdvance |
| 1 | 0x0801C111 | 0x0801C110 Fight_UiActionStep | (mantido) |
| 2 | 0x0801C139 | 0x0801C138 (função desde o sync de 2026-10-14 (nome: Fight_SubstateA_2_TryUiActionDefaultAndAdvance)) | Fight_SubstateA_2_TryUiActionDefaultAndAdvance |
| 3 | 0x08000841 | 0x08000840 NoOpActionHandler | (mantido, stub compartilhado) |
| 4 | 0x0801C189 | 0x0801C188 Gfx_LoadAssetAndAdvance | (mantido) |
| 5 | 0x0801C1C1 | 0x0801C1C0 Fight_SpawnHitEffect | (mantido) |
| 6 | 0x0801C23D | 0x0801C23C Fight_TimerStepAdvance | (mantido) |
| 7 | 0x0801C299 | 0x0801C298 FUN_0801C298 | Fight_SubstateA_7_TimerTick60ThenClearTile3 |
| 8 | 0x08000841 | 0x08000840 NoOpActionHandler | (mantido, stub compartilhado) |

## Tabela C (0x083AD5A4): entradas (índice = fi+0x9C)

| Índice | Ponteiro | Função (endereço) | Nome |
|---|---|---|---|
| 0 | 0x0801C32D | 0x0801C32C (função desde o sync de 2026-10-14 (nome: Fight_SubstateC_0_CpuCopyThenBotStep)) | Fight_SubstateC_0_CpuCopyThenBotStep |
| 1 | 0x0801C361 | 0x0801C360 Fight_StepSpriteGroupMoveAnim | (mantido) |
| 2 | 0x0801C389 | 0x0801C388 Fight_TickEntryAnimAndLoadTiles | (mantido) |
| 3 | 0x0801C3B1 | 0x0801C3B0 Fight_BotEntryAnimStep | (mantido) |
| 4 | 0x0801C3F9 | 0x0801C3F8 (função desde o sync de 2026-10-14 (nome: Fight_SubstateC_4_AnimateEntryFramesOnly)) | Fight_SubstateC_4_AnimateEntryFramesOnly |
| 5 | 0x0801C40D | 0x0801C40C Fight_RunEntryAnimStep | (mantido) |
| 6 | 0x0801C445 | 0x0801C444 Fight_SideEffectStepAdvance | (mantido) |
| 7 | 0x0801C471 | 0x0801C470 Fight_BotSideAnimStep | (mantido) |
| 8 | 0x0801C4A5 | 0x0801C4A4 Fight_RunEntryVisualStep | (mantido) |
| 9 | 0x0801C4E1 | 0x0801C4E0 Fight_AnimateBotEffects | (mantido) |

Nota de Ghidra: 0x0801C0F8, 0x0801C138, 0x0801C32C e 0x0801C3F8 não têm função definida (get_function_by_address e
disassemble_function falham). O corpo foi lido a partir dos bytes da ROM com um decoder Thumb mínimo (scratchpad),
então o nome está no SD_simbolos.csv mas não foi aplicado no Ghidra (sem função para renomear).

---

## Despachante A (fi+0xA1)

## 0x0801C110 Fight_UiActionStep (tabela A, índice 1)
- Assinatura: `int f(void)` (Ghidra: `undefined4 Fight_UiActionStep(void)`)
- O que faz: chama `Fight_TryUiActionByIndex(fi+0xA2)`; se devolver != 0, incrementa `fi+0xA1` (avança o sub-estado).
  É o passo de ação de UI pelo código recebido. Ghidra confirma o corpo (decompile lido).
- Chama: `Fight_TryUiActionByIndex` (ver FL2/HA). Chamada por: `Fight_RunSubstateDispatch` (índice 1).
- Memória: lê `fi+0xA2` (código) e `fi+0xA1` (lê e incrementa), via literal 0x0801C134 = 0x03001800.
- Para o Lua: portar junto com `Fight_TryUiActionByIndex`. A função do mtbe (`provavel`) está certa.
- Retorno: nenhum útil (o despachante lê `fi+0xA1`).
- Renomeado no Ghidra: não (nome específico, já existe).
- Confiança: confirmado (decompile).

## 0x0801C188 Gfx_LoadAssetAndAdvance (tabela A, índice 4)
- Assinatura: `void f(void)`
- O que faz: descomprime um asset (`decompress_and_store(DAT_0801C1AC = 0x084CA064, DAT_0801C1B0)`),
  copia 8 words com CpuFastSet (`bios_cpufastset(src, dst, 8)`), e incrementa `fi+0x9C` (o índice da tabela C).
  Ou seja, este passo de carga de gráfico não avança `fi+0xA1`: quem avança é o próprio C.
- Chama: `decompress_and_store`, `bios_cpufastset` (0x0807CCC0, wrapper SWI 0x0C). Chamada por: tabela A (índice 4).
- Memória: lê ROM comprimida; escreve VRAM/buffer; `fi+0x9C` (incremento).
- Para o Lua: o Lua não precisa do descompressor (usar os gráficos já extraídos); o efeito útil é `fi+0x9C += 1`.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile + literal fi = 0x03001800 em 0x0801C1BC).

## 0x0801C1C0 Fight_SpawnHitEffect (tabela A, índice 5)
- Assinatura: `void f(void)`
- O que faz: monta um sprite de efeito de golpe no contexto de animação em 0x03002C30 (gObjectSlots): zera `+0xDC`,
  grava X (`+0xC0`) e Y (`+0xC4`) a partir da tabela de posição do bot (bloco do bot `+0x1742/+0x1744`, índice
  `fi+0x87`), define `+0xCA = 6`, `+0xC6`, `+0xCB = 0x81`; toca o SFX 0x2F (`snd_sfx_request`), e incrementa `fi+0x9C`.
- Chama: `snd_sfx_request`. Chamada por: tabela A (índice 5).
- Memória: lê `fi+0x87`, bloco do bot (0x03000590 + bot*0x100 + 0x1742/0x1744); escreve `gObjectSlots` (0x03002C30,
  literal 0x0801C22C) e `fi+0x9C`.
- Para o Lua: portar (escrita de 6 campos e SFX). O som pode virar uma chamada de evento.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile + literais). Significado dos campos do sprite: provável (ver FL4).

## 0x0801C23C Fight_TimerStepAdvance (tabela A, índice 6)
- Assinatura: `void f(void)` (Ghidra: `undefined4 Fight_TimerStepAdvance(void)`)
- O que faz: se o flag em `*0x0801C28C` (bit 0) está ligado, incrementa o contador de 16 bits `+0xC4` do bloco
  `0x0801C290` (gObjectSlots-like, ver FL5); quando ele chega a `bloco[bot*0x100 + 0x1744] + 4`, zera `fi+0x9D` e
  incrementa `fi+0x9C`. Ou seja: espera um tempo proporcional ao bot e então avança o C.
- Chama: nada. Chamada por: tabela A (índice 6).
- Memória: flag em `*0x0801C28C`; contador em `0x0801C290 + 0xC4`; `fi+0x87` (bot); escreve `fi+0x9D` e `fi+0x9C`.
- Para o Lua: portar (contador + comparação). O bloco 0x0801C290 é o mesmo do FL5 (provável).
- Renomeado no Ghidra: não (nome já específico).
- Confiança: confirmado (decompile). Significado do bloco 0x0801C290: provável.

## 0x0801C298 FUN_0801C298 -> Fight_SubstateA_7_TimerTick60ThenClearTile3 (tabela A, índice 7)
- Assinatura: `void f(void)`
- O que faz: incrementa `fi+0x9D`. Se `byte[0x03000440+0x89] == 0` e o bit 0 de `byte[0x03001798]` está ligado,
  força `fi+0x9D = 0x3C` (60 quadros). Quando `fi+0x9D == 60`, chama `Tile_ClearSlots(3, 1)` (0x08040CD0) e
  incrementa `fi+0x9C`. Temporizador de 1 s, depois limpa o slot 3 e avança a tabela C.
- Chama: `Tile_ClearSlots` (0x08040CD0; o mtbe chama de ClearObjectAttributeEntries, o decompile e o FH dizem Tile_ClearSlots).
  Chamada por: tabela A (índice 7).
- Memória: lê `0x03000440+0x89`, `0x03001798`; escreve `fi+0x9D`, `fi+0x9C`.
- Para o Lua: portar a contagem de 60 quadros; o gatilho (bit em 0x03001798) precisa de confirmação com o HA.
- Renomeado no Ghidra: sim (FUN_0801C298 -> Fight_SubstateA_7_TimerTick60ThenClearTile3).
- Confiança: confirmado (decompile + literais). Significado do bit 0x03001798: provável.

## 0x0801C360 Fight_StepSpriteGroupMoveAnim (tabela C, índice 1)
- Assinatura: `void f(void)`
- O que faz: chama `Fight_UpdateSpriteGroupMoveAnim(fi+0x87)` (bot atual); se devolver != 0, incrementa `fi+0x9C`.
  É um passo de animação de grupo de sprites que avança quando termina.
- Chama: `Fight_UpdateSpriteGroupMoveAnim` (0x08016438, ver FL). Chamada por: tabela C (índice 1).
- Memória: lê `fi+0x87`; escreve `fi+0x9C`. Literal 0x0801C384 = 0x03001800.
- Para o Lua: portar junto com `Fight_UpdateSpriteGroupMoveAnim`.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile + literal).

## 0x0801C388 Fight_TickEntryAnimAndLoadTiles (tabela C, índice 2)
- Assinatura: `void f(void)`
- O que faz: `Fight_AnimateEntryFrames(fi+0x87)` e `Fight_LoadBotSideTiles(fi+0x87)`, depois incrementa `fi+0x9C`
  (sem condição). Um quadro da entrada do bot, com carga de tiles laterais.
- Chama: `Fight_AnimateEntryFrames` (0x080164F4), `Fight_LoadBotSideTiles` (0x0801AC98). Chamada por: tabela C (índice 2).
- Memória: lê `fi+0x87`; escreve `fi+0x9C`. Literal 0x0801C3AC = 0x03001800.
- Para o Lua: portar as duas chamadas e o incremento.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile).

## 0x0801C3B0 Fight_BotEntryAnimStep (tabela C, índice 3)
- Assinatura: `void f(void)`
- O que faz: anima os frames de entrada (`Fight_AnimateEntryFrames`), refaz o texto do nome (`Fight_LayoutBotNameText`)
  e atualiza o visual do bot (`Fight_UpdateBotVisual`), todos com `fi+0x87`. Depois: se `fi+0x9D == 0` marca `fi+0x9E = 1`
  (saída: o chamador de C avança) e `fi+0x9C += 1`; se `fi+0x9D != 0`, **define `fi+0x9C = 5`** (salto para o índice 5).
- Chama: `Fight_AnimateEntryFrames`, `Fight_LayoutBotNameText` (0x08015B3C), `Fight_UpdateBotVisual` (0x0801AD10).
  Chamada por: tabela C (índice 3).
- Memória: lê `fi+0x87`, `fi+0x9D`; escreve `fi+0x9E`, `fi+0x9C`. Literal 0x0801C3E4 = 0x03001800.
- Para o Lua: portar; atenção ao salto para 5 quando `fi+0x9D != 0` (lido no decompile; a leitura do asm sozinha confundia).
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile + literal).

## 0x0801C40C Fight_RunEntryAnimStep (tabela C, índice 5)
- Assinatura: `void f(void)`
- O que faz: copia 8 words com CpuFastSet de `0x084E44B8` para `0x05000320` (paleta/OBJ), depois
  `Fight_AnimateEntryFrames(fi+0x87)` e `Fight_BuildSideEffectSprites8(fi+0x87)`, e incrementa `fi+0x9C`.
- Chama: `bios_cpufastset` (0x0807CCC0), `Fight_AnimateEntryFrames`, `Fight_BuildSideEffectSprites8` (0x0801AD48).
  Chamada por: tabela C (índice 5).
- Memória: lê ROM em 0x084E44B8; escreve paleta (0x05000320) e `fi+0x9C`. Literal 0x0801C440 = 0x03001800.
- Para o Lua: o copiamento da paleta vira carga de paleta já extraída; o resto é a mesma lógica dos frames.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile + literais).

## 0x0801C444 Fight_SideEffectStepAdvance (tabela C, índice 6)
- Assinatura: `void f(void)`
- O que faz: `Fight_AnimateEntryFrames(fi+0x87)`, depois `Fight_StepSideEffectSprites8(fi+0x87)`; se este devolver != 0,
  incrementa `fi+0x9C`. Avança o passo quando os sprites de efeito lateral terminam.
- Chama: `Fight_AnimateEntryFrames`, `Fight_StepSideEffectSprites8` (0x0801ADE8). Chamada por: tabela C (índice 6).
- Memória: lê `fi+0x87`; escreve `fi+0x9C`. Literal 0x0801C46C = 0x03001800.
- Para o Lua: portar junto com `Fight_StepSideEffectSprites8`.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile).

## 0x0801C470 Fight_BotSideAnimStep (tabela C, índice 7)
- Assinatura: `void f(void)`
- O que faz: `Fight_AnimateEntryFrames(fi+0x87)`, `Fight_AnimateSideEffectSprites8(fi+0x87)`, `Fight_LoadBotSideTiles(fi+0x88)`,
  e incrementa `fi+0x9C` sem condição. Um quadro lateral do bot (atacante em `+0x87`, alvo em `+0x88`).
- Chama: `Fight_AnimateEntryFrames`, `Fight_AnimateSideEffectSprites8` (0x0801AE70), `Fight_LoadBotSideTiles` (0x0801AC98).
  Chamada por: tabela C (índice 7).
- Memória: lê `fi+0x87`, `fi+0x88`; escreve `fi+0x9C`. Literal 0x0801C4A0 = 0x03001800.
- Para o Lua: portar as três chamadas e o incremento.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile).

## 0x0801C4A4 Fight_RunEntryVisualStep (tabela C, índice 8)
- Assinatura: `void f(void)`
- O que faz: `Fight_AnimateEntryFrames(fi+0x87)`, `Fight_AnimateSideEffectSprites8(fi+0x87)`, `Fight_LayoutBotNameText(fi+0x88)`,
  `Fight_UpdateBotVisual(fi+0x88)`; depois marca `fi+0x9E = 1` (saída: o chamador de C avança) e incrementa `fi+0x9C`.
  É o último passo visual da entrada do bot.
- Chama: `Fight_AnimateEntryFrames`, `Fight_AnimateSideEffectSprites8`, `Fight_LayoutBotNameText`, `Fight_UpdateBotVisual`.
  Chamada por: tabela C (índice 8).
- Memória: lê `fi+0x87`, `fi+0x88`; escreve `fi+0x9E`, `fi+0x9C`. Literal 0x0801C4DC = 0x03001800.
- Para o Lua: portar a sequência de quatro chamadas; o `fi+0x9E = 1` é o sinal de término.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile).

## 0x0801C4E0 Fight_AnimateBotEffects (tabela C, índice 9)
- Assinatura: `void f(void)`
- O que faz: `Fight_AnimateEntryFrames(fi+0x87)` e `Fight_AnimateSideEffectSprites8(fi+0x87)`. Não mexe em `fi+0x9C`
  nem em `fi+0x9E`: é um passo de repetição (fica no índice 9 até outro handler mudar o índice). É a última entrada da tabela.
- Chama: `Fight_AnimateEntryFrames`, `Fight_AnimateSideEffectSprites8`. Chamada por: tabela C (índice 9).
- Memória: lê `fi+0x87`. Literal 0x0801C4F8 = 0x03001800.
- Para o Lua: portar; o loop termina quando alguém escreve `fi+0x9C`.
- Renomeado no Ghidra: não (nome específico).
- Confiança: confirmado (decompile).

## 0x08000840 NoOpActionHandler (tabela A, índices 3 e 8; também várias entradas da C)
- Assinatura: `void f(void)` (decompile: `void NoOpActionHandler(void) { return; }`)
- O que faz: nada. Na tabela A, o índice 3 é o sinal de "pronto": o despachante vê `fi+0xA1 == 3` e devolve 1 (e zera).
  O índice 8 da A também aponta para ele (o despachante não trata 8 de forma especial).
- Chama: nada. Chamada por: tabela A (índices 3 e 8). Também aparece em entradas de tabelas vizinhas depois de 0x083AD5CC
  (outros despachantes), fora do escopo de C.
- Memória: nenhuma.
- Para o Lua: `function() end`.
- Renomeado no Ghidra: NÃO. É um stub compartilhado; renomear para um índice enganaria as outras tabelas.
- Confiança: confirmado (decompile).

## 0x0801C4FC (fora da tabela C, só para o limite)
- `Fight_DispatchOnSubstateAA` (0x0801C4FC) usa `0x083AD5CC` como base de tabela (índice `fi+0xAA`). É ela que fecha
  a tabela C em 10 entradas. Não documentada aqui (é outro despachante).

---

## Pendentes deste GH-SD (sem função definida no Ghidra, decodificados dos bytes)

## 0x0801C0F8 Fight_SubstateA_0_StartUiActionAndAdvance (tabela A, índice 0; sem função no Ghidra)
- Assinatura: `void f(int code)` (r0 = code do despachante, passado sem mudança).
- O que faz: `bl Fight_MaybeStartUiAction` (0x0801ABF8) com r0 = code; depois `fi+0xA1 += 1`, sem condição.
- Chama: `Fight_MaybeStartUiAction` (0x0801ABF8). Chamada por: tabela A (índice 0).
- Memória: escreve `fi+0xA1`. Literal 0x0801C10C = 0x03001800.
- Para o Lua: portar junto com `Fight_MaybeStartUiAction`.
- Renomeado no Ghidra: não (sem função definida; o nome vale só para o SD).
- Confiança: provável (decodificado dos bytes; não passou pelo decompile).

## 0x0801C138 Fight_SubstateA_2_TryUiActionDefaultAndAdvance (tabela A, índice 2; sem função no Ghidra)
- Assinatura: `void f(int code)`.
- O que faz: `bl Fight_TryUiActionDefault` (0x0801AC6C) com r0 = code; se devolver != 0, `fi+0xA1 += 1`.
- Chama: `Fight_TryUiActionDefault` (0x0801AC6C). Chamada por: tabela A (índice 2).
- Memória: escreve `fi+0xA1`. Literal 0x0801C150 = 0x03001800.
- Para o Lua: portar junto com `Fight_TryUiActionDefault`. Diferente de `Fight_UiActionStep` (índice 1): usa a versão "default".
- Renomeado no Ghidra: não (sem função definida).
- Confiança: provável (decodificado dos bytes).

## 0x0801C32C Fight_SubstateC_0_CpuCopyThenBotStep (tabela C, índice 0; sem função no Ghidra)
- Assinatura: `void f(void)`.
- O que faz: CpuFastSet de 8 words de `0x084E4498` para `0x05000300` (paleta/OBJ); depois chama `0x08016370(fi+0x87)`
  (sem nome; ver FL) e incrementa `fi+0x9C`.
- Chama: `bios_cpufastset` (0x0807CCC0), 0x08016370 (sem nome). Chamada por: tabela C (índice 0).
- Memória: lê ROM 0x084E4498 e `fi+0x87`; escreve paleta 0x05000300 e `fi+0x9C`. Literal 0x0801C358 = 0x05000300.
- Para o Lua: carga de paleta já extraída + `0x08016370` (ver o nome dele).
- Renomeado no Ghidra: não (sem função definida).
- Confiança: provável (decodificado dos bytes; 0x08016370 ainda sem nome).

## 0x0801C3F8 Fight_SubstateC_4_AnimateEntryFramesOnly (tabela C, índice 4; sem função no Ghidra)
- Assinatura: `void f(void)`.
- O que faz: só `Fight_AnimateEntryFrames(fi+0x87)`. Não incrementa `fi+0x9C`: fica nesse índice até outro handler mudar.
- Chama: `Fight_AnimateEntryFrames` (0x080164F4). Chamada por: tabela C (índice 4).
- Memória: lê `fi+0x87`. Literal 0x0801C408 = 0x03001800.
- Para o Lua: portar (uma chamada).
- Renomeado no Ghidra: não (sem função definida).
- Confiança: provável (decodificado dos bytes).
