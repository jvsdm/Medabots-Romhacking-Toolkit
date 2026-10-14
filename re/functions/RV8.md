# RV8 - revisão de nomes (60 endereços, ActionScript/Fight/Util)

Feitos: 60/60 decompilados e gravados em `RV8_simbolos.csv`. Renomeados no Ghidra: 21 (lista abaixo; 22 linhas do CSV trazem `(antes: X)`, a 22ª é 0x08034E94, que manteve o nome).

## Trocas de nome
- 0x08034CD4, 0x08034CF0, 0x08034D0C, 0x08034D28..0x08034DB4, 0x08034DD0..0x08034E5C: `ActionScript_Op5B..Op63` e `Op64..Op69` de `_pendente` para `_CallBC..CallBQ` (wrappers que chamam `Fight_DispatchOnSubstateAA_BC..BQ`, confirmado).
- 0x08034E78: `ActionScript_Op6A_BR` para `ActionScript_Op6A_CallBR` (mesmo padrão).
- 0x08034F68: `Fight_ApplyDamageToPart` para `Fight_SplitDamageByPartArmor` (não subtrai armadura; divide o dano em absorvido e excedente).
- 0x08035234: `Fight_QueueHeadBrokenBotForResolution` para `Fight_QueueBrokenBotForResolution` ("cabeça" não confirmada).
- 0x08035478: `Fight_SelectLowestPriorityTargetBot` para `Fight_SelectOpponentBotByExtremeValue` (mínimo para um lado, máximo para o outro).
- 0x08035740 e 0x08035754: `safe_add` / `safe_subtract` para `Util_AddByteClampToMax` / `Util_SubtractShortClampZero` (prefixo `Util_` novo; antes eram minúsculos).

## Achados
- 0x08034E94 (`Fight_LookupActionScriptByte`) saiu de `chute` para confirmado: lê o byte do roteiro no cursor.
- Wrappers Op51..Op58 (0x08034BBC..0x08034C80): o nome do efeito vem do handler e não foi verificado no corpo; ficaram como `provavel`.
- Op40/44/48 e Op47 (sprites laterais) ficaram `provavel`: o nome descreve o efeito, mas o sufixo AO/AP/AQ não é um verbo.
