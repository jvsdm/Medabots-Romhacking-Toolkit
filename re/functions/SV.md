# SV — sistema de save (Medabots Metabee EU, ROM original)

Escopo: mapa save (EEPROM 8 KB) <-> RAM, checksum/assinatura, leitura e gravação.
Fonte: Ghidra (decompile + disassembly + literal pools lidos da ROM original, somente leitura).
Nada foi gravado no Ghidra; comentários em memória em `site_SaveGame` e `Save_ReadVerify`.

## Fluxo geral

- Gravação: `Site_RunSaveLoadOption(0)` -> `site_SaveGame(stg)` monta o buffer em EWRAM `0x02030000` (staging, 0x1CC0 B)
  -> `Save_WriteBlocksEeprom(stg, 0)` grava **diferencial** os 0x398 blocos de 8 B (só regrava bloco que mudou).
- Leitura: `Site_RunSaveLoadOption(1)` -> `Save_ReadVerify(stg)` lê os 0x398 blocos e valida -> `site_LoadGame(stg)` copia para a RAM.
  Atenção: `site_LoadGame` roda mesmo se a validação falhar; o retorno só serve de gatilho de retry.
- Apagar/invalidar: `Site_RunSaveLoadOption(2)` escreve no cabeçalho a string de 16 B da ROM em `0x084836C5`
  (começa com "MEDAROT2", mas não é a assinatura "Ver.KBT") e grava diferencial. A validação então falha (retorno 1).
- Save_WriteEepromAndFinish (`0x0806050C`): zera o bloco 0 (8 B) da EEPROM com até 4 tentativas, restaura IME/IE,
  põe gMainState=3. Não tem xref direto; é chamado pela tabela de handlers do fluxo de salvar (ponteiro em `0x08483770`).
- Retry no título: `Save_InitScreen` -> `Save_ClearAndRetryLoad` (zera o nome em RAM com 0xFE e chama `Site_RunSaveLoadOption(1)` até 4 vezes).

## Tabela save (offset) <-> RAM

Offsets do buffer lógico (desinvertido). Tamanho em bytes.

| offset save | tamanho | região RAM (ROM Metabee) | conteúdo |
|---|---|---|---|
| 0x000 | 16 | ROM 0x084836B4 | assinatura "MEDAROT2 Ver.KBT" (13 B validados) |
| 0x010 | 4 | 0x03002B70 | dinheiro u32 (limite validado 99999; provável centavos) |
| 0x014 | 16 | 0x03002B74 | nome do jogador, 0xFE = fim/vazio (0 aceito como vazio) |
| 0x024 | 2 | 0x03002B84 | total de lutas u16 (limite 9999) |
| 0x026 | 2 | 0x03002B86 | vitórias u16 (≤ total) |
| 0x028 | 4 | 0x03002B88 | tempo de jogo em quadros u32 (máx 21.599.999 = 100 h a 60 fps) |
| 0x02C | 0x780 | 0x03000BE0 | 30 medalhas x 0x40 (id em +1 do registro, confirmado pelo HE3/DT3) |
| 0x7AC | 0x780 | 0x03004C40 | inventário de peças: 960 bytes da RAM, cada um vira u16 no save (ver nota) |
| 0xF2C | 0x048 | 0x030017A0 | equipe: 9 registros x 8 B (estado, gênero, medalha, cabeça, braço D, braço E, pernas, índice) |
| 0xF74 | 0x080 | 0x03002BA0 | itens: 64 pares [0x80|id, qty] |
| 0xFF4 | 2 | — | checksum u16 = soma de bytes 0x000..0x1CBB com o próprio campo zerado |
| 0xFF8 | 0xCC0 | 0x030057E0 | bloco de estado de eventos E (+0x000..+0xCBF): flags/história (ex.: E+0xC89, E+0xC93) |
| 0x1CB8 | 1 | 0x03001788, 0x03001360, 0x03000B94, 0x03001784 | opções em bits: b0-1 = [0x03001788]; b2 = [0x03001360]; b3-4 = [0x03000B94]; b5 = bit1 de [0x03001784] |
| 0x1CBC..0x1CBF | 4 | — | padding (fora do checksum); EEPROM usa 0x398 blocos = 0x1CC0 B |

Nota: 0x03001784 bit0 é ligado antes de salvar (`*DAT_0805de50 |= 1`), então é flag de "salvando".

## Inventário: detalhe importante (o extrator provavelmente lê errado)

- `docs/EXPANSAO_MEDABOTS.md` diz que na RAM original há 4 blocos de 0xF0 (120 bots x 2 bytes) por tipo, endereço `tipo*240 + bot*2`.
- `site_SaveGame` copia 960 **bytes** da RAM 0x03004C40 e grava **cada byte como u16**. Logo, o par de um bot fica em
  `0x7AC + tipo*480 + bot*4` (qty) e `+2` (eq), no save original.
- `pc/extract/extract_save.py` lê `0x7AC + tipo*480 + bot*2` como (qty, eq) de bytes. Com o layout original, isso
  pega o byte-alvo de outro bot, e o "eq" (byte alto do u16) é sempre 0. A nota do N3 já diz que o save da ROM
  expandida grava byte a byte, então o extrator foi calibrado no layout expandido.
- Confiança: confirmado no código de `site_SaveGame`; a correspondência par (qty, eq) é provável. Não testado com savestate (sem emulador nesta tarefa).

## Checksum e assinatura

- `Save_ReadVerify` valida, nesta ordem: 13 B da assinatura; campo nome; money; total/vitórias; tempo; bits de opções;
  depois o checksum. Retorno 0 = OK, 1 = assinatura errada, -1 = outra falha.
- Checksum: o disassembly mostra que o valor gravado em +0xFF4 é lido **antes** de zerar o campo; compara com a soma de
  0x000..0x1CBB. O decompile sozinho sugeria comparação com zero (enganoso). Igual ao `checksum()` do extrator.

## Cópias e slots

- Uma única cópia. Só as rotinas `Save_*` acessam a EEPROM (xref de `lib_eeprom_*`). Não há backup nem slot 2.
- Blocos 0x398..0x3FF (104 blocos) não são usados por essas rotinas.

## EEPROM (lib)

- `lib_eeprom_write_verify` (0x0807CC80): até 3 tentativas de `lib_eeprom_write64` + `lib_eeprom_verify64`.
- `lib_eeprom_write64` (0x0807CB4C): endereço de 14 bits, dados em 4 halfwords, DMA3 para 0x0D000000, checa bloco < nº de blocos.
- `Save_WriteEepromAndFinish` faz 4 chamadas ao `write_verify`, então até 12 escritas (a nota antiga de FL4 dizia "4").
- `.sav` do mGBA guarda cada bloco de 8 B invertido (o extrator faz `swap8`).

## Outros

- `bios_store_idx` (0x0807C918): só seleciona uma de 4 entradas de operação e grava um ponteiro. `Save_InvokeFlashOp`
  chama com índice 2 e ponteiro 0x03000BCC. Não é gravação em flash.
- Tabela de handlers do fluxo de salvar: ponteiros em 0x08483614 (Save_InitScreen), 0x08483688 (Save_StepThenFlashOp),
  0x08483724 (Save_FinishFlashStep), 0x08483770 (Save_WriteEepromAndFinish); e 0x080AFCD4/0x080AFCDC (WaitSfx, RunOptions).

## Confiança

- Confirmado: mapa de offsets, assinatura, checksum, validação, EEPROM 0x398 blocos, cópia única, lib_eeprom.
- Provável: nomes de campos (dinheiro em centavos, nome, opções), função de cada bit de opções, papel de Save_WriteEepromAndFinish.
- Não feito: testar com savestate ou save real; mapear 0x03001360 ao nome da opção (A6 usa como flag de UI de batalha).
