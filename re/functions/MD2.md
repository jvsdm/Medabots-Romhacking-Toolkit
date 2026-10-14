# MD2: significado dos bytes do descritor de Medaforce (GH-MD2, 2026-10-14, PARCIAL)

Leitura estática: ROM original (só leitura, `python -I`), decompile do Ghidra (sem renomear, sem salvar, sem
comentar). Escopo: o descritor `desc6` 0x083B7E54 (6 B por id, ids 0x00..0x33) e o bloco do bot
0x03000590 + bot*0x100, offsets B+0xA8..+0xAD. Continua o MF.md (§2-§3) e o MF2.md (§3).

Escrita (confirmada, decompile de `Fight_SelectMedaforceActionSource` 0x08015668):
`Mem_CopyBytes_Dup(desc6 + scratch[slot]*6, B+0xA8, 6)`. Ou seja, os 6 bytes do descritor vão INTEIROS para
B+0xA8..B+0xAD, e nada mais escreve nesses bytes (nenhum outro escritor achado; ver §5).

## 1. Tabela byte -> significado

| byte (bot) | desc6 | significado | confiança | leitores (Ghidra) |
|---|---|---|---|---|
| B+0xA8 | b0 | id da medaforça. Usado por BR_47 (nome) e BR_49 (script = 0x50 + id, em F+0xA8) | confirmado | `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction` 0x08031918 (lê B+0xA8 e grava F+0xA8 = B+0xA8+0x50); `BR_47_LoadMedaforceName` 0x080318CC |
| B+0xA9 | b1 | **índice de especialidade** (0..7: strike, berserk, shoot, aim_shot, defend, heal, support, interrupt). Escolhe qual nível de especialidade entra na rolagem: `byte(B+0x20 + B[+0xA9])` | confirmado (decompile de RollPart; bate com TSV `+0x20..+0x27` = níveis) | `Fight_RollPartValue` 0x0803769C |
| B+0xAA | b2 | **sem leitor achado**. Valores 0x00..0x50 (0x50 = teto do medidor em 3 ids). Hipóteses não confirmadas: chance (%) ou custo | pendente | nenhum achado (ver §4) |
| B+0xAB | b3 | **valor base da rolagem** (ROS). Soma em `F+0xE8` junto com compat. da medalha, `rand&7` e nível da especialidade | confirmado | `Fight_RollPartValue` 0x0803769C |
| B+0xAC | b4 | **bônus fixo de efeito**. `F+0xEC = (F+0xE8 >> 3) + B[+0xAC]` (se 0, grava 1). É o dano/efeito que vai para `Fight_ApplyDamageToPart` (AR_00 usa F+0xEC) | confirmado (decompile); "dano" é provável | `Fight_ComputeEffectValueFromRos` 0x080376F4 |
| B+0xAD | b5 | **sem leitor achado**. Valores 0..3 (faixa pequena; hipótese: classe ou alcance/alvo, não confirmada) | pendente | nenhum achado (ver §4) |

Fórmula confirmada (uma rolagem = ops 0x42 do roteiro, `ActionScript_Op42_RollPartValueAndEffect` 0x08034708 e
`Fight_DispatchOnSubstateAA_AO_01_RollPartValueForBot` 0x080268AC, que chama as duas funções e depois avança o cursor):

    F+0xE8 = B[+0xAB] + Compat(0x0803576C) + (rand & 7) + B[0x20 + B[+0xA9]]
    F+0xEC = (F+0xE8 >> 3) + B[+0xAC]        (mínimo 1)

Obs.: AO_01 (0x080268AC) também soma 1 em **F+0xAA** (o contador de passo da luta, não B+0xAA). Ver §3.

## 2. Exemplos por id (desc6, bytes 1..5 = b1..b5)

| id | b1 esp. | b2 | b3 base | b4 bônus | b5 | comentário |
|---|---|---|---|---|---|---|
| 00 | 01 (berserk) | 50 | 00 | 14 | 01 | roteiro 06..21 (sem op de rolagem no segmento lido de MF2) |
| 01 | 00 (strike) | 3c | 00 | 1e | 00 | op 0x42 + dano em peça (0x50) |
| 02 | 03 (aim_shot) | 50 | 00 | 0a | 01 | op 0x41 + 0x42 + 0x43 |
| 0a | 04 (defend) | 3c | 1e | 1e | 00 | op 0x52 (suporte do time) |
| 0c | 00 | 32 | 10 | 28 | 00 | op 0x54 (estado em adversários) |
| 14 | 05 (heal) | 46 | 00 | 3c | 01 | caminho com 0x47 (cauda não lida) |
| 1b | 05 (heal) | 50 | 00 | 00 | 01 | op 0x49 (metade da armadura do próprio bot) |
| 17 | 07 (interrupt) | 32 | 00 | 00 | 00 | técnica sem dano (0x5A) |
| 23 | 06 (support) | 28 | 00 | 1e | 02 | id de exemplo do MF.md (`23 06 28 00 1e 02`) |
| 32 | 07 (interrupt) | 3c | 00 | 0a | 01 | roteiro com 0x4D (sorteio) |
| 33 | 07 (interrupt) | 46 | 00 | 50 | 01 | b4 = 0x50 (maior bônus fixo da tabela) |

Padrões observados (só descritivos): b3 é 0 na maioria dos ids com dano simples e 0x08..0x1e nos de suporte;
b4 tem o maior valor (0x3c, 0x50) nos ids de efeito grande. b5 nunca passa de 3.

## 3. Correções ao MF.md e MF2.md

- MF2 §1 e §6 diziam `Fight_AdvanceActionScriptCursor` 0x08034F08 grava `B+0xAA = 0`. Decompile e DM.md dizem
  **F+0xAA** (`fi+0xAA`, o passo da luta). O AO_01 e o AR_00 também incrementam F+0xAA. B+0xAA NÃO é zerado pelo
  cursor. (Reescrever MF2 §1 e §3: o byte b2 do desc6 não tem relação com esse contador.)
- MF2 §3 dizia "b1 classe ou especialidade (hipótese)". Confirmado: é índice de especialidade (+0xA9).
- MF.md §9 hipotetizava "custo = medidor cheio + nível". Não há evidência de que b2 seja custo; o gate BR_14/
  `Fight_TrySelectMedaforceAction` exige B+0xC1 == 0x50 (igualdade), não compara com b2.
- MF2 §4 dizia que op 0x42 era "rolagem de peça" de forma geral. Confirmado só o uso da rolagem por op 0x42;
  ver §5 sobre técnicas normais.

## 4. Quem lê B+0xAA e B+0xAD (pendente)

Leitores de bot+offset passam por literal 0x03000590 (ou 0x080155DC etc.). Foram lidos (decompile), e **não** leem
B+0xAA nem B+0xAD:
- `Fight_DispatchOnSubstateAA_AR_00_DamageRandomPartsOfOpponentTeam` 0x08027044 (op 0x50): usa F+0xEC e F+0xAA++.
- `ActionScript_Op41_PickRandomOpponentTargetPart` 0x080346DC; `Fight_ComputeTotalRos` 0x08038BC0 (ROS normal,
  lê B+0xD6 e B+0xB9, não A8..AD); `Fight_TickActiveBotStatusEffects` 0x08018CC8; `Fight_CommitActiveBotPartFields`
  0x0801B3BE; `Fight_ApplySpecialityDamageFlags` 0x08033B7C; `Fight_ScaleDamageByPartScalar` 0x08035BFC;
  `Fight_DispatchOnSubstateAA_BR_20_NoPartCheckMedaforceFull` 0x08030CFE (lê C1); `Fight_InitializeBotFromTemplate`
  0x08015142 (preenche pesos B+0x20..0x27 e não zera A8..AD).
- Não lidos: os outros ~70 leitores do literal 0x03000590 (ver `get_xrefs_to 0x03000590`), dispatchers AS..BB
  (0x0802793C..0x0802FE50, que usam helpers), e os handlers 0x5B..0x6A.
Próximo passo (para quem for continuar): decompile dos handlers 0x51 (AS, 0x0802793C), 0x59 (BA, 0x0802A0E8) e
0x5B..0x5C (BC/BD), que são os que mais provavelmente usam b2 (chance/custo) e b5 (classe/alcance).

## 5. Técnica normal x medaforça

- Ação normal: `Fight_SelectActionSource` 0x08015564 grava só B+0xDD, B+0xE0, B+0xE1 (não toca A8..AD).
  O índice de roteiro normal (F+0xA8) vem de outra tabela, não de B+0xA8.
- Varredura do roteiro (python -I, só leitura, `scratchpad/md2/scan42.py`): entre os scripts da tabela
  gTechniqueActionScriptPtrTable 0x083C6CA8 (índices 0..0x4F, os distintos) **nenhum** contém opcode 0x42; o 0x42
  aparece só nos roteiros de medaforça (0x51..0x83). Confiança: provável (o varredor para no primeiro 0x01 e usa
  LEN 0x083C6EB8; não validado no emulador, o que é proibido nesta tarefa).
- Conclusão provável: a rolagem (0x42), que é a única leitora de B+0xA9/AB/AC, não aparece em roteiro normal;
  logo, para a ação normal o bloco B+0xA8..+0xAD não entra no cálculo de valor. Se a ação normal usar B+0xA8 como
  índice de roteiro, isso ainda não foi lido (pendente).

## 6. Pendências

1. Leitor de B+0xAA (b2) e de B+0xAD (b5): ver §4. Hipóteses: chance, custo, classe, alcance. Nenhuma confirmada.
2. Confirmar em runtime (proibido nesta tarefa) se b2 é usado.
3. Escritor de B+0xC1 = 0 ao usar a medaforça (fora do escopo desta rodada; BR_49 não escreve C1).
4. Nenhum nome novo foi aplicado no Ghidra: as funções-chave já tinham nome. Símbolos em `MD2_simbolos.csv`.
