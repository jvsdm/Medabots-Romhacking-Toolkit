# TINPET: opcode 0x3D (Get_Tinpet) e o registro que ele grava

GH-TINPET, 2026-10-18. Leitura de decompile (Ghidra MCP, só leitura) + docs já existentes (DB2.md, HC.md, EV2.md)
+ `pc/extract/extract_save.py` (sentinela 0x78) + `pc/game` (porte). Nada validado no emulador.
"confirmado" = decompile lido nesta sessão; "provável" = inferência; "aberto" = só o emulador resolve.

## Função do opcode (confirmado no decompile)

`EventOpcode3D_GetTinpet` = 0x0804EF5C (nome já no Ghidra; HC.md: continuação do opcode 0x3D).
Máquina de estados de 3 passos. O estado fica num byte do bloco de evento (`gEventBlock + DAT_0804EF78`):

| estado | ação |
|---|---|
| 0 | `FieldMenu_ClearTwoSlotPairs()`, SFX 0x0E (`Snd_SfxRequestWrapper`). Se `Tinpet_IsPoolFull_Wrapper()` == 0: `Tinpet_CreateRecord_Wrapper(param)` e estado = 2. Senão estado = 1. |
| 1 | pool cheio: `Obj_TwoStageInitThenDispatch` com o objeto em `DAT_0804EFC8+0x78` (mensagem de "não cabe"); quando termina, `Event_EndOpcode`. |
| 2 | `Obj_TwoStageInitThenDispatch` com o objeto escolhido por `table[(byte(DAT_0804EFF8)+0x1F)*4 + DAT_0804EFF4]` (mensagem da entrega); quando termina, `Event_EndOpcode`. |

- `param` = byte lido do bloco de evento em `DAT_0804EFAC` (provável: o operando do opcode, byte 1 do evento).
  No porte, `extract_events.py` lê esse byte como `id` (0 = macho, 1 = fêmea); decompile e porte batem.
- Wrappers: `Tinpet_IsPoolFull_Wrapper` 0x08047E30 -> `Tinpet_IsPoolFull` 0x08042218;
  `Tinpet_CreateRecord_Wrapper` 0x08047E3C -> `Tinpet_CreateRecord` 0x08042238 (confirmado).
- Objetos de mensagem (ids dos textos de 1 e 2): **aberto** (DAT_0804EFC8, DAT_0804EFF4/F8 não lidos como literais).

## O registro (confirmado no decompile de `Tinpet_CreateRecord`)

Varre até 9 registros de 8 B (pool a partir de `DAT_08042264`, provável IWRAM 0x030017A0 = time, ver DB2.md) e usa o
primeiro com byte 0 == 0:

- byte 0 = 1 (em uso)
- byte 1 = p & 1 (sexo: 0 macho, 1 fêmea)
- byte 2 = 0x1E (30): medalha. Fora da tabela de medalhas do porte (0..29, `parts.lua` medals). Provável: id de
  "sem medalha" / medalha de Tinpet. Aberto.
- bytes 3..6 = 0x78 + (p & 1): as 4 peças = bot 120 (macho) ou 121 (fêmea).
- byte 7: não escrito pela função (fica o que estava no slot livre). Índice do time: aberto (TM2).

`Tinpet_IsPoolFull` (0x08042218) = 1 se os 9 registros estão com byte 0 != 0.

## Por que o Tinpet nasce sem peças

- `pc/extract/extract_save.py`: "0x78 = sentinela 'sem peca', nao entra" (BOTS = 120). Ou seja, 120/121 nas peças
  do time quer dizer "vazio". `parts.lua` só tem bots 0..119 (480 entradas), então `parts[120*4+k]` não existe.
- O Tinpet entra com as 4 peças vazias; o jogador monta peças pela tela MEDABOTS (BUILD). Isso bate com o pedido de
  "luta quando tiver medalha e peças" (regra do porte, `savegame.isReady`).
- Tela de debug (DB2.md, "Tinpet" em 0x0800C6F8): chama o mesmo `Tinpet_CreateRecord` com o valor da linha 0. Não é
  o caminho da história; só mostra a função.

## Eventos que dão Tinpet (EV2/CAPITULOS, confirmado no cache de eventos do porte)

- ev 269 (mapa 30, compra do kit, cap. 0): op 0x3D macho (id 0). Ver QA5/QA6.
- ev 151 (mapa 17, cap. 2): macho.
- ev 402 (fêmea, sim) / 403 (macho, não), mapa 41, caps 2..8.

## Divergência aberta (não resolvida aqui)

- A savestate do usuário mostra só 1 registro ativo no time (Metabee, byte 0 = 0x03) no início do cap. 1. O Tinpet
  grava byte 0 = 1. Ou o Tinpet do ev 269 não cai no slot 1 do time, ou o registro é apagado depois (fila de peças
  quebradas, DB2/ `Tinpet_ActivateMatchingEntry` FS4). Medir no emulador: time antes e depois do ev 269.
- Quais peças o jogador recebe depois (QA5: o ev 269 também compra 4 peças): não ligado ao Tinpet no decompile.
