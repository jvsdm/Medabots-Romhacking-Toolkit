# Opcodes do action script 0x36..0x6A (Medabots Metabee EU) — GH-AS2

Continuação de `HN2.md` (AR..AV = 0x50..0x54) e de `DT2.md` (tabela `gActionScriptOpcodeHandlerTable` 0x083AF180).
Leitura na ROM original `Medabots - Metabee (Europe).gba` (só leitura). Disassembly com `arm-none-eabi-objdump`
(`-m arm -M force-thumb`) sobre 0x08034500..0x08034E80, conferido com os nomes do Ghidra.
Convenção: entrada da tabela = `0x08000000 + offset` com bit thumb; a coluna "handler" mostra o endereço sem o bit.

## Ponto importante: os handlers não existem como função no Ghidra

`get_function_by_address` e `decompile_function_by_address` devolvem "No function found" para todos os handlers
0x36..0x6A (e para 0x08034524). Os 27 alvos que eles chamam (0x08024998 .. 0x0802FE50 e os auxiliares) já têm
nome no Ghidra. Por isso não há `FUN_` a renomear neste lote. Nomes sugeridos estão em `AS2_simbolos.csv`
(prefixo `ActionScript_Op`) e não foram aplicados no Ghidra (criar função não estava no pedido).

## Padrão dos handlers

Maioria (0x36..0x3C, 0x3E, 0x3F, 0x50..0x6A): wrapper de 10 instruções

    push {lr}
    bl   <Fight_DispatchOnSubstateAA_XX>   ; despachante por +0xAA (um opcode = uma animação)
    ldr  r0, =0x03001800
    adds r0, #0xB5
    ldrb r0, [r0]                          ; +0xB5 = handler concluído?
    cmp  r0, #0
    beq  +
    bl   0x08034F08                        ; avança o roteiro (ActionScript_Advance)
    pop  {r0}
    bx   r0

Inline (sem chamar despachante): 0x3D, 0x43, 0x46, 0x49, 0x4C; e misto (chamadas auxiliares + avanço): 0x40,
0x41, 0x42, 0x44, 0x45, 0x47, 0x48, 0x4A, 0x4B, 0x4D, 0x4E, 0x4F.

Base do combate: `fight = 0x03001800`; bot = `0x03000590 + bot*0x100` (peças a partir de +0x59, passo 0x18,
ver `R1.md`/`HN2.md`).

## Tabela por opcode

| op | handler | Ghidra hoje | o que faz | confiança |
|----|---------|-------------|-----------|-----------|
| 0x36 | 0x08034524 | sem função | wrapper → `Fight_DispatchOnSubstateAA_AF` (0x08024998) | confirmado (código) |
| 0x37 | 0x08034540 | sem função | wrapper → `..._AG` (0x08024E04); efeito da animação não lido | confirmado (wrapper) / efeito pendente |
| 0x38 | 0x0803455C | sem função | wrapper → `..._AH` (0x0802518C); AH..AP em `HA8.md` | confirmado (wrapper) |
| 0x39 | 0x08034578 | sem função | wrapper → `..._AI` (0x080257C0) | confirmado (wrapper) |
| 0x3A | 0x08034594 | sem função | wrapper → `..._AJ` (0x08025AF8) | confirmado (wrapper) |
| 0x3B | 0x080345B0 | sem função | wrapper → `..._AK` (0x08025F2C) | confirmado (wrapper) |
| 0x3C | 0x080345CC | sem função | wrapper → `..._AL` (0x08025FA0) | confirmado (wrapper) |
| 0x3D | 0x080345E8 | sem função | inline: zera os bytes +2..+5 do registro de suporte do time do bot ativo (`fight+0x2A + 8*fight[0x8A]`), depois avança | confirmado (disasm) |
| 0x3E | 0x08034628 | sem função | wrapper → `..._AM` (0x08026108) | confirmado (wrapper) |
| 0x3F | 0x08034644 | sem função | chama `..._AN` (0x0802645C); se `+0xB5` e `+0xAB != 1`, grava cena 8 em 0x03002B8C e 0 em 0x0300137C (saída do combate); se `+0xAB == 1`, avança | provável (disasm; cena 8 não validada no emulador) |
| 0x40 | 0x08034684 | sem função | `FightPreview_UpdateSingleLayerSideSprites` (0x08035DE8), `FightPreview_AdvanceLeftSideSequenceStep` (0x080361C4) e `Right` (0x08036218) guardam resultados em fight, depois `Fight_DispatchOnSubstateAA_AO` (0x08026884); avança | confirmado (chamadas) / efeito fino provável |
| 0x41 | 0x080346C4 | sem função | `Fight_GetOpponentSideBaseIndex` (0x08032FB8) → `Fight_PickRandomActiveBotInSide` (0x08033090) → `Fight_SelectRandomTargetPartForSelectedBot` (0x08035528) → `Fight_CopySelectedTargetToFightInfo` (0x080355C4); avança. **Escolhe alvo aleatório (bot + peça do lado adversário)** | confirmado (nomes + chamadas) |
| 0x42 | 0x08034708 | sem função | `Fight_RollPartValue` (0x0803769C) e `Fight_ComputeEffectValueFromRos` (0x080376F4); avança. Sorteia valor de peça e calcula efeito pelo ROS | confirmado (nomes); detalhe provável |
| 0x43 | 0x08034728 | sem função | inline: laço de 3 bots do lado escolhido (base por byte da RAM); soma o campo +4 (passo 0x18) nos registros com +15 != 0 e estado 2 ou 3, e acumula em u16 (alvo não identificado); avança | provável |
| 0x44 | 0x0803476C | sem função | `UpdateSingleLayerSideSprites` + `AdvanceLeft/RightSideSequenceStep` (como 0x40) + `Fight_DispatchOnSubstateAA_AP` (0x08026CF4); avança | confirmado (chamadas) |
| 0x45 | 0x080347AC | sem função | `Fight_IncrementActionUsageCounter` (0x08036270); avança | confirmado (nome + chamada) |
| 0x46 | 0x080347BC | sem função | inline: soma, sobre as 4 peças do bot de índice `byte` (ver `R1.md`), `armadura máx − atual` e acumula em u16 de fight; avança | provável (disasm; leitura de `[+0]`/`[+13]` confirmada) |
| 0x47 | 0x080347F8 | sem função | chamadas `UpdateSingleLayerSideSprites`, `AdvanceLeft/RightSideSequenceStep` e `Fight_CopySelectedTargetToFightInfo`; disasm truncado (60 instruções) | **pendente** (cauda não lida) |
| 0x48 | 0x080348D0 | sem função | `UpdateSingleLayerSideSprites` + `AdvanceLeft/RightSideSequenceStep` + `Fight_DispatchOnSubstateAA_AQ` (0x08026ED0); avança | confirmado (chamadas) |
| 0x49 | 0x08034910 | sem função | inline: para as 4 peças do bot ativo (`fight+0x87`): `fight+0xD6+2k = armadura_k >> 1` e `fight+0xEC += armadura_k >> 1` (metade da armadura e soma); avança | confirmado (disasm) |
| 0x4A | 0x0803496C | sem função | `Fight_GetAllySideBaseIndex` (0x08032FC4) + laço inline de 45 instruções; avança | **pendente** (laço não decodificado) |
| 0x4B | 0x080349D8 | sem função | `Fight_GetOpponentSideBaseIndex` + `Fight_FindTargetBotWithPart` (0x08037798) + `Fight_PickRandomActiveBotInSide` + `Fight_SelectRandomTargetPartForSelectedBot` + `Fight_CopySelectedTargetToFightInfo`; avança. Parecido com 0x41, mas com busca de bot que tenha a peça | provável (nomes) |
| 0x4C | 0x08034A68 | sem função | inline: `fight+0xEC = armadura da peça fight[0x8B] do bot fight[0x88]` (u16); avança | confirmado (disasm) |
| 0x4D | 0x08034AA0 | sem função | chama `randbyte` (0x080424B8) duas vezes (sorteio); avança | provável (sorteio; efeito do resultado não lido) |
| 0x4E | 0x08034AF4 | sem função | `Fight_GetOpponentSideBaseIndex` + `Fight_FindActiveBotInSideWithGroup` (0x08037830) + escolha aleatória de bot e peça (como 0x41); avança | provável (nomes) |
| 0x4F | 0x08034B68 | sem função | `Fight_GetOpponentSideBaseIndex` + `Fight_FindActiveBotInSideWithGroup`; avança | provável (nomes) |
| 0x50 | 0x08034BA0 | sem função | wrapper → `Fight_DispatchOnSubstateAA_AR` (0x08026FD0): dano em peça aleatória dos adversários | confirmado (`HN2.md`) |
| 0x51 | 0x08034BBC | sem função | wrapper → `..._AS` (0x0802793C): estado por técnica no próprio bot (4..8) | confirmado (`HN2.md`) |
| 0x52 | 0x08034BD8 | sem função | wrapper → `..._AT` (0x08027F5C): bônus de suporte do time | confirmado (`HN2.md`) |
| 0x53 | 0x08034BF4 | sem função | wrapper → `..._AU` (0x08028248): marca de reação no próprio bot | confirmado (`HN2.md`) |
| 0x54 | 0x08034C10 | sem função | wrapper → `..._AV` (0x08028404): estado em todos os adversários | confirmado (`HN2.md`) |
| 0x55 | 0x08034C2C | sem função | wrapper → `..._AW` (0x08028C40) — time: proteção contra status (`HANDOFF_DISCOVERY`) | provável (nome na nota) |
| 0x56 | 0x08034C48 | sem função | wrapper → `..._AX` (0x080294B4) — time (curar) | provável |
| 0x57 | 0x08034C64 | sem função | wrapper → `..._AY` (0x08029840) — time (reparar armadura) | provável |
| 0x58 | 0x08034C80 | sem função | wrapper → `..._AZ` (0x08029E0C) — time (restaurar peças) | provável |
| 0x59 | 0x08034C9C | sem função | wrapper → `..._BA` (0x0802A0E8) — time (gauge) | provável |
| 0x5A | 0x08034CB8 | sem função | wrapper → `..._BB` (0x0802A6AC) — técnica sem dano | provável |
| 0x5B | 0x08034CD4 | sem função | wrapper → `..._BC` (0x0802AAD0) | pendente |
| 0x5C | 0x08034CF0 | sem função | wrapper → `..._BD` (0x0802ACCC) | pendente |
| 0x5D | 0x08034D0C | sem função | wrapper → `..._BE` (0x0802B000) | pendente |
| 0x5E | 0x08034D28 | sem função | wrapper → `..._BF` (0x0802B7E8) | pendente |
| 0x5F | 0x08034D44 | sem função | wrapper → `..._BG` (0x0802BD6C) | pendente |
| 0x60 | 0x08034D60 | sem função | wrapper → `..._BH` (0x0802C2AC) | pendente |
| 0x61 | 0x08034D7C | sem função | wrapper → `..._BI` (0x0802C644) (tabela 0x083AE994) | pendente |
| 0x62 | 0x08034D98 | sem função | wrapper → `..._BJ` (0x0802CBE8) (tabela 0x083AE9D4) | pendente |
| 0x63 | 0x08034DB4 | sem função | wrapper → `..._BK` (0x0802D688) (tabela 0x083AEA30) | pendente |
| 0x64 | 0x08034DD0 | sem função | wrapper → `..._BL` (0x0802DCB4) (tabela 0x083AEA88) | pendente |
| 0x65 | 0x08034DEC | sem função | wrapper → `..._BM` (0x0802E424) (tabela 0x083AEB1C) | pendente |
| 0x66 | 0x08034E08 | sem função | wrapper → `..._BN` (0x0802EBC0) (tabela 0x083AEB88) | pendente |
| 0x67 | 0x08034E24 | sem função | wrapper → `..._BO` (0x0802F030) (tabela 0x083AEBC4) | pendente |
| 0x68 | 0x08034E40 | sem função | wrapper → `..._BP` (0x0802F628) (tabela 0x083AEC04) | pendente |
| 0x69 | 0x08034E5C | sem função | wrapper → `..._BQ` (0x0802F990) (tabela 0x083AEC58) | pendente |
| 0x6A | 0x08034E78 | sem função | wrapper → `..._BR` (0x0802FE50) (tabela 0x083AEC98) | confirmado (wrapper; cauda truncada na leitura) |

Os despachantes AF..BR (0x08024998..0x0802FE50) estão no Ghidra com nome `Fight_DispatchOnSubstateAA_XX`
(de R1/R1a). Só AR..AV têm efeito documentado (`HN2.md`); AH..AP em `HA8.md`.

## Pendências

- Efeito de AF, AG, AM..AQ fora de HA8 e de AW..BR: não decompilado neste lote (cada despachante tem 30 a 60
  passos).
- Opcodes 0x47 (cauda truncada) e 0x4A (laço inline): precisam de disasm mais longo.
- Tabela de scripts por técnica (ponteiros por técnica): **não localizada neste lote**. Os bytes depois de
  `0x083AF380` (`DT2.md`) são o caminho mais provável; não foi lido.
- Nenhum endereço novo de função foi criado no Ghidra (não há função nos handlers; criar não estava no pedido).
