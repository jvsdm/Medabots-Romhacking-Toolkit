# Saltos indiretos ainda abertos (GH-JT4, 2026-10-14)

Continuação de JT.md/JT2/JT3. Método: disassemble no Ghidra (leitura) + leitura de halfwords/palavras da ROM
original (python -I, só leitura). Retorno = `pop {..}; pop {r0}; bx r0` (ou `pop {pc}`); tabela real = `mov pc,rN`
ou `bx rN` precedido de `ldr rN,[tabela+idx*4]`.

## Itens

### 1. SU2: S 0x0F/0x10/0x16/0x17/0x26/0x27 (0x08021418, 0x08021564) — RETORNO
- Ponteiro "não recuperado" em 0x0802155C = `bx r0` (0x4700), precedido de `pop {r0}` (0x0802155A) e `pop {r4,r5}`.
  É o retorno comum da função. Não há tabela. Próximo +0xAA = +1 fixo, como já dito em SU2.md.

### 2. N1 / MS2 #18: Palette_FadeStepAndDispatch (0x08040B0C), salto em 0x08040B4C — RETORNO
- 0x08040B4C = `bx r0` (0x4700), precedido de `pop {r0}` (0x08040B4A) e `pop {r4-r6}`. Retorno. Sem tabela.

### 3. Sm_StepSfxRepeat3 (0x080657C4), `mov pc,r0` em 0x08065868 — TABELA REAL (6 entradas)
- Índice = byte/word +8 do estado (cmp #5, bls). Literal 0x0806586C aponta para tabela 0x08065870.
- 0 -> 0x08065888 (confere condição via 0x0803FB3C, SFX 0x0D, avança)
- 1, 2, 3 -> 0x080658DC (mesmo handler: confere, depois espera tecla A 3 vezes, SFX 0x0D)
- 4 -> 0x08065970 (espera e fecha o passo)
- 5 -> 0x080659B0 (finaliza: laço 0x0805DDEC, avança +3)
- Alvos são labels internos do corpo de 0x080657C4 (não funções). Vão para JT4_lista.txt.
- Comentário Ghidra colocado em 0x08065868.

### 4. Screen_IntroTimeline (0x0806DAA4) — RETORNO (FL5 errado)
- O salto flagado (0x0806DAA2 é `pop {r0}; bx r0` da função anterior; o fim desta é 0x0806DBE8 `bx r0`).
  A função não tem tabela: é cadeia de if/else por contador +8 (0x39, 0x40, 0x41...). FL5.md dizia "jump table"; não é.

### 5. Anim_SlotScriptTick (0x0803C680) — RETORNO + tabela de DADOS
- `bx r0` em 0x0803C67E é retorno da função anterior. Tabelas DAT_0803c6ec / DAT_0803c734 são de dados (JT3 já cobriu).

### 6. Screen_LayerHideTimedStep (0x0806E96C) — RETORNO
- Sem tabela. Termina `pop {r4}; pop {r0}; bx r0` (0x0806E9BE). O "destino indireto" de FR2 era o retorno.

### 7. Screen_TableStepTimedSetup (0x08071304) — TABELA REAL (26 entradas)
- `cmp r0,#0x19; bls; ... ldr r0,[r0+r1]; mov pc,r0` em 0x0807131C. Literal 0x08071324 -> tabela 0x08071328.
- Índice = estado +8 (0..0x19). Alvos (labels internos, não funções):
  - 0 -> 0x08071390: setup (0x0803FAA0), sem SFX; avança.
  - 1,5,9,13,17,21 -> 0x080713A2: lê entrada de dados (0x0803FB3C); se 0 espera; senão avança.
  - 2,6,10,14,18 -> 0x080713E2: avança +8 (espera).
  - 3,7,11,15,19 -> 0x08071424: chama 0x08071DDC; se 0 espera; senão avança.
  - 4,8,12,16,20,24 -> 0x0807139C: 0x0803FB0C e avança (fecha subpasso).
  - 22 -> 0x080713F0: carrega gfx/paleta (0x08048864), posiciona registros IO, avança.
  - 25 -> 0x080713CC: lê entrada de dados (0x0803FB3C) por (estado>>2)+8; se 0 espera; senão avança.
- Comentário Ghidra em 0x0807131C.

### 8. Sm_StepSfxRepeat3 (0x080657C4) — ver item 3 (tabela real, 6 entradas, comentário em 0x08065868).

### 9. AD: AP 15 (0x08026D44) — RETORNO. Sem tabela; só +0xAA += 3 (ou +1) e `pop {r0}; bx r0` (0x08026D96).
### 10. AD: AP 18 (0x08026D98) — RETORNO. Sem tabela; termina em 0x08026EB0.
### 11. AD: AQ 00 (0x08026EF8) — RETORNO. Chama 0x08033184; termina em 0x08026F32.
### 12. FR1: Anim_RampStepThenSfx54 (0x08071EFC) — RETORNO. `pop {r1}; bx r1` (0x08071F72). Sem tabela.
### 13. FR2: 0x08071304 — ver item 7 (tabela real).
### 14. FL5/SU2/N1/MS2 (0x08040B4C, 0x08021418, 0x08021564, 0x0802155C): ver itens 1 e 2.

## Tabelas achadas nesta rodada, ainda sem função-alvo

- SC (R3, despachante do mapa, literal 0x08050CF0 -> tabela 0x08050CF4, 5 entradas): 0 -> 0x08050D08, 1 -> 0x08050D40,
  2 -> 0x08050D72, 3 -> 0x08050F38, 4 -> 0x08050F40. Nenhum é função no Ghidra (labels internos). Sítio do `mov pc`
  não localizado nesta rodada: comentário no Ghidra **pendente**.

## Pendente (não tratado nesta rodada, por orçamento de contexto)

- FL5.md:172 (Anim_* com DAT_0803c734), FL10.md:103 (tabela de 6), FL8.md:363 (jump final sem endereço), FL.md:20
  (DAT_08007D4C, <5), FL6.md:230 (ver JT2 §: 0x08071650 já lido, confirmar sítio), FL3.md:264.
- HA4.md:92 (ponteiro 0x08020CE8 é literal que aponta para RAM 0x03001800, não tabela: confirmar o sítio real).
- HA8.md:36 (AI 0x08), HN3.md:31 (+0x51 < 6), HN1.md:168, HN2.md:86/157.
- R0.md:496/565 (Parts_DispatchSlotHandler e vizinho, tabela DAT_08006F60, índice +0x5A < 5).
- R3.md:767 (tabela DAT_08050CF0 = SC acima; falta o sítio do mov pc), R3.md:1174/1182, R3.md:1258, R3.md:1303.
- R4.md:263 (DAT_0806a12c: literal leva a RAM, não a ROM; confirmar), R4.md:342 (DAT_0806bd94, 8 handlers), R4.md:422.
- KI_lista.txt (salto de KeyItemBag_ListInputStep, Ghidra não recupera tabela 0x0805D2C2; literal leva a dado — confirmar).
- TA2.md:58 (jump 0x0802845C, 29 entradas; provavelmente já em TA2 — confirmar).
- FR2.md:125 e FR2.md:145 (já vistos: 0x08071304 cobre; confirmar se o chamador é o mesmo).

