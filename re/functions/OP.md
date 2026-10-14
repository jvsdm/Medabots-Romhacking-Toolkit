# OP: menu de opções (Medabots Metabee EU)

Status: PARCIAL (2026-10-14, agente medabots): tela de OPTIONS e leitores das 3 opções confirmados; nomes dos efeitos e conflito de 0x03001360 pendentes. Fontes: TL.md (GS 5 do título = OPTIONS, 0x0805C574), SV.md (bits do save +0x1CB8),
A6.md (0x03001360 = flag de UI de batalha). Ghidra só leitura. Símbolos: OP_simbolos.csv. Sem função: OP_lista.txt.

## 1. Tela de OPTIONS (GS 5 do título) = `Menu_ThreeRowCursorInput` 0x0805C574 (confirmado no decompile + literais)

Cursor de 3 linhas em `ctx = *0x03006F74` (+5 = linha 0..2, +2 = contador que volta a 0x14). Entrada: 0x0300058C (bits do teclado).
Literais lidos da ROM original (python -I, só leitura): 0x03001788, 0x03001360, 0x03000B94; tabela de músicas 0x080AF8CA.

| linha | opção (RAM) | valores | ação na tela | confiança |
|---|---|---|---|---|
| 0 | `0x03001788` | 0..2 (cíclico) | ESQ/DIR mudam o valor, SFX 2 | confirmado (código); nome do efeito pendente |
| 1 | `0x03001360` | 0/1 (XOR 1) | ESQ ou DIR inverte, SFX 2 | confirmado (código); nome pendente (ver conflito abaixo) |
| 2 | `0x03000B94` | 0..2 (cíclico) | ESQ/DIR mudam e tocam música `0x080AF8CA[valor]` | confirmado (código); provável "trilha/som" |

- Cima (0x40) / baixo (0x80): mudam a linha (cíclico 0..2), tocam música da linha 2 se cursor==2, reinicia contador.
- A (0x01) ou B (0x02): para a música (`snd_song_request(1)`), SFX 4, cursor = 2, limpa bit 14 de `0x03002C30+0x380`
  (`0xBFFF`), e **GS (0x0300137C) -= 1** (volta ao menu do título, GS 4). Confirmado.
- Entrada da tela: GS 4 do menu (`Menu_TickCursorInputAndConfirm`, TL.md) com cursor 2 -> GS 5 = esta função.

## 2. Conflito a revisar

- `0x03001360` é lido em `Fight_TryUiActionByIndex`/`Fight_TryUiActionDefault`/`Fight_MaybeStartUiAction` como "flag 0 = pode disparar"
  (A6.md) e **também** é uma opção de 0/1 escrita por esta tela e salva em 0x1CB8 b2 (SV.md). Ou é uma flag de UI que o
  menu de opções liga/desliga, ou o nome A6 está errado. Pendente de confirmar lendo os três leitores.

## 3. Leitores no jogo (decompile Ghidra, confirmado)

### 0x03001788 (linha 0 da tela; save b0-1; 0..2)
- `Fight_SetCounterTimerByState` 0x08018BD4 (xref em 0x08018BF2): se ctx+0x89 == 1, grava 0x28 em ctx+0x57; senão lê
  0x03001788: 0 -> 0x28, 1 -> 0x1E, 2 -> 0x14 (grava em ctx+0x57). Ou seja, o valor maior = contador de luta mais curto (mais rápido).
  ctx de luta = 0x03000440 (provável, mesmo +0x89 usado em A6). **Confirmado** (leitura do código); o nome "velocidade da batalha"
  é **provável** (o que o contador controla não foi lido).
- Não há leitor de velocidade de texto nesse byte. Velocidade do texto (ScriptVM) não encontrada entre as 3 opções salvas.

### 0x03001360 (linha 1 da tela; save b2; 0/1)
- Escrito só pela tela (XOR 1) e pelo carregamento/zeragem do save (`SaveLoad_LoadGame` 0x0805DFAA, `Save_InitScreen` 0x0805C39A).
- Lido em `Fight_TryUiActionByIndex` 0x0801AC24, `Fight_TryUiActionDefault` 0x0801AC6C, `Fight_MaybeStartUiAction` 0x0801ABF8:
  o gate é `if (*0x03001360 == 0 || ctx+0x89 == 1)` -> dispara a ação (`StateMachine_DispatchStep` / `Obj_InitSlotAndLoad(1,2,0)`); senão não faz nada.
  **Confirmado** (código). Ou seja, com a opção em 1 a ação da luta fica bloqueada, se o byte for o mesmo.
- Conflito: o nome "gMessageBusyFlag" (A6) e "opção salva" (SV) descrevem o mesmo byte. Decisão pendente: ou é uma opção que
  o jogo usa como gate de UI de batalha (e o jogo trava com 1, o que precisaria de teste), ou o byte é compartilhado por outra
  rotina. **Não confirmado em emulador** (tarefa sem emulador).

### 0x03000B94 (linha 2 da tela; save b3-4; 0..2)
- `Fight_PlayResultCueByState` 0x08018A54 (xref em 0x08018A9C): se ctx+0x89 == 1 toca música 9; senão, se o byte de estado
  `& 0xF0 == 0` toca `0x080AF8CA[valor]`, se `== 0x10` toca 12, senão 13. **Confirmado** (código): o valor escolhe a
  música do "cue" de resultado da luta (de 3 opções: ids 9, 10, 11 pela tabela 0x080AF8CA).
- A própria tela toca `0x080AF8CA[valor]` ao mudar a linha (prévia de música). Tabela 0x080AF8CA: bytes 9, 10, 11, 9 (o 4º não é usado pela opção).

## 4. Save e padrões (SV.md confirmado; ver também)

- Os 3 bytes ficam em 0x1CB8 (b0-1, b2, b3-4). Sem save válido: `Save_InitScreen` 0x0805C2DC zera os três (`*puVar4=0; *puVar5=0; *puVar6=0`):
  padrão 0 para tudo. Com save válido, mantém o que o `site_LoadGame` carregou.
- Não há leitor no menu de campo: nenhum xref para os três bytes fora da tela de OPTIONS, dos leitores de luta e do save.
  Menu de campo não tem tela de opções própria (confirmado pelos xrefs).

## 5. Renomes e itens em aberto

- Função: `Menu_ThreeRowCursorInput` 0x0805C574 renomeada para `Options_ThreeRowMenuStep` (confirmado: é a tela de OPTIONS).
- Dados (pendente, só provável): 0x03001788 = `Opt_BattleTimerSpeed`; 0x03000B94 = `Opt_ResultMusicVariant`; 0x03001360 = `Opt_UiActionGate` (conflito, não renomeado no Ghidra).
- Pendente: confirmar em jogo (replay) o que a linha 1 bloqueia; achar o nome dos rótulos de linha (texto do jogo, não copiar aqui).

## 6. Arquivos
- `OP_simbolos.csv`: nomes acima (funções confirmadas, dados provável/conflito marcados no comentário).
- `OP_lista.txt`: código sem função no Ghidra usado pela tela (ver arquivo).
