# MF8: última leva dos efeitos de Medaforce (2026-10-18)

Fontes: MF6 §2/§5, MF4 (efeito dos opcodes 0x43..0x6A), AS5 (AU, AX), AS6 §4 (4A), e os roteiros lidos direto da ROM
(`python -I`, tabela de ponteiros 0x083C6CA8[0x50+id], tamanhos por opcode em 0x083C6EB8). Porte:
`pc/game/src/fight/medaforce.lua` (tabela EFFECT). Teste: `pc/game/tests/test_medaforce.lua` (`OK 77`).

Limite desta rodada: **não houve decompile novo no Ghidra**. O que segue vem dos docs MF4/AS5/AS6 (decompile já feito
antes) e dos roteiros da ROM. Os itens marcados "Ghidra pendente" precisam de decompile antes de virar regra.

## 1. Roteiros confirmados na ROM (MF8, python -I)

| id | roteiro (op(bytes)) | leitura |
|----|---------------------|---------|
| 00 | 06 08 1A 1B 1C 40 20 45 21 | AS1 (06..21): sem efeito conhecido, 40 = rolagem de peça; Ghidra pendente |
| 0A | 42 52 45 | suporte do time (AT, op 0x52) |
| 0B | 53 45 | AU_08 = bit 0x02 em bot_ativo+0xC0 (AS5) |
| 15 | 56 45 | AX_02 marca aliados com +0xC9 != 0; AX_06 limpa +0xC9..+0xCC |
| 16 | 42 59 45 | BA_08 (fi+0xEC dividido por partes livres → bot+0x10, teto +0x08) |
| 1A | 42 57 45 | AY_06 reparo de armadura do time |
| 1C | 42 58 45 | AZ_06 restaura peça quebrada do time |
| 1D | 41 42 08 4A 1A 1B 1C 1D 44 20 45 21 | 4A conta peças quebradas aliadas (*10 em fi+0x8C), depois dano 0x44 |
| 1F/21/23/25 | 42 5E 45 | BF: boost no byte escolhido por fi+0x51 (+1/+2/+4/+5) |
| 22 | 42 5F 45 | BG: transfere medidor fi+0x8A → fi+0x89 |
| 24 | 42 60 45 | BH: aliados +EC no byte bot+1 (teto 0x78), adversários −EC |
| 26 | 42 61 45 | BI: bot+0xC4=0x26, +C5=0, +C6=fi+0xE8, +C7=EC |
| 27 | 42 62 21 45 | BJ: −EC na peça 0 (cabeça) dos marcados |
| 29 | 42 63 45 | BK: status Impair (0x2B) nos marcados |
| 2A | 42 64 21 45 | BL: dano em peça aleatória dos marcados |
| 2C | 42 65 21 45 | BM_0F drena C1 e dá dano; **não há BE no roteiro** |
| 2E | 42 67 45 | BO: bot+5 += EC (teto 255); significado de +5 aberto |
| 2F | 68 45 | BP: dano na peça 3 dos marcados, sem rolagem |

## 2. Efeito por id: o que foi portado nesta rodada

| id | tipo | alvo | fórmula / regra | status no porte |
|----|------|------|-----------------|-----------------|
| 0B | reação (self_c0) | próprio bot | bot+0xC0 \|= 0x02 (AU_08, AS5) | portado + teste |
| 15 | cura de status | aliados vivos com +0xC9 != 0 | +0xC9, +0xCA, +0xCB, +0xCC = 0 (AX_06). Nomes de +0xCA/+0xCB/+0xCC conferem com o bloco de status (MF4 0x63) | portado + teste |
| 1D | dano + contagem | aliados (contagem) e 1 adversário vivo (dano) | fi+0x8C = 10 × peças com armadura 0 nos aliados (4A, AS6 §4); depois dano com EC num adversário | portado + teste. Aberto: uso de fi+0x8C no 0x44 e o critério exato de "aliado com bot+0xBA != 0" (usado: aliado vivo) |

## 3. Continuam não portados (com motivo)

- **00 (AS1):** ops 06/08/1A/1B/1C/40/20 não lidos. Ghidra pendente.
- **0A (AT, 0x52):** `support.lua` expõe só `M.perform` de outra ação; o mapeamento para o bônus de apoio do time não foi feito.
- **16 (BA_08):** SplitCountByFree não portado; campos bot+0x08/+0x10 sem nome confirmado.
- **1A (AY_06) e 1C (AZ_06):** dependem de SplitCountByCapacity / SplitCountByMax e da escolha de peça (AQ/AZ). Só descritos em MF4, sem porte.
- **1F/21/23/25 (BF), 22 (BG), 24 (BH), 26 (BI), 27 (BJ), 29 (BK), 2A (BL), 2F (BP):** todos dependem de **marcas** (+0xD2) que vêm de fora do roteiro. A origem dessas marcas para o 0x2C, 0x27, 0x29, 0x2A e 0x2F continua aberta (o roteiro não chama BE). Sem a origem, o porte iria escolher alvos errados.
- **1E/20 (BE):** a marcação já está portada (`M.markBE`); o status sorteado (BE_RollRandomStatus, código em fi+0xD6+2k) não tem a origem da rolagem.
- **2E (BO, 0x67):** `bot+5 += EC` está confirmado como escrita, mas não se +5 é nível. Escrever no nível seria um efeito enorme; por isso não portado.
- **2C (origem das marcas):** ABERTO. Ver §1 (roteiro sem BE). O porte segue usando todos os adversários vivos, com nota no código.

## 4. Testes

`lua D:/claude-brain/Medabots/pc/game/tests/test_medaforce.lua` → `OK 77` (era 71). Casos novos: bloco 8 (0x0B, 0x15, 0x1D).
Sem emulador: nenhum efeito foi visto no jogo.
