# FN5: nomes das FUN_ restantes (lista FN5_lista.txt)

Nomes em ingles Subsistema_VerboObjeto. Confianca: confirmado (lido no decompile) | provavel | chute.

## 0x080576D0 GameState_StartFlowCommand6
- Wrapper: chama `GameState_StartFlowByCommand(6, 0)` e retorna. Confianca: confirmado (corpo) / provavel (nome).

## 0x08057D14 Obj_TwoStageInitSetState2_A
- `Obj_TwoStageInitThenDispatch(1,1,0, [DAT_08057D38+0x204])`; se retorna != 0, grava 2 no byte de estado. Par de 0x08057D50. Confianca: provavel.

## 0x08057D50 Obj_TwoStageInitSetState2_B
- Mesmo padrao de 0x08057D14, mas le o campo +0x200 de outra base. Confianca: provavel.

## 0x0805C988 Text_StepCharIncrement
- Le o caractere apontado por um ponteiro; se for ' ' grava 9 e 0; senao incrementa o caractere em 1 e grava metade (>>1) do byte em outro halfword. Parece contador de texto/rolagem. Confianca: chute.

## 0x08050320 Obj_ComputeActorTableIndex
- `Event_GetTrackedActorSlot(arg)` -> indice = ((byte +0x4F do ator * 0x40 + base + 0x2B) * 12) + arg2*3. Confianca: confirmado (formula) / provavel (nome).

## 0x0805E404 Task_CallHandlerTableThenCount
- Chama handler de tabela indexada (CallViaR0); se flag bit0 setado, incrementa dois shorts (contador + espelho +2); depois `Obj_SetFieldFromTable4((flag & 0x7FFFF) >> 3)`. Confianca: provavel.

## 0x0805EFD4 Sfx_WaitChannel7ThenAdvance
- Se `snd_sfx_query_playing(7) == 0`, zera byte +0x84 de um objeto e incrementa um contador. Espera o SFX do canal 7 terminar. Confianca: provavel.

## 0x0805FC58 Task_CallHandlerThenCountIfFlag
- Chama handler indexado (CallViaR0); se bit0 do flag, incrementa short +2 de um objeto. Confianca: provavel.

## 0x0805FFF8 Screen_IntroFlashScrollStep
- Maquina de tempo (timer +0x3A ate 8): em 4 altera REG_DISPCNT (bit 0x200) e toca SFX 0x17; depois faixa de rolagem (`Screen_ScrollTileBand`) e atualizacao de atributos por tabela. Longa; so o esqueleto foi lido. Confianca: chute no detalhe, provavel no papel de intro de tela.

## 0x08060280 Task_CallHandlerThenDecrementIfFlag
- Como 0x0805FC58, mas decrementa o short +2 se bit0 do flag. Confianca: provavel.


## 0x0804B15C Stub_ReturnNoop_0804B15C
- Corpo vazio (so retorno). Sem xref direto; ponteiro addr|1 em 8 pontos da regiao 0x083D215C-0x083D21BC (tabela de callbacks). Confianca: confirmado (corpo) / provavel (papel de stub).

## 0x08060688 / 0x080617E0 / 0x08061958 / 0x08063948
- Ja tinham nome no Ghidra quando conferi (Dispatch_CallTableEntryByByte, Fx_StepCounterResetAtZero, Task_CallHandlerTableThenCount_B, Task_CopyCallTableEntryThenCount): pulados, nao sao mais FUN_. Nao registrados aqui.

## 0x08064A08 Obj_BuildActiveSlotMaskAndReset
- Varre 29 entradas de 0x40 bytes; onde byte +0 tem bit7, ORa 1<<byte[1] numa mascara. Zera +0x48..+0x4B e grava mascara ou 0 em +0x1C conforme +0x7A. Confianca: provavel no papel, chute no nome.

## 0x08064B90 Task_CallHandlerTableIndexedCopy
- Copia 2 words para pilha e chama CallViaR0 com indice de tabela. Confianca: provavel.

## 0x08064BBC Obj_CheckReachedTargetSetState14
- Grava +6 do objeto; se +0xA e +0xE iguais ao valor, grava 0x14 e zera um byte. Confianca: chute.

## 0x08064DB4 Task_CallHandlerTableThenCountIfFlag_V2
- Mesmo padrao de 0x0805E404 (CallViaR0 + contadores se bit0). Confianca: provavel.

## 0x08067B9C Task_CallHandlerCopyThenDecrement
- Copia 28 bytes de tabela para pilha, CallViaR0 indexado, decrementa short +2 de um objeto. Confianca: provavel.

## 0x08068920 Stub_ReturnTrue_08068920
- Corpo `return 1`. Confianca: confirmado.
