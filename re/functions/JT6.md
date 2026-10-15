# JT6: "Could not recover jumptable" em 0x0806B134, 0x0806B1FC, 0x0806B248, 0x0806E038, 0x0806E074 (2026-10-14)

Tarefa JT6 (vindo de FS9.md). Resultado principal: **nenhuma das cinco tem tabela de salto.** Cada uma termina em
`pop {rN}; bx rN` (retorno com o LR empilhado no prólogo), igual aos falsos positivos de JT.md. O Ghidra leu o
`bx` como salto indireto. As "tabelas" citadas no FS9 (0x084857D0, 0x084857E0, 0x084859A4...) não são saltos:
são **posições dentro de tabelas de ponteiros de função** (callbacks chamados por `call_r1`, 0x0807CCF4).

Leitura só pela ROM (`python -I`) e Ghidra (`disassemble_function`, `get_function_by_address`).

## Padrão de cada função (todas: sem add pc / ldr pc / mov pc)

| Função | Corpo | Fim | Literais (ROM) |
|---|---|---|---|
| 0x0806B134 | +0x1A < 0x20 -> +2++ e +4 por byte de 0x08485944+stage; senão ajusta +2 | `pop {r4,r5}; pop {r1}; bx r1` | 0x0806B16C = 0x03002C30 (bots, stride 0x40); 0x0806B170 = 0x08485924 (bytes) |
| 0x0806B1FC | ajusta +4 por faixa de +0x1A (<0x10 +2; 0xD2..0xE1 -2); se >0xE1 chama 0x0806B718 | `pop {r1}; bx r1` | 0x0806B238 = 0x03002C30 |
| 0x0806B248 | cor do bot (0x1000 ou (0x20-n)<<8 | n) por +0x1A; desce +4 se ímpar; zera contador se >0x3F | `pop {r4,r5}; pop {r1}; bx r1` | 0x0806B268 = 0x03002C30; 0x0806B26C/0x0806B2A0 = 0x04000052 (registrador de cor); 0x0806B2B4 = 0x04000050 |
| 0x0806E038 | timer +4 = 0xC8 ou 0x73 (bit0 de +0x22); +0x22++ | `pop {r4}; pop {r1}; bx r1` | 0x0806E058 = 0x03002C30 |
| 0x0806E074 | espera/piscar: estágio +0x22 até 8; sorteia tempo com rand (máscara 0x1FF + 0x20); usa flag 0x0300137C==9 | `pop {r4,r5,r6}; pop {r1}; bx r1` | 0x0806E0B8 = 0x03002C30; 0x0806E0D8 = 0x0300137C |

Os cinco são funções de **um bot por vez**: recebem o índice do bot em r0 e tratam o registro do bot.

## Quem chama (hospedeiro): 0x0806B0D4 (renomeada BotAnim_TickStageCallbacksAll)

- Loop sobre os bots 0..0x13 (20). Para cada um: byte `bot+0x20` = número do estágio (1..11).
- Se estágio != 0, chama `tabela[estágio-1]` via `call_r1` (0x0807CCF4) com r0 = índice do bot.
  Tabela = **0x084857D0** (literal 0x0806B104), 11 ponteiros de função (pode ser lida como tabela de 11 entradas).
  - Retorno != 0: bot+0x1A++ (avança a fase).
  - Retorno == 0 e bot+0x20 != 0: chama 0x0806B530 (não analisada aqui).
- Índice = bot+0x20 (byte), sem limite. Entrada 11 (0x084857F8) = 0x0806B130 (pequena função 0x2000 / bx lr).
- Tabela 0x084857D0 (estágio -> alvo):

| Estágio | Entrada | Alvo | Obs |
|---|---|---|---|
| 1 | 0x084857D0 | 0x0806B134 | (este doc) |
| 2 | 0x084857D4 | 0x0806B1A8 | começa com lsl r0,r0,#24 (código, sem push) |
| 3 | 0x084857D8 | 0x0806B1D0 | idem |
| 4 | 0x084857DC | 0x0806B1F8 | mov r0,#1 |
| 5 | 0x084857E0 | 0x0806B1FC | (este doc) |
| 6 | 0x084857E4 | 0x0806B248 | (este doc) |
| 7 | 0x084857E8 | 0x0806B2B8 | lsl r0,r0,#24 |
| 8 | 0x084857EC | 0x0806B2F4 | push {r4-r7,lr} |
| 9 | 0x084857F0 | 0x0806B3D8 | push (oscila +2; FS9 "bob") |
| 10 | 0x084857F4 | 0x0806B484 | lsl r0,r0,#24 |
| 11 | 0x084857F8 | 0x0806B130 | mov r0,#0; bx lr |

Nota de confiança: as entradas 2,3,4,7,10,11 não têm `push` no início (são código de 1 a 3 instruções ou
tail-call); não foram decompiladas nesta passada. Os nomes do estágio 1..9 ficam para a próxima rodada.

## E038 / E074 (0x0806E038, 0x0806E074)

- Referenciadas como entradas da tabela **0x08485988** (11 ponteiros): 0x08485988 = 0x0806DC7C,
  0x0848598C = 0x0806DCC0, 0x08485990 = 0x08000840 (rotina `bx lr` no início da ROM), 0x08485994 = 0x0806DE9C
  (Battle_BuildBotSpriteOam), 0x08485998 = 0x0806DFC0, 0x0848599C = 0x0806DFF0, 0x084859A0 = 0x0806E014,
  0x084859A4 e 0x084859A8 = 0x0806E038 (E038), 0x084859AC e 0x084859B0 = 0x0806E074 (E074).
- Tabela 0x08485988 **não tem literal exato** no ROM (só endereços dentro dela: 0x08485994 no pool de 0x0806DE68,
  0x084859B4 em 0x0806DE64, 0x084859BC em 0x0806E148). O consumidor (hospedeiro) não foi achado:
  o Ghidra não tem função em 0x0806DD80 nem em 0x0806DE20 e 0x0806E148 (região sem função definida); 0x0806DE9C é função desde o sync de 2026-10-14 (nome: Battle_BuildBotSpriteOam).
  **Em aberto.**
- O estágio de E038/E074 é o byte +0x22 do bot (E038 incrementa, E074 incrementa e reseta), o que bate com
  o hospedeiro acima usar bot+0x20 como índice. Não confirmado.

## Renomeações no Ghidra

- 0x0806B0D4 -> `BotAnim_TickStageCallbacksAll` (confirmado no corpo: loop de 20 bots, call_r1 por estágio).
- Os cinco alvos não renomeados (ficam como FUN_ / chute do FS9). Nada mais renomeado.

## Pendências

- Nomear estágios 1..9 da tabela 0x084857D0 (decompile de 0x0806B1A8, 0x0806B1D0, 0x0806B1F8, 0x0806B2B8,
  0x0806B2F4, 0x0806B3D8, 0x0806B484, 0x0806B130).
- Achar o consumidor de 0x08485988 (E038/E074).
- Decompile de 0x0806B530 (chamado quando o estágio retorna 0).
