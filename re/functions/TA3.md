# TA3: quem grava a técnica ativa do bot (B+0xA8) (GH-TA3, 2026-10-14, PARCIAL)

Leitura: ROM original (`python -I`, só leitura; ver scratch) + Ghidra (decompile, get_xrefs_to). Ghidra não salvo, não fechado.
Base: `TA.md`, `TA2.md`, `MD2.md`, `MD3.md`. Símbolos: `TA3_simbolos.csv`.
Convenção: fight F = 0x03001800; bot = F+0x87; B = 0x03000590 + bot*0x100; peça k = B + k*0x18 + 0x49 (técnica da peça, byte +0x49 em coordenadas do Ghidra).

## 1. Medaforça: gravador de B+0xA8 = CONFIRMADO

`Fight_SelectMedaforceActionSource` 0x08015668 (decompile lido, confirmado):
- literais do pool: 0x080156C0 = 0x03000590 (base do bot), 0x080156C4 = 0x083B7F8C (tabela de medalha, 3 B por medalha),
  0x080156C8 = 0x0300052E (buffer de 3 B, um por slot de medaforça), 0x080156CC = 0x083B7E54 (descritor, 6 B por id).
- passo 1: `Mem_CopyBytes_Dup(0x083B7F8C + (B+1)*3, 0x0300052E, 3)` (escolhe a linha de 3 B da medalha do bot).
- passo 2: `Mem_CopyBytes_Dup(0x083B7E54 + scratch[slot]*6, B+0xA8, 6)` (**grava B+0xA8..+0xAD**).
- passo 3: `Fight_SelectActionSource(bot, slot+4)`.
- Chamada por `Fight_SelectMedaforceSourceStep` 0x08014DEC (quando bot+0xDD > 3, slot = +0xDD-4) e por `Fight_TrySelectMedaforceAction` 0x080155F0.
- Descritores (ROM): id 0 = `00 01 50 00 14 01`, id 1 = `01 00 3C 00 1E 00`, id 2 = `02 03 50 00 0A 01`. Byte 0 = id da medaforça → B+0xA8 = id (0..); depois `BR_49` faz F+0xA8 = B+0xA8 + 0x50. Consistente com MD2.
- Único gravador de B+0xA8 com cópia confirmado nesta rodada. Os bytes 1..5 do descritor não têm leitor confirmado (MD2 §2).

## 2. Ação normal (peça): NÃO grava B+0xA8 (confirmado nos decompiles)

- `Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart` 0x08030A24: grava **F+0xA8** (`F+0xA8 = byte [B+k*0x18+0x49]`, técnica da peça) e F+0xA7 (= k), F+0x164 = 1. Não escreve B+0xA8..0xAD.
- `Fight_SelectActionSource` 0x08015564: grava só bot+0xDD, bot+0xE0, bot+0xE1 (a peça).
- `Fight_DispatchPartPreparation` 0x08031A84 e `Fight_DispatchPartExecution` 0x08031AC0: leem o byte de técnica (+0x49) e chamam, por CallViaR0, a tabela de setup 0x083AEEA0[tec] (preparação) ou a de validação/execução 0x083AEF90[tec] (execução). Não escrevem B+0xA8.
  - Setup 0x083AEEA0: técnicas 2..9 → 0x08031AFC (`Fight_SetupTargetSelectionBySpeciality`, só alvo); 0 e 1 e 10, 11, 12 → stub 0x08000840 (vazio).
  - Validação/execução 0x083AEF90: stub 0x08000840 para a maioria; 0x080332ED (ids 22, 31, 33, 38, 39, 41, 42, 44, 46, 47, 50) = `Fight_ValidatePart_ClearPartByte0F` (escreve part+0x57 = 0). Não escreve A8.
- `Fight_SetPartActionMetadata` 0x08033154: grava part+0x56/+0x57 (alvo e slot). Não A8.
- `Fight_BuildPartActionMetadataForAllParts` 0x08031A3C: laço de 3 peças (prep + exec). Não A8.
- Outros checados sem escrita em A8: `Fight_ResetBotPartState` 0x08036FB0 (zera +0x40, +0x59, +0xB9..+0xBD, +0xC4..+0xCF, +0xDC..+0xE3); `Fight_CopyBotSlotRecords` 0x08014F1E (copia bloco de 0x40 → gObjectSlots); `Fight_CopyBotIdentityRecords` 0x08014F6E (8 B → bot+0); `Fight_SelectActionsForBotGroup` 0x0801532C (só chama seleção); `Fight_FindAllyFollowupActionSource` 0x0801D5E0 (lê/escreve F+0xDE/DF, B+0xB5 e F+0xAA); `Fight_DispatchOnSubstateAA_AN_07_ClearSlotsCopyPartBlobs` 0x08026838 (copia 0x0F B de blobs de peça; F+0xA6 = 0x2F).

Conclusão desta rodada: **pelas funções lidas, a ação normal não copia F+0xA8 para B+0xA8**. O único caminho de escrita de B+0xA8 confirmado é o descritor de medaforça (§1). Isso contradiz a premissa de TA.md (§0, "técnica ativa do bot para os efeitos"). Ver §4.

## 3. Leitores de B+0xA8 (relembrados; confirmados nesta rodada)

- `Fight_DispatchOnSubstateAA_AS_SelectVariantByTechnique` 0x08027974: lê `B+0xA8` (literal 0x03000590 + bot*0x100 + 0xA8, confirmado no decompile) e grava F+0x51 e F+0xAA. Idem TA2 §1a.
- TA2 §2 (AV, AW, BF, BE, BR_47, BR_49, G_03, Op0D/Op1E/Op1F/Op2C/Op32/Op39) continuam válidos como "leitores"; nenhum deles é gravador.
- `ActionScript_Op1B_LoadPaletteAndSetTechniqueId` 0x080340F8: **só lê** B+0xA8 (`ldrb`, soma 0x50) e grava r3+0xF9, bot+0x69, bot+0xA9 (TA2 §4). Não grava B+0xA8.

## 4. Pendente e risco (não resolvido por leitura estática)

- Se a ação normal não escreve B+0xA8, os leitores AS/AV/AW/BF/BE (técnicas 4..40) leem o valor residual (medaforça anterior ou template). Ou (a) há um gravador por caminho que a varredura não viu (escrita com base calculada, `stm`/`str` de bloco, ou `Mem_CopyBytes` por outro endereço), ou (b) esses leitores só rodam quando F+0xA8 veio de medaforça. **Não decidido.** Próximo passo: achar quem chama `Fight_DispatchOnSubstateAA_AS_SelectVariantByTechnique` (substate AS) e se o roteiro normal passa por ele; ou varrer as ~100 funções com literal 0x03000590 (lista na seção de notas) buscando `strb`/`str` com +0xA8.
- Consumidor de **fight+0x51** (variante): escrito por 0x08027974 (AS), 0x080284D0/0x0802843C (AV), 0x08028C78 (AW), 0x0802B868 (BF). Consumidor não lido.
- Consumidor de **bot+0xD6** (bits 0x01, 0x02, 0x04, 0x08, 0x10, 0x20 de TA2 §1b): não lido nesta rodada.
- Confirmação em runtime (rec_2, Sword/Missile): não feita (sem emulador nesta tarefa).

## 5. Ponteiros úteis

- Medaforça: `Fight_SelectMedaforceActionSource` 0x08015668; `Fight_SelectMedaforceSourceStep` 0x08014DEC; `Fight_IsMedaforceSelectionBlockedByBattleState` 0x08015648.
- Ação normal: `BR_14` 0x08030A24; `Fight_SelectActionSource` 0x08015564; `Fight_SelectActionSourceStep` 0x08014DA2.
- Tabelas: setup 0x083AEEA0; validação/execução 0x083AEF90; roteiro 0x083C6CA8 (TQ).
