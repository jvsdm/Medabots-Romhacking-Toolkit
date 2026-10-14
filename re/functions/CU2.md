# CU2: continuações de evento e campos de objeto pendentes (Medabots Metabee EU)

Status: GH-CU2 (2026-10-14). Método: Ghidra (decompile/xrefs, renomes em memória, sem salvar) + literais da ROM lidos com `python -I` (só leitura). Sem emulador, sem commit.
Base: CU.md §5. Símbolos: `CU2_simbolos.csv`. Convenção: E = gMapState 0x030057E0; slot k = 0x03002C30 + k*0x40; bloco do opcode = 0x03006D20 ([10] = render).

## 1. Continuações de evento (pendências do CU.md §5)

| Op | Continuação | Efeito | Fim | Confiança |
|---|---|---|---|---|
| 0x41 | `Event_Op41_Cont_FlickerAllActors` 0x0804F0A4 | Para i=0..3 com ator rastreado (slot != -1): se halfword 0x030055A0[(slot+0x4F)*4] == 0 conta "inativo"; senão `Map_TickObjectFlicker(slot+0x4F)` e soma o retorno. | Termina (Event_EndOpcode) se TODOS os 4 rastreados estão inativos OU se algum flicker devolveu 1 (o flicker devolve 1 ao restaurar a visibilidade). | confirmado (decompile) |
| 0x2B | `Event_Op2B_Cont_SpawnAndStepActor` 0x0804E700 | Fase E+0xC93 = 0: `Map_SpawnTrackedObjectAtTile(E+0xC94..C97)` (packed, sprite, x, y), `Map_StartObjectStep(blk[10], E+0xC98, speed 0)`, fase = 1. Fase 1: `Map_TickObjectStep(blk[10], 0)`; quando devolve 1, Event_EndOpcode. | Quando o passo do ator acaba. | confirmado |
| 0x6E | `Event_Op6E_Cont_SpawnAndStepActorFast` 0x0804F940 | Igual ao 0x2B, mas `Map_StartObjectStep(..., speed 1)` (dobro de velocidade, índice 1). | Idem. | confirmado |
| 0x2D | `Event_Op2D_Cont_StepActor` 0x0804E784 (nome do mtbe "RotateActor" é enganoso) | Sem spawn: fase 0 `Map_StartObjectStep(blk[10], E+0xC95, speed 0)`; fase 1 Tick(blk[10], 0) até 1 -> End. | Idem. | confirmado (cmd em E+0xC95); quem põe blk[10] (render) é o primário 0x2D, não lido |

Notas:
- Os 0x2B/0x6E usam `E+0xC93` como fase e `E+0xC94..C98` como argumentos (5 B: packed, sprite, x, y, move), igual ao descrito no CU.md §4.
- O 0x2D lê o comando em E+0xC95, não em E+0xC94: o 2º byte do opcode vai para E+0xC95 (pelo que o continuation indica).

## 2. Cadeia grupo -> pose -> índice de tile (+0x30)

- `Map_InitObjectSprite` 0x080503FC: `slot+0x30 = (short) FUN_08048684(slot+0x2B)` (valor de retorno usado; confirmado).
- `FUN_08048684` -> renomeado `Map_GetObjectPoseByGroup`: `id = byte[ *(0x08413160 + gCurrentMapId*4) + grupo ]`; chama `Map_PoseIdToTileIndex` com esse id e devolve o resultado. Ou seja: a POSE vem de uma tabela por mapa (ponteiro em 0x08413160 indexado por gCurrentMapId = byte E+0x7FE) e por grupo.
- `FUN_08048210` -> renomeado `Map_PoseIdToTileIndex`. Regras (leitura do decompile, literais lidos):
  1. id == 0: id = `byte[ byte[E+0xC5C] + 0x083D2B26 ]` (índice pelo gStepCollisionIdx).
  2. id > 0xCF e id != 0xFF: id = `byte[ *(0x08413EE0 + (id-0xD0)*4) + byte[E+0xC38 + (id-0xD0)] ]` (tabela aninhada; só para ids >= 0xD0).
  3. id == 0xFF: fica 0xFF.
  4. Se id != 0xFF e id > 0xBF: id = (id - 0xC0)*2 + byte[0x03006D3E] + 0x28. Ou seja, 0xC0..0xCF caem direto nessa fórmula; 0xB0..0xBF NÃO são remapeados.
- CORREÇÃO ao texto antigo ("0xB0..0xCF = tabela aninhada"): o decompile só desvia para a tabela aninhada para ids > 0xCF (a condição é 0xaf<x && 0xbf<x && 0xcf<x).
- Pendente: o conteúdo das tabelas (0x08413160, 0x083D2B26, 0x08413EE0) e o significado de 0x03006D3E (base do índice, RAM) não foram dumpados.

## 3. Campos do slot

### +0x32 (valor visível salvo do pisca): ESCRITOR CONFIRMADO
- `Map_InitializeObjectRenderSlot` 0x08048864(render, u16 v, u16 frame, u8 grupo, u8 flag): grava `slot+0x00 = v` e `slot+0x32 = v` (halfword), `slot+0x06 = frame`, `slot+0x0A = grupo` (byte), `slot+0x1C = flag` (byte).
- `Map_InitObjectSprite` passa v = 0xC804 (literal 0x08050530) para atores; a sombra (`0x79+id`) usa 0xCC0C (literal 0x08050550).
- Ou seja: +0x32 = valor de +0x00 no momento do init. `Map_TickObjectFlicker` restaura +0x00 a partir de +0x32; como nenhum leitor achado altera +0x32 depois do init, o pisca volta sempre ao valor inicial (0xC804 para ator de mapa).
- Chamadores de `Map_InitializeObjectRenderSlot`: Map_InitObjectSprite, Overworld_InitPlayerSprites, Map_LoadEntitySpawnsForCurrentMap, Map_InitSecondaryMarkerOverlay, new_game_start, obj_setup_a..e, scene_init_d, scene_init_grid_9, Screen_* (ver xrefs). Escritores de +0x32 fora deste inicializador não foram achados (busca por endereço literal do slot é inviável no Ghidra; só cobre xrefs de literal).
- +0x00 também é escrito por `Slot_BlinkRepeatStep` 0x08035410 (pisca de BATALHA, bit 0x8000 e máscara): não toca +0x32.

### +0x0B: ESCRITORES E LEITOR PROVÁVEL
- `Map_ResetObjectField0B` 0x080487C8 (chamado no init e pelo arco): `+0x0B = tab[pose]` com tab = byte em 0x080487E0 + pose: 2 -> 0xFF, 1 -> 0x00, outro -> 0x40.
- `Map_ObjectArcStep` põe 0xFF enquanto o salto não pousou (confirmado no CU.md §3b).
- Leitor: `BuildShadowOamFromObjectState` 0x08001520 copia o byte +0x0B para a lista de índices ordenada por `SortActiveObjectIndicesByPriority` (chave de prioridade do OAM). Confiança: provável (a cópia está no decompile; o uso como chave de ordenação não foi lido dentro do sort).
- Nota: 0x08001520 é o builder de OAM da batalha (lê também +0x06, +0x08, +0x18, +0x1C, +0x1D). A base de slot usada ali não foi confirmada como 0x03002C30.

### +0x21: escritor confirmado, leitor não achado
- Escrito por `Map_SetObjectRenderSlotFacingVariant` 0x08052D90 (cópia do facing). Nenhum leitor em `BuildShadowOamFromObjectState` nem nas funções de passo/arco/animação. Leitor pendente.

### +0x2C: escritores confirmados, leitor não achado
- Escrito por `Map_InitObjectSprite` (0) e por `Map_ObjectArcStep` (tab >> 4 por quadro; 0 no fim). Nenhum leitor no builder OAM. Provável: altura/deslocamento visual do arco, a confirmar no render do mapa (não achado nesta rodada).

## 4. Chamadores de Map_TickObjectStep 0x0805018C e desarme de +0x28

Chamadores (xrefs): Event_RunInitiateMovingActorSequence (0x1F), Event_RunActorStepSequence (0x23), Event_RunAllTrackedActorsStepSequence (0x29/0x2A/0x7E), EventOpcode2B/2D, Event_Op6C_Cont, Event_StepWaitThenEndOpcode (0x-, sem opcode próprio no lote), Event_MapObjectStepOpcode (0x6E), Event_OpWaitMapObjectStep, EventOpcode82_ExecuteBatchedObjectCommandBlock.

- Ao devolver 1, nenhum chamador zera +0x28 nem chama Tick de novo para o mesmo ator no mesmo passo:
  - Event_StepWaitThenEndOpcode e Event_OpWaitMapObjectStep: fase 1 -> Tick == 1 -> Event_EndOpcode.
  - RunAll: fase 1 -> soma != 0 -> Event_EndOpcode.
  - 0x82 (batch): sub-estado 1 -> Tick == 1 -> sub-estado 2; sub-estado 2 não ticka: zera para 0 e avança o comando; o próximo comando de tipo 0 chama Map_StartObjectStep (rearma +0x27 e +0x28).
- Logo: +0x28 FICA ARMADO (16/delta) depois do fim, mas o desarme é pelo protocolo do chamador, não por escrita. Nenhum outro escritor de +0x28 é chamado depois do 1 (Event_Op28 zera +0x28 só no arco 0x28).
- ARMADILHA (código, não ativa hoje): o decremento de +0x27 é `char`. Com +0x27 == 0 (após o 1), uma nova chamada de Tick volta a andar (o +0x28 armado não é 0), e o decremento faz +0x27 = 0xFF, que não é "== 0" e portanto NÃO devolve 1: o ator andaria sem parar. Por isso o protocolo "parar de ticar ao receber 1" é obrigatório. Qualquer novo chamador de Tick sem Start precisa checar isso.

## 5. Pendências que sobram

- Leitores de +0x21 e +0x2C no render do mapa (não achados no builder OAM; procurar o desenho de sombra/arco do mapa).
- Tabelas 0x08413160 (por mapa), 0x083D2B26, 0x08413EE0 e byte 0x03006D3E: conteúdo não dumpado.
- Escritores de +0x32 fora do inicializador (busca por acesso ao slot, não só por literal).
- Emulador: nada confirmado em execução (regra desta tarefa).
