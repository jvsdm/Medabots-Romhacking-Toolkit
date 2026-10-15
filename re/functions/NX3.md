# NX3 - lista de 13 funcoes reparadas pelo sync

Cada entrada: decompile + chamador (BL/ponteiro via scan python -I na ROM) + nome novo.

- 0x08045B68 `CompositePreview_QueueEntryWithBounds` (antes: QueueVisibleCompositePreviewTemplateEntryWithBounds). Provavel: testa tela, monta OAM e grava 4 shorts. Sem chamador achado.
- 0x08045C30 `CompositePreview_QueueEntryWithAffineBounds` (antes: ...WithAffineBounds). Provavel: igual, com matriz afim. Sem chamador achado.
- 0x08053360 `Viewport_ReplaceZeroWithDefault`. Provavel: troca x==0 por padrao (guarda de divisor). Chamado 5x por Viewport_ScaleStep.
- 0x080533A4 `Viewport_ScaleIfNegative`. Provavel: multiplica se x<limite. Chamado 11x por Viewport_ScaleStep.
- 0x080534C4 `Viewport_ComputeTriNormalAndUpdate`. Provavel: normaliza 3 vetores e chama UpdateFromCamera. Chamado por Viewport_UpdateEdgesAndStore.
- 0x080535C8 `Viewport_ComputeNormalAndDistance`. Provavel: produto vetorial normalizado. Chamado por ComputeTriNormalAndUpdate.
- 0x080537BC `Viewport_GetCenter`. Provavel: le 3 words de struct global. Chamado por ComputeTriNormalAndUpdate.
- 0x080537D8 `Viewport_CallSetFloatBounds`. Confirmado: so chama SetFloatBounds. Chamador 0x08053466 em funcao nao identificada.
- 0x08053858 `Viewport_ComputeRectEdgeA`. Confirmado: nome mantido. Chamado por SetFloatBounds, FUN_08053fbc, UpdateEdgesAndStore.
- 0x0805390C `Viewport_ComputeRectEdgeB`. Confirmado: nome mantido. Chamado pelos mesmos 3 chamadores da A.
- 0x08053B04 `Viewport_UpdateFromCamera`. Confirmado: nome mantido. Chamado por ComputeTriNormalAndUpdate.
- 0x08053C7C `Viewport_ProjectToScreen` (antes: Viewport_AxisHelperC). Provavel: projecao 3D->tela. Chamado por ComputeOffsetA e ClampAxisB.

## Em aberto
- Chamador de CompositePreview_QueueEntry* nao achado (nem BL nem ponteiro); pode ser despachado por tabela com outro formato.
- Funcao que contem 0x08053466 (chamador de Viewport_CallSetFloatBounds) nao localizada.
- Constantes DAT_0805337x/080533cx/080535xx nao lidas; nomes "provavel" dependem do valor delas.
