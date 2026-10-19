# BBG - Fundos (cenário) da Robattle: tabela por id de banco

Status: PARCIAL (2026-10-18). Confirmado: formato da tabela, quem a lê, de onde vem o id, mapas 0..10 decodificados e
vistos na luta (LÖVE headless). Aberto: id nas batalhas de história, id do mapa da escola, tiles de HUD x cenário na BG3.

## Quem escolhe o id
- `fight_information+0x38` (0x03001838, bloco 0x03001800) é o id de banco do cenário.
- Encontro aleatório: `Fight_CopyEncounterTemplateEntry` 0x08011DE4 copia o byte +1 de `gMapEncounters[mapa]`
  (ROM 0x083BEC10, 8 B por mapa) para +0x38. Valores 0..10 (ver EV4.md). Confirmado (decompile).
- Encontro de link: `Link_RollTableEntryAndAdvance` 0x0803D730 põe +0x38 = tabela[rand & 0x1F] (32 entradas, ponteiro em
  DAT_0803D744). Confirmado (decompile).
- Batalha de história (cmd 0x20 / op33): NENHUM escritor direto de +0x38 achado no Ghidra. Escritores por ponteiro não
  aparecem em xref (só endereço constante). Pendente: `--watch 0x03001838` numa batalha de história (replay).
- Cena `Scene_LoadBankAAndResetFx` 0x08069490 (scene_init_b): o id vem de RAM 0x03006F84 (não de +0x38), grava +0x38 e
  chama GfxBankA com ele. Quem põe 0x03006F84 não foi achado.

## Carga do fundo: G03 0x0801E09C (Fight_DispatchOnSubstateAA_G_03_BuildBattleBackground)
- Passo 0: REG_DISPCNT &= 0xE0FF; BG0CNT=0x0500, BG1CNT=0x1801, BG2CNT=0x590A, BG3CNT=0x5B0B;
  `Lz_DecodeIfMagic(0x084C8034, 0x06017400)` (fixo, não depende do banco); `BgMap_FillRect16(...)`;
  `GfxBankA_LoadTilesAndPalette(fight+0x38, 0)`; zera 0x2000 B em 0x03002C30.
- Passo 1: REG_DISPCNT |= 0x8000, FxBlend, gráficos da técnica/medaforce (OBJ), grava 0x08043E49 em 0x030017F0.
- Decompile e literais em HA2.md (0x0801E09C) e HA2b.md. Confirmado.

## Tabela de bancos: GfxBankA_LoadTilesAndPalette 0x08043B7C
- Ponteiro da tabela: literal 0x08043BD8 = **0x083B2578**. Entrada de 16 B por banco `k`, em `0x083B2578 + 16*k`:
  | off | conteúdo | destino |
  |---|---|---|
  | +0 | bloco Malias de tiles (char) | `Lz_DecodeIfMagic` -> VRAM 0x06008000 (char base 0x8000, BG2/BG3) |
  | +4 | ponteiro de paleta (ROM) | `bios_cpufastset` -> 0x05000000, 0x40 palavras (param2 == 0) |
  | +8 | bloco Malias de mapa A | `LoadTilesWithMalias` -> buffer EWRAM (staging 0x02030000) |
  | +12 | bloco Malias de mapa B | `LoadTilesWithMalias` -> staging |
  O staging depois é copiado por CPU para as telas BG2 (0x0600C800) e BG3 (0x0600D800) (R5, pass 6).
- Entradas 0..15 lidas (ponteiros de ROM):
  | k | +0 tiles | +4 pal | +8 mapA | +12 mapB |
  |---|---|---|---|---|
  | 0 | 0x085227F4 | 0x0853E1B0 | 0x08537C44 | 0x0853AB80 |
  | 1 | 0x08524B60 | 0x0853E2B0 | 0x08537CCC | 0x0853B3E4 |
  | 2 | 0x08526664 | 0x0853E3B0 | 0x08537E8C | 0x0853BA7C |
  | 3 | 0x08528818 | 0x0853E4B0 | 0x08537FFC | 0x0853C31C |
  | 4 | 0x0852A2E8 | 0x0853E5B0 | 0x085388A4 | 0x0853C3A4 |
  | 5 | 0x0852BFDC | 0x0853E6B0 | 0x08539068 | 0x0853C4D8 |
  | 6 | 0x0852E2FC | 0x0853E7B0 | 0x085398A8 | 0x0853C754 |
  | 7 | 0x0852FD34 | 0x0853E8B0 | 0x08539FD0 | 0x0853C8CC |
  | 8 | 0x08531DD0 | 0x0853E9B0 | 0x0853A0BC | 0x0853CF00 |
  | 9 | 0x08533C74 | 0x0853EAB0 | 0x0853A834 | 0x0853D01C |
  | 10 | 0x08535A88 | 0x0853EBB0 | 0x0853A8A8 | 0x0853D8F0 |
  | 11 | 0x0853ECB0 | 0x08550F58 | 0x08556600 | 0x08552D4C |
  | 12 | 0x08553C0C | 0x08540938 | 0x085510C0 | 0x08556700 |
  | 13 | 0x08552E6C | 0x0855404C | 0x08542188 | 0x08551384 |
  | 14 | 0x08556800 | 0x08552FF8 | 0x08554484 | 0x0854407C |
  | 15 | 0x085516B0 | 0x08556900 | 0x085532B0 | 0x085548E0 |
  **Válidos: 0..10.** Nos bancos 11..15 o bloco de mapa (+8/+12) não decodifica como Malias de 4096 B: não são fundos
  deste formato (ver também 0x08553ECB0 etc.). Lida direto da ROM original (EU).
- Banco 4 bate com o extrator de corte (`extract_battle_bg.py`, spec "cut": tiles 0x52A2E8, pal 0x53E5B0, mapas
  0x53C3A4 e 0x5388A4). Confirmado pela tabela; o R5 já tinha visto esses endereços no f1417/f1423 da rec_2.
- Mapas de banco 0..10 (encontro aleatório) ainda não batem com o extrator de campo (tiles 0x545CD4, pal 0x556A00):
  esses endereços não aparecem na tabela acima. Conferir antes de usar o campo.

## Mapas dos bancos 0..10 (cut, 512x160 gerado por pc/extract/extract_battle_bg.py; 8 bancos de paleta x 16 cores)
Nomes pelo que se vê na imagem (conferido na luta, LÖVE headless, `--bgBank K`):
| k | cena |
|---|---|
| 0 | deserto (areia, mesas de rocha) |
| 1 | quase branco/amarelo (brilho ou transição; a conferir) |
| 2 | céu com nuvens |
| 3 | costa rochosa / penhasco com água |
| 4 | **Floresta**: campo verde, árvores ao fundo, flores (é o banco da área 4 de gMapEncounters) |
| 5 | cachoeira / água com pedras |
| 6 | rua (asfalto, prédios, ônibus) |
| 7 | interior (piso de tacos, paredes, mobília) |
| 8 | praia (areia, pedras, mar) |
| 9 | mar (degradê azul, sem objetos) |
| 10 | base/laboratório metálico |
Teste: banco 4 e banco 6 dão imagens diferentes na luta (diferença em toda a tela).

## Nomes (Forest, City...) e id da escola
- Nomes de terreno (Forest, Street, Inside, Plains, Valley, Haunted House) vêm só do GameFAQs em `ref/gamefaqs/`
  ("Forest (inside the school)"). Não há nome de cenário na ROM achado. NÃO mapeado a banco ainda.
- Mapas com encontro aleatório e área 4 (= banco 4, Floresta): **1, 46, 173** (battle_tables.map_encounter). Qual é a
  escola ainda não confirmado: o layer0 do mapa 1 sai quase vazio. Falta ler gCurrentMapId (0x03005FDE) em rec_7 (escola).
  Replay headless do tools/replay_record.py abre janela do mGBA: não usado (o usuário joga no mGBA).

## Tiles de HUD x cenário (aviso do coordenador)
- Na luta, BG3 (tela 0xD800, char 0x8000) mistura cenário (grama/fundo) com tiles de HUD (molduras, ícones, barras).
  Pelo que está acima, o banco cobre só o cenário (+0 tiles, +4 paleta, +8/+12 mapas). Tiles de HUD vêm de outro carregamento
  (ex.: `Fight_InitBotPartPanel`, `Overlay_CopyBufferAndLayoutText`). Quais linhas/tiles da BG3 são cenário ainda não medido:
  o porte deve desenhar o cenário e deixar o HUD por cima (battle.drawHud depois do fundo).

## Porte (pc/game)
- Encontro aleatório: `encState.area` (= +1 de gMapEncounters, copiado em `encounter.enterMap`) vai como `bgBank` em
  `switchScene("battle", ...)` (world.lua, randomBattle). `battle.lua` configure carrega `battle/bg/<k>_field.png` e
  `_cut.png`; sem banco (luta de história) fica o desenho à mão. Teste: `pc/game/tests/test_bgbank.lua` OK.
- Cache: `pc/cache/battle/bg/<k>_{field,cut}.png` (bancos 0..10), gerado por extract_battle_bg.py; registrado em extract_all.py.

## Abertos
1. Escritor de +0x38 nas batalhas de história (watch 0x03001838 no replay). Hoje a luta de história fica sem fundo ROM.
2. Quem põe 0x03006F84 (scene_init_b).
3. Confirmar o mapa da escola (1, 46 ou 173?) lendo 0x03005FDE em rec_7 sem janela do mGBA.
4. Tiles de HUD x cenário na BG3 (aviso da HUD): o banco cobre só o cenário; HUD vem de outro carregamento.
5. Rolagem (scroll) da BG não medida; o recorte de campo usa o canto sup. esq. (0,0).
