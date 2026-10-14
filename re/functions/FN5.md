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

