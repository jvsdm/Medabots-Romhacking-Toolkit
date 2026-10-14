# EF - armazenamento de estado por evento (flag e campo de 3 bits)

Status: layout do byte = **confirmado** (literais da ROM + decompile). Escritores = **confirmados** (xrefs).
Persistência no save = **confirmada** para E..E+0xCBF; lacuna nos ids 0..15 = **provável** (não testada em jogo).
GH-EF, 2026-10-14. ROM: `D:/claude-brain/Medabots/Medabots - Metabee (Europe).gba` (só leitura, `python -I`).
Nenhum texto do jogo aqui: só endereços, bytes, ids e nomes.

## 1. Base e layout (corrige HE2 e ST2)

- A base do array **não é E (0x030057E0)**. Todas as funções de flag/campo leem a base de um literal de pool que
  aponta para **0x030057D8** (`gEventFlagArrayBase`). Literais lidos na ROM: 0x08053308, 0x080532F0, 0x08053218,
  0x08053200, 0x08053260, 0x0805327C, 0x08053184, 0x0805319C, 0x080531C0, 0x080531DC = 0x030057D8 (todos).
  HE2 (§0x2F) e ST2 diziam 0x030057E0 + id>>1: **errado**, o certo é 0x030057D8 + id>>1 (= E - 8 + id>>1).
- Índice: `idx = (id & 0xFFF) >> 1`. Um **byte por par de ids** (id 2k e 2k+1 compartilham o byte idx k).
- Tamanho: o limite nominal é 0x800 B (0x030057D8..0x03005FD7, `gEventFlagArrayEnd` 0x03005FD8). Ids vão até 0xFFF,
  então idx até 0x7FF. Eventos da ROM: 2288 (EV2), que usam idx até ~0x477. Não há colisão com a tabela de objetos
  ativos (0x03005DBC, idx 0x5E4..0x7E3) para esses ids; a colisão só existiria se houvesse id >= ~0xBC8.

Layout do byte (k = id>>1):

| bits | id par (id&1 = 0) | id ímpar (id&1 = 1) |
|---|---|---|
| 7 | flag (set/test) | - |
| 6..4 | campo de 3 bits (mask 0x70) | - |
| 3 | - | flag (set/test) |
| 2..0 | - | campo de 3 bits (mask 0x07) |

Confirmado no código:
- `EventFlag_IsSetByPackedId` 0x080531E0: par `>>7`; ímpar `(b & 8) >> 3`.
- `EventField3Bit_GetByPackedId` 0x080532D4: par `(b & 0x70) >> 4`; ímpar `b & 7`. Faixa 0..7 (usada pelo 0x2F).
- `EventFlag_SetIndexedFlagByPackedId` 0x080531A0: par `b = (b & 0x7F) | 0x80`; ímpar `b = (b & 0xF7) | 0x08`.
- `EventFlag_ClearIndexedFlagByPackedId` 0x08053168 (R3, mesmo layout, zera o bit).
- `EventField3Bit_SetByPackedId` 0x08053238: par `b = (b & 0x8F) | (v << 4)`; ímpar `b = (b & 0xF8) | v` (v = arg & 7).
- `EventField3Bit_IncrementByPackedId` 0x08053280: par `b = (b & 0x8F) | ((campo+1) << 4)`; ímpar `b = ((b&7)+1) | (b & 0xF8)`.
  **Transbordo (confirmado no código):** campo 7 + 1 = 8 cai no bit da flag (bit 7 no par, bit 3 no ímpar): a
  flag fica ligada e o campo volta a 0. Efeito no jogo não testado.
- `EventFlag_SetOrClearByPackedId` 0x0805321C: `on == 0` chama clear 0x08053168; senão set 0x080531A0 (R3).

Ou seja: a flag = "evento feito/removido" (lida pelo spawn loader 0x08051452 para esconder entidades; escrita por
Op12/Op13). O campo de 3 bits = **o estado da conversa/NPC** que o 0x2F usa para escolher o ramo.

## 2. Quem grava o campo (opcodes) e quem lê

Escritores do campo (xrefs confirmadas):

| Função | Opcode | Args (bytes após o op) | Ação | Usos no EV2 (variante A) |
|---|---|---|---|---|
| `Event_Op17_SetEventVariable` 0x0804BD08 | 0x17 | 3: id BE (2) + valor (&7) | `EventField3Bit_SetByPackedId` 0x08053238 | 1260 sítios |
| `Event_Op18_IncrementEventVariable` 0x0804BD44 | 0x18 | 2: id BE | `EventField3Bit_IncrementByPackedId` 0x08053280 | 44 |
| `Map_SyncMarkerEventFields` 0x080582B8 | (marcador, não é opcode) | - | escreve flag+campo do marcador | - |

Escritores de flag (para completar o quadro):
- Op12 `Event_Op12_AddEventPermanently` 0x0804BBC8: clear (0x08053168) — 1114 sítios.
- Op13 `Event_Op13_RemoveEventPermanently` 0x0804BBF4: set (0x080531A0) — 2549 sítios.

Leitor do campo: **só** `Event_Op2F_JumpTableByEventField` 0x0804C444 (chama 0x080532D4 com `gCurrentEventId`
0x0300646E). Ou seja: o 0x2F de um evento N lê o campo do **próprio** N; o campo de N é gravado por scripts com
op 0x17/0x18 apontando para N (podem ser outros eventos). Leitor da flag: spawn loader (0x08051452) e marcador.

Resposta à pergunta "quem grava o nibble" (ST2 §5, aberta): são os opcodes **0x17 e 0x18**. Sem outro escritor
de campo no código da ROM (além do marcador).

Parâmetros: o campo é 3 bits (0..7), não nibble. O C# `Set_Value_From_EventFlag_Nibble` (nome) e o PC
`setflag`/`incflag` (HE1 Op17/18) estão de acordo. Conferir no PC se o valor é mascarado com &7 (HE1 já pede).

## 3. Save (SV.md corrigido e confirmado)

- `SaveLoad_SaveGame` 0x0805E010 (comentário no Ghidra: "SV layout"): cópia do bloco E por BIOS CpuSet.
  Literais: 0x0805E114 = **0x030057E0** (origem), 0x0805E118 = **0xFF8** (destino no buffer), 0x660 halfwords
  = **0xCC0 bytes** (E..E+0xCBF). Confirmado.
- Consequência (**provável lacuna**): os 8 bytes **0x030057D8..0x030057DF** (idx 0..7 = ids 0..15) **não entram no
  save**. Eventos 0..15 existem na ROM (ex.: evento 0 tem 0x2F, E2F §1.4). Se o campo de um desses ids muda e o
  jogador salva, o valor volta ao carregar. Para confirmar: savestate, mudar um campo de id < 16 com 0x17 e recarregar.
  Não testado (sem emulador nesta tarefa).
- `SaveLoad_LoadGame` 0x0805DF18: não lido por inteiro (assume-se cópia simétrica; a conferir).
- O campo E+0x7FE (0x03005FDE) = gCurrentMapId, fora do array; o campo 0x03005FDF = capítulo (EX, AI2).

## 4. Confirmados x abertos

Confirmado (lido no código/literais): base 0x030057D8; layout do byte; escritores 0x17/0x18 e marcador; leitor 0x2F;
cópia do save E..E+0xCBF; transbordo do incremento.

Aberto:
1. Save não cobre idx 0..7 (ids 0..15): confirmar em jogo (ver §3).
2. Transbordo do Op18 (campo 7 +1): confirmar se algum script chega a 7 e depende disso.
3. Limite real do array (0x800 nominal vs. ids usados): `gEventFlagArrayEnd` 0x03005FD8 é provável, não visto.
4. Load (0x0805DF18) não lido por inteiro.
5. Ghidra: nenhuma função renomeada nesta tarefa (os nomes já estavam em mtbe.csv). Comentário de layout
   adicionado no decompile de 0x080532D4.

## 5. Para o port (Lua/PC)

Estado mínimo por evento: `flag[id]` (bool) e `field[id]` (0..7), guardados num byte por par no vetor de 0x800 B;
o save do PC pode gravar o vetor inteiro (decisão de porte; a ROM original não grava idx 0..7, ver §3).
O PC (`script.lua`, `"nib"..id`, ST4) deve usar o layout acima, não nibble de evento.
