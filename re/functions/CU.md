# CU: atores e movimento em cutscenes/eventos (Medabots Metabee EU)

Status: EM ANDAMENTO (GH-CU, 2026-10-14). Método: Ghidra (decompile/xrefs) + literais da ROM original lidos com `python -I` (só leitura). Sem emulador, sem commit.
Símbolos: `CU_simbolos.csv` (`# address,name,kind,comment`).
Base de leitura: HE1.md, HE2.md, HC2.md, HC5.md, OW.md, OW2.md.

## 0. Divergências dos docs (estado após GH-CU)

- OW.md §1/§5: +0x10 é Y e +0x0C é X (ver §3c). Confirmado por Map_InitObjectSprite, Map_SpawnTrackedObjectAtTile e Map_TickObjectStep. OW.md está invertido nesse ponto.
- OW.md §5: a pose com SFX 0x57 é 0x71/0x78/0x82-0x84 (0x81 não entra). Confirmado.
- OW2 passo 6 (posição do jogador em px): para OBJETOS a posição é word px<<8 (fração fixa). Ambos coerentes com a leitura de Map_TickObjectStep.
- HE2 (0x28) e HE1 (0x1D) usam +0x10 como Y: confirmado.
- HE2 citava 0x08050710 como Map_SetTrackedObjectSlotByIndex: o nome correto é Map_FindFreeTrackedObjectSlot. O 0x0804DBD0 é Event_SetTrackedActorSlot.
- HC5 (0x7F): fontes e destinos conferidos (ver §3e). Fonte de X/Y = E+0xC6E/E+0xC70 gravados nos offsets +0x0C/+0x10 do slot 0x30.
- HC5 (0x7E) e HE2 (0x29): modo do RunAll confirmado (flag = índice de velocidade; mode 1 = Map_StartObjectStep, mode 0 = Obj_StartStepNoAnim).
- HC2 (0x29): o fim do laço é no PRIMEIRO ator que termina (ou no primeiro ator com halfword 0). Confirmado.
- OW.md §5 e OW2: velocidade delta = T[+0x2A]*T[0x03006D3A] com a mesma tabela. Confirmado.

## 1. Slot de objeto (gObjectSlots 0x03002C30, 0x40 B, slot k = 0x03002C30 + k*0x40)

Escrita (E) / leitura (L) por campo. "Ghidra" = visto no decompile desta rodada; "HE/HC" = nota dos docs, não relida.

| Off | Campo | Quem escreve (E) | Quem lê (L) | Confiança |
|---|---|---|---|---|
| +0x00 | halfword: bit 0x8000 = ativo (batalha); o pisca troca por 0 e restaura | Map_TickObjectFlicker (E, Ghidra) | FH (HE/HC) | confirmado (pisca); bit 0x8000 provável no mapa |
| +0x04 | X usado pelo sistema de animação de batalha (FH); NO MAPA X está em +0x0C | FH (HE) | FH | provável; não é campo de passo |
| +0x06 | halfword: frame base = 12*grupo + 3*facing | Map_SetObjectRenderSlotFacingVariant (E, Ghidra) | render/OAM (provável) | confirmado (escrita) |
| +0x0B | byte de estado do arco: 0xFF quando não pousou | Map_ObjectArcStep, Map_ResetObjectField0B (E, Ghidra) | ? | provável |
| +0x0C | word: X em px<<8 (byte baixo = fração, nunca muda no passo) | Map_InitObjectSprite (E: tileX*16+u16[E+0xC48]); Map_TickObjectStep, +0x20 = 2/3 (E, Ghidra); 0x7F só no slot 0x30 | Map_TickObjectStep (L) | confirmado |
| +0x10 | word: Y em px<<8 (fração fixa) | Map_InitObjectSprite (E); Map_SpawnTrackedObjectAtTile (+5 se packed&0x40); Map_TickObjectStep, +0x20 = 0/1; Map_ObjectArcStep (base - arco); 0x7F só no slot 0x30 | Map_ObjectArcStep (L); Event_Op28 Hop (L, HE2); sombra (L, Y-5) | confirmado (+0x10 = Y) |
| +0x20 | byte: FACING (0 N, 1 S, 2 W, 3 E) | Map_SetObjectRenderSlotFacingVariant (E); Map_InitObjectSprite (sprite&3); Obj_StartStepNoAnim (cmd>>4) | Map_TickObjectStep (L: direção); Overworld_GetObjectAnimationIndexForFacing (L); Map_ObjectAnimStep (L) | confirmado |
| +0x21 | byte: cópia do facing | Map_SetObjectRenderSlotFacingVariant (E) | ? | confirmado (escrita); uso não achado |
| +0x26 | byte: frames do pisca de todos (0x41) | Event_Op41 (HE2) | cont. 0x0804F0A4 (não lida) | confirmado (escrita) |
| +0x27 | byte: contador de TILES restantes do passo (cmd & 0xF) | Map_StartObjectStep, Obj_StartStepNoAnim (E); Map_TickObjectStep decrementa | Map_TickObjectStep (L) | confirmado |
| +0x28 | byte: TIMER do passo (16/delta px) E contador do arco (0..0x10) E timer do pisca de todos (=8 em 0x41) | Map_StartObjectStep, Obj_StartStepNoAnim, Map_TickObjectStep (rearma); Map_ObjectArcStep (+1); Event_Op28 (zera); Event_Op41 (=8) | Map_TickObjectStep (0 = parado); Map_ObjectArcStep (<0x10) | confirmado (campo sobrecarregado) |
| +0x29 | byte: 8 no 0x41 (reload do pisca de todos) | Event_Op41 (HE2) | cont. 0x0804F0A4 | provável |
| +0x2A | byte: ÍNDICE DE VELOCIDADE (3º arg de Map_StartObjectStep); T[+0x2A] na tabela 0x083D2A40 | Map_StartObjectStep, Obj_StartStepNoAnim (E) | Map_GetScaledStepDelta (L) | confirmado |
| +0x2B | byte: GRUPO de animação (= sprite >> 4) | Map_InitObjectSprite (E) | Overworld_GetObjectAnimationIndexForFacing (anim = facing + 4*grupo); Map_SetObjectRenderSlotFacingVariant (+0x06); FUN_08048684 (pose) | confirmado |
| +0x2C | byte: 0 no spawn; no arco recebe tab>>4 por quadro; 0 no fim | Map_InitObjectSprite, Map_ObjectArcStep (E) | ? | provável |
| +0x2D | byte: flag de pisca (0 = visível, 1 = oculto) | Map_TickObjectFlicker (E) | render (provável) | confirmado |
| +0x30 | halfword: POSE (FUN_08048684 do grupo; cadeia não lida) | Map_InitObjectSprite (E) | Map_TickObjectStep: SFX 0x57 se pose 0x71/0x78/0x82-0x84 | confirmado (leitura); origem da pose provável |
| +0x32 | halfword: valor visível salvo de +0x00 (pisca) | ? (pendente) | Map_TickObjectFlicker (L) | provável |

Tabelas de atores (fora do slot):
- `0x03006484` (E+0xCA4, 16 B): `byte[id] = slot+1` (0 = vago). Escrita: Event_SetTrackedActorSlot 0x0804DBD0 (Ghidra); zerada no opcode 0x06 End (HE1). Leitura: Event_GetTrackedActorSlot 0x0804DBF0 (-1 = vago).
- `0x030055A0`, índice `render*4` (render = slot+0x4F): halfword. 0xFFFF = ativo (Map_InitObjectSprite e 0x7F); 0 = inativo (não entra no RunAll).
- Registro do ator = slot 0x4F+id; sombra = slot 0x79+id (Map_InitObjectSprite). Slot 0x30 = registro de trabalho do 0x7F e companheiro (OW2); slot 0x5A = cópia da sombra no 0x7F.
- Bloco de argumentos/estado de 1 opcode = `0x03006D20` (= DAT_08050848 do pisca, DAT_080508C8 da animação, DAT_080509B0 do arco). [10] = render; [0..3] conforme o passo (ver §3b).

## 2. Velocidade e passo

- `Map_GetScaledStepDelta` 0x08051940: `delta = T[+0x2A] * T[0x03006D3A]`, mesma tabela `T = 0x083D2A40` (OW2). T = 01 02 04 00 01 02 00 04 05 06 00 08 09 0A 00 00 | 00 00 00 00 02 01 00 08 0A 09 00 04 06 05 00 00.
- Com +0x2A = 0 (caminhar): delta = 1 px/quadro, 16 quadros por tile. +0x2A = 1: T[1] = 2 px/quadro (dobro). +0x2A = 2: 4 px/quadro.
- Tempo de passo: `+0x28 = 16 / delta` quadros por passo de 1 tile (16 px), via `aux_div_s32`.
- Animação do passo: `Map_TickObjectAnimationTimes` 0x08050168 chama `Map_TickObjectAnimation` `delta` vezes por quadro (animação acompanha a velocidade).
- Argumento de movimento do evento (0x10 / 0x23 / 0x29 / 0x6C): byte = (direção << 4) | tiles. Direção 0 N, 1 S, 2 W, 3 E.
- Correção do passo do objeto frente a OW.md §5: (a) +0x10 = Y e +0x0C = X (§3c); (b) a fração (byte baixo) é fixa; o passo é inteiro; (c) a pose de SFX 0x81 NÃO entra (0x82-0x84 entram).

## 3. Funções (decompile/xrefs)

Ver 3a..3f (passo, arco, pisca, animação, spawn, cópia 0x7F, sprite). Renomeado no Ghidra: 0x08050124 -> Obj_StartStepNoAnim.

## 3a. Passo de objeto (lido no Ghidra, 2026-10-14)

- `Map_StartObjectStep` 0x080500BC(slot, byte cmd, byte speedIdx) [Ghidra: sem renome ainda]:
  - `+0x27 = cmd & 0xF` = QUANTIDADE de tiles do passo (contador; 16 px cada). ESCRITOR (+0x27).
  - `Map_SetObjectRenderSlotFacingVariant(slot, cmd >> 4)` = DIREÇÃO (nibble alto). Mesmo valor vai para `Overworld_GetObjectAnimationIndexForFacing(slot, cmd>>4)` -> `Map_SetObjectAnimationVariant(slot, anim)`.
  - `+0x2A = speedIdx` (3º argumento). ESCRITOR de +0x2A. Não é "flag": é o índice do objeto na tabela de velocidade (0 = andar, 1 = correr no uso de 0x29/0x2A do evento; ver HC2).
  - `+0x28 = 16 / Map_GetScaledStepDelta(slot)` (quadros por passo de 1 px, com `aux_div_s32`). ESCRITOR de +0x28.
  - Formato do argumento de movimento (evento 0x10/0x23/0x29...): byte = (direção << 4) | distância.
- `Map_TickObjectStep` 0x0805018C(slot, byte animFlag) [retorna 1 no fim do passo]:
  - Se `+0x28 == 0` retorna 0 (objeto parado).
  - Move por `delta = Map_GetScaledStepDelta(slot)` conforme `+0x20` (facing): 0 (N) -> Y- (`+0x10`), 1 (S) -> Y+ (`+0x10`), 2 (W) -> X- (`+0x0C`), 3 (E) -> X+ (`+0x0C`). Escrita: `word = (word & 0xFF) | ((word>>8) + delta) << 8`: o byte baixo (fração) nunca muda; o passo é inteiro. Logo Y = word(+0x10) >> 8 e X = word(+0x0C) >> 8 (pixel). Ver §3c.
  - Se `animFlag != 0` chama `Map_TickObjectAnimationTimes` 0x08050168.
  - Decrementa `+0x28`. Se chegou a 0: toca SFX 0x57 (`Snd_SfxRequestWrapper`) se a pose em `+0x30` (halfword) for 0x71, 0x78, 0x82, 0x83 ou 0x84 (CORRIGE OW.md: 0x81 NÃO entra); depois `+0x28 = 16/delta` e decrementa `+0x27`. Se `+0x27` chegou a 0, retorna 1.
  - OBSERVAÇÃO (código, a confirmar com replay): ao retornar 1, `+0x28` fica armado (16/delta) e `+0x27 = 0`. Uma nova chamada de Tick volta a mover o objeto; quem para é o fim do opcode (não chamar mais).
- Base de endereço: os literais DAT_08050120/DAT_080501BC/DAT_08050248/DAT_0805028C lidos na ROM = 0x03002C30 (§3d).

## 3b. Sequência de vários atores, arco, pisca e animação (Ghidra, lido 2026-10-14)

- `Event_RunAllTrackedActorsStepSequence` 0x0804E5F0(byte flagSpeed, byte mode). Estado = sub-fase em E+0xC93 (0x030057E0+0xC93).
  - Fase 0: para i = 0..3, `slot = Event_GetTrackedActorSlot(i)` (0x0804DBF0, -1 = vago). Se slot != -1 e a halfword `0x030055A0[(slot+0x4F)*4]` != 0: se mode == 1 chama `Map_StartObjectStep(slot+0x4F, cmd, flagSpeed)`; senão `FUN_08050124(slot+0x4F, cmd, flagSpeed)` (variante, ver pendente). `cmd` = `*(E+0xC94)` = o MESMO byte para os 4 atores (o ponteiro não avança). Ao fim incrementa E+0xC93 (fase 1).
  - Fase 1: para i = 0..3: slot vago -> conta `vagos`; halfword == 0 -> soma 1 em `soma`; senão `soma += Map_TickObjectStep(slot+0x4F, mode)`. Se vagos == 4 -> Event_EndOpcode; se soma != 0 -> Event_EndOpcode.
  - CONSEQUÊNCIA (código): o opcode termina no PRIMEIRO ator que acaba OU no primeiro ator tracked com halfword 0. Confirma HC2; o "todos juntos" é só enquanto todos andam.
  - `flagSpeed` (3º arg) vai para +0x2A (speed index) de cada ator. Para 0x29 e 0x2A o flag é 0 e 1 (HC2): 0x29 = índice 0, 0x2A = índice 1 (provável: caminhar x correr; ver seção 2).
- `Map_ObjectArcStep` 0x08050918 (salto/arco; bloco de estado DAT_080509B0: [10] = render, [8] u16 = Y base do solo):
  - Enquanto `obj+0x28 < 0x10`: `Y = base - (tab[obj+0x28][0] >> 4)` (tab = DAT_080509B4, 16 linhas de 0x10 B, valor = altura em 1/16 px; arredonda negativo +15) escrito em `word(obj+0x10)`; `obj+0x2C = tab2 >> 4` (mesma linha; significado a confirmar); `obj+0x28 += 1`; se `Y_atual < base-0x10` (linha de checagem) chama `Map_ResetObjectField0B(render)`, senão `obj+0x0B = 0xFF`. Retorna 0.
  - Ao chegar a 0x10: `Map_ResetObjectField0B(render)`, `word(obj+0x10) = base << 8 | byte baixo`, `obj+0x2C = 0`, retorna 1.
  - Consequência: `obj+0x10` é Y (não X); ver correção abaixo.
- `Map_TickObjectFlicker` 0x0805082C(render) (bloco global DAT_08050848; fica no argumento de 0x26/0x1E, provavelmente 0x03006D20, a confirmar pelo literal):
  - bloco[0]==0 -> restaura: `obj+0x2D = 0`, `halfword(obj+0x00) = halfword(obj+0x32)`, retorna 1.
  - senão: contador bloco[2] (quando 0, bloco[1]++ e recarrega com bloco[3]); fase par -> `obj+0x2D = 0`, `obj+0x00 = obj+0x32` (visível); fase ímpar -> `obj+0x2D = 1`, `obj+0x00 = 0` (oculto). Decrementa bloco[0].
  - Ou seja: o pisca troca o halfword `+0x00` (com bit 0x8000 = ativo) por 0 e de volta; `+0x32` guarda o valor visível.
- `Map_ObjectAnimStep` 0x080508B4 (bloco DAT_080508C8: [0] estado, [10] = render, [2] = quadros restantes):
  - Estado 0: `anim = Overworld_GetObjectAnimationIndexForFacing(render, obj+0x20)`; `Map_SetObjectAnimationVariant(render, anim)`; estado -> 1; retorna 0.
  - Estado 1: se [2]==0 retorna 1; senão `Map_TickObjectAnimation(render)` e `[2]--`.
  - Confirma: a direção de animação é `obj+0x20` (facing).

## 3c. CORREÇÃO de eixo (leitura conjunta dos quatro itens acima)

- `+0x20` = facing do objeto (0 N, 1 S, 2 W, 3 E, o mesmo do jogador em OW2). `Map_TickObjectStep` com `+0x20`:
  - 0 (N): `word(+0x10) -= delta`; 1 (S): `word(+0x10) += delta`  -> **+0x10 = Y** (word = Y<<8 | fração);
  - 2 (W): `word(+0x0C) -= delta`; 3 (E): `word(+0x0C) += delta`  -> **+0x0C = X**.
- Portanto OW.md §1/§5 está INVERTIDO (ali +0x10 = X, +0x0C = Y); HE1 (0x1D) e HE2 (0x28) estão corretos ao usar +0x10 como Y. Confirmar com o spawn (0x08050358) na seção 3d.
- Confirmação do passo do jogador (OW2 passo 3): 0 e 1 movem o eixo Y, 2 e 3 o X. Consistente.

## 3d. Literais confirmados (ROM EU, leitura por python -I; palavra no endereço do pool)

- Base de objetos = `0x03002C30` (gObjectSlots) em todas as funções de passo/arco/pisca/animação acima.
- Bloco de ator/argumento = `0x03006D20`: é o bloco do pisca (`Map_TickObjectFlicker`, DAT_08050848) e do estado de animação (`Map_ObjectAnimStep`, DAT_080508C8) e do arco (`Map_ObjectArcStep`, DAT_080509B0). Mesmo bloco de HE1/HE2 (0x03006D20[10] = render).
- Estado do evento E = `0x030057E0`; sub-fase = `E+0xC93`; comando/argumento = `E+0xC94`.
- Tabela de atores rastreados = `0x030055A0` (halfword por render, passo 4): `0x030055A0 + (render)*4`.
- Tabela de velocidade = `0x083D2A40` (OW2). Tabela de altura do arco = `0x083D6A2C` (16 linhas de 0x10 B; ver §4).
- `0x03002C30 + k*0x40` = slot k. Por isso: `0x03003FF0` = slot 0x4F (0x4F = 79), `0x03004A70` = slot 0x79 (121), `0x03003830` = slot 0x30 (48), `0x030042B0` = slot 0x5A (90).

## 3e. Spawn, variante sem animação e cópia do 0x7F (Ghidra, lido)

- `Map_SpawnTrackedObjectAtTile` 0x08050358(packed, sprite, x, y): `id = packed & 0xF` (ator rastreado 0..15); se `packed & 0xF0 != 0` e o tile (x,y) é warp, toca SFX da tabela do warp (+3); `slot = Map_FindFreeTrackedObjectSlot()`; `Event_SetTrackedActorSlot(id, slot)`; `Map_InitObjectSprite(slot, sprite, x, y)`; se `packed & 0x40`: `word(obj+0x10) += 5 << 8` (desloca Y em +5 px). CONFIRMA: **+0x10 = Y**.
  - Quem põe a halfword de `0x030055A0[(render)*4]` para ativo não está neste corpo (ver pendente).
- `FUN_08050124` 0x08050124(render, cmd, speedIdx): mesma escrita de `Map_StartObjectStep` SEM mexer na animação/sprite: `+0x27 = cmd & 0xF`; `+0x20 = cmd >> 4` (facing gravado direto); `+0x2A = speedIdx`; `+0x28 = 16/delta`. Nome sugerido: `Obj_StartStepNoAnim` (chute do Ghidra: FN2). Usada pela fase 1 de 0x29/0x2A quando mode != 1 (chamada com mode 0 no 0x7E).
- `Event_Op7F_Cont_Copy_Tracked_Actor_Records` 0x0804FC7C (sem espera; corpo 1 rodada):
  1. `u16 E+0xC6E -> word(0x0300383C) bytes 1..2` = slot 0x30 +0x0C (X). Byte 0 preservado.
  2. `u16 E+0xC70 -> word(0x03003840) bytes 1..2` = slot 0x30 +0x10 (Y).
  3. `mem_copy(slot 0x30, slot 0x4F+arg, 0x40)` — registro do ator rastreado -> slot 0x30 (que o companheiro usa, OW2).
  4. `halfword 0x030055A0[(0x4F+arg)*4] = 0xFFFF` (ator passa a "ativo" para 0x29/0x2A/0x7E).
  5. `mem_copy(slot 0x5A, slot 0x79+arg, 0x40)` — segundo registro -> slot 0x5A.
  - arg = byte E+0xC94. Termina com Event_EndOpcode.
  - Função dos slots 0x30/0x5A (cópia de trabalho do ator?): chute; o slot 0x30 é o companheiro segundo OW2.

## 3f. Criação e variante de sprite (Ghidra, lido 2026-10-14)

- `Map_InitObjectSprite` 0x080503FC(actorId, sprite, tileX, tileY): `render = actorId + 0x4F`; `Map_ResetObjectRenderSlot(render)`; `Map_InitializeObjectRenderSlot(render, 0xC804, FUN_080502C8(sprite), sprite>>4, 1)`; então:
  - `+0x2B = sprite >> 4` (GRUPO de animação; anim = facing + 4*grupo, ver §3g);
  - `+0x20 = sprite & 3` (facing inicial);
  - `word(+0x0C) = (tileX*16 + u16[E+0xC48]) << 8` (X); `word(+0x10) = (tileY*16 + u16[E+0xC4A]) << 8` (Y). Offsets `E+0xC48/0xC4A` = origem do mapa (literais 0x0805053C/40);
  - `halfword 0x030055A0[render*4] = 0xFFFF` (ator ativo; literal 0x08050548);
  - `+0x2C = 0`; `+0x30 (halfword) = FUN_08048684(+0x2B)` (pose do grupo; cadeia não lida);
  - `Map_ResetObjectField0B(render)`;
  - se `table(0x083D2D0D)[pose] != 0`: cria SOMBRA no slot `0x79+actorId` (`Map_InitializeObjectRenderSlot(0x79+id, 0xCC0C, 0x90, 0xE, 1)`), mesmo X, Y−5.
- `Map_SetObjectRenderSlotFacingVariant` 0x08052D90(render, facing): `+0x20 = facing`; `+0x21 = facing` (cópia); `halfword(+0x06) = 12*(+0x2B) + 3*facing` (0..3). Ou seja, cada grupo tem 4 direções x 3 quadros (12 frames).
- `Overworld_GetObjectAnimationIndexForFacing` 0x08050300(render, facing) = `facing + 4*(+0x2B)` (anim = índice de 4 direções dentro do grupo).
- `Event_SetTrackedActorSlot` (0x0804DBD0, ver Ghidra): `byte[0x030057E0+0xCA4 + id] = slot + 1` (tabela de 16 atores; 0 = vago). `Event_GetTrackedActorSlot` lê o inverso (-1 = vago). Usada por spawn (0x21/0x1F/0x2B) e por RunAll.
## 4. Opcodes de ator (primário em HE1/HE2; continuação na tabela 0x083D2630)

| Op | Primário (lido em HE1/HE2) | Continuação e movimento | Estado |
|---|---|---|---|
| 0x1F | 5 B: spawn + `E+0xC94` a `E+0xC98` | 0x0804E480 -> Event_RunInitiateMovingActorSequence 0x0804E498: spawn, `Map_StartObjectStep(render, move, 0)`, Tick até 1 | confirmado (HC2) |
| 0x20 | igual ao 0x1F | 0x0804E48C: mesma chamada | confirmado |
| 0x21 | 4 B: `Map_SpawnTrackedObjectAtTile` | sem continuação (só spawn) | confirmado |
| 0x22 | 2 B: `Map_SetObjectParamAndFrame(slot&0xF, dir)` | imediato | confirmado |
| 0x23 | 2 B (slot, move) | 0x0804E51C -> Event_RunActorStepSequence 0x0804E534: `Map_StartObjectStep(render, move, 0)`, Tick até 1 | confirmado (HC2) |
| 0x24 | igual ao 0x23 | 0x0804E528: mesma chamada | confirmado |
| 0x25 | 1 B: `Map_RemoveTrackedObjectByPackedId` 0x08050554 | imediato | confirmado |
| 0x26 | 2 B (slot, frames): preenche 0x03006D20 | 0x0804E58C: pisca (`Map_TickObjectFlicker`) | confirmado |
| 0x27 | 3 B (slot, variante, frames): `Map_SetObjectRenderSlotFacingVariant`, [2] = frames | 0x0804E5A8: `Map_ObjectAnimStep` 0x080508B4 até 1 | confirmado |
| 0x28 | 1 B (slot): Y em 0x03006D20+8, zera +0x28, SFX 0x70 | 0x0804E5BC: `Map_ObjectArcStep` 16 quadros, depois Event_EndOpcode | confirmado |
| 0x29 | 1 B (move) | 0x0804E5D0 -> RunAll(flag 0, mode 1): `Map_StartObjectStep(render, move, 0)` = andar | confirmado (HC2) |
| 0x2A | 1 B (move) | 0x0804E5E0 -> RunAll(flag 1, mode 1): mesmo com +0x2A = 1 (dobro de velocidade) | confirmado (flag); "correr" provável |
| 0x2B | 5 B: spawn (como 0x6C) | 0x0804E700 não lida | pendente |
| 0x2C | 1 B: liga/desliga bit 4 de `0x0300643D` (prioridade) | imediato | provável |
| 0x2D | 2 B (render, pendente) | 0x0804E784 não lida (nome do mtbe: RotateActor) | pendente |
| 0x35 | 1 B (variante): `Map_SetObjectRenderSlotFacingVariant(render, v)` em todo slot ativo 0..3 | imediato | confirmado |
| 0x41 | 1 B (frames): escreve +0x26..+0x29 em todo slot ativo 0..3 | 0x0804F0A4 não lida | confirmado (escrita) |
| 0x66 | setup grava slot em 0x03006D2A | 0x0804F83C: `Map_TickTrackedObjectTransition` 0x080509EC (nome Ghidra: Map_ObjectMoveStateStep) | confirmado (HC5) |
| 0x67 | 2 B consumidos | 0x0804F850: só termina (1 quadro) | confirmado |
| 0x6C | 5 B (id, sprite, x, y, move) | 0x0804F864: `FUN_08050358` spawn, `080500BC(obj, move, 1)` (speed 1), Tick até 1 | confirmado (HC5); speed 1 = rápido (provável) |
| 0x6E | spawn + passo (HE4) | 0x0804F940 não lida neste lote | pendente |
| 0x7E | 1 B (move) | 0x0804FC6C: `RunAll(0, 0)`: mode 0 = `Obj_StartStepNoAnim` (sem trocar sprite) e Tick sem animação; flag 0 | confirmado (HC5) |
| 0x7F | 1 B (arg = actor id) | 0x0804FC7C: cópia de registro (seção 3e) | confirmado (Ghidra) |

Consequência de design (confirmada no decompile do RunAll): o opcode termina no primeiro ator que acaba ou no primeiro ator tracked com halfword 0.

## 5. Pendente

- Conteúdo da cadeia `FUN_08048684` -> `FUN_08048210` (grupo -> pose em +0x30) e o retorno (o decompile mostra void, mas o chamador usa o valor).
- Quem grava +0x32 (valor salvo para o pisca) e quem lê +0x2D/+0x21/+0x2C/+0x0B no render.
- Continuações não lidas nesta rodada: 0x0804F0A4 (0x41), 0x0804E700 (0x2B), 0x0804E784 (0x2D), 0x0804F940 (0x6E), 0x0804E534 (nome do helper não confirmado no Ghidra).
- Quem põe 0xFFFF (ator ativo) em 0x030055A0 fora do spawn e do 0x7F (checar callers de Map_InitObjectSprite).
- Efeito de 0x7F nos slots 0x30 e 0x5A: copiar o ator para o companheiro (chute; o companheiro usa o slot 0x30, OW2).
- Após retornar 1, `Map_TickObjectStep` deixa +0x28 armado: confirmar com replay se o objeto volta a andar se Tick for chamado de novo.
- Confirmação em emulador: nenhuma (regra desta tarefa: sem emulador).

