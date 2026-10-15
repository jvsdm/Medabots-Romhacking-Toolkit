# MS: motor de mensagens / texto (GH-MS, 2026-10-14)

Continua `A6.md` (cadeia fight+0xA6 -> gTextGameMessages -> StateMachine_DispatchStep). Aqui: tabela de estados,
interpretador de controle, impressora real. Textos do jogo NÃO copiados. Confiança: "confirmado" = lido no
decompile ou na ROM (literal/bytes); "provável" = leitura parcial; "aberto" = não fechado.

Ctx `0x03000440` (gMessageCtx), campos usados (confirmados nos decompiles abaixo):
- +0x04 estado (índice em gMessageStateHandlerTable; `StateMachine_DispatchStep` retorna `estado > 7`)
- +0x08 ponteiro do roteiro; +0x0C pc (halfword)
- +0x13 flag de parada do laço (ScriptVM sai do laço quando != 0)
- +0x14 caneta X; +0x15 largura restante da palavra; +0x16 subcoluna; +0x17 linha/banco de tile
- +0x18 flag "glifo/palavra pronta"; +0x19 modo de quebra (1 = flush + nova linha, 2 = espera tecla)
- +0x1A contador de espera (0xFF = sem espera)
- +0x1B byte de despacho (estados 9..22, via tabela)
- +0x1C contador do fade (estado 1); +0x25 contador de página; +0x26 passo do estado 3
- +0x27 itálico (flag, alternado por F8); +0x89 ocupado (espera de frame)

## 1. gMessageStateHandlerTable 0x083B291C (ROM, leitura direta; entradas Thumb)

| estado | entrada    | função / nome                    | o que faz                                                         | confiança |
|--------|------------|----------------------------------|-------------------------------------------------------------------|-----------|
| 0      | 0x0803FB6C | Gfx_PaletteFlushStep             | copia buffers de paleta p/ hardware; avança                       | confirmado (A6) |
| 1      | 0x0803FB94 | MsgState_FadeToneStep (sem func.)| fade de tom: BL 0x0803F684 (cor 0x1B2, peso 2*ctx+0x1C, máscara 0x1FFF) e BL 0x0803F998 (cor 0x192, máscara 0x3FFF); ctx+0x1C 0..8 (9 passos), depois estado++ | confirmado (decode da ROM; 0x0803F684 = blend de tom, E3) |
| 2      | 0x0803FBDC | MsgState_BG0OnThenDispatch (sem func.) | se ctx+2 == 1 -> estado = 4 (pula 3); senão estado = 3, ctx+0x26 = 0 e liga BG0 em DISPCNT (0x04000000 |= 0x100) | confirmado no byte-level; resto do bloco (cópia de 0x500) aberto |
| 3      | 0x0803FC20 | Gfx_LoadTileSetStep3 (já nomeado)| 3 passos (ctx+0x26 = 0,1,2): carrega moldura da caixa (tiles para VRAM 0x06000020 e BG0CNT 0x04000008); depois estado++ | confirmado (literais: 0x084870CC, 0x08486FC0, 0x08486E6C = moldura, H1) |
| 4      | 0x0803FCC8 | MsgState_ClearBoxAndRunScript (sem func.) | laço de 0x34 blocos de 0x40 B (limpa caixa via wrapper BIOS 0x0807CCxx), BL 0x0803F15C; estado++; chama ScriptVM_RunOpcodes no mesmo quadro | provável (laço e chamadas lidos; alvo do wrapper não confirmado) |
| 5      | 0x0803FD28 | ScriptVM_RunOpcodes (já nomeado)| interpretador do roteiro (ver seção 2)                            | confirmado (decompile) |
| 6      | 0x080408D0 | MsgState_DispatchByScriptByte (sem func.) | lê o byte do roteiro em pc+1, índice = byte*4 sobre a tabela 0x083B2940 (literal confirmado), chama o handler via wrapper 0x0807CCF0 | provável (faixa de índice e número de entradas não fechados) |
| 7, 8   | 0x08000841 | sentinela (BX lr)                | sem efeito; 8 = "terminou" para StateMachine_DispatchStep        | confirmado (mesmo ponteiro) |
| 9..22  | 0x080408F4 .. 0x08040B50 | handlers do mesmo tipo (dispatch por ctx+0x1B) | 9 e 10..12 usam ctx+0x1B; 13 = 0x08040974 (despachante de JT2, ctx+0x1B). Estados >= 8 não são alcançados pelo fluxo normal de mensagem | provável (ligação a JT2 confirmada só para 0x08040974) |

Nota: a tabela 0x083B291C só é usada com ctx+4 <= 8 pelo fluxo de mensagem; as entradas 9..22 pertencem à família
de despacho por ctx+0x1B (JT2). Não confundir com a tabela 0x083B2990 (JT2, 23 entradas).

Entradas listadas em `MS_lista.txt` (função desde o sync de 2026-10-14 nos casos conferidos: 0x0803FB94 = MsgState_FadeToneStep, 0x0803FBDC = MsgState_BG0OnThenDispatch, 0x0803FCC8 = MsgState_ClearBoxAndRunScript; demais na linha 36): 0x0803FB94, 0x0803FBDC, 0x0803FCC8,
0x080408D0, 0x08040898, 0x080408BC e as de 9..22 (+ 0x080408F4..0x08040B50).

## 2. Controle do texto: ScriptVM_RunOpcodes 0x0803FD28 (decompile confirmado)

Laço: lê o byte em `roteiro + pc`. Se o byte for <= 0xF6 (ou o padrão), vai para o impressor de glifo
`Text_RenderGlyphStep` 0x08040038 (ponto de chamada único: 0x0803FE86, confirmado por xref). Casos:

| byte | handler (Ghidra)                     | efeito                                                             | pc |
|------|--------------------------------------|--------------------------------------------------------------------|----|
| F7 n | Script_OpWaitFrames 0x080407A4       | espera n quadros SEM tecla (n = byte pc+1); n=0xFF não espera     | +2 |
| F8   | (inline)                             | alterna itálico (ctx+0x27 ^= 1)                                    | +1 |
| F9   | padrão: Text_RenderGlyphStepVariant 0x080402B4 | glifo "variante" (provável: caractere de substituição <MEM>); aberto | +1 |
| FA n | Script_OpWaitFramesOrKey 0x0804070C  | espera até n quadros, ou avança antes se A/B estiver pressionado (DAT_080407A0 & 3) | +2 |
| FB a,b,c | Script_OpShowPortrait 0x0803FEB8 | retrato (3 bytes; só mensagens de sistema)                      | +4 (provável) |
| FC   | Text_WaitKeyOrClearBox 0x080405D0    | pausa de tecla A/B; limpa a caixa (nova página); ctx+0x25++      | +1 quando a tecla é lida |
| FD   | (inline) + Text_FlushAndNextLine 0x0804056C | nova linha; se ctx+0x19 == 2, espera tecla antes              | +1 |
| FE   | Script_OpAdvanceStep 0x0803FEA8      | avança estado (não decompilado a fundo)                            | — |
| FF n | Script_OpSubcommandOrNextState 0x08040820 | limpa a caixa (CpuFastSet 0x10 palavras); n < 5 -> handler da tabela 0x08040884; n >= 5 -> estado++ | — |

Padrões de quebra: ctx+0x19 == 1 -> flush + nova linha; == 2 -> espera tecla (FD/FC e o padrão usam isso).

### FA 50 (resolvido)
`FA 50` = espera de até 0x50 = 80 quadros (~1,3 s a 60 fps), que termina antes se A ou B for pressionado
(a condição de avanço é `DAT_080407A0 & 3 != 0`, mesma convenção do FC). Em `FA FF` não há espera.
Confiança: confirmado (decompile); valor 80 quadros = conversão do byte (provável em relação ao relógio real).
Isto é o "pausa de tecla" de combate do A6; a mensagem de combate usa FA 50 antes do END.

### Argumento do FF (0, 2, 3, 4)
FF n chama `tabela[n]` para n < 5 (tabela de 5 ponteiros em 0x08040884, lida na ROM):
- n = 0, 2, 3 -> 0x08040898: se o byte de flag do ctx (ldrb [ctx+0]) & máscara for != 0, chama o SFX 0x0D
  (`snd_sfx_request(0xd)`, mesmo som de confirmação do FC) e avança o estado de ctx (ctx+4++). Confirmado: SFX 0x0D
  e estado++; a condição exata (máscara do flag) fica provável.
- n = 1, 4 -> 0x080408BC: avança ctx+4++ do MESMO ctx 0x03000440 (MS2; antes dizia outro objeto),
  sem SFX (MS2).
- n >= 5: estado++ do ctx (padrão).
Conclusão: o byte n do FF escolhe a ação de fechamento (som + avanço / avanço de outro objeto). Os valores vistos
nas mensagens (0, 2, 3, 4) caem nas duas famílias; o FF não é "fim" literal: é o comando de término da mensagem.

## 3. Impressora real

- **Text_RenderGlyphStep 0x08040038 é a impressora de texto de mensagem.** Chamado só pelo ScriptVM (caso padrão).
  Calcula a largura da palavra (laço até byte >= 0xF8 ou 0), guarda em ctx+0x15, quebra quando
  ctx+0x14 + largura >= 0xD1 (confirmado no decompile: `0xd1`), copia o tile 8x8 (0x40 B) com CpuSet 0x20 palavras
  e OR de subcoluna. Tabela de largura: 0x08040138 (normal) ou 0x08040100 (itálico, ctx+0x27). Já tinha nome
  correto; nenhum rename necessário.
- **Text_RenderGlyphStepVariant 0x080402B4**: mesma lógica para glifo variante (rota do F9 / deslocamento de subcoluna).
- **Text_TypewriterRowStep 0x080678D4 NÃO é a impressora de mensagem.** Decompile: chama `PointRing_StepFrame`
  (anel de pontos, 0x08066D8C) e escreve 14 colunas por linha com `bg_copy_panel_rows` ao fim: é o efeito de
  linha de menu/painel. A suposição do mtbe.csv (provável) deve ser corrigida: ver seção 5.
- **FUN_08047C24** (já citado em FR1/R2b/HC1) é o dono do ciclo de evento de "mostrar mensagem"
  (Event_Op01 fase 1 espera este retorno e ele chama StateMachine_DispatchStep). Não imprime.

## 4. Códigos de mensagem (resumo, sem texto)

Mantém o que está em A6 (0x1A, 0x1C, 0x1D, 0x25, 0x2B..0x2D, 0x39, 0x24). O formato real do roteiro está na
seção 2 deste arquivo. Mensagens de combate: END com `FA 50` antes; de sistema: END sem FA.

## 5. Pendente / para o Opus

- Criar as funções listadas em `MS_lista.txt` no Ghidra (sem função hoje): os handlers 1, 2, 4, 6 e 9..22 (e 0x08040898 / 0x080408BC).
- Corrigir `re/symbols/mtbe.csv` linha 0x080678D4 (Text_TypewriterRowStep: "NÃO é impressora de mensagem; linha de menu/anel") e
  o comentário de 0x0803FC20 (é o carregador da moldura, consistente com H1).
- Fechar o miolo do estado 2 (cópia de 0x500 após liga BG0) e o alvo do wrapper 0x0807CCxx do estado 4.
- Fechar a máscara do flag do FF 0/2/3 (FF 1/4 já resolvido: mesmo ctx 0x03000440, MS2).
- Não testado no emulador (fora desta tarefa).
