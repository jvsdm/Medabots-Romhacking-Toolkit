# QA4: técnica de peça x Medaforce (GH-QA4, 2026-10-14)

Leitura: ROM original `Medabots - Metabee (Europe).gba` (só leitura, `python -I`), roteiros por BFS com 0x01 e 0x00 como
fim de segmento, decompile Ghidra de `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction` 0x08031918 (leitura).
Base: TQ2, TQ2_fluxo, TA2 §6, TA3, MF2, AS3c. Nenhum commit, sem emulador. Scripts em scratchpad (apagados).

## Veredito

**Confirmado.** Os opcodes 0x40..0x6A nunca aparecem no roteiro de nenhuma técnica de peça (0..59). Eles aparecem só
nos roteiros 0x50+id (Medaforce). Logo os valores 4..40 lidos de B+0xA8 são **ids de Medaforce** (byte 0 do descritor),
não técnicas de peça. A tabela "técnica 0..59" de TA2 §6 mistura os dois espaços: as colunas de estado/variante
(4..8, 12..19, 31, 33, 35, 37, 40) valem para o id de Medaforce de mesmo número.

Correção colateral: MF2 §6 diz que BR_49 grava `B+0xA8 = desc_id + 0x50`. O decompile (0x08031918) mostra
`F+0xA8 = B[bot*0x100 + 0xA8] + 0x50` (e `B+0xA8` não é escrito ali). TA3 §1 está certo: B+0xA8 = id copiado do
descritor; F+0xA8 = id + 0x50 = índice da tabela de roteiros.

## 1. Roteiros das técnicas de peça (0..59)

- Entradas: 0..9, 13..20, 34, 35 apontam para 0x083C6A04 (fluxo compartilhado); 10, 11, 12, 21..33, 36..59 têm entrada própria.
- Caminhada com todos os ramos (0x03 = escolha de 4 rótulos, 0x02 = salto), parando em 0x01: todas as técnicas de peça
  terminam em 0x01 (ex.: 10 -> 0x83C6A58, 49 -> 0x83C6B2B, 59 -> 0x83C6B91).
- Opcodes que aparecem em algum caminho de técnica de peça: **0x03..0x3F** (com 0x01 e 0x00 de fim).
- Opcodes >= 0x40 em roteiro de técnica de peça: **nenhum**.

## 2. Roteiros Medaforce (0x50+id)

- Segmento `[ptr[0x50+id], ptr[0x51+id])`, termina em 0x01. Ids 0x00..0x33 (52).
- Opcodes usados: 0x40..0x6A (exclusivos de Medaforce) mais 0x06, 0x08, 0x1A..0x1D, 0x20, 0x21 (compartilhados com
  peça, como visual/sincronização). Os demais opcodes de peça (0x03..0x3F) não aparecem em Medaforce.
- Padrão dos ids (alvo/rolagem/efeito): 0x42 (rolagem) + 0x45 (contador) + op de efeito 0x50..0x6A; 0x41 (alvo) aparece
  nos ids de dano em peça (01-03, 14, 1b, 1d, 32); 0x4E/0x4F (busca) só no 33.

## 3. Quem é quem em B+0xA8 e F+0xA8

| espaço | quem grava | valor | índice de roteiro | ops usados |
|---|---|---|---|---|
| peça: F+0xA8 | `Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart` 0x08030A24 = byte [B+k*0x18+0x49] | 0..59 (técnica) | ptr[0..59] | 0x03..0x3F |
| Medaforce: B+0xA8 | `Fight_SelectMedaforceActionSource` 0x08015668 (copia 6 B do descritor) | 0..0x33 (id) | — | — |
| Medaforce: F+0xA8 | BR_49 0x08031918 = B+0xA8 + 0x50 | 0x50..0x83 | ptr[0x50..0x83] | 0x40..0x6A + comuns |

Peça não grava B+0xA8 (TA3 §2, sem mudança). Medaforce não grava F+0xA8 com técnica 0..59.

## 4. Tabela correta: B+0xA8 (id de Medaforce) -> segmento e classe

Leitura de efeito: "rolagem" = op 0x42; "alvo" = 0x41; "estado" = 0x51..0x54 (ver MF2 §4). Efeito exato (valor) segue
pendente (MF2 §3, §7).

| id | ops do segmento | classe (MF2 §4) | nota para TA2 §6 |
|---|---|---|---|
| 04..09 | 51 | estado no próprio bot | TA2 lia "4..8" como técnica Laser..Break; é o id 4..8 da Medaforce |
| 0c, 0e, 10, 12 | 42 54 | estado em todos os adversários | TA2 lia 12..18 como AttackClr..Hold |
| 0d, 0f, 11, 13 | 55 | proteção contra status do time | idem |
| 0a | 42 52 | suporte do time | — |
| 0b | 53 | marca de reação | — |
| 14 | 41 42 .. 47 .. | alvo + rolagem + 0x47 (cauda não lida) | — |
| 15 | 56 | cura do time | TA2 lia 31/33 como cura/reativa |
| 16 | 42 59 | gauge | — |
| 17 | 5A | técnica sem dano | — |
| 18, 19 | 42 5B / 42 5C | pendentes | — |
| 1A | 42 57 | reparo de armadura | — |
| 1B | 41 42 08 49 .. | alvo + metade da armadura do bot | — |
| 1C | 42 58 | restaura peças | — |
| 1D | 41 42 08 4A .. | 0x4A pendente | — |
| 1E..31, 33 | 42 + 5D..6A (ver MF2 §2) | ordem BE..BR (pendente) | TA2 §6 lia 36..49 como nomes de peça |
| 27 | 42 62 21 45 | rolagem + 0x62 | — |
| 2B | 4B 42 08 4C .. | alvo com busca de bot + armadura da peça | — |
| 32 | 41 42 08 4D .. | alvo + sorteio | — |
| 33 | 4E 42 08 4F .. | busca de bot + busca | — |
| 00 | 06 08 1A 1B 1C 40 20 45 21 | sem op de efeito conhecido (AS1) | — |
| 01..03 | 42 50 / 41 42 43 / 41 42 46 | dano em peça (0x50, 0x43, 0x46) | — |

## 5. O que diferencia cada um (critério operacional)

- **Técnica de peça**: byte +0x49 do bloco da peça -> F+0xA8 (0..59) -> `gTechniqueActionScriptPtrTable[0..59]` -> roteiro
  só com 0x03..0x3F; setup em 0x083AEEA0 e validação/execução em 0x083AEF90 (TA3 §2). Não toca B+0xA8.
- **Medaforce**: descritor 0x083B7E54 (byte 0 = id) -> B+0xA8 (id) -> F+0xA8 = id+0x50 -> `ptr[0x50+id]` -> roteiro
  com 0x40..0x6A. Gate de medidor: B+0xC1 == 0x50 (MF2 §6, sem escritor de zeragem achado). Entra por
  `Fight_SelectMedaforceActionSource` 0x08015668 e `BR_49`.
- Leitores de B+0xA8 (AS 0x08027974, AV, AW, BF, BE, BR_47, BR_49, Op1B...) só fazem sentido para ids de Medaforce.

## 6. Pendente

1. Confirmar em runtime (rec de Medaforce, p.ex. id 0x0C) que a leitura de AS/AV ocorre só dentro de roteiro 0x50+.
2. Quem chama AS 0x08027974 (TA3 §4) continua aberto; a hipótese explica o resíduo: os leitores rodam só com F+0xA8 >= 0x50.
3. Medidor B+0xC1: escritor de zeragem continua não achado (MF2 §6).
