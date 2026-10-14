# DC3: sentido da cópia de Mem_CopyBytes (0x0803F63C)

Confirmado no Ghidra (decompile 0x0803F63C, 2026-10-14): `Mem_CopyBytes(SRC, DST, n)`, com `*param_2 = *param_1`.
Ou seja, o 1º argumento é a origem e o 2º é o destino. `Mem_CopyBytes_Dup` (0x08041D04) tem o mesmo sentido (confirmado no decompile).

Cada entrada abaixo: arquivo:linha -> antes -> depois. Só a frase com o sentido errado foi trocada; o resto da linha ficou igual.

## Corrigido

| Arquivo:linha | Antes | Depois |
|---|---|---|
| DF.md:67-68 | "copia o bloco do bot (0x40 B em bot+0x00..0x3F) de volta para o registro da medalha no roster ... sentido bloco -> save" | "copia 0x40 B do registro de medalha do roster para o bloco do bot ... (DC3: sentido SRC->DST; origem = 0x03000BE0 + slot*0x40, destino = bot+0x00)" |
| DF.md:76-77 | "copia bot+0x00..0x07 (cabecalho de identidade, 8 B) para a tabela de 8 B indexada pelo id do slot" | "copia o registro do time (0x030017A0 + t*8, 8 B) para bot+0x40..+0x47 (DC3: sentido SRC->DST; ver PB.md passo 3.3)" |
| DF.md:83 (tabela) | "Quem le" = Fight_CopyBotIdentityRecords para +0x00 | "- (DC3: Fight_CopyBotIdentityRecords nao le +0x00; copia o time para +0x40..+0x47)" |
| DF2.md:41-42 | "A origem de B+0x43..+0x46 do jogador (roster -> bloco) ... nao foi achada nesta rodada." | "(DC3: time -> bloco, em Fight_CopyBotIdentityRecords, SRC->DST) ... foi achada depois (ver PB.md, passo 3.3)." |
| DF2.md:65 (pendencias) | "(roster -> bloco no inicio da luta): nao achada." | "(DC3: time -> bloco no inicio da luta, via Fight_CopyBotIdentityRecords, SRC->DST; ver PB.md passo 3.3)." |
| PB.md:24 | "(DF2: copia bot+0x00..0x07 para tabela de 8 B por id)" | "(DF2 dizia 'copia bot+0x00..0x07 para tabela'; ERRADO, DC3: copia time -> bloco+0x40..0x47)" |
| PB.md:30 | "=> sentido BLOCO -> ROSTER (save): os 0x40 B do bloco do bot i voltam para o registro de medalha do roster" | "=> sentido ROSTER -> BLOCO (DC3: sentido SRC->DST; a leitura antiga estava invertida): os 0x40 B do registro de medalha do roster entram em bot+0x00..0x3F" |
| PB.md:31 | "Efeito no save: grava no registro de medalha o que estiver no bloco (bot+0x00..0x3F)" | "Efeito: o bloco do bot recebe do registro de medalha (bot+0x00..0x3F)" |
| PB.md:33 | "e a copia de trabalho e o roster recebe o estado de volta (pendente" | "e a copia de trabalho; nesta funcao o roster nao recebe nada de volta (pendente" |
| PB.md:35 | "Confianca: confirmado (leitura de Mem_CopyBytes(dst, src, n)" | "Confianca: confirmado (Mem_CopyBytes(src, dst, n), decompile 0x0803F63C; DC3)" |
| HA2.md:33-34 | "(origem = 0x030042B0 - 0xE40/0xE00/0xDC0/0xD80; a ordem dos argumentos dst/src é provável)" | "(DC3: sentido SRC->DST, decompile 0x0801D738: origem = 1º argumento = (bot-1)*0x100 + 0x030042B0 + k*0x40, destino = 2º = 0x03003470 + k*0x40; ordem confirmada, ver acima)" |
| HA5.md:28 | "copia 0x40 bytes duas vezes com FUN_0803F63C (em 0x03003470+0x00 e +0x40, com origens +0x40 e +0x80: ordem dos argumentos provável)" | "... (DC3: sentido SRC->DST, decompile 0x08021A6C: origem 0x03003470+0x00 para destinos +0x40 e +0x80)" |
| HA7.md:31 | "Copia 4x 0x40 bytes do bot (0x880..0x940) via FUN_0803f63c" | "Copia 4x 0x40 bytes de 0x880..0x940 para o bot (bot*0x100+0x1680..0x1740) via FUN_0803f63c (DC3: sentido SRC->DST, decompile 0x08024210)" |
| R2a.md:48 | "copia 8 bytes do campo +0x74 do objeto (DAT_08043E70) para a tabela em DAT_08043E74 + byte(+0x38) * 8" | "copia 8 bytes da tabela em DAT_08043E74 + byte(+0x38) * 8 para o campo +0x74 do objeto (DAT_08043E70) (DC3: sentido SRC->DST, decompile 0x08043E54)" |
| R2.md:787 (0x0803E870) | "copia 8 bytes de ctx+0x39 para a tabela de slots indexada por ctx+0x38" | "copia 8 bytes da tabela de slots indexada por ctx+0x38 para ctx+0x39 (DC3: sentido SRC->DST, decompile 0x0803E870)" |
| R2.md:789 (0x0803E870) | "Mem_CopyBytes de 8 bytes para tabela indexada por +0x38*8" | "Mem_CopyBytes de 8 bytes da tabela indexada por +0x38*8 para ctx+0x39 (DC3)" |
| R0.md:1361 (0x080121A8) | "tabela indexada por byte +0x38, 8 bytes por entrada, origem em +0x39" | "tabela indexada por byte +0x38, 8 bytes por entrada, DC3: sentido SRC->DST = tabela -> destino em +0x39" |

## Checado e sem mudança

- HN10.md:40 (0x080310A0): "Copia o nome do vencedor para T+0x9E" = correto (T+0x9E é o destino).
- HA7.md:38 (0x08024538): texto neutro ("entre ... ordem src,dst"); decompile confere.
- HA7.md:100 (AF 0x12) e HA2.md:110 (handler com "copia dados do bot e avanca"): consistentes com src->dst; não decompilados.
- HN1.md:151 e SU.md:172 (Y 0x0F, 0x08022FB4): decompile = Mem_CopyBytes(bot alvo, dado, 0xE), ordem bate com o texto.
- HN9.md:170 (BR 0x47): FUN_0803F63C([tabela], buffer, 0x11) = ROM -> buffer, consistente.
- R2.md:1348 e R2.md:1260/1265 (0x08042028 Slot_CopyRecordIntoFree, 0x08041D04): corretos (src -> dst).
- NM.md:10: "de um registro para o buffer do rótulo" = correto (origem = tabela, destino = +0x39).
- DF.md:107 (BR_50): "copia o nome, 0x03002B74 para 1 = jogador" = correto (origem 0x03002B74).

## Não editado (fora do escopo ou dúvida)

- DF_simbolos.csv:9 ("copia bot+0x00..0x07 para tabela de 8B indexada...") e :11 ("copia bloco do bot (0x40 B) para registro de medalha do roster") têm o mesmo sentido errado. Não editado (escopo: *.md). Corrigir na próxima rodada.
- R2_simbolos.csv:106 ("Mem_CopyBytes de 8 bytes para tabela indexada...") idem. Não editado.
- AI.md:154: "Mem_CopyBytes_Dup com src/dst pode estar invertida". A função 0x08041D04 está confirmada src->dst; a dúvida se reduz à leitura do call site da tabela 0x083AD470. Não editado (dúvida declarada, não frase errada).
- HA3.md:356 (K_06, 0x08020004): a chamada no texto (0x03001800+cat*0x100..., DAT_0802003C, 0xE) está na ordem do decompile (src = bloco do bot, dst = DAT_0802003c), mas a base 0x03001800 e a descrição "entrada de tabela por lado" não batem com o decompile. Não editado.
- SU.md:172, SU2.md:80, HN1.md:151: "(mesmo efeito de M_05)". Y_0F copia bot alvo -> dado; M_05 (0x08020768) copia tabela (ROM) -> DAT_080207a0. O sentido da origem é diferente. Não editado (é claim de equivalência, não de sentido de frase).
- R2.md:770-776 (Overlay_CopyBufferAndLayoutText), FL*.md e itens de R0 com "monta em buffer" não foram decompilados. Não há frase de sentido invertida óbvia neles.
