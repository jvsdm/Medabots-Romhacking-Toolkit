# Comandos de cena (SC) — Medabots Metabee EU

Sistema que roda uma "cena" de transição (fade, carga de mapa, música, troca de fluxo) como uma
máquina de 5 passos. Os opcodes de evento chamam `Event_RunSceneCommandSequence` em loop até ele devolver
!= 0, e então fecham o opcode com `Event_EndOpcode`.

Confiança geral: **confirmado** no decompile/disasm (Ghidra + ROM original). Onde não está, marcado.

## 1. Entrada e estado

- `Event_ResetSceneCommandStep` 0x08050CC0: zera o byte +0x0B do bloco de estado de cena (`DAT_08050CC8`,
  = IWRAM 0x03006D20). Chamado antes de cada comando (opcodes 0x71/0x72/0x73/0x7A/... no subpasso 0).
  Origem: R3.md (confirmado no código).
- `Event_RunSceneCommandSequence` 0x08050CCC, assinatura `longlong f(u32 cmd, u32 arg)` (r0 = cmd, r1 = arg;
  o Ghidra mostra `void(void)` porque não recuperou os argumentos — **confirmado no disasm**: `add r4,r0` e
  `add r5,r1` no começo).
  - Lê o passo = byte +0x0B do bloco 0x03006D20 (literal 0x08050CEC).
  - Se passo > 4: retorna 0 sem fazer nada.
  - Senão despacha por jump table de 5 entradas em **0x08050CF4** (literal 0x08050CF0).
  - Retorno: 0 = ainda rodando; != 0 = comando terminou (o opcode chama `Event_EndOpcode`).
  - Quem chama o passo: `r4` (cmd) é usado por todos os passos; o passo é incrementado em cada passo pelo código
    do próprio passo (0x08050F2A / 0x08050F38).

### 1.1 Os 5 passos (tabela 0x08050CF4)

| passo | endereço | o que faz (confirmado no disasm) |
|---|---|---|
| 0 | 0x08050D08 | Se cmd != 0x20: se cmd == 0x12 chama política 0x17 (fade); depois `Overworld_ResetAndStartScreenTransitionEffect` 0x08049D90 com modo 0x08 (0xFF se cmd == 8). Cmd 0x20 pula direto para avançar. Avança. |
| 1 | 0x08050D40 | Cmd 0x20: `Overworld_BuildEncounterNpcSnapshot` 0x080505FC + `GameState_StartFlowByCommand(0x20, arg)`, avança. Outros cmds: espera `Overworld_RunScreenTransitionStep` 0x08049E08 devolver 0; ao terminar, se cmd == 0x12 chama política 0x18; depois snapshot 0x080505FC + `GameState_StartFlowByCommand(cmd, arg)`; avança. **Aqui é onde o comando vira fluxo de jogo.** |
| 2 | 0x08050D72 | Limpa OAM e paleta por DMA (`Map_ClearOamShadowViaDma` 0x0804B248, `Overworld_RebuildActiveObjectRenderSlots` 0x080506BC), zera tiles (`set_zeros` 0x08049714), grava 2 (cmd 0x10 ou 0x20) ou 1 em 0x0300549A (flag de cena). Carrega a cena: `Map_LoadCurrentMapAndEntities` 0x080480E4, `Map_ClampCameraToLoadedMap` 0x08052AA4, `Overworld_ResetMapSceneBlendState` 0x08048830. Se cmd == 0x13: `Overworld_BeginUiSceneState(0x93, ...)` 0x080533E4 e espera `Map_ModeDispatch` 0x08050BC8 != 0. Se cmd != 8: `Overworld_StartScreenTransitionEffect` 0x08049D18 (args 0, 32.0f, 8.0f). Depois o **switch por cmd** (0x08050E6C, ver §2) chama a política de música com o código da tabela. Cmd > 0x20 não faz nada. Avança. |
| 3 | 0x08050F38 | Só avança. |
| 4 | 0x08050F40 | Cmd == 8: termina (devolve 1). Senão espera `Overworld_RunScreenTransitionStep` 0x08049E08 terminar, então devolve 1. |

## 2. Tabela cmd -> código de política (switch em 0x08050E6C, 33 entradas, cmd 0..0x20)

Só 12 cmds têm entrada própria; os outros 21 (0x04, 0x06, 0x07, 0x09..0x0F, 0x15..0x1F) vão para o avanço
de passo sem política. O código é passado a `SND_PolicyRequest` 0x0804842C (ver §4).

| cmd | código (r0) | Observação |
|---|---|---|
| 0x00 | 0x10 | política sem case = nada |
| 0x01 | 0x11 | política sem case = nada |
| 0x02 | 0x12 | política sem case = nada |
| 0x03 | 0x14 | política sem case = nada (loja: ver §3) |
| 0x05 | 0x1A | política: troca música (`Sound_RequestSong`) |
| 0x08 | 0x1B | política sem case = nada |
| 0x10 | 0x15 | troca música |
| 0x11 | 0x16 | troca música |
| 0x12 | 0x19 | política por tabela (passo 1 chama antes o código 0x18) |
| 0x13 | 0x13 | troca música pelo escolhedor `FUN_080483F8` |
| 0x14 | 0x0F | troca música |
| 0x20 | 0x0D | troca música (batalha de história) |

## 3. Efeito de cada comando (fluxo de jogo, `GameState_StartFlowByCommand` 0x08047878)

O passo 1 chama `GameState_StartFlowByCommand(cmd, arg)`. Valores lidos no decompile (a tabela de
`MN.md` §2.1 confere; em MN o cmd 0x20 aparece como "0x080479A0", mas esse é só um miolo do corpo da função, não
função própria, porque o Ghidra diz "No function found at 0x080479A0").

| cmd | gMainState | gamestate_menu | outros | o que é (provável) | Opcode que usa |
|---|---|---|---|---|---|
| 0x00 | 2 | 0x12 | 0x03000584 = 0 | flow de menu 0x12 | 0x71 |
| 0x02 | 2 | 0x12 | 0x03000584 = 2 | flow de menu 0x12 | 0x72 |
| 0x03 | 2 | 0x0B | 0x03002420 = **arg** (id da loja) | **loja** | 0x59 |
| 0x05 | 2 | 0x14 | 0x03000588 = 0 | (não nomeado) | 0x76, depois de StartFlow(4) |
| 0x08 | 2 | 0x16 | — | (não nomeado) | 0x73 |
| 0x10 | 3 | 4 | — | (não nomeado; sem flag) | 0x78 |
| 0x11 | 3 | 5 | — | (não nomeado) | 0x7A |
| 0x12 | 3 | 3 | sub = 0 | (não nomeado) | 0x79 |
| 0x13 | 3 | 6 | — | (não nomeado) | 0x70 |
| 0x20 | 0 | 6 | sub = 0; **0x03005E0D = 1** | **batalha de história** (encontro por evento) | 0x33 |
| 0x21 | 0 | 6 | sub = 0; 0x03005E0D = 0 | encontro aleatório (não é cena; não passa pelo despachante) | `Overworld_WalkStepByDirection` |
| (flow 4, não é cmd de cena) | 2 | 0x15 | — | link Medabot (nome do Toolkit "Start_Medabot_Link", provável) | 0x76 (chamada direta de `GameState_StartFlowByCommand(4, 0)`) |

Cmd 0x20: a parte do fluxo que o Ghidra mostra no caso 0x20 é o miolo 0x080479A0 (escreve 1 em
0x03005E0D). Cmd 0x21 é o mesmo caso com 0.

Cmd 0x12 no passo 1: chama política 0x18 antes do snapshot (ver §1.1).

## 4. Política de música `SND_PolicyRequest` 0x0804842C

Nome atual no `mtbe.csv`: `SND_PolicyRequest`, comentário diz "políticas de modo de cena" com byte em
`event_flag_list[0xC30]`. **O corpo confirmado é de música**: cada case grava o id da música em
+0xC30 (base por literal) e chama `Sound_RequestSong` ou `Sound_RequestFade(0x1E)`. Casos:
0x00/0x0A/0x0C (só troca se a música atual difere), 0x01 (música = arg), 0x02 (zera e para), 0x03 (músicas com
bit 0x80), 0x04, 0x05..0x09/0x13/0x1C (escolhedor FUN_080483F8), 0x0B/0x0D/0x0E/0x0F/0x15/0x16/0x1A (troca),
0x17 (fade 0x1E), 0x18/0x19 (lê tabela de 4 bytes). Códigos 0x10, 0x11, 0x12, 0x14, 0x1B não têm case.

Ver também: `re/functions/HE3.md`: os opcodes de música 0x46 Stop_Music (política 2) e 0x49 Play_Map_Music (política 4) chamam esta mesma função direto, sem passar pela cena.

## 5. Como o evento espera o fim

- Opcodes de cena sempre fazem: passo 0 = `Event_ResetSceneCommandStep`; passo 1 = `Event_RunSceneCommandSequence(cmd, arg)`
  até retornar != 0; depois `Event_EndOpcode` 0x08050290.
- Cenas que esperam mais: 0x70 (cmd 0x13) e o passo 2 do cmd 0x13 espera `Map_ModeDispatch`.
- O resultado do último comando de cena fica em 0x03005349 (0x03005310+0x39). 0x4F (`Jump_If_Previous_Scene_Command_Failed`)
  salta se vale 2 (HE3.md). 0x7A grava 1 ali **só** se `0x0804B738` != 0, e essa função é um stub que devolve 0
  (confirmado em `mtbe.csv`: "Funcao vazia: retorna 0"). Logo, neste build, esse flag nunca é gravado por 0x7A.
  Ainda aberto: quem grava 2 (falhou).

## 6. Opcodes de evento que usam comandos de cena

| opcode | continuação | comando(s) | origem |
|---|---|---|---|
| 0x33 StartBattle | 0x0804E8C4 | 0x20 (passo 1 = `Event_RunSceneCommandSequence(0x20, 0)`) | RW.md, MN.md |
| 0x59 Open_Shop | 0x0804F67C | 0x03 com id da loja (subpasso 1) | HC4.md, HE3.md |
| 0x70 | 0x0804FA1C | 0x13 | HC.md, HE4.md |
| 0x71 | 0x0804FA5C | 0x00 | HC5.md |
| 0x72 | 0x0804FA9C | 0x02 | HC5.md |
| 0x73 | 0x0804FADC | 0x08 | HC5.md |
| 0x76 | 0x0804FB1C | `GameState_StartFlowByCommand(4)` e depois cmd 0x05 (só se 0x03006D20+0xC == 7) | HC5.md, HE4.md |
| 0x78 | 0x0804FB8C | 0x10 | HE4.md (0x78: "3 conjuntos de peças"; a espera usa 0x0804B738 = stub) |
| 0x79 | 0x0804FBDC | 0x12 | HE4.md |
| 0x7A | 0x0804FC1C | 0x11 + flag 0x03005349 (ver §5) | HC5.md |
| 0x4F | 0x0804CB94 | lê 0x03005349 (não roda cena) | HE3.md |

## 7. Pendente

- Nome dos menus de `GameState_StartFlowByCommand` 0x12, 0x14, 0x15, 0x16, 0x10, 0x11, 0x13 (ver `MN.md`).
- Quem grava 0x03005349 = 2 (falha) e o significado do flag 0x0300549A.
- Conflito: o flag do cmd 0x10 (0x0804FB8C) "espera 0x0804B738" pode travar se a leitura estiver certa; checar no emulador.
- Os argumentos de `Event_RunSceneCommandSequence` (cmd, arg) não aparecem no decompile do Ghidra: ficou só no disasm.
- Nomes Ghidra não aplicados (sem save): ver `SC_simbolos.csv` e `SC_lista.txt`.
