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

## Tarefas, em ordem

### 1. Identificar os estados principais
- Decompilar `MainState0..3` (0x08002534, 0x0804763C, 0x0805BBE0, 0x08068440).
- Classificar cada um (intro/logos, titulo, campo, batalha, menu) pelas funcoes chamadas, textos e tabelas usadas.
- Renomear para `MainState_<Nome>`. Aceite: cada estado com nome e uma frase de justificativa no comentario.

### 2. Engine de acoes de batalha
- Achar a tabela de dispatch das 60 acoes (indice = byte 1 do registro de peca, "technique"; nomes em `gTechniqueNames` 0x083BE868).
- Comecar pelas xrefs de `GetPartStats` dentro do estado de batalha.
- Nomear a tabela (`gBattleActionTable`) e os handlers conhecidos (`BattleAction_Sword`, ..., `BattleAction_Transform`). A ordem das acoes segue a lista da Medapedia (1 Sword ... 60 Reinforce); confirmar pelo indice.
- Aceite: tabela com N ponteiros no CSV e pelo menos os handlers de Transform (51-55) nomeados.

### 3. Struct do Medabot em batalha
- A partir do handler de Transform (troca a peca em batalha), achar a struct em RAM: IDs das 4 pecas, armadura atual/maxima por peca, medalha, Medaforce, status.
- Cruzar com `gBattlePartState` (0x020260D0, 50 x 16 B por molde) e `gBattleRootX/Y`.
- Criar o tipo no Data Type Manager (`BattleMedabot`) e aplicar.
- Aceite: struct com offsets documentados em comentario e listada no CSV como label do array.

### 4. Medaforce
- Achar a tabela de Medaforces (3 por medalha: Lv10/30/60) e o handler de execucao.
- Referencia: lista de Medaforces por medalha na pagina "Medals in Medarot 2 Core" da Medapedia.
- Aceite: `gMedaforceTable` e handlers nomeados.

### 5. Medalhas
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
