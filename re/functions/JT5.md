# Saltos indiretos restantes (GH-JT5, 2026-10-14)

Continuação de JT.md, JT2, JT3 e JT4. Método: disassemble no Ghidra (leitura), leitura de palavras da ROM
original (python -I, só leitura), decisão por item: retorno (`pop {..}; pop {r0}; bx r0`) ou tabela real
(`mov pc,rN` / `bx rN` após `ldr rN,[tabela+idx*4]`). Tabela real recebe comentário `JT5:` no salto do Ghidra.

## Itens

### 1. FL5.md — Anim_SlotScriptTick (0x0803C680) — RETORNO (FL5 corrigido)
- Sem jump table. Termina `pop {r4,r5}; pop {r0}; bx r0` (0x0803C732). As tabelas DAT_0803c6ec e DAT_0803c734 são
  de dados (o comentário Ghidra em 0x0803C6CA já diz "tabela de dados, nao salto"). Ver JT4 item 5.

### 2. FL5.md — Screen_IntroTimeline (0x0806DAA4) — RETORNO / cadeia if (FL5 corrigido)
- Sem jump table (JT4 item 4). O salto flagado é o `pop {r0}; bx r0` da função anterior. FL5 corrigido com "(JT4)".

### 3. FL8.md:363 — Fight_TickBattleFrame (0x0806BC48) — RETORNO
- Sem tabela. Chamadas em sequência (0x0806BFEC/0x0806C218/0x0806BF18 ou 0x0806C694, depois 0x0806C084, C3A0,
  C660, D398, CF7C, CD68, CEDC, CC80) e termina `pop {r0}; bx r0` (0x0806BC92). O "salto indireto" era o retorno.

### 4. FL.md:20 — Tile_ClearSlotsAndDispatchState (0x08007D28) — TABELA REAL (5 entradas)
- `mov pc,r0` em 0x08007D46 (o Ghidra deu o endereço 0x08007D44 como ponto do salto; o salto é 0x08007D46).
- Índice = byte +0x3C de DAT_08007D48, só se <= 4. Literal 0x08007D4C aponta para tabela 0x08007D50:

| Índice | Alvo | O que faz |
|---|---|---|
| 0 | 0x08007DC8 | zera estado (retorno) |
| 1 | 0x08007D64 | copia byte +0x3D para +0x97 de DAT_08007D7C; estado = 3; próximo estado 0x10 |
| 2 | 0x08007D88 | `bl 0x0800B498`; dois CpuFastSet (0x0807CCC0) de palavras zeradas na pilha para destinos fixos (um deles 0x05000000 = paleta, `mov r1,#0xA0<<19`); estado = 7; próximo 0x3C |
| 3 | 0x08007DC8 | zera estado (retorno) |
| 4 | 0x08007DC8 | zera estado (retorno) |

- Comentário Ghidra colocado em 0x08007D46.
- Confiança: confirmado (lido na ROM e no disassembly). Nome do estado continua chute (FL.md).

### 5. FL3.md:264 — FUN_08071F74 (0x08071F74) — RETORNO (valor 1/0 em r0)
- Sem tabela. O texto de FL3 ("termina chamando um ponteiro de jump table com 1 ou 0") está errado: a função
  devolve r0 = 1 (caso da queda: grava SFX 0x54, byte 0x21, `ctx+0x10 += 1`) ou 0, e sai por `pop {r1}; bx r1`
  (0x08071FF6). Correção sugerida no FL3: "retorno com r0 = 1/0".

### 6. HA4.md:92 — R 0x08 Fight_DispatchOnSubstateAA_R_08 (0x08020B4C) — RETORNO
- Não há tabela. O "ponteiro 0x08020CE8" é o literal que aponta para RAM 0x03001800 (fight_information): o código
  faz `+0xAA += 1` (0x08020CC2) e sai por `pop {r0}; bx r0` (0x08020CD6). O próximo passo é 0x09 (ver HA4).

### 7. HA8.md:36 — AI 0x08 Fight_DispatchOnSubstateAA_AI_LoadTechniqueGraphics (0x08025858) — RETORNO
- Sem tabela. Escolhe o gráfico (LZ77 ou cpufastset), ajusta IO, grava `+0xAA += 1` (0x0802593E) e sai por
  `pop {r0}; bx r0` (0x08025944). O "salto por outra tabela" de HA8 não existe.

### 8. HN3.md:31 — AV 0x06 Fight_DispatchOnSubstateAA_AV_06_SpawnTargetEffectSlots (0x08028A38) — TABELA REAL (6)
- `mov pc,r0` em 0x08028AB4. Índice = byte +0x51 do bloco fi (+0x60 no contexto), `cmp #5; bhi` (<=5).
- Literal 0x08028AD4 -> tabela 0x08028AD8 (6 ponteiros):

| Índice | Alvo | Ação (store halfword em +6 do slot 0x21+i) |
|---|---|---|
| 0 | 0x08028AF0 | r1 = 0x0DE<<7 (valor de atributo), depois 0x08028B32 |
| 1 | 0x08028AFA | valor do literal 0x08028B04 |
| 2 | 0x08028B08 | valor do literal 0x08028B10 |
| 3 | 0x08028B14 | valor do literal 0x08028B1C |
| 4 | 0x08028B20 | valor do literal 0x08028B28 |
| 5 | 0x08028B2C | valor do literal 0x08028B50 |

- Comentário Ghidra em 0x08028AB4. Os valores exatos dos literais não foram decodificados (são só atributos de OAM/tile).
- Nota: a função passa a ter o salto resolvido; o "sem alvo" de HN3 era só o switch não lido.

### 9. HN1.md:168 — AP 0x18 (0x08026D98) — RETORNO
- Sem tabela (verificado de novo). Grava +0xAA += 1 (0x08026EAA) e sai por `pop {r0}; bx r0` (0x08026EB0).

### 10. R0.md:496 — Parts_DispatchSlotHandler (0x08006824) — DUAS TABELAS REAIS (5 cada)
- Gate: bit 0x8 de DAT_08006848 (`and #8`). Índice = byte +0x5A de DAT_0800684C (ou DAT_080068A4 no 2º ramo), `cmp #4; bhi`.
- `mov pc,r0` em 0x08006844 (bit ligado): literal 0x08006850 -> tabela 0x08006854:
  0->0x08006868 (`0x08006238(0)`), 1->0x08006870 (`0x08006284(0)`), 2->0x08006878 (`0x080062D0(0)`),
  3->0x08006880 (`0x0800631C(0)`), 4->0x08006888 (`0x08006368(0)`).
- `mov pc,r0` em 0x080068A2 (bit desligado): literal 0x080068A8 -> tabela 0x080068AC: os mesmos cinco
  chamados com argumento 1 (0x080068C0, C8, D0, D8, E0).
- Comentários Ghidra em 0x08006844 e 0x080068A2. Os alvos são o miolo da própria função (os cinco helpers
  0x08006238/0x08006284/0x080062D0/0x0800631C/0x08006368 não foram abertos aqui).

### 11. R0.md:565 — Parts_DispatchSlotAction (0x08006F34) — TABELA REAL (5)
- Gate: bit 0 de DAT_08006F58 (`and #1`). Índice = byte +0x5A de DAT_08006F5C, `cmp #4; bhi` (<=4).
- `mov pc,r0` em 0x08006F56. Literal 0x08006F60 -> tabela 0x08006F64:
  0->0x08006F78, 1->0x08006F84, 2->0x08006F8A, 3->0x08006F90, 4->0x08006F9E.
- Os alvos checam bytes +0x97..+0x9B do bloco de parte (contra 0x1E e 0x77) e devolvem r0 = 1 ou 2 (retorno
  `bx lr`, 0x08006FAE). Comentário Ghidra em 0x08006F56.

### 12. HN2.md:86 — AS 0x01 Fight_DispatchOnSubstateAA_AS_LoadIconAtActor (0x080279E4) — TABELA REAL (5)
- `mov pc,r0` em 0x08027A16. Índice = (byte +0xA8 do bloco) − 4, `cmp #4; bhi` (<=4).
- Literal 0x08027A30 -> tabela 0x08027A34: 0,1->0x08027A48 (SFX 0x27); 2,3->0x08027A74 (SFX 0x45);
  4->0x08027AA0 (SFX 0x33). Os três caminhos carregam gfx (0x080009BC) e paleta (0x0807CCC0) e tocam o SFX.
- Comentário Ghidra em 0x08027A16.

### 13. HN2.md:157 — AV 0x01 Fight_DispatchOnSubstateAA_AV_LoadEffectGfx (0x08028504) — TABELA REAL (29, 6 caminhos)
- `mov pc,r0` em 0x08028526. Índice = (byte +0xA8 do bloco) − 0xC, `cmp #0x1C; bls` (0..28).
- Literal 0x08028530 -> tabela 0x08028534 (29 ponteiros). Caminhos únicos:

| Índices | Alvo | Caminho |
|---|---|---|
| 0 | 0x080285A8 | carrega gfx (0x080009BC) e CpuFastSet 8; zera um byte de estado; grava 0x5B (0x080286B0) |
| 2 | 0x080285E8 | carrega gfx e CpuFastSet 8; grava estado 1 num byte de +0x..; cai em 0x080286AA |
| 4 | 0x0802861C | carrega gfx e CpuFastSet 8; grava estado 2 (0x080286AA) |
| 6 | 0x08028650 | carrega gfx e CpuFastSet 8; grava estado 3 e 0x5C (0x080286B2) |
| 28 | 0x08028690 | carrega gfx e CpuFastSet 8; grava estado 4 e 0x5B (0x080286B0) |
| 1,3,5,7..27 | 0x080286B4 | padrão: paleta 0x084E4658, slot 0x21 OBJ 0xC604 (y 0xE0, x 0xB8 ou 0x38), Slot_SetupFromRecordList(0x083B6100[n], 0x21) |

- Os dois ramos de `0x080286B4` cobrem os índices ímpares e os de 7 a 27. Comentário Ghidra em 0x08028526.
- Com isto, o "tabela de 29" de HN2 fica resolvido.

## Já tratados antes (não refeitos aqui)

- FL10.md:103 — Dispatch_ByteStateIndexTable (0x0801387C): tabela real de 6 entradas em 0x080138A8, salto em
  0x08013898, comentário já no Ghidra. Ver JT2.md §1. (FL10 ainda diz "não recuperada": ver fechamento.)
- 0x08065868 (Sm_StepSfxRepeat3) e 0x0807131C (Screen_TableStepTimedSetup): JT4 itens 3 e 7, comentários feitos.
- 0x08071650 (despachante de fight+0x08, FL6.md:230): JT2.md, tabela de 8 passos.
- 0x08040B4C, 0x08021418/0x0802155C (SU2, N1/MS2), 0x0801DADE, 0x0801E08C, 0x0801F28A, 0x0802040C,
  0x0801FD70, 0x08026D44/D98/EF8, 0x08071F72, 0x0806E9BE: retorno (JT, JT2, JT4).

## Pendente

- FL3.md:264 (FUN_08071F74), FL.md ok (feito), HA4.md:92, HA8.md:36, HN3.md:31, HN1.md:168, HN2.md:86/157,
  R0.md:496/565 (DAT_08006F60), R3.md:1174/1182/1258/1303, R4.md:263/342/422, KI 0x0805D2C2, TA2.md:58
  (0x0802845C), sítio do `mov pc` da tabela SC (0x08050CF4).

## Rodada 2 (continuação, 2026-10-14)

### 14. TA2.md:58 — Fight_DispatchOnSubstateAA_AV_SelectVariantByTechnique (0x0802843C) — TABELA REAL (29)
- `mov pc,r0` em 0x0802844C. Índice = byte (+0xA8 do bloco) − 0x0C, `cmp #0x1C; bhi` (<=28). Literal 0x08028458 -> tabela 0x0802845C.
- Entradas: 12->0x080284D0 (fight+0x51=0), 14->0x080284D8 (=1), 16->0x080284E0 (=2), 18->0x080284E8 (=3), 40->0x080284F0 (=4);
  demais (13,15,17,19..39 incl.) -> 0x080284F8 (só fight+0xAA++). Comentário Ghidra em 0x0802844C.
- Obs.: 0x080284F8 = `+0xAA++` (sai por bx lr). Isto fecha TA2.md:58.

### 15. SC (0x08050CF4) — sítio do mov pc = Event_RunSceneCommandSequence (0x08050CCC) — TABELA REAL (5)
- Sítio: `mov pc,r0` em 0x08050CEA (literal 0x08050CF0 -> tabela 0x08050CF4). Índice = byte +0xB de 0x03006D20, `cmp #4; bls`.
- Alvos: 0->0x08050D08, 1->0x08050D40, 2->0x08050D72, 3->0x08050F38 (+0xB++), 4->0x08050F40 (r6=1).
- Bônus: segunda tabela no mesmo corpo, `mov pc,r0` em 0x08050E4E (literal 0x08050E68 -> tabela 0x08050E6C, 33 entradas, comando r4 0..32,
  `cmp #0x20; bhi`). Alvos 0x08050EF0..0x08050F22; maioria 0x08050F2A. Comentário Ghidra em 0x08050E4E.
- Fecha o "sítio do mov pc" de SC e R3.md:767.

### 16. R3.md:1174/1182 — Scene_DispatchStateHandler (0x0805424C) e Tail (0x08054280) — TABELA REAL (0x8C)
- Não é "não recuperada": o despacho é 2 níveis. 0x0805424C pega `ldr [tabela_estado 0x083D5F70 + (estado&0x3F)*4]` (literal 0x080542A8) -> ponteiro de sub-tabela em r9.
- Tail 0x08054280: `mov pc,r0` em 0x080542A2 (literal 0x080542AC -> tabela 0x080542B0, 0x8C entradas). Código = [r9 + (+0xFC)*4]; código > 0x8B chama 0x080567F0.
- 90 das 140 entradas apontam para 0x080567F0 (Overworld_EnterSceneState); 46 alvos distintos no total. Comentário Ghidra em 0x080542A2.
- Correção a R3.md:1182: 0x080567F0 NÃO salta pela tabela; ele chama 0x08054280 (`bl`, em 0x08056818 e 0x080568C0).

### 17. R3.md:1256 — Screen_UpdateByStep (0x0805830C) — 5 TABELAS REAIS
- `mov pc` em 0x08058328: 8 ptrs em 0x08058334 (literal 0x08058330), idx = step (<=7). Alvos: 0x08058354, 0x08058428, 0x08058478, 0x08058508, 0x08058550, 0x08058630, 0x0805864E, 0x080586F4.
- `mov pc` em 0x08058378: 6 ptrs em 0x08058384 (literal 0x08058380), idx = byte de estado <=5. Alvos: 0x0805839C, 0x080583B8, 0x0805840C, 0x080583D4, 0x080583F0, 0x0805840C.
- `mov pc` em 0x08058572: 6 ptrs em 0x0805857C (literal 0x08058578), idx <=5. Alvos: 0x080585CC, 0x08058594, 0x080585B0, 0x080585CC, 0x080585CC, 0x080585E8.
- `mov pc` em 0x08058716: 5 ptrs em 0x08058720 (literal 0x0805871C), idx <=4. Alvos: 0x08058734 (x2), 0x08058750, 0x0805876C, 0x08058788.
- `mov pc` em 0x08058858: 7 ptrs em 0x0805886C (literal 0x08058868), idx <=6. Alvos: 0x0805899E (0,1,2,5), 0x08058888, 0x0805894C, 0x08058954.
- Comentários Ghidra nos 5 sítios. Lacunas (0x080589A4, 0x08058A10 ...) continuam sem função.

### 18. R3.md:1303 — Screen_RedrawAndDispatchStep (0x08058C0C) — TABELA REAL (8)
- `mov pc` em 0x08058C44: 8 ptrs em 0x08058C54 (literal 0x08058C50), idx = step r5 (sem cmp: chamador passa 0..7; step > 7 chama 0x080593FA e lê fora da tabela).
  0->0x08058C74, 1->0x08058EC0, 2->0x08058F5C, 3->0x08058FBC, 4->0x08058FEC, 5->0x080591AC, 6->0x08059220, 7->0x08059338. Comentário em 0x08058C44.

## Pendente (rodada 2, depois de 18)
- R3.md:1258 é a própria Screen_UpdateByStep (item 17). Restam: R4.md:263 (dispatch_scene_state 0x0806A114), R4.md:342 (dispatch_state_8 0x0806BD70), R4.md:422 (anim_dispatch_state 0x0806CB28), KI 0x0805D2C2 (KeyItemBag_ListInputStep).

### 19. R4.md:263 — dispatch_scene_state (0x0806A114) — TABELA REAL (23)
- `mov pc` em 0x0806A12A. Idx = estado em RAM 0x03006F88 (literal 0x0806A12C: é RAM, não ROM; o que é ROM é a tabela), `cmp #0x16; bls`.
- Literal 0x0806A130 -> tabela 0x0806A134. Alvos: 0 -> 0x0806A190; 1-5 -> 0x0806A1D4; 6-13 -> 0x0806A1C8; 14 -> 0x0806A1E4; 15 -> 0x0806A20C;
  16 -> 0x0806A218; 17 -> 0x0806A230; 18 -> 0x0806A2A0; 19 -> 0x0806A2C4; 20 -> 0x0806A340; 21 -> 0x0806A360; 22 -> 0x0806A3A0. Fora: 0x0806A3B4.

### 20. R4.md:342 — dispatch_state_8 (0x0806BD70) — TABELA REAL (8)
- `mov pc` em 0x0806BD8C. Idx = byte +0x20 de RAM 0x03002C30, `cmp #7; bls`. Literal 0x0806BD94 -> tabela 0x0806BD98.
- Alvos: 0 -> 0x0806BDB8; 1 -> 0x0806BDE4; 2 -> 0x0806BE0C; 3 -> 0x0806BE22; 4,5,6 -> 0x0806BE8A; 7 -> 0x0806BEBA. Comentário em 0x0806BD8C.

### 21. R4.md:422 — anim_dispatch_state (0x0806CB28) — TABELA REAL (7)
- `mov pc` em 0x0806CB58. Idx = (byte +0x20 de 0x03002C30) − 1, `cmp #6; bls`. Literal 0x0806CB68 -> tabela 0x0806CB6C.
- Alvos: 0 -> 0x0806CB88; 1 -> 0x0806CBBC; 2 -> 0x0806CBF4; 3,4,5 -> 0x0806CBFA; 6 -> 0x0806CC28. Comentário em 0x0806CB58.

### 22. Screen/Scene: R4 e R3 da rodada 2 fechados (itens 16-21). Pendente: KI 0x0805D2C2 (próximo).

### 23. KI 0x0805D2C2 — KeyItemBag_ListInputStep (miolo 0x0805CFB4) — TABELA REAL (41)
- `mov pc` em 0x0805D2C2. Idx = halfword em 0x02040000 + (byte +0xC do cursor)*4, `cmp #0x28; bls` (0..40). Literal 0x0805D2C4 -> tabela 0x0805D2C8.
- Alvos: 0->0x0805D36C; 1->0x0805D384; 5->0x0805D398; 12->0x0805D3AC; 13->0x0805D3C0; 19->0x0805D3D4; 39->0x0805D3E8; 40->0x0805D3FC;
  demais (2-4, 6-11, 14-18, 20-38) -> 0x0805D410 (`mov r0,#4; bl 0x080476E8`, fora de faixa tratado como erro).
- Comentário Ghidra em 0x0805D2C2. KI.md "tabela não recuperada" fica resolvido; o literal aponta para ROM (tabela), não RAM.

## Fechamento
- Itens 1-23 desta rodada (JT5 1-13 da rodada 1 + 14-23 da rodada 2). Pendências da lista original: TODAS tratadas
  (FL5, FL10 via JT2, FL8, FL, FL3, HA4, HA8, HN3, HN1, HN2, R0, R3 (1174/1182/1256/1303), R4 (263/342/422), KI, TA2:58, SC 0x08050CF4).
- Ressalvas: itens com "sem tabela" foram lidos por disassembly; alvos de 0x08006238..0x08006368 (R0) e 0x08048960 (KI, chamado por
  ListInputStep) não foram decompilados aqui. 0x0805D2C8 (KI) e 0x080542B0 (Scene) são tabelas de dados ainda sem nome no Ghidra.
