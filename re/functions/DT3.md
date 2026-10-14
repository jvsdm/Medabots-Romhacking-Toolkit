# Tabelas de dados da ROM, lote 3 (Medabots Metabee EU) — GH-DT3

Continuação de `DT.md`. Cobre as tabelas pendentes: `gMapArea`, `gMapAreaB`, `gBotNames`, `gTechniqueNames`,
`gMoldTable` e os campos de `gMedals`. Leitura feita na ROM original (só leitura). Convenção: `ROM = 0x08000000 + offset`.
Texto do jogo: 0x01-0x1A = A-Z, 0x1B-0x34 = a-z, 0xFE/0xFF = fim de string, demais bytes em `[xx]`. Só 2-3 exemplos por tabela.

## 0x083D2B6B gMapArea — área (região) por mapa
- Tamanho: 192 B (1 B por mapa; índice = id do mapa, 0..191). Valores 0..10 (11 valores distintos).
  Contagem: 0x00 (34), 0x01 (1), 0x02 (6), 0x03 (10), 0x04 (6), 0x05 (27), 0x06 (14), 0x07 (12), 0x08 (11), 0x09 (42), 0x0A (29).
- Leitores (achados pelo literal pool; a hipótese de `Anim_LoadFrameEvery10` está errada, essa função não lê esta tabela):
  - `FUN_08048960` 0x08048960, literal 0x08048A40. No caso 0 do `switch`, lê `gMapArea[mapa]` e devolve 2 se o valor é 0x0A.
  - `Overworld_CheckStepAllowed` 0x08057E6A (lê `gMapArea` pelo literal 0x08057EA0): mesmo teste, valor 0x0A = passo negado (0 = permitido).
- Índice: byte da RAM em `0x03005FDE` (= base IWRAM 0x030057E0 + 0x7FE, literais 0x08048A44 e 0x08048A48). Provável: id do mapa atual.
  Como a tabela tem 192 entradas, o índice é o id do mapa.
- Significado: 0x0A = área sem passo livre (provável; 29 mapas têm). Os outros valores (0..9) não têm efeito conhecido na leitura.
- Nome: `gMapArea` (já no mtbe). Renomeado no Ghidra.
- Confiança: confirmado (tamanho, valores, leitores pelo literal pool e índice por mapa). Sentido do 0x0A: provável.

## 0x08483774 gMapAreaB — cópia de gMapArea
- Tamanho: 192 B. Difere de `gMapArea` em 4 bytes (0x0A em 27 mapas em vez de 29; 0x00 em 36 em vez de 34).
  Histograma igual nos demais valores.
- Literal de pool: só 0x08060948 (valor 0x08483774). O xref do Ghidra aponta para `Screen_InitLz77TilesAndSprites` 0x08060880,
  mas não há função em 0x08060948 e o literal não foi ligado a leitor de passo. Nenhum leitor confirmado.
- Nome: `gMapAreaB` (mantido do mtbe; ainda não renomeado no Ghidra).
- Confiança: confirmado (tamanho e diferença). Uso: desconhecido; provável cópia de dados para outro sistema (região do mapa para outra tela ou cópia de backup).

## 0x083BA658 gBotNames — nome de cada bot (tabela de ponteiros)
- Tamanho: 120 ponteiros u32 (480 B). Índice = id do bot (0..119). Todos os 120 apontam para a ROM.
- Leitores: `GetBotName` 0x08047BDC (literal 0x08047BE8; devolve `gBotNames[bot]`), `Fight_InitializeBotFromTemplate`
  0x08015166 (literal 0x080151E0; usa o nome ao montar o bot da batalha). Chamador de `GetBotName`: não percorrido.
- Formato do texto: strings curtas (4 a 16 caracteres) terminadas em 0xFE. Exemplo: bot 0 = `NOCTOBAT` (bytes 0E 0F 03 14 0F 02 01 14 FE).
  Alguns nomes têm bytes fora do alfabeto (ex.: 0x00 e 0x43 dentro de nomes), mostrados como `[xx]`.
- Nome: `gBotNames` (já no mtbe; Simon: `gPartNamePointerTable`). Renomeado no Ghidra.
- Confiança: confirmado (120 ponteiros válidos; leitores no código; formato de texto confirmado no bot 0).

## 0x083BE868 gTechniqueNames — nome de cada técnica (tabela de ponteiros)
- Tamanho: 60 ponteiros u32 (240 B). Índice = id da técnica (0..59). Todos os 60 apontam para a ROM.
- Leitor: `Fight_DrawSelectedActionIcon` 0x08016D6C (literal 0x08016D98). Lê o byte do registro da peça selecionada
  (`bot_block + (sel-1)*0x18 + 0x49`, sel = 1..3) como índice, copia o nome para o buffer de texto (12 bytes) e desenha.
  Se sel = 0 mostra 10 glifos repetidos. Provável: o byte é o campo "técnica" (byte 1 do registro de peça, ver HANDOFF).
  Obs.: o Ghidra mostra `memcpy(name, buf, 0xC)` com os argumentos trocados; o sentido é cópia do nome para o buffer.
- Outros xrefs: `Fight_DispatchOnSubstateAA_F_00_CopyPartDataPair` 0x0801DE9A (literal 0x0801DED0 fica fora de função no Ghidra; não lido).
- Exemplos: técnica 0 = `Sword`, 1 = `Hammer`, 2 = `Rifle`, 3 = `Chain Gun`, 4 = `Laser`, 5 = `Beam`.
- Nome: `gTechniqueNames` (já no mtbe). Renomeado no Ghidra.
- Confiança: confirmado (60 ponteiros válidos; leitor e índice pelo código). Índice = byte +0x49 do bloco: provável.

## 0x083D1D78 gMoldTable — tabela de ponteiros para moldes (registros de 0x18 B)
- Tamanho da tabela: 50 ponteiros u32 (200 B). Índice = id do molde (0..49). **Correção:** o mtbe e `DT3` antigos
  falam em "50 x 0x18", mas o que está em 0x083D1D78 são ponteiros; os registros de 0x18 B ficam em outro lugar.
- Entradas: 0 = nulo; 1, 2, 7..49 = 45 ponteiros para registros na ROM (0x083D1940, 0x083D1958, 0x083CF5B0 ...;
  consecutivos com passo 0x18 a partir de 0x083D1940); 3, 4, 5, 6 = endereços de RAM (0x030052D0, 0x030052B0, 0x03005290, 0x030052F0),
  preenchidos em tempo de execução.
- Registro de 0x18 B (medido nos 45 registros da ROM):
  - +0x00 (u32): ponteiro para a lista de artes (pares imagem/paleta). Nulo em 2 registros (entradas 1 e 2).
  - +0x0F e +0x10 (u8): índices de filhos/juntas; +0x10 não zero em 21 registros, +0x0F não zero em 10. Provável.
  - +0x11 (u8): paleta, valores 0..3 (3 em 30 registros, 0 em 9). Provável.
  - +0x12 e +0x13 (u8): tamanho do molde em pixels (8, 16, 32 ou 64). Provável.
  - +0x14 (u8): 0x11 em todos os 45. Não entendido.
  - +0x15..+0x17: zero em todos os 45. **Discordância:** o mtbe diz "+0x15 forma OBJ"; nos dados esse byte é sempre 0.
- Exemplo (entrada 7): `B0 F5 3C 08` (ponteiro 0x083CF5B0), +0x10 = 08, +0x12 = 10, +0x13 = 10, +0x14 = 11.
- Leitores: parâmetro de inicialização em `FightPreview_InitCompositeBuffers` 0x08044668, `InitializeSelectedMedabotPreviewRenderer`
  0x080180FC, `BattlePanel_InitOrRedraw` 0x08062A6C e 0x08062AE2, `Battle_InitPartSprites` 0x080065B4,
  `init_estado_parte_batalha` 0x080052E8, `PartsGrid_InitScreenBuffers` 0x0800F28A, `PartPreview_InitScreenBuffers` 0x0800F65A
  (todos usam o literal 0x083D1D78 como argumento; o uso interno não foi lido).
- Nome: `gMoldTable` (já no mtbe; Simon: `sCompositePreviewDescriptorPointerTable`). Renomeado no Ghidra.
- Confiança: confirmado (ponteiros e tamanho dos registros; estrutura de +0x00). Campos +0x0F..+0x14: provável.

## 0x083B62D8 gMedals — campos (30 registros de 0x10 B)
- Índice = id da medalha (0..29). Leitores (copiam o registro de 16 B para o estado da peça, em `iVar2 + 0x10E`):
  `desenha_stats_parte` 0x08005D1C (literal 0x08005D8C), `desenha_stats_parte_b` 0x08005E84 (literal 0x08005EF4),
  `desenha_stats_parte_d` 0x08006154 (literal 0x080061C4). O índice é o byte +1 do registro de 0x40 B da RAM (0x03000BE0 + slot*0x40).
  Ou seja: o leitor lê a medalha pelo id que está no slot da RAM.
- Campos (conferidos nos 30 registros):
  - +0x00 (u8): id. +0x01 (u8): repete o id nas medalhas 0..26; nas medalhas 27, 28 e 29 o +0x00 é 0x1B, 0x1C, 0x1D e o +0x01 é 0x1A.
  - +0x02 (u8): classe (0..7, quase uniforme). Provável: classe de medalha, comparada com o byte +4 das peças (ver `Fight_GetMedalPartCompatibilityBonus` 0x0803576C).
  - +0x03..+0x06 (u8 x 4): quatro limiares de rank. Valores: +03 = 10/30/60/255, +04 = 30/60/255, +05 = 60/255, +06 = 100/255.
    Os valores 0xFF = sem limite. Provável.
  - +0x07 (u8): bônus de compatibilidade (0, 4, 5, 6, 7, 8, 9, 10; 7 em 13 medalhas). Provável, confere com o desenho do bônus.
  - +0x08..+0x0F (u8 x 8): base de especialidade (0, 4, 6, 8, 10, 14). Oito bases por medalha; combina com o texto do HANDOFF
    ("8 bases de especialidade"). Provável.
- Exemplo: medalha 0 = `00 00 02 0A 1E 3C 64 04 0A 0A 00 00 06 00 06 00`.
- Nome: `gMedals` (já no mtbe; Simon: `rom_medal_information`). Renomeado no Ghidra.
- Confiança: confirmado (tamanho 30 x 16 B, id +0, leitores). +0x02..+0x0F: provável, pela estrutura do HANDOFF e pelos valores;
  o uso exato de cada byte no código (compatibilidade, limiares) não foi confirmado nas funções que consomem o registro.

## Em aberto
- `gMapAreaB`: sem leitor confirmado.
- Limiares de medalha em `gMedals` +0x03..+0x06: não casei com os limites 10/29/59 de `R0a.md` (que vêm de outra estrutura).
- Mapa de handlers de evento (0x083AF8C0, 0x083AF180, 0x083D222C/0x083D2630) e textos pré/pós-batalha (0x083C2D3C, 0x083C3B04) continuam fora deste lote.
