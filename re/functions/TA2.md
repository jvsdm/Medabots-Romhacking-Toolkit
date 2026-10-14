# TA2: leitores de B+0xA8 (técnica ativa do bot) — decisão por técnica (GH-TA2, 2026-10-14, PARCIAL)

Base: `TA.md` (§2 pendentes), `TQ.md` (§4 tabela 0..59). Leitura só da ROM original (`python -I`) e Ghidra
(decompile, disassemble_function, get_xrefs_to). Ghidra não salvo, não fechado.
Convenção: fight = 0x03001800; bot = fight+0x87; B = 0x03000590 + bot*0x100; B+0xA8 = técnica do bot.
Índices de jump table são `técnica - K` (K varia por despachante, ver cada seção).

## 1. Despachante AS (ícone/estado; `v = técnica - 4`, tabela de 5 entradas, técnicas 4..8)

Entradas conferidas na ROM (ponteiros da tabela): 0x080279A8, 0x080279B0, 0x080279B8, 0x080279C0, 0x080279C8.
Disassembly de 0x08027974 (confirmado):

### 1a. Fight_DispatchOnSubstateAA_AS_SelectVariantByTechnique — 0x08027974 (substate AS)
Escreve `fight+0x51` (variante do sub-roteiro) e `fight+0xAA++`.

| técnica | v | fight+0x51 |
|---|---|---|
| 4 Laser | 0 | 0 |
| 5 Beam | 1 | 1 |
| 6 Missile | 2 | 2 |
| 7 Napalm | 3 | 3 |
| 8 Break | 4 | 4 |
| outras (0..3, 9..59) | fora | 5 (caminho padrão) |

Confirmado (asm). Significado de `fight+0x51` como variante: provável (usado depois pelo roteiro; não lido).

### 1b. Fight_DispatchOnSubstateAA_AS_SetStatusFlagByTechnique — 0x08027E30 (substate AS)
Escreve `bot+0xD6 |= bit` e `fight+0xAA++`. Bits por técnica (confirmado no asm):

| técnica | bit em bot+0xD6 |
|---|---|
| 4 Laser | 0x01 |
| 5 Beam | 0x08 |
| 6 Missile | 0x02 |
| 7 Napalm | 0x10 |
| 8 Break | 0x04 |
| outras | 0x20 (caminho padrão) |

Confirmado (asm). Nomes dos bits: provável (bit 0x20 = "sem efeito de técnica 4..8"). Ler quem testa bot+0xD6 fica pendente.

### 1c. Fight_DispatchOnSubstateAA_AS_SetStepCodeByTechnique — 0x08027EEC (substate AS)
Escreve `fight+0xA6` (código de passo) e `fight+0xAA++`. Tabela 0x08027F0C:

| técnica | fight+0xA6 |
|---|---|
| 4 Laser | 0x4A |
| 5 Beam | 0x61 |
| 6 Missile | 0x4B |
| 7 Napalm | 0x62 |
| 8 Break | 0x4C |
| outras | 0x63 |

Confirmado (asm). Significado do código de passo: provável (0x4A..0x4C e 0x61..0x63 são pares de passo por técnica).

## 2. Despachante AV (efeito de dano; `v = técnica - 0x0C`, técnicas 12..40, 29 entradas)

### 2a. Fight_DispatchOnSubstateAA_AV_SelectVariantByTechnique — 0x0802843C (substate AV)
Se `v > 0x1C` (técnica fora de 12..40) só `fight+0xAA++`. Senão jump table 0x0802845C e `fight+0xAA++`.
Disassembly de 0x080284D0 (confirmado). Entradas que escrevem `fight+0x51`:

| técnica | fight+0x51 |
|---|---|
| 12 Attack Clr | 0 |
| 14 Virus | 1 |
| 16 Freeze | 2 |
| 18 Hold | 3 |
| 40 Accel Chrg | 4 |
| 13, 15, 17, 19..39 | não escreve (entrada padrão 0x080284F8: só `+0xAA++`) |

Confirmado (asm). Nota: a entrada 40 cai no índice 28 (última), por isso o caminho de 40 é diferente dos vizinhos.
Esta é a mesma variável `fight+0x51` do item 1a, mas com outro conjunto de valores (0..4) e outras técnicas:
o consumidor de `fight+0x51` precisa ser lido antes de afirmar o que a variante muda (pendente).

### 2b. Fight_DispatchOnSubstateAA_AV_ApplyStatusToOpponents — 0x080287DC (função; 0x08028834 é o miolo) (substate AV)
Para cada um dos 3 slots do lado adversário (base em fight+0x60, via `Fight_GetOpponentSideBaseIndex` 0x08032FB8),
com o bot vivo (C+0x40 e C+0x59 != 0) e sem técnica 16 (`Fight_HasNoTechnique16Part` 0x080331F8), e com
`B(atacante)+0xA8 - 0xC <= 0x1C`, despacha pela tabela 0x08028854 (ponteiro no literal 0x08028850).
Cada caso: `fight+0xD2+k = 1` (marca alvo), chama 0x08036E64(slot, status, fight+0xE8, fight+0xEC) e, em alguns,
antes chama 0x08036D64(slot, status) e só aplica se retornar 0 (provável: "já tem o status").

| técnica | idx | status (r1) | checa antes (0x08036D64) | marca fight+0xD2 |
|---|---|---|---|---|
| 12 Attack Clr | 0 | 0x0D | sim | sim |
| 14 Virus | 2 | 0x10 | sim | sim |
| 16 Freeze | 4 | 0x12 | sim | sim |
| 18 Hold | 6 | 0x13 | sim | sim |
| 40 Accel Chrg | 28 | 0x2C | não | sim |
| 13, 15, 17, 19..39 | — | nenhum (padrão 0x080289E4) | — | — |

Confirmado (asm) para os valores de status e checagens. Nome do que 0x08036E64 faz (aplica status): provável.
Fim: `fight+0xAA++`. Os IDs de status 0x0D, 0x10, 0x12, 0x13, 0x2C são números de status da ROM; o nome de cada um
não foi lido (pendente: cruzar com AW_00 e com a tabela de status de `DM.md`/`TQ2.md`).

### 2c. Fight_DispatchOnSubstateAA_AW_00_SelectStatusTypeFromTechnique — 0x08028C78 (substate AW_00)
Lê B+0xA8 do atacante e escreve `fight+0x51` (tipo de status) e `fight+0xAA++`:

| técnica | B+0xA8 | fight+0x51 |
|---|---|---|
| 13 Bug | 0x0D | 0 |
| 15 Thunder | 0x0F | 1 |
| 17 Wave | 0x11 | 2 |
| 19 Fire | 0x13 | 3 |
| outras | — | não escreve |

Confirmado (asm). Mesma variável `fight+0x51` das seções 1a e 2a (sem sobreposição de técnica).

### 2d. Fight_DispatchOnSubstateAA_BF_SelectAlliesAndStat — 0x0802B868 (substate BF)
Chama `FightPreview_ClearPartCounters3`, base de aliados em fight+0x60 (`Fight_GetAllySideBaseIndex`), marca
`fight+0xD2+k = 1` para cada um dos 3 aliados vivos. Depois, por B+0xA8 do atacante, escreve `fight+0x51`:

| técnica | B+0xA8 | fight+0x51 |
|---|---|---|
| 31 AutoRecover | 0x1F | 0 |
| 33 Reactivate | 0x21 | 1 |
| 35 Anti-Sea | 0x23 | 2 |
| 37 Conceal | 0x25 | 3 |
| outras | — | não escreve |

Confirmado (asm). Fim: `fight+0xAA++`. Nota: esta função marca aliados (`+0xD2` = 1), não alvos inimigos.

### 2e. Fight_DispatchOnSubstateAA_BE_SetResultMessage — 0x0802B7BC (substate BE)
Se B+0xA8 do atacante == 0x1E (técnica 30 Recovery): `fight+0xA6 = 0x4D`; senão `0x4E`. `fight+0xAA++`.
Confirmado (asm). Mesmo campo `fight+0xA6` de 1c (código de passo). Significado de 0x4D/0x4E: provável (mensagem de
resultado; 0x4D = cura, 0x4E = outros).

### 2f. Fight_DispatchOnSubstateAA_BR_47_LoadMedaforceName — 0x080318E0 (substate BR_47)
Copia 0x11 bytes de uma tabela de ponteiros indexada por B+0xA8 (`ptr[B+0xA8]`, tabela em 0x08031904) para um buffer
de nome (destino 0x08031910). Escreve `fight+0xA6 = 0x38`, `fight+0xAA++`. B+0xA8 aqui é índice de nome, não decisão.
Confirmado (asm/decompile). Efeito por técnica: só o nome exibido (sem valor por técnica lido).

### 2g. Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction — 0x08031938 (substate BR_49; função inicia em 0x08031938)
Inicia a ação de medaforça: `fight+0xA8 = B(atacante)+0xA8 + 0x50` (técnica 0..59 -> medaforça 0x50..0x8B),
`fight+0x164 = 1`, zera `bot+0xE0` e `bot+0xE1`, grava 9 e 0 em duas palavras de estado (0x08031970/0x08031974).
Confirmado (asm). É o único ponto que converte técnica em medaforça (0x50 + id), confirmando TA.md §1 (F+0xA8 de medaforça).

### 2h. Fight_DispatchOnSubstateAA_G_03_BuildBattleBackground — 0x0801E1B4 (substate G_03)
Máquina de 2 estados (contador em 0x0801E0B4+0x0801E0B8). Estado 0: limpa, carrega fundo, CpuFastSet.
Estado 1: carrega gráfico de sprite do atacante: se `B+0xDD < 4` usa a cor da peça (`B+0xA7*0x18 + 0x49`); senão
usa `B+0xA8 + 0x50` (medaforça). `fight+0xAA++`. Não há decisão por técnica; B+0xA8 só entra no ramo de medaforça.
Confirmado (asm/decompile). Mesma lógica de ObjScript_Op1E (TA.md §2).

### 2i. ObjScript_Op0D_SpawnEffectSlot4PartPal — 0x080398EC (opcode 0x0D do roteiro de objeto)
Carrega paleta de 9 cores: se `B+0xDD < 4` usa cor da peça; senão `B+0xA8 + 0x50` (medaforça). Cria slot de animação
(`Fight_InitAnimScriptSlot`) com tabela 0x08039998 e ajusta posição/sequência. Confirmado (asm/decompile). Gráfico/paleta,
não dano, não é decisão por técnica.

## 3. Resumo do que muda por técnica até aqui (AS, AV, AW, BF, BE)

Técnicas com efeito visível neste conjunto: 4..8 (AS: variante, bit de status, passo), 12..40 (AV: variante;
status 12, 14, 16, 18, 40 com checagem em 12/14/16/18), 13/15/17/19 (AW: tipo de status em fight+0x51), 31/33/35/37
(BF: aliados + variante), 30 (BE: mensagem). 0x1E = 30 (Recovery), não 34/35 (TQ.md §4).

## 4. Pendências desta rodada (ainda não decompiladas)

### 2j. Os 4 restantes de gfx/paleta (ObjScript Op1F 0x0803A60C, Op2C 0x0803AE3C, Op32 0x0803B3B4, Op39 0x0803B794)
Mesmo padrão de Op0D (2i): cor/gfx da peça se `B+0xDD < 4`, senão `B+0xA8 + 0x50` (medaforça). Diferença só no slot
criado: Op1F cria estado 0x12, Op2C estado 0x13, Op39 estado 0x10 sem troca de prioridade; Op32 só carrega lista de gfx.
Confirmado (asm/decompile). Nenhum decide efeito por técnica.

## 4. Quem grava B+0xA8 (varredura da ROM, read-only) — CORREÇÃO de TA.md

Varredura (python -I, `re/functions/`: scan de `add Rd,#0xA8` seguido de `strb [Rd,#0]` em 14 meias-palavras, e
busca de literal 0x03000590+bot*0x100+0xA8 para bot 0..15 na área de código): **nenhum gravador novo de B+0xA8
foi confirmado.** Candidatos checados à mão:

| endereço (add/store) | o que é | conclusão |
|---|---|---|
| 0x080340F6 / 0x0803410E | Op1B (`ActionScript_Op1B_LoadPaletteAndSetTechniqueId`): lê B+0xA8 (`ldrb` em 0x080340F8) e soma 0x50; grava r3+0xF9 e bot+0x69 e bot+0xA9 | **não grava B+0xA8.** TA.md §0 ("gravada por Op1B") está errado. Op1B é leitor. |
| 0x08030C64 / 0x08030C68 | F+0xA8 (`BE_14_ActionSelectValidatePart`) | é F+0xA8 (fight), não B |
| 0x08031936 / 0x08031940 | F+0xA8 = B+0xA8+0x50 (`BR_49`) | é F+0xA8; o store 0x0803194A é bot+0xE0 (não B+0xA8) |
| 0x08067024 / 0x08067032 | `PointRing_StepFrame` (0x08067030), struct de anel (`*DAT_08066dd4`) | falso positivo: não é o bot |

Portanto, B+0xA8 do bot continua sendo escrito só pelo caminho de seleção de técnica (ver `TQ.md` §1: a técnica
entra em F+0xA8 e o bot recebe a técnica ao entrar no roteiro). **Pendente:** achar a rotina que copia F+0xA8 (ou
tabela de técnica) para B+0xA8; não está na área de código varrida com o padrão `add+strb`. Limitação da varredura:
não cobre escrita com registrador copiado por `mov` antes do `strb` nem `strb` com offset diferente de 0.

## 5. Consumidores pendentes e limites

- `fight+0x51` (variante): escrito por 1a (técnicas 4..8), 2a (12..40), 2c (13/15/17/19), 2d (31/33/35/37). Consumidor não lido.
- `bot+0xD6` (bits 0x01,0x02,0x04,0x08,0x10,0x20 de 1b): consumidor não lido.
- `fight+0xA6` (código de passo): escrito por 1c (4..8 e outras), 2e (30), 2f (0x38). Consumidor não lido.
- 0x08036D64 (checa status na peça) e 0x08036E64 (aplica status): nomes são **provável**; corpo não lido.
- Efeito real de cada variante/bit (gráfico, alvo, status em termos de dano): depende de consumidores acima; não confirmado em runtime.

## 6. TABELA técnica 0..59 -> o que muda (TQ.md + leitores de B+0xA8)

Colunas: setup/alvo (TQ §4); efeito de estado/variante/passo (TA2 deste arquivo); "—" = sem leitor de B+0xA8 neste
conjunto (efeito vem só do roteiro/opcodes, TQ2). **Confirmado** = lido em asm/decompile; **provável** = por nome ou
por consumidor não lido. Perna/anti-ar só para 34 e 35 (TQ §4); demais não tratam perna por este caminho.

| id | técnica | família (TQ) | setup/alvo (TQ) | estado/status (TA2) | variante fight+0x51 | bit bot+0xD6 | passo fight+0xA6 | gráfico/paleta | confiança |
|---|---|---|---|---|---|---|---|---|---|
| 0 | Sword | corpo a corpo | nada | — | — | — | — | — | TQ |
| 1 | Hammer | corpo a corpo | nada | — | — | — | — | — | TQ |
| 2 | Rifle | tiro | BySpeciality | — | — | — | — | — | TQ |
| 3 | Chain Gun | tiro | BySpeciality | — | — | — | — | — | TQ |
| 4 | Laser | energia | BySpeciality | — | 0 | 0x01 | 0x4A | — | confirmado (1a,1b,1c) |
| 5 | Beam | energia | BySpeciality | — | 1 | 0x08 | 0x61 | — | confirmado |
| 6 | Missile | explosão | BySpeciality | — | 2 | 0x02 | 0x4B | — | confirmado |
| 7 | Napalm | área/fogo | BySpeciality | — | 3 | 0x10 | 0x62 | — | confirmado |
| 8 | Break | quebra | BySpeciality | — | 4 | 0x04 | 0x4C | — | confirmado |
| 9 | Press | esmagamento | BySpeciality | — | — | — | — | — | TQ |
| 10 | Grap Trap | armadilha | nada | — | — | — | — | — | TQ |
| 11 | Shot Trap | armadilha | nada | — | — | — | — | — | TQ |
| 12 | Attack Clr | limpeza | nada | status 0x0D, checa antes, marca alvos | 0 | — | — | — | confirmado (2a,2b) |
| 13 | Bug | status | nada | tipo de status 0 (fight+0x51) | 0 (2c) | — | — | — | confirmado (2c) |
| 14 | Virus | status | nada | status 0x10, checa antes | 1 (2a) | — | — | — | confirmado |
| 15 | Thunder | elemental | nada | tipo de status 1 | 1 (2c) | — | — | — | confirmado (2c) |
| 16 | Freeze | elemental | nada (guarda: sem técnica 16) | status 0x12, checa antes | 2 (2a) | — | — | — | confirmado |
| 17 | Wave | elemental área | nada | tipo de status 2 | 2 (2c) | — | — | — | confirmado (2c) |
| 18 | Hold | prende | nada | status 0x13, checa antes | 3 (2a) | — | — | — | confirmado |
| 19 | Fire | elemental | nada | tipo de status 3 | 3 (2c) | — | — | — | confirmado (2c) |
| 20 | Melt | elemental | nada | — | — | — | — | — | TQ (sem leitor) |
| 21 | Break Form | status | nada | — | — | — | — | — | provável (TQ) |
| 22 | Stability | suporte | RequirePendingEffect | — | — | — | — | — | TQ |
| 23 | Force Bind | prende | nada | — | — | — | — | — | TQ |
| 24 | CancelOptic | cancela | nada | — | — | — | — | — | TQ |
| 25 | CancelBomb | cancela | nada | — | — | — | — | — | TQ |
| 26 | CancelGrav | cancela | nada | — | — | — | — | — | TQ |
| 27 | Defence | defesa | nada | — | — | — | — | — | TQ |
| 28 | Full Block | defesa | nada | — | — | — | — | — | TQ |
| 29 | Half Block | defesa | nada | — | — | — | — | — | TQ |
| 30 | Recovery | cura | RequireDamagedPart | mensagem de resultado 0x4D (BE_SetResultMessage) | — | — | 0x4D | — | confirmado (2e) |
| 31 | AutoRecover | cura auto | ChooseActionTargetSlot | marca aliados +0xD2; variante 0 (2d) | 0 (2d) | — | — | — | confirmado |
| 32 | Repair | reparo | RequireBrokenPart | — | — | — | — | — | TQ |
| 33 | Reactivate | reativa | RequireStateBA2 | marca aliados; variante 1 (2d) | 1 (2d) | — | — | — | confirmado |
| 34 | Anti-Air | anti-ar | alvo perna 'd' (0x64) | — | — | — | — | — | TQ (perna) |
| 35 | Anti-Sea | anti-mar | alvo perna 'j' (0x6A) | marca aliados; variante 2 (2d) | 2 (2d) | — | — | — | confirmado (2d) + TQ (perna) |
| 36 | Scout | reconhecimento | nada | — | — | — | — | — | TQ |
| 37 | Conceal | ocultação | nada | marca aliados; variante 3 (2d) | 3 (2d) | — | — | — | confirmado |
| 38 | Boost Chrg | carga | ChooseActionTargetSlot | — | — | — | — | — | TQ |
| 39 | Rapid Chrg | carga | ExcludeSelf | — | — | — | — | — | TQ |
| 40 | Accel Chrg | carga | nada | status 0x2C sem checagem, marca alvos | 4 (2a) | — | — | — | confirmado |
| 41 | Drain Chrg | dreno | RequireNoTech16 | — | — | — | — | — | TQ |
| 42 | Pushover | golpe | BySpeciality | — | — | — | — | — | TQ |
| 43 | Impair | status (peça) | peça | — | — | — | — | — | TQ |
| 44 | Confusion | status | RequireNoTech16 | — | — | — | — | — | TQ |
| 45 | Use Drain | dreno | RequireUsableHeadAction | — | — | — | — | — | TQ |
| 46 | No Escape | status | RequireNoTech16 | — | — | — | — | — | TQ |
| 47 | No Defense | status | RequireNoTech16 | — | — | — | — | — | TQ |
| 48 | Destroy | ataque direto | nada | — | — | — | — | — | TQ |
| 49 | Sacrifice | sacrifício | BySpeciality | — | — | — | — | — | TQ |
| 50 | Forcedrain | dreno | RequireMedaforceToken | — | — | — | — | — | TQ |
| 51 | Team Form | suporte | nada | — | — | — | — | — | TQ |
| 52 | TeamAttack | time | BySpeciality | — | — | — | — | — | TQ |
| 53 | Strengthen | bônus poder | nada | — | — | — | — | — | TQ (DM) |
| 54 | CntrAttk | contra-ataque | nada | — | — | — | — | — | TQ |
| 55 | Change | troca estado | nada | — | — | — | — | — | TQ |
| 56 | ItrpChange | interrompe | nada | — | — | — | — | — | TQ |
| 57 | AttkChange | troca ataque | nada | — | — | — | — | — | provável (TQ) |
| 58 | DEF Change | troca defesa | nada | — | — | — | — | — | provável (TQ) |
| 59 | HealChange | troca cura | nada | — | — | — | — | — | provável (TQ) |

Notas de leitura da tabela:
- Técnicas 4..8 e 12..40 têm variante/bit próprios; 13/15/17/19 e 31/33/35/37 têm variante própria em `fight+0x51`
  (mesma variável, valores 0..3); 0..4 de `fight+0x51` servem a dois grupos diferentes, então o consumidor é decisivo.
- Sem leitor de B+0xA8 neste conjunto: 0..3, 9..11, 20..29 (exceto 30), 32, 36, 38, 39, 41..59. Não é "efeito zero":
  o efeito deles depende de roteiro e opcodes (`TQ2.md`), não deste caminho.
- Medaforça (técnica + 0x50 em F+0xA8) para todas as técnicas quando o bot usa medaforça (BR_49, G_03, Op0D/Op1F/Op2C/Op32/Op39): só
  gráfico/paleta, não muda efeito por técnica.
