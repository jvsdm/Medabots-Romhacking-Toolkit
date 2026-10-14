# SV2 - layout exato do save (GH-SV2, 2026-10-14)

Status: CONFIRMADO no código (decompile + disassembly + literal pools lidos da ROM EU, somente leitura).
Não testado com savestate (sem emulador nesta tarefa). Não lido: nenhum `.sav` do usuário.
Substitui a parte "mapa" de SV.md onde houver diferença (ver seção "Conflito resolvido").

Fontes: `SaveLoad_SaveGame` 0x0805E010 e `SaveLoad_LoadGame` 0x0805DF18 (decompile, pools 0x0805DFD0 e 0x0805E0EC),
`Save_ReadVerify` 0x0805E138 (decompile + disassembly completo), `Save_WriteBlocksEeprom` 0x0805E248 (decompile),
`EventFlag_IsSetByPackedId` 0x080531E0 (decompile + pool 0x08053218/0x08053200 lidos na ROM).
`lib_memcpy` = FUN_08041D04 (memcpy de bytes, README das funções). `bios_cpuset` = wrapper de CpuSet
(args: src, dst, ctrl; ctrl sem bit 26 = unidades de 16 bits).

## Regra geral

- Buffer de staging: EWRAM 0x02030000, 0x1CC0 B = 0x398 blocos de 8 B da EEPROM (SaveGame só preenche 0x1CBC + 4 de padding).
- Todas as cópias usam o buffer lógico (desinvertido). A inversão de bytes de 8 B da EEPROM fica no `.sav`, fora desta análise.
- `lib_memcpy(dst, src, n)`: no SaveGame o 1º argumento é o save (dst); no LoadGame o 1º é a RAM (dst). Ordem confirmada nas duas funções.

## Tabela save-offset -> RAM

| offset save | tamanho | RAM (origem em SaveGame) | conteúdo | prova |
|---|---|---|---|---|
| 0x000 | 16 | ROM 0x084836B4 (não é RAM) | assinatura "MEDAROT2 Ver.KBT" (ReadVerify valida 13 B: 0..12) | SaveGame: loop 0x10 bytes de pool 0x0805E0F0; ReadVerify: loop `cmp r6,#0xc` (0x0805E188) |
| 0x010 | 4 | 0x03002B70 | dinheiro u32 (limite validado ≤ 99999 = 0x1869F) | SaveGame: 7 words de pool 0x0805E0EC -> +0x10..+0x27; ReadVerify `ldr r1,[r7,#0x10]` vs 0x1869F |
| 0x014 | 16 | 0x03002B74 | nome do jogador (0xFE = fim; 0 = vazio aceito) | idem (word +0x14..+0x23); ReadVerify checa [0x14..0x23] com 0xFE/0 |
| 0x024 | 2 | 0x03002B84 | total de lutas u16 (≤ 9999 = 0x270F) | idem (+0x24); ReadVerify `ldrh r1,[r7,#0x24]` vs 0x270F |
| 0x026 | 2 | 0x03002B86 | vitórias u16 (≤ total) | idem (+0x26); ReadVerify `cmp r1,r0; bcc` (wins ≤ total) |
| 0x028 | 4 | 0x03002B88 | tempo de jogo u32 em quadros (≤ 0x14996FF = 21.599.999) | idem (+0x28); ReadVerify `ldr r1,[r7,#0x28]` vs 0x14996FF |
| 0x02C | 0x780 | 0x03000BE0 | 30 medalhas x 0x40 B (id em +1 do registro) | SaveGame: loop de 30 iterações (`iVar8=0x1d` decrescendo) com lib_memcpy 0x40 |
| 0x7AC | 0x780 | 0x03004C40 | inventário de peças: 960 bytes da RAM, **cada byte vira um u16** no save (2 B por entrada) | SaveGame: `*puVar4 = (ushort)*(byte*)(iVar12+iVar2)`, iVar12 de 0 a 959, puVar4 += 1 (ushort) |
| 0xF2C | 0x48 | 0x030017A0 | equipe 9 registros x 8 B | SaveGame: 9 iterações (`iVar12=8`..0) de 8 B; pool 0x0805E108 e 0x0805E10C=0xF2C |
| 0xF74 | 0x80 | 0x03002BA0 | itens: 64 pares [0x80|id, qtd] | SaveGame: `bios_cpuset(0x03002BA0, save+0xF74, 0x40)` = 0x40 hw = 0x80 B |
| 0xFF4 | 2 | — (calculado) | checksum u16 = soma de bytes 0x000..0x1CBB (0x1CBC bytes) com o próprio campo zerado | SaveGame: zera [FF4..FF5], soma `uVar10 <= 0x1CBB`, grava. ReadVerify disasm 0x0805E1F0: `ldrh r4,[r1]` (lê o gravado) ANTES de `strh r0=0` (0x0805E1F8); compara `r2 == r4` (0x0805E20E) |
| 0xFF8 | 0xCC0 | 0x030057E0 | bloco de estado de eventos E (+0x000..+0xCBF) | SaveGame: `bios_cpuset(0x030057E0, save+0xFF8, 0x660)` = 0x660 hw = 0xCC0 B; LoadGame: idem inverso (pool 0x0805DFF4/0x0805DFF8) |
| 0x1CB8 | 1 | 0x03001788, 0x03001360, 0x03000B94, 0x03001784 | opções em bits (ver abaixo) | SaveGame: `(B1360<<2) | B1788 | (B0B94<<3) | (((B1784&3)>>1)<<5)`; LoadGame: inverso (`&3`, `>>2&1`, `>>3&3`, `>>5&1`) |
| 0x1CB9..0x1CBB | 3 | — (não escrito por SaveGame) | entram na soma do checksum; SaveGame não os preenche, valor herdado do staging EWRAM | soma vai até 0x1CBB (ver FF4) |
| 0x1CBC..0x1CBF | 4 | — | padding; não somado, não validado; vai para a EEPROM como parte do bloco 0x398 | ReadVerify/WriteBlocks usam 0x398 blocos |

Bits de opções (0x1CB8), pela ordem do código:

| bit | RAM | valor | nota |
|---|---|---|---|
| b0-1 | 0x03001788 (0..2) | opção de 2 bits | SV.md: "luta: contador" (OP.md, linha 0 do menu de opções) |
| b2 | 0x03001360 (0/1) | opção de 1 bit | OP.md: gate de `Fight_TryUiActionByIndex`. Conflito de nome com A6, não resolvido |
| b3-4 | 0x03000B94 (0..2) | opção de 2 bits | OP.md linha 2 |
| b5 | bit 1 de 0x03001784 | flag | Load escreve 0x03001784 `&= ~2 | bit<<1` |

Obs.: `0x03001360 << 2` não é mascarado. Se o valor passar de 1, polui b3+. Não é problema hoje (o menu grava 0/1).

## Conflito resolvido: 0x030057D8 (flags de evento) x checksum

- 0x030057D8 é **base de array de flags** (não ponteiro): o pool da ROM em 0x08053218, 0x08053200, 0x08053260,
  0x0805327C, 0x08053184, 0x0805319C contém o valor 0x030057D8 direto. Leitura: `byte[0x030057D8 + (id>>1)]` (id até 0xFFF).
  Array = 0x030057D8..0x03005FD7 (0x800 B). Bit de id par = bit 7; ímpar = bit 3 (EventFlag_IsSetByPackedId 0x080531E0).
- **Não é linear no save.** SaveGame começa a cópia de E em 0x030057E0 (save +0xFF8). Os 8 primeiros bytes do array
  (0x030057D8..0x030057DF = ids 0..15) **não são copiados** para o save. Não cai em +0xFF0 e não colide com o checksum.
- Para k = (id>>1) >= 8, a posição no save é `0xFF0 + k` (ex.: k=8 -> 0xFF8, k=0x7FF -> 0x17EF).
  Para k < 8 não existe posição no save: as flags de ids 0..15 não persistem (ver "Em aberto").

## Outros endereços de save em E (derivados do mapa linear)

| RAM | save | tamanho | conteúdo | confiança |
|---|---|---|---|---|
| 0x03005FDE | 0x17F6 | 1 | gCurrentMapId (E+0x7FE) | provável (mtbe.csv) |
| 0x03005FDF | 0x17F7 | 1 | gCurrentChapter (E+0x7FF) | confirmado pela cópia linear de E; PR.md |
| 0x03005FD8 | — | — | fim do array de flags (fora da cópia de E? não: 0x03005FD8 está em E, save 0x17F0) | confirmado pela cópia linear |

Obs.: 0x03005FD8 está dentro de E (E = 0x030057E0..0x030064A0), logo é salvo em 0x17F0. O "fim do array" é só rótulo.

## Validação na leitura (Save_ReadVerify)

1. Lê os 0x398 blocos de 8 B para o buffer (lib_eeprom_read64 em loop).
2. Assinatura: 13 bytes (0..12) contra ROM 0x084836B4 (pool 0x0805E218). Diferente -> retorno 1.
3. Nome (+0x14): 0xFE no início, ou 0 seguido de 0 até 0xFE/fim em até 16 bytes. Senão falha.
4. money ≤ 99999; total ≤ 9999; wins ≤ total; tempo ≤ 21.599.999; opções b0-1 < 3 e b3-4 < 3.
5. Checksum: soma 0x1CBC bytes (0..0x1CBB) com FF4 zerado, comparada com o u16 gravado (lido antes de zerar).
   Retorno 0 = OK; -1 = outra falha.

Atenção: o decompile do Ghidra mostra a comparação com 0 (zerar antes de ler). Está errado: o disassembly confirma a leitura
antes do `strh`. SV.md já tinha esse aviso.

## Gravação diferencial (Save_WriteBlocksEeprom)

- param 0: para cada um dos 0x398 blocos, lê a EEPROM, compara 4 halfwords com o buffer e só chama
  `lib_eeprom_write_verify` se mudou. Param != 0: grava todos os blocos.
- Buffer e EEPROM são comparados no formato lógico (sem inversão).

## Divergências do extrator (pc/extract/extract_save.py): só leitura, não editado

1. **Inventário (decode_inventory)**: lê `(qty, eq)` como 2 bytes em `0x7AC + tipo*480 + bot*2`, com 240 "bots" por tipo.
   No save original cada byte da RAM vira um u16 (2 B), e o par de um bot é `qty` em `0x7AC + tipo*480 + bot*4`
   e `eq` em `+2`, com 120 bots por tipo. O byte alto de cada u16 é sempre 0 no original.
   Logo o extrator lê o par de outro bot e o "eq" é sempre 0. O extrator está calibrado no layout da ROM expandida
   (um u16 = qty+eq), que não vale para a ROM original. Confirmado no código (SaveGame loop + decode_inventory).
2. **Equipe**: docstring diz "8 registros", o código usa TEAM_SLOTS = 9. O SV2 confirma 9 (0x48 B). Só docstring.
3. **Opções 0x1CB8, dinheiro, medalhas e flags**: o extrator não decodifica (vem nil). Dá para decodificar com este mapa:
   money = u32 em 0x010; opções como acima; flags = `0xFF0 + k` para k >= 8.
4. **Checksum e tamanho**: `SIZE = 0x1CBC`, `CHECKSUM = 0xFF4` e a soma com campo zerado estão corretos.
5. **Inversão de 8 B (swap8)**: não verificada aqui (o `.sav` não foi lido, por regra).

## Em aberto

- As flags de ids 0..15 (RAM 0x030057D8..0x030057DF) não são salvas. Se algum evento usa esses ids, a flag se perde
  no load. Não verificado: quem grava esses ids. Procurar `EventFlag_Set*` com id < 16.
- 0x1CB9..0x1CBB: se o staging EWRAM 0x02030000 não é zerado antes de cada save, o checksum depende de lixo de
  memória (ainda consistente, porque o lixo vai para a EEPROM). Não verificado.
- Nomes de opção (0x03001788, 0x03001360, 0x03000B94) continuam provisórios (ver OP.md).
- Savestate não usado. Os valores do mapa são confirmados pelo código, não medidos.
