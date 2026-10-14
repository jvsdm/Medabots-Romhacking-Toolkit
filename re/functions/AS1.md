# AS1: opcodes do roteiro de ação (action script), 0x01..0x35 — PARCIAL (1ª rodada)

Tabela: `gActionScriptOpcodeHandlerTable` 0x083AF180 (ROM EU, 4 B/entrada, thumb). Lida com `python -I` na ROM original.
Entradas (ponteiro, função = ponteiro - 1):

| op | entrada | função | nome no Ghidra | status |
|----|---------|--------|----------------|--------|
| 0x00 | 0x08000841 | stub `bx lr` 0x08000840 | — | sentinela |
| 0x01 | 0x080338DD | 0x080338DC | sem função criada | PENDENTE |
| 0x02 | 0x080338F5 | 0x080338F4 | Fight_WaitForActionScriptByteMatch | documentado |
| 0x03 | 0x08033975 | 0x08033974 | Fight_WaitForActionScriptChoiceByteMatch | documentado |
| 0x04 | 0x080339F5 | 0x080339F4 | sem função criada | PENDENTE |
| 0x05 | 0x08033A11 | 0x08033A10 | sem função criada | PENDENTE |
| 0x06..0x35 | ver tabela (0x08033A2D .. 0x08034509) | — | — | PENDENTE |

## Laço: Fight_RunActionScriptLoop 0x08033840 (confirmado, decompile)
- Antes de tudo: `Battle_CheckExitShortcut()`; se 0, faz o quadro (CpuFastSet se o modo é 2; desenha overlay de bots se o flag de +0x1AE é 0).
- Loop `do { ... } while (*flag == 0)`, onde `flag` = byte em `DAT_080338CC + DAT_080338D0` (é o `action_handler_complete_flag` +0xB5 de fight_information, HA4/HN9; confiança provável pelo offset).
- Cada passo: zera o flag; lê o opcode com `Fight_ReadActionScriptByte` (FUN_08034E94); enquanto o byte é >= 0x80 (rótulo/controle), chama `Fight_AdvanceActionScriptCursor` e relê. Guarda o opcode em `DAT_080338D4` (o byte de opcode fica no ponteiro, +0xA8 do bloco de script).
- Despacho: `CallViaR0(table[op])` com a tabela 0x083AF180 (`DAT_080338D8`).
- Termina quando o handler põe o flag em 1 (os handlers 0x02/0x03 fazem isso no fim; os demais: pendente).
- Ponteiro do script: `Fight_ReadActionScriptByte`/`Fight_AdvanceActionScriptCursor` usam fight_information +0xA8 (índice do script) e +0xAC..+0xBC (cursor e dados); ler com `FUN_08034E94`/`Fight_AdvanceActionScriptCursor` 0x08034F08 (R1b.md).

## 0x02 Fight_WaitForActionScriptByteMatch 0x080338F4 (decompile, confiança provável)
- Lê argumentos inline (`Fight_ReadActionScriptInlineArgs` 0x08034EB8), avança o cursor.
- Espera: a cada iteração compara o byte de fight+0xAC com o byte da tabela apontada por `table[fight+0xA8]` no índice do u16 de +0xBC; ao casar, avança e marca `bVar1`. Repete avançando o cursor até casar.
- Ao sair, grava 1 no flag de conclusão (+0xB5). Efeito real: é uma espera por condição (sincroniza o script com o estado da luta), não mexe em dados de batalha.

## 0x03 Fight_WaitForActionScriptChoiceByteMatch 0x08033974 (decompile, confiança provável)
- Igual ao 0x02, mas compara o byte de fight+0xAC + fight+0xAB (índice variável) em vez de só +0xAC. Também grava 1 no flag de conclusão ao casar.

## Pendentes nesta rodada
- 0x01, 0x04, 0x05 e todos de 0x06..0x35: Ghidra não tem função nesses endereços (decompile e disassemble falham). Próximo passo: ler os bytes da ROM (`python -I`) desses endereços, decodificar thumb à mão ou criar a função no Ghidra, e então documentar.
- Tabela de argumentos do roteiro de ação (bytes depois de 0x083AF380 não lidos nesta rodada; DT2 diz que é provável contagem de argumentos).
- Renomear handlers sem nome bom: nenhum renomeado nesta rodada (não há handler sem função a renomear ainda).
