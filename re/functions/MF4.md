# MF4: efeito dos opcodes de Medaforce pendentes (GH-MF4, 2026-10-14, leitura estatica completa)

Leitura estática: decompile do Ghidra (sem salvar, sem fechar). Continua MF3.md (§Pendências 1 e 3).
Nomes e comentários em `MF4_simbolos.csv`. Convenção: `bot` = bloco 0x03000590 + bot*0x100 (offsets Ghidra);
peça k = bot+0x4C+0x18*k (+0 armadura máx, +0xD armadura atual); `fi` = 0x03001800 (fight_information);
`fi+0x87` = índice do bot ativo; `fi+0xEC` = valor de efeito (dano) que o despachante de dano usa.

## Tabela op -> efeito -> prova

| op | efeito (estado que muda) | prova | conf. |
|----|--------------------------|-------|-------|
| 43 | `fi+0xEC += bot_ativo.peça_k+2` para cada peça k (3 iterações) com atual (+0xD) != 0 e byte peça_k-2 ∈ {2,3}. Só soma ao valor de efeito, não mexe em peça. | decompile `ActionScript_Op43_SumSideField4` 0x08034728 | leitura confirmada; significado do byte −2 (estado 2/3) provável |
| 46 | `fi+0xEC += Σ(max − atual)` das 4 peças do bot `fi+0x87` (dano já sofrido pelo bot indicado). Confirma a fórmula de MF3. | decompile `ActionScript_Op46_SumPartDamageOfTargetBot` 0x080347BC | confirmado |
| 47 | Não causa dano nem status. Avança o cursor das sprites laterais, `fi+0xF6=3`, grava o alvo (`fi+0x87`) e alterna o bit 2 de `fi+0xF8`, que liga timer de 0x28 quadros em `fi+0x114`/`fi+0xFF` (ou no lado oposto). | decompile 0x080347F8 | confirmado (campos visuais); "contador bot+0xE0" continua sem significado claro |
| 4A | `fi+0x8C += 10` para cada peça com atual (+0xD) == 0 dos bots aliados com `bot+0xBA != 0` (3 bots × 4 peças). Conta peças quebradas do lado aliado. Correção: AS3c dizia "+0"; o byte lido é +0xD. | decompile `ActionScript_Op4A_AllySideLoop` 0x0803496C | confirmado no byte (+0xD); destino `fi+0x8C` e o índice do bot no laço: provável |
| 4B | Escolhe bot adversário que tenha peça (`Fight_FindTargetBotWithPart`); se não achar, bot aleatório do lado e `fi+0xB8=2`. Grava `bot_ativo+0xE0 = índice+1` e `+0xE1 = parte` (de `+0xDE` do bot achado). Copia o alvo. Não dá dano. | decompile 0x080349D8 | confirmado (escolha de alvo) |
| 4E | Escolhe bot adversário com o mesmo grupo do ativo (`Fight_FindLiveSlotMatchingField91`); se não achar, bot aleatório. Grava `+0xE0 = índice+1`, peça aleatória e copia o alvo. Não dá dano. | decompile 0x08034AF4 | confirmado (escolha); "grupo" = campo 0x91, nome provável |
| 4F | Se não há bot vivo com o grupo do ativo no lado adversário: `fi+0xEC = 10` (valor fixo). Se há, nada. Não escolhe alvo. | decompile 0x08034B68 | confirmado |

## Lote 2: 0x48, 0x55..0x58 (despachantes AQ, AW, AX, AY, AZ; decompile do passo que muda estado)

| op | efeito (estado que muda) | prova | conf. |
|----|--------------------------|-------|-------|
| 48 | Sprites laterais e depois `Fight_DispatchOnSubstateAA_AQ_02_RepairActorPart`: divide `fi+0xEC` entre as peças do ATOR (`fi+0x87`, faixa `fi+0xD0`) por capacidade e reparo; `fi+0xAA++`. Ou seja, o 0x48 reparo do próprio ator usando o valor de efeito. | decompile AQ_02 0x08026F34 (chama `FightPreview_SplitCountByCapacity`) | provável (nome do helper); AQ_0D não lido |
| 55 | Proteção de status do TIME. Para cada um dos 3 bots aliados marcados (`bot+0xD2 != 0`), lê o tipo de proteção `fi+0x51` (0..3, vindo de AW_00): grava bit em `bot+0xCF` (0→bit 1, 1→bit 8, 2→bit 2, 3→bit 4). Se o status atual da peça (`bot+0xC9`) for do grupo daquele bit, limpa os campos de status (`FightPreview_ClearPartFields`). Não mexe em vida. | decompile AW_09 0x08029354 | confirmado (bits em +0xCF; mesmos de SS2) |
| 56 | **Cura de STATUS do time** (não de armadura, corrige MF3 "cura do time"). Para cada aliado marcado (`+0xD2`), limpa os campos de status (`FightPreview_ClearPartFields`); `fi+0xAA++`. AX_02 só marca os aliados com status. | decompile AX_06 0x080297EC | confirmado (ação); "campos" = status, provável |
| 57 | **Reparo de ARMADURA do time**. Para cada aliado marcado: repete até 4 vezes, pega a peça mais danificada reparável (`Fight_PickMostDamagedRepairablePart1Based`), soma ao atual (+0xD da peça) a parcela de `SplitCountByCapacity` do valor de efeito (provável `fi+0xEC`), acumula em `fi+0xD6+2k`. | decompile AY_06 0x08029B84 | confirmado (estrutura); origem do valor provável |
| 58 | **Restaura peças do TIME** (revive). Para cada aliado marcado: pega peça quebrada/faltando (`Fight_PickRandomBrokenOrMissingPart1Based`), dá a ela a parcela do valor (`SplitCountByMax`) no atual (+0xD), copia stats se flag, até 4 vezes. Depois atualiza sprites. | decompile AZ_06 0x08029FE0 | confirmado (estrutura); origem do valor provável |

Correção MF3 nesta tabela: 0x56 é cura de status e 0x57 é reparo de armadura (AY); 0x58 é restauração de peças (AZ).

## Lote 3: 0x59, 0x5B, 0x5C, 0x5D, 0x5E (despachantes BA, BC, BD, BE, BF)

| op | efeito (estado que muda) | prova | conf. |
|----|--------------------------|-------|-------|
| 59 | "Gauge do time": para cada aliado marcado (`+0xD2`), divide `fi+0xEC` entre as partes livres (`SplitCountByFree`) e soma o valor ao campo do bot `+0x10`, com teto em `+0x08`. Não é o medidor C1. Campos sem nome confirmado. | decompile BA_08 0x0802A534 | confirmado (fluxo); campos +0x08/+0x10 provável |
| 5B | `bot_ativo+0xD1 += fi+0xEC` (teto 255). Sem dano direto. Nome do Ghidra "AddValueToBotD1". | decompile BC 0x0802AC7C | confirmado (escrita); significado do byte provável |
| 5C | `bot_ativo+0xD0 += fi+0xEC` (teto 255). Mesmo padrão do 5B. | decompile BD 0x0802AFB0 | confirmado (escrita); significado provável |
| 5D | **Aplica status nos alvos marcados** (`+0xD2`): pelo código sorteado em `fi+0xD6+2k` (BE_RollRandomStatus): 0 → status pendente 0x0D (Bug), 1 → 0x11 (Wave), 2 → 0x13 (Fire), 3 → 0x10 (Freeze), via `Fight_SetPendingPartStatus` com `fi+0xE8` e `fi+0xEC`. 4 → 0x2B (Impair) grava direto em +0xC9/CA/CB/CC. 5 → 0x2C (Confusion). 6 → 0x2E e 7 → 0x2F (buffs de esquiva/defesa, SS2). `bot+0xAA` do fi++. | decompile BE 0x0802B5AC | confirmado (tipos e ramos); sorteio em BE_RollRandomStatus não lido |
| 5E | Boost de atributo do bot marcado: `fi+0xEC` (teto 255) soma a um byte escolhido por `fi+0x51` (0..3 → bot+0x4D / +0x4E / +0x50 / +0x51, que caem nos bytes +1/+2/+4/+5 do bloco de peças). Depois copia/recalcula stats (`BackupHalvedStats`, `CopyStatsIfFlagSet`). `fi+0x51` vem de BF_SelectAlliesAndStat (MD3). | decompile BF 0x0802BC60 | confirmado (ramos); significado do atributo provável |

Nota 5D: alvos marcados vêm de BE_SelectTargets (0x0802B184, não lido em detalhe).

## Lote 4: 0x5F, 0x60, 0x62, 0x63, 0x64 (despachantes BG, BH, BJ, BK, BL)

| op | efeito (estado que muda) | prova | conf. |
|----|--------------------------|-------|-------|
| 5F | **Transfere medidor** entre dois bots: `fi+0xEC` vai do registro do bot `fi+0x8A` (subtrai, piso 0) para o registro do bot `fi+0x89` (soma, teto 0xA0). Registros de 8 bytes a partir de `fi+0x28`. Também efeitos visuais (flash/movimento). **Correção:** MF3 dizia só "flash/movimento"; a tabela tem `BG_TransferGauge`. | decompile BG_TransferGauge 0x0802C24C | confirmado (escrita); registro de 8 bytes = medidor (provável) |
| 60 | **Impulso (boost)**: aliados marcados (`+0xD2`) ganham `fi+0xEC` no byte `bot+0x01` (teto 0x78); depois os adversários marcados (`fi+0xD6+2k != 0`) perdem `fi+0xEC` nesse byte (piso 0). O byte `bot+0x01` é o mesmo que o 5B soma (teto 255). | decompile BH_BoostAlliesDrainOpponents 0x0802C588 | confirmado (escrita); nome do byte "boost" provável |
| 62 | **Dano à peça 0 (provável cabeça)** dos alvos marcados: subtrai `fi+0xD6+2k` do atual (+0xD) da peça 0 (`Fight_SubtractPartArmorClampZero`). Se a peça 0 zera: enfileira cabeça quebrada, marca o alvo (`+0xD2 = 0x80`) e fi+0xAA = 0x10. Senão: `bot+0xBD |= 1` (se bit 4 livre). **Correção:** MF3 dizia "levanta/desprende"; a parte de dano está nesta tabela (BH_CommitArmorCheckHeads). | decompile BH_CommitArmorCheckHeads 0x0802D5BC | confirmado (escrita); "peça 0 = cabeça" provável |
| 63 | **Status Impair (0x2B = 43)** pendente nos alvos marcados: `bot+0xC9 = 0x2B`, `+0xCB = fi+0xE8` (duração, teto 99), `+0xCC = fi+0xEC` (valor, teto 99), `+0xCA` = 2/3/1 conforme o valor do alvo. | decompile BK_0E_SetPendingStatus2B 0x0802DBD8 | confirmado (campos C9/CA/CB/CC; SS) |
| 64 | **Dano em peça aleatória** dos alvos marcados: sorteia peça (`Fight_RollStatusRowRandomEntry`), chama `Fight_ApplyDamageToPart(bot, peça, 200)`, subtrai `fi+0xD6+2k` do atual (+0xD) da peça sorteada; se peça 0 zera, enfileira cabeça quebrada. **Correção:** MF3 dizia "efeito de tela/borda"; a tabela tem `BL_12_DamageRandomPartOfTargets`. | decompile BL_12 0x0802E1C8 | confirmado (escrita de dano); significado do arg 200 não entendido |

## Lote 5: 0x66, 0x67, 0x68, 0x69, 0x6A (despachantes BN, BO, BP, BQ, BR)

| op | efeito (estado que muda) | prova | conf. |
|----|--------------------------|-------|-------|
| 66 | Sem dano. Para cada aliado vivo marcado (`+0xD2`), `bot+0xC0 |= 8`. Esse é o bit que, segundo MG3, faz a carga de medaforça dobrar e faz o BR_26 usar 0x50 (medidor cheio). Id 2D (`66 45 01`) é então um "marca o time para carga cheia" (provável). | decompile BN_08 0x0802EFD0 | confirmado (escrita); efeito no medidor = MG3 |
| 67 | Sem dano. `bot_ativo+0x05 += fi+0xEC` (teto 255). **Atenção:** MF3 chama o byte +5 de "nível do bot"; se esta soma for ao nível, é um efeito grande. Não verificado no emulador. | decompile BO_09 0x0802F5D8 | confirmado (escrita); nome do campo +5 = pendente |
| 68 | **Dano na peça 3** dos alvos marcados (`Fight_ApplyDamageToPart(bot, 3, fi+0xD6+2k)`), depois subtrai o valor do atual (+0xD) da peça 3 e grava `fi+0xD6+2k = fi+0xC8`. Senão zera o alvo sem valor. | decompile BP_08 0x0802F8E4 | confirmado (dano; peça 3 fixa no código); nome da peça (perna?) não confirmado |
| 69 | Sem dano. `bot_ativo+0xC0 |= 1` (bit de reação 0). Depois fi+0xAA++. | decompile BQ_09 0x0802FE0C | confirmado (escrita); efeito do bit = leitor não achado |
| 6A | Sem dano. `bot_ativo+0xC0 |= 4` (bit de reação 2). | decompile BR_07 0x0803059C | confirmado (escrita); leitor do bit = não achado |

Nota: 0x69 e 0x6A escrevem em bot+0xC0 (bits 1 e 4); 0x66 escreve bit 8 (carga dobrada). BR_07 e BQ_09 são as mesmas rotinas de "reação" de AS2/AS3c.

## Lote 6: 0x5A, 0x61, 0x65 (BB, BI, BM)

| op | efeito (estado que muda) | prova | conf. |
|----|--------------------------|-------|-------|
| 5A | **Põe o medidor de técnica em 20 nos adversários vivos**: para cada oponente com `+0x40 != 0` e cabeça (+0x59) != 0: marca alvo (`+0xD2 = 1`), `bot+0xBD |= 4`, `bot+0xBB = 0x14` (20), zera `+0xD7`/`+0xDA`. Cria slots de efeito. Mostra que o medidor de técnica é `bot+0xBB` (teto 0x1F, README), diferente do medidor de medaforça `+0xC1` (teto 0x50). | decompile BB_MarkOpponentsSetGauge 0x0802A824 | confirmado (escrita 0x14 em +0xBB) |
| 61 | **Efeito secundário de carga de boost** nos alvos marcados: `bot+0xC4 = 0x26`, `+0xC5 = 0`, `+0xC6 = fi+0xE8` (teto 99, duração), `+0xC7 = fi+0xEC` (teto 99, valor). Helper `Fight_CommitBoostChargeSecondaryEffectToMarkedTargets` 0x0802CB34. +0xC4 é o campo do efeito secundário (SS: AutoRecover também em +0xC4). | decompile 0x0802CB34 (BI 0x0802C644 chama) | confirmado (escrita); significado de 0x26 = provável |
| 65 | Sem novo efeito a documentar: o BM completo para id 2C usa `BM_0F_DrainMedaforceAsDamage` (zera `bot+0xC1` dos adversários com `+0xD2` e converte em dano; MG3). **Passo exato do 2C ainda não lido** (MF3 pendência 1). | MG3 (decompile 0x0802EAD8) | confirmado (BM_0F); passo do 2C aberto |

## Medidor de Medaforce: b2 (bot+0xAA) e bot+0xC1 (fechado com a cadeia completa)

Cadeia (todos confirmados no decompile):
1. `Fight_DispatchOnSubstateAA_BR_45_SetupMedaforceSlots` 0x08031704 (GS 0x35): `fi+0x51 = bot_ativo+0xAA` (= b2 do
   descritor). Também monta os slots 0x21/0x22 da animação. É o único leitor de bot+0xAA.
2. `Fight_DispatchOnSubstateAA_BR_46_WaitMedaforceAnim` 0x0803185C (GS 0x36), por quadro: chama
   `Fight_StepBotVisualDown(fi+0x87)` e roda os slots até o byte de fim (-1).
3. `Fight_StepBotVisualDown` 0x0801AFB8: se `fi+0x51 != 0` e `bot+0xC1 != 0`, faz `bot+0xC1 -= 1` e `fi+0x51 -= 1`
   (redesenha o medidor). Esta é a ESCRITA em bot+0xC1 que consome o medidor.
4. Resultado: o medidor cai `min(b2, bot+0xC1)`, um ponto por quadro, enquanto a animação dura. Se a animação terminar
   antes, o consumo para antes de b2.
- BR_26 (0x08030DB4, GS 0x16) também grava `fi+0x51` (0x28, ou 0x50 com `bot+0xC0 & 8`). Como BR_45 vem depois
  (GS 0x35 > 0x16), o valor que BR_46 consome é o b2 da medaforça. Ordem BR_20 → BR_26 → BR_45 → BR_46 assumida; não
  verificado que o caminho da medaforça passa por BR_26.
- Outros escritores de bot+0xC1: `Fight_StepBotVisualUp` 0x0801AF84 (soma até 0x50, carga); `BM_0F` 0x0802EAD8 (zera,
  técnica drenar). `StepBotVisualDown` também é chamado por AJ_0E 0x08025D7C.
- Conclusão: b2 é a QUANTIDADE de medidor gasta pela medaforça (custo), descontada do medidor junto com a animação. Não é
  o contador de passos nem a chance. Nota: o medidor de Medaforce é bot+0xC1 (teto 0x50); README cita outro medidor em
  bot+0xBB (teto 0x1F), que é outro recurso e não entra aqui.
- Pendente: confirmar no emulador (medidor 0x50, usar medaforça com b2 conhecido, ler C1 antes/depois). Proibido nesta tarefa.

## Pendências (MF4)

- Todos os ops pendentes da MF3 (43, 46, 47, 48, 4A, 4B, 4E, 4F, 55..6A) têm efeito escrito acima. Os passos de
  cada despachante foram lidos só pelo passo que muda estado; passos de animação não foram lidos.
- Campos sem nome confirmado: bot+0x01, +0x05 (op 0x67, pode ser nível), +0x08/+0x10 (0x59), +0xD0/+0xD1 (0x5B/0x5C),
  +0x4D/+0x4E/+0x50/+0x51 (0x5E), bit de reação +0xC0 bits 1/4 (0x69/0x6A, leitor não achado).
- Peça 0 = cabeça e peça 3 = (perna?) em 0x62/0x68: provável, não confirmado.
- Índice exato do bot no laço do 0x4A e destino de `fi+0x8C`: provável.
- Passo exato do 2C (BM): não lido (MG3 confirma que o BM contém BM_0F).
- b5 (bot+0xAD): sem leitor (MD3), não tratado aqui.
- Emulador (proibido nesta tarefa): consumo do medidor = min(b2, C1), efeito de 0x67 no nível, b2 como custo real.

## Renomeados no Ghidra (in-memory, sem salvar)

- 0x08034728 `ActionScript_Op43_AddActiveBotPartBonusToEC`
- 0x0803496C `ActionScript_Op4A_CountBrokenAllyParts`
- 0x08034B68 `ActionScript_Op4F_SetEffect10IfNoGroupMatch`

## Correções ao MF3 / AS3c

- 0x4A: o byte lido é a armadura ATUAL (+0xD), não +0 (AS3c dizia +0).
- 0x56 é cura de status (não de armadura); 0x57 = reparo de armadura; 0x58 = restaura peças.
- 0x5F tem transferência de medidor (`BG_TransferGauge`); 0x64 é dano em peça aleatória (não efeito de tela);
  0x62 tem dano à peça 0 (não só "levanta/desprende").
- 0x5A põe `bot+0xBB = 20` nos adversários (medidor de técnica), diferente de `+0xC1` (medaforça).
