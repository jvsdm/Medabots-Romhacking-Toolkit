# MS2: pendências do motor de mensagem + handlers 9..22 (GH-MS2, 2026-10-14)

Continua `MS.md`. Ghidra MCP usado só para leitura (decompile/disasm/xref); nada salvo no programa. Literais e
tabelas lidos na ROM original com `python -I`. Textos do jogo NÃO copiados.

## 1. Pendências do MS fechadas

### Miolo do estado 2 (MsgState_BG0OnThenDispatch 0x0803FBDC) — confirmado (decompile + literal)
```
if (ctx+2 == 1) ctx+4 = 4              // pula o estado 3
else { ctx+4++; ctx+0x26 = 0; REG_DISPCNT |= 0x100; REG_BG0CNT (0x04000008) = 0x0500; }
```
- 0x0500 em BG0CNT = screen-base 5 (SBB5, VRAM 0x06002800), char-base 0. Bate com H1 (BG0 SBB5).
- A cópia de 0x500 que o MS.md dizia "aberta" é só esse store de BG0CNT; não há cópia de bloco.

### Wrapper do estado 4 (MsgState_ClearBoxAndRunScript 0x0803FCC8) — confirmado (decompile + literal)
- Laço de 0x34 iterações: `bios_cpufastset(src=0x084BFC64, dst=0x06000280 + 0x40*k, 0x10 words)`. O alvo do
  wrapper é `bios_cpufastset` 0x0807CCC0 (SWI 0x0C, confirmado no Ghidra).
- Depois `BgMap_CopyRectWithOffset(0x1A, 4, 2, 0x0E, ...)` (cópia de retângulo do mapa de tiles da caixa), ctx+4++,
  e `ScriptVM_RunOpcodes()` no mesmo quadro.
- Não confirmado: se o src 0x084BFC64 é um bloco de tile vazio ou outra coisa (é só ponteiro lido na ROM).

### Estado 6 (MsgState_DispatchByScriptByte 0x080408D0) — confirmado
- Índice = `byte[ctx+0x0C + ctx+0x08 + 1] * 4 + 0x083B2940`, chamado via `CallViaR0` 0x0807CCF0.
- 0x083B2940 = `gMessageStateHandlerTable + 9*4` (0x083B291C + 0x24). Ou seja: **o byte de comando de 1 byte do
  roteiro escolhe o handler `gMessageStateHandlerTable[9 + byte]`**. As entradas 0..13 dessa tabela são exatamente
  os handlers 9..22 (ver seção 3). Faixa de byte válida: não fechada (a tabela continua com entradas que não são
  dos handlers 9..22: 0x0804169D, 0x0804177D).

### FF: máscara do flag (0x08040898 = MsgFF_SfxAdvanceCtx) — confirmado (disasm)
- `ldr r0,[0x080408B4]` = **0x03001798** (keys_repeat, confirmado no MN.md). `if ((byte[0x03001798] & 3) == 0) return;`
  Ou seja: máscara **0x03** = botão A (bit 0) ou B (bit 1).
- Se algum dos dois está apertado: `snd_sfx_request(0x0D)` (0x08074DE0) e `ctx+4++` (ctx = 0x03000440).
- Se não: não avança e não mexe no pc; o FF repete no próximo quadro. **FF 0/2/3 = espera tecla A ou B, depois SFX
  0x0D e avança estado.** (O MS.md dizia "máscara provável"; agora fechado.)

### FF: alvo de 1/4 (0x080408BC = MsgFF_AdvanceOtherObj) — confirmado (disasm). CORRIGE MS.md
- `ldrb r0,[r3,#4]; add r0,#1; strb r0,[r3,#4]` com r3 = ctx = **0x03000440** (mesmo ctx, carregado de
  0x08040874 em 0x08040864 `add r3,r2,#0`). Não é "outro objeto". Só faz ctx+4++, sem SFX.
- Ou seja, FF 1 e FF 4 = avança estado sem som; FF >= 5 = avança estado (0x080408C4), o mesmo efeito.

### FF: limpeza antes do despacho (Script_OpSubcommandOrNextState 0x08040820) — confirmado
- Antes de tudo, se `ctx+0x14 & 7 != 0`: `bios_cpufastset(0x02020000, dst, 0x10)` com
  `dst = 0x06000280 + ((((ctx+0x14)+8)>>3)-1)*0x40 + ctx+0x17*0x680`: limpa o resto da coluna de tile da caixa.
- Zera ctx+0x14 (caneta X) e lê o subcomando de `pc+1`; tabela `PTR_MsgFF_SubcommandTable_08040880` = 0x08040884.

## 2. Handlers 9..22 (gMessageStateHandlerTable[9..22], mesma faixa de 0x083B2940) — decompile

| estado | entrada | nome no Ghidra | o que faz (decompile) |
|---|---|---|---|
| 9  | 0x080408F4 | Dispatch_CallHandlerByByte1B | `CallViaR0(tabela[ctx+0x1B])`; tabela 0x083B2954 (literal 0x0804090C) |
| 10 | 0x08040914 | Dispatch_CallHandlerByByte1B_08040914 | igual, tabela 0x083B296C (literal 0x0804092C) |
| 11 | 0x08040934 | Fight_CallHandlerByIndex | igual, tabela 0x083B2984 (literal 0x0804094C) |
| 12 | 0x08040954 | Dispatch_CallHandlerByIdx1B | igual, tabela 0x083B2988 (literal 0x0804096C) |
| 13 | 0x08040974 | MsgState13_CtxB1BDispatch | igual, tabela 0x083B2990 (JT2, 23 entradas; literal 0x0804098C) |
| 14/20 | 0x08040994 | BgMap_TickRectFillState | se ctx+1 != 2 e ctx+0x12 == 1: preenche/move retrato (3 ou 0x13 de tile), ctx+0x0E/0x12 = 0; depois ctx+0x1B++ |

Não confirmado: quais bytes de roteiro chegam a cada um; o estado 14 usa `Move_Character_Portraits` (nome já no Ghidra).

| estado | entrada | nome no Ghidra | o que faz (decompile) |
|---|---|---|---|
| 15/21 | 0x08040A00 | BgMap_TickFlashStep | se ctx+2 == 1: limpa retângulo 0x1E,6 (13 linhas) e ctx+0x1B += 2; senão ctx+0x1B += 1 |
| 16/22 | 0x08040A40 | BgMap_LoadTileStageStep | etapa por ctx+0x26: 1 = limpa retângulo; 2 = descomprime + tiles (Malias); 3 = descomprime + tiles; decrementa ctx+0x26; ao fim ctx+0x1B++ |
| 17 | 0x08040AF0 | Slot_AdvanceStepByFlag | se `*DAT_08040B00 == 0` ctx+0x1B += 1, senão += 2 |
| 18 | 0x08040B0C | Palette_FadeStepAndDispatch | fade de cor 0x1B2 (2 chamadas, A e B) com peso ctx+0x1C << 1; se ctx+0x1C == 0 ctx+0x1B++, senão ctx+0x1C--; depois salto por jump table não recuperado |
| 19 | 0x08040B50 | Slot_SetTimer8 | grava 8 em ctx+4 (volta a estado 8, "terminou") |

## 3. Tabela de handlers 9..22 (mapa)

Estados 9..22 são os entries 9..22 de `gMessageStateHandlerTable` 0x083B291C. A tabela 0x083B2940 do estado 6
é o mesmo array a partir do entry 9 (script byte = índice). Confirmado: ponteiros 0x080408F4, 0x08040914,
0x08040934, 0x08040954, 0x08040974, 0x08040994, 0x08040A00, 0x08040A40, 0x08040AF0, 0x08040B0C, 0x08040B50 (+ repetidos).

## 4. Pendências do MS2 (não fechadas nesta rodada)

- 0x08040898 e 0x080408BC: o Ghidra não tem função nelas (ficam dentro de 0x08040820 no disasm). Criar função
  pela UI do Ghidra ou manter só o CSV. Não renomeado (rename_function_by_address exige função).
- Conflito de nome: 0x08034AA0 está como `Fight_RollDamageFromTwoDice` no Ghidra e como
  `ActionScript_Op4D_RollDamageFromTwoDice` em `AS3c.md`/`AS3c_simbolos.csv` (que é o nome certo: é opcode 0x4D do
  action script). Não renomeado aqui; decisão para o Opus.

## 5. TX: quem chama Fight_RollDamageFromTwoDice e o dado

- Não há `BL` para 0x08034AA0 na ROM (varredura de BL com python: nenhum). Nem xref no Ghidra.
- Quem chama é o **despachante do action script**: `gActionScriptOpcodeHandlerTable` 0x083AF180 [opcode]. O alvo
  está em 0x083AF2B4 = 0x083AF180 + 0x4D*4, ou seja **opcode 0x4D**. Confirmado (ponteiro na ROM).
- O laço que lê o opcode é `Fight_RunActionScriptLoop` 0x08033840 (ver `DT2.md`/`AS1.md`).
- Dado usado: `fight+0x188` e vizinho recebem os dois dados; `fight+0xEC` (u16) = (d1+d2)*10 e é o acumulador de
  dano/ataque lido pela cadeia ROS/dano (`DM.md`: F+0xEC = acumulador de dano/ataque). Confiança: confirmado no
  decompile do handler; ligação com o dano aplicado: provável (vem de DM/MD).
- **Qual técnica usa o 0x4D: não fechado.** Ele aparece nos roteiros de técnica (TQ.md §2, lista de opcodes), mas
  não percorri os roteiros 0..59 de `gTechniqueActionScriptPtrTable` 0x083C6CA8. Próximo passo: varrer cada
  roteiro com o decodificador de opcodes (tamanho variável) e listar as técnicas que chegam a 0x4D.

## 6. TX: sites de texto pré/pós-batalha
- As quatro funções existem no Ghidra (0x08010814 Fight_ShowPreBattleTextForCharacter, 0x08010BD0
  Fight_ShowPostBattleTextForCharacter, 0x0803D9A0 Fight_ShowPreBattleTextIndex1, 0x0803DB4C
  Fight_ShowPostBattleTextIndex1), com corpo 0x08010814–0x0801084F etc.
- Decompile de 0x08010814 (confirmado): `Obj_StepAccumMoveA()`; `r = StateMachine_DispatchStep(tabela[byte 0x03001800])`;
  se r != 0: `Fight_UpdateBotPartEntryMotion()`, grava 8 em bloco+0x84 e +0x85 do bot (0x03001800+...) e
  incrementa o contador 0x0300137C; depois chama `Fight_UpdateBotPartEntryMotion()` de novo. Bate com TX.md §1.
- Quem chama esses sites continua aberto (TX.md §1): não há BL nem literal achado nesta rodada (a varredura de BL
  com python não encontrou chamadas para 0x08010814/0x08010815 nem 0x0803D9A0).
