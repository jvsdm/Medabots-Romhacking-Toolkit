# REFKIT: o que já existe (WPF + scripts). Procure aqui ANTES de escrever algo novo

Caminhos: `T` = `D:/claude-brain/Medabots/git/Medabots-Romhacking-Toolkit`, `M` = `D:/claude-brain/Medabots`.
Regra de endereços: o `MedabotsRomSchema.cs` usa offsets da **Rokusho (E)**. Na Metabee EU a região de dados
0x07xxxx-0x3Dxxxx fica **0x20 bytes antes** (confirmado: toda tabela abaixo bate com `mtbe.csv` em offset-0x20).
`RomLayout.Resolve` acha as tabelas pelo código (literal pools), então funciona em qualquer versão/ROM expandida.
Offset de arquivo = endereço - 0x08000000. Nunca copie texto do jogo.

## 1. Biblioteca C# `T/src/Medabots.Rom/` (pule `obj/`, `bin/`)

| Área | Arquivos | Lê/escreve | Formato / endereço (offset arquivo, esquema Rokusho) |
|---|---|---|---|
| Cabeçalho/perfil | `Projects/MedabotsRomTextProfiles.cs`, `MedabotsRomAddresses.cs` | detecta ROM por 18 B em 0xA0 | Perfil **Metabee E** (offsets já EU, bate com mtbe.csv): ponteiros de texto 0x47DF24, dump de texto 0x7F5500, starter 0x7852CF, ponteiros de batalha 0x3C1B80 (0xF6 batalhas), tabela de eventos 0x769EC0 (0x8F0 eventos) |
| Layout | `Layout/RomLayout*.cs`, `RomTableSignatures.cs` | resolve ~40 tabelas por assinatura de código | usar `RomLayout.Resolve(rom)`; `TryGet(nome)`; nomes = campos `*Offset` de `MedabotsRomSchema` sem o sufixo |
| Esquema | `Projects/MedabotsRomSchema.cs` | constantes | ver índice no fim; tamanhos: parte 0x10 (481), batalha 0x28 (3 bots de 12 B após cabeçalho de 4), medalha info 0x10 (27), encontro 4 B (192), loja 4 B x 15 (0xFF vazio, 0x20 condicional), mapas 192, 3 camadas, 16 capítulos, paleta de mapa 0x200, cabeçalho de tilemap 0x10, retrato 9 por personagem (176), paleta 0x20 |
| Ponteiros | `GbaPointer.cs`, `RomFile.cs` | ToFileOffset/ToRomAddress/TryReadFileOffset/WriteFileOffset | base 0x08000000 |
| Compressão | `Compression/GbaLz77.cs`, `Malias2.cs`; `Expansion/ExpansionCodecs.cs` | Decompress/Compress/TryGetEncodedLength | LZ77 BIOS (0x10) e Malias2 ("Le") dos gráficos; Python equivalente em `M/tools/codecs_gba.py` |
| Texto | `Text/MedabotsTextCodec.cs`, `MedabotsMessageTableReader.cs`, `MedabotsTextPatcher.cs`, `MessageId.cs`, `PtBrFont.cs`, `ExtendedFont.cs`, `ImportedFont.cs`, `ItalicFontRepair.cs`, `ptbr_font.bin` | Encode/Decode/DecodeMessage; 16 bancos (`MessageBankCount`), ponteiro de banco -> lista de ponteiros de mensagem; `MessageId(bank,index)` | fonte normal 0x4BFC64 e itálica 0x4C59A4 (ROM Rokusho; Metabee -0x20); glifos 8x16 4bpp; ver também 2.3 |
| Eventos | `Events/EventScriptReader.cs`, `EventScriptSerializer.cs`, `EventScriptRewriter*.cs`, `EventInstructionPatcher*.cs`, `EventDatabaseRelocator.cs`, `EventOpcodeDefinitions.cs`, `LegacyEventOpcodeTable.cs`, `EventAstTypes.cs` | lê script por id, reserializa, recoloca o banco inteiro em espaço livre, labels/jumps relativos | ver 1.1 |
| Mapas | `Maps/*` (`MapEntitySpawn*`, `MapWarp*`, `MapCollision*`, `MapEncounter*`, `MapMusic*`, `MapLayer*`, `MapTileset*`, `MapOverlay*`, `MapExpander`, `MapAddition`) | spawns/pessoas, warps, colisão, encontros, música, 3 camadas de tilemap, tileset/paleta, criar/expandir mapa | ver 1.2 |
| Peças | `Parts/PartTableReader.cs`, `PartDefinition.cs`, `PartKind.cs` | 481 x 16 B; id = bot*4 + tipo (0 cabeça, 1 braço D, 2 braço E, 3 pernas) | bytes: 0 compat. medalha, 1 técnica (pernas: tipo), 2 especialidade, 3 gênero, 4 armadura, 5 taxa de sucesso (pernas: propulsão), 6 poder (pernas: evasão), 7 reação em cadeia (pernas: defesa), 8 usos (pernas: proximidade), 9-15 desconhecidos. Tabela achada pela assinatura `0F 22 02 00 23 15 08 01 08 00` |
| Batalhas | `Battles/BattleTableReader.cs`, `BattleDefinition.cs`, `BattleBot.cs`, `BattleAction*.cs`, `BattleSpecialityTemplateHelper.cs`; JSON `battleactions.json` | ponteiros u32 -> 0x28 B | cabeçalho: [0] personagem, [1] modo de init, [2] nº de bots, [3] flags de template; bots a partir de +4, 12 B: cabeça, braço D, braço E, pernas, medalha, nível, semente de especialidade (4 B), reset do ciclo, 0. Action scripts: tabela 0x3C6CC8 (0x78), handlers de opcode 0x3AF1A0 (0x40); `battleactions.json` descreve 26 opcodes e 48 ações |
| Encontros | `Encounters/*` | 192 x 4 B (4 ids de batalha) | assinatura `00 00 00 00 9B F5 9B A7 9B F5` |
| Lojas | `Shops/*` | 15 x 4 slots de 1 B (id de bot; 0xFF vazio; 0x20 condicional) | assinatura `13 00 FF FF 13 00 42 FF 13 00` |
| Inicial | `Starter/*` | peça e medalha iniciais, gênero | assinatura `01 02 00 56 5D 01 62 17 01` (delta -1) |
| Imagens | `Images/*` (`ImageAssetRepository*`, `TileImageCodec`, `PortraitExpander`, `OverworldSpriteFrameExtractor`, `SpriteProjectEditSystem`) | sprites overworld (336), retratos, mini-sprites compostos de batalha, peça grande (`LargePartDisplay`, 63 KB de regras), PNG import/export | `TileImageCodec`: 4bpp<->índices, BGR555->RGB888 |
| Expansão | `Expansion/BotAdder.cs`, `RomExpander.cs` (ROM de 16 MB), `NaviImporter.cs`, `ShingataGraphicsPorter.cs`, `MiniSpritePalette.cs`, `ExpansionPatchSites.cs`, `BotArt.cs`, `expansion.manifest.json` | adiciona bots (120+), porta bots do Shingata/Navi, patch dos sítios de código que contam bots | manifesto gerado por `M/tools/export_manifest.py`; sítios por `M/tools/export_patch_sites.py` (a partir de `M/tools/patches.py`) |
| Projeto | `RomHackProject.cs`, `RomHackSession.cs`, `Projects/RomHackProjectSerializer.cs` (JSON), `RomHackProjectApplicator.cs`, `FreeSpaceAllocator.cs`, `RomPatchAction.cs`, `*ProjectEditSystem.cs` | projeto = lista de patches (mensagens, scripts de evento, spawns, warps, colisão, encontros, música, sprites, retratos...) aplicada sobre a ROM do usuário | `RomPatchAction(offset, bytes, descrição)`; `FreeSpaceAllocator.ForRom(rom).Reserve(len, align)` |
| Catálogo | `medabots.catalog.json` (+ `Metadata/MedabotsCatalog.cs`, `MedabotsMetadata.cs`) | nomes: `characters`(96), `maps`(192), `parts`(121), `bots`(121), `specialities`(8), `techniques`(60), `medals`(30), `songNames`(52), `bestMedalByBot`, `femaleBots` | nomes só para exibição |

### 1.1 Eventos: endereçamento e opcodes
- Banco de eventos: evento N fica em `base + bank[N]*0x4000 + (ptr[N] - 0x4000)`, `base = tabela - 0x4000`, ptr u16 em `tabela + 2N`, bank u8 na tabela de bancos (`base + header[2..3]`; schema 0x11E0 = literal de código). Máximo 256 bancos x 16 KB. Argumento `EventBank` no arquivo = valor - 2 (`EventBankBias`).
- Constantes: END = 0x06, GOTO_EVENT = 0x19, multi-jump condicional = 0x2F (até 8 entradas), movimento: máscara 0xF0 = direção (N 0x00, S 0x10, O 0x20, L 0x30, nada 0xFF), 0x0F = distância. Jumps são deslocamentos relativos. Tamanhos legados por opcode em `LegacyEventOpcodeTable.cs`.
- Python equivalente: `M/pc/extract/extract_events.py` (saída `events.lua`).
- Tipos de argumento: Byte, Short, Jump, PackedTrackedObjectId, TrackedObjectSlot, Medal, Direction, Part, Bot, Move, BattleId, BattleModeFlags, PostBattleModeFlags, MapSceneVariant, EventBank, Music, Sound.
- Opcodes (`EventOpcodeDefinitions.cs`, ids hex; 125 definidos; os ausentes são livres/desconhecidos):
  - Texto/tempo: 01,02 Show_Message_A/B(bank,id) | 03 Close_Message_Box | 04 Wait_X_Frames(frames) | 05 Wait_For_Button_Press | 06 END (schema).
  - Tela: 07 Fade_To_Black | 08 Fade_From_Black | 09 Fade_To_White | 0A Fade_From_White (todos frames) | 40 Pulse_Screen_Transition | 43 Shake_Camera(axis,duration).
  - Mapa/jogador: 0B,0C Warp_A/B(map,u1,u2,x,y) | 0D Player_Face_Direction | 0E Hop_In_Place | 0F Hop_In_Direction | 10 Move_Player(4 passos) | 11 Change_Player_Costume | 42 Move_Player_One_Step | 69 Set_Map_Scene_Variant(variant,skip) | 84 ...When_Player_Faces_Up | 6B Set_Current_Chapter.
  - Eventos/variáveis: 12 Add_Event_Permanently(id16) | 13 Remove_Event_Permanently | 14,15 Remove_Current_Event_Now(_Keep_Collision) | 16 Remove_Event_Now | 17 Set_Event_Variable(id16,val) | 18 Increment_Event_Variable | 19 Goto_Event | 3E,3F Increase/Decrease_Capped_Event_Counter(slot,amount) | 2F Set_Value_From_EventFlag_Nibble.
  - NPC/atores: 1A NPC_Do_Animation | 1B NPC_Reset_Animation | 1C Rotate_Event_NPC | 1D Hop_Event_NPC | 1E Flicker_Event_NPC | 34 Rotate_NPC(id16,dir) | 1F,20,2B,6C,6E Initiate_Moving_Actor_*(packed_id,sprite,x,y,move) | 21 Initiate_Actor | 22 Rotate_Actor | 23,24,2D,6D,6F Move_Actor_*(slot,move) | 25 Unload_Actor | 26 Flicker_Actor | 27 Actor_Do_Animation | 28 Hop_Actor | 29,2A,7E Move_All_Tracked_Actors_* | 2C Set_Actor_Render_Priority | 35 Set_All_Tracked_Actors_Facing_Variant | 41 Flicker_All_Tracked_Actors | 53,83 Set_Object_Render_Mode | 60 Focus_Event_Object | 61 Restore_Focused_Event_Mode_Flags | 66 Animate_Tracked_Object_Transition | 68 Clear_Collision_For_Packed_Object | 63 Show_Effect(id,x,y) | 80-82 bloco de comandos de objeto em lote (Begin/Queue(8 args)/Execute) | 85-87 ciclo de facing (Begin/Queue(8 args)/Execute).
  - Fluxo: 30 Relative_Long_Jump(s16) | 31,32 Yes_or_No / No_or_Yes_Box(jump) | 4A Jump_If_Not_Player_Direction | 4B Jump_If_Item_Count_Below_Amount | 4C Jump_If_Missing_Part | 4D Jump_If_Not_Has_Medal | 4E Jump_If_Not_Has_Money | 4F Jump_If_Previous_Scene_Command_Failed | 50 Jump_If_Current_Active_Object_Index_Not_Equal | 51 Random_Jump | 5B Jump_If_Fewer_Than_Three_Complete_Part_Sets | 5C Jump_If_Missing_Complete_Monochrome_Part_Set | 7B Jump_If_No_Spare_Part | 78 Run_Scene_Command_10_If_At_Least_Three_Complete_Part_Sets.
  - Itens/jogo: 2E Adjust_Value_If_No_Spare_Part | 33 Start_Battle(battle,flags,pos_flags) | 36,37 Get/Lose_Item(id,qtd) | 38,39 Get/Lose_Part(bot,part,qtd) | 3A,3B Get/Lose_Money(u16) | 3C Get_Medal | 3D Get_Tinpet | 59 Open_Shop | 62 Equip_Starter_Parts | 76 Start_Medabot_Link | 5D-5F modos de overworld/encontro | 64 Reset_Overworld_UI_Render_State | 56 Load_Entities | 70-73,79,7A Run_Scene_Command_*.
  - Som: 44 Play_Sound | 45 Play_Music | 46 Stop_Music | 47 Fade_Out_Music | 48 Play_Persistent_Music | 49 Play_Map_Music.
  - NoOp: 00 Nop; 55, 57, 65, 74, 7C Reserved_NoOp.

### 1.2 Mapas: registros
- `MapEntitySpawnRecord` (pessoas/triggers; ponteiro por mapa em 0x483254): TileX u8, TileY u8, u16 `kind<<12 | id12` (kind 0x8 = gatilho ao pisar; 0x0/0x4/0x6 = gatilho de frente/marcador), u8 sprite+facing empacotados, u8 grupo de spawn, u16 máscara de visibilidade por capítulo (bit = capítulo 0..15).
- `MapWarpRecord` (ponteiro por mapa em 0x412E80): x, y, mapa destino, u8 (alto nibble = tipo de transição, baixo 3 bits = facing de chegada), 2 B desconhecidos, x/y destino.
- Tabelas por mapa (192): dimensões em metatiles 0x3F8920; tileset gfx 0x3F8AA0; atributos de cor 0x3F8DA0; tilemap das 3 camadas 0x3F90A0; paleta 0x3F99A0 (0x200 B); colisão 0x404354; warps 0x412E80; recurso de objetos de evento 0x413180; música 0x47E664; pessoas/spawns 0x483254; configuração de encontro 0x3BEC30. Camadas = LZ77, cabeçalho 16 B + u16 (tile 10 bits, flip H/V, banco de paleta 4 bits).
- Sprites overworld: ponteiros 0x3F7EA0, paletas 0x3F83E0, 336 sheets; Python: `extract_sprites.py`.

## 2. Editor WPF `T/src/Medabots.Rom.WPFEditor/`
Abas (MainWindow.xaml): Messages, Events, Maps, Parts, Robobattles, Encounters, Shops, Starter, Sprites, Font, Changes.
| Arquivo | Função |
|---|---|
| `MainWindow.Events.cs`, `EventScriptEditorWindow.*`, `EventPresentationBuilder.cs`, `EventStructuralActionsControl.*` | lista/edição de scripts, inserir/remover instrução, labels, mostrar texto formatado do evento |
| `MainWindow.Maps.cs` (147 KB), `.MapTools.cs`, `.MapTileset.cs`, `.MapExpansion.cs`, `.NewMap.cs` | render de mapa em camadas, editar spawns/warps/colisão/encontros/música, tileset PNG, novo mapa, expandir |
| `MainWindow.Sprites.cs` (164 KB), `.SpriteTools.cs`, `SpriteBrowserTreeBuilder.cs`, `PartSpriteDisplayLayout.cs` | navegador de sprites/retratos/peça grande/mini-sprites, export/import PNG, paleta |
| `MainWindow.MessagePreview.cs` | prévia de caixa de diálogo com larguras da fonte (equivale a `medtext.py`) |
| `MainWindow.Font.cs`, `TrueTypeRasterizer.cs` | fonte estendida / TTF para 8x16 (equivale a `port_ttf.py`) |
| `MainWindow.Expansion.cs`, `.PortraitExpansion.cs`, `.Roster.cs` | adicionar bots/retratos, elenco |

## 3. Scripts Python/Lua (USE ANTES DE ESCREVER UM NOVO). `python -I script ...`
Marca **[USE]** = ponto de partida obrigatório para o assunto.

### 3.1 Extratores do porte PC: `M/pc/extract/*.py` (entrada ROM original; saída `pc/cache`, fora do git)
| Script | Faz | Saída | Uso |
|---|---|---|---|
| **[USE]** `extract_all.py` | roda todos os extratores (mapas 0..191, fonte, texto, peças) | pc/cache inteiro | `extract_all.py ROM.gba` |
| `extract_map.py` | camadas do mapa | `maps/NNN/*.png`, `map.lua` | `ROM MAP_ID... [--out]` |
| `extract_objects.py` | colisão, warps, pessoas/triggers | `maps/NNN/objects.lua` | `ROM MAP_ID...` |
| `extract_sprites.py` | sprites overworld (12 quadros 16x24) | `sprites/SSS.png` | `ROM MAP_ID...` |
| **[USE]** `extract_events.py` | scripts mínimos de evento por mapa (reescreve events.lua inteiro) | `events.lua` | `ROM MAP_ID...` |
| **[USE]** `extract_text.py` | texto todo via dumper do Toolkit | `text.lua` | `ROM [--out]` |
| `extract_font.py` | fonte normal/itálica, avanços | `font/*.png`, `font.lua` | `ROM [--ptbr bin]` |
| **[USE]** `extract_parts.py` | bots, peças, técnicas, medalhas | `parts.lua` | `ROM` |
| `extract_part_sprites.py` | peça grande (480) | `parts_large/*.png` | `ROM` |
| `extract_portraits.py` | retratos 64x64 (176 x 9) | `portraits/`, `portraits.lua` | `ROM` |
| `extract_battles.py` | tabela Robattle (bots por batalha) | `battles.lua` | `ROM` |
| **[USE]** `extract_battle_tables.py` | medalhas, skill base inimiga, prioridade de IA, encontro por mapa, templates | `battle_tables.lua` | `ROM` |
| `extract_battle_sprites.py` | gráficos de batalha por bot | `battle/BBB/` | `ROM BOT_ID...` |
| `extract_battle_bg.py` | fundos de batalha | `battle/bg_*.png` | `ROM` |
| `extract_effects.py` | efeitos do Missile (corte) | `effects/`, `effects.lua` | `ROM` |
| `extract_motion.py` | roteiros de animação (gMotion*, gMoveScriptTable, gMoveStyle) | `motion.lua` | `ROM` |
| `extract_shops.py` | 15 lojas | `shops.lua` | `ROM` |
| `extract_music.py`, `extract_sfx.py` | PCM e índice; SFX | `sound/`, `music.lua`, `sfx.lua` | `ROM` |
| `extract_intro.py`, `extract_ui.py`, `extract_medabots_menu.py`, `extract_credits.py` | título; caixa de diálogo; tela MEDABOTS; créditos | `intro/`, `ui/`, `credits.lua` | `ROM` |
| `extract_save.py` | .sav/savestate -> dados crus | `save.lua` | `SAV_OU_SS` |

### 3.2 Ferramentas `M/tools/`
| Script | Faz | Uso |
|---|---|---|
| **[USE]** `medarom.py` | base comum: ROM, ponteiros, texto, offsets Metabee EU já corrigidos | `import medarom` |
| `codecs_gba.py` | Malias e LZ77 (leitura) | import |
| `rom_asm.py`, `shingata_dis.py`, `xrefs.py`, `find_store.py`, `mapflow.py`, `crossref_shingata.py`, `gen_signatures.py`, `bot_tables.py` | RE em Thumb: disassembly com literais, xrefs a tabelas, stores para um endereço, fluxo do id do mapa, assinaturas | ex.: `find_store.py ADDR [ROM]`; `rom_asm.py` import |
| **[USE]** `replay_record.py` | repete gravação de `battle_record.lua` headless no mGBA; dump de memória/quadros/breakpoints/watch | `REC_DIR --frames F1,F2 --out DIR [--dump ADDR:LEN@F] [--break ADDR] [--watch ADDR] [--rom]` |
| **[USE]** `battle_record.lua` | mGBA: grava savestate + teclas de uma batalha (reprodutível) | carregar no mGBA (Tools > Scripting) |
| **[USE]** `live_record.lua` / `live_record_view.py` / `live_diff.py` | log leve de mudanças de estado ao vivo; resumo; linha do tempo e 1ª divergência contra `--trace` do porte | `live_diff.py tl ARQ`; `live_diff.py cmp LIVE PORT [--ctx N]`; `live_record_view.py LOG [--max N]` |
| **[USE]** `oam_render.py` | desenha OAM de quadros da gravação como o GBA | repete via replay_record; ver docstring |
| `battle_capture.lua`, `analyze_captures.py`, `oam_film.py`, `render_capture.py`, `render_screen.py` | captura de batalha (OAM/VRAM/IO), resumo, tira de quadros, remontagem de OBJ e da tela inteira | `oam_film.py cap_N [passo] [colunas]` |
| `ewram_scan.lua`, `inv_dump.lua`, `gdbrsp.py` | blocos de EWRAM tocados; dump de inventário (EWRAM 0x0202E000); cliente GDB do mGBA (regs/mem/watch) | `gdbrsp.py mem ADDR LEN` |
| `debug_keys.py`, `debug_poke.py`, `menu_nav.py` | rodar savestate com teclas/pokes de RAM e tirar prints | args na docstring |
| `render_map.py`, `render_bot.py` | mapa em PNG (Core e Shingata); bot de batalha + peças | `render_map.py`; `render_bot.py BOT [rom] [out]` |
| `save_edit.py` | edita .sav EEPROM 8 KB (blocos de 8 B invertidos; checksum u16 em +0xFF4) | args na docstring |
| `build.py`, `patches.py`, `export_manifest.py`, `export_patch_sites.py`, `gfx_port.py`, `port_shingata.py`, `shingata.py`, `shingata_names.py`, `extract.py`, `mini_palette.py` | pipeline de expansão (ROM 16 MB, bots novos, sítios de código, gráficos do Shingata) | `build.py [--shop SHOP:SLOT:BOT] [--out]` |
| **[USE]** `story_graph.py` | grafo da história por capítulo: virada (op chapter), estado/registro exigidos, quem grava/esconde, rota de mapas/tiles e avisos; sai `pc/research/STORY_GRAPH.md` (`--dot` = Graphviz) | `python -I tools/story_graph.py [--dot ARQ] [--chapters 0,1]` (lê `pc/cache`, só leitura) |
| `export_template.py`, `check_template.py`, `navi_*.py`, `navi.py`, `mold_lab.py`, `custom_lab.py`, `medarot9.py` | modelos de desenho de peças, moldes novos, Navi, Medarot 9 | `export_template.py BOT_ID` |

### 3.3 `T/re/tools/` (Toolkit)
| Script | Faz | Uso |
|---|---|---|
| **[USE]** `text/medtext.py` | tabela de caracteres, larguras da fonte, quebra de caixa como o jogo | import |
| **[USE]** `text/dump_text_v3.py` / `insert_text_v3.py` | texto da ROM <-> JSON (preserva traduções; `--reflow` na inserção) | `dump ROM out.json`; `insert ROM_IN json ROM_OUT [--reflow]` |
| `text/port_ttf.py`, `ptbr_font_extract.py`, `font_fix.py` | TTF -> fonte 8x16; glifos PT-BR -> `ptbr_font.bin`; alinhamento | args na docstring |
| `large_part_layout.py` | posição dos pedaços do bot grande (regra de `ImageAssetRepository.LargePartDisplay.cs`) | `ROM --bot N --side 0` |
| `find_ui_gfx.py` | acha assets Malias2 de uma tela pelo literal pool | args na docstring |
| `mold_lab.py`, `navi_lab.py`, `custom_lab.py` | cópias de `M/tools/` | idem |

## 4. Ghidra e símbolos (`T/re/`)
| Item | Faz |
|---|---|
| **[USE]** `tools/build_sync.py` | funde `functions/*_simbolos.csv` em `symbols/mtbe.csv` (nome antigo vai para o comentário) e gera `ghidra/medabots_sync.tsv` (linhas F/L/S/D) |
| **[USE]** `tools/build_types.py` | gera `ghidra/medabots_types.tsv` de `functions/ST_*.csv` (struct,offset,size,name,confidence,source) |
| `ghidra/MedabotsSync.java` | aplica funções/nomes do tsv no Ghidra (log em `medabots_sync_log.txt`); depois salvar o projeto |
| `ghidra/MedabotsTypes.java` | cria structs `/Medabots` e tipa globais (linhas T/M/A) |
| `ghidra/MedabotsSetup.java`, `MedabotsExportSymbols.java`, `MedabotsPortSymbols.java` | preparar programa; exportar nomes para mtbe.csv; portar símbolos Rokusho -> Metabee por assinatura de bytes |
| `symbols/mtbe.csv` | `address,name,kind,count,comment` (~4.6k linhas); busca só com `python -I` |
| `reference/*.md`, `HANDOFF.md` | afinidades de pose, animação de batalha, layout de peça grande, retratos, expansão |

## 5. Índice: endereços/formatos que o WPF conhece e NÃO estão no `mtbe.csv`
Endereços são offsets de arquivo do esquema Rokusho; para a Metabee EU subtraia 0x20 (a confirmar para os marcados `?`: a região acima de 0x3Dxxxx pode ter outro deslocamento).
- Evento: tabela de bancos `0x11E0` (literal de código; fórmula em 1.1), `EventBankSize` 0x4000, janela 0x4000-0x7FFF.
- `0x50F31C` paletas OBJ dos mini-sprites compostos (6 paletas) ?
- `0x50F4BC` conjunto de paletas dos componentes da tela de seleção de peça ?
- `0x50F33C`, `0x50F39C`, `0x4E441C` blocos A/B/C de paleta OBJ da tela de detalhe de peça ?
- `0x785FD8` folha de tiles compartilhada da tela de peças (largura 8 tiles) ?
- `0x4C4C00`, `0x4C4FE0`, `0x4C4B70`, `0x4C5020`, `0x4C53EC`, `0x4C3E88`, `0x4C88DC` folhas A..G de tiles do detalhe de peça ?
- Formatos sem símbolo de struct: `MapEntitySpawnRecord` (7 B de campos + máscara u16), `MapWarpRecord` (8 B), `PartDefinition` bytes 9-15, `BattleBot` (12 B), cabeçalho de batalha (4 B), `Battle Action` `battleactions.json`.
- Assinaturas de código que localizam tabelas (úteis em ROM expandida): parte, encontro, loja, starter (ver seção 1).
