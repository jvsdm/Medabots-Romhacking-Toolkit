# RD1: revisao de rotulos data/table (12 enderecos)

Trocas:
- 0x08001A00: era `gMapState_base` (data). Nao e dado: e codigo Thumb no meio da funcao 0x08001520, sem xref de dado. Virou `label` com nome chute `Obj_Mid_08001A00_InFunc08001520`. Ghidra nao renomeado (label de codigo).
- 0x0800467C, 0x0800E7D0, 0x0800E884, 0x0800E948, 0x0801FBEC, 0x0802845C: `data` -> `table`, com contagem confirmada (5, 7, 8, 8, 5, 29 entradas x 4).
- 0x080156F0: `gFightPriorityRowTable` -> `Fight_FallbackPriorityRowsPtr` (antes: gFightPriorityRowTable). E so o literal (ponteiro para 0x083AD470), nao a tabela. rename_data enviado ao Ghidra (retorno "attempted", nao reconferido).
- 0x080156C0..CC: mantidos; confirmados como literais de pool (1 x 4) via xref de Fight_SelectMedaforceActionSource.

Achados:
- Nao ha xref de dado para 0x08001A00; o comentario antigo de base 0x030057E0 provavelmente pertence a outra referencia (nao investigado).
- Tabelas de JT ja bate com as notas JT5/JT7/JT8 de mtbe.csv.
