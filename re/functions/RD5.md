# RD5 - revisao de rotulos de dados/tabelas (70 enderecos)

Fonte: `RD5_lista.txt`. Saida: `RD5_simbolos.csv` (70 linhas). Nenhum nome foi trocado: todos descrevem o conteudo
e o tamanho confirmados ou provaveis. Sem uso do Ghidra (todos tinham doc ou mtbe + bytes da ROM).

## Trocas de kind (16)
- `ptr` -> `table` (13): gTextSpecialAttackDesc, gPartNames, gTextPartCodeNames, gTextStatDescriptions,
  gTechniqueNames, gTextLegTypes, gTextBattlefieldNames, gEncounterTemplatePtrs, gTextCharacterNames,
  gTextMenuLabels, gTextStatusStrings, gTextSystemMessages, PartPreviewType_KindVector.
- `data` -> `table` (2): FightPreview_SceneRowPointerTable (132 x 4 B), Fight_AnimSlot_TableA (8 x 4 B).
- `label` -> `table` (1): gEventContinuationTable (137 x 4 B de ponteiros Thumb).

## Achados e conflitos em aberto
- 0x083BDA04 gTextStatDescriptions: a ROM tem 11 ponteiros, mas JT8 diz que o indice 0x13 e usado. Ou o leitor
  usa outra base, ou a tabela e maior. Revisar JT8.
- 0x083D1D78 PartPreviewType_KindVector: DT3 chama de gMoldTable (ponteiros para moldes, registros de 0x18 B). IC2
  chama de vetor kind->struct. Os words sao misturados (0, ROM, RAM). Tamanho nao fechado.
- 0x083D2630 gEventContinuationTable: EV/HC dizem 137 entradas; DT2 reserva 0x400 B (256 slots). Usei 137 x 4 B.
- 0x083D6298 unknown_ChapterLiteralPool: pool de enderecos RAM (0x03005FDF, 0x03006413, 0x030057B1) sem leitor
  conhecido. Nome mantido, com "unknown_" pendente.
- Tamanho nao fechado: gObjectShadowFlagTable, gObjectField0BByPoseTable (indexados por pose).
- gMapPassBlockTable: provavel <=26 x 8 B (limite pelo proximo simbolo; VA leu so idx 0..9).
- gTextPartCodeNames, gTextLegTypes, gTextBattlefieldNames, gTextCharacterNames, gTextMenuLabels,
  gTextStatusStrings: tamanho confirmado pela contagem de ponteiros = mtbe, mas o nome e "provavel" (vem do dump
  do Toolkit, sem leitor visto).
- gTextSystemMessages: a ROM mostra 32 ponteiros (0..31) e zeros em 32..33, confirmando o mtbe (ok).
