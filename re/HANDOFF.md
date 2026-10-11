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
