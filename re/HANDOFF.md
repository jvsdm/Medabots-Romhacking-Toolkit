# Handoff - engenharia reversa Medabots Metabee (E)

Instrucoes para o Claude Code local, com o Ghidra aberto e o GhidraMCP conectado.

## Contexto

- Jogo: Medabots: Metabee (Europe), GBA, `MEDABOTSMTBVA8BPE9`. Mesma engine de Rokusho e de Shingata Medarot.
- Objetivo: mapear a engine para hacks no estilo CFRU (C injetado + hooks) e alimentar o Toolkit (schema de tabelas no estilo HexManiacAdvance).
- Documento tecnico de referencia: `re/reference/EXPANSAO_MEDABOTS.md` (PT) e `re/reference/MEDABOTS_EXPANSION.en-US.md` (EN), copias sincronizadas do vault local (a fonte e o vault; enderecos em offset de arquivo da Metabee E). Secao 14: quebra de linha do dialogo, retratos, divisao de trabalho.
- Fonte da verdade dos nomes: `re/symbols/mtbe.csv`. O banco do Ghidra e descartavel.

## Estado do projeto Ghidra

- Import: Raw Binary, `ARM:LE:32:v4t`, base `0x08000000`.
- `re/ghidra/MedabotsSetup.java` ja rodou: blocos EWRAM/IWRAM/IO/PAL/VRAM/OAM, ROM dividida em `ROM` (0x08000000-0x0807FFFF, exec) e `ROM_DATA` (0x08080000+, sem exec), 148 simbolos aplicados.
- Auto-analise rodou com Aggressive Instruction Finder (2) e Non-Returning Functions - Discovered desligados.
- Validado: `GetPartStats` (0x08042B58) decompila para `&gPartStats + bot*0x40 + kind*0x10`, copia 16 B.

## Protocolo nuvem <-> local

- **Nuvem** (este repo, sem ROM nem Ghidra): analise estatica pesada. Le o codigo com as ferramentas que tiver,
  propoe nomes, struct e hipoteses; grava no CSV (comentario "provavel" quando nao confirmado) e numa secao
  "Achados" no fim deste arquivo, com o que precisa ser testado localmente.
- **Local** (maquina do usuario: Ghidra 12.1.4 + GhidraMCP, mGBA com scripts Lua de captura, ROMs, editor WPF):
  confirma no Ghidra e no jogo, faz as mudancas no Toolkit, atualiza o vault e devolve aqui o que confirmou
  (tira o "provavel" do CSV, move o item de "Achados" para "Fatos ja estabelecidos").
- Cada lado registra no fim deste arquivo o que fez e o que deixou para o outro (secao "Diario").
- **Toolkit**: o trabalho de expansao (bots ate 236, mapas ate 255, eventos, mensagens com quebra de linha,
  retratos ate 256, editor WPF) foi publicado nesta branch num commit unico (2026-10-10), igual a branch local
  `medabots-expansion`. A sessao local continua sendo onde o editor e testado; na nuvem, mudar o Toolkit so com
  testes (`tests/Medabots.Rom.Tests`, que precisam das ROMs e por isso rodam aqui).

## Regras

1. Todo nome novo vai para o Ghidra via MCP e, no fim de cada tarefa, para o CSV: pedir ao usuario para rodar Tools > Medabots > Export Symbols (ou editar `re/symbols/mtbe.csv` direto, mantendo o formato e a ordem por endereco).
2. Formato do CSV: `address,name,kind,count,comment`. kinds: `func_thumb`, `func_arm`, `ptr` (count ponteiros), `data` (count bytes), `label`.
3. Ponteiros de funcao com bit 0 ligado = Thumb; endereco real sem o +1.
4. Codigo fica abaixo de `0x08080000`. Nao criar funcoes em `ROM_DATA`.
5. Convencao de nomes: funcoes `PascalCase`, globais `gNome`, tabelas `gNomeTable`, registradores `REG_*`, sitios de patch `site_*`, nomes provisorios com sufixo `_Unk` ou comentario "provavel".
6. Nao versionar ROM, `.sav`, save states, `.gpr`/`.rep` nem dumps de RAM/VRAM.
7. Commits na branch `claude/medabots-project-ajyaeb` do repo `jvsdm/Medabots-Romhacking-Toolkit`.

## Fatos ja estabelecidos

| Endereco | Nome | Nota |
|---|---|---|
| 0x080000C0 | crt0 | ARM; stacks, INTR_VECTOR, chama AgbMain |
| 0x08000104 | IntrMain | ARM; le REG_IE/REG_IF |
| 0x0800025C | AgbMain | Thumb; loop principal |
| 0x083ABF98 | gMainStateTable | 6 ponteiros; estados 0-3 reais, 4-5 = MainStateNop |
| 0x0300178C | gMainState | boot = 3, soft reset = 0 |
| 0x03002B88 | gPlayTimeFrames | trava em 21599999 (99:59:59) |
| 0x0807CCF0 | CallViaR0 | chama ponteiro em r0 |
| 0x0807CCE8 | VBlankWait | provavel |

Demais enderecos: ver `re/symbols/mtbe.csv`.

## Banco do Simon (STulling) - Rokusho (E)

- O Simon passou o banco Ghidra dele, da **Rokusho (E)** (`MEDABOTSRKSVA9BP`): 734 funcoes nomeadas, 45 tipos
  (structs de batalha, medalha, peca, mapa, evento; enums ActionId/TechniqueId/MedalId/AttributeId/...), 744 comentarios.
  O arquivo `.gzf` fica fora do git (pedir ao usuario).
- Codigo Rokusho = Metabee no mesmo endereco ate ~0x08070000 (so os literais para dados mudam); dados em -0x20
  (-0x24 depois de ~0x08488000); wrappers de BIOS em -0x20.
- Portado em 2026-10-10 para `re/symbols/mtbe.csv` (comentarios "Simon/Rokusho"): funcoes so quando os 48 primeiros
  bytes (BL mascarado) batem na Metabee; tabelas pela comparacao de conteudo; RAM no mesmo endereco. Nomes nossos
  ja estabelecidos ficaram, com o do Simon no comentario. Fora: `re/symbols/simon_skipped.csv` (41, conferir).
- Tipos dele em `re/types/medabots_simon.gdt` (Data Type Manager > Open File Archive; arrastar para o programa).
- **Tudo que veio dele e "provavel" na Metabee ate conferir no Ghidra/emulador.**

## Tarefas, em ordem

### 1. Identificar os estados principais
- Decompilar `MainState0..3` (0x08002534, 0x0804763C, 0x0805BBE0, 0x08068440).
- Classificar cada um (intro/logos, titulo, campo, batalha, menu) pelas funcoes chamadas, textos e tabelas usadas.
- Renomear para `MainState_<Nome>`. Aceite: cada estado com nome e uma frase de justificativa no comentario.

### 2. Engine de acoes de batalha
- **Status (Simon)**: `gTechniqueActionValidationHandlerTable` 0x083AEF90 (60), `gTechniqueActionSetupHandlerTable` 0x083AEEA0 (120), `gActionScriptOpcodeHandlerTable` 0x083AF180 (128), handlers `ActionFamily_*`, `Fight_DispatchActionSequenceTable0..3`. Falta conferir na Metabee e nomear Transform 51-55.
- Achar a tabela de dispatch das 60 acoes (indice = byte 1 do registro de peca, "technique"; nomes em `gTechniqueNames` 0x083BE868).
- Comecar pelas xrefs de `GetPartStats` dentro do estado de batalha.
- Nomear a tabela (`gBattleActionTable`) e os handlers conhecidos (`BattleAction_Sword`, ..., `BattleAction_Transform`). A ordem das acoes segue a lista da Medapedia (1 Sword ... 60 Reinforce); confirmar pelo indice.
- Aceite: tabela com N ponteiros no CSV e pelo menos os handlers de Transform (51-55) nomeados.

### 3. Struct do Medabot em batalha
- **Status (Simon)**: tipos `BattleBotStateRecord` (256 B; pecas `BattlePartState[4]` em +0x48, 24 B cada; Medaforce em +0xC1), `BattleSideState` (3 bots), `FightInformation`. Falta achar a instancia em RAM na Metabee e conferir.
- A partir do handler de Transform (troca a peca em batalha), achar a struct em RAM: IDs das 4 pecas, armadura atual/maxima por peca, medalha, Medaforce, status.
- Cruzar com `gBattlePartState` (0x020260D0, 50 x 16 B por molde) e `gBattleRootX/Y`.
- Criar o tipo no Data Type Manager (`BattleMedabot`) e aplicar.
- Aceite: struct com offsets documentados em comentario e listada no CSV como label do array.

### 4. Medaforce
- **Status (Simon)**: `medal_medaforce_list` 0x083B7F8C e `Fight_TrySelectMedaforceAction`/`Fight_SelectMedaforceActionSource`. Falta o handler de execucao.
- Achar a tabela de Medaforces (3 por medalha: Lv10/30/60) e o handler de execucao.
- Referencia: lista de Medaforces por medalha na pagina "Medals in Medarot 2 Core" da Medapedia.
- Aceite: `gMedaforceTable` e handlers nomeados.

### 5. Medalhas
- **Status (Simon)**: 30 medalhas x `RomMedalInfo` (16 B: id, classe, 4 limites de rank, bonus de compatibilidade, 8 bases de especialidade) em 0x083B62D8. Conferir e corrigir o count no CSV.
- `gMedals` em 0x083B62D8: confirmar se sao 30 registros de 16 B (A-Z + Cat, ?, Bottlero, !) e decodificar os campos (atributo, bonus, alvo, 8 skills).
- Procurar constantes `#0x1D`/`#0x1E` ligadas a contagem de medalhas.
- Aceite: layout do registro documentado; count corrigido no CSV.

### 6. Montagem do bot grande para qualquer bot (PRIORIDADE - PC)
- Ler `src/Medabots.Rom/Images/ImageAssetRepository.LargePartDisplay.cs`, `LargePartDisplayAsset.cs`,
  `ImageAssetPatcher.LargePartDisplay.cs` e a previa do WPF (`Medabots.Rom.WPFEditor`, grep "LargePart").
- Entregar `re/reference/large_part_layout.md`: tabelas (enderecos EU; lembrar do desvio +0x20 do Toolkit em
  varias tabelas), layout dos registros, quantos moldes por peca, como sai a POSICAO de cada molde por bot (o
  local ja tem o Metabee montado pela OAM: cabeca m09 (-12,-49) / m10 (-15,-75), braco esq m14 (-3,-63) /
  m15 (0,-42), pernas m20-m24, braco dir m17 (-36,-62) / m18 (-36,-39), origem = centro dos pes) e
  pseudocodigo Python para montar qualquer bot. Aceite: as offsets do Metabee saem da sua regra.

### 7. Animacao de batalha por molde (0x083D1E40)
- Formato da tabela: quadros, poses de ataque (bracos pose1/pose2), efeitos (tiro, explosao), duracao.
- Cruzar com `battleactions.json` e `BattleActionScript*` do Toolkit. Entregar `re/reference/battle_anim.md`.

### 8. Mini sprite: os 8 quadros e as paletas
- `BattleCompositeSpriteComponentAsset`, `SpriteProjectEditSystem`, `OverworldSpriteFrameExtractor`, familia de
  paleta 0x3BEB94. Quais quadros o jogo usa para andar/atacar/peca quebrada; como escolhe a paleta OBJ.
- Local mediu: pernas 1,2,1,3 (~5 quadros cada) andando ate o centro; bracos so sobem/descem.

### 9. Arte do HUD da batalha
- Onde estao (ROM, formato) os icones das faixas, a barra central, digitos do TIME, barras de HP, letreiros
  "CRITICAL HIT", "LEG DEFENSE", "HEAD DEFENSE", "STOP MOBILITY", e a moldura do menu de comandos.
- Local ja achou os FUNDOS (Malias): campo tiles 0x545CD4, mapas 0x554D1C/0x553374, paleta 0x556A00; corte
  tiles 0x52A2E8, mapas 0x53C3A4/0x5388A4, paleta 0x53E5B0. Os assets do HUD devem estar perto.

### 10. Retratos dos dialogos
- `PortraitAsset`, `PortraitExpander`, tag `<PORTRAIT:a,b,c>` do texto: tabela, formato, expressoes, o que
  significa cada um dos 3 bytes. Entregar a regra para desenhar o retrato na caixa de dialogo.

## Pendencias conhecidas

- `MedabotsLabel.java` antigo do usuario usa enderecos com base 0 (sem 0x08). Converter os nomes uteis para o CSV somando 0x08000000: `fn_MedapartMenu_ButtonHandler` 0x6980, `fn_MedapartMenu_Render` 0x59C0, `fn_MedapartMenu_PartTypeLogic` 0x6F5C, `fn_PageWrap` 0x4361C e os sitios `PATCH_*`/`SKIP_*`.
- Conflito: o script antigo chama 0x3B62D8 de `tbl_MedapartStats` e 0x3BBB4C de `tbl_SpritePointers`; o documento diz medalhas e nomes de pecas. Confirmar pelo codigo que le cada tabela.
- `0x08000000` (o `b crt0`) nao esta desmontado; pode virar `start` (`func_arm`).

## Objetivo de medio prazo

Form-change (Medachange) no modelo "forma = outro bot": trocar os 4 IDs de peca do Medabot em batalha pelos de um bot-forma, somar armadura ao entrar e dividir ao sair, gatilho via Medaforce. Depende das tarefas 2, 3 e 4.

## Diario

### 2026-10-10 - sessao local
- GhidraMCP 1.4 instalado (extensao ajustada para o Ghidra 12.1.4; release original e para 11.3.2). Falta o
  usuario ativar o plugin (File > Configure > Developer > GhidraMCPPlugin) e reiniciar o Claude Code.
- Conferido no codigo: `GetPartName` (0x08047BEC) le 0x083BBB4C com (bot*4 + tipo)*4 -> `gPartNames` correto;
  o `tbl_SpritePointers` do script antigo estava errado (pendencia resolvida).
- CSV: +13 simbolos ja confirmados localmente (texto do dialogo, retratos, fontes, tabelas de opcode de evento).
- Docs sincronizados em `re/reference/`.
- Toolkit da expansao publicado nesta branch (commit unico, autor jvsdm).
- GhidraMCP testado: o plugin responde em http://127.0.0.1:8080/ com o programa Medabots aberto.
- Para a nuvem: tarefas 1-5 deste arquivo continuam valendo. Sugestao de proxima analise pesada: tarefa 2
  (dispatch das 60 acoes) e o formato da tabela 0x083D1E40 (animacao de batalha por molde, base para um editor
  de animacao).

### 2026-10-10 - sessao local (2)
- Analisados os dois `.gzf`: o nosso so tinha o CSV; o do Simon e Rokusho (E) e bem mais completo (ver secao acima).
- Portados 866 simbolos do Simon para o CSV + 5 provisorios trocados; 41 fora (`simon_skipped.csv`); tipos em `re/types/`.
- Conflitos para conferir: 0x020260D0 (nosso gBattlePartState x Simon gSelectedMedabotPreviewTemplateTable) e
  0x0804D83C (achar evento N x EventScript_ReadNextByte).
- Achado: menu de debug embutido (`debug_menu_subroutines` 0x083ACA5C, 59 rotinas). Bom candidato de analise para a nuvem.
- Para a nuvem: tarefas 2-5 agora sao de conferencia; prioridade nova = formato das animacoes (0x083D1E40) e o menu de debug.

### 2026-10-10 - sessao local (3): injecao de C funcionando
- Pipeline em `inject/` (ver `inject/README.md`): Arm GNU Toolchain 14.3, simbolos gerados do CSV, structs geradas
  dos tipos do Simon (todos os offsets conferidos por _Static_assert), build que grava codigo + hooks na ROM.
- Piloto: `GetPartStats` reescrita em C e desviada; 182 chamadas no jogo a partir de um save na tela de pecas, tela
  igual a original. Funciona na ROM original e na expandida.
- Teste no emulador: mgba-mcp (mGBA 0.11 noturno) com `MGBA_EXTRA_ARGS="-C fpsTarget=6000 -C audioSync=0"`
  (30 mil quadros em ~40 s). Nao e deterministico entre execucoes (IWRAM 0x03001408-0x0300167F varia).
- Para a nuvem: ao propor codigo novo, escrever em `inject/src/` + `patches.json` e deixar o teste para a sessao
  local. Proximo alvo sugerido: acao de batalha nova via `gTechniqueActionValidationHandlerTable`/`Setup...` -
  precisa entender a assinatura dos handlers (decompilar 2-3 handlers e documentar aqui).

### 2026-10-10 - sessao local (4): menu de debug liberado
- Cena 63 de `menu_list` = menu de debug (11 opcoes; nomes em `debug_menu_text`). `inject/src/debug_menu.c` abre
  com L+R+SELECT no mapa trocando `gMainStateTable[1]` e reinicia ao sair trocando `gMainStateTable[0]`. Testado no
  mGBA (abre, troca opcoes, Motion mostra dois Medabots, B reinicia, jogo normal sem a combinacao).
- Simbolos novos no CSV: DebugMenu_Dispatch/Init/Exit/RunOption, gDebugReturnScene/Step, gDebugMenuOption.
- Proximo: usar a opcao Motion para decodificar os roteiros de movimento (`gMoveScriptTable` 0x0807E8EC, tocados por
  0x08047428) e a arvore de moldes (registro de 0x18 B: +0x0F/+0x10 filhos, +0x12/+0x13 tamanho, +0x15 forma do
  sprite; cada pedaco = 1 sprite de hardware, max 64x64).

### 2026-10-10 - sessao local (5): animacoes e moldes 50-63
- Decodificados o esqueleto (arvore de moldes) e os roteiros de movimento: ver `re/reference/` secao 15.
- Moldes 50-63 funcionam com 3 mudancas so de dados (tabela de moldes 64, mapa de espelho 0x08081490 com 64,
  roteiros de movimento com 63 linhas). `re/tools/mold_lab.py` gera a ROM de teste; conferido no visualizador
  Motion do debug: identico ao original nos dois lados. A explicacao antiga ("vetor de 50 invade o inimigo")
  estava errada: o vetor tem 65 posicoes por lado.
- Proximo (local): levar isso ao Toolkit (expansao de moldes + visualizador/editor de arvore e animacao), depois
  bots com mais pedacos e o Medachange.
- Para a nuvem (sem ROM, barato): nada urgente. Se acionada: revisar `re/tools/mold_lab.py` e propor o desenho
  da classe C# `MoldExpander` no estilo de `src/Medabots.Rom/Maps/MapExpander.cs`.

### 2026-10-10 - sessao local (6): bots do Navi
- Linhas do Navi: pernas tem 32 B (nao 64). `NaviImporter` (C#) adiciona bots do Navi com a geometria do Navi;
  lote de 21 bots testado no visualizador Motion, 19 iguais ao Medapedia (ver `re/reference/` 15.4).
- Pendentes locais: perna sem paleta propria (Hophop) e bracos 64x64 + pernas em pedacos no lado direito.
- Para a nuvem (sem ROM, barato): nada. As formas de Medachange do Navi ainda nao foram achadas; e busca na ROM do
  Navi (local).


### 2026-10-10 - sessao local (7): montagem fiel dos bots do Navi
- Bracos/pernas saiam trocados de lado: o Navi guarda o lado do membro na junta, o Core no 1o pedaco. Agora o
  `NaviImporter` grava uma 2a tabela de moldes (marcador `MBXNAVIT`) com os registros do Navi e o offset da junta
  somado ao 1o pedaco de cada linha; entradas do Navi levam bit 16. `inject/src/navi_molds.c` (hooks em
  0x08045E6C, 0x080465B8, 0x08046034) escolhe a tabela por peca; `build.py` liga sozinho ao achar o marcador.
- Lado 1: o Navi nao faz hflip em peca com bit 15 (0x0801A7F4); o importador grava a 2a arte ja invertida.
- 21 bots conferidos nos dois lados; bots do Core identicos com e sem hooks. Detalhes: `re/reference/` 15.5.
- Pendente local: Hophop (perna sem paleta propria); Medachange do Navi.
- Para a nuvem (sem ROM, barato): nada.

### 2026-10-10 - sessao local (8): peca quebrada (Tinpet) em bot do Navi
- Causa real dos lados trocados: no Core a linha do braco direito usa moldes 17-19 e a do esquerdo 14-16; o mapa
  mandava o direito do Navi para 14/15. Com isso o braco de Tinpet (14) colidia com o braco do Navi no mesmo molde
  e pegava o registro do Navi. Mapa corrigido por lado (NaviImporter).
- `navi_molds.c`: no lado 1 a tabela segue o molde espelhado; troca de arte entre bracos pulada se os tamanhos
  diferem ou a peca do Navi tem bit 15. Bots do Core identicos; misturas Core/Navi encaixam.
- Pendente local: asa 64x64 do Navi sem arte de lado 1 num bot do Core (lado 1); Hophop; Medachange.
- Para a nuvem (sem ROM, barato): nada.
- (8b) Cintura: o molde 2 do Navi (pai das juntas da cabeca e bracos) tem offset (-4,0), o do Core e zero; o
  importador soma isso as raizes da cabeca e dos bracos (pernas penduram no molde 1).
- (8c) Linha do chao: ajuste por bot (px) nos bits 24-31 da 1a entrada da perna, medido na pose parada contra a
  mediana do Core; `navi_molds.c` desloca o bot na raiz (molde 1). Tela de montagem: B, A, A, A; pecas equipadas
  em 0x030017A3-A6 (IDs de bot); Baixo + Start troca o conjunto.

### 2026-10-11 - sessao local (9): Medachange do Navi
- Formas em 0x4C2BC0 (32 B/bot, apos as pernas; 0x1818 = sem forma), moldes 31-37 na junta das pernas.
  `NaviImporter` importa a forma como bot (Medachange: true); partes vazias recebem um molde vazio (o jogo le a 1a
  entrada de toda parte). 16 formas ok no visualizador Motion.
- Retratos do Navi: 0x62CBCC. Mini sprites dos Medarots do Navi: nao encontrados.
- Para a nuvem (sem ROM, barato): nada.

### 2026-10-11 - nuvem: fonte estendida no Toolkit (nao compilado)

Escrito na nuvem sem .NET (proxy bloqueia o SDK). Tarefa da sessao local: compilar, testar e validar nas 4 ROMs.

O que existe:
- `src/Medabots.Rom/Text/ExtendedFont.cs`: glifos 0x4F o-til, 0x7D-0x7F A-til a-til O-til (4bpp, folha do usuario; o-til alinhado ao o-trema) e larguras 0x4F-0x7F (tabela do Hyd do `insert_text_v2.py`, sem as entradas 0x80-0x81).
- Fonte localizada por assinatura, nao por endereco fixo: larguras 0x00-0x4E (158 B) + glifos A-C (192 B). `FontLayout(Font, Widths)`; italico = Font + 0x5D40, larguras italicas = Widths + 0x200.
  - Metabee (E): fonte 0x4BFC6C, larguras 0x3B4E08 (confirmado).
  - Rokusho (E): fonte 0x4BFC90, larguras 0x3B4E28 (confirmado no banco do Simon; fonte e larguras identicas byte a byte).
  - Metabee (U) / Rokusho (U): nao verificado.
- `CanApply`: fonte achada e slots em branco (0x11) ou ja estendidos. Senao nao aplica.
- `FontProjectEditSystem` no applicator; `RomHackProject.UseExtendedFont` (padrao true, salvo no projeto; projetos antigos carregam true).
- `MedabotsTextCodec`: tabela 0x00-0x7F completa (acentos e glifos novos).
- WPF: aba `Font` (checkbox "Use extended font", botao `?` com `ExtendedFont.Summary`, status com offsets achados, folha 16x8). Preview de mensagem usa o layout localizado por ROM (antes era fixo na Metabee E) e os glifos/larguras estendidos quando ligado.

Checklist:
1. `dotnet build` da solucao; corrigir erros de compilacao.
2. `dotnet test`; os testes novos sao `ExtendedFontTests` e `MedabotsTextCodecTests.EncodeAndDecode_RoundTripsExtendedGlyphs`. Rodar a suite inteira: `UseExtendedFont` padrao true acrescenta um change de fonte em todo projeto; ver se algum teste compara a ROM inteira e quebra.
3. Abrir cada ROM no WPF (Metabee E/U, Rokusho E/U): aba Font mostra os offsets; folha da fonte correta; preview de mensagem com texto normal e `<I>` italico.
4. Nas (U): se `Locate` nao achar, comparar a fonte/larguras da (U) com a (E) e ajustar a assinatura; se os slots 0x4F/0x7D-0x7F nao estiverem em branco, decidir com o usuario. Acrescentar os offsets em `Locate_FindsFontInLocalRoms` e no comentario de `ExtendedFont`.
5. Exportar ROM com o toggle ligado e desligado; no emulador conferir um texto com "ão ões Ã Õ" e acentos (largura certa, sem sobreposicao). Com o toggle desligado a fonte/larguras devem ficar iguais a ROM original.
6. Outros enderecos fixos da Metabee E no Toolkit (ex.: `ExpansionPatchSites.ItalicFont` usado em `RepairItalicFont`): listar e avisar o usuario; nao faz parte desta tarefa.
7. Commit e push na `claude/medabots-project-ajyaeb`; registrar resultado neste diario.

### 2026-10-11 - sessao local (10): fonte estendida compilada e testada
- `dotnet build` ok (0 erros). `dotnet test`: 248 ok, 3 falhas antigas (LargePartDisplay, ja falhavam antes). Os
  testes novos passam; nenhum teste de ROM inteira quebrou com `UseExtendedFont` ligado por padrao.
- Locate/CanApply nas ROMs locais (nao temos a Metabee (U)):
  - Metabee (E): fonte 0x4BFC6C, larguras 0x3B4E08, CanApply true.
  - Rokusho (E): 0x4BFC90 / 0x3B4E28, true.
  - Rokusho (U): 0x4C01D0 / 0x3B4C88, **false**. A fonte da (U) so tem 0x00-0x4E (sem acentos); de 0x50 em diante
    e outro dado, entao 0x7D-0x7F estao ocupados. O italico da (U) tambem nao fica a +0x5D40. Nao mexido: decidir
    com o usuario. Teste `Locate_FindsFontInLocalRoms` ganhou o caso (U) com CanApply false.
- Exportacao (API do applicator): desligado = ROM identica byte a byte (Metabee E e Rokusho E); ligado = so a
  tabela de larguras 0x4F-0x7F e os 4 glifos mudam. Na (U) nada e aplicado.
- Emulador (Metabee E, descricao "No Item" do menu de itens trocada por "ãoõÃÕéá"): os glifos aparecem. õ, Ã e
  Õ encostavam no proximo caractere; larguras corrigidas para 6/7/7 (largura desenhada + 1, como A e O).
  0x64, 0x65, 0x7B e 0x7C (tabela do Hyd) tem o desenho mais largo que o avanco: nao mexido, so anotado.
- WPF (aba Font, folha, preview <I>): nao conferido nesta sessao (precisa de interacao na interface).
- Enderecos fixos da Metabee (E) fora da deteccao por assinatura: `ExpansionPatchSites.ItalicFont` 0x4C59AC
  (+ demais sitios de ExpansionPatchSites, usados em RomExpander.RepairItalicFont); `RomExpander` PartStats
  0x3B83FC, BotNames 0x3BA658, PartNames 0x3BBB4C; `ShingataGraphicsPorter.DescriptorTable` 0x3D1D78;
  `MiniSpritePalette.EuPalettes` 0x50F498; `BotAdder.MoveStyleLiteral` 0x044AF8; `NaviImporter` MirrorMapLiteral
  0x046654 e literais de movimento 0x044AFC/0x044B0C/0x044B10. (As constantes de `MedabotsRomSchema` sao o
  fallback de schema e sao resolvidas por assinatura.)

### 2026-10-11 - nuvem (2): italico, larguras calculadas, export, bloqueio (U) (nao compilado)

Escrito na nuvem sem .NET (proxy bloqueia o SDK). Tarefa da sessao local: compilar, testar e validar.

O que existe:
- Base: sessao local (10). Larguras calculadas dao o-til 6, A-til 7, O-til 7 (iguais a correcao manual da sessao local) e corrigem tambem O-trema/o-trema/AE/ae.
- `src/Medabots.Rom/Text/ExtendedFont.cs`:
  - glifos 0x4F o-til, 0x7D A-til, 0x7E a-til, 0x7F O-til na fonte normal e na italica (italico extraido do print do usuario com a paleta do dialogo).
  - larguras 0x4F-0x7F das duas tabelas calculadas pelos pixels (`MeasureGlyph`), nao mais a tabela do Hyd (estava errada em varios glifos, ex.: O-trema 4 com 6 px de tinta). Regra conferida contra a ROM: normal avanco = ultima coluna com tinta + 2 (74/78 glifos originais), classe vertical 0/1/2/3 por topo/base (78/78); italico avanco = ultima coluna + 1, classe 0. Linhas 12-15 ignoradas (pixels soltos em a-agudo, e-agudo, e-trema, o-trema, u-trema).
  - fonte localizada por assinatura (larguras 0x00-0x4E + glifos A-C). Metabee (E) 0x4BFC6C / 0x3B4E08, Rokusho (E) 0x4BFC90 / 0x3B4E28. Italico = fonte + 0x5D40, larguras italicas = larguras + 0x200.
  - so ROM europeia: `IsEuropean` = byte 0xAF do cabecalho == 'P'. ROM (U) nao aplica.
  - italico so e escrito se os slots italicos estiverem em branco; a normal e obrigatoria.
- `FontProjectEditSystem` no applicator; `RomHackProject.UseExtendedFont` (padrao true, salvo no projeto).
- `MedabotsTextCodec`: tabela 0x00-0x7F completa.
- WPF aba `Font`:
  - status com offsets; ROM (U) -> painel bloqueado com aviso "Font changes are for European (E) ROMs only".
  - checkbox "Use extended font", botao `?` (`ExtendedFont.Summary`, explica avanco e classe vertical).
  - "Export Original Normal Font..." / "Export Original Italic Font...": PNG indexado 128x128 (16 por linha), indices da paleta preservados, fonte da ROM sem a extensao.
  - folhas normal e italica a 4x com grade nas celulas 8x16 e linha vermelha no avanco de cada glifo.
  - preview de mensagem usa o layout da ROM e, ligado, glifos e larguras estendidos (normal e italico).

Checklist:
1. `dotnet build`; corrigir erros de compilacao.
2. `dotnet test` da suite inteira. Novos: `ExtendedFontTests`, `MedabotsTextCodecTests.EncodeAndDecode_RoundTripsExtendedGlyphs`. `UseExtendedFont` padrao true acrescenta um change em todo projeto: ver se algum teste compara a ROM inteira.
3. WPF (nao conferido na sessao 10): (E) mostra offsets e folhas com linhas vermelhas coerentes; (U) mostra a aba bloqueada; preview com `<I>`.
4. Exportar as duas fontes originais e abrir o PNG: 128x128, indexado, paleta amarela do dialogo.
5. Exportar ROM com toggle ligado/desligado; no emulador conferir texto normal e `<I>` italico com "ao oes A-til O-til" e acentos. Desligado: fonte, italico e larguras iguais a ROM original.
6. A-til italico tem um pixel do acento agudo na linha 1 (desenho do usuario); mostrar ao usuario.
7. Commit e push na `claude/medabots-project-ajyaeb`; registrar resultado aqui.

### 2026-10-11 - sessao local (11): validacao da nuvem (2)
- `dotnet build` ok (0 erros). `dotnet test`: 250 ok, so as 3 falhas antigas de LargePartDisplay.
- WPF, aba Font (aberta por automacao, prints conferidos):
  - (E): status com offsets (fonte 0x4BFC6C, italico 0x4C59AC, larguras 0x3B4E08/0x3B5008); folhas normal e
    italica com grade e linha vermelha coerente; botoes de export ok.
  - (U) (Rokusho USA): painel bloqueado com "Font changes are for European (E) ROMs only".
  - Botao `?` aparecia vazio (Width=22 nao cabia o texto com o padding do tema): trocado por MinWidth 28 +
    Padding; conferido.
  - Preview com `<I>`: o trecho italico sai claro/apagado. E o defeito da fonte italica da ROM EU original (tons
    B-F viraram 0), o mesmo que `RomExpander.RepairItalicFont` corrige. O jogo mostra igual (conferido).
- Export das fontes originais (Metabee E): PNG 128x128 indexado, paleta amarela do dialogo, indices preservados.
  A normal sai sem a extensao. A italica ORIGINAL ja tem desenhos em 0x4F/0x7D/0x7E/0x7F (o-til, A-til, a-til,
  O-til) com largura 0. Por isso o Toolkit nao grava o italico do print (slots nao vazios); com as larguras
  calculadas, os glifos originais ficam visiveis. O pixel "do agudo" no A-til italico e os pontinhos no til do
  a-til sao do desenho original da ROM.
- ROM exportada: desligado = identica a original (fonte, italico e as duas tabelas), na EU e na expandida.
  Ligado: 199 bytes (4 glifos normais + larguras normal/italico).
- Emulador (descricao "No Item" do menu de itens):
  - Normal ligado: "ãoõÃÕéá" certo e espacado.
  - Italico: na EU original sai apagado ate com letras comuns (fonte italica estragada). Na expandida (italico
    reparado) "<I>NoItem" sai certo, e "<I>ãõÃÕéá" aparece com os glifos originais da ROM (larguras novas).
- Sugestao: aplicar o reparo do italico (`RepairItalicFont`) tambem pela aba Font em ROM EU simples; sem ele o
  italico continua quebrado. Nao feito.

### 2026-10-11 - sessao local (12): reparo do italico pela aba Font
- `Text/ItalicFontRepair.cs`: o reparo do italico (antes so em `RomExpander.RepairItalicFont`) virou rotina
  compartilhada; o RomExpander usa ela (teste da ROM expandida byte a byte continua passando).
- `FontProjectEditSystem`: com "Use extended font" ligado numa ROM (E), aplica antes o reparo do italico (na
  posicao localizada, nao fixa) e depois a extensao. Desligado: ROM identica.
- `ExtendedFont.Preview(rom, layout)`: copia da ROM como exportada. O preview de mensagem e as folhas do WPF leem
  glifos e larguras dela. Antes o preview desenhava os glifos italicos do print, que a exportacao nao grava na
  (E) (slots italicos ja ocupados), e media larguras sobre eles.
- Testes: `ItalicFontRepair_RebuildsLostDarkPixels`, `Project_RepairsEuropeanItalicFont`. Suite: 252 ok + 3
  falhas antigas (LargePartDisplay).
- Emulador, Metabee (E) simples com o projeto aplicado: "<I>NoItem" e "<I>ãõÃÕéá" escuros e legiveis; texto
  normal igual ao anterior. WPF: preview italico escuro, igual ao jogo.

### 2026-10-11 - sessao local (13): pendencias registradas (fonte e custom)
- Fonte: o Toolkit acha a normal/italica 8 bytes adiante do jogo. Codigo (0x08040132: glifo<<6 + ponteiro):
  normal 0x084BFC64, italica 0x084C59A4 (literais 0x40164, 0x40144...); Toolkit usa 0x4BFC6C / 0x4C59AC.
  Corrigir lendo os literais e realinhar os 4 glifos novos. A italica e mal desenhada na propria ROM (confirmado
  pelo usuario no Tile Molester): refazer a mao (folha PNG / base inclinada / Import Italic Font). Paleta do
  dialogo 0x4C7F34. Usuario pediu para deixar para depois.
- Bot customizado (custom_lab, KWG-03BH): pausado; desmonta fora da animacao de espera.
- Docs: `re/reference/` secao 16 (fonte) e 15.7 (custom pausado).
- Para a nuvem (sem ROM, barato): nada.

### 2026-10-11 - sessao local (14): ROM PT-BR de teste (fonte estendida + traducao v1)
- `dotnet build` ok. Fonte: `ExtendedFont.Preview` (mesma saida do Export com "Use extended font") sobre a
  Metabee (E) -> `Medabots_font.gba`: larguras 0x4F/0x7D-0x7F = 06 01 / 07 00 / 06 01 / 07 00, 4 glifos gravados,
  italico reparado (1465 bytes).
- `insert_text_v3.py ... --reflow`: "5595 strings (5072 unicas), 0 aviso(s), texto ate 0x86A346"; sem aviso de
  largura zerada.
- Emulador (mGBA; prints locais em `build/ptbr_smoke/`, fora do git):
  - Inicio (laboratorio, Ikki/assistente do Dr. Aki): a/e/a/u acentuados e cedilha ok, caixas de 2 linhas, sem corte.
  - "Beleza, vamos de Robattle!" ok na tela de inicio do Robattle.
  - Medawatch > Medals > detalhes: "Tiro"/"Golpe" ok.
  - **Bug: rotulos com acento na tela de detalhes da peca.** WeaponTypeNames[34] "Antiaereo" (com e agudo) sai
    "Antia" e o nome da peca ao lado (PSYCHO MISSILE, bytes do nome iguais aos da ROM original) vira "YC". Na ROM
    so com a fonte e na (E) original o mesmo quadro sai certo ("Anti-Air" / "PSYCHO MISSILE"); sem acento
    ("Regenerar", "Impedir") sai certo. Hipotese: esses rotulos usam a fonte de sprites (OBJ), que nao tem os
    acentos; o codigo do acento le tiles fora dela e suja o nome vizinho. Outras strings com acento em tabelas de
    rotulo, a conferir: WeaponTypeNames 6/9/14/15/23/24/34/39 (+3), AttackTypeNames 1, LegTypes 3/6,
    BattlefieldNames 4/9, MedalAbilityNames 2/5/15, MedaforceNames (14), MenuLabels 0/4, MedalNames 3/9/10/18/23.
    Proximo: achar a fonte OBJ desses rotulos e decidir entre glifos acentuados nela ou rotulos sem acento.
  - Fora do dump (continua em ingles): dicas do Medawatch ("Change page with Control Pad...", "Change Medals
    with...", "SELECT WITH CURSOR"), descricoes de peca "(Anti-Air) Damages Air Medabots greatly." e nomes de
    peca/bot (PartNames/BotNames sem traducao no JSON, por decisao do glossario?).
  - Nao testado: loja (preco com libra), Robattle completo (mensagens "<?>"/0x50, nomes de Medaforca, italico
    <I>), descricoes de medalha/ataque longas. A opcao Battle do menu de debug fica em tela preta depois de
    "Okay let's Robattle!" tambem na (E) original (limitacao do atalho, nao da traducao); o unico save com
    progresso e o de `Medabots_expanded` (escola). Precisa de save/estado perto da loja e de um Robattle.
- Para a nuvem: achar a fonte dos rotulos OBJ (tela de detalhes da peca) e o caminho de desenho deles (estatico).

### 2026-10-11 - sessao local (15): fonte PT-BR (100%) no Toolkit
- Base: ROM editada a mao pelo usuario (`Medabots - Metabee (Europe)editada.gba`, fora do git): fonte normal com 43
  glifos retocados e Ã ã Õ õ nos slots de Ä ä Ö ö (0x50/0x51/0x64/0x65); italica toda redesenhada. Ela confirmou
  o endereco do jogo (0x4BFC64 / 0x4C59A4).
- `re/tools/text/ptbr_font_extract.py` -> `src/Medabots.Rom/Text/ptbr_font.bin` (so glifos diferentes da original).
  `Text/PtBrFont` + opcao "PT-BR font (100%)" na aba Font (`RomHackProject.UsePtBrFont`, salvo no projeto):
  grava no endereco do jogo, copia Ã ã Õ õ para 0x7D/0x7E/0x7F/0x4F, larguras 0x4F-0x7F pela regra com minimo da
  letra base. `FontLayout.GameFont/GameItalicFont`; folhas, preview e export de PNG leem o endereco do jogo.
- Conferido: original + PT-BR font = editada nos 256 glifos (fora as copias); WPF Export ROM = saida da API;
  emulador (texto normal e italico com ã õ Ã Õ e acentos) sem sobreposicao. Testes: 255 ok + 3 antigas.
- `insert_text_v3.py --reflow` le as larguras da ROM de entrada.
- ROM de teste: `build/Medabots_PT-BR_fonte100.gba` (fonte 100% + traducao v1; 5595 strings, 0 aviso).
- Pendente: extensao antiga ainda grava em +8; bug dos rotulos com acento (sessao 14) continua (fonte OBJ).
- Revisao do usuario no emulador (mesmo dia): maiusculas/numeros 1 px abaixo, ponto alto, 'l' alto, espacos largos
  em e/o/a e no 'í', quebra de linha mantendo a do tradutor. Corrigido: ajustes de glifo no extrator (linha de base
  10, '.'/',' descem, 'l' com haste do 'b', corpo do 'í' na coluna 0), larguras das letras pela tinta >= 8
  (a/e/o = 5, acentuados pelo corpo, í = i) e `medtext.reflow` refazendo toda caixa. Conferido no emulador.
  `build/Medabots_PT-BR_fonte100.gba` regenerada (5595 strings, 0 aviso).
- Revisao 2 do usuario: "oa" grudado e texto "sujo" (sombras claras da borda direita encostando na letra seguinte
  depois das larguras menores; a paleta nao mudou). O extrator limpa no corpo das letras a sombra < 8 a direita da
  ultima coluna forte e o pe do 'a'. 'r' seguido de 'd' continua com vao maior (o 'r' so tem largura no braco;
  avanco 3 encostaria o braco nas letras com haste a esquerda). Conferido no emulador.

### 2026-10-12 - sessao local (16): teste de fonte TrueType (Roboto)
- `re/tools/text/port_ttf.py`: rasteriza uma TTF nas celulas 8x16 4bpp (linha de base 10, antialias -> tons 1-F) e
  grava as larguras. Roboto 10 px peso 500 (normal + italica) ficou a mais proxima da fonte do jogo; no emulador,
  texto normal e italico com acentos ok. ROM de teste local `build/Medabots_PT-BR_roboto_teste.gba`.
- **Limite do jogo: avanco maximo 8 px.** Com M/m/W em 9, a letra seguinte some em certas posicoes ("Tem um" saiu
  "Tem  m"; com o espaco em 4, sumiu o 'e' de "Tem"). Com tudo <= 8, certo. A fonte original nao passa de 8.
- Revisao com o usuario (comparando com fonts.google.com/specimen/Roboto): avancos agora vem das larguras da fonte
  (+0,3 px antes de arredondar; espaco 3) em vez da tinta rasterizada (o acento do 'í' e o antialias do 's' abriam
  buracos); '·' (reticencias) desenhado como '.' na linha de base. ROMs locais `Medabots_PT-BR_roboto400.gba` e
  `..._roboto500.gba`.
- Comparacao com o Google Fonts (Roboto 20 px, 7 frases): com +0,3 px as linhas ficavam ~10% mais largas (+15 a
  +23 px). Arredondamento simples com espaco 3 da a menor diferenca (media +1,2 px por linha); adotado.
- Toolkit: "Import Font (TTF/OTF)..." na aba Font (`Text/ImportedFont` + `WPFEditor/TrueTypeRasterizer`). Tamanho
  pela altura do H (7 px, linhas 4-10), contornos do `GlyphTypeface` desenhados 8x e somados em cobertura, tons
  da paleta do dialogo, avancos da propria fonte arredondados pelo menor erro em frases PT-BR (max 8 px),
  '·' como '.', caractere que falta na fonte (Roboto nao tem ♥) fica com o glifo e a largura do jogo. Salvo no
  projeto (`ImportedFont`, `UseImportedFont`; vence PT-BR e estendida). Testado: import pela interface,
  emulador com Roboto 400 (variavel lida na instancia padrao). Testes: 260 ok + 3 antigas.
- Local (2026-10-12): pasta do projeto organizada. ROMs geradas em `build/{expansao,traducao,debug}`, texto da
  traducao em `traducao/` (fora do git), arte de fa em `art/custom/`; nota central do Obsidian `_Medabots.md`.
  Docs: secao 16.2 (import de fonte). Para a nuvem: nada.
- Local: apagados o fork antigo `toolkit/`, `dist/` e `data/captures/`; este clone e o unico Toolkit local
  (os geradores `tools/export_manifest.py`, `export_patch_sites.py` e `gen_signatures.py` escrevem aqui).

### 2026-10-12 - sessao local (batalha no PC)
- Projeto PC (LOVE) no repo principal `pc/`: mapa, dialogo PT-BR, batalha 1x1. Achados confirmados com replay
  deterministico das gravacoes do usuario (`tools/battle_record.lua` + `tools/replay_record.py --break/--watch`):
  - Bloco do bot em batalha: IWRAM 0x030005D0 (jogador) / 0x030008D0 (inimigo), 0x100 B; pecas a partir de +0x0C,
    0x18 B cada: +0 armadura max, +0xD atual. Medaforce candidato 0x03000951 (0-80).
  - Dano 0x08033D30 devolve ctx+0xEC; queda de HP = min(dano, HP da peca), sem transbordar.
  - ROS real 70-160 (nivel de skill ~10), esquiva 18-42, defesa 19-26 (so desvia > 31), critico zera esquiva.
  - Tabela de batalhas EU 0x3C1B80 (bot da batalha 1 da Erika em 0x3BF53C); injetar Rokusho funcionou.
  - Fundos da batalha: ver tarefa 9. Decoder Malias 0x080009BC (r0 = src ROM, r1 = dst).
- Para a nuvem: tarefas 6-10 (imagens, leitura do Toolkit C#); prioridade 6 e 7.

### 2026-10-12 - sessao nuvem: tarefa 6 (montagem do bot grande), estatica, nao testada
- Li `ImageAssetRepository.LargePartDisplay.cs` (caminho `Composite*`/`ReadMedabotLargeDisplayFrame`), `LargePartDisplayAsset`,
  `ImageAssetPatcher.LargePartDisplay`, `RomLayout.Tables` e `NaviImporter.RestPose`. Entregues:
  `re/reference/large_part_layout.md` (tabelas, formato da entrada e do registro de molde, algoritmo, pseudocodigo) e
  `re/tools/large_part_layout.py` (implementa o algoritmo sobre a ROM; so compilado, nunca executado).
- Achados (todos "provavel" ate conferir): posicao = ancora acumulada (+0x0C em y, +0x0D em x, negado no lado 1) - pivo
  (+4 = rawY, +8 = rawX); filho herda a ancora ja deslocada, irmao a do pai; ancora inicial (0x3C,0x32), chave 0x1C2;
  lado 1 usa registro de geometria do molde espelhado (14-16 <-> 17-19) e `x = ancora - (largura - rawX)`; juntas 3-6 sao
  4 registros de 0x18 B em `0x0807EE10` (antes de `gPreviewHead`; novo no CSV `gPreviewJointTemplates`), com `byte 0x10` = molde
  raiz da peca e `0x0D` -> `-2-0x0D` no lado 1. O Toolkit le **0x12 = altura, 0x13 = largura**; EXPANSAO 15.1 diz o contrario.
  O `descriptor_table 0x083D1D98` do Simon e a nossa `0x083D1D78` (Rokusho = Metabee + 0x20).
- Local precisa conferir (detalhe em `large_part_layout.md` secao 7): (1) rodar o script com a ROM no Metabee, lados 0 e 1, e
  comparar com as offsets do HANDOFF (cabeca m09 (-12,-49)/m10 (-15,-75), braco esq m14 (-3,-63)/m15 (0,-42), braco dir m17
  (-36,-62)/m18 (-36,-39)); (2) `--dump-molds` dos moldes 1-6 (quem liga as juntas 3-6) e `--histogram` (moldes por peca);
  (3) no Ghidra, `RenderCompositePreviewDescriptorRecursive` 0x080465B8: ordem altura/largura e sinais; (4) confirmar `0x0807EE10`.
  Se bater, tirar o "provavel" do CSV e marcar a tarefa 6 como feita; se nao, os suspeitos estao em 5 do .md.
- Proxima da nuvem: tarefa 7 (formato da tabela de animacao 0x083D1E40), mas ja ha `re/reference` 15.2; falta so a ROM.

### 2026-10-12 - sessao nuvem: tarefa 7 (animacao de batalha por molde), estatica
- `re/reference/battle_anim.md`: cadeia tipo (0-46) -> `gMoveScriptTable[tipo*7 + gMoveStyle]` -> roteiro (0-58)
  -> duracoes/angulos/deslocamentos; tocador e interpolador decompilados (duracao = quadros ate o quadro-chave,
  interpolacao linear, angulo pelo menor caminho, 63 moldes); tabela dos 47 tipos e perfil dos 59 roteiros
  (quadros-chave, total de quadros, moldes que mexem). Tipo 41 = entrada (roteiro 35); tipo 44 = cenas fora da batalha.
- CSV: `gObjectSlots` 0x03002C30 (+0x2C = tipo de movimento do slot), `FightPreview_ApplyObjectMotionKind`
  0x0803BBE0, `ObjectSlot_TickRepeat` 0x0803CB54, `DebugMotion_PlayKind` 0x0800D90C, `gDebugMotionKinds` 0x03000475.
- Local: no visualizador Motion do debug, direita/esquerda trocam o tipo (0x03000475); rotular os 47 tipos
  (secao 6 do .md). Em batalha, watchpoint em `fight_information+0x112` e `gObjectSlots[i]+0x2C` para ligar
  acao -> tipo de movimento. Proxima da nuvem: tarefa 8 (mini sprite) ou 9 (HUD).

### 2026-10-12 - sessao nuvem: acento nos rotulos (tela de detalhes da peca)
- Causa: rotulos (nomes de medalha, arma, perna, item, Medaforca...) sao sprites montados por
  `BuildObjectTextFromGlyphStream` 0x08040DB8: cada letra vira um slot com quadro `char-1` do set 0 de
  `gSpriteSetTable0` 0x083ABFB0 -> `gObjTextGlyphFrames` 0x08075FF8, que so tem **78 quadros (0x01-0x4E)**. Os
  quadros apontam tiles OBJ 0x3A0+char-1, carregados da fonte 8x8 `gObjTextFontGfx` 0x084C8034 (comprimida,
  0x9C0 B = 78 tiles) em 0x06017400 por ~20 telas. Letra >= 0x4F le ponteiros depois da tabela (lixo; quadros com
  contagem errada podem gastar os 128 OAM e sumir com o texto vizinho, o "YC").
- Correcao aplicada: rotulos sem acento. `medtext.LABEL_TABLES` + `fold_accents`; `insert_text_v3.py` tira o acento
  dessas tabelas sozinho (strings sem tags); `game_text_ptbr.json` regenerado (nao versionado).
- Para ter acento nos rotulos (depois): (1) desenhar os glifos 8x8 acentuados e recomprimir a fonte com mais tiles;
  (2) tabela de quadros nova com 128 entradas (registros de 12 B como os de 0x08075C50, tile = 0x3A0+char-1) e
  repontar `gSpriteSetTable0[0]`; (3) a fonte passa de 0x3ED: tiles 0x3EE-0x3FF (18) so se nenhuma das ~20 telas
  usar; conferir no emulador (VRAM OBJ) cada tela que carrega 0x06017400. 18 tiles: á â ã é ê í ó ô õ ú ç (11)
  + Á É Ó Ú Ç Ã Õ (7); à fica sem acento.
