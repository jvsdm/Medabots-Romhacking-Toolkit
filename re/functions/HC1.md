# HC1: continuacoes dos opcodes de evento (tabela 0x083D2630), lote 1

Continuacao = segunda fase de um opcode: rodada enquanto E+0xC89 != 0 (chamada por `Event_DispatchContinuation` 0x0804DC0C).
Estado: E = 0x030057E0 (gate = E+0xC88 = 0x03006468, ≠0 = evento rodando; E+0xC89 = 0x03006469 = byte de estado que escolhe o despacho; E+0xC93 = substado; E+0xC94.. = argumentos) (QA3).
Termino padrao: `Event_EndOpcode` 0x08050290 (zera E+0xC89 e E+0xC93).

## 0x0804DC34 Event_Op00_Cont_NoOp
- Assinatura: `void Event_Op00_Cont_NoOp(void)`
- O que faz: so `return`. Alvo comum de 53 opcodes (0x00, 0x06, 0x0D, 0x12..0x19, 0x1B, 0x1C, 0x21, 0x22, 0x25, 0x2C, 0x34, 0x35, 0x3E, 0x3F, 0x44..0x50, 0x60, 0x61, 0x64, 0x65, 0x68, 0x69, 0x6B, 0x74, 0x75, 0x77, 0x7B..0x7D, 0x80, 0x81, 0x83, 0x85, 0x86, 0x88). Esses opcodes nao tem segunda fase; o pendente nem precisa ser zerado aqui (quem liga E+0xC89 e o primario).
- Usado por (HC.md): 53 opcodes listados acima.
- Para o Lua: nao precisa (no-op).
- Confianca: confirmado (decompile: `return`).

## 0x0804DC38 Event_Op01_Cont_ShowMessageA
- Assinatura: `int Event_Op01_Cont_ShowMessageA(void)`
- O que faz: maquina de 2 fases pelo substado E+0xC93. Fase 0: chama `FUN_08047C00`, grava em 0x030057A0+0x2E o banco (arg0 - 2) e em +0x30 o id da mensagem (u16 BE de E+0xC95..C96), chama `FUN_0804B738` (stub que devolve 0; se nao zero, termina) e avanca para fase 1. Fase 1: escolhe a tabela de textos (banco 0/1 -> ponteiro 0x084144B4; banco >= 2 -> entrada (banco-2) de 0x0847DF24), pega o ponteiro da mensagem pelo id e chama `FUN_08047C24(0,0,0,ponteiro)` ate retornar nao-zero; ai termina (`Event_EndOpcode`).
- Usado por (HC.md / HE1): opcode 0x01 Show_Message_A (primario `Event_ReadMessageArgsAndDefer` 0x0804B758 liga E+0xC89).
- Para o Lua: "caixa de texto" = banco + id -> texto; `script.lua` ja le msg/bank. Efeito de desenho em `FUN_08047C24` (nao lido a fundo).
- Confianca: confirmado (fluxo e argumentos, pelo decompile e pool 0x0804DC4C..DC94); provavel (que FUN_08047C24 desenha a caixa).

## 0x0804DCF8 Event_Op02_Cont_Finish
- Assinatura: `void Event_Op02_Cont_Finish(void)`
- O que faz: so chama `Event_EndOpcode` 0x08050290 (zera E+0xC89 e E+0xC93). Nao desenha nada. Divergencia: o primario 0x02 (Show_Message_B) usa o mesmo helper 0x0804B758 do 0x01, mas a continuacao 0x02 nao tem fase de caixa; logo a caixa do 0x02 nao aparece aqui. Pode ser o tick de mensagem do overworld ou um erro do mtbe (nome "ShowMessageB"). NAO RESOLVIDO: conferir no emulador (replay do 0x02) antes de portar.
- Usado por (HC.md / HE1): opcode 0x02 Show_Message_B.
- Para o Lua: terminar o opcode na proxima fase; decidir sobre a caixa depois da checagem no emulador.
- Confianca: confirmado (decompile de 1 chamada); divergencia com 0x01 provavel (nao verificada).


## 0x0804DD04 Event_Op03_Cont_CloseMessageBox
- Assinatura: `void Event_Op03_Cont_CloseMessageBox(void)`
- O que faz: chama `StateMachine_DispatchSub` 0x08041CA4 a cada quadro (a animacao de fechar a caixa avanca nele); quando devolve nao-zero, `Event_EndOpcode`. O primario 0x03 so liga o pendente.
- Usado por (HC.md / HE1): opcode 0x03 Close_Message_Box.
- Para o Lua: "fechar caixa" = esperar a caixa sumir, depois proximo opcode.
- Confianca: confirmado (decompile); provavel (que a maquina e a da caixa de texto).

## 0x0804DD18 Event_Op04_Cont_WaitFrames
- Assinatura: `void Event_Op04_Cont_WaitFrames(void)`
- O que faz: contador regressivo no byte E+0xC94 (0x03006474, lido pelo primario 0x04 como frames). Se for 0, `Event_EndOpcode`; senao decrementa. Com N gravado, dura N+1 quadros de continuacao.
- Usado por (HC.md / HE1): opcode 0x04 Wait_X_Frames.
- Para o Lua: espera = N+1 ticks (confirmar a contagem no replay do opcode 0x04 se for usar exato).
- Confianca: confirmado (decompile + pool E+0xC94).

## 0x0804DD5C Event_Op07_Cont_FadeToBlack
- Assinatura: `void Event_Op07_Cont_FadeToBlack(void)`
- O que faz: chama `Overworld_RunScreenTransitionStep` 0x08049E08 (argumento 0) a cada quadro; quando retorna nonzero (transicao acabou), `Event_EndOpcode`. O efeito (preto, duracao) foi armado no primario 0x07 via `Overworld_ResetAndStartScreenTransitionEffect` 0x08049D90.
- Usado por (HC.md / HE1): opcode 0x07 Fade_To_Black.
- Para o Lua: esperar o fim da transicao de tela; nao precisa de logica propria alem do interpolador.
- Confianca: confirmado (decompile); provavel (argumento 0 = slot da transicao).


## 0x0804DD70 Event_Op08_Cont_FadeFromBlack
- Assinatura: `void Event_Op08_Cont_FadeFromBlack(void)`
- O que faz: igual ao 0x07 no corpo: `Overworld_RunScreenTransitionStep` 0x08049E08 (arg 0) a cada quadro; ao retornar nonzero, `Event_EndOpcode`. Difere do 0x07 so no efeito armado pelo primario 0x08 (sentido do fade).
- Usado por (HC.md / HE1): opcode 0x08 Fade_From_Black.
- Para o Lua: esperar o fim da transicao; mesmo interpolador do 0x07.
- Confianca: confirmado (decompile, corpo identico ao 0x07).

## 0x0804DDB4 Event_Op0B_Cont_WarpA_LoadScene
- Assinatura: `void Event_Op0B_Cont_WarpA_LoadScene(void)`
- O que faz: so chama `Map_LoadCurrentSceneResources` 0x0804DDCC (paleta, tiles, animacoes, sprites da cena nova). Esse loader termina chamando `Event_EndOpcode` 0x08050290 (zera o pendente) — por isso a continuacao nao tem fim proprio. O primario ja gravou mapa, x/y e direcao.
- Usado por (HC.md / HE1): opcode 0x0B Warp_A.
- Para o Lua: troca de mapa = carregar cena nova e depois o proximo opcode.
- Confianca: confirmado (decompile + nota R3 sobre o tail-call a 0x08050290); provavel (ordem exata de carga, sem ler o R3 inteiro).

## 0x0804DDC0 Event_Op0C_Cont_WarpB_LoadScene
- Assinatura: `void Event_Op0C_Cont_WarpB_LoadScene(void)`
- O que faz: identico ao 0x0B (chama `Map_LoadCurrentSceneResources`, que termina com `Event_EndOpcode`). Difere so na primaria: scene variant 0 (0x0300643B) em vez de 1.
- Usado por (HC.md / HE1): opcode 0x0C Warp_B.
- Para o Lua: igual ao 0x0B, com a variante de cena zerada.
- Confianca: confirmado (decompile); provavel (mesmo tail-call).


## 0x0804E43C Event_Op1A_Cont_NpcDoAnimation
- Assinatura: `void Event_Op1A_Cont_NpcDoAnimation(void)`
- O que faz: chama `Map_ObjectAnimStep` 0x080508B4 a cada quadro (tick da animacao do objeto render da pessoa do evento); quando devolve nonzero, `Event_EndOpcode`. O primario 0x1A ja escolheu a variante (0x03006D2A, frames em 0x03006D22).
- Usado por (HC.md / HE1): opcode 0x1A NPC_Do_Animation. Mesmo tick usado pela continuacao 0x27 (HC2).
- Para o Lua: esperar a animacao da pessoa terminar.
- Confianca: confirmado (decompile); provavel (nome do tick, lido em R3).

## 0x0804E450 Event_Op1D_Cont_HopEventNpc
- Assinatura: `void Event_Op1D_Cont_HopEventNpc(void)`
- O que faz: chama `Map_ObjectArcStep` 0x08050918 a cada quadro (arco do pulo do objeto); quando devolve nonzero, `Event_EndOpcode`. O primario 0x1D ja gravou o slot (0x03006D2A), a altura base (0x03006D28) e tocou o som 0x70.
- Usado por (HC.md / HE1): opcode 0x1D Hop_Event_NPC.
- Para o Lua: pulo = arco ate o fim; o som ja toca na primaria.
- Confianca: confirmado (decompile); provavel (nome do arco, lido em R3).

Pendente: nenhum. HC1 completo: 11 de 11 alvos da HC1_lista.txt (0x00, 01, 02, 03, 04, 07, 08, 0B, 0C, 1A, 1D). Divergencia aberta: op 0x02 (secao Event_Op02_Cont_Finish).
