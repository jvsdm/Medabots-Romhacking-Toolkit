# RF4 - trocas e achados

Lista: RF4_lista.txt (34 itens, todos marcados "dado"; na prática são todos labels de código dentro de
Fight_J02 e Fight_DispatchOnSubstateAA, então o kind gravado é `label`/`table`). Detalhe em RF4_simbolos.csv.

## Trocas de nome
- 0x080318DE: `Fight_DispatchOnSubstateAA_BR_47_LoadMedaforceName_Entry` (antes:
  `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction`). Confiança: provavel. Está no meio do host BR_47
  (push em 0x080318CC). Nos bytes é só leitura de B+0xA8 (add #0xA8 + ldrb), sem store.
- Demais 33 itens: nome mantido (já estavam em mtbe.csv com o nome certo).

## Achados
- Tabela 0x08028854 (AV_ApplyStatusToOpponents) tem 29 entradas x4 (tecnica 12..40), não 30 como em QA5.md.
  Última entrada em 0x080288C4; 0x080288C8 (AV_0D) começa logo depois, sem sobreposição. Idx 0 = AV_0D,
  idx 2 = AV_10, idx 4 = AV_12, idx 6 = AV_13, idx 28 = AV_2C. Default da tabela: 0x080289E4.
- Comentário de mtbe em 0x080318DE ("grava fight+0xA8 em 0x08031940") não vale para aquele endereço; o store
  de medaforça está no BR_49 (0x08031938). Ajuste no mtbe fica para quem edita o CSV de origem.
- J_02: as entradas 3 e 4 da tabela 0x0801FBEC (ptrs em 0x0801FBF8 e 0x0801FBFC) são 0x0801FD10 e
  0x0801FD3C. Confirmado nos bytes. Tabela de 5 entradas x4.
- 0x0802B550 (BE_RollRandomStatus): label confirmado por 3 desvios internos (0x0802B532, B53C, B546).
- Tabelas JT5 conferidas nos bytes (stride 4): 0x08027A34 = 5; 0x08028534 = 29; 0x08028AD8 = 6;
  0x0802845C = 29 (já em mtbe). Literal 0x03000590 em 0x08028450 e 0x08028458 aponta para 0x0802845C.
- 0x0801FD70 e 0x0802040C são `bx r0` (retorno), confirmados nos bytes.

## Em aberto
- 0x08030E8C (BR_50_CheckLeadersDefeated): nome chute. O corpo chama Fight_SelectBotActivePart; ninguém
  decompilou este passo.
- 0x080288C8, 0x08028908, 0x08028944, 0x08028980, 0x080289BC: têm cara de função (sem PUSH, só chamados por
  ponteiro), mas continuam como labels aqui. Criar como função depende de encolher 0x080287DC (QA5_criar.txt).
- Não foi usado Ghidra nem renomeado nada no Ghidra: todos os itens desta lista são labels.
- 0x080308BC (BR_10): bl para Fight_ResetBotPartFlags vem de TR.md; o destino do bl não foi conferido nos bytes.
