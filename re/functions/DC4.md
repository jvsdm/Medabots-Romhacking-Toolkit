# DC4: correções de sentido de Mem_CopyBytes (SRC, DST, n) (DC4)

Base: decompile somente leitura no Ghidra (2026-10-14), sem salvar nem fechar o projeto.
Convenção confirmada: `Mem_CopyBytes(SRC, DST, n)` (0x0803F63C); `Mem_CopyBytes_Dup` (0x08041D04) igual.

## Correções

| arquivo:linha | antes | depois |
|---|---|---|
| DF_simbolos.csv:9 (0x08014F44, Fight_CopyBotIdentityRecords) | "copia bot+0x00..0x07 para tabela de 8B indexada por fi+0x41+i" | "copia tabela de 8B indexada por (fi+0x41+i)&0xF para bot+0x00..0x07 (DC4)". Decompile: SRC = tabela, DST = bloco do bot i |
| DF_simbolos.csv:11 (0x08014EE8, Fight_CopyBotSlotRecords) | "copia bloco do bot (0x40 B) para registro de medalha do roster 0x03000BE0" | "copia registro do roster 0x03000BE0 (0x40 B, indexado por Slot_FindByIdPlus1(id da peca)) para o bloco do bot i (DC4)". Decompile: SRC = registro do roster, DST = bot |
| R2_simbolos.csv:106 (0x0803E870, Overlay_CopyEightBytesForIndex) | "Mem_CopyBytes de 8 bytes para tabela indexada por +0x38*8" | "copia 8 bytes da tabela indexada por ctx+0x38*8 para ctx+0x39 (DC4)". Igual ao R2.md:789 |
| AI.md:153-154 (Fight_RandomizePriorityListSlot 0x080156D0) | "a leitura da origem (Mem_CopyBytes_Dup com src/dst) pode estar invertida" | Hipótese de inversão descartada: decompile = `Mem_CopyBytes_Dup(tabela ROM + (rand&0x1F)*4, fi+0x64, 4)`, origem = tabela. Continua em aberto só o conteúdo da tabela (valores até 0xF0) |
| HA3.md:356-357 (K_06, 0x08020004) | "chama FUN_0803F63C(0x03001800 + cat*0x100 + 0x.., DAT_0802003C, 0xE) (entrada de tabela por lado)" | "chama FUN_0803F63C(DAT_08020038 + cat*0x100, DAT_0802003C, 0xE) (copia 0xE bytes do bloco do bot do lado cat)". Base 0x03001800 não é do decompile |
| HA3.md:359 (K_06) | "Memória: 0x03001800 (+0x51, +0xA6, +0xAA); tabela por lado (cat*0x100 + DAT_08020038, não resolvido)" | "Memória: struct de fight DAT_08020034 (+0x51, +0xA6, +0xAA; endereço do literal não resolvido); bloco do bot por lado (DAT_08020038 + cat*0x100, base não resolvida)" |
| SU.md:172 (Y 0x0F, 0x08022FB4) | "FUN_0803F63C(bot alvo, dado, 0x0E) (mesmo efeito de M_05)" | "... (copia bot alvo -> dado; M_05 faz o inverso, tabela -> dado; DC4)". Decompile de Y_0F: SRC = bot alvo, DST = DAT_08022FEC |
| SU2.md:80 (Y 0x0F) | "0x0F reaplica o efeito do M_05 (FUN_0803F63C, id 0x0E)" | "0x0F copia 0xE bytes do bot alvo para o buffer (FUN_0803F63C(bot alvo, dado, 0xE); M_05 copia o sentido inverso, tabela -> buffer; DC4)" |
| HN1.md:151 (Y 0x0F) | "FUN_0803F63C(bot alvo, dado, 0x0E) (mesmo efeito de M_05)" | "... (copia bot alvo -> dado; M_05 faz o inverso, tabela -> dado; DC4)" |
| R2.md:775 (Overlay_CopyBufferAndLayoutText, 0x0803E7F0) | "copia 11 bytes para um buffer escolhido pelo índice em +0x38" | "copia 0xB bytes de um ponteiro escolhido pela tabela de ponteiros indexada por ctx+0x38 (4 B por entrada) para o buffer fixo DAT_0803E838 (DC4)". Decompile: SRC = ponteiro da tabela, DST = buffer fixo, 0xB; depois Text_LayoutGlyphSprites |
| R2.md:777 (0x0803E7F0) | "copia 0xB bytes para buffer indexado por +0x38" | "copia 0xB bytes da fonte indexada por +0x38 para o buffer fixo DAT_0803E838 (DC4)" |

## Verificado sem mudança

- M_05 (0x08020768): decompile = `Mem_CopyBytes(tabela[+0xA7]*4 + DAT_08020798, DAT_080207A0, 0xE)`, tabela -> buffer. Usado como referência acima.
- R2.md:789 (0x0803E870, "para ctx+0x39"): já estava correto.

## Em aberto

- R2_simbolos.csv: linhas 211 (0x08042BC0) e 222 (0x08044A3C) têm número de colunas diferente de 4 (CSV com problema de formato). Não tocadas.
- HA3_simbolos.csv:33 ainda diz "FUN_0803F63C (entrada por lado)". Não é invertido, mas é vago. Não tocado.
- PB.md:53 usa base 0x03000BA0 para o registro do roster; o CSV DF usa 0x03000BE0. Divergência não resolvida aqui.
- AI.md: conteúdo da tabela 0x083AD470 (valores até 0xF0) segue sem fechar, como já registrado.
