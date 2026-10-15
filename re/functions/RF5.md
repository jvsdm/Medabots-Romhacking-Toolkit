# RF5: revisao dos 34 itens de RF5_lista.txt

Feitos: 34/34 (ver RF5_simbolos.csv). Renomeados no Ghidra: 0. Trocas de nome no CSV: 0.

## Trocas por tipo de item
- DADO -> label (kind): 7 itens (0x08031A84, 0x08031AC0, 0x080332EC, 0x080340A4, 0x080340F6, 0x080340F8, 0x080361F6).
  Os bytes nao comecam com PUSH (xx B5); sao meio de funcao (alvos de desvio ou continuacao de chamada). Nao sao tabelas.
- FUNCAO: 27 itens confirmados como funcao (prologo ou decompile). Os 25 wrappers ActionScript_Op0D..Op2F ja tinham nome no Ghidra e
  batem com AS3a/AS3b (cada um chama Fight_DispatchOnSubstateAA_X e depois avanca o cursor se +0xB5). Decompile feito em Op1E e Op2F.

## Achados
- 0x08034E94: Ghidra ainda mostra Fight_LookupActionScriptByte. RV24 propoe Fight_LookupByteViaPtrTable_A8 (antes: Fight_LookupActionScriptByte).
  Mantido o nome atual: a tabela lida e gTechniqueActionScriptPtrTable (0x083C6CA8, TQ) e a funcao busca o byte do roteiro no cursor.
  Se o time preferir o nome de RV24, a troca e um rename no Ghidra.
- 0x08036D64: Ghidra mostra Fight_IsPartStatusProtected. RV24 propoe FightPreview_IsPartTypeFlagSet (antes: Fight_IsPartStatusProtected).
  Mantido o nome atual: os chamadores sao de batalha (AV/BE/I_00, SS2), nao de preview, entao o prefixo FightPreview_ engana.
- 0x080332EC continua sem funcao no Ghidra (decisao de TQ). Nao criada nesta rodada.
- 0x080361F6 e um ponto de continuacao dentro de FightPreview_AdvanceLeftSideSequenceStep, nao o prologo, apesar do que AN.md sugere.

## Em aberto
- Escolher entre o nome atual e o de RV24 nos dois casos acima.
- Os 25 wrappers estao confirmados pela doc (AS3b), mas so 2 foram lidos no decompile nesta rodada.
