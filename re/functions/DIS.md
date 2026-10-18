# DIS: técnicas de interrupção e troca ainda sem porte (Metabee EU), 2026-10-17 (rev. DIS2, 2026-10-18)

> **Correções DIS2 (bytes da ROM, lidos com `python -I`):** tec 41 = `06 08 34 01` (AD); tec 42 = `06 08 35 21` (AE, não AD); 43 = 36 (AF), 44 = 37 (AG), 45 = 38 (AH), 46 e 47 = 39 (AI), 50 = **3A** (AJ), 22 = 2B (U). O roteiro de Forcedrain NÃO chama op 0x50 (ver §6). Op 0x08 NÃO tem teste de acerto (ver §7).

Fontes: `TQ.md`/`TQ2.md`/`TQ2_fluxo.txt` (roteiros por técnica), `AS2.md`, `AS3b.md`, `AS3c.md`, `AD.md`, `R2.md`,
decompile Ghidra (sem salvar). Porte: `pc/game/src/fight/disrupt.lua`. Testes: `pc/game/tests/test_disrupt.lua` (OK 35).

## 1. Roteiros (ponteiro em `gTechniqueActionScriptPtrTable` 0x083C6CA8, entrada = técnica)

| técnica | entrada no fluxo | setup (alvo) | ops do roteiro | confiança |
|---|---|---|---|---|
| 41 (AD, Pushover?) | 0x083C6AAF | BySpeciality 0x08031AFC (?) | 0x34 (AD) | confirmado (bytes) / nome a confirmar |
| 42 Pushover? | 0x083C6AB3 | BySpeciality 0x08031AFC (?) | 0x35 (AE) + 0x21 | corrigido DIS2 (antes dizia 0x34) |
| 45 Use Drain | 0x083C6AC0 | RequireUsableHeadAction 0x08032A68 | 0x08 (D), 0x38 (AH) | confirmado (roteiro) |
| 50 Forcedrain | 0x083C6B2C | RequireMedaforceToken 0x08032C08 | 0x3A (AJ 0x08025AF8) | confirmado (bytes); efeito aberto (§6) |
| 55 Change | 0x083C6B88 | nenhuma (0x08000840) | 0x3F (AN, cena) | confirmado (roteiro) |
| 56..59 ItrpChange / AttkChange / DEF Change / HealChange | 0x083C6B8A / 8C / 8E / 90 | nenhuma | não lidos | aberto |
| 43 Impair / 44 Confusion / 46 No Escape / 47 No Defense | 0x083C6AB8 / ABC / AC4 / AC8 | RequireNoTech16AndClearPending 0x080328B8 (43: 0x08032DA0 peça) | 0x08 (D) e depois 0x36 / 0x37 / 0x39 (43/44/46) | confirmado (roteiro) |

Todos os roteiros de interrupção começam com `ActionScript_Op08_DispatchOnSubstateAA_D` (0x08033A94): é o
passo que pode ter o teste de acerto, mas o corpo de `..._D` NÃO foi lido (ver §3).

## 2. Efeitos lidos

- **Pushover (42), confirmado na escrita:** `Fight_DispatchOnSubstateAA_AD_16_SetStepCode2D` 0x08024078 grava
  `messageCode = 0x2D`. A6.md: 0x2D = "absorveu tempo de preparação do inimigo". Efeito = pending 0x2D no
  alvo (`bot.pending.type = 45`), que faz a próxima ação do alvo não sair. Duração (1 ação) e a checagem na IA:
  **provável / ABERTO** (ai.lua ainda não lê o tipo 45).
- **Change (55..59), confirmado no decompile de AN_06** `Fight_DispatchOnSubstateAA_AN_06_ApplyRandomPartReplacementEffect`
  0x08026678: a família é `techniqueId − 0x37` (0..4) da peça `parts[partIndex]` do **próprio** ator; chama
  `Fight_SelectReplacementPartIdByIconFamily` 0x08037578 (família, peça); copia o registro de 0x18 B
  (`Mem_CopyBytes`), `GetPartStats`, roda as fases da peça e seleciona a fonte de ação (a peça nova **age**).
  O sorteio dentro de 0x08037578 usa uma tabela por lado e máscara do campo +0x8C (R2.md): **tabela não
  decodificada (ABERTO)**. No porte: `tables.replacement[family]` (lista de peças) ou função(família, k).
  Retorno: `act_as = {technique = nova}` para o battle.lua executar; NÃO rodar o roteiro 0x3F no porte.
- **Use Drain (45):** o roteiro chama AH (0x0802518C), o mesmo despachante de formação/divisão (HA8). O contador
  `Fight_IncrementActionUsageCounter` 0x08036270 (op 0x45) não está no roteiro de 45 e incrementa
  `sideRecords[targetSlot].field_0x1`, não usos de peça. Quantidade e alvo exatos: **ABERTO (chute)**: porte
  tira 1 uso da cabeça do alvo e soma à cabeça do ator (`tables.use_drain_amount`).
- **Forcedrain (50):** o setup exige token de Medaforce no alvo (RequireMedaforceToken); o roteiro chama op 0x50
  (CallAR → AR 0x08026FD0), que na AS2/HN2 é **dano em peça aleatória**, não dreno de medidor. Conflito com
  REF_MEDAPEDIA ("tira MF"). **ABERTO**: porte passa o medidor do alvo ao ator (quantidade padrão = tudo,
  `tables.forcedrain_amount`), marcado como chute.
- **Impair/Confusion/No Escape/No Defense:** o efeito de status é o que já existe em `status.setPending`
  (43 trava peça, 44 troca alvo, 46/47 bypass em calc.lua). O que faltava é o **teste de acerto**.

## 3. Teste de acerto das Interrupt (41, 43, 44, 46, 47)

Não localizado. O roteiro passa pelo op 0x08 (`..._D`, corpo não lido). No porte, `disrupt.hits` usa a mesma
regra do golpe normal: `calc.totalRos` e depois `calc.evasion` (ROS zero depois da esquiva = erro; No Escape
desliga a esquiva). **Chute (provável só no desenho)**: a rolagem do jogo pode ser outra. Próximo passo:
decompilar `Fight_DispatchOnSubstateAA_D` a partir de 0x08033A94.

## 4. Pendências

- 56, 57, 58, 59 (roteiros): não lidos. Presume-se o mesmo AN_06 com outra família (provável, não confirmado).
- Op 0x3F (AN) grava cena 8 e 0 em 0x0300137C quando `+0xB5` e `+0xAB != 1` (AS2): é a saída do combate. Por
  isso o porte NÃO roda esse roteiro para Change.
- Forcedrain: decidir quantidade e a conciliação com op 0x50 (§2).
- Tabela de peças por família (0x08037578): fonte dos sorteios de Change.

## 5. Tabela de sorteio do Change (DIS2, 2026-10-18, Ghidra + ROM, confirmado)

- Corpo de `Fight_SelectReplacementPartIdByIconFamily` 0x08037578 (param1 = família = techniqueId−0x37, **param2 = índice
  da peça do ator (`partIndex`), NÃO lado do time**; confirmado no decompile de AN_06 0x08026678: `Fight_SelectReplacementPartIdByIconFamily(*pcVar7, *pbVar9)`).
- Chama `randbyte()` e usa o último byte do RNG (ctx 0x03000440+0x8C, ver R2.md). Índice = byte & máscara.
- Ponteiros (literais 0x08037600..0x08037694, lidos da ROM): família 0 → 0x083CEE88 / 0x083CEF08 / 0x083CEF88 (128 B cada, máscara 0x7F);
  famílias 1..4 → 0x083CF008+0x40·k (peça 0,1,2 em +0,+0x40,+0x80 de cada base; bases 0x083CF008, 0x083CF0C8, 0x083CF188, 0x083CF248; 64 B, máscara 0x3F).
- Peça 3 (ou índice fora de 0..2) → retorna 0 (sem tabela no código). **Pendente:** confirmar que índice 3 é a perna.
- Valores são **ids de peça** (`parts[].partId`), não nomes. Exportados em `pc/cache/battle_tables.lua` como `change_table[familia] = {mask, sides[peca] = {ids}}`
  (extrator `pc/extract/extract_battle_tables.py`, função `decode_change_table`). Ligação com o porte: `battle.lua` `replacementList`
  deve ler `change_table` no lugar do chute por especialidade (não editado nesta tarefa).
- Famílias: 0 = Change 55; 1..4 = 56..59 (rótulos ItrpChange/AttkChange/DEF Change/HealChange vindos de §1, nomes do Ghidra não lidos).
- Efeito já confirmado: AN_06 troca a peça do PRÓPRIO ator pelo id sorteado, copia 0x18 B (`Mem_CopyBytes`) e roda as fases da peça.

## 6. Use Drain 45 e Forcedrain 50 (DIS2, parcial)

- Roteiros (bytes): 45 = `06 08 38 01` → op 0x38 = `ActionScript_Op38_CallAH` 0x0803455C → `_AH` 0x0802518C (formação/divisão: AH_Split, AH_Draw, AH_Form, AH_Accum; ver HA8.md).
- 50 = `06 08 3A 01` → op 0x3A = `ActionScript_Op3A_CallAJ` 0x08034594 → `_AJ` 0x08025AF8 (AJ_Swap, AJ_0C/0D_R*, AJ_0E_S, AJ_Rest, AJ_13/14_S; AS3c).
- **Correção:** nenhum roteiro de 45 ou 50 chama op 0x50 (AR 0x08026FD0 é só um despachante de substate). O 0x50 da nota anterior vinha de outra técnica.
- Quantidade de usos tirada/dada (45) e medidor (50): **ABERTO**. Não decompilado: corpo de AH_Accum e de AJ_Swap/AJ_Rest. Próximo passo: decompilar esses passos.

## 7. Teste de acerto das Interrupt 41, 43, 44, 46, 47 (DIS2, parcial)

- Op 0x08 = `ActionScript_Op08_DispatchOnSubstateAA_D` 0x08033A94 → `Fight_DispatchOnSubstateAA_D` 0x0801D40C. Substates:
  00 `CpuFastSet` (0x0801D434), 01 `ClearSlot9AndPlaceSprites` (0x0801D45C), 02 `WaitSlot9ReachTarget` (0x0801D524), `WaitAnimDone` (0x0801D544).
  **Só animação e espera. NÃO é o teste de acerto.**
- Roteiros: 41 → AD (0x34, 0x080344EC wrapper, `_AD` 0x08023C90); 43 → AF (0x36, `_AF` 0x08024998); 44 → AG (0x37, `_AG` 0x08024E04); 46/47 → AI (0x39, `_AI` 0x080257C0).
- `_AF` (0x08024998) é só um despachante por substate; os passos são AF_00 `AccumPartAndCheckTech` 0x080249C0, AF_08 `SetupOverlay` 0x08024A30, AF_0B 0x08024AF4, AF_0C `WaitSlot22` 0x08024BCC, AF_0E 0x08024BF8, AF_0F 0x08024C44, AF_10 0x08024C94, AF_11 0x08024D18, AF_12 0x08024DA8 ...
- A checagem de status ("StatusCheckFamily", AS3c) é o candidato a conter o acerto. **ABERTO**: decompilar AF_00 e a substate que chama o status (AF table em DAT_080249BC). Nada confirmado nesta rodada.

## 8. Imunidade passiva de Stability 22 (DIS2, parcial)

- Roteiro 22 = `06 08 2B 01 2C 01 01 01` → op 0x2B = U (0x080343F0 → `_U` 0x08021E04), sem passos documentados. A ação de Stability (setup 0x08031F2C) escolhe aliados com status pendente; não é a imunidade.
- Leitor passivo (técnica 22 nas peças do defensor antes de aplicar status): **não encontrado**. Os trechos em `status.lua` e `ai.lua` que checam `parts[k].technique == 22` são uma suposição, não leitura da ROM.
