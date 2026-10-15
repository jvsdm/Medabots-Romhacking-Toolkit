# NX2 - lista de funcoes reparadas pelo sync (12 enderecos)

Ferramenta: Ghidra MCP (decompile_function_by_address, get_xrefs_to) + varredura da ROM com python -I
(ponteiros addr|1 e ldr-pc). Renomeados 4 no Ghidra; 8 confirmados sem troca. Ver NX2_simbolos.csv.

- 0x08000840 NoOpActionHandler: confirmado, corpo vazio (return); placeholder em ~1191 ponteiros da ROM. Mantido.
- 0x08001278 Gfx_VBlankDmaAlternatingRow (antes FUN_08001278): provavel. Copia 8 hw da linha par/impar por gFrameCount e arma DMA0 para a proxima; ponteiro em 0x08001274 carregado em 0x08001246 (sem funcao no Ghidra). Destino exato em aberto.
- 0x080288C8 Fight_DispatchOnSubstateAA_AV_0D_ApplyBugVirusUnlessMasked: confirmado, teste de protecao (flag 0x0D) e Fight_ApplyPartStatus; chamado pelo despachante 0x08028846 (tabela 0x08028854). Mantido.
- 0x08028908 Fight_DispatchOnSubstateAA_AV_10_ApplyFreezeUnlessMasked: confirmado, mesmo corpo com 0x10. Mantido.
- 0x08028944 Fight_DispatchOnSubstateAA_AV_12_ApplyHoldUnlessMasked: confirmado, mesmo corpo com 0x12. Mantido.
- 0x08028980 Fight_DispatchOnSubstateAA_AV_13_ApplyFireUnlessMasked: confirmado, mesmo corpo com 0x13. Mantido.
- 0x080289BC Fight_DispatchOnSubstateAA_AV_2C_ApplyConfusion: confirmado, aplica 0x2C sem teste de protecao. Mantido.
- 0x08043AE0 Gfx_VBlankBgScrollDmaPingPong (antes Unk_08043AE0): provavel. Grava 8 hw de shadow em BG0HOFS/VOFS (0x04000010..) com fila par/impar e arma DMA0; ponteiro em 0x08043AD8 carregado em 0x08043ABA (sem funcao no Ghidra).
- 0x08043B58 Gfx_VBlankStopDma0AndResetHandler (antes Unk_08043B58): provavel. VBlankWait, zera DMA0CNT_H e grava NoOpActionHandler em 0x03000B98. Chamador nao achado.
- 0x080459A4 CompositePreview_SortOamRangeByY (antes OffsetCompositePreviewTemplateEntry): provavel. Corpo e selection sort de registros de 8 bytes pela chave short em +6. A descricao antiga no mtbe.csv fala em offset de X e nao bate com o corpo: conferir. Chamador nao achado.
- 0x08045A40 IsCompositePreviewPointOnScreen: confirmado, recorte -0x40<x<0xA0, -0x20<y<0xF0. Chamado pelas tres variantes QueueVisible.
- 0x08045AEC QueueVisibleCompositePreviewTemplateEntry: confirmado, filtra por tela e enfileira OAM. Chamador nao achado.

Divergencia: o README (RV9) diz que 0x08036D64 virou Fight_IsPartStatusProtected, mas o Ghidra ainda mostra FightPreview_IsPartTypeFlagSet. Nao renomeei (fora da lista).
Em aberto: destinos exatos dos buffers DMA em 0x08001278 e 0x08043AE0; quem instala os dois callbacks VBlank.
