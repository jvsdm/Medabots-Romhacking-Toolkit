# DC5 - correcao da base do array de flags/campo de evento (GH-DC5, 2026-10-14)

Fonte: EF.md §1 (base 0x030057D8, indice `(id&0xFFF)>>1`). Marcado `(EF)` em cada linha alterada.
Nao alterado: o bloco de estado E = 0x030057E0 (E+0xC89, E+0xC93 etc. estao certos).

| arquivo:linha | antes | depois |
|---|---|---|
| HE1.md:7 | - Bloco de estado do evento: base **0x030057E0** (mesma base do `event_flag_list`). Offsets usados aqui: | - Bloco de estado do evento: base **0x030057E0**. O array de flags de evento (`event_flag_list`) NAO usa esta base: e 0x030057D8, indice `(id&0xFFF)>>1` (EF). Offsets usados aqui: |
| HE3.md:8 | - Base RAM do evento = `0x030057E0` (event_flag_list). Offsets usados: | - Base RAM do bloco de estado do evento = `0x030057E0`. O array de flags (`event_flag_list`) e `0x030057D8`, indice `(id&0xFFF)>>1` (EF). Offsets usados: |
| HE2.md:103 (linha nova apos o 0x2F 'cursor = base + byte') | (nenhuma base no texto) | - Leitura do campo (EF): o byte e `0x030057D8 + ((gCurrentEventId & 0xFFF) >> 1)` (id par = bits 4-6, impar = bits 0-2). A base NAO e E (0x030057E0). |
| E2F.md:46 | (byte em `0x03... + id>>1`, bits 0-2) | (byte em `0x030057D8 + ((id&0xFFF)>>1)`, bits 0-2; base corrigida, EF) |
| EX.md:10 | impar -> bit 3 de [ptr 0x030057D8 + id>>1] | impar -> bit 3 de [0x030057D8 + ((id&0xFFF)>>1)] (EF) |
| PR.md:53 e 54 | `base + id>>1` | `0x030057D8 + ((id&0xFFF)>>1)` (EF) |

## Sem alteracao (verificado)

- `ST2.md`: nao contem base de flag nem `id>>1`; nada a corrigir. A frase do EF.md §1 sobre ST2 nao confere.
- `HE2.md` linha 9 (`E = 0x030057E0`): e o bloco de estado, correto. A secao 0x2F nao tinha base; a linha 103 foi acrescentada (EF).
- `SV2.md:53`: ja usava 0x030057D8. `EX.md` foi reescrito para `(id&0xFFF)>>1`.
- Ocorrencias de 0x030057E0 em CU, DB2, HC*, HE4, OW, MP, R3, RW, SV etc. sao enderecos do bloco E ou do mapa: nao tocadas.

## Pendente para o EF.md

- EF.md §1 diz que "HE2 (§0x2F) e ST2 diziam 0x030057E0 + id>>1". No arquivo, HE2 nao dava base e ST2 nao tinha a formula; o erro real estava em HE1.md:7, HE3.md:8 (chamavam o event_flag_list de E) e E2F.md:46. Corrigir a frase no EF.md (nao feito aqui).
