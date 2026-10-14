# Tabelas de handlers e textos pendentes da ROM (Medabots Metabee EU) — GH-DT2

Continuação de `DT.md`. Leitura feita na ROM original `Medabots - Metabee (Europe).gba` (só leitura, Python -I).
Convenção: `ROM = 0x08000000 + offset`. Ponteiros de código têm bit thumb (ímpar); a coluna "alvo" mostra o endereço sem o bit.
Literal pool e leitores: busca de palavras da ROM com o endereço da tabela e de `LDR Rd,[PC,#imm]` (thumb e ARM)
apontando para elas; função "contém" só quando o corpo do Ghidra (`get_function_by_address`) cobre o endereço.

## 0x083AF8C0 gObjectOpcodeHandlerTable — handlers do roteiro de objeto (ObjectScript)
- Tamanho: 0x50 entradas × 4 B (0x140 B; 0x083AF8C0..0x083AFA00). Índice = opcode do objeto (`ctx+0x29`).
  Sentinela `0x08000841` (aponta para o stub `bx lr` em 0x08000840) em 0x00 e em 0x3B..0x4F. Em 0x50 começam palavras
  que não são ponteiros de handler (0x696B6A6A, 0x006A6C6C, depois ponteiros para outra área, 0x083AFA00...).
- Número de entradas: 58 handlers reais (opcodes 0x01..0x3A); 0x00 e 0x3B..0x4F são sentinela (sem efeito).
  Os 59 de HO1 ("0x00 a 0x3A") incluem o 0x00 sentinela.
- Formato: ponteiro thumb u32. Handlers 0x01..0x1D estão em `HO1.md` e 0x1E..0x3A em `HO2.md`.
- Usado por: `ObjectScript_OpcodeDispatch` 0x080393EC (única referência: literal 0x0803942C, instrução LDR em 0x08039410).
- Nome: `gObjectOpcodeHandlerTable` (renomeado no Ghidra com rename_data; antes `ObjectOpcodeHandlerTable` no mtbe.csv).
- Confiança: confirmado (tamanho pelo corte em 0x50 e pelos sentinelas; um único leitor no literal pool).

## 0x083AF180 gActionScriptOpcodeHandlerTable — handlers do roteiro de ação (action script)
- Tamanho: 0x80 entradas × 4 B (0x200 B; 0x083AF180..0x083AF37F). A partir de 0x083AF380 (índice 0x80) são bytes,
  não ponteiros (padrões `04 03 02 01`, `01 01 01 01`, ... até 0xBF; provavelmente tabela de contagem/argumentos, não lida).
- Índice = opcode do roteiro de ação. Entradas: 0x00 = `0x08000841` (sentinela); 0x01..0x6A = 106 handlers
  (ex.: 0x01 = 0x080338DD, 0x50..0x54 = 0x08034BA1, 0x08034BBD, 0x08034BD9, 0x08034BF5, 0x08034C11, os "AR..AV");
  0x6B..0x7F = sentinela `0x08000841`.
- Número de entradas com handler: 106 (0x01..0x6A). Tamanho da tabela 0x80: provável (corte pelo início dos bytes).
- Usado por: `Fight_RunActionScriptLoop` 0x08033840 (única referência de literal: 0x080338D8, LDR em 0x08033882).
- Nome: `gActionScriptOpcodeHandlerTable` (já no mtbe.csv; renomeado no Ghidra com rename_data).
- Confiança: confirmado (ponteiros, leitor e sentinelas); o tamanho 0x80 é provável; a tabela de bytes depois dela não foi lida.

## 0x083D222C gEventOpcodeTableA — handlers primários dos opcodes de evento
- Tamanho: 256 entradas × 4 B (0x400 B; 0x083D222C..0x083D262B). Índice = opcode do evento (`entrada = base + 4*op`).
  Logo depois, em 0x083D262C, há uma palavra de dados (0x02030001) que não é entrada; a tabela B começa em 0x083D2630.
- Entradas: 0x00..0x88 = 137 ponteiros thumb (ex.: 0x00 = 0x0804B73D, 0x30 = 0x0804C479, 0x44 = 0x0804C94D,
  0x88 = 0x0804D7F9). **0x89..0xFF = zero (null)**, inclusive 0xD8..0xFF. Ou seja, a tabela não tem handler
  nem efeito para 0xD8..0xFF.
- Correção: a nota de SND2/SND11 ("0xD8..0xFF = comandos de música ou da tabela de eventos") não bate com a ROM. A tabela
  real termina no 0x88; 0xD8..0xFF não são entradas dela. Quem mantém `SND2_som.md`/`SND11_sequencia.md` deve corrigir.
- Usado por (literal pool; três leitores, todos thumb):
  - `EventScript_FetchAndRunPrimaryOpcodeHandler` 0x0804B6F4 (literal 0x0804B734, LDR em 0x0804B710) — o leitor principal.
  - `Event_RunEmbeddedInitOpcode` (corpo perto de 0x0804D59C; literal 0x0804D5D8, LDR em 0x0804D5BA).
  - `EventOpcode80_BeginBatchedObjectCommandBlock` 0x0804D4FC (literal 0x0804D57C, LDR em 0x0804D558; corpo confirmado no Ghidra).
- Nome: `gEventOpcodeTableA` (mantido; no mtbe.csv Simon: `sEventPrimaryOpcodeHandlerTable`). Renomeado no Ghidra com rename_data.
- Confiança: confirmado (tamanho 0x100 entradas pelo fim em 0x083D262C; 137 entradas não nulas; leitores pelo literal pool).
  Nome dos leitores 2 e 3: provável (csv), corpo do leitor 2 não confirmado no Ghidra.

## 0x083D2630 gEventOpcodeTableB — handlers de continuação dos opcodes de evento
- Tamanho: 256 entradas × 4 B (0x400 B; 0x083D2630..0x083D2A2F). Índice = opcode (`entrada = base + 4*op`).
- Entradas: 0x00..0x88 = 137 ponteiros thumb; **0x89..0xFF = zero**. Muitas entradas apontam para o mesmo stub
  0x0804DC34 (`NoOp`, 53 opcodes; ex.: 0x00, 0x06, 0x0D, 0x12..0x19). Lista completa e nomes em `HC.md`.
- Usado por: `Event_DispatchContinuation` 0x0804DC0C (literal 0x0804DC28, LDR em 0x0804DC0E), que indexa por `E+0xC92`.
- Nome: `gEventOpcodeTableB` (já no mtbe.csv; Simon: `sEventContinuationOpcodeHandlerTable`). Renomeado no Ghidra com rename_data.
- Confiança: confirmado (tamanho, entradas, único leitor). O tamanho real é 256 entradas, como A; os 137 alvos vêm de `HC.md`.

## 0x083C2D3C gTextPreBattleMessage — texto de antes da batalha (tabela de ponteiros)
- Tamanho: 96 ponteiros u32 (0x180 B; 0x083C2D3C..0x083C2EBB). Índice = id da mensagem (0x00..0x5F).
- Formato: ponteiro para string na ROM, sem bit thumb, em ordem crescente. Os textos ficam ANTES da tabela
  (0x083C1F58..0x083C2D3B; ex.: entrada 0x00 = 0x083C1F58, 0x5F = 0x083C2D18). Sem terminador da tabela: a tabela
  seguinte começa imediatamente em 0x083C2EBC (texto pós-batalha, ver abaixo).
- Usado por (literal pool, dois leitores, thumb):
  - literal 0x08010850 (valor 0x083C2D3C) lido em 0x0801081A. O corpo de `Fight_InitBotPartSelectScreen` 0x0801058C (Ghidra:
    0x0801058C–0x080106E9) cobre esse endereço: função que lê = `Fight_InitBotPartSelectScreen` (confirmado pelo corpo, nome provável).
  - literal 0x0803D9C0 lido em 0x0803D9A6. Função que contém: não confirmada (o corpo de `Fight_ScrollEntryCoords` 0x0803D4EC
    termina em 0x0803D537 e não cobre; a carga é no meio de código não definido no Ghidra).
- Nome: `gTextPreBattleMessage` (já no mtbe; fonte `dump_text_clean_v2.py`). Renomeado no Ghidra com rename_data.
- Confiança: confirmado (tamanho, formato e texto nos ponteiros); leitor 1 confirmado pelo corpo; leitor 2 em aberto.

## 0x083C3B04 gTextPostBattleMessage — texto de depois da batalha (tabela de ponteiros)
- Tamanho: 96 ponteiros u32 (0x180 B; 0x083C3B04..0x083C3C83). Índice = id da mensagem (0x00..0x5F), mesmo formato da PRE.
- Formato: ponteiros para strings que ficam antes da tabela, em 0x083C2EBC..0x083C3AE9 (entrada 0x00 = 0x083C2EBC,
  0x5F = 0x083C3AE9). Sem bit thumb, crescente.
- Usado por (literal pool, dois leitores, thumb):
  - literal 0x08010C14 (valor 0x083C3B04) lido em 0x08010BDA. Função que contém: não confirmada. O corpo de
    `Overworld_ReloadMapSceneAndAdvanceGameState` 0x08010A7C (0x08010A7C–0x08010ABD) e o de `Battle_GaugeTickStep`
    0x08010B54 (0x08010B54–0x08010B9B) não cobrem 0x08010BDA. Aberto.
  - literal 0x0803DB84 lido em 0x0803DB52. `Fight_ClearBattleBgAndBlend` 0x0803DA90 (corpo 0x0803DA90–0x0803DADF) não cobre.
    Aberto (mesmo caso da PRE, na mesma faixa de batalha).
- Nome: `gTextPostBattleMessage` (já no mtbe; fonte `dump_text_clean_v2.py`). Renomeado no Ghidra com rename_data.
- Confiança: confirmado (tamanho, formato); quem lê: confirmado só o endereço da carga, não a função.

## Pendências
- Os dois leitores de texto sem função confirmada (0x0801081A/0x0803D9A6 para PRE; 0x08010BDA/0x0803DB52 para POST) precisam
  de `decompile_function_by_address` no miolo (o Ghidra não tem o corpo que cobre essas cargas).
- Nenhum handler individual novo foi documentado nesta rodada (já estão em HO1/HO2/HC/HE*).
