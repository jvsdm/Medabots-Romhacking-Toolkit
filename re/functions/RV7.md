# RV7 - revisao de nomes (parcial)

Trocas de nome (renomeadas no Ghidra):
- 0x08031A3C: `Fight_BuildPartActionMetadataForAllParts` -> `Fight_RunAllPartActionPhases` (antes: Fight_BuildPartActionMetadataForAllParts). Roda prepare+execute para 3 partes; nao monta metadata.
- 0x08033224: `Fight_IsBotMedaforceInactive` -> `Fight_IsBotAutoRecoverInactive` (antes). Testa bot+0xC4 (AutoRecover segundo GH-SS); o medidor de Medaforce e +0xC1.
- 0x08033240: `Fight_IsBotPendingActionClear` -> `Fight_IsBotPendingStatusClear` (antes). Testa bot+0xC9 (tipo de status pendente).

Achados: 0x080336DC testa kind de parte 0/1 e nao especialidade; nome mantido por enquanto.

Pendente: de 0x080338DC ate 0x0803447C (ver RV7_lista.txt), nao decompilado nesta sessao.
