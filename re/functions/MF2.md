# MF2: efeito de cada Medaforce (GH-MF2, 2026-10-14, PARCIAL)

Leitura estática: ROM original (só leitura, `python -I`), decompile do Ghidra (sem renomear, sem salvar).
Ids 0x00..0x33 (52 medaforças). Nomes de id são só números; nada de texto do jogo aqui.

## Status

- Mecanismo do roteiro: CONFIRMADO (decompile + ROM).
- Sequência de opcodes de cada id: CONFIRMADA (bytes), nomes dos handlers 0x36..0x6A: de AS2.md.
- Efeito exato (dano/cura/status por valor): PENDENTE (handlers 0x41..0x6A não lidos até o fim).
- Bytes 1..5 do descritor: valores brutos confirmados; significado PENDENTE.
- Quem zera B+0xC1: PENDENTE (ver §6).

## 1. Mecanismo do roteiro (confirmado)

- `Fight_AdvanceActionScriptCursor` 0x08034F08: `B+0xAA = 0`; `cursor(+0xBC) += 1 + LEN[op]`, onde
  `op` = byte em `ptr[F+0xA8] + cursor` e `LEN` = bytes 0x083C6EB8 (base somada ao opcode).
- `Fight_ReadActionScriptInlineArgs` 0x08034EB8: copia `LEN[op]` bytes seguintes para `+0xAC`.
- `Fight_LookupActionScriptByte` 0x08034E94 (nome de chute no Ghidra): `ROM[ptr[F+0xA8] + cursor]`.
- `ptr` = `gTechniqueActionScriptPtrTable` 0x083C6CA8; medaforça id usa índice `0x50 + id`.
- `LEN` (0x083C6EB8, 1 B por opcode): só são não-zero `LEN[0x02]=1`, `LEN[0x03]=4`, e na faixa 0x90.. (dados).
  Para os opcodes usados pelas medaforças (0x01, 0x06..0x6A) todo LEN é 0: o roteiro é só uma sequência de opcodes.
- Bytes >= 0x80 são rótulo/controle: o laço pula (AS1). Não aparecem nos trechos de medaforça.

## 2. Cada id = um segmento curto que termina em 0x01 (confirmado nos bytes, hipótese para o fim)

Os ponteiros 0x50..0x83 são estritamente crescentes; o segmento de cada id é `[ptr[0x50+id], ptr[0x50+id+1])`.
Todo segmento termina em `0x01` (o handler de 0x01 está em 0x080338DC). Ex.: id 0x04 = `51 45 01`.

Segmentos (opcode em hex; a sequência de handlers está em §4):

| id | segmento | leitura curta |
|----|----------|---------------|
| 00 | 06 08 1a 1b 1c 40 20 45 21 01 | só ops 0x06..0x21 (sem op de efeito conhecido; AS1 pendente) |
| 01 | 42 50 45 01 | rolagem de valor + dano em peça aleatória do adversário (0x50) |
| 02 | 41 42 43 08 1a 1b 1c 1d 44 20 45 21 01 | alvo aleatório + rolagem + soma de campo do lado (0x43) |
| 03 | 41 42 46 08 1a 1b 1c 1d 44 20 45 21 01 | alvo aleatório + rolagem + soma armadura máx−atual (0x46) |
| 04..09 | 51 45 01 (04 = 0x51 com 4; 05..09 idem, mesma forma) | estado por técnica no próprio bot (0x51) |
| 0a | 42 52 45 01 | rolagem + bônus de suporte do time (0x52) |
| 0b | 53 45 01 | marca de reação no próprio bot (0x53) |
| 0c, 0e, 10, 12 | 42 54 45 01 | rolagem + estado em todos os adversários (0x54) |
| 0d, 0f, 11, 13 | 55 45 01 | proteção contra status do time (0x55) |
| 14 | 41 42 08 1a 1b 1c 1d 44 47 1b 1c 48 20 45 21 01 | alvo + rolagem; 0x47 com cauda não lida |
| 15 | 56 45 01 | cura do time (0x56) |
| 16 | 42 59 45 01 | rolagem + gauge (0x59) |
| 17 | 5a 45 01 | técnica sem dano (0x5A) |
| 18, 19 | 42 5b 45 01 / 42 5c 45 01 | rolagem + 0x5B / 0x5C (pendentes) |
| 1a | 42 57 45 01 | rolagem + reparo de armadura (0x57) |
| 1b | 41 42 08 49 1a 1b 1c 1d 44 20 45 21 01 | alvo + rolagem + metade da armadura do próprio bot (0x49) |
| 1c | 42 58 45 01 | rolagem + restaura peças (0x58) |
| 1d | 41 42 08 4a 1a 1b 1c 1d 44 20 45 21 01 | alvo + rolagem + 0x4A (pendente, laço não decodificado) |
| 1e, 20, 21, 22..26, 28, 29, 2a, 2b, 2c, 2d, 2e, 2f, 30, 31 | 42 + 5d..6a + 45 01 (ver §3) | rolagem + op de ordem BE..BR (pendentes) |
| 27 | 42 62 21 45 01 | rolagem + 0x62 + op 0x21 |
| 2b | 4b 42 08 4c 1a 1b 1c 1d 44 20 45 21 01 | alvo com busca de bot + armadura da peça (0x4C) |
| 32 | 41 42 08 4d 1a 1b 1c 1d 44 20 45 21 01 | alvo + rolagem + sorteio (0x4D) |
| 33 | 4e 42 08 4f 1a 1b 1c 1d 44 20 45 21 01 | busca de bot (0x4E) + rolagem + busca (0x4F) |

Observação: os segmentos 0x04..0x09 e 0x0c..0x13 são todos de 3 a 4 bytes, então o efeito real está quase todo
no que o handler faz com o id (ver §5). Ops 0x06..0x35 (AS1) não estão decodificados; os de 0x1a, 0x1b, 0x1c, 0x20,
0x21, 0x08 e 0x1d aparecem em todas as medaforças de dano e são visuais/sincronização (ver §4).

## 3. Descritor (desc6 0x083B7E54, 6 B por id)

Byte 0 = id (confirmado: bate em todos os 52). Bytes 1..5 (valores brutos, significado PENDENTE):

| id | b1 | b2 | b3 | b4 | b5 |
|----|----|----|----|----|----|
| 00 | 01 | 50 | 00 | 14 | 01 |
| 01 | 00 | 3c | 00 | 1e | 00 |
| 02 | 03 | 50 | 00 | 0a | 01 |
| 03 | 02 | 3c | 00 | 0a | 01 |
| 04 | 02 | 28 | 00 | 00 | 03 |
| 05 | 03 | 46 | 00 | 00 | 03 |
| 06 | 02 | 28 | 00 | 00 | 03 |
| 07 | 03 | 46 | 00 | 00 | 03 |
| 08 | 02 | 28 | 00 | 00 | 03 |
| 09 | 03 | 46 | 00 | 00 | 03 |
| 0a | 04 | 3c | 1e | 1e | 00 |
| 0b | 04 | 1e | 00 | 00 | 03 |
| 0c | 00 | 32 | 10 | 28 | 00 |
| 0d | 01 | 3c | 00 | 00 | 02 |
| 0e | 00 | 32 | 08 | 08 | 00 |
| 0f | 01 | 3c | 00 | 00 | 02 |
| 10 | 00 | 32 | 10 | 0a | 00 |
| 11 | 01 | 3c | 00 | 00 | 02 |
| 12 | 00 | 32 | 10 | 08 | 00 |
| 13 | 01 | 3c | 00 | 00 | 02 |
| 14 | 05 | 46 | 00 | 3c | 01 |
| 15 | 05 | 28 | 00 | 00 | 02 |
| 16 | 07 | 28 | 00 | 02 | 02 |
| 17 | 07 | 32 | 00 | 00 | 00 |
| 18 | 04 | 28 | 00 | 0a | 03 |
| 19 | 04 | 28 | 00 | 0a | 03 |
| 1a | 05 | 3c | 00 | 3c | 02 |
| 1b | 05 | 50 | 00 | 00 | 01 |
| 1c | 05 | 46 | 00 | 28 | 02 |
| 1d | 05 | 32 | 00 | 14 | 01 |
| 1e | 02 | 32 | 10 | 0a | 00 |
| 1f | 03 | 28 | 00 | 1e | 02 |
| 20 | 02 | 32 | 10 | 0a | 00 |
| 21 | 03 | 28 | 00 | 1e | 02 |
| 22 | 06 | 1e | 00 | 1e | 02 |
| 23 | 06 | 28 | 00 | 1e | 02 |
| 24 | 06 | 1e | 00 | 1e | 02 |
| 25 | 06 | 28 | 00 | 1e | 02 |
| 26 | 06 | 28 | 10 | 0a | 02 |
| 27 | 06 | 3c | 00 | 02 | 00 |
| 28 | 07 | 32 | 10 | 0a | 00 |
| 29 | 07 | 32 | 10 | 0a | 00 |
| 2a | 01 | 46 | 00 | 01 | 00 |
| 2b | 03 | 46 | 00 | 00 | 01 |
| 2c | 07 | 3c | 00 | 00 | 00 |
| 2d | 07 | 32 | 00 | 00 | 02 |
| 2e | 04 | 28 | 00 | 14 | 03 |
| 2f | 04 | 46 | 00 | 00 | 00 |
| 30 | 04 | 32 | 00 | 00 | 03 |
| 31 | 04 | 46 | 00 | 00 | 03 |
| 32 | 07 | 3c | 00 | 0a | 01 |
| 33 | 07 | 46 | 00 | 50 | 01 |

Hipóteses (chute, não confirmadas): b1 0..7 = classe ou especialidade (faixa 0..7 igual ao campo de especialidade
de DM); b4 = valor de efeito; b5 0..3 = alcance. b2 nos valores 0x28/0x32/0x3C/0x46/0x50 parece custo ou
probabilidade, não confirmado. Pendente: ler quem lê F+0xA9..0xAD (bot+0xA9..0xAD copiados em
`Fight_SelectMedaforceActionSource`).

## 4. Handlers usados (AS2.md e Ghidra; nomes de função não criados)

- 0x40, 0x44, 0x48: visuais (sprites de lado e sequência); ver AS2.md.
- 0x41: alvo aleatório (bot + peça). 0x42: rolagem de valor e efeito pelo ROS (`Fight_RollPartValue` 0x0803769C,
  `Fight_ComputeEffectValueFromRos` 0x080376F4).
- 0x43: soma o campo +4 dos bots ativos do lado (provável). 0x45: incrementa contador de uso (confirmado).
- 0x46: soma armadura máxima − atual (provável). 0x49: metade da armadura do bot ativo (confirmado no disasm).
- 0x4C: armadura da peça alvo para fight+0xEC (confirmado). 0x4D: sorteio (provável).
- 0x50: dano em peça aleatória do adversário (HN2, confirmado). 0x51: estado no próprio bot (4..8) (HN2).
- 0x52: bônus de suporte do time (HN2). 0x53: marca de reação no próprio bot (HN2). 0x54: estado em todos os
  adversários (HN2). 0x55..0x5A: time (proteção, cura, reparo, restaura, gauge, técnica sem dano) (provável; AS2).
- 0x5B..0x6A: wrappers BC..BR, sem efeito lido (pendente).
- 0x01: escreve 8 em 0x03002B8C e 0x40 em 0x0300137C (literal pool 0x080338EC/0x080338F0). Confirmado no disasm.
  Não é o fim da ação dentro da batalha pelo que se sabe: escrever cena 8 é saída do combate (ver AS2 op 0x3F).
  Contradiz a hipótese de fim de segmento. Pendente: testar em runtime o que 0x01 faz no meio do roteiro.

## 5. Efeito por id (resumo)

- Dano em adversário: 01 (50), 02 e 03 (alvo + rolagem; sem 0x50 visto no segmento).
- Estado no próprio bot: 04..09 (51). Estado em adversários: 0c, 0e, 10, 12 (54).
- Suporte do time: 0a (52), 0d, 0f, 11, 13 (55 proteção), 15 (56 cura), 1a (57 reparo), 1c (58 restaura), 16 (59 gauge).
- Sem dano próprio: 17 (5A). Pendente: 14, 18, 19, 1b (49 metade da armadura, efeito no próprio bot), 1d, 1e..33.
- Os ids 1e..31 e 33 usam ops BE..BR ainda não lidos: efeito PENDENTE.

## 6. Quem zera B+0xC1 (medidor)

- `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction` 0x08031918 (decompile desta rodada): grava
  `F+0x164 = 1`, `F+0xA8 = B+0xA8 + 0x50` (QA4; antes escrito como B+0xA8, errado), zera `B+0xE0` e `B+0xE1`, `GM = 9`, `GS = 0`. NÃO escreve B+0xC1.
- Gate `Fight_TrySelectMedaforceAction` 0x080155A8 só lê B+0xC1 (== 0x50).
- `Fight_AddMedaforceChargeFromAction` 0x0803628C só soma (teto 0x50).
- Resultado: quem zera B+0xC1 ao usar ainda NÃO foi achado. Próximo passo: achar escritas em `0x03000590 + bot*0x100 + 0xC1`
  (= 0x03000651 para bot 0) no Ghidra e nos roteiros (op 0x01 e handlers 0x41..0x6A, em particular 0x49, 0x51 e 0x59).

## 7. Pendências

1. Handlers 0x41..0x6A com efeito completo (AS2 §Pendências) e 0x06..0x35 (AS1).
2. Significado de b1..b5 do descritor; quem lê B+0xA9..0xAD.
3. Escritor de B+0xC1 = 0 ao usar a medaforça.
4. Função do op 0x01 no meio do roteiro (escreve cena 8 e GS 0x40).
5. Ghidra: não foram criadas funções nem renomeações nesta rodada (ver MF2_simbolos.csv).
