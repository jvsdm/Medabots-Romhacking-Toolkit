# NX4 - lista de funcoes 0x08053D78..0x0806B484 (11 entradas)

Tudo decompilado. Renomes aplicados no Ghidra; CSV em NX4_simbolos.csv.

- 0x08053D78 Viewport_WorldToScreenXY (antes Viewport_AxisHelperD): provavel. Converte x,y em coordenada de tela com escala/offset de 0x03005350.
- 0x08053DE0 Viewport_CheckBoundsSetState (antes Viewport_AxisHelperE): provavel. Projeta, e se o slot entra na faixa, seta estado=2.
- 0x08054014 Viewport_UpdateEdgesAndStore: confirmado 240x160 (tela GBA) e loop de 16 bots nos cantos. Nome mantido.
- 0x080569E4 Viewport_TweenStepByMode (antes Viewport_ScaleStep): provavel. Passo de interpolacao por modo 0x10..0x17.
- 0x08056F24 Viewport_TweenAdvanceCheckDone (antes Viewport_CompareAndClampFloat): provavel. Avanca tween e retorna 1 ao chegar.
- 0x0806B130 BotAnim_StageDone11 (antes FUN_): confirmado, retorna 0. Estagio 11 da tabela 0x084857D0.
- 0x0806B1A8 BotAnim_StageSlideDown2 (antes FUN_): confirmado, estagio 2.
- 0x0806B1D0 BotAnim_StageSlideUp3 (antes FUN_): confirmado, estagio 3.
- 0x0806B1F8 BotAnim_StageHold4 (antes FUN_): confirmado, retorna 1 (espera).
- 0x0806B2B8 BotAnim_StageWobbleTimed7 (antes FUN_): confirmado, estagio 7.
- 0x0806B484 BotAnim_StageWaitTimer10 (antes FUN_): confirmado, estagio 10.

Chamadores (BL scan na ROM, python -I):
- 0x08053D78 <- 0x08053C06 (wrapper 0x08053C00, Viewport_ClampAxisA) e 0x08053DE0.
- 0x08053DE0 <- 0x08053C2A (Viewport_ClampAxisB em 0x08053C14).
- 0x08054014 <- 0x08054D5A: funcao que o Ghidra nao definiu (ver abertos).
- 0x080569E4 <- 0x08055580: idem.
- 0x08056F24 <- 0x0805473A: idem.
- Estagios BotAnim_*: ponteiros na tabela 0x084857D0; a tabela e lida por BotAnim_TickStageCallbacksAll (0x0806B0D4).

Abertos:
- Os chamadores 0x08054D5A, 0x08055580 e 0x0805473A estao em regiao sem funcao definida; o nome de quem chama o tween/UpdateEdges continua desconhecido.
- Viewport_ClampAxisA/B (0x08053C00/0x08053C14) estao com corpo truncado pelo Ghidra (halt_baddata, pool no meio). Nao renomeei.
- Viewport_ScaleStep e CompareAndClampFloat sao chute de modo de tween; falta confirmar as constantes float e o significado de cada modo.
