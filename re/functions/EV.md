# EV - máquina de eventos do mapa (visão geral, GH-EV, 2026-10-14)

Status: PARCIAL (passos 1-4 feitos; pendências na §7). Fontes: HE1..HE4, HC, HC1 (já lidos), extract_events.py (só leitura),
Ghidra para o laço principal (passo 2). Nomes e endereços da EU (Medabots - Metabee (Europe).gba).

## 1. Estado do evento (bloco E = 0x030057E0)

Tabela de campos com endereço absoluto (confirmado nos HE1/HC1; "provável" onde só o nome sugere).

| off E | endereço | nome | confiança |
|---|---|---|---|
| +0x5DC | 0x03005DBC | tabela de objetos ativos (8 B/entrada; +0x02 u16 id, 12 bits baixos = id do evento; +0x04 byte render_slot) | confirmado |
| +0xC5F/+0xC60 | 0x0300643F/0x03006440 | +0xC5F: flag 'rodar passo da cena também neste quadro' (laço, passo 2); zerados pelo End (0x06) | confirmado |
| +0xC88 | 0x03006468 | gCurrentEventPendingFlag (≠0 = evento rodando) | confirmado |
| +0xC89 | 0x03006469 | gCurrentEventStateByte (0 = fase primária, 1 = continuação) | confirmado |
| +0xC8A | 0x0300646A | cursor do script (u16, BE na leitura de bytes) | confirmado |
| +0xC8E | 0x0300646E | gCurrentEventId (u16) | confirmado |
| +0xC92 | 0x03006472 | opcode atual (índice das duas tabelas) | confirmado |
| +0xC93 | 0x03006473 | subestado do opcode (fases de continuação) | confirmado |
| +0xC94.. | 0x03006474 | gEventScriptArguments (args lidos por Script_ReadBytes) | confirmado |
| +0xCB4 | 0x03006494 | índice do ActiveMapObjectEntry do evento atual (-1 se não tiver objeto) | confirmado (Goto) |
| — | 0x03006D2A / 0x03006D22 / 0x03006D28 | slot, frames e altura de animação/pulo (usados por 0x1A/0x1D) | confirmado (HC1) |
| — | 0x03006D3E, 0x03005348 | zerados pelo End | confirmado |
| — | 0x03006484 (0x10 B), 0x03006D20 (0x10 B) | zerados pelo End | confirmado |
| — | 0x03005815 | =1 no End (0x03002C30+0xBE5) | confirmado |
| — | 0x0300643B | variante de cena (0 no Warp B, 1 no Warp A) | confirmado (HE1) |
| — | 0x030057A0+0x2E / +0x30 | banco e id da mensagem (opcode 01) | confirmado (HC1) |

## 2. Laço principal

- `EventScript_RunCurrent` 0x0804A220: se Pending != 0 e StateByte == 0 -> `Script_DispatchHandler` 0x0804B6F4;
  se StateByte != 0 -> `Event_DispatchContinuation` 0x0804DC0C.
- `Script_DispatchHandler` 0x0804B6F4: lê o byte do opcode (EventScript_ReadNextByte), grava em +0xC92, zera +0xC89
  e +0xC93, chama `0x083D222C[op]` (tabela primária, 0x00..0x88; 0x89+ = zero).
- `Event_DispatchContinuation` 0x0804DC0C: chama `0x083D2630[+0xC92]` (tabela de continuação, 137 entradas, ver HC.md).
- `Event_EndOpcode` 0x08050290: zera +0xC89 e +0xC93 (fim de qualquer opcode diferido).
- `Script_ReadBytes(n)` 0x0804D8CC: lê n bytes do script para 0x03006474.
- Opcode imediato: não toca +0xC89, então o próximo roda no tick seguinte. Diferido: liga +0xC89 = 1 e
  a continuação faz o efeito visível.

## 3. Goto (0x19) e fim (0x06)

- 0x19 Goto_Event 0x0804BD70 (confirmado, HE1): lê 2 bytes (id BE) -> gCurrentEventId; zera o cursor; se o id tiver
  objeto ativo, grava o índice em +0xCB4. NÃO é salto relativo: o script do novo evento começa do início.
- 0x06 End 0x0804B7C8 (confirmado, HE1): zera Pending, StateByte, +0xC93, cursor, +0xC8E, +0xC5F/+0xC60; zera
  0x03006D3E, 0x03005348, 0x03006484 (0x10), 0x03006D20 (0x10); grava 1 em 0x03005815; chama 0x0805196C. Imediato.
- 0x2F multi-jump e 0x30 relative long jump são saltos relativos (ver extract_events.py, BRANCH_OPS).

## 4. Onde ficam os scripts na ROM (confirmado no extrator; ver §7 para a verificação)

- Base dos scripts: EVENT_BASE = 0x765EC0 (tratado como offset de arquivo no extrator).
- Cabeçalho: u16 [0..1] = 0x4000; u16 [2..3] = offset relativo da tabela de bancos. Contagem = (bank_rel - 0x4000)/2.
- Ponteiro do evento N: u16 em (base+0x4000 + 2N) (janela 0x4000); banco = byte em (tabela de bancos + N).
  Offset final = base + banco*0x4000 + (ptr - 0x4000).
- Pessoas do mapa: ponteiro GBA por mapa em 0x483234 (4 B/mapa); registros de 8 B até 0xFF; id = u16 em +2 & 0xFFF.
- Bancos de texto: 16 ponteiros em 0x47DF24.

## 5. Escolha e início do evento (gatilhos) - PENDENTE (passo 3)

### Passo 2 (Ghidra, confirmado no decompile + literal pool da ROM)
- `EventScript_RunCurrent` 0x0804A220 (laço por quadro): (a) contador de quadros em 0x03005314 (+1 por tick; base
  0x03005310 = gOverworldAffineState no mtbe). (b) Pendente (0x03006468) == 0 -> `CallViaR0(0x083D212C[gamestate_subroutine])`
  (0x0300137C); i.e. SEM evento, roda o passo da cena (aqui também é onde se inicia evento - a confirmar, passo 3).
  (c) Pendente != 0: StateByte (0x03006469) == 0 -> `Script_DispatchHandler`; senão `Event_DispatchContinuation`.
  (d) Depois do despacho, se 0x0300643F (E+0xC5F) != 0 -> chama de novo `0x083D212C[0x0300137C]` (passo da cena no mesmo
  tick). (e) Depois: `Map_UpdateVisibleEventObjectSprites`, `Map_TickAllObjectAnimations`, `Map_UpdateBackgroundAnimations`,
  `Map_RecomputeRenderAffineEntry` x32, `Map_UpdateEventObjectDepthAndMarkerState`, cópia de 32 entradas de 0x03005DBC.
- `Script_DispatchHandler` 0x0804B6F4: confirmado: lê byte (`EventScript_ReadNextByte`), grava em 0x03006472, zera 0x03006469
  e 0x03006473, chama `0x083D222C[op]` (literal 0x0804B734).
- `Event_EndOpcode` 0x08050290: confirmado: zera 0x03006473 (E+0xC93) e 0x03006469 (E+0xC89). NÃO zera o pendente.
- Divergência de nome: mtbe.csv 0x0300137C = `gamestate_subroutine` ("passo interno da cena atual"); o HE/HC não tratam isso.

### Passo 3 (escolha e início do evento, Ghidra decompile)
- Tabela de objetos/gatilhos do mapa na RAM: 64 entradas de 8 B (provável `0x03005DBC`, ver §1); campos: byte0 = x (tile),
  byte1 = y (tile), u16 em +2: **kind = bits altos (u16>>12), id = bits baixos (&0xFFF)**. Kinds no código:
  **8 = gatilho de passagem** (walk-over), **4 = gatilho de frente/conversa** (pessoa), 0 e 6 também entram no caminho de frente.
- `Map_CheckWalkOverTrigger` 0x08052400 (kind 8, casa do jogador): grava Pendente=1 (0x03006468), id em
  0x0300646E e índice da entrada em 0x03006494 (+0xCB4). Confirmado no decompile.
- `Map_CheckFacingEventTriggerAhead` 0x08052330 (botão A/frente): calcula a casa à frente pela direção do jogador
  (0 cima, 1 baixo, 2 esquerda, 3 direita; byte de direção em (DAT_080522C0+0xBE0), a confirmar no mtbe) e, se há entrada
  com kind 0/4/6 nessa casa, faz o mesmo (Pendente=1, id em E+0xC8E, zera cursor, índice em E+0xCB4). Se a casa à frente
  não bate, procura pela casa do jogador (laço de 64). Confirmado no decompile; a diferença entre kind 0 e 6 não resolvida.
- Zera do cursor no início: o decompile escreve 0 no byte 0x0300646A e no u16 0x0300646B; o HE1 diz "cursor u16 em
  +0xC8A". **Divergência a checar**: o cursor pode ocupar +0xC8A..+0xC8B (ou +0xC8C, citado pelo usuário). Não resolvido.
- Outros escritores de Pendente: `Map_InitScreenAndTransition` 0x0804A650, `Overworld_InitMapScene` 0x0804A7BC/0x0804A7D6
  (entrada do mapa, evento de abertura), `MainState_RequestModeTransition` 0x080477E8, `Event_Op06_End` 0x0804B7D2 (limpa).
  Ainda a ver: Overworld_InitMapScene e Map_LoadEntitySpawnsForCurrentMap (leitura do ponteiro 0x483234).

### Passo 4 (leitor de script, bancos e gatilho - Ghidra decompile + literal pool)
- `EventScript_ReadNextByte` 0x0804D83C (confirmado): base = ponteiro ROM guardado em RAM 0x03005590 (valor esperado
  0x08765EC0 = EVENT_BASE do extrator; não lido em runtime). Janela 0x4000. Ponteiro do evento = u16 LE em
  base+0x4000+2*id (bias 0xFFFFC000). Banco = byte em base+bank_rel+id+0x4000 (bank_rel = u16 em base+2).
  Byte lido = base + ((ptr-0x4000) + cursor) + banco*0x4000. Id = u16 em 0x0300646E (E+0xC8E); cursor = u16 em
  0x0300646A (E+0xC8A), incrementado a cada byte.
- O cursor do script é u16 em E+0xC8A (HE1 correto). Divergência anterior resolvida: o gatilho de frente zera o
  byte E+0xC89 (StateByte) e o u16 E+0xC8A (cursor), não zera cursor em outro lugar.
- Gatilho de frente (`Map_CheckFacingEventTriggerAhead`) também grava 1 em 0x03005815 (= 0x03002C30+0xBE5, mesmo byte
  que o End escreve com 1): "modo evento" (provável; nome a confirmar no emulador).
- Carga das entradas do mapa: `Map_LoadEntitySpawnsForCurrentMap` (entrada = 0x080513C8; 0x08051416 é o miolo) (EX): lê ponteiro por mapa
  na tabela de ponteiros (literal 0x08483234 com xref confirmado; índice = gCurrentMapId 0x03005FDE, u32[0x08483234 + 4*id] (EX)). Cada
  registro de 8 B é percorrido até 0xFF: +0 x, +1 y, +2 u16 (kind<<12 | id&0xFFF), +4 byte (nibble alto = variante de
  sprite, baixo = direção), +5 grupo (0..31; laço externo de 0x20 grupos), +6 u16 máscara de condição. Só entra se
  bit da máscara bate (deslocamento do literal 0x08051484) e o id NÃO está com flag marcada (EventFlag_IsSetByPackedId):
  evento já feito some. Escreve a entrada na RAM 0x03005DBC (8 B) e, para kinds 0/4/6, marca célula de mapa = 7.
  Kind 2 = ator de cena (slot 0x31+k).
- Escritor de Pendente no início de mapa: `Overworld_InitMapScene` 0x0804A7A0 só ZERA Pendente (0x0804A7BC), StateByte
  e cursor (0x0804A7D6: u16 E+0x8A), depois chama `Map_LoadCurrentMapAndEntities`. Logo o evento de abertura (ex.: 185 no New Game, já
  registrado em pc/research) NÃO sai daqui: sai pelo laço de gatilhos (passagem kind 8 ou pessoa) - a confirmar.
- Gatilho de passagem `Map_CheckWalkOverTrigger` 0x08052400 e de frente `Map_CheckFacingEventTriggerAhead` 0x08052330:
  quem chama (passo do jogador/`Overworld_ProcessDirectionInput` 0x08051D08 e `Overworld_WalkStepByDirection`
  0x08051BB6 pelo xref) - a confirmar.

## 6. Divergências com o extrator (extract_events.py, só leitura)
1. Extrator: id = u16 & 0xFFF (kind = nibble alto). Confere com `Map_LoadEntitySpawns` e os gatilhos (0xFFF). OK.
2. Extrator: 8 bytes/registro, 0xFF termina. Confere. `people_event_ids` lê só o id (+2) e ignora +4 (sprite/direção),
   +5 (grupo) e +6 (máscara de condição) que o jogo usa para filtrar. Efeito no PC: não verificado (checar pc/game).
3. Extrator: lê um ponteiro por mapa (0x483234). O jogo percorre um laço de 0x20 grupos e relê a mesma lista (NÃO há tabela de ponteiros por grupo; EX)
   (literais 0x08051478/0x0805147C, índice por byte: provavelmente o mapa atual, não resolvido). Se a tabela tiver
   mais de um ponteiro por mapa, o extrator perde pessoas. Confirmar no ROM.
4. Extrator: `event_start` usa bank_tbl = ptr_tbl + count*2 com count = (bank_rel-0x4000)/2. Código do jogo: bank =
   base + bank_rel + id (equivalente). Confere.
5. Extrator: 0x06 = fim e 0x19 = goto (sem salto relativo). Confere com HE1.
6. Extrator não trata o gatilho de frente (kind 0/4/6) nem o de passagem (8) como entrada de evento no próprio
   script; só usa ids. Ok para o PC se o PC reproduzir o gatilho (ver pc/game).

## 7. Pendente (não feito nesta rodada)
- Escolha da cena (tabela 0x083D212C, indexada por 0x0300137C): qual subestado chama os gatilhos e a checagem de
  passo. Ghidra: decompile de `Overworld_ProcessDirectionInput` 0x08051D08 (não feito).
- Campo do bit em 0x08051484 (deslocamento da máscara) e 0x03005815 (nome exato): confirmar no emulador.
- Diferença kind 0 vs 6 no gatilho de frente.
