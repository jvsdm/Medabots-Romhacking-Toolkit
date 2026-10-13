# Handlers chamados por ponteiro (H): despachantes e tabelas

Lista completa dos endereços: `H_lista.txt` (702 linhas, sem duplicatas e sem os que já têm seção em R0-R4).
Nenhum handler individual foi documentado nesta rodada: só os despachantes e as tabelas. Confiança das
contagens: "heurística" onde o tamanho da tabela não é verificado no código.

## 0x0801C4FC..0x0802C2AC Fight_DispatchOnSubstateAA_* (60 stubs, já em R1.md)
- O que faz: zera o byte +0xB5 do bloco e chama `table[+0xAA]` via trampolim `0x0807CCF0`. Mesmo código nos 60 stubs.
- Literais (em stub+0x20 e stub+0x24, confirmado em 0x0801C4FC e 0x0801F484): bloco = `0x03001800` (igual em todos),
  tabela = ponteiro próprio de cada stub, na faixa `0x083AD5CC`..`0x083AE954`.
- Tamanho das tabelas: heurística = distância até a tabela seguinte / 4 (gaps de 32 a 320 bytes). Não verificado.
- Confiança: confirmado (código lido nos stubs A e I; o resto por padrão de bytes).

## 0x0801A788 Fight_RunStateDispatch
- Tabela `0x0801A7B4`, 9 entradas (estado 0..8). Os 9 handlers (0x0801A7D8..0x0801A9C8) já são seções "miolo" em R1.md.
- Os ponteiros não têm bit Thumb (par), mas apontam para código Thumb válido (halfword 0x4808 etc.).

## 0x080393EC ObjectScript_OpcodeDispatch
- Tabela `0x083AF8C0` (literal em `0x0803942C`), indexada pelo opcode do objeto.
- Tamanho heurístico: 80 entradas válidas (corrida de ponteiros de código, cap 256). Não confirmado.
- 59 handlers novos em H_lista; `0x080395E8` (handler 8, SetMotionFromTable) já está em R2/A2.

## 0x0803FD28 ScriptVM_RunOpcodes
- Tabela `0x0803FD54` (literal `0x0803FD50`), 9 entradas para opcodes 0xF7..0xFF.
- Os alvos (0x0803FD78..0x0803FE54) são rótulos LOCAIS do switch dentro da própria função, não handlers
  separados. Por isso não entraram em H_lista.

## 0x08050BC8 Map_ModeDispatch (NÃO RESOLVIDO)
- Literal `0x08050C24` -> tabela `0x08050C28` (23 entradas, `cmp r1,#0x16`).
- Problema: as entradas apontam para `0x08050C84`/`0x08050C88`, dentro do próprio despachante (quase todas iguais).
  A leitura não bate com um switch real. Conferir no Ghidra antes de usar.

## 0x0805424C Scene_DispatchStateHandler (NÃO É TABELA DE HANDLERS)
- Literal `0x080542A8` -> tabela `0x083D5F70` (índice = `& 0x3F`, 64 entradas).
- As entradas apontam para `0x083D30D4`.. (dados; halfwords 0x0001), não para código. Provavelmente não é
  tabela de handlers. Nada foi adicionado à H_lista.

## 0x083D222C tabela de opcodes de evento (CORRIGE a nota de SND11)
- Indexada pelo opcode: entrada = `0x083D222C + 4*op`. Entradas válidas só em `op` 0x00..0x88 (137 handlers,
  todos novos em H_lista).
- 0x89..0xFF são ZERO nesta tabela, inclusive 0xD8..0xFF. A nota do SND11 (e a do SND2) diz que 0xD8..0xFF
  é a tabela de eventos: a leitura desta rodada contradiz isso. Quem mantém SND11 deve conferir.
- Exemplos: 0x30 -> 0x0804C478 (Relative_Long_Jump; entrada em 0x083D22EC), 0x44 -> 0x0804C94C
  (Play_Sound), 0x45 -> 0x0804C960 (Play_Music), 0x49 -> 0x0804C9C4 (Play_Map_Music).
- Nomes no Toolkit: `src/Medabots.Rom/Events/EventOpcodeDefinitions.cs` tem 125 definições (`[0xNN] = new
  EventOperationDefinition(0xNN, "Nome", ...)`). 12 entradas da tabela (0x00..0x88) não têm definição lá.
- Cobertura do PC (`pc/extract/extract_events.py`, estimativa por nome, não por opcode): decodifica cerca de 33 nomes
  de `op` (battle, close, jump, msg, stop, wait, fade, move_player, warp, shop, chapter, item, money, part,
  setflag, incflag, visible, yesno, branch, goto, end, remove_*, actor_*, face_*). Cerca de 41 nomes do Toolkit
  parecem cobertos (match por prefixo, não confiável). Não tratados pelo PC (exemplos sem casamento): Play_Sound
  (0x44), Play_Music (0x45), Stop_Music (0x46), Fade_Out_Music (0x47), Play_Map_Music (0x49), Random_Jump (0x51),
  Equip_Starter_Parts (0x62), Relative_Long_Jump (0x30), Start_Battle (0x33) como opcode, e a maioria dos
  Jump_If_* (0x4A-0x5C). Confirmar com o mapeamento opcode->nome antes de fechar a lista.
