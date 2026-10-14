# QA5: falhas do MedabotsSync (4) e 88 "miolo" (Metabee EU)

Método: ROM original só leitura (python -I): BL/B/Bcc Thumb, ponteiros e LDR literal; Ghidra só leitura
(get_function_by_address nos hospedeiros). Sem commit, sem editar mtbe.csv, sem emulador.

## (1) Falhas de criação (4)

Causa: são DADOS (tabelas de ponteiros e pool de literais), não código. Ghidra recusa createFunction em dado.
Os 3 primeiros já têm rótulo (L) criado pelo sync; o `C` no TSV está errado.

| endereço | o que é | quem aponta |
|---|---|---|
| 0x08050CF4 SceneCommandStepTable | 5 ponteiros de passo (0x08050D08, D40, D72, F38, F40) | LDR literal em 0x08050CE4 (pool 0x08050CF0) |
| 0x08050E6C SceneCommandCodeTable | tabela de ponteiros do switch de comando | LDR literal em 0x08050E48 (pool 0x08050E68) |
| 0x08057534 FieldMenu_RowActionTable | 4+ ponteiros (0x080575D4, 0x0805754C, 0x08057564, 0x08057570) | LDR literal em 0x08057520 (pool 0x08057530) |
| 0x08066860 pool de Menu14_CallSubstateHandler | 1a palavra 0x080AFCC0 (LinkCommandHandlerTable), depois ponteiros IWRAM | LDR literal em 0x08066828 (função 0x08066824) |

Ação: remover as linhas `C` desses 4 do medabots_sync.tsv (manter só o `L` dos 3 primeiros). Não é função,
é rótulo/dado. Obs.: o mtbe.csv chama 0x08066824 de Menu14_CallSubstateHandler, mas o FL8 chama de
Fight_CallHandlerCopyArgs52: conflito de nome a resolver pelo dono do mtbe.

## (2) 88 "miolo" (dentro de outra função)

Conferência por ROM e pelos limites do Ghidra:
- Maioria (~75): alvo sem chamada (sem BL, sem prólogo PUSH), sem ponteiro próprio; o Ghidra juntou porque o
  corpo é contíguo ou porque o switch foi recuperado. Veredito: ROTULO (alt entry / caso / ponto de laço).
- Alvos de tabela de passos em Event_RunSceneCommandSequence (0x08050D08, D40, D72, F38, F40; corpo Ghidra
  0x08050CCC..0x08050F59): rótulo. Mesmo com `b` antes (D72, F40), o alvo é o item da tabela.
- Overworld_UpdateMapSceneStateMachine (corpo 0x08050F5C..0x0805139F): MapSub0..3 e 0x08051368 são casos da
  máquina de estados: rótulo.
- 0x08050F2A: recebe desvios de dentro do próprio host (0x08050D0C, D36, D50, E44): rótulo de laço/caso.
- BE_RollRandomStatus 0x0802B550: 3 desvios internos: rótulo (Loop).
- Falsos "BL" de dado: 0x080398EC (BL de 0x083564FE), 0x08051BC8 (BL de 0x0835D9E2), 0x0807C92C (BL de
  0x08295154). Esses endereços de origem são bytes de dado: não contam como chamada. Rótulo.
- 0x0806B826 (push r6,r7 no meio de Screen_InitObjectSlotsLz77): sem chamada, não é prólogo de função: rótulo.
- 0x080318DE e 0x080318E0 estão no mesmo host BR_47 (0x080318CC). O TSV deu a 0x080318DE o nome de BR_49
  (errado): rótulo com nome de BR_47.

### Hospedeira errada (dividir): só o grupo de handlers de tabela
Hospedeira Fight_DispatchOnSubstateAA_AV_ApplyStatusToOpponents (Ghidra 0x080287DC..0x080289FB). Cinco alvos
de uma tabela de 30 ponteiros (0x08028854..): 0x080288C8, 0x08028908, 0x08028944, 0x08028980, 0x080289BC.
Cada um vem depois de pool de literais (0x080288A8..0x080288C4), começa sem PUSH e é chamado só pelo
ponteiro: são funções. A hospedeira termina depois delas porque o corpo é contíguo. Para virar função:
encolher 0x080287DC para terminar antes de 0x080288C8 e depois criar os 5. Isso NÃO foi feito (sem
alterar Ghidra). Por isso estão em QA5_criar.txt, com aviso de dividir antes.

## Arquivos
- QA5_simbolos.csv: 92 linhas (88 miolo + 4 dados). Nomes gerados automaticamente (sufixo _AltEntry_ ou o
  nome do TSV com _Label); revisar antes de fundir no mtbe.csv (ex.: 0x08005A60 ficou "Parts_PrepareFourPartSlots_AltEntry_8005a60").
- QA5_criar.txt: 5 endereços (handlers de AV), exigem divisão antes.

## Em aberto
- Nomes do CSV são provisórios; os de substate (AA_*) pedem decisão de quem nomeou.
- Os falsos BL de dado (0x0835.., 0x0829..) apontam que o scanner Thumb pega bytes de dado; não usar BL de
  região de dado como prova.
