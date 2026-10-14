# TR2: Scout, Conceal e suporte de sensor/furtividade: onde guardam estado (GH-TR2, 2026-10-14)

Continuação de TR.md. Só leitura no Ghidra (decompile e xrefs) e ROM; nada salvo; mtbe.csv não editado.
Bloco do bot: `B = 0x03000590 + bot*0x100` (Ghidra). Offset de pesquisa do PC = offset Ghidra − 0x40.
Confiança: C = confirmado (decompile lido nesta rodada) | P = provável | A = aberto.

## Resposta direta

| Pergunta | Scout (36) | Conceal (37) |
|---|---|---|
| Onde fica | Não está no bloco do bot. Fica na struct de luta `fi = 0x03001800`: byte `fi+0x28+8*k` (k = `fi+0x89`), absoluto 0x03001828 + 8k. Conf. C | Byte `bot+0xB9` do DEFENSOR. Absoluto 0x03000649 (bot 0), offset de pesquisa 0x79 (= 0xB9 − 0x40). Conf. C (leitura); label antigo `player_1_conceal_value` |
| Efeito no cálculo | `Fight_ComputeTotalRos` 0x08038BC0 soma o byte ao ROS total do ATACANTE (`E8 += fi.rec[fi+0x89].+0x28`). C | `Fight_ComputeTotalRos` subtrai `defensor.+0xB9` do ROS do atacante (`safe_subtract`). Ou seja, mais Conceal = mais difícil de acertar. C |
| Quem escreve | Não achado (A). Literal 0x03001828 aparece só em `Fight_ComputeTotalRos` (0x08038C7C); a escrita deve ser por base+offset | Zerado por `Fight_ResetBotPartState` 0x08036FA0 (derrota, C). Incremento NÃO achado: a ação 37 (roteiro 0x083C6AA4) começa com opcode 0x31 → `ActionScript_Op31_CallSubstateAA` 0x08034498 → `Fight_DispatchOnSubstateAA_AA_0B_SetStepCode1D` 0x08023718, que só grava fi+0xA6 = 0x1D e fi+0xAA++ (C). O +0xB9 é gravado depois, em outro ponto (A) |
| Quem lê | `Fight_ComputeTotalRos` 0x08038BC0 (C). Único leitor achado | `Fight_ComputeTotalRos` 0x08038BC0, último passo (C). Não usado em `Fight_ApplyEvasionReductionToRos` (lê +0xA3, que é "legs_evasion", não Conceal) |
| Duração | A. Pelo que está lido, o registro muda com a fase (`fi+0x89`, `Fight_UpdatePhaseFromCounter` 0x0801A640, C), mas a limpeza do byte 0 do registro não foi achada | A. Só zera na derrota (`Fight_ResetBotPartState`, C). Não há zeragem por turno/ação no que foi lido. Não confirmado se persiste até a derrota |
| Quando é limpo | A (ver duração) | Derrota (`Fight_ResetBotPartState`, C). Início de ação (`Fight_ResetBotPartFlags` 0x0801B60C) NÃO zera +0xB9 (C) |

Observação: `Fight_DispatchOnSubstateAA_H_01_ChooseActionPath` 0x0801E3C0 zera `+0xB9` (HA2.md), mas ali o bloco é
`0x03001800` (struct de luta, fi+0xB9 = 0x030018B9), não o bot. Não confundir com bot+0xB9.

## Por que não é o bloco do bot (Scout)

- `Fight_ComputeTotalRos` (C): `iVar3 = fi; E8 += byte(fi[0x89]*8 + fi+0x28)`. Ou seja, a fonte é a tabela de 8 B em
  fi+0x28, indexada pela fase. Não há leitura de `bot+0xXX` para Scout.
- Os demais termos desta função: `Fight_AccumRosPart` (peça atual), anti-ar/anti-mar (peça+0x4D × 0x10), bit 0x04 de
  bot+0xD6 (+peça+0x4D), `Fight_AddStrengthenRosBonus` (Strengthen), técnica da peça (+0x49 = 8/9 soma peça+0x4D; = '4' com fi+0x8A+0x2B == '3' soma +0x2C, Team Form), e
  `Fight_ApplyVirusBugRosPenalty` (Bug/Virus). Todos C.

## Achados de suporte (sensor/stealth)

- Conceal: `battle_bot_conceal_value` (renomeado de `player_1_conceal_value`, 0x03000649) é o único estado de Conceal
  achado no bloco do bot. Conf. C na leitura e na subtração; a escrita é A.
- Team support (AT 0x52/0x0B) continua como em TR.md (registro fi+0x2A..0x2D, zerado por T_11 e Op3D). Não é Scout nem Conceal.
- Os bits de bot+0xBC (0x04 e 0x08) são de efeito 0x2E/0x2F, não de Scout/Conceal (TR.md).

## Renomeações feitas no Ghidra (rename_data, confirmadas pela leitura acima)

- 0x03000649: `player_1_conceal_value` -> `battle_bot_conceal_value` (é bot+0xB9 de QUALQUER bot, lido pelo defensor).
- 0x03001828: `fight_support_record_0x28` -> `fight_scout_bonus_table_0x28` (base da tabela de 8 B; byte 0 = bônus de Scout).

## Aberto (A)

1. Escritor de bot+0xB9 (incremento do Conceal). Caminho sugerido: rodar o roteiro 0x083C6AA4 até o fim e ver os opcodes
   seguintes ao 0x31 em `TQ2_fluxo.txt` (o roteiro 0x083C6AA4 tem fluxo linear), ou varrer funções que escrevem em `base+0xB9`.
2. Escritor da tabela fi+0x28+8k (Scout) e forma de limpeza do byte 0.
3. Duração do Conceal entre turnos: se persiste até a derrota (hoje só sabemos a limpeza na derrota).
4. Para replay: no emulador, ler 0x03000649 antes/depois de uma ação 37 (não feito aqui: sem emulador nesta rodada).
