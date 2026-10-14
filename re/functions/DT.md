# Tabelas de dados da ROM (Medabots Metabee EU) — GH-DT

Inventário das tabelas de dados (não código) citadas pelas funções documentadas em `re/functions/*.md`
e marcadas em `re/symbols/mtbe.csv`. Ordem: peças, medalhas, técnicas, Medaforça, mensagens, música e mapa,
tabelas de handlers. Leitura feita na ROM original `Medabots - Metabee (Europe).gba` (só leitura).
Convenção: `ROM = 0x08000000 + offset`. Confiança em cada seção.

## 0x083B83FC gPartStats — estatísticas das peças
- Tamanho: 480 registros × 16 B (7680 B). Índice = `bot*0x40 + kind*0x10` (bot 0..119, kind 0..3).
  Kind: 0 = cabeça, 1 = braço direito, 2 = braço esquerdo, 3 = pernas (ver BT2 em `pc/research/README.md`).
- Leitor: `GetPartStats` 0x08042B58 (copia 16 B para o buffer do chamador). Chamada por `desenha_stats_parte`
  (0x08005D1C e irmãs), `PartsGrid_*`, `FightPreview_LoadCachedAndEncounterLoadouts` (literal 0x08069158).
- Campos (confirmados contra `pc/cache/parts.lua`, bot 0 kinds 0 e 1):
  - +0x00 medalha (u8): 15 nas duas peças de bot 0.
  - +0x01 técnica (u8): 34 ("Anti-Air") nas duas.
  - +0x02 especialidade (u8): 2 ("Shoot").
  - +0x03 gênero (u8): 0.
  - +0x04 armadura (u8): 35 e 30 (kind 0 e 1).
  - +0x05 sucesso (u8): 21 e 28.
  - +0x06 poder (u8): 8 e 3.
  - +0x07 carga (u8): 1.
  - +0x08 usos (u8): 8 e 14.
  - +0x0B preço (u8): 14 e 11.
  - +0x09, +0x0A (= 0x20 nas quatro peças), +0x0C (= 1 nas quatro peças): não entendidos.
  - Pernas (kind 3) têm outra leitura de +0x06..+0x09 (propulsão, evasão, defesa); não conferido no parts.lua.
- Nome sugerido: `gPartStats` (já no mtbe; nome de Simon: `gRomPartRecordTable`).
- Confiança: confirmado para +0x00..+0x08 e +0x0B (bot 0, kinds 0 e 1). Demais bytes: chute.

## 0x083B62D8 gMedals — medalhas
- Tamanho: 30 registros × 16 B (480 B = 0x1E entradas). Índice = id da medalha (0..29).
- Leitor: funções de atributo de peça `PartDetail_UpdateHeadAttributeMatchMarker` (0x08005D1C),
  `desenha_stats_parte_b` (0x08005E84), `desenha_stats_parte_c` (0x08005FEC),
  `PartDetail_UpdateLegsAttributeMatchMarker` (0x08006154), `PartsGrid_CopyCellRecordToSl*` (literal 0x0800B590).
- Campos observados nos 6 primeiros registros:
  - +0x00 id (u8) e +0x01 id de novo (u8, repetido): 00/00, 01/01, 02/02, 03/03, 04/04, 05/05.
  - +0x02 (u8): 02, 00, 04, 05, 06, 07 (não entendido; talvez índice de compatibilidade).
  - +0x03 (u8): 0x0A ou 0x1E.
  - +0x04..+0x06: 0x1E / 0x3C / 0x64 (ou 0xFF = sem valor): provável limiares de nível.
  - +0x07 (u8): 04 ou 07 (grupo; não entendido).
  - +0x08..+0x0F: não entendidos (valores pequenos, 0x00..0x0A).
- Nomes de texto: `gTextMedalNames` 0x083B6590 (34 ptrs, nomes), `gTextMedalAbilityDesc1` 0x083B6B48 (27 ptrs,
  compatibilidade peça→medalha) e `gTextMedalAbilityDesc2` 0x083B6FA4 (26 ptrs, medalha→peça).
- Nome sugerido: `gMedals` (já no mtbe; nome de Simon: `rom_medal_information`).
- Confiança: confirmado o tamanho e o id (+0x00); o resto é provável/chute. Não confrontado com o PC (`savegame.medals`).

## 0x083AEEA0 gTechniqueActionSetupHandlerTable — ponteiros de handler de preparação de técnica
- Tamanho: 60 entradas × 4 B (u32 thumb), porque a próxima tabela (`gTechniqueActionValidationHandlerTable`)
  começa em 0x083AEF90 (0xF0 = 60 × 4). Tamanho = provável (espaçamento), 60 é o mesmo número de `gTechniqueNames`.
- Índice = id da técnica (provável). Entrada 0 e 1 = 0x08000841 (sentinela, sem handler). Entradas 2..5 = 0x08031AFD.
- Leitor: `Fight_DispatchPartPreparation` 0x08031A70 (literal 0x08031AB4).
- Nome sugerido: `gTechniqueActionSetupHandlerTable` (já no mtbe; nome de Simon: `Simon/Rokusho em 0x083AEEC0`).
- Confiança: confirmado o endereço e o leitor; tamanho e índice provável; função de cada entrada não lida.

## 0x083AEF90 gTechniqueActionValidationHandlerTable — ponteiros de handler de validação de técnica
- Tamanho: 60 entradas × 4 B (u32 thumb). Fim = 0x083AF180 (`gActionScriptOpcodeHandlerTable`): 0xF0.
  Provável (espaçamento). Índice = id da técnica (provável), igual à tabela anterior.
- Entradas 0..5 = 0x08000841 (sentinela). Muitas entradas vazias: técnicas sem validação própria.
- Leitor: `Fight_DispatchPartExecution` 0x08031ABC (literal 0x08031AF0).
- Nome sugerido: `gTechniqueActionValidationHandlerTable` (já no mtbe).
- Confiança: endereço e leitor confirmados; tamanho/índice provável; conteúdo das entradas não lido além das 6 primeiras.

## 0x083B74B8 gTextMedaforceNames — nomes das Medaforças (tabela de ponteiros de texto)
- Tamanho: 50 ponteiros u32 para strings na ROM (200 B). Próxima tabela em 0x083B7580 (`gTextMedaforceDesc`):
  0xC8 = 50 × 4, então 50 entradas. (Nota do mtbe: o script de tradução usava 52 e invadia a tabela de descrições.)
- Índice = id da Medaforça (0..49). Ponteiros de exemplo: 0x083B724C, 0x083B7258, 0x083B7262, 0x083B726D.
- Leitores: `PartsGrid_DrawDetailRows` (0x0800A2E8; literais 0x0800A398/0x0800A470/0x0800A544),
  `Fight_HandleRewardGrantOrSkipByPostBattleFlags` (0x08010C50, literal 0x080111F4), `Fight_InitKindObjectG` (0x08017208, literal 0x080172E8).
- Nome sugerido: `gTextMedaforceNames` (já no mtbe; texto PT-BR em `dump_text_clean_v2.py`).
- Confiança: confirmado (ponteiros caem na ROM; contagem pelo espaçamento até `gTextMedaforceDesc`).

## 0x083B7580 gTextMedaforceDesc — descrições das Medaforças (tabela de ponteiros)
- Tamanho: 53 ponteiros u32 (mtbe). Começa logo após `gTextMedaforceNames`.
- Índice = id da Medaforça (mesmo índice da tabela de nomes). Desc. de 1 e 2 linhas usam `gTextMedaforceDesc1` 0x083B80C0 (5 ptrs).
- Leitor: mesmo grupo de `gTextMedaforceNames` (`PartsGrid_DrawDetailRows` e a tela de recompensa); leitor exato não confirmado.
- Nome sugerido: `gTextMedaforceDesc`.
- Confiança: provável (tamanho 53 do mtbe, não recontado).

## 0x083C6744 gTextGameMessages — mensagens do jogo (tabela de ponteiros)
- Tamanho: 176 ponteiros u32 (704 B).
- Índice = id da mensagem (0..175). Ids usados pela batalha (BT2 em `pc/research/README.md`): [107] "Peça <MEM:1> "<MEM:2>" foi obtida!",
  [109] XP, [110] derrota (sem perda de peça).
- Leitores: `Fight_HandleRewardGrantOrSkipByPostBattleFlags` 0x08010C50 (literais 0x08010D90, 0x08010FD4, 0x08011040, 0x08011124, ...).
- Nome sugerido: `gTextGameMessages` (já no mtbe).
- Confiança: confirmado o endereço e a contagem do mtbe; índices 107/109/110 confirmados no PC (`battle.lua`), não na ROM.

## 0x083AECD8 gFightSceneSteps — passos da cena 8 (tabela de ponteiros de handler)
- Tamanho: não medido (mtbe não traz contagem). Entradas u32 thumb: 0x08030891, 0x08030911, 0x08030991, 0x080309C9,
  0x08030A25, 0x08000841 (sentinela vazia na entrada 5).
- Índice = `0x0300137C` (`gMainState`/cena 8, confirmado em HN10). Cada entrada é um passo da cena de batalha.
- Leitor: `Fight_SceneMain` 0x08030860 (literal 0x08030888).
- Nome sugerido: `gFightSceneSteps` (já no mtbe).
- Confiança: confirmado o leitor e o índice (HN10); tamanho não medido.

## 0x0847E644 gMapMusic — música por mapa
- Tamanho: 192 B (1 B por mapa, índice = id do mapa 0..191). Valores 0x16..0x34 (ids 22..52), 24 valores distintos.
  Mais frequente: 0x31 (71 mapas).
- Verificado: mapa 23 (New Game) = byte 0x1A = 26 (`0x0847E644 + 23`), igual ao id 26 @0x080B0510 em SND11
  (`pc/research/SND11_sequencia.md`).
- Leitor: literal 0x08048418 dentro de `Map_LoadOverworldSpriteGraphicsForCurrentMap` 0x08048288 (carga do mapa).
  O pedido de música passa por `Sound_RequestSong` = `FUN_0804842C(1, id)` (HANDOFF). Leitor exato da música não confirmado.
- Nome sugerido: `gMapMusic` (já no mtbe).
- Fora deste inventário, mas ligado: a tabela de músicas (song table) fica em ROM 0x080B012C (4 B por entrada, 54 ids,
  SND2 em `pc/research/README.md`) e a tabela de amostras PCM em 0x080B16E4 (SND10). Fora do intervalo 0x083..0x085 pedido.
- Confiança: confirmado (tamanho, índice e valor do mapa 23). Leitor: provável.

## 0x083F8900 gMapDimensions — largura e altura do mapa em tiles
- Tamanho: 192 × 2 B (384 B). Registro = (largura, altura), índice = id do mapa. Mapa 10 = (24, 22) = 0x18, 0x16.
- Valores frequentes: 0x14 (76 vezes), 0x12 (73), 0x20 (71). 23 combinações distintas.
- Consistente com a regra de centralização do W3 (`pc/research/README.md`): origem = ((30 - w) // 2, (20 - h) // 2);
  mapa 10 dá origem (3, 0), como medido.
- Leitor: `Map_GetMapSizeHalfTileByAxis` 0x080529AC (literais 0x080529C8 e 0x080529D8). O nome do leitor diz "half tile";
  os valores batem com tiles pela regra W3, então a unidade do leitor fica em aberto.
- Nome sugerido: `gMapDimensions` (já no mtbe; Simon: `sMapDimensionsInMetaTilesTable`).
- Confiança: confirmado o formato (regra W3 em mapa 10); unidade do leitor: provável.

## 0x083BEC10 gMapEncounters — encontros aleatórios por mapa
- Tamanho: 192 × 8 B (1536 B), índice = id do mapa. Só os bytes +0..+3 variam; +4..+7 são sempre 0.
- Campos (medidos na tabela inteira):
  - +0 (u8): 0 ou 1 (2 valores). Nome do mtbe: `map_has_enemies_list` → flag "o mapa tem encontro". Provável.
  - +1 (u8): 0..10 (11 valores). Provável índice de área/modelo de encontro. Não confirmado.
  - +2 (u8): 0 ou 1. Não entendido.
  - +3 (u8): 0 ou 1. Não entendido.
- Leitores: `Fight_CopyEncounterTemplateEntry` 0x08011DE4 (literal 0x08011E14, copia 2 B para a tabela de templates),
  `reset_step_and_get_encounter_flag` 0x0804387C (literal 0x080438B4; encontro aleatório: passos > 16 e
  (passos > 31 ou randbyte & 0x1F == 0x1F), `pc/research/README.md`), e `PartSet_HasCompleteMonochromeEntry` 0x08047FBC
  (literais 0x080480A4 e 0x080480C8; nome do leitor suspeito, provavelmente errado).
- Nome sugerido: `gMapEncounters` (já no mtbe; Simon: `map_has_enemies_list`).
- Confiança: confirmado o tamanho e a faixa de valores; o significado de +1..+3 é provável ou chute.

## 0x083BF210 gMapEncountersExtra — bots de encontro por mapa
- Tamanho: 192 × 4 B (768 B), índice = id do mapa. 128 entradas são zero (todas os 4 bytes); as outras 64 têm valores.
- Campos: +0..+3 são ids de 1 byte (0..245). Os valores mais comuns são 0x9B (155, 13 vezes), 0xF5 (245), 0xA7 (167),
  0x2E (46), 0xAC (172). Padrão de exemplo nas entradas 1..6: `9B F5 9B A7` (o mesmo quarteto em 6 mapas).
- Interpretação provável: 4 modelos de bot (ou templates) que podem aparecer no encontro daquele mapa. Não confirmado
  contra o encontro de batalha (`battle.lua` ainda não lê esta tabela).
- Leitor: `Fight_SelectTemplateForCurrentEncounter` 0x08011E20 (literal 0x08011E64).
- Nome sugerido: `gMapEncountersExtra` (já no mtbe; Simon: `gMapEncounterBattleTable` / `map_encounters`).
- Confiança: confirmado o tamanho e a zeragem; significado dos ids: provável.

## 0x084838E4 gShops — conteúdo das lojas de Medapeças
- Tamanho: 15 lojas × 4 slots × 1 B = 60 B. Slot = id de bot (0..119; 0xFF = vazio). Ex.: loja 0 = `13 00 FF FF`
  (bots 19 e 0), loja 2 = `13 00 42 1C`.
- Índice: id da loja (0..14), vindo do evento 0x59 Open_Shop (1 B). Eventos que abrem loja: 274, 532, 987, 1134, 1141, 1403, 1586, 1719, 2029 (`pc/research/README.md`, SH1).
- Preço: por peça, não pela loja (`parts.lua` campo `price`, ver gPartStats +0x0B).
- Leitor: `Shop_InitPartSelectionScreen` 0x080619B0 (literal 0x08061CCC).
- Nome sugerido: `gShops` (já no mtbe; Simon: `shop_contents`).
- Confiança: confirmado (leitor e formato; a loja 0 e 2 batem com o `shops.lua`). Se o slot é id de bot ou de peça: o leitor usa como id de bot (0..119).

## 0x083CAD78 gMoveStyle — estilo de deslocamento das pernas por bot
- Tamanho: 122 B (1 B por bot). Valores 0..6 (7 distintos): 0x02 é o mais comum (62 bots), depois 0x03 (23), 0x01 (13).
- Índice = id do bot (122 entradas; há 120 bots, as 2 últimas não conferidas).
- Leitor: `MotionPlayStep` 0x08044A3C (literal 0x08044AF8). Escolhe o tipo de movimento na animação de batalha (B7/B8).
- Nome sugerido: `gMoveStyle` (já no mtbe).
- Confiança: confirmado o tamanho e o leitor; o significado de cada valor (0..6) é provável.

## 0x083BEB94 gPaletteFamily — família de paleta do sprite composto
- Tamanho: 122 B (1 B por bot). Valores 0..2 (3 distintos): 0 em 106 bots, 1 em 12, 2 em 4.
- Índice = id do bot. O valor é o banco de OBJ da paleta do mini sprite (mtbe: "familia de paleta = banco OBJ do mini sprite").
- Leitores: `CreateCompositeBattleSpriteObjects` 0x08042E58 (literal 0x08042F28), `BuildCompositeBattleSpriteObjects`
  0x0804305C (literal 0x08043120), `Shop_LoadPartIconFrame` 0x0805B188 (literal 0x0805B228).
- Nome sugerido: `gPaletteFamily` (já no mtbe; Simon: `sCompositeBattleSpritePaletteFamilyTable`).
- Confiança: confirmado o tamanho e os valores; o uso como banco de OBJ: provável.

## 0x083B29EC gRandomLUT — tabela de bytes do randbyte
- Tamanho: 256 B (índice de 8 bits). Quase permutação: 255 valores distintos.
  Início: `47 AE 03 F3 06 75 40 85 90 56 23 24 C9 01 8A 4D`.
- Uso: `randbyte` 0x080424B8 quando o flag em 0x03000440+0x89 é ≠ 0: B = ROM[0x083B29EC + idx++] (idx em +0x8D).
  Leitor literal em 0x080424E8 (dentro de `randbyte`). Ver `re/functions/R2.md` e `pc/research/README.md` (RNG).
- Nome sugerido: `gRandomLUT` (já no mtbe; Simon: literal 0x083B2A0C).
- Confiança: confirmado (tamanho pelo índice de 8 bits; leitor no código); conteúdo: só o início lido.
