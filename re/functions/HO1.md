# HO1: handlers do ObjectScript (script de objetos da batalha)

Despachante `ObjectScript_OpcodeDispatch` 0x080393EC (Ghidra: já com nome). Tabela de handlers em ROM **0x083AF8C0**
(ponteiros de 4 B, thumb; índice = opcode, ×4).

## Tamanho real da tabela
- Entradas válidas: **0x00 a 0x3A (59 entradas)**. Índices 0x3B a 0x4F apontam todos para o stub 0x08000840 (`bx lr`, sem efeito).
- A partir de 0x50 a palavra deixa de ser handler (dados de outra estrutura).
- Portanto: opcode >= 0x3B = **fora da tabela** (stub, não faz nada). O tamanho 80 da lista era heurístico.
- A primeira linha de `HO1_lista.txt` (0x08000840, "Fight_DispatchOnSubstateAA...") não é handler: é o stub do índice 0x00.
  A lista só cobre 0x00 a 0x1D; os handlers 0x1E a 0x3A foram incluídos aqui.

## Despachante (confirmado, decompilado)
- `ObjectScript_ReadWord` 0x0803D424 lê um halfword do fluxo; o byte baixo vira o opcode em `ctx+0x29`.
- Enquanto o halfword tiver bit 15 (sinal) ligado, o despachante chama `FightPreview_AdvanceSequenceCursor` 0x0803D4AC e lê de novo.
  Ou seja: halfwords com bit 15 = prefixo/continuação, não opcode.
- Handler = `tabela[opcode]` via `CallViaR0`. Argumentos: os handlers leem os halfwords seguintes com
  `ObjectScript_ReadWord` / `FightPreview_UnpackSequenceEntry` 0x0803D450 (copia a entrada de bloco) e avançam o cursor.
- Ponteiro de banco: `ctx_B` = 0x03001800; byte +0xF9 escolhe o banco (tabela de ponteiros em ROM, p. ex. 0x083CE164
  nos handlers 02/03); cursor ushort +0x100; entrada/registro +0x102.
- Contexto de script: **0x03002C30** (opcode +0x29, flag de fim de bloco +0x2A, ver handlers 01/09).
- Estado da batalha: **0x03001800** (+0xB7, +0xB8, +0xF6, +0xF8, +0xFD: flags/estado lidos pelos handlers 04 a 07).
- Confiança geral dos handlers 0x01 a 0x3A: **triagem** (desmonte com capstone da ROM original, resumo de chamadas,
  literais e deslocamentos). Exceto onde marcado "confirmado", o papel de cada um é chute ou provável.

## Handlers

## 0x08000840 ObjScript_Op00_Stub
- Assinatura: `void f(void)`
- O que faz: `bx lr`. Opcode 0 (e 0x3B a 0x4F, por ponteiro) não faz nada.
- Confiança: confirmado (bytes 0x7047).

## 0x08039430 ObjScript_Op01_SetFlag2A_B5
- Assinatura: `void f(void)` (sem argumento)
- O que faz: grava 1 em `ctx+0x2A` (fim de bloco) e 1 em `0x03001800+0xB5`.
- Memória: 0x03002C30+0x2A, 0x03001800+0xB5.
- Confiança: confirmado (código de 8 instruções); nome provável.

## 0x08039448 ObjScript_Op02_WaitHalfwordMatch
- Assinatura: `void f(void)`; argumentos: 2 halfwords seguintes lidos pelo cursor (entrada do banco).
- O que faz: laço: a cada passo compara o halfword atual do bloco (`cursor`) com `tabela_ROM[banco]` (ponteiro 0x083CE164)
  indexado pelo cursor; avança o cursor e repete até casar. Só sai quando achou o valor (espera de sincronização).
- Chama: FightPreview_UnpackSequenceEntry 0x0803D450, FightPreview_AdvanceSequenceCursor 0x0803D4AC.
- Memória: 0x03001800 (+0xF9, +0x100, +0x102); ROM 0x083CE164.
- Para o Lua: portar como espera (while não casado: avança).
- Confiança: confirmado (decompilado no Ghidra; renomeado).

## 0x080394C4 ObjScript_Op03_WaitHalfwordMatchFlag
- Assinatura: `void f(void)`
- O que faz: mesma espera de 0x02, mas o alvo é indexado por `0x03001800+0xFD` (pelo byte de flag de 0x04/0x05) e
  compara com `tabela_ROM`; ao casar, grava 1 na flag local.
- Memória: 0x03001800 (+0xF9, +0xFD, +0x100, +0x102), ROM 0x083CE164.
- Confiança: provável (desmonte, sem decompilação).

## 0x08039540 ObjScript_Op04_SetFlagFromB7
- Assinatura: `void f(void)`
- O que faz: copia para `+0xFD` um valor derivado do byte `+0xB7` (1 se for 1, senão 0).
- Memória: 0x03001800+0xB7 (lê), +0xFD (escreve).
- Confiança: provável (desmonte curto, 19 instruções).

## 0x08039568 ObjScript_Op05_SetFlagFromF6Bit0
- Assinatura: `void f(void)`
- O que faz: `+0xFD` = bit 0 de `+0xF6` (1 ou 0), depois avança o cursor e chama um alvo por ponteiro (jump table não recuperada).
- Memória: 0x03001800 (+0xF6, +0xFD).
- Confiança: provável (Ghidra: FUN_08039568 renomeada).

## 0x08039598 ObjScript_Op06_SetFlagIfB8Is2
- Assinatura: `void f(void)`
- O que faz: `+0xFD` = 1 se `+0xB8 == 2`, senão 0. Depois avança o cursor.
- Memória: 0x03001800 (+0xB8, +0xFD).
- Confiança: provável.

## 0x080395C0 ObjScript_Op07_SetFlagIfB8Is4
- Assinatura: `void f(void)`
- O que faz: igual ao 0x06, mas compara `+0xB8 == 4`.
- Memória: 0x03001800 (+0xB8, +0xFD).
- Confiança: provável.

## 0x080395E8 ObjectOpcode08_SetMotionFromTable
- Assinatura: `void f(void)`; argumentos: halfword de índice `k` (cursor).
- O que faz: escreve no slot de objeto (+0x2C = tipo de movimento) lendo `fight_information+0x104+2*k`
  (ver A4 em `pc/research/README.md`; store em 0x08039640). Ponto de entrada da cinemática de objeto.
- Memória: 0x03001800 (+0xF6, +0xF8, +0x6B, +0x6C, +0x70, +0xA7, +0xAB, +0xAC, +0xB0), 0x03002C30 (+0x6B).
- Confiança: confirmado (A4, rec_2), tabela ok; detalhes de argumento provável.

## 0x080396C4 ObjScript_Op09_SetFlag2A
- Assinatura: `void f(void)`
- O que faz: `ctx+0x2A = 1` e volta. Fim de bloco sem efeito colateral.
- Memória: 0x03002C30+0x2A.
- Confiança: confirmado (5 instruções).

## 0x080396D4 ObjScript_Op0A_SetMotionFromTableAlt
- Assinatura: `void f(void)`
- O que faz: igual ao 0x08 (mesma estrutura com `+0xAB`, `cursor` e tabela de +0x104), com o deslocamento de linha +4 em vez de +0.
- Memória: como o 0x08 (0x03001800 e 0x03002C30).
- Confiança: provável (desmonte; não comparado com rec_2).

## 0x080397B0 ObjScript_Op0B_SetDataFrom181
- Assinatura: `void f(void)`
- O que faz: copia o halfword `0x03001800+0x181` para `0x03002C30+0xEB` (byte), e marca `+0xEC` conforme `+0xF8 & 2`.
- Memória: 0x03001800 (+0x181, +0xF8), 0x03002C30 (+0xEB, +0xEC).
- Confiança: chute (nome) / provável (leitura de memória).

## 0x0803981C ObjScript_Op0C_SetFlag2AIfF8
- Assinatura: `void f(void)`
- O que faz: usa `+0xF8 & 2` como condição, grava `ctx+0x2A`; usa um literal 0x15F (valor de ID). Resto não lido.
- Memória: 0x03001800+0xF8, 0x03002C30+0x2A.
- Confiança: chute.

## 0x080398AC ObjScript_Op0D (sem nome)
- Assinatura: `void f(void)`; chama rotinas de desenho (0x0801BCE4, 0x0801BD1C), IDs 0x129/0x12F.
- O que faz: maior handler de desenho/efeito (120 instruções, não resolvido por completo). Provável que escreva OAM ou efeito de tela.
- Confiança: chute.

## 0x080399EC ObjScript_Op0E (sem nome)
- O que faz: escreve 1 em `ctx+0x2A` e chama FightPreview_AdvanceSequenceCursor.
- Memória: 0x03002C30+0x2A.
- Confiança: chute (flag de fim de bloco).

## 0x08039A2C ObjScript_Op0F (sem nome)
- O que faz: usa `ctx+0x29` (opcode) e campos `+0x2B` a `+0x2E` do slot de objeto (parâmetros de movimento); 71 instruções.
- Confiança: chute.

## 0x08039AC8 ObjScript_Op10 (sem nome)
- O que faz: chama 0x08015838, mexe em `0x03000590` (bloco do bot) e campos do slot `+0x29`..`+0x2E`, `+0x64`, `+0x67`, `+0x87`, `+0x91`, `+0xF6`.
- Confiança: chute.

## 0x08039BFC ObjScript_Op11 (sem nome)
- O que faz: grande (87 instruções); mexe em `+0xC2`, `+0xC4`, `+0xE4`, `+0xE5`, `+0xE9`, `+0xEC`, `+0xED`, `+0xF8` do bloco de estado.
- Confiança: chute.

## 0x08039CB4 ObjScript_Op12 (sem nome)
- O que faz: lê `0x03000590` (bot) e `0x03001800+0xFD`; usa `+0x88`, `+0xBC`. Provável: flag de bot/peça.
- Confiança: chute.

## 0x08039CF8 ObjScript_Op13 (sem nome)
- O que faz: chama 0x0801BCE4 / 0x0801BD1C (desenho), IDs 0x129, 0x12B, 0x12D, 0x12F; ~120 instruções.
- Confiança: chute.

## 0x08039E00 ObjScript_Op14 (sem nome)
- O que faz: chama 0x08036120, 0x080447EC, 0x08044A3C; lê `0x0300183C..0x03001867` (campos de estado de outra estrutura).
- Confiança: chute.

## 0x08039FE4 ObjScript_Op15 (sem nome)
- O que faz: chama 0x08040CD0 e IDs 0x782, 0x7A5, 0x7A9, 0x7AB (provável: spawn de efeito/sprite).
- Confiança: chute.

## 0x0803A0B8 ObjScript_Op16 (sem nome)
- O que faz: 15 instruções; lê ID 0x7A9 e `ctx+0x2A`.
- Confiança: chute.

## 0x0803A0E0 ObjScript_Op17 (sem nome)
- O que faz: chama 0x080009BC (decoder Malias, ver README), 0x0807CCC0 (wrapper CpuFastSet, ver README), VRAM 0x06010000.
  Carga de gráfico na VRAM. Provável.
- Confiança: provável.

## 0x0803A130 ObjScript_Op18 (sem nome)
- O que faz: chama 0x08040B90, 0x08040CD0; literal 0x4004; mexe no slot `+0x24..+0x2D`. Chute.
- Confiança: chute.

## 0x0803A264 ObjScript_Op19 (sem nome)
- O que faz: lê `ctx+0x2A`, `+0xF8`; 60 instruções.
- Confiança: chute.

## 0x0803A2E8 ObjScript_Op1A (sem nome)
- O que faz: chama 0x08040CD0; escreve `+0x2`; 14 instruções.
- Confiança: chute.

## 0x0803A310 ObjScript_Op1B (sem nome)
- O que faz: mexe em `ctx+0x2B`, `+0x2D`, `+0x2E` do slot; chama o desenho base.
- Confiança: chute.

## 0x0803A370 ObjScript_Op1C (sem nome)
- O que faz: igual ao 0x15 (mesmos IDs 0x782, 0x7A5, 0x7A9, 0x7AB, mesmo 0x08040CD0).
- Confiança: provável (desmonte de estrutura igual; não conferido).

## 0x0803A444 ObjScript_Op1D (sem nome)
- O que faz: escreve no registrador de DMA `0x04000008` (BG0CNT) e chama 0x0803EFD8. Ajuste de fundo.
- Confiança: provável.

## 0x0803A474 ObjScript_Op1E (sem nome)
- O que faz: chama 0x0801BCE4, 0x0801BD1C, 0x08040CD0; IDs 0x8C4, 0x8E9, 0x8EB, 0x8EE; 120 instruções.
- Confiança: chute.

## 0x0803A5CC ObjScript_Op1F (sem nome)
- O que faz: como 0x13 (chamadas 0x0801BCE4/0x0801BD1C), IDs 0x129, 0x12F.
- Confiança: chute.

## 0x0803A6FC ObjScript_Op20 (sem nome)
- O que faz: escreve registradores de I/O `0x04000008` e chama 0x0803EFD8 e 0x08040CD0; mexe em `0x03005000`/`0x03004C30`.
- Confiança: chute.

## 0x0803A760 ObjScript_Op21 (sem nome)
- O que faz: estrutura igual à do 0x08 (mesmos campos `+0x6B..+0xB0`, `+0xF6`, `+0xF8`), 108 instruções.
- Confiança: provável (parente do 0x08; não confirmado).

## 0x0803A840 ObjScript_Op22 (sem nome)
- O que faz: idem ao 0x21 (mesmo corpo, 108 instruções).
- Confiança: provável.

## 0x0803A920 ObjScript_Op23 (sem nome)
- O que faz: 9 instruções; literal 0x8E7 e `ctx`. Chute.
- Confiança: chute.

## 0x0803A93C ObjScript_Op24 (sem nome)
- O que faz: literal 0x329; mexe nos campos `+0x28`..`+0x2D` e `+0xC`, `+1`, `+2`, `+3`, `+6` do slot; chama 0x08040CD0.
- Confiança: chute.

## 0x0803AA00 ObjScript_Op25 (sem nome)
- O que faz: 65 instruções; mexe em `+0x24`, `+0x25`, `+0xF8`.
- Confiança: chute.

## 0x0803AA90 ObjScript_Op26 (sem nome)
- O que faz: 49 instruções; só ctx, sem efeito externo identificado.
- Confiança: chute.

## 0x0803AB00 ObjScript_Op27 (sem nome)
- O que faz: chama 0x08001380 e IDs 0x9E9, 0x9EB, 0x9EF; grava `+1`.
- Confiança: chute.

## 0x0803AB9C ObjScript_Op28 (sem nome)
- O que faz: copia para a VRAM/OAM de sprites (`0x05000280..0x05000360` = paleta OBJ; `0x0202F880..0x0202F960` = buffer), chama 0x0807CCC0 (CpuFastSet).
  Carga de paleta/sprite. Provável.
- Confiança: provável.

## 0x0803AC4C ObjScript_Op2A (sem nome)
- O que faz: chama 0x0807CCC0 (CpuFastSet) com buffer `0x0202FA00`.
- Confiança: chute.

## 0x0803ADE8 ObjScript_Op2B (sem nome)
- O que faz: 7 instruções; chama 0x08040CD0.
- Confiança: chute.

## 0x0803ADFC ObjScript_Op2C (sem nome)
- O que faz: chama 0x0801BCE4, 0x0801BD1C; IDs 0x129, 0x12F; 120 instruções (desenho).
- Confiança: chute.

## 0x0803AF2C ObjScript_Op2D (sem nome)
- O que faz: IDs 0x282, 0x2A5, 0x2A9, 0x2AB; `0x4004`; chama só 0x0803D450 (leitura de bloco); 120 instruções.
- Confiança: chute.

## 0x0803B0C0 ObjScript_Op2E (sem nome)
- O que faz: chama 0x08040B90; `0xC004`; lê `0x083AFACC` e `0x083B6100` (tabelas de ROM).
- Confiança: chute.

## 0x0803B19C ObjScript_Op2F (sem nome)
- O que faz: chama 0x08044B28 e 0x0807CCC0; carga com `0x050003C0` (paleta BG).
- Confiança: chute.

## 0x0803B1EC ObjScript_Op30 (sem nome)
- O que faz: chama 0x08040B90, 0x08040CD0; `0x4004`; 120 instruções.
- Confiança: chute.

## 0x0803B328 ObjScript_Op31 (sem nome)
- O que faz: chama 0x0801BF78; mexe em `ctx+0x2E`.
- Confiança: chute.

## 0x0803B374 ObjScript_Op32 (sem nome)
- O que faz: chama 0x0801BC5C; lê `0x03000590` (bot) com `+0x49`, `+0x50`, `+0x87`, `+0xA7`, `+0xA8`, `+0xDD`.
- Confiança: chute.

## 0x0803B3C8 ObjScript_Op33 (sem nome)
- O que faz: chama 0x080009BC (decoder Malias) e 0x0807CCC0; VRAM 0x06010000 (igual ao 0x17). Carga de gráfico.
- Confiança: provável.

## 0x0803B428 ObjScript_Op34 (sem nome)
- O que faz: chama 0x08040B90; mexe em `+0x23`, `+0x26`, `+0x29`, `+0x2D`; 95 instruções.
- Confiança: chute.

## 0x0803B4F8 ObjScript_Op35 (sem nome)
- O que faz: chama 0x08040B90, 0x08042BC0; lê `0x03001906` (fight_information) e `+0x1C`, `+0x40`, `+0xB`.
- Confiança: chute.

## 0x0803B5DC ObjScript_Op36 (sem nome)
- O que faz: carga de gráficos: IDs 0x26D, 0x26E; VRAM 0x06006400..0x06006800; chama 0x080009BC e 0x0807CCC0; ROM de gráficos 0x08487D90..0x084E4638.
- Confiança: provável (carga de sprites/fundo).

## 0x0803B6D8 ObjScript_Op37 (sem nome)
- O que faz: 15 instruções; lê ID 0x269 e `ctx+0x2A`.
- Confiança: chute.

## 0x0803B700 ObjScript_Op38 (sem nome)
- O que faz: chama 0x0803EF8C e 0x08040CD0; escreve `0x04000008` / `0x0400000A` (BGCNT).
- Confiança: chute.

## 0x0803B754 ObjScript_Op39 (sem nome)
- O que faz: como 0x13 (IDs 0x129, 0x12F), chamadas 0x0801BCE4/0x0801BD1C; 120 instruções.
- Confiança: chute.

## 0x0803B884 ObjScript_Op3A (sem nome)
- O que faz: chama 0x08074DE0; lê `0x03001988`; escreve `ctx+0x40`, `+0x56`.
- Confiança: chute.

## Fora da tabela: opcodes 0x3B a 0x4F
- Apontam todos para 0x08000840 (stub `bx lr`). Sem efeito. Opcodes >= 0x50: a palavra na tabela não é handler (dados de outra estrutura).
