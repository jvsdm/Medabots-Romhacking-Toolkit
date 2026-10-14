# AN2: dono das tabelas de ponteiros 0x083AFCB8 / 0x0807E874 e chamador de FUN_08043FEC

Status: parcial, pontos limpos (2026-10-14). Leitura da ROM (python -I, so leitura) e decompile no Ghidra.
Nenhum emulador usado.

## 1. Tabela 0x083AFCB8..0x083AFDA4: parte de uma tabela de passos da tela LINK (menu 10)
- A tabela inteira comeca em **0x083AFC78** (96 ponteiros Thumb, ja marcada no mtbe como gMenu10StepTable_AFC78).
  0x083AFCB8 = indice 16 (0x40/4); 0x083AFDA4 = indice 75. Nao ha literal de 0x083AFCB8 na ROM: a tabela e acessada por base.
- Leitor: `Scene_GsDispatch_Menu10_Link` 0x0803D538 (confirmado no decompile):
  `CallViaR0( *(0x083AFC78 + 4*GS) )`, com GS = gMainSubStateCounter 0x0300137C (confirmado: literal 0x0803D59C = 0x083AFC78).
  Antes do despacho: se GS nao esta em 0x10..0x1F e Battle_CheckExitShortcut dá 0, faz CpuFastSet se 0x03000430==2 e Overlay_ClearTimerIfFlag.
- Entrada da tela: `Fight_SceneStep53_ExitBattleScene` 0x0803133C (mtbe, HN10): "link -> menu 10 sub 0x20". Ou seja, a tela e o modo de
  LINK (cabo) apos a batalha. Confianca: provavel (o nome "Link" vem do HN10 e dos passos Link_*, nao de tela vista).
- Os passos Anim_* (indices 17,19,20,23 e 55..67, 72..75): 18 funcoes distintas (21 entradas). Todas desenham a
  barra de 2 tiles (Gauge_DrawTwoTileSegment) ou tocam sons de barra; nenhuma e a barra de Medaforce nem o medidor da batalha.
  - Anim_DrawSegmentUntil120 (0x0803E0F0, confirmado): conta +0x86 de 0 a 0x78 (120 quadros) desenhando o segmento; depois avanca GS.
  - Anim_TweenUntilDoneThenReset (0x0803E098, confirmado): tween ate o alvo; ao chegar toca o som de barra cheia (Gauge_FullTriggersSound7).
  - Anim_TweenClampThenSfx2A (0x0803E0C0), Anim_RunSlotScriptThenSfx54 (0x0803E03C): tween/script de slot com SFX 0x2A / 0x54.
  - Anim_DrawSetValue3E (0x0803E48C, confirmado): desenha segmento, +0x86++, grava 0x3E em 0x0803E4B0 (valor fixo).
  - Anim_GpuBlendTickThenCount (0x0803E4D8, confirmado): GpuBlend_TickStep; ao terminar, incrementa contador.
  - Demais (InitSlotAndLoadCount, DrawSegmentThenJumpPtr/LoadSlot/DispatchLoad/DispatchStep, UntilOverlayReached,
    CycleStateZeroToFour, FadeSfx8Step, FlushPaletteThenReset, FadeRowsStep, PickSlotActivateEntryCopy): ver FS7.md (provavel).
- Nao-Anim na faixa: idx 16 PeerLost_MenuStep16, 18 Gfx_BlankSceneInit, 22 PeerLost_MenuStep16, 24 Boot_ResetSystemWrapper;
  idx 45/68 Screen_ResetAndSetSub4Mode; idx 54 Link_StepGaugeAndTween (0x0803E018).
- Divergencia com a tarefa: FS7 diz "19 passos Anim_*"; contei 18 funcoes distintas (21 entradas) nesta faixa.

## 2. Tabela 0x0807E874: entradas de trilha de som dentro de uma tabela de IRQ copiada no boot
- Bloco ROM 0x0807E860..0x0807E898 (14 ponteiros): [0]=0x08002439, [1]=0x080005D1, [3]=0x08000825,
  [5]=0x0800099D (Sound_TickTrackAStub), [6]=0x080009A9 (Sound_TickTrackBStub); os demais sao 0x08000841 (vazio).
- Copia no boot: `InitializeInterruptsAndDefaultHandlers` 0x0800045A faz `bios_cpuset(0x0807E860 -> 0x03000BA0, 0x0400000E)`
  (14 words em 32 bits). Confirmado pelo literal 0x080004A8 e pelo decompile.
- Leitor: `IntrMain` 0x080001D8 (confirmado no decompile): le REG_IE/REG_IF e chama `(*(0x03000BA0 + offset))()`,
  com offset por bit: 0xC0->0, 0x1->4, 0x4->8, 0x2->0xC, 0x100->0x10, 0x200->0x14, 0x400->0x18, 0x800->0x1C, 0x1000->0x20, 0x20->0x24, 0x2000->0x28.
- Logo, 0x0807E874 (idx 5) = handler do IRQ 0x200 (DMA1 no hardware GBA) e 0x0807E878 (idx 6) = IRQ 0x400 (DMA2).
  Os stubs de trilha A/B ficam nesses IRQs de DMA de som (FIFO A/B). Confianca: provavel (mapeamento de bits pelo decompile;
  identificacao DMA1/DMA2 e do hardware, nao lida no jogo).
- Nota: o literal 0x0807E860 aparece so em 0x080004A8 (copia), nao ha literal de 0x0807E874.

## 3. FUN_08043FEC (renomeada no Ghidra para Anim_AdvanceRingFrameEvery3)
- Chamador: `Obj_StepAccumMoveBThenSync` 0x08044100 (BL em 0x0804410C, confirmado no xref do Ghidra).
  Obj_StepAccumMoveBThenSync e chamado por ponteiro: o literal 0x08044101 esta em 0x08012B38, dentro de `Fight_PartPanelStep`
  (0x08012A20, provavel: o literal fica no pool antes de Gfx_FadeStepPaletteRows 0x08012B40).
- O que faz (decompile): contador ciclico 0..2 em obj+0x7D; a cada 3 chamadas: avanca anel obj+0x7C (&0x7F), le byte de
  0x083C4238[param+...] para obj+0x7E e byte do anel 0x083C4243 + param*0x80 para obj+0x7F (reinicia se 0xFF), e faz
  CpuSet de (obj+0x7E * obj+0x7F * 0x20 + 0x02027000) para VRAM 0x06008020 (count = obj+0x7E << 3, 32 bits).
  Ou seja: streaming de quadros de tiles (animacao de tile) por anel na ROM. Confianca: provavel (o que e o tile nao foi visto).

## 4. Pendencias
- Quem chama `Fight_PartPanelStep` nao foi tracado; o uso de 0x08044101 e so pelo literal.
- Quais tiles sao 0x083C4243 (grafico do anel) nao foi conferido visualmente.
- Os 18 Anim_* nao foram decompilados um a um (so os 5 citados); tela Link nao foi rodada no emulador.

## 5. Acrescimo AN2b (2026-10-14): quem le a tabela, qual tela, o que cada passo faz
Pontos confirmados por literal na ROM (python -I, so leitura) e decompile. Nao rodei emulador.

### 5.1 Leitor e tela
- Literal 0x0803D59C = 0x083AFC78 (base) e usado so em 0x0803D570 (ldr r0) = `Scene_GsDispatch_Menu10_Link` 0x0803D538.
  Ele faz `CallViaR0(tabela[GS])` com GS = 0x0300137C. Os indices 16..75 (0x083AFCB8..0x083AFDA4) sao os passos desta tela.
- O despachante de menus contem o literal 0x0803D539 em 0x083AC2F0 (tabela de menus; provavel, nao conferi a entrada 10).
- A tabela tem 96 ponteiros (0x083AFC78..0x083AFDF8). O literal 0x083AFDF8 esta em 0x0803D744 (inicio da tabela vizinha; nao tracei).
- Tela: LINK (cabo) pos-batalha, como em 1. Os passos Link_* (34..54) e LinkSync_MenuStep52/53 (indices 52, 53)
  pertencem ao mesmo fluxo. Confianca: provavel.

### 5.2 A barra NAO e o medidor de Medaforce
- `Gauge_DrawTwoTileSegment` 0x0803ED6C e `Gauge_DrawBar12Tiles` 0x0803EA38 escrevem o tilemap de BG (FillBgTilemapRectSequential)
  com 12 tiles de barra; o valor vem de obj+0x1A0 (0..0x60, >>3 = tiles cheios). Medaforce de batalha e BattleBot_MedaforceGauge
  (0x03000651), outra estrutura.
- Chamadores de Gauge_DrawTwoTileSegment: os passos Anim_* desta tabela, LinkSync_MenuStep52/53 e Gauge_DrawByFlag4 (0x0803EB0C).
  Ou seja, todos os desenhos ficam dentro da tela LINK. Confianca: provavel.

### 5.3 Passos (indice: funcao: o que faz). Estado +0x84, contador +0x86 (confirmados nos decompiles abaixo)
- 16 PeerLost_MenuStep16 e 22: nao e Anim. 18 Gfx_BlankSceneInit: nao e Anim. 21 Flow_StepCounterTickA: nao e Anim.
  24 Boot_ResetSystemWrapper: nao e Anim. 25..31, 46, 47, 69..71, 95: NoOpActionHandler (vazio).
- 17, 19, 23: Anim_GpuBlendTickThenCount (0x0803E4D8, confirmado): GpuBlend_TickStep; ao terminar, +1 no contador.
- 20: Anim_InitSlotAndLoadCount (0x0803E618): nome; corpo nao decompilado.
- 55: Anim_RunSlotScriptThenSfx54 (0x0803E03C, confirmado): roda script de slot (indice 0x75 - byte+0x1A2);
  quando obj+0x122 == -1 toca SFX 0x54 e avanca.
- 56: Anim_TweenUntilDoneThenReset (0x0803E098, confirmado): tween ate o alvo; ao chegar, Gauge_FullTriggersSound7
  (som de barra cheia), reseta direcao do tween e avanca.
- 57: Anim_TweenClampThenSfx2A (0x0803E0C0, confirmado): tween com clamp; ao chegar toca SFX 0x2A, zera +0x86, avanca.
- 58: Anim_DrawSegmentUntil120 (0x0803E0F0, confirmado): desenha o segmento, +0x86 ate 0x78 (120 quadros), depois avanca.
- 59, 60/73, 61, 62, 63, 74: Anim_DrawSegment* (desenham o segmento; decidem o proximo passo):
  59 JumpPtr (0x0803E120, confirmado): desenha, +0x86++, se o byte do slot == 4 avanca, senao grava 0x48 e pula por
  ponteiro (jumptable nao recuperada). 60/73 LoadSlot, 61 DispatchLoad, 62 DispatchStep, 63 UntilOverlayReached,
  74 DispatchLoadAlt: nomes do Ghidra, corpo nao decompilado.
- 64: Anim_DrawCycleStateZeroToFour (0x0803E270, confirmado): desenha o segmento, +0x86++; estado +0x84 vai de 0 a 4
  (volta a 0 e avanca ao passar de 4).
- 65 Anim_DrawFadeSfx8Step, 66 Anim_DrawFlushPaletteThenReset, 67 Anim_DrawFadeRowsStep: fade/paleta + segmento
  (nomes; corpo nao decompilado).
- 72 Anim_PickSlotActivateEntryCopy: escolhe slot, ativa e copia entrada (nome; nao decompilado).
- 75 Anim_DrawSetValue3E (0x0803E48C, confirmado): desenha o segmento, +0x86++, grava 0x3E em 0x0803E4B0 (valor fixo).

Contagem: 21 entradas Anim_ (indices 17,19,20,23,55..67,72..75) em 18 funcoes distintas. O enunciado falava em 19; nao
consegui fechar em 19 com a tabela atual. Nao renomeei nada: os nomes ja existentes batem com os corpos lidos.

### 5.4 Tabela 0x0807E874 (AN2 secao 2): quem usa
- Nao ha literal de 0x0807E874 nem de 0x0807E878 em nenhuma instrucao da ROM. O unico leitor e a copia de boot:
  literal 0x0807E860 em 0x080004A8 (ldr r0 em 0x0800045A, `InitializeInterruptsAndDefaultHandlers`).
- Conteudo confirmado pela ROM: literal 0x0800099D esta em 0x0807E874 e 0x080009A9 em 0x0807E878 (os trampolins de trilha).
- Quem chama os trampolins: `IntrMain` 0x080001D8 pela IWRAM 0x03000BA0 (secao 2 do AN2). Nao ha uso direto na tela.

### 5.5 Pendencias
- Corpos nao decompilados: 20, 60, 61, 62, 63, 65, 66, 67, 72, 74 (so pelo nome).
- Entrada 10 do despachante em 0x083AC2F0 nao conferida.
- Falta gravar na tela LINK (emulador) para ver o desenho; nao feito.
