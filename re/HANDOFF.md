# Handoff - engenharia reversa Medabots Metabee (E)

Instrucoes para o Claude Code local, com o Ghidra aberto e o GhidraMCP conectado.

## Contexto

- Jogo: Medabots: Metabee (Europe), GBA, `MEDABOTSMTBVA8BPE9`. Mesma engine de Rokusho e de Shingata Medarot.
- Objetivo: mapear a engine para hacks no estilo CFRU (C injetado + hooks) e alimentar o Toolkit (schema de tabelas no estilo HexManiacAdvance).
- Documento tecnico de referencia: `EXPANSAO_MEDABOTS.md` (expansao de 181 bots, enderecos em offset de arquivo da Metabee E). Pedir ao usuario se nao estiver no repo.
- Fonte da verdade dos nomes: `re/symbols/mtbe.csv`. O banco do Ghidra e descartavel.

## Estado do projeto Ghidra

- Import: Raw Binary, `ARM:LE:32:v4t`, base `0x08000000`.
- `re/ghidra/MedabotsSetup.java` ja rodou: blocos EWRAM/IWRAM/IO/PAL/VRAM/OAM, ROM dividida em `ROM` (0x08000000-0x0807FFFF, exec) e `ROM_DATA` (0x08080000+, sem exec), 148 simbolos aplicados.
- Auto-analise rodou com Aggressive Instruction Finder (2) e Non-Returning Functions - Discovered desligados.
- Validado: `GetPartStats` (0x08042B58) decompila para `&gPartStats + bot*0x40 + kind*0x10`, copia 16 B.

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
