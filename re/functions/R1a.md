# R1a: funções 0x0802C644..0x08034E94 (33 funções)

Lista: `R1a_lista.txt`. Todas eram `FUN_` no Ghidra no início; 31 foram renomeadas no Ghidra (nomes do
`mtbe.csv` ou novos marcados como provável). Literais do pool lidos direto na ROM original (offset = addr - 0x08000000).
A struct de estado de combate usada pelos despachantes e getters é `0x03001800` (literal do pool; os campos
+0xA8, +0xAA, +0xB5, +0xBC são lidos aqui). O `R1.md` diz que o bloco de bot é `0x03000590 + bot*0x100`; a
relação entre `0x03001800` e esse bloco não foi verificada.

## 0x0802C644 Fight_DispatchOnSubstateAA_BI
- Assinatura: `void f(void)`
- O que faz: zera +0xB5 da struct 0x03001800 e chama o handler `tabela[+0xAA]` (tabela 0x083AE994).
- Chama: CallViaR0 / Chamada por: não verificado.
- Memória: escreve +0xB5; lê +0xAA.
- Para o Lua: idem (despacho por estado do combate).
- Confiança: confirmado (decompile).

## 0x0802CBE8 Fight_DispatchOnSubstateAA_BJ
- Mesmo padrão. Tabela de handlers 0x083AE9D4. Confiança: confirmado.

## 0x0802D688 Fight_DispatchOnSubstateAA_BK
- Mesmo padrão. Tabela de handlers 0x083AEA30. Confiança: confirmado.

## 0x0802DCB4 Fight_DispatchOnSubstateAA_BL
- Mesmo padrão. Tabela de handlers 0x083AEA88. Confiança: confirmado.

## 0x0802E424 Fight_DispatchOnSubstateAA_BM
- Mesmo padrão. Tabela de handlers 0x083AEB1C. Confiança: confirmado.

## 0x0802EBC0 Fight_DispatchOnSubstateAA_BN
- Mesmo padrão. Tabela de handlers 0x083AEB88. Confiança: confirmado.

## 0x0802F030 Fight_DispatchOnSubstateAA_BO
- Mesmo padrão. Tabela de handlers 0x083AEBC4. Confiança: confirmado.

## 0x0802F628 Fight_DispatchOnSubstateAA_BP
- Mesmo padrão. Tabela de handlers 0x083AEC04. Confiança: confirmado.

## 0x0802F990 Fight_DispatchOnSubstateAA_BQ
- Mesmo padrão. Tabela de handlers 0x083AEC58. Confiança: confirmado.

## 0x0802FE50 Fight_DispatchOnSubstateAA_BR
- Mesmo padrão. Tabela de handlers 0x083AEC98. Confiança: confirmado.

## 0x08031A3C Fight_BuildPartActionMetadataForAllParts
- Assinatura: `void f(u8 bot)`
- O que faz: grava o bot em +0x5C, zera +0x5D e chama 3 vezes (laço de 3 partes) `Fight_DispatchPartPreparation` e `Fight_DispatchPartExecution`, incrementando +0x5D.
- Chama: FUN_08031A70 (Fight_DispatchPartPreparation), FUN_08031ABC (Fight_DispatchPartExecution).
- Memória: escreve +0x5C, +0x5D da struct 0x03001800.
- Para o Lua: porta junto com as duas funções de despacho.
- Confiança: confirmado (decompile; nome do CSV confere).

## 0x08031A70 Fight_DispatchPartPreparation
- Assinatura: `void f(void)`
- O que faz: chama `Fight_ClearPartActionMetadata`, depois `FUN_0803f624(bot, 3, 0)` e então CallViaR0 com handler indexado pelo byte de técnica da peça (registro de 0x18 bytes, +0x49 dentro do bloco do bot de 0x100 bytes; base em DAT_08031AB8, não lido).
- Chama: Fight_ClearPartActionMetadata, FUN_0803F624 (não documentado aqui), CallViaR0.
- Memória: lê +0x5C/+0x5D (índices de bot/parte); lê o byte de técnica das peças.
- Para o Lua: idem (preparação por técnica).
- Confiança: provável (nome novo; a tabela de handlers não foi lida).

## 0x08031ABC Fight_DispatchPartExecution
- Assinatura: `void f(void)`
- O que faz: CallViaR0 com handler indexado pelo byte de técnica da peça corrente (+0x5C bot, +0x5D parte), mesmo cálculo de 0x08031A70.
- Chama: CallViaR0.
- Memória: lê +0x5C, +0x5D; lê o byte de técnica.
- Para o Lua: idem.
- Confiança: provável (nome novo; a tabela não foi lida).

## 0x08032F64 Fight_ClearPartActionMetadata
- Assinatura: `void f(void)`
- O que faz: zera +0x5E e +0x5F (escratch da ação) e, no registro da peça corrente, +0x56 e +0x57 (alvo bot e alvo parte).
- Chama: não chama outras. Chamada por: FUN_08031A70.
- Memória: escreve +0x5E, +0x5F; escreve +0x56, +0x57 no registro da peça.
- Para o Lua: porta.
- Confiança: confirmado (decompile; nome do CSV confere).

## 0x08032FB8 Fight_GetOpponentSideBaseIndex
- Assinatura: `int f(uint bot)`
- O que faz: retorna 3 se bot < 3, senão 0 (base do lado adversário).
- Confiança: confirmado.

## 0x08032FC4 Fight_GetAllySideBaseIndex
- Assinatura: `int f(uint bot)`
- O que faz: retorna 0 se bot < 3, senão 3 (base do próprio lado).
- Confiança: confirmado.

## 0x08032FD0 Fight_HasActivePartWithSpeciality
- Assinatura: `int f(int bot, uint tech)`
- O que faz: percorre as 3 peças (registro de 0x18 bytes) do bot; retorna 1 se alguma tem o campo +0x4A igual a `tech` e está íntegra (+0x59 != 0).
- Memória: lê bloco do bot (base DAT_08032FEC + 0x4A + bot*0x100 + part*0x18).
- Para o Lua: porta.
- Confiança: confirmado (decompile).

## 0x08032FFC Fight_HasAnyCandidateTarget
- Assinatura: `int f(void)`
- O que faz: retorna 1 se algum dos 3 bytes de alvo candidato em DAT_0803300C (fight_information[0x61..0x63]) é != 0.
- Confiança: confirmado.

## 0x0803301C Fight_PickRandomCandidateTargetSlot
- Assinatura: `int f(void)`
- O que faz: escolhe um índice aleatório (via `Fight_RandomSlotFromTable16`) e anda até 3 passos (direção sorteada por `randbyte`) procurando um slot candidato != 0 na tabela DAT_08033044; retorna o valor do slot achado (ou o primeiro valor se nenhum).
- Chama: FUN_08033074 (Fight_RandomSlotFromTable16), randbyte.
- Para o Lua: porta (usa rand; precisa do mesmo RNG para reproduzir).
- Confiança: provável (nome novo).

## 0x08033074 Fight_RandomSlotFromTable16
- Assinatura: `int f(void)`
- O que faz: sorteia `randbyte() & 0xF` e retorna o byte correspondente da tabela DAT_0803308C (16 entradas).
- Chama: randbyte. Chamada por: 0x0803301C, 0x08033090, 0x080330F4.
- Para o Lua: porta a tabela de 16 bytes (ler na ROM, literal DAT_0803308C) e o RNG.
- Confiança: provável (a tabela não foi lida).

## 0x08033090 Fight_PickRandomActiveBotInSide
- Assinatura: `int f(int side_base)`
- O que faz: sorteia um índice de lado (Fight_RandomSlotFromTable16) e anda até 3 passos (direção por `randbyte`) procurando um bot ativo (flag em +0x40 do bot `side_base + idx`, bloco de 0x100); retorna o índice achado ou `side_base + idx` do último passo.
- Chama: FUN_08033074, randbyte.
- Para o Lua: porta.
- Confiança: confirmado (decompile; nome do CSV confere).

## 0x080330E8 (sem nome, FUN_080330e8)
- Assinatura: `void f(void)`
- O que faz: só chama `FUN_0803556c` e retorna. Wrapper.
- Chama: FUN_0803556C (não documentado aqui; não tem nome no mtbe.csv).
- Para o Lua: depende de FUN_0803556C.
- Confiança: confirmado (wrapper). Nome não definido: deixado como FUN_.

## 0x080330F4 Fight_PickRandomIntactPart
- Assinatura: `int f(int bot)`
- O que faz: sorteia um slot de peça (via Fight_RandomSlotFromTable16, 0..2) e anda até 3 passos procurando uma peça íntegra (+0x59 != 0) no bloco do bot; retorna o índice.
- Para o Lua: porta.
- Confiança: confirmado (decompile; nome do CSV confere).

## 0x08033154 Fight_SetPartActionMetadata
- Assinatura: `void f(int bot, int part)`
- O que faz: copia o escratch da ação (+0x5E e +0x5F da struct 0x03001800) para +0x56 e +0x57 do registro da peça `part` do bot `bot`.
- Memória: lê +0x5E, +0x5F; escreve +0x56, +0x57 do registro da peça.
- Confiança: confirmado (decompile; nome do CSV confere).

## 0x08033184 Fight_PickMostDamagedRepairablePart1Based
- Assinatura: `int f(int bot)`
- O que faz: monta uma tabela de 4 entradas (usa `bios_cpufastset` para zerar) com a diferença de dano por peça (+0x4C menos +0x59) para peças com +0x59 != 0; ordena decrescente (`Sort_Records8Descending`) e retorna o slot 1-based da maior, ou 0 se nenhuma.
- Chama: bios_cpufastset (BIOS SWI 0x0C), Sort_Records8Descending.
- Para o Lua: porta a ordenação (ordem estável ou não: conferir ao portar).
- Confiança: confirmado (decompile; nome do CSV confere). Ponto em aberto: o cálculo exato de "dano" (+0x4C - +0x59) confere com a ROM, mas o slot 0 e empates não foram testados.

## 0x080331F8 Fight_HasNoTechnique16Part
- Assinatura: `int f(int bot)`
- O que faz: retorna 0 se alguma peça íntegra (+0x59 é `pcVar[0x10]` no decompile, técnica == 0x16 em +0x49); retorna 1 caso contrário.
- Confiança: confirmado (decompile; nome do CSV confere, retorno 0/1 invertido em relação ao nome "HasNo": ver código).

## 0x08033224 Fight_IsBotMedaforceInactive
- Assinatura: `bool f(int bot)`
- O que faz: retorna true se o byte +0xC4 do bloco do bot é 0.
- Confiança: confirmado.

## 0x08033240 Fight_IsBotPendingActionClear
- Assinatura: `bool f(int bot)`
- O que faz: retorna true se o byte +0xC9 do bloco do bot é 0.
- Confiança: confirmado.

## 0x0803325C Fight_PickRandomBrokenOrMissingPart1Based
- Assinatura: `int f(int bot)`
- O que faz: sorteia uma das 4 posições (head/right/left/legs) e anda procurando uma com +0x59 == 0 (peça zerada); retorna slot 1-based ou 0.
- Chama: randbyte (2x).
- Para o Lua: porta (usa RNG).
- Confiança: confirmado (decompile; nome do CSV confere).

## 0x080332B4 Fight_IsHeadActionUsable
- Assinatura: `bool f(int bot)`
- O que faz: retorna true se o byte +0x58 do bloco do bot é != 0.
- Confiança: confirmado.

## 0x080332D0 Fight_IsBotMedaforceTokenActive
- Assinatura: `bool f(int bot)`
- O que faz: retorna true se o byte +0xC1 do bloco do bot é != 0.
- Confiança: confirmado.

## 0x08033318 Fight_BeginPartActionValidation
- Assinatura: `int f(int bot, int part)`
- O que faz: zera +0x51 da struct 0x03001800; grava bot em +0x5C e (part-1) em +0x5D; chama CallViaR0 com handler indexado pelo byte de técnica da peça (part-1) do bot. Retorna +0x51.
- Memória: escreve +0x51, +0x5C, +0x5D; lê o byte de técnica da peça.
- Para o Lua: porta (início da validação de ação de peça).
- Confiança: confirmado (decompile; nome do CSV confere).

## 0x08034E94 (sem nome, FUN_08034e94)
- Assinatura: `u8 f(void)`
- O que faz: retorna o byte apontado por `tabela[+0xA8 da struct 0x03001800]` (tabela 0x083C6CA8, ponteiro de 4 bytes) + u16 em +0xBC.
- Memória: lê +0xA8 e +0xBC da struct 0x03001800; lê a tabela 0x083C6CA8.
- Para o Lua: porta a tabela (ler na ROM) e o acesso.
- Confiança: confirmado (decompile e literais 0x083C6CA8 / 0x03001800 lidos na ROM). Nome não definido (não está claro o que o campo representa): deixado como FUN_.
