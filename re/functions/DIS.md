# DIS: técnicas de interrupção e troca ainda sem porte (Metabee EU), 2026-10-17

Fontes: `TQ.md`/`TQ2.md`/`TQ2_fluxo.txt` (roteiros por técnica), `AS2.md`, `AS3b.md`, `AS3c.md`, `AD.md`, `R2.md`,
decompile Ghidra (sem salvar). Porte: `pc/game/src/fight/disrupt.lua`. Testes: `pc/game/tests/test_disrupt.lua` (OK 35).

## 1. Roteiros (ponteiro em `gTechniqueActionScriptPtrTable` 0x083C6CA8, entrada = técnica)

| técnica | entrada no fluxo | setup (alvo) | ops do roteiro | confiança |
|---|---|---|---|---|
| 42 Pushover | 0x083C6AB3 | BySpeciality 0x08031AFC | 0x34 (AD) | confirmado (roteiro) |
| 45 Use Drain | 0x083C6AC0 | RequireUsableHeadAction 0x08032A68 | 0x08 (D), 0x38 (AH) | confirmado (roteiro) |
| 50 Forcedrain | 0x083C6B2C | RequireMedaforceToken 0x08032C08 | ver §4 | confirmado (setup) / efeito aberto |
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
