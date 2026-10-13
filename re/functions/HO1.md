# HO1: handlers do ObjectScript (script de objetos da batalha)

Despachante `ObjectScript_OpcodeDispatch` 0x080393EC (Ghidra: já com nome). Tabela de handlers em ROM **0x083AF8C0**
(ponteiros de 4 B, thumb; índice = opcode, ×4).

Rodada GH-HO1-redo: todas as seções de handler abaixo foram refeitas com `decompile_function_by_address` (os
handlers por ponteiro agora são funções no Ghidra) e literais lidos na ROM EU. Nomes no Ghidra = nomes dos cabeçalhos.

## Tamanho real da tabela
- Entradas válidas: **0x00 a 0x3A (59 entradas)**. Índices 0x3B a 0x4F apontam todos para o stub 0x08000840 (`bx lr`, sem efeito).
- A partir de 0x50 a palavra deixa de ser handler (dados de outra estrutura).
- Portanto: opcode >= 0x3B = **fora da tabela** (stub, não faz nada). O tamanho 80 da lista era heurístico.
- A primeira linha de `HO1_lista.txt` (0x08000840, "Fight_DispatchOnSubstateAA...") não é handler: é o stub do índice 0x00.
  A lista só cobre 0x00 a 0x1D; os handlers 0x1E a 0x3A foram incluídos aqui. **0x29 (0x0803ACB4) faltava** na versão
  anterior deste arquivo; agora tem seção.

## Despachante, fluxo e tamanho dos argumentos (confirmado, decompilado)
- Laço externo `FUN_080393BC` (único chamador do despachante): `while (ctx+0x2A == 0) ObjectScript_OpcodeDispatch();`
  depois `FUN_0803B8C8()` e zera `ctx+0x2A`. Ou seja, **`ctx+0x2A` = "ceder o quadro"**: os handlers rodam em sequência
  no mesmo quadro até um deles gravar 1 ali. Handler que grava +0x2A sem avançar o cursor = espera (repete no quadro seguinte).
- `ObjectScript_ReadWord` 0x0803D424 lê o halfword do cursor; o byte baixo vira o opcode em `ctx+0x29`.
  Enquanto o **byte** tiver bit 7 ligado (`op*0x1000000 < 0`, i.e. 0x80..0xFF) o despachante avança e lê de novo:
  valores 0x80+ são **rótulos** (alvos de 0x02/0x03), não opcodes. (A nota antiga dizia bit 15: errado.)
- Fluxo: `stream = *(u32*)(0x083CE164 + 4*fight[+0xF9])` (banco = linha da técnica), cursor u16 em `fight+0x100`.
- **Tamanho dos argumentos**: tabela u8 em ROM **0x083CE374**, indexada pelo halfword do cursor (= opcode):
  `FightPreview_AdvanceSequenceCursor` 0x0803D4AC faz `cursor += 1 + argc[op]`; `FightPreview_UnpackSequenceEntry`
  0x0803D450 copia `argc[op]` halfwords (stream[cursor+1..]) para `fight+0x102..`. Neste arquivo `aN` = halfword N
  depois do opcode = `0x03001800 + 0x102 + 2*N`.
- argc por opcode (lido na ROM): 00:0 01:0 02:1 03:4 04:0 05:0 06:0 07:0 08:7 09:0 0A:7 0B:1 0C:1 0D:8 0E:1 0F:5
  10:5 11:3 12:0 13:8 14:0 15:5 16:0 17:1 18:8 19:2 1A:2 1B:5 1C:5 1D:0 1E:8 1F:8 20:0 21:7 22:7 23:0 24:7 25:3 26:3
  27:5 28:0 29:8 2A:2 2B:0 2C:8 2D:8 2E:4 2F:1 30:8 31:2 32:0 33:2 34:6 35:4 36:2 37:0 38:0 39:8 3A:0; 0x3B+ e 0x80+ = 0.
  Conferido handler a handler: o uso de `aN` no código bate com argc em todos (nenhuma divergência). Único caso com
  argumento não lido: 0x03 (argc 4, a flag só vale 0/1, então a2/a3 nunca são usados por ele).
- Contexto de objetos **0x03002C30** = vetor de **slots de 0x40 B** (`slot n = 0x03002C30 + n*0x40`). O slot 0 é o
  próprio contexto do script (+0x29 opcode, +0x2A ceder, +0x30 contador do 0x0E). Campos de slot vistos: +0x00 atributo
  (u16; 0x1000 = espelho H, 0x2000 = espelho V, 0x200), +0x02 X, +0x04 Y, +0x08/+0x09, +0x0A, +0x0B, +0x1C, +0x23..+0x2E
  parâmetros, **+0x29 = estado/comando do slot** (0 = livre/parado), +0x30 parâmetro u16. Slots fixos: 1 = bot do lado
  esquerdo, 2 = bot do lado direito (motion em +0x2B/+0x2C/+0x2D/+0x30), 3 (0xC0), 4 (0x100) e 0x23 (0x8C0) = efeitos
  com anim script, 9 (0x240) = efeito de BG, 10..18 = sprites livres, 12/13, 0x1E (0x780), 0x27 (0x9C0).
- Estado da batalha **0x03001800**: +0xF8 bit 1 = lado do atacante (espelha X: `0xF0 - x`, ângulo `0x20 - a & 0x1F`),
  +0xF6 bit 1 = troca a peça (+0xA7 vs +0x162), +0xFD = flag de condição (lida pelo 0x03), +0x87/+0x88 = bot
  atacante/alvo, +0x154..+0x15A = X/Y de cada lado, +0x15F/+0x160 = flags por lado.

## Handlers

## 0x08000840 ObjScript_Op00_Stub
- Assinatura: `void f(void)`; argc 0.
- O que faz: `bx lr`. Opcode 0 (e 0x3B a 0x4F, por ponteiro) não faz nada e nem avança o cursor.
- Confiança: confirmado (bytes 0x7047).

## 0x08039430 ObjScript_Op01_SetFlag2A_B5
- Assinatura: `void f(void)`; argc 0.
- O que faz: **fim do script**: `ctx+0x2A = 1` (cede o quadro) e `0x03001800+0xB5 = 1` (handler de passo concluído, ver H.md).
  Não avança o cursor: fica parado no 0x01.
- Memória: 0x03002C30+0x2A, 0x03001800+0xB5.
- Confiança: confirmado.

## 0x08039448 ObjScript_Op02_SkipToLabel
- Assinatura: `void f(void)`; argc 1 (a0 = rótulo).
- O que faz: **goto para frente**: avança o cursor entrada a entrada (cada passo pula `1+argc`) até achar um halfword
  igual a a0 e para logo depois dele. Tudo no mesmo quadro (não é espera). Rótulos são valores 0x80+.
- Chama: FightPreview_UnpackSequenceEntry 0x0803D450, FightPreview_AdvanceSequenceCursor 0x0803D4AC.
- Memória: 0x03001800 (+0xF9, +0x100, +0x102); ROM 0x083CE164.
- Para o Lua: laço de avanço até casar o rótulo (sem rótulo = laço infinito no jogo).
- Confiança: confirmado (corrige "espera de sincronização" da versão anterior).

## 0x080394C4 ObjScript_Op03_SkipToLabelByFlag
- Assinatura: `void f(void)`; argc 4 (a0..a3 = rótulos).
- O que faz: igual ao 0x02, mas o rótulo é `a[fight+0xFD]` (flag de condição gravada por 0x04..0x07 e 0x12, 0 ou 1).
  É o "if": flag 0 -> rótulo a0, flag 1 -> rótulo a1.
- Memória: 0x03001800 (+0xF9, +0xFD, +0x100, +0x102), ROM 0x083CE164.
- Confiança: confirmado.

## 0x08039540 ObjScript_Op04_SetFlagIfB7Is1
- Assinatura: `void f(void)`; argc 0.
- O que faz: `fight+0xFD = (fight+0xB7 == 1)`; avança.
- Memória: 0x03001800 +0xB7 (lê), +0xFD (escreve).
- Confiança: confirmado.

## 0x08039568 ObjScript_Op05_SetFlagFromF6Bit0
- Assinatura: `void f(void)`; argc 0.
- O que faz: `fight+0xFD = fight+0xF6 & 1`; avança. O "jump table não recuperado" do Ghidra é só o epílogo (retorno).
- Memória: 0x03001800 (+0xF6, +0xFD).
- Confiança: confirmado.

## 0x08039598 ObjScript_Op06_SetFlagIfB8Is2
- Assinatura: `void f(void)`; argc 0.
- O que faz: `fight+0xFD = (fight+0xB8 == 2)`; avança.
- Memória: 0x03001800 (+0xB8, +0xFD).
- Confiança: confirmado.

## 0x080395C0 ObjScript_Op07_SetFlagIfB8Is4
- Assinatura: `void f(void)`; argc 0.
- O que faz: `fight+0xFD = (fight+0xB8 == 4)`; avança.
- Memória: 0x03001800 (+0xB8, +0xFD).
- Confiança: confirmado.

## 0x080395E8 ObjectOpcode08_SetMotionFromTable
- Assinatura: `void f(void)`; argc 7 (a0 = parâmetro u16, a1..a3 = motion do slot 1 por peça, a4..a6 = motion do slot 2).
- O que faz: lado do atacante (`+0xF8 & 2 == 0`) -> slot 1: `+0x6B = 1`, `+0x6C = a[1 + peça]`, `+0x70 = a0`, `+0x6D = 0`;
  senão slot 2 (`+0xAB = 1`, `+0xAC = a[4 + peça]`, `+0xB0 = a0`, `+0xAD = 0`). Peça = `fight+0xA7` (ou `+0x162` se `+0xF6 & 2`).
  Bate com A4 (`fight_information+0x104+2*k` = a1+k). No Ghidra estava FUN_080395E8: renomeado com o nome do mtbe.
- Memória: 0x03001800 (+0xF6, +0xF8, +0xA7, +0x162, +0x102..+0x10E), 0x03002C30 (+0x6B..+0x70, +0xAB..+0xB0).
- Para o Lua: o PC (`extract_motion.py`) lê os valores a partir de `cursor+2` com `cnt = argc = 7`: pega a1..a7, um
  halfword a mais (a7 = próximo opcode), ignora a0 e o lado (slot 2 usa a4..a6).
- Confiança: confirmado.

## 0x080396C4 ObjScript_Op09_SetFlag2A
- Assinatura: `void f(void)`; argc 0.
- O que faz: `ctx+0x2A = 1` sem avançar: **parada** (cede todo quadro, para sempre, sem sinalizar +0xB5 como o 0x01).
- Memória: 0x03002C30+0x2A.
- Confiança: confirmado.

## 0x080396D4 ObjScript_Op0A_SetMotionFromTableOtherSide
- Assinatura: `void f(void)`; argc 7 (mesmo layout do 0x08).
- O que faz: espelho do 0x08: lado do atacante -> slot 2 (`+0xAB = 1`, `+0xAC = a[4+peça]`, `+0xB0 = a0`, `+0xAD = 0`);
  senão slot 1 (`a[1+peça]`). Ou seja, mexe no bot do outro lado.
- Memória: como o 0x08.
- Confiança: confirmado.

## 0x080397B0 ObjScript_Op0B_SetupSlot3Anim
- Assinatura: `void f(void)`; argc 1.
- O que faz: slot 3: `+0xEB = a0`, `+0xEC = lado` (0/1), `+0xF0 = fight+0x154` (ou `+0x158`: X do lado),
  `+0xE9 = 8` (estado). Avança.
- Memória: 0x03001800 (+0x102, +0xF8, +0x154, +0x158), 0x03002C30 (+0xE9, +0xEB, +0xEC, +0xF0).
- Confiança: confirmado (código); nome provável.

## 0x0803981C ObjScript_Op0C_WaitSideFlag
- Assinatura: `void f(void)`; argc 1 (a0 = 0 próprio lado, 1 outro lado, outro valor = ambos).
- O que faz: espera: se a flag do lado (`fight+0x15F` ou `+0x160`, escolhida por a0 e `+0xF8 & 2`) for != 0 avança;
  com a0 >= 2 exige as duas. Sempre cede o quadro (`ctx+0x2A = 1`).
- Memória: 0x03001800 (+0x102, +0xF8, +0x15F, +0x160), 0x03002C30+0x2A.
- Confiança: confirmado (código); papel das flags +0x15F/+0x160 provável.

## 0x080398AC ObjScript_Op0D_SpawnEffectSlot4PartPal
- Assinatura: `void f(void)`; argc 8.
- O que faz: efeito no slot 4: paleta da peça do atacante (bot `+0x87`: `+0x49 + 0x18*peça`, ou `+0xA8 + 0x50`) via
  `Fight_CopyPaletteRowFromTable(pal, a4, 9)`; `+0x12E = a0`, `+0x12F = a1`, `+0x12B = a6` (anim: tabela 0x083CEDF4 ->
  `Fight_InitAnimScriptSlot(.., 0, 0)`), `+0x130 = a5`, `+0x129 = 0x10` (estado), Y `+0x104 = a3`, X `+0x102 = a2`
  (espelhado: `0x1E - 2*(a0>>1) - a2`), `+0x12C = a7` (+2 espelhado), `+0x12D = 0`.
- Memória: 0x03000590 (bot), 0x03001800 (+0x87, +0xA7, +0xF8, +0x102..+0x110), 0x03002C30 (+0x102..+0x130).
- Confiança: confirmado.

## 0x080399EC ObjScript_Op0E_WaitFrames
- Assinatura: `void f(void)`; argc 1 (a0 = quadros).
- O que faz: contador em `ctx+0x30`: se == a0 zera e avança; senão incrementa. Sempre cede o quadro. Espera a0 quadros.
- Memória: 0x03002C30 (+0x30, +0x2A), 0x03001800+0x102.
- Confiança: confirmado.

## 0x08039A2C ObjScript_Op0F_SetSlotMotion
- Assinatura: `void f(void)`; argc 5.
- O que faz: slot `a0+5`: `+0x30 = a1`, `+0x2B = a2`, `+0x2C = 0`, `+0x2D = a3`, `+0x2E = a4`, `+0x29 = 0x18` (estado).
- Memória: 0x03001800 (+0x102..+0x10A), 0x03002C30 (slots 5+).
- Confiança: confirmado (código); nome provável.

## 0x08039AC8 ObjScript_Op10_SetSlotMotionByDamage
- Assinatura: `void f(void)`; argc 5.
- O que faz: como o 0x0F (slot `a0+5`, `+0x30 = a1`, `+0x2D = a3`, `+0x2E = a4`, estado 0x18), mas `+0x2B` vem da tabela
  `0x083AFA08[a2]` indexada por `bot[+0x91] - 100` (bot = `fight+0x87`, ou `+0x161` se `+0xF6 & 2`), ou 3 se `fight+0x67 >= 0x78`.
  Antes chama `Fight_GetDisplayedPartIdsOrBrokenGlyphs(bot, fight+0x64)`.
- Memória: 0x03000590 (+0x91), 0x03001800 (+0x64, +0x67, +0x87, +0x161, +0xF6), 0x03002C30.
- Confiança: confirmado (código); significado de +0x91/+0x67 chute.

## 0x08039BFC ObjScript_Op11_SetupSlot3Effect
- Assinatura: `void f(void)`; argc 3.
- O que faz: slot 3: `+0xE4 = a0`, `+0xE5 = a1` (espelhado `0x20 - a1 & 0x1F`), `+0xED = a2`, `+0xEC = lado`,
  `+0xC2/+0xC4 = fight+0x154/+0x156` (ou `+0x158/+0x15A`), `+0xE9 = 9` (estado).
- Memória: 0x03001800 (+0x102..+0x106, +0xF8, +0x154..+0x15A), 0x03002C30 (+0xC2..+0xED).
- Confiança: confirmado.

## 0x08039CB4 ObjScript_Op12_SetFlagFromBotBC20
- Assinatura: `void f(void)`; argc 0.
- O que faz: `fight+0xFD = (bot[fight+0x88].+0xBC & 0x20) != 0` (bot alvo); avança.
- Memória: 0x03000590 + bot*0x100 + 0xBC, 0x03001800 (+0x88, +0xFD).
- Confiança: confirmado (código); significado do bit chute.

## 0x08039CF8 ObjScript_Op13_SpawnEffectSlot4FixedPal
- Assinatura: `void f(void)`; argc 8.
- O que faz: igual ao 0x0D, mas a paleta é fixa (linha 0x3C) em vez da peça.
- Confiança: confirmado.

## 0x08039E00 ObjScript_Op14_SpawnActorOffscreen
- Assinatura: `void f(void)`; argc 0.
- O que faz: põe o bot do lado do atacante fora da tela (lado 0: X `fight+0x154 = 300`, Y 0x50; lado 1: X `0x03001958 = 0x1C4`,
  Y 0x50), carrega as peças exibidas (`FightPreview_LoadDisplayedPartIdsForSide`), desenha duas camadas (`FUN_080447EC`)
  e inicia o movimento 0x29 (`fight+0x112/+0x113 = 0x29`, `MotionPlayStep(lado, 0x29, ..)`).
- Memória: 0x03001800 (+0xF8, +0x112..+0x115, +0x154..+0x15A).
- Confiança: confirmado (código); papel provável (entrada em cena).

## 0x08039FE4 ObjScript_Op15_SpawnSlot1EState0A
- Assinatura: `void f(void)`; argc 5.
- O que faz: `Tile_ClearSlots(0x1E, 1)` e slot 0x1E (0x780): `+0x2B = a0`, X `+0x02 = a1` (espelhado `0xF0 - a1`), Y `+0x04 = a2`,
  ângulo `+0x25 = a3` (espelhado), `+0x24 = a4`, `+0x29 = 0x0A`; `+0x30 = X`.
- Memória: 0x03001800 (+0x102..+0x10A, +0xF8), 0x03002C30 (+0x780..+0x7B0).
- Confiança: confirmado.

## 0x0803A0B8 ObjScript_Op16_WaitSlot1EIdle
- Assinatura: `void f(void)`; argc 0.
- O que faz: espera o slot 0x1E terminar (`ctx+0x7A9 == 0` -> avança); sempre cede o quadro.
- Confiança: confirmado.

## 0x0803A0E0 ObjScript_Op17_LoadObjGfxAndPal
- Assinatura: `void f(void)`; argc 1.
- O que faz: tabela `0x083AFA0C` (pares gfx/paleta): `decompress_and_store(tab[2*a0], 0x06010000)` (tiles OBJ) e
  `CpuFastSet(tab[2*a0+1], 0x050003C0, 8)` (paleta OBJ 14, 32 B).
- Confiança: confirmado.

## 0x0803A130 ObjScript_Op18_SpawnFreeSlotState0B
- Assinatura: `void f(void)`; argc 8.
- O que faz: acha slot livre 10..18 (+0x00 == 0; se nenhum, índice indefinido: bug latente), limpa e inicia: atributo
  `(a6 << 10) | 0x4004` (+0x1000 espelhado), `+0x1C = 0`, `+0x0A = 0x0E`, `+0x0B = 0x80`, `+0x2D = a7` ->
  `Slot_SetupFromRecordList(0x083B6100[a7], slot)`, `+0x2B = a0`, X = a1 (espelhado), Y = a2, ângulo `+0x25 = a3`,
  `+0x24 = a4`, `+0x2C = a5`, `+0x29 = 0x0B`.
- Confiança: confirmado.

## 0x0803A264 ObjScript_Op19_WaitSlotReachX
- Assinatura: `void f(void)`; argc 2.
- O que faz: espera até o X do slot a1 chegar a a0 (`a0 <= X`; espelhado `X <= 0xF0 - a0`); sempre cede o quadro.
- Confiança: confirmado.

## 0x0803A2E8 ObjScript_Op1A_ClearSlots
- Assinatura: `void f(void)`; argc 2.
- O que faz: `Tile_ClearSlots(a0, a1)` (libera a1 slots a partir de a0).
- Confiança: confirmado.

## 0x0803A310 ObjScript_Op1B_SetSlotState20
- Assinatura: `void f(void)`; argc 5.
- O que faz: slot a2: `+0x2B = a0`, `+0x2D = a1`, `+0x2E = a3`, `+0x0A = a4`, `+0x29 = 0x20`.
- Confiança: confirmado.

## 0x0803A370 ObjScript_Op1C_SpawnSlot1EState0C
- Assinatura: `void f(void)`; argc 5.
- O que faz: igual ao 0x15, com estado `+0x29 = 0x0C`.
- Confiança: confirmado.

## 0x0803A444 ObjScript_Op1D_SetBg0CntClearMap
- Assinatura: `void f(void)`; argc 0.
- O que faz: `BG0CNT (0x04000008) = 0x4500` e `FUN_0803EFD8(0x40, 0x20, 0, 0, BG0CNT, 0)` (preenche o mapa 64x32 do BG0).
- Confiança: confirmado (código); papel de FUN_0803EFD8 provável.

## 0x0803A474 ObjScript_Op1E_SpawnEffectSlot23PartPal
- Assinatura: `void f(void)`; argc 8.
- O que faz: como o 0x0D, mas no slot 0x23 (0x8C0), com `Tile_ClearSlots(0x23,1)` antes, estado `+0x8E9 = 0x11`,
  `Fight_InitAnimScriptSlot(.., 1, 1)`, X espelhado `0x1D - 2*(a0>>1) - a2`, e zera `+0x8ED` e `+0x8E8`.
- Confiança: confirmado.

## 0x0803A5CC ObjScript_Op1F_SpawnEffectSlot4State12
- Assinatura: `void f(void)`; argc 8.
- O que faz: como o 0x0D (slot 4, paleta da peça), estado `0x12`, Y fixo 4, X = a2 (lado 0) ou a3 (lado 1, sem conta de espelho).
- Confiança: confirmado.

## 0x0803A6FC ObjScript_Op20_ClearTilesReset
- Assinatura: `void f(void)`; argc 0.
- O que faz: limpa os dois mapas 32x32 do BG0 (`FUN_0803EFD8`), `BG0CNT = 0x500`, zera `0x03005000` e `0x03004C30`,
  `Tile_ClearSlots(4,1)` e `(0x23,1)`. Fim de efeito (desfaz 0x0D/0x1E/0x1D).
- Confiança: confirmado.

## 0x0803A760 ObjScript_Op21_SetMotionOwnSideHold
- Assinatura: `void f(void)`; argc 7.
- O que faz: igual ao 0x08, mas grava `+0x2D = 1` (em vez de 0) no slot do bot.
- Confiança: confirmado (código); significado de +0x2D chute.

## 0x0803A840 ObjScript_Op22_SetMotionOtherSideHold
- Assinatura: `void f(void)`; argc 7.
- O que faz: igual ao 0x0A (bot do outro lado), com `+0x2D = 1`.
- Confiança: confirmado.

## 0x0803A920 ObjScript_Op23_SetFlagAdvance
- Assinatura: `void f(void)`; argc 0.
- O que faz: `ctx+0x8E7 = 1` (slot 0x23 +0x27: sinal para o efeito do 0x1E) e avança.
- Confiança: confirmado (código); papel provável.

## 0x0803A93C ObjScript_Op24_SpawnSlot12or13State22
- Assinatura: `void f(void)`; argc 7.
- O que faz: slot 12 se livre (`+0x329 == 0`), senão 13: `+0x2B = a0`, `+0x2E = a1`, `+0x30 = a2`, `+0x32 = a3`,
  `+0x2C = a4`, `+0x2D = a5`, `+0x27 = a6`, `+0x28 = 0`, `+0x29 = 0x22`.
- Confiança: confirmado.

## 0x0803AA00 ObjScript_Op25_SetSlotAngle
- Assinatura: `void f(void)`; argc 3.
- O que faz: slot a0: `+0x25 = a1` (espelhado `0x20 - a1 & 0x1F`), `+0x24 = a2`.
- Confiança: confirmado.

## 0x0803AA90 ObjScript_Op26_ToggleSlotFlip
- Assinatura: `void f(void)`; argc 3.
- O que faz: slot a0: se a1 != 0 inverte bit 0x1000 do atributo (espelho H); se a2 != 0 inverte 0x2000 (espelho V).
- Confiança: confirmado.

## 0x0803AB00 ObjScript_Op27_StartSlot27PalEffect
- Assinatura: `void f(void)`; argc 5.
- O que faz: slot 0x27 (0x9C0): a0 == 0 -> `CopyPaletteBuffersToHardware()`, `+0x2F = 0`, estado 0x28; senão estado 0x29.
  `+0x2B..+0x2E = a1..a4`.
- Confiança: confirmado (código); "efeito de paleta" provável.

## 0x0803AB9C ObjScript_Op28_SaveObjPalsToBuffer
- Assinatura: `void f(void)`; argc 0.
- O que faz: 4 `CpuFastSet(.., 8)` da paleta OBJ do hardware para o buffer: lado 0 linhas 8-11 (0x05000300.. ->
  0x0202F900..), lado 1 linhas 4-7 (0x05000280.. -> 0x0202F880..). Direção hardware -> buffer (a nota anterior dizia o contrário).
- Confiança: confirmado.

## 0x0803ACB4 ObjScript_Op29_SpawnFreeSlotState0D
- Assinatura: `void f(void)`; argc 8.
- O que faz: igual ao 0x18 (slot livre 10..18), estado `+0x29 = 0x0D`. Faltava na versão anterior.
- Confiança: confirmado.

## 0x0803AC4C ObjScript_Op2A_SavePalRowToBuffer
- Assinatura: `void f(void)`; argc 2.
- O que faz: a0 == 0: linha a1 da paleta BG (0x05000000 + 0x20*a1) -> 0x0202FA00 + 0x20*a1; senão linha a1 da paleta OBJ
  (0x05000200 + 0x20*a1) -> 0x0202F800 + 0x20*a1.
- Confiança: confirmado.

## 0x0803ADE8 ObjScript_Op2B_ClearSlots12And13
- Assinatura: `void f(void)`; argc 0.
- O que faz: `Tile_ClearSlots(0x0C, 2)` (desfaz o 0x24).
- Confiança: confirmado.

## 0x0803ADFC ObjScript_Op2C_SpawnEffectSlot4State13
- Assinatura: `void f(void)`; argc 8.
- O que faz: igual ao 0x1F, estado `0x13`.
- Confiança: confirmado.

## 0x0803AF2C ObjScript_Op2D_SetupFourSlots
- Assinatura: `void f(void)`; argc 8.
- O que faz: centro no slot 10 (0x280): `+0x2B = a0`, X = a1 (espelhado), Y = a2, ângulo = a3, `+0x24 = a4`, `+0x2C = a5`,
  estado 0x30. Mais 4 sprites nos slots 11..14 (atributo `(a6<<10)|0x4004`, `+0x2D = a7` -> record list 0x083B6100,
  `+0x2B = a0`, `+0x2C = a5`, estado 0x31, ângulo `u*0x40 - 0x20`) em volta do centro, raio 0x10
  (`CompositeSprite_ComputeOffsetWord`).
- Confiança: confirmado.

## 0x0803B0C0 ObjScript_Op2E_SetupFourSprites
- Assinatura: `void f(void)`; argc 4.
- O que faz: slots 10..13: atributo 0xC004, X/Y da tabela ROM 0x083AFACC (4 x {u16 X, Y}; X espelhado `0xF6 - X & 0x1FF`),
  `+0x2B = +0x2C = a0`, ângulo `+0x25 = a1`, `+0x24 = a2`, `+0x2D = a3` -> record list 0x083B6100, estado 0x0B.
- Confiança: confirmado.

## 0x0803B19C ObjScript_Op2F_LoadGfxToEwramAndPal
- Assinatura: `void f(void)`; argc 1.
- O que faz: tabela 0x083AFADC: `Gfx_LoadDefaultAsset(tab[2*a0], 0x02000000)` e paleta `tab[2*a0+1]` -> 0x050003C0.
- Confiança: confirmado.

## 0x0803B1EC ObjScript_Op30_SetupFreeSlot
- Assinatura: `void f(void)`; argc 8.
- O que faz: igual ao 0x18 (slot livre 10..18), estado `+0x29 = 0x0E`.
- Confiança: confirmado.

## 0x0803B328 ObjScript_Op31_SetSlotAnimScript
- Assinatura: `void f(void)`; argc 2.
- O que faz: slot a1: `+0x2E = a0`; `Fight_InitAnimScriptSlotB(0x083CEDF4[a0], 0, 0)`.
- Confiança: confirmado.

## 0x0803B374 ObjScript_Op32_LoadGfxList
- Assinatura: `void f(void)`; argc 0.
- O que faz: `Fight_LoadGfxListToObjVram(gfx da peça do atacante)` (mesma escolha de peça do 0x0D: `+0x49 + 0x18*peça`
  ou `+0xA8 + 0x50`).
- Confiança: confirmado.

## 0x0803B3C8 ObjScript_Op33_LoadObjGfxAtTileAndPal
- Assinatura: `void f(void)`; argc 2.
- O que faz: como o 0x17 (tabela 0x083AFA0C[a0]), mas o destino é `0x06010000 + 0x20*a1` (tile a1).
- Confiança: confirmado.

## 0x0803B428 ObjScript_Op34_SetupAffineSlotState34
- Assinatura: `void f(void)`; argc 6.
- O que faz: slot a0: atributo `|= 0x200`, `+0x23 = a1`, `+0x2D = a2` -> record list 0x083B6100, `+0x26 = a3`,
  `+0x08 = a4`, `+0x09 = a5`, `+0x18 = 0`, estado 0x34.
- Confiança: confirmado (código); "affine" chute.

## 0x0803B4F8 ObjScript_Op35_SetupEightSprites
- Assinatura: `void f(void)`; argc 4.
- O que faz: 8 sprites nos slots 11..18 em círculo em volta do slot a0 (raio a3, ângulo `u*0x20`), atributo 0xC604,
  `+0x08 = +0x09 = 8`, `+0x23 = a1`, `+0x2D = a2` -> record list, `+0x2B = 0`, estado 0x36.
- Confiança: confirmado.

## 0x0803B5DC ObjScript_Op36_LoadBgFxStartSlot9
- Assinatura: `void f(void)`; argc 2.
- O que faz: descomprime 4 gráficos de BG (0x084C571C, 0x08487D90, 0x084C3E64, 0x084C7B64 -> 0x06006400/6520/6600/6800)
  e 3 paletas BG (-> 0x05000140, 0x05000100, 0x05000160); slot 9: `+0x26D = lado`, `+0x26E = 1`, `+0x26B = a0`,
  `+0x26C = a1`, estado `+0x269 = 0x38`.
- Confiança: confirmado.

## 0x0803B6D8 ObjScript_Op37_WaitThenSetFlag
- Assinatura: `void f(void)`; argc 0.
- O que faz: espera o slot 9 terminar (`ctx+0x269 == 0` -> avança); sempre cede o quadro. Par do 0x36.
- Confiança: confirmado.

## 0x0803B700 ObjScript_Op38_ClearTilesFx
- Assinatura: `void f(void)`; argc 0.
- O que faz: `FUN_0803EF8C(0x20, 0x20, 0, 0, BGxCNT, 0)` para BG0 e BG1, e `Tile_ClearSlots(4,1)` / `(0x23,1)`.
- Confiança: confirmado.

## 0x0803B754 ObjScript_Op39_SpawnEffectSlot4NoPrioShift
- Assinatura: `void f(void)`; argc 8.
- O que faz: igual ao 0x0D, mas `+0x12C = a7` sem o +2 quando espelhado.
- Confiança: confirmado.

## 0x0803B884 ObjScript_Op3A_StartTwinSlotsSfx
- Assinatura: `void f(void)`; argc 0.
- O que faz: slots 10 e 11: `+0x28 = fight+0x188+u`, `+0x23 = 0`, `+0x26 = 0x56+u`, estado 0x3A; toca o efeito sonoro 0x10
  (`snd_sfx_request`).
- Confiança: confirmado (código); papel chute.

## Fora da tabela: opcodes 0x3B a 0x4F
- Apontam todos para 0x08000840 (stub `bx lr`). Sem efeito. Opcodes >= 0x50: a palavra na tabela não é handler (dados de outra estrutura).
- Halfwords com byte baixo 0x80..0xFF não chegam à tabela: são rótulos pulados pelo despachante.
