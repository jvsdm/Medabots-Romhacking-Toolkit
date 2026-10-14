# RV9: revisão de nomes (60 endereços de Fight_/FightPreview_)

Feitos: 60 de 60 decompilados (CSV `RV9_simbolos.csv`). Confirmados: 56. Provaveis: 2. Trocas de nome: 2 (renomeadas no Ghidra).

## Trocas de nome (Ghidra e CSV)
- 0x08036D64: FightPreview_IsPartTypeFlagSet -> Fight_IsPartStatusProtected (antes: FightPreview_IsPartTypeFlagSet). Le o flag de protecao bot+0xCF por tipo de status (combate, nao tela).
- 0x08036E64: Fight_ApplyPartStatus -> Fight_SetPendingPartStatus (antes: Fight_ApplyPartStatus). So grava o registro pendente bot+0xC9..0xCC.

## Achados
- 0x08036270: o Ghidra mostra o nome com sufixo `_08036270`; a lista e o comportamento batem com `Fight_IncrementActionUsageCounter`. Nao renomeado (so o sufixo do Ghidra fica).
- 0x08035A70 (`Fight_TargetHasAction34ReactionAffinity`): confere acao 0x34 + flag bot+0xC0 bit1; "Reaction" e chute do nome.
- 0x08037230 (`FightPreview_IsPartSlotEdge`): retorna 1 se armadura atual = 0 ou = maxima. Nome mantido, mas vago.
