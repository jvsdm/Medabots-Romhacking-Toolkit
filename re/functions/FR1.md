# FR1 — pendências de FL2/FL3 e funções FUN_ (rodada de fechamento)

Fonte: pendentes de FL2 (6 alvos da FL_lista, sem seção) e FL3 (0x0806F5A0), mais os `FUN_` ainda sem nome
nos `FL2_simbolos.csv` / `FL3_simbolos.csv`. Tamanho real = Body do `get_function_by_address`.
Nomes no Ghidra: inglês, `Subsistema_Verbo`. Só renomeado o que está confirmado ou provável.

## 0x0806F23C Screen_TextCounterStep
- Assinatura: `void Screen_TextCounterStep(void)`
- Tamanho: 0x0806F23C - 0x0806F56D (868 bytes)
- O que faz: passo por quadro de uma tela de texto/contador. Incrementa dois contadores no contexto (+0xF0, +0xF2 += 0x6F). Pelo estado em +8: se < 5 despacha por tabela de ponteiros; senão, se 0xC4 faz a inicialização (limpa texto, esconde sprites, zera 6 slots); 0xC5..0xC9 despacha por outra tabela; demais estados fazem contagem com `counter_2_digits` e escrevem caractere com `text_step_char`. Ao final atualiza números de 6 dígitos (`format_6_digits`) e slots (`slots11_update`) e, ao passar de 0x187 no contador, muda estado para 0x0D.
- Chama: `counter_2_digits`, `text_step_char`, `clear_text_buffer`, `init_fila_4`, `init_hide_sprites`, `slot_start`, `format_6_digits`, `slots11_update`, tabelas de ponteiros. Chamada por: não verificado.
- Memória: RAM de contexto (DAT_0806F274 / DAT_0806F400 / DAT_0806F570); RAM de slots 0x03000000+.
- Para o Lua: precisa se a tela de texto for portada (ver tabelas de estado).
- Confiança: provável (o nome é pelo padrão contador + texto; tabelas de salto não recuperadas).

## 0x08071138 Anim_DisableAnim4IfIdle
- Assinatura: `void Anim_DisableAnim4IfIdle(void)`
- Tamanho: 0x08071138 - 0x0807115B (44 bytes)
- O que faz: se o byte de flag em (DAT_0807115C + 0x660) é zero, chama `disable_anim4(0x19, 0x4B)` e incrementa o contador apontado por DAT_08071160.
- Chama: `disable_anim4`. Chamada por: não encontrada nas xrefs de função (`get_function_xrefs` vazio; pode ser só ponteiro).
- Memória: RAM de flag em DAT_0807115C+0x660; contador em DAT_08071160.
- Para o Lua: não precisa (controle de animação de hardware/slot).
- Confiança: provável.

## 0x08071EFC Anim_RampStepThenSfx54
- Assinatura: `void Anim_RampStepThenSfx54(void)`
- Tamanho: 0x08071EFC - 0x08071F73 (120 bytes)
- O que faz: passo de rampa temporizada. Sobe o short em +0x104 de 2 em 2 até 0x36, depois de 1 em 1 até 0x3A. Depois usa o ushort em +0x11A (até 0x20). Ao chegar, se o short em +0x102 < 0x45, chama `slot_ptr_stub(8)`, toca SFX 0x54 (`snd_sfx_request`), zera +0x11A, incrementa o byte em DAT_08071F64+0x10 e sai com 1; senão, subtrai 4 de +0x102. Termina com salto indireto por tabela não recuperada (passa 0 ou 1).
- Chama: `slot_ptr_stub`, `snd_sfx_request(0x54)`, salto indireto. Chamada por: não verificado (lista de alvos com refs=1).
- Memória: RAM de contexto DAT_08071F18+0x102/+0x104/+0x11A; byte DAT_08071F64+0x10.
- Para o Lua: precisa se a sequência de rampa for portada (o efeito é som + avanço de etapa).
- Confiança: provável (vizinho de FUN_08071F74, que tem o mesmo padrão e som 0x54).

## 0x0806F5A0 Screen_TextCounterStepB
- Assinatura: `void Screen_TextCounterStepB(void)`
- Tamanho: 0x0806F5A0 - 0x0806F8CD (814 bytes; a lista dava 4920 porque é "até a próxima função", com literal pool e outros dados no meio)
- O que faz: gêmea de `Screen_TextCounterStep` (0x0806F23C): mesma estrutura (contadores +0xF0/+0xF2, tabela de 5 estados, init em 0xC4, tabela 0xC5..0xC9, `counter_2_digits`/`text_step_char`, `format_6_digits`, `slots11_update`, e fim em 0x187 mudando estado). Diferenças: usa outro contexto (DAT_0806F5D8), passa 0x28/0 e caracteres 2/3 em vez de 0/1, e ao fim grava 9 (não 6/0xD) no campo de estado. Trata-se de uma segunda tela de texto/contador do mesmo tipo.
- Chama: as mesmas de `Screen_TextCounterStep`. Chamada por: não verificado.
- Memória: RAM de contexto DAT_0806F5D8 / DAT_0806F764 / DAT_0806F8D0.
- Para o Lua: mesmo caso da gêmea.
- Confiança: provável (estrutura confirmada no decompile; o que a tela mostra não foi verificado no jogo).

## 0x0807474C Snd_TrackUpdateFlagged
- Assinatura: `void Snd_TrackUpdateFlagged(void)`
- Tamanho: 0x0807474C - 0x0807477B (48 bytes)
- O que faz: chama `snd_channel_update_1` no canal de `*DAT_0807477C + 0x1D0`. Se `snd_track_test_flag2()` devolver zero, ativa os bits 0x5F no byte +0x50 da faixa e atualiza a faixa de novo.
- Chama: `snd_channel_update_1` (x2), `snd_track_test_flag2`. Chamada por: não encontrada nas xrefs de função (pode ser ponteiro).
- Memória: RAM de faixa de som (ponteiro em DAT_0807477C).
- Para o Lua: não precisa (driver de som).
- Confiança: provável.

## 0x08074CA0 Snd_ApplyAllTracksCounted
- Assinatura: `void Snd_ApplyAllTracksCounted(u32 arg)`
- Tamanho: 0x08074CA0 - 0x08074CD7 (56 bytes)
- O que faz: incrementa o contador de vozes em +0x730 (ushort, até DAT_08074CDC), chama `snd_track_apply_all_0c(arg)` e depois decrementa o mesmo contador se for diferente de zero. Par de `Snd_StartSfxCounted` (0x08074E24), que usa o mesmo campo +0x730.
- Chama: `snd_track_apply_all_0c`. Chamada por: nenhuma xref de função (refs=0 na lista).
- Memória: RAM de faixa de som (ponteiro em DAT_08074CD8), campo +0x730.
- Para o Lua: não precisa.
- Confiança: provável.

## 0x0800B79C FUN_0800b79c (mantido)
- Assinatura: `void FUN_0800b79c(void)`
- Tamanho: 0x0800B79C - 0x0800B80D (114 bytes)
- O que faz: lê o byte em (DAT_0800B7C4 + DAT_0800B7C8). Se for 0x1E, liga a flag 0x4000 no halfword ctx+0x1C0; senão, se o bit 4 do byte estiver zero liga 0x4000, e se estiver ligado aplica máscara (DAT_0800B7CC) ao halfword. Depois incrementa um contador byte (DAT_0800B810 ou DAT_0800B7E8 + base).
- Chama: nada. Chamada por: nenhuma xref de função (uso por ponteiro/tabela).
- Memória: RAM de contexto (base DAT_0800B7C4) +0x1C0 e um byte de estado.
- Para o Lua: não sei o uso.
- Confiança: chute. Subsistema não fechado; não renomeado.

## 0x08057CB0 Flow_WaitObjectThenCount
- Assinatura: `void Flow_WaitObjectThenCount(void)`
- Tamanho: 0x08057CB0 - 0x08057CFF (80 bytes)
- O que faz: chama `FUN_08047C24(1,1,0,id)` com id = tabela[ctx+0x70 byte + 0x82] (entrada da tabela em DAT_08057CE8). Enquanto devolver zero, não faz nada. Quando devolver não zero, incrementa o byte ctx+0x70; se o novo valor for < 6 grava (*DAT_08057CF0 − 1) em *DAT_08057CF0, senão grava 2 em DAT_08057D00.
- Chama: `FUN_08047C24`. `FUN_08047C24` é a máquina de 2 estados de init/load de objeto (ver R2b_simbolos.csv) e também a espera de HC1 (Event_Op01 fase 1). Chamada por: não encontrada nas xrefs de função.
- Memória: RAM de contexto DAT_08057CEC+0x70; tabela em DAT_08057CE8; saídas em DAT_08057CF0 / DAT_08057D00.
- Para o Lua: depende de `FUN_08047C24` (portar se a sequência for portada).
- Confiança: provável no padrão (espera + contador de 6 passos); o subsistema exato não fechado.

## 0x08066460 FUN_08066460 (mantido)
- Assinatura: `void FUN_08066460(void)`
- Tamanho: 0x08066460 - 0x0806652D (206 bytes)
- O que faz: passo de comando com handshake. Se o byte de flag +0x7A do objeto estiver zero: sem entrada (DAT_080664B4 = 0) grava 0xEF00 | byte(+5) no halfword de saída; com entrada, se o halfword recebido tiver high byte 0xEF guarda o byte baixo em +5 e liga +0x7A; se for igual a DAT_080664CC, ecoa. Com +0x7A ligado, copia o halfword de parâmetro. Ao fim, se o campo +0xE bater com DAT_08066504: se byte +5 == 4 muda estado para 8 e chama `Obj_ResetStateFields`; senão `Obj_InitTimerRecord(..., 0xC0)` e avança +2.
- Chama: `Obj_InitTimerRecord`, `Obj_ResetStateFields`. Chamada por: não encontrada nas xrefs de função.
- Memória: RAM de objeto (ponteiro em DAT_08066480) +0x5, +0x7A, +0x50; saída em DAT_08066500; entrada em DAT_080664B8.
- Para o Lua: não sei se é o protocolo de link (0xEF00 pode ser só comando interno; não confirmado).
- Confiança: provável no fluxo; subsistema (comando 0xEF00) não fechado; não renomeado.

## 0x0807DAE8 Math_Sf64CompareSafeAlt (duplicata)
- Mesma função que `Math_Sf64CompareSafe` (0x0807DB34): corpo idêntico (76 bytes). Renomeado só para evitar nome duplicado no Ghidra; no mtbe, usar o de 0x0807DB34.
- Confiança: confirmado (decompile comparado).

## FL3 — funções com nome FUN_ (tamanhos = Body do Ghidra)

### 0x08000824 FUN_08000824 (mantido)
- `void FUN_08000824(void)`, 0x08000824 - 0x08000837 (20 bytes). Chama o callback em `*DAT_08000838` via `CallViaR0` e grava estado 2 em `DAT_0800083C`. Wrapper genérico, sem subsistema claro. Confiança: chute.

### 0x0800B814 Gfx_SlideFadeStepThenClear
- `void Gfx_SlideFadeStepThenClear(void)`, 0x0800B814 - 0x0800B84B (56 bytes; passo por quadro). Decrementa de 8 o byte ctx+0x1C8 e o byte em (ctx + DAT_0800B850). Quando ctx+0x1C8 chega a zero: `Slot_ClearAttrMask(7,1)` e incrementa o byte em DAT_0800B854. Mesmo padrão de fade/slide de `Gfx_BgStripeStep`. Confiança: provável.

### 0x08010B24 FUN_08010B24 (mantido)
- `void FUN_08010B24(void)`. Zera um bloco de 0x14 bytes (`Mem_FillBytes`) em DAT_08010B48 e grava 2, 3 e 0 em três bytes de estado (DAT_08010B48, DAT_08010B4C, DAT_08010B50). Inicialização de estado; subsistema não fechado. Confiança: chute.

### 0x0803E634 Flow_StepCounterTickA
- `void Flow_StepCounterTickA(void)`. Chama `StateMachine_DispatchStep` no objeto em (DAT_0803E650 + 0x30); se devolver não zero, incrementa o byte em DAT_0803E654. Padrão "espera de máquina + contador" (igual a `Flow_StepCounterTickB`, `Flow_StepDispatchCounter`). Confiança: provável (padrão; a tela não identificada).

### 0x0805C0D4 FUN_0805C0D4 (mantido)
- `void FUN_0805C0D4(void)`. Chama por `CallViaR0` o alvo da tabela indexada por DAT_0805C118 (tabela em DAT_0805C114) e avança dois pares x/y (DAT_0805C11C; e DAT_0805C124 se o bit 0 de DAT_0805C120 estiver ligado). Passo de animação com despacho por tabela. Confiança: chute.

### 0x0805CAF4 Anim_WaitRecordThenState3
- `void Anim_WaitRecordThenState3(void)`. Espera `Anim_StepRecordCounter(objeto + 0x50)` devolver não zero; então grava 3 em DAT_0805CB18 e 0 em DAT_0805CB1C. Confiança: provável.

### 0x0805D580 Flow_StepCounterTickB
- `void Flow_StepCounterTickB(void)`. Igual a `Flow_StepCounterTickA` (0x0803E634), mas com o objeto em (DAT_0805D59C + 0x20). Confiança: provável.

### 0x0805DB20 Fx_StepThenInitRecord8
- `void Fx_StepThenInitRecord8(void)`. `StateMachine_DispatchStep` no objeto em (DAT_0805DB44 + 0x48); ao terminar (não zero): `Fx_InitRecord8(1, 0x16, 7)` e incrementa DAT_0805DB48. Confiança: provável.

### 0x080645A0 Obj_StepThenTimerOnMatch
- `void Obj_StepThenTimerOnMatch(void)`. Escolhe o estado (offset 0x6C ou 0x68, conforme o byte +0x7A do objeto em DAT_08064600), chama `StateMachine_DispatchStep`; ao terminar grava DAT_08064608 em +6 e, se os halfwords +0xA e +0xE baterem com esse valor, incrementa DAT_0806460C e chama `Obj_InitTimerRecord(objeto+0x50, 0, 0, 1, 0xC0)`. Confiança: provável.

### 0x08071F74 Anim_RampStepThenSfx54B
- `void Anim_RampStepThenSfx54B(void)`, 0x08071F74 - 0x08071FF7 (132 bytes). Gêmea de `Anim_RampStepThenSfx54` (0x08071EFC) com outros offsets: rampa em +0x144 (até 0x3E, depois 0x42), +0x15A (até 0x20), +0x142 (< 0x41); ao fim chama `slot_ptr_stub(9)`, `snd_sfx_request(0x54)`, grava 0x21 e sai por salto indireto. Confiança: provável.

## Sumário
- Pendentes de FL2 (6) e FL3 (0x0806F5A0): todos documentados e renomeados (ver FR1_simbolos.csv).
- FUN_ de FL2/FL3 (13 no total): 8 renomeados, 5 mantidos como FUN_ por chute (0x0800B79C, 0x08066460, 0x08000824, 0x08010B24, 0x0805C0D4).
- Xrefs de função (`get_function_xrefs`) quase todas vazias: alvos chamados só por ponteiro/tabela. Não foi possível fechar o subsistema pelos chamadores; a classificação é pelo corpo.
- Nota: a lista FL_lista/FL3 dava "4920 bytes" para 0x0806F5A0; o Body real é 0x0806F5A0-0x0806F8CD (814 bytes). Os tamanhos da lista são "até a próxima função" e incluem dados.
- Nota: `Math_Sf64CompareSafe` (0x0807DB34) e 0x0807DAE8 têm o mesmo corpo; o segundo ficou como `Math_Sf64CompareSafeAlt`.
- Não feito: merge no `re/symbols/mtbe.csv` (fica para depois, como nas outras faixas).
