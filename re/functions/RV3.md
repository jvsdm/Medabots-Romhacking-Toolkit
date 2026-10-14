# RV3 - revisão de nomes (60 endereços de RV3_lista.txt)

Confirmações ficam só em RV3_simbolos.csv (60 linhas). Trocas e achados:

- Renomeado (Ghidra): 0x080185A8 `Fight_SlotConfirmStartSlide` -> `Fight_SlotConfirmRedrawAndBlink` (antes: Fight_SlotConfirmStartSlide). Não desliza nada: desenha o slot escolhido, limpa 8 OBJ e ativa o blink do painel.
- Renomeado (Ghidra): 0x08015438 `Fight_UnknownStep_08015438` -> `Fight_AdvanceSpecialityCycleForActiveBots` (antes: Fight_UnknownStep_08015438). Ghidra não decompilava (lia ARM); os bytes são Thumb (30 B5 = push {r4,r5,lr}) e o corpo é um laço nos 6 bots com +0x40 != 0 chamando 0x08015414 (Fight_AdvanceSpecialityCycleIndex).
- Achado: 0x08015390 lê o ciclo de especialidade como byte[bot+0x18 + byte(bot+0x3F)] (casos 0..7 = tipo de ação); 0x08015438 avança esse índice para todos os bots ativos.
- Provável, não confirmado: 0x08015924 (ShouldMirrorReactionIndicatorX) e 0x08015738 (LoadHudTileset) mantêm o nome, mas o efeito visual não foi visto no jogo.
- Provável, não confirmado: 0x08018A18 (Fight_ApplyPostBattleMode20PlayerSideState) grava 0x50 em 3 bots do lado do jogador; o offset foi lido como +0xC1 (medidor de medaforça, teto 0x50), não conferido com savestate.
- Nenhum outro nome foi trocado: os demais 56 batem com o que o decompile mostra.
