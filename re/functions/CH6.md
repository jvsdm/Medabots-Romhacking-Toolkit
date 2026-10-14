# CH6 - revisao de chutes (FS1..FS5)

Escopo: linhas com "chute" em FS1..FS5_simbolos.csv sem confirmado/provavel em RV*/CH*. Eram 4 pendentes (3 ja
revistas por outros agentes, fora do escopo).

## Trocas e achados

- **0x08007DDC**: `PartsGrid_SetSelectorPosByIndex` -> `PartsGrid_SelectNextStateByCursor` (renomeado no Ghidra).
  Achado: nao escreve posicao. Limpa OAM, le byte +0x42 do ctx 0x03000440 (cursor 0..3) e grava em GS
  0x0300137C o valor 8 / 0x18 / 0x28 / 0x38 (proximo estado). Confirmado no efeito.
  Divergencia: mtbe.csv ainda tem o nome antigo e a nota "eixo (X/Y) chute" (nao editado por mim).

- **0x08007834 / 0x08007998** (PartsGrid_SlideInStep / SlideOutStep): mantidos, provavel. Achado: os dois corpos
  sao identicos (mesmos alvos BL, mesmos literais 0x03001780 e 0x0300137C). Sentido in/out so existe pela
  tabela 0x083AC86C (GS 1 vs 4/7/105). Nao ha como distinguir pelo codigo.

- **0x080140B0** (Fight_SceneStep65_TimedSequence): mantido. Confirmado que e a entrada 65 da tabela 0x083AD080
  (ref em 0x083AD184). Nome do efeito segue provavel.

## Em aberto
- Os nomes SlideIn/SlideOut sao de sentido indeterminado; se quiser neutro, renomear para A/B.
- mtbe.csv (linha 0x08007DDC) e FS5_simbolos.csv (linha 0x080140B0 ainda com FUN_) precisam de atualizacao pelo dono
  do arquivo. Nao edito mtbe.csv, conforme regra.
