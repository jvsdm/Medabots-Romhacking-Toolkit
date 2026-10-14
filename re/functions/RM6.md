# RM6: revisão de nomes de RAM (RM6_lista.txt, 14 itens)

## Trocas feitas
- 0x03006472: `gEvt_ContinuationIndex` -> `gCurrentEventOpcodeByte` (antes: gEvt_ContinuationIndex). Confirmado por EV.md (E+0xC92 = opcode atual), FI2.md e xrefs Script_DispatchHandler (WRITE). Renomeado no Ghidra via rename_data.

## Duplicata 0x03006472 vs 0x03006492
- 0x03006492 não tem xrefs no Ghidra. FI_simbolos registrou `gEvt_ContinuationIndex` com a nota "E+0xC92", mas E+0xC92 = 0x03006472. A linha 0x03006492 do mtbe.csv é provavelmente erro de transcrição.
- Resolução: o nome gEvt_ContinuationIndex fica só em 0x03006492 como "duplicata suspeita". Não foi renomeado. Sugestão: remover a linha 0x03006492 do mtbe.csv (quem é dono do mtbe.csv decide; não editei).

## Sem troca (mantidos, com achado)
- 0x0300643B, 0x0300643E: confirmados, nomes mantidos.
- 0x0300643D gOverworldEventModeFlags: nome mantido. Conflito: OW.md:30 chama o mesmo byte de gStepKindFlags (provável). Pendente de decisão.
- 0x030064F4, 0x030064FC, 0x03006506, 0x03006507, 0x0300650D, 0x0300650E, 0x0300650F: nomes mantidos, sem doc de conteúdo. Têm xrefs em Overworld_InitChoiceMenuScreen, Map_TickChoiceListOpen, FieldMenu_ChoiceSub2_SelectRow, Menu_StepCursorWrap e Map_CreateChoiceSprites (cluster de menu de escolha). Menu_StepCursorWrap acessa o estado por base de ponteiro (DAT_08057fac+0x65), não pelo endereço absoluto, então não serve para confirmar 0x0300650D sozinho.
- 0x0300650F: JT2.md:134 diz que é contador de Menu_ChoiceCountdownStep, o que conflita com o nome choice_flags. Pendente.
- Os nomes `gEventScriptRuntimeState_*` vieram do mtbe.csv (outro projeto); ainda não os padronizei para gCamelCase porque faltou confirmação de conteúdo.

## Fora do escopo (mantidos)
- sp_usr 0x03007F00, sp_irq 0x03007FA0: ponteiros de pilha do crt0 (RM3: não mexer).
- INTR_VECTOR 0x03007FFC: confirmado (MN.md) como endereço de IntrMain. Mantido.

## Em aberto
- Confirmar 0x0300650F (contador vs flags) e 0x0300650D (cursor_y) lendo Overworld_InitChoiceMenuScreen inteira.
- Entender o uso de 0x03006472 em Event_Op80/Op81 (também escreve/lê ali).
- Padronizar os nomes gEventScriptRuntimeState_* para gCamelCase quando houver prova do conteúdo.
