# QA2: limites de função (endereços que caem dentro de outra função no Ghidra)

Verificado com `get_function_by_address` e `disassemble_function` no Ghidra (sem salvar nem alterar nada) e com a
tabela de jump tables lida só da ROM (`python -I`, leitura).

Veredito geral: em nenhum dos 4 grupos o Ghidra juntou funções erradas. Os limites das funções estão corretos
(cada função termina em `pop {..,pc}`/`bx`). Os endereços listados são **labels internos** (alvos de jump table,
ou pontos de chamada dentro de um handler). O problema está no `mtbe.csv`: marcam como `func_thumb` e, em dois
casos, usam nome copiado de outro opcode. Nada precisa ser dividido, então não há `QA2_split.txt`.

## (a) Parts_DispatchSlotAction (0x08006F34)

- Função válida, 0x08006F34..0x08006FAE, termina em `bx lr`. Despacho por jump table de 5 entradas
  (literal 0x08006F60 -> tabela 0x08006F64): 0x08006F78, 0x08006F84, 0x08006F8A, 0x08006F90, 0x08006F9E.
- 0x08006F44 é o trecho da cabeça que lê o byte de ação em +0x5A (`add r2,#0x5a; ldrb`). Não é entrada de
  jump table nem função. O nome `PartSet_UpdateFromCurrentLoadoutMaybe` não bate com o código.
  Vira label: `Parts_DispatchSlotAction_ReadActionIndex`.
- 0x08006FA0 é o trecho do caso 0x08006F9E que lê o byte +0x9B e compara com 0x77. Não é função. O nome
  `PartSet_EquipCurrentLoadoutMaybe` não bate com o código. Vira label: `Parts_DispatchSlotAction_CheckByte9B`.

## (b) Fight_RunStateDispatch (0x0801A788)

- Função válida, 0x0801A788..0x0801ABF4, termina em `bx r1`. O despacho é por tabela de 9 entradas
  (literal 0x0801A7B0 -> tabela 0x0801A7B4), e as entradas são exatamente os endereços do mtbe:
  A7D8, A818, A85C, A8BC, A910, A950, A958, A998, A9C8. Todos são alvos de caso (o caso termina em
  `b` para o fim ou em `b 0x0801abec`), não funções.
- Os State0..State8 viram `kind label`. Existe um problema extra: 0x0801A998 (state 7) estava com o nome
  `Fight_RunStateDispatch`, igual ao da função principal. Corrigido para `Fight_RunStateDispatch_State7`.
- Os nomes de carga de gráfico e as contagens ("Carrega N gráficos") do mtbe estão no comentário dos
  labels, sem mudança de conteúdo.

## (c) Opcodes 0x4B, 0x4C, 0x4D (tabela 0x083D222C)

Leitura da tabela (u32 LE, entrada = 0x083D222C + 4*op; o bit Thumb é o último):

| opcode | entrada | função correspondente |
|--------|---------|------------------------|
| 0x4B | 0x0804CA29 | 0x0804CA28 Event_Op4B_Jump_If_Item_Count_Below_Amount (CSV correto) |
| 0x4C | 0x0804CA85 | 0x0804CA84 Event_Op4C_Jump_If_Missing_Part (CSV correto) |
| 0x4D | 0x0804CAE9 | 0x0804CAE8 Event_Op4D_Jump_If_Not_Has_Medal (CSV correto) |
| 0x81 | 0x0804D581 | 0x0804D580 Event_Op81_Queue_Batched_Object_Command (CSV correto) |
| 0x02 | 0x0804B74D | 0x0804B74C (não verificado aqui, só a entrada) |

Os nomes das funções principais estão certos. O erro está nos labels internos:

- **0x0804CAD8** fica dentro de Op4C (0x0804CA84..0x0804CAE0). É o trecho que roda quando
  `bl 0x08047E50` devolve -1 (parte ausente): soma o byte de salto ao ponteiro de script. Não é a função do
  opcode 0x4B. O nome `EventOpcode4B_JumpIfItemCountBelowAmount` (e a descrição do Simon/Rokusho) é da
  Op4B em 0x0804CA28. Corrigido para label `Event_Op4C_TakeJumpIfPartMissing`.
- **0x0804CB04** fica dentro de Op4D (0x0804CAE8..). É o ponto onde chama `bl 0x08047DB0` (checagem de medalha);
  se o resultado for 0, aplica o salto. O nome `EventOpcode4C_JumpIfMissingPart` é o da Op4C. Corrigido para
  label `Event_Op4D_CallHasMedalCheck`.

Nomes no Ghidra: não renomeei nada. Esses dois endereços não são função (`get_function_by_address` devolve
"No function found"), então `rename_function_by_address` não se aplica. Os nomes corretos ficam só no
`QA2_simbolos.csv` como `kind label`.

## (d) Event_Op81_Queue_Batched_Object_Command (0x0804D580)

- Função válida, 0x0804D580..0x0804D5CC, termina em `bx r0`.
- **0x0804D584** é o `bl 0x0804D8CC` com r0=8 (leitura de bloco de 8 argumentos). Não é a entrada do opcode 0x02.
  O nome `EventOpcode02_RunEmbeddedInitOpcodeWithSavedArgs` com a descrição "Normal event opcode 0x02" está
  errado para este endereço. Label: `Event_Op81_CallReadArgs8`.
- **0x0804D59C** é o cálculo do registro salvo: `r1 = base + 0x12 + 8*byte` seguido de cópia de 8 bytes
  (`bl 0x080497C0`). Não é `Event_RunEmbeddedInitOpcode`. Essa descrição (ponte para a tabela de inicialização)
  pertence a outra função, não confirmada aqui. Label: `Event_Op81_SelectSavedArgRecord`.

## Em aberto

- A descrição "Normal event opcode 0x02" (e o nome `EventOpcode02_...`) e a de `Event_RunEmbeddedInitOpcode`
  parecem ser de funções reais em outro lugar. Eu não localizei o endereço delas. Vale procurar a entrada 0x02
  (0x0804B74C) e os callers de `0x0804D8CC` / `0x080497C0` na próxima rodada.
- Os labels de (a) e (b) e os de (c)/(d) não têm referência de Ghidra. Se alguém criar labels no Ghidra
  depois, os nomes do CSV podem ser usados como guia.
