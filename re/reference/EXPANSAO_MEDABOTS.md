# Expansão de Medabots e peças — Medabots: Metabee (Europe), GBA

Documentação técnica do projeto de expansão da lista de Medabots/peças da versão europeia de
**Medabots: Metabee** (Medarot 2 Core), com porte dos 60 Medarots do **Shingata Medarot** (mesma engine).

> Estado em 2026-10: ROM com **181 bots** (120 originais + Sägehorn + 60 do Shingata), testada em
> mGBA 0.10.5: loja, inventário, save/load, equipar, batalha (incluindo peças destruídas → Tinpet),
> páginas da lista de peças, imagens grandes, animações e deslocamento de batalha, cores dos mini
> sprites. Tinpet com ID fixo (236/237). Histórico na [seção 12](#12-histórico-de-mudanças-changelog).

Todos os endereços são **offsets na ROM da Metabee (E)** (`MEDABOTSMTBVA8BPE9`), salvo indicação.
Endereços com `0x08…` são ponteiros GBA; `0x02…` EWRAM; `0x03…` IWRAM.

---

## 1. Visão geral da solução

| Problema | Solução |
|---|---|
| Tabelas de bots/peças têm tamanho fixo (120) e não há espaço para crescer no lugar | ROM expandida para 16 MB; tabelas reescritas em `0xC00000` e **todos os ponteiros do código reapontados** |
| Constantes 119/120/121 espalhadas pelo código | 66 constantes trocadas por `N-1` / `N` / `N+1`, classificadas com o Shingata como referência |
| Tinpet usa IDs 120/121 (`gênero + 120`) | Tinpet **fixo em 236/237**, independente de `N` (saves não quebram ao acrescentar bots) |
| Inventário de 960 B na IWRAM, sem espaço | Movido para EWRAM `0x0202E000`, 240 vagas por tipo; save/load passam a gravar byte a byte |
| Lista de peças com 30 páginas fixas | Volta de página ajustada para `ceil(N/4)` |
| Seletor de imagem das peças com 7 bits (128 por molde, moldes de braço cheios) | Patch de 2 instruções: seletor de 8 bits (256 por molde) |
| Endereços diferentes entre versões (EU/US, Metabee/Rokusho) | Tabelas localizadas pelo próprio código (literal pools das funções de acesso) |

Nada disso exige reescrever rotinas: são realocações de dados, troca de imediatos e um patch de
duas instruções. O código do jogo continua o original.

---

## 2. Tabelas por bot / peça

### 2.1 Funções de acesso (pontos de entrada)

| Função | O que faz |
|---|---|
| `0x042B58` `GetPartStats(bot, tipo, destino)` | copia 16 B de `PART_STATS + bot*64 + tipo*16` |
| `0x047BDC` `GetBotName(bot)` | `BOT_NAMES[bot]` |
| `0x047BEC` `GetPartName(tipo, bot)` | `PART_NAMES[bot*4 + tipo]` |

Peça = `bot*4 + tipo` (0 cabeça, 1 braço direito, 2 braço esquerdo, 3 pernas).
IDs de peça em batalhas, lojas e Medabots montados são **1 byte** (= ID do bot).

### 2.2 Tabelas (Metabee E)

| Tabela | Offset | Entradas | Bytes/entrada | Refs no código |
|---|---|---|---|---|
| Stats das peças | `0x3B83FC` | 480 | 16 | 2 |
| Nomes dos bots | `0x3BA658` | 120 | 4 (ponteiro) | 2 |
| Nomes das peças | `0x3BBB4C` | 480 | 4 (ponteiro) | 16 |
| Sprites de batalha (Malias) | `0x3B1A08` | **122** (+Tinpet M/F) | 24 (6 ponteiros) | 5 |
| Família de paleta (= banco OBJ do mini sprite) | `0x3BEB94` | 122 | 1 | 3 |
| Estilo de deslocamento em batalha (0–6) | `0x3CAD78` | 122 | 1 | 1 |
| Preview cabeça | `0x07EE70` | 122 | 16 | 5 |
| Preview braço direito | `0x07F610` | 122 | 16 | 8 |
| Preview braço esquerdo | `0x07FDB0` | 122 | 16 | 8 |
| Preview pernas | `0x080550` | 122 | **32** (`bot << 5`) | 6 |

As tabelas gráficas têm **122** entradas: 120 bots + Tinpet macho (120) e fêmea (121).

### 2.3 Registro de peça (16 bytes)

| Byte | Cabeça/braços | Pernas |
|---|---|---|
| 0 | medalha compatível | medalha compatível |
| 1 | técnica (índice na tabela de técnicas, `0x3BE868`, 60 nomes) | tipo de perna (100–106) |
| 2 | especialidade (Strike, Berserk, Shoot, Aim, Defend, Heal, Support, Interrupt) | — |
| 3 | fêmea (0/1) | fêmea |
| 4 | armadura | armadura |
| 5 | acerto | propulsão |
| 6 | poder | evasão |
| 7 | reação em cadeia | defesa |
| 8 | usos / carga | proximidade |
| 9 | radiação | distância |
| 10 | "rank" do bot (igual nas 4 peças) | idem |
| 11 | **preço** na loja, em libras inteiras (54 → £54.00; 0 não compra) | idem |
| 12 | flag (1 normal, 32 em alguns chefes) | idem |

### 2.4 Loja

`0x4838E4`: 15 lojas × 4 slots, cada slot é um **ID de bot** (`0xFF` = vazio). Localizável pela
assinatura `13 00 FF FF 13 00 42 FF 13 00` (usada pelo Randomizer).

---

## 3. Constantes que dependem da quantidade de bots

Regra da engine (confirmada no Shingata, mesma engine com 60 bots): com `N` bots,
último ID real = `N-1`, contagem = `N`, Tinpet macho = `N`, Tinpet fêmea = `N+1`.
Na expansão o Tinpet fica fixo em `T = 236` (`patches.TINPET_ID`); como no Shingata contagem e
Tinpet coincidem, cada sítio foi reclassificado lendo o código.

| Grupo | Qtd | Exemplo | Troca |
|---|---|---|---|
| "É peça real?" / laços / volta de cursor (`cmp/mov #0x77`) | 45 | `0x005A70`, `0x043830` | `N-1` |
| "É o Tinpet macho?" (`cmp #0x78`, ciclo de peças `0x436AC`) | 2 | `0x0436D0` | `T` |
| Tinpet (`adds #0x78` = gênero + 120) | 9 | batalha `0x015862`, criação `0x042246` | `T` |
| Rolagem da loja (`adds #0x78` sobre `0x2023000`) | 5 | `0x061D6C` | `N` |
| Tinpet fêmea (`#0x79`) | 2 | `0x043752` | `T+1` |
| Última página da lista (`#0x1D`) | 5 | `0x00F0A8` | `ceil(N/4)-1` |

Lista completa e origem de cada sítio em `tools/patches.py`.

### 3.1 Loja (compra/venda) e bots com ID ≥ 128
- **Contadores de 1 byte com sinal**: laços `0..N-1` da loja fazem `lsls #24 ; asrs #24` no contador e no índice.
  Com mais de 128 bots o laço não termina (trava ao comprar) e o índice vira negativo. 7 `asrs` → `lsrs`
  (`0x062428`, `0x062680`, `0x0626F0`, `0x0623D8`, `0x06240A`, `0x062656`, `0x0626C6`).
- **Vetores de 120 itens**: quantidades em `0x2023000 + i` e itens (8 B: bot, quantidade, preço×100) em
  `0x2040000 + i*8` (espelho de `0x02000000`). A **rolagem** fica logo depois das quantidades
  (`0x2023078` = base + 120); com N itens ela vai para `base + N` — 5 `adds #0x78` e **9 literais**
  (`0x05B3E8`, `0x05B9C0`, `0x05BA14`, `0x061954`, `0x061E84`, `0x0620BC`, `0x0620F4`, `0x0623F4`, `0x062804`).
  O último item (`0x020403B8`, literal em `0x05B178`) vira `base + (N-1)*8`. As áreas que crescem estavam livres.
- **Armadilha**: os bytes `78 30` de um literal `0x02023078` parecem `adds r0, #0x78`; três literais foram
  confundidos com cálculo de Tinpet numa primeira versão (rolagem gravada num lugar e lida em outro).

**Método** (útil para qualquer engine com duas versões de tamanhos diferentes): para cada `#0x77/#0x78`,
mascara-se uma janela de bytes ao redor (imediato, BL, branches e `ldr [pc]`) e procura-se a mesma
janela no Shingata. Se lá o valor for `0x3B/0x3C` (59/60), é limite de contagem. Os que não têm par
foram classificados lendo o contexto. **Cuidado**: `0x1D` também é "última medalha" (30 medalhas) em
~40 lugares, e `0x78` aparece em coordenadas, tamanhos e efeitos sonoros (`0x04C996` = som 120).

---

## 4. Inventário e save

- Original: IWRAM `0x03004C40`, 4 blocos de `0xF0` (120 bots × 2 bytes) por tipo. Endereço = `tipo*240 + bot*2`.
  O 240 é calculado no código como `tipo*15 << 4` em **20 lugares**.
- Byte 0 da entrada = quantidade em estoque (máx. 99); byte 1 = quantidade total/equipada.
- Novo: EWRAM `0x0202E000` (região confirmada livre por varredura de 88 mil frames e sem
  referências no código), `<< 4` → `<< 5` (480 B por tipo = 240 vagas).
- Save/load (`0x5E010` / `0x5DF18`): o original grava cada byte do inventário como **halfword**
  (`0x780` bytes no buffer, offset `+0x7AC`). Trocando para byte a byte, as 240 vagas cabem
  **exatamente no mesmo espaço** do save. Saves feitos com a ROM original não são compatíveis.
- Outros ajustes: limpeza no new game (`0x4A2F0`), contadores (`0x440D4`, `0x5D860`) e a rotina do
  bot inicial (`0x44B58`, reescrita com o mesmo tamanho).
- Save total: `0x1CBC` bytes com checksum em `+0xFF4` (soma de 16 bits dos bytes `0x0000–0x1CBB`, campo zerado);
  EEPROM de 8 KB → ~830 bytes livres no fim. No `.sav` do mGBA cada bloco de 8 bytes fica invertido.

---

## 5. Gráficos

### 5.1 Sprites de batalha e paletas
6 blocos **Malias** (cabeçalho `Le`) por bot: o mini sprite (16×16 por componente, pernas 0x180 B).
Cada componente usa o banco OBJ = "família de paleta" do bot da peça. O jogo EU carrega 4 paletas de
`0x50F498` nos bancos 0–3; a 4ª é zerada. O Shingata usa **paletas próprias** (`0x58E0A0/C0/E0`),
então os tiles portados são remapeados (`tools/mini_palette.py`): paleta extra calculada no banco 3,
banco de menor erro por bot, ajuste +15% brilho / −10% saturação. Na batalha só o banco 3 está livre.

### 5.1.1 Deslocamento em batalha
`MOVE_STYLE` (`0x3CAD78`, 1 byte por bot) escolhe o roteiro de movimento do ataque, indexado pelo
**bot das pernas**: `0x44A3C` lê `0x807E8EC[tipo*7 + estilo]`. Posição da raiz da montagem em batalha:
EWRAM `0x20260E8` (y, ponto fixo 8.8) / `0x20260EA` (x).

### 5.2 Imagem grande das peças (previews)
Cada entrada de preview (4 bytes) = **molde** (bits 0–5) + **seletor** (bits 6–13) + variante
de pose (bit 15) + flag centralizado (bit 14).

- **Molde** (descritor): forma e posição da peça, registros de 0x18 bytes em uma tabela de
  ponteiros `0x3D1D78` com **50 entradas** (logo depois, `0x3D1E40`, começa outra tabela).
  Bytes `0x0F` e `0x10` do registro = moldes-filho. IDs 1–2 = raízes da montagem;
  3–6 = juntas montadas em RAM; 7–49 = moldes reais (todos em uso).
- **Seletor**: escolhe a arte daquele bot na lista de blobs do molde, em pares
  `[imagem LZ77, paleta 32 B]` (paleta == imagem significa "sem paleta").

### 5.3 O limite de 7 bits e o patch
Em `0x45E6C` o jogo calcula `((entrada >> 6) + variante) & 0x7F) << 1`
(`lsls #25 ; lsrs #24` em `0x45ECC`). Os moldes de braço bípede (14, 15, 17, 18) já usam
113–127 das 128 vagas no original. Patch:

```
0x045ECC: 0x0652 -> 0x0612   (lsls r2, r2, #25 -> #24)
0x045ECE: 0x0E12 -> 0x0DD2   (lsrs r2, r2, #24 -> #23)
```

Seletor de 8 bits = 256 vagas por molde. Seguro para os bots originais (nenhum usa o bit 13).

### 5.4 Por que não clonar moldes
Tentativa abandonada: clonar moldes cheios em IDs livres (50–63). O estado de cada peça em
batalha é um vetor **indexado pelo número do molde**, com 50 posições de 16 bytes (EWRAM
`0x20260D0`), seguido do vetor do inimigo. Um molde 60 cai na posição 10 do inimigo: as peças
"voam" e o inimigo anima peças que não são dele. Achado com watchpoint no mGBA em `0x2026491`
(função `0x45E6C`, `r4 = base + molde*16`).

---

## 6. Porte do Shingata Medarot

- Mesma engine: `GetPartStats` é idêntico byte a byte. Tabelas localizadas pelas mesmas funções
  (Kabuto: stats `0x467238`, nomes `0x4682C4`, peças `0x4689BC`, sprites `0x461168`,
  paleta `0x46A72C`, previews `0x091A98…`, moldes `0x47A93C`, estilo de deslocamento `0x4748D4`,
  paletas de mini sprite `0x58E0A0`).
- **Stats** na mesma escala; **técnicas** com os mesmos IDs (conferido pela tabela de nomes);
  **medalhas** 0–26 na mesma ordem (os bots usam 0–24). Nada é convertido.
- **Texto**: charset do Medarot Navi (mdr-navi-i18n) + códigos de dakuten `0x38–0x51`, `ー` = `0xBC`,
  `・` = `0xBD`, aprendidos alinhando a ROM com os nomes da Medapedia (60/60 sem conflito).
  O hiragana do Shingata ainda não está mapeado (3 bots com nomes provisórios).
- **Moldes**: os 50 são idênticos nos dois jogos; a arte do bot portado entra numa vaga livre do
  mesmo molde (128–255 depois do patch).
- Capacidade: os 60 cabem; depois deles sobram ~61 vagas nos moldes de mão (15/18).

---

## 7. Localização de tabelas por código (independente de versão)

As constantes fixas do Medabots-Romhacking-Toolkit valem só para a Rokusho (E); na Metabee (E) há
deslocamentos de −0x20, −0x24 e outros, e na Rokusho (U) chegam a −0x15C4. Endereço fixo não serve.

Método: para cada tabela, pega-se a função que a lê, mascara-se o imediato do `ldr [pc]`, BLs e
branches, e guarda-se o padrão. Na ROM-alvo, acha-se o padrão (único), lê-se o literal e soma-se
um delta. Funciona em todas as versões **e** em ROMs expandidas (o código já aponta para a tabela
nova). 40 tabelas cobertas (34 por assinatura, 6 derivadas de vizinhas). Implementado no fork do
Toolkit (`Layout/RomLayout`, `romFile.Layout.XxxOffset`), assinaturas geradas por `tools/gen_signatures.py`.

---

## 8. Limites atuais

| Limite | Valor | Origem | Como passar |
|---|---|---|---|
| Bots | 236 | Tinpet fixo em 236/237 dentro das 240 vagas do inventário | subir `TINPET_ID` junto com o inventário (seção 9) |
| IDs de bot | ~252 | ID de 1 byte em RAM, save, batalhas, lojas e eventos; `0xFF` = vazio | ver seção 9 |
| Vagas por molde | 256 | seletor de 8 bits (bit 14 = flag) | — |
| Moldes (formas) | 50, todos em uso | vetor de estado de batalha com 50 posições | aumentar o vetor da RAM de batalha |
| Bípedes extras depois do Shingata | ~30–60 | moldes de mão 15/18 | idem |

---

## 9. Ir além de 240 / 256

- **Mudar tabelas de lugar não é problema**: ponteiros são de 32 bits e já fazemos isso.
- **240 → ~252**: o gargalo é o espaço do inventário no save. Há ~830 bytes livres na EEPROM depois
  do bloco de save; bastaria gravar as vagas 240+ (8 bytes por bot) nessa área extra, estendendo
  save/load. Viável.
- **Além de 256**: o ID de 1 byte está em todo lugar (Medabot montado, batalhas, lojas, scripts de
  evento, save). Uma função de acesso "estendida" resolveria as tabelas, mas não os bytes guardados em
  RAM/save/scripts. Seria preciso um esquema de "banco" (ID + página) tocando cada estrutura —
  na prática, território de decompilação/reimplementação.

---

## 10. Ferramentas (`tools/`)

| Arquivo | Função |
|---|---|
| `build.py` | ROM original + `data/bots/*.json` → ROM expandida (opções `--shop`, `--preco-teste`, `--out`) |
| `patches.py` | todas as constantes/patches, com origem documentada |
| `extract.py` | 120 bots originais → `data/bots/NNN_nome.json` |
| `shingata.py`, `port_shingata.py` | extrai e porta os bots do Shingata |
| `gfx_port.py`, `codecs_gba.py` | cópia de gráficos (Malias, LZ77, moldes/seletores) |
| `mini_palette.py` | paleta extra e remapeamento de cores dos mini sprites portados |
| `render_capture.py`, `analyze_captures.py` | reconstrói a camada de sprites das capturas / resumo por captura |
| `gdbrsp.py` | cliente do servidor GDB do mGBA (registradores, memória, watch/break) |
| `export_manifest.py` | gera o manifesto da expansão embutido no Toolkit |
| `save_edit.py` | edita o `.sav` (`--todas-pecas`): desinverte os blocos de 8 B do mGBA, recalcula o checksum, faz backup |
| `xrefs.py`, `crossref_shingata.py`, `bot_tables.py`, `gen_signatures.py` | análise estática |
| `render_bot.py`, `oam_film.py` | conferência visual |
| `ewram_scan.lua`, `inv_dump.lua`, `battle_capture.lua` | scripts mGBA (RAM livre, inventário, captura de animação) |

Formato de bot (`data/bots/NNN_nome.json`): `bot_id`, `name`, `parts` (nome + stats nomeados) e,
para bots novos, `graphics_from` (copiar gráficos de um bot original) ou `graphics_source`
(copiar de outra ROM da engine; opção `native_previews` para usar a imagem grande de um bot nativo).

### Depuração com mGBA
- Scripts Lua 0.10 só leem memória por frame. Para pegar quem escreve um endereço, usar
  **Tools → Open debugger console**: `watch/c <endereço>`, `continue`, `info registers`.
- Saves valem entre builds enquanto os bots só forem acrescentados (Tinpet fixo). Save states
  dependem da ROM: prefira resetar e carregar o save.

---

## 11. Problemas em aberto

- **Lojas**: só a loja 0 é configurada pelo build (`--shop`); falta distribuir os bots novos.
- **Hiragana do Shingata** e nomes oficiais das peças portadas (hoje romanização provisória).
- **Toolkit**: lê e gera a ROM expandida. Falta editar a lista de bots novos pela interface (hoje o
  manifesto é gerado por `tools/export_manifest.py`) e opções de loja/preço no diálogo.

### Funcionalidades pedidas (para avaliar depois)

**Medalhas: editar e, talvez, adicionar** — pedido em 2026-10-06, em espera.
- *Motivo a confirmar:* olhar os jogos seguintes da série (ex.: Medarot 8, ver `ref/Medals in Medarot 8.txt`)
  e ver se existem medalhas que justifiquem aumentar a lista, e não só editar as atuais.
- *O que já se sabe:* 27 medalhas (0–26), registro de 16 B em `0x3B62D8` (Metabee E; o Toolkit já acha
  pelo código) e uma tabela de especialidade por classe. Pistas de limite interno 30 (`#0x1D` =
  "última medalha"; o validador aceita medalha < 30): talvez 3 vagas sem mudar código.
- *Dificuldade:* o Shingata tem as mesmas 27 medalhas, então não serve de "pedra de Roseta" como serviu
  para os bots; as constantes teriam que ser classificadas lendo o código.
- *Primeiros passos, se aprovado:* (1) mapear o registro de 16 B e listar as 27 medalhas; (2) testar as
  vagas 27–29; (3) só então decidir se vale passar de 30 (tabelas, constantes, save).

## 12. Histórico de mudanças (changelog)

Datas de 2026-10. Hashes = commits do repositório de ferramentas.

### Toolkit: retratos novos, mensagens, ferramentas de pintura — fork `e76a8b7`…`c044f5e`
- **Mapas como os bots**: mapas novos ficam no projeto e entram todos no Export; renomear com duplo clique;
  "+" > New map (vazio até 64×64, tileset em branco ou de outro mapa, PNG opcional) e Copy map; map_id
  como lista no editor de eventos; mapa abre centralizado.
- **Ferramentas de pintura** no mapa e nos sprites: lápis, linha, retângulo, retângulo cheio, balde,
  conta-gotas, desfazer/refazer (P, L, R, F, G, I, Ctrl+Z, Ctrl+Y).
- **Visualizador de mensagens**: cada caixa como no jogo (fonte e larguras reais, retrato à esquerda
  espelhado). Ao salvar um texto editado, o editor escreve os `<NL>`/`<NB>` onde o jogo quebraria (§14.1).
- **Retratos novos** (+ verde na aba Sprites): personagem novo como cópia de outro (dados próprios) ou em
  branco, até 256; nomes de personagem no projeto (§14.2). Teste: `build/Medabots_expanded+teste_retrato.gba`.

### Toolkit: bots no editor, dark mode, eventos, mapas novos e tilesets — fork `0066ee5`…`ad417fb`
- **New Medabots** (+ verde na aba Parts): bot criado no editor sobre o corpo de um bot base (nomes e
  stats vêm do base; campos de lista com os nomes do editor de peças: atributo + medalha, técnica,
  especialidade, tipo de perna); cada pedaço (imagens grandes e os 8 quadros do mini sprite) aceita PNG,
  com exportação de modelos. Vários bots formam uma **lista** que pode ser salva e reaberta (JSON + pasta
  `<lista>_art/`) e entra de uma vez numa ROM (`BotAdder.AddAll`).
- **Dark mode** (View > Dark mode, lembrado). Preço da loja/peças: o byte é o preço em unidades
  (11 → 11,00); o "×100" saiu.
- **Eventos — dois bugs corrigidos.** Criar um ID de evento novo movia a tabela sem recalcular os
  ponteiros (relativos à base): os 2288 eventos quebravam. Na expandida de 16 MB o espaço livre ficava
  além dos 4 MB que os bancos alcançam: evento que crescia não era gravado. O jogo (0x0804D83C) acha o
  evento N em `base + banco×0x4000 + (ponteiro − 0x4000)`, lendo os dois deslocamentos do cabeçalho; os
  scripts só se referem por ID e saltos relativos. Agora tabela + scripts mudam juntos de lugar
  (literais da base em 0x4A584/0x4A8C8/0x4B124 reapontados) quando a tabela cresce ou o espaço livre
  está fora do alcance. O espaço livre de todas as edições começa após o último byte usado (≥ 8 MB):
  na expandida fica dentro dos 16 MB.
- **Mapas novos** (+ verde na aba Maps). O ID do mapa é 1 byte (RAM 0x3005FDE) usado direto como índice,
  sem checagem contra 192; um rastreio do ID pelo código achou 16 tabelas por mapa (as 11 que o Toolkit
  conhecia + duas de área 0x3D2B6B/0x483774, encontros extra 0x3BF210 e ponteiros 0x409480/0x40F880),
  cada uma carregada por 1–4 literais e referenciada só por eles. A expansão move as 16 para 255
  entradas e grava o marcador `MBXMAPS` (mapas em uso, capacidade) antes da tabela de camadas; o novo
  mapa é uma cópia de um existente (camadas, tileset, paleta, colisão, música, área), com warps e
  pessoas vazios ou copiados. Teste: `build/Medabots_expanded+mapa_teste.gba` (porta da sala do térreo
  esquerda da escola de Riverview → mapa 192).
- **Tileset e paleta de mapa em PNG** (aba Maps, Layer Editor): exportar folha (16 tiles por linha, cada
  tile no banco de paleta em que é usado, cor 0 transparente) e grade 16×16 de paletas; importar de volta
  para todos os mapas que usam o tileset ou só para o atual (patch de projeto, gravado no Export).

### Shingata opcional e "Add Bot" no Toolkit — fork `fd2aa0a`
- A expandida pode ser gerada **só com a ROM EU** (120 originais + Sägehorn, 121 bots); a ROM Shingata
  passou a ser opcional (`build.py --sem-shingata` gera o mesmo que o Toolkit sem ela).
- **File → Add Bot to ROM...** acrescenta um bot a uma ROM já expandida a partir de um JSON no formato
  de `data/bots` (`name`, `parts` com stats, `graphics_from`, `recolor` opcional; exemplo em
  `examples/bot_exemplo.json`). O bot entra como o próximo ID (linhas já reservadas até 235), as
  imagens recoloridas vão para vagas livres de seletor (listas originais de 128 vagas são ampliadas
  para 256 quando preciso), o mini sprite é mapeado nas 4 paletas que já existem e as constantes de N
  sobem para N+1, conferidas contra uma ROM gerada do zero. Saves da ROM continuam valendo.
- Ainda não: bot com desenhos próprios (`art`) pelo Toolkit (só pelo `build.py`).

### Cores próprias para bots que reaproveitam desenhos (Sägehorn)
- Campo `recolor` no JSON (`"#rrggbb": "#rrggbb"`): as paletas das imagens grandes do bot de origem
  são copiadas com as cores trocadas e ligadas a vagas novas de seletor (mesma imagem); peças sem
  paleta própria usam a da principal. O mini sprite entra no remapeamento com a paleta trocada (sem o
  ajuste HSV do Shingata). Sägehorn: desenhos do Metabee em marrom, azul-marinho e olhos vermelhos.

### Fonte itálica da ROM EU consertada
- Texto em itálico (entre códigos `F8`, ex.: "Click!", mensagem 0:263) vem quebrado **na própria ROM
  europeia**: a fonte itálica (`0x4C59AC`, 128 glifos 8×16, lida pelo código de texto 0x40144–0x40504)
  perdeu os tons escuros B–F (viraram 0 = transparente) e o 8 (virou 5). Os formatos estão intactos.
- O build e o Toolkit reconstroem cada pixel perdido pela quantidade de vizinhos com tinta (4 → F,
  3 → E, menos → D); 92% dos pixels iguais a uma fonte de referência corrigida à mão. Só roda se a
  fonte tiver o defeito; não depende de nenhum arquivo além da ROM EU.

### Toolkit abre a ROM expandida; eventos pelo código — fork `8b5d295`
- A procura de "tabela de eventos relocada" aceitava qualquer `00 40` acima de 0x800000 e achava
  falsos positivos nos dados da expandida (217 de 2288 eventos quebravam → "Open Failed"). Agora a base
  vem do literal que o jogo carrega (0x4A3B6), que o próprio Toolkit reaponta quando relocaliza.
- Os perfis tinham o endereço da Rokusho (E) para todos: Metabee (E) = `0x769EC0`, Rokusho (U) = `0x768CB0`.
- Batalhas com Tinpet (120/121): as 93 ocorrências são vagas de enchimento (inimigo além do número
  da batalha), não carregadas; nenhum inimigo em uso tem peça > 119. Nada a corrigir.

### Toolkit gera a ROM expandida — fork `58ecf6a`
- **File → Generate Expanded ROM...**: pede a ROM Metabee (E) e a Shingata Kabuto (confere os
  headers), salva num arquivo novo (nunca sobrescreve as de origem) e abre o resultado.
- `Medabots.Rom.Expansion` espelha o `tools/build.py`. O manifesto embutido
  (`tools/export_manifest.py`) só tem o que nenhuma das ROMs traz: Sägehorn, a ordem dos bots e os
  nomes da Medapedia; stats e gráficos vêm das ROMs.
- **Saída idêntica byte a byte** à do build em Python (teste automático contra
  `build/reference_expanded.gba`), inclusive a paleta extra calculada por k-means.
- O bot de teste 181 (MULTIKOLOR-C) saiu: a expansão oficial tem 181 bots (0–180).

### Toolkit lê a ROM expandida — fork `eef915f`
- `RomLayout.BotCount` (imediato do validador de Medabot, `0x43820`), `TinpetId` (criação do Medabot
  pelado, `0x42238`) e tabelas de nomes (`GetBotName`/`GetPartName`), todos por assinatura de código,
  conferidos em Rokusho E/U, Metabee E e na expandida.
- Leitor de peças, limites dos sprites de batalha, listas da loja e do bot inicial e nomes que
  faltam no catálogo passam a seguir a ROM. 200/203 testes (as 3 falhas de `LargePartDisplay` são anteriores).

### Validação no jogo (2026-10-06)
- Save com todas as peças (`tools/save_edit.py --todas-pecas`) carregado sem problemas: os 182 bots
  aparecem no inventário.
- Animações e deslocamento de batalha conferidos em vários bots portados além dos usados nos testes A/B.
- Cores dos mini sprites aprovadas (ajuste HSV).
- **Tinpet fixo (236/237) validado**: peças removidas na tela de Medabots viram Tinpet, o ciclo de
  peças passa pelos bots portados sem mostrar vagas vazias, e peça quebrada em batalha vira Tinpet
  sem travar (Predicador, bot 136).

### Total de páginas na lista de peças ("/30")
- Tilemap Malias em `0x4F1514` (BG1, 32 colunas, tile guardado = VRAM − `0x200`). Os 7 bytes do
  "30" (`0x4F1560–63`, `0x4F1580–81`, `0x4F1583`) são literais e exclusivos no stream comprimido; o
  build troca o desenho fixo pela fonte do número da página (VRAM `0x360 + 2·dígito`, paleta 7)
  com `ceil(N/4)`. Achado lendo a VRAM de save states do mGBA (chunk `gbAs`, VRAM em `+0x1000`).

### Tinpet com ID fixo
- Tinpet passa a **236/237 fixos** (`patches.TINPET_ID`), independente de `N`. IDs `N..235` ficam
  vazios (linhas de enchimento nas tabelas; inventário zerado → o ciclo de peças os pula).
- Os sítios `#0x78` foram separados por papel: "é Tinpet?" (`0x0436D0`, `0x04374E`) e
  `gênero + 0x78` → `TINPET_ID`; os 5 `adds #0x78` da loja são o endereço da rolagem → `N`.
- Saves com Tinpet montado sobrevivem a bots novos (desde que os bots sejam só acrescentados).
  Teto: 236 bots (59 páginas cheias).

### Cores dos mini sprites portados — `de1a158`, `c4fb9d2`
- O Shingata usa paletas próprias para os mini sprites (`0x58E0A0/C0/E0`); a comparação antiga com
  o bloco idêntico ao EU (`0x58DF40`) estava errada — esse bloco não é usado.
- O jogo EU carrega 4 paletas de `0x50F498` (bancos OBJ 0–3) e a 4ª é zerada: o build grava ali
  uma paleta extra (k-means pelas cores dos bots portados), escolhe o banco de menor erro por bot e
  remapeia os tiles. Ajuste de estilo: +15% de brilho, −10% de saturação.

### Movimento em batalha dos não bípedes — `073e782`
- Tabela **MOVE_STYLE** (`0x3CAD78`, 1 byte por bot, 0–6) escolhe o roteiro de deslocamento
  (`0x44A3C`: `0x807E8EC[tipo*7 + estilo[bot das pernas]]`). Não era realocada: bots ≥ 120 liam lixo
  (Valentorero = 12) e andavam pelo topo da tela. Agora vai junto com as tabelas gráficas;
  no Shingata fica em `0x4748D4`.

### Loja com IDs ≥ 128 e N bots — `8c57aaa`, `c579ee2`, `b719bd4`
- 7 laços com contador de 1 byte com sinal (`asrs` → `lsrs`) travavam ao comprar peças ≥ 128.
- Variável de rolagem em `0x2023000 + N` (9 literais) e literal do último item; 3 literais tinham
  sido confundidos com `adds #0x78` do Tinpet.

### Porte do Shingata — `aabf38a` … `87637a2`
- 60 bots portados com gráficos (bots 121–180), nomes de peças pela Medapedia.
- Seletor de imagem de peça de 7 → 8 bits (patch em `0x45ECC`, 256 vagas por molde).
- Clones de molde (IDs ≥ 50) abandonados: o estado das peças em batalha é um vetor de 50 posições
  por molde (`0x20260D0`), seguido do vetor do inimigo.

### Base da expansão — `73fd11d`, `8265870`, `52ce839`
- ROM de 16 MB, tabelas em `0xC00000`, 66 constantes por `N`, inventário em EWRAM com 240 vagas,
  save byte a byte, páginas `ceil(N/4)`, Sägehorn (bot 120).
- Toolkit: endereços resolvidos pelo código (assinaturas de literal pool).

### Ferramentas de diagnóstico — `9df86e2`, `bc6866e`, `7b55f93`, `8ce1135`
- `battle_capture.lua` (OAM/IWRAM/EWRAM/VRAM por frame, L+R), `render_capture.py`,
  `analyze_captures.py`, `gdbrsp.py` (servidor GDB do mGBA).
- Bots de experimento A/B (ROKUSHO-C, MULTIKOLOR-C) para isolar causas.

---

## 13. Mapas, eventos e limites

Validado no jogo em 2026-10-07: o mapa 192 (cópia da sala de aula 3, ligado à porta de cima à esquerda do
corredor do térreo da escola de Riverview) carrega completo, com NPCs, e a saída volta ao corredor.

### 13.1 Mapas
- **ID do mapa = 1 byte** na RAM em `0x3005FDE` (base `0x30057E0` + `0x7FE`), usado direto como índice por
  ~60 rotinas, **sem comparação com 192**. Por isso mapas novos só precisam de espaço nas tabelas.
- **16 tabelas por mapa** (EU; offsets do esquema do Toolkit − 0x20), achadas rastreando o ID pelo código
  (`tools/mapflow.py`):

| Tabela | Offset EU | Bytes/mapa | Refs no código |
|---|---|---|---|
| Camadas (3 ponteiros LZ77) | 0x3F9080 | 12 | 2 |
| Encontros | 0x3BEC10 | 8 | 4 |
| Encontros (extra) | 0x3BF210 | 4 | 1 |
| Área / região (0–10) | 0x3D2B6B | 1 | 2 |
| Área / região (cópia B) | 0x483774 | 1 | 1 |
| Dimensões (largura, altura em tiles) | 0x3F8900 | 2 | 2 |
| Gráficos do tileset (LZ77) | 0x3F8A80 | 4 | 1 |
| Atributos de cor | 0x3F8D80 | 4 | 1 |
| Paletas do tileset (0x200 B) | 0x3F9980 | 4 | 1 |
| Colisão (1 B por metatile 16×16) | 0x404334 | 4 | 3 |
| Bloco por mapa A | 0x409480 | 4 | 1 |
| Bloco por mapa B (0x80 B cada) | 0x40F880 | 4 | 1 |
| Warps (8 B cada, fim 0xFF) | 0x412E60 | 4 | 3 |
| Sprites usados no mapa (fim 0xFF) | 0x413160 | 4 | 1 |
| Música | 0x47E644 | 1 | 1 |
| Pessoas / gatilhos (8 B cada, fim 0xFF) | 0x483234 | 4 | 1 |

- Nenhuma tabela é referenciada fora desses literais (conferido em toda a ROM), e nenhum array na RAM é
  indexado pelo ID do mapa (o save não tem dados por mapa).
- **Expansão** (`MapExpander.Expand`): as 16 tabelas vão para espaço livre com **255 entradas**, os literais
  são reapontados e o marcador `MBXMAPS\0` + mapas em uso (u16) + capacidade (u16) fica 16 bytes antes da
  tabela de camadas; o `RomLayout` lê a quantidade de mapas dali (sem marcador = 192).
- **Novo mapa** (`MapExpander.AddMap`, + verde na aba Maps): copia as 16 entradas de um mapa existente
  para o próximo ID; warps e pessoas vazios (lista só com o terminador) ou copiados.
- Sem saída o mapa novo não é alcançável: é preciso um warp chegando (de outro mapa) e um saindo.

### 13.2 Tileset e paleta
- O tileset é descomprimido na VRAM em `0x6009000` (bloco de caracteres 2 + 0x1000); a cópia do tilemap
  para a tela soma 128 ao índice. Os 3 BGs usam telas 64×64 em `0x6003000`, `0x6005000` e `0x6007000`.
- A paleta do mapa ocupa os 16 bancos de BG (0x200 B); os mapas originais usam todos os 16.
- No Toolkit: aba Maps → Layer Editor → exportar/importar tileset (folha de 16 tiles por linha, cada tile no
  banco em que é usado, cor 0 transparente) e paletas (grade 16×16). Vai para o projeto como
  `MapTilesetPatch`, para todos os mapas que usam o tileset ou só o atual.

### 13.3 Eventos
- O jogo (rotina `0x0804D83C`) acha o evento N em `base + banco[N]×0x4000 + (ponteiro[N] − 0x4000)`;
  cabeçalho na base: `[0..1]` = deslocamento da tabela de ponteiros (0x4000), `[2..3]` = da tabela de bancos.
  A base fica na RAM em `0x3005590`, copiada dos literais `0x4A584`, `0x4A8C8`, `0x4B124` (EU: `0x765EC0`).
- Scripts se referem só por ID e saltos relativos → o Toolkit move tabela + scripts juntos
  (`EventDatabaseRelocator`) quando a tabela cresce ou o espaço livre está além do alcance dos bancos.

### 13.4 Limites

| Item | Limite | Por quê | Maior uso no jogo original |
|---|---|---|---|
| Mapas | **255** (IDs 0–254) | ID de 1 byte; 0xFF = "nenhum" | 192 |
| Tamanho de mapa | **64 × 64 tiles** (512 × 512 px, 32 × 32 metatiles) | telas de BG 64×64; cópia com 64×64 | 62 × 64 (mapa 67) |
| Tiles por tileset | **896** (28 KB descomprimidos) | VRAM de `0x6009000` até os sprites em `0x6010000`; índice + 128 ≤ 1023 | 895 |
| Paletas de BG por mapa | **16 × 15 cores** (+ cor 0 transparente) | paleta de BG inteira | 16 bancos |
| Colisão | 1 byte por metatile (até 32 × 32 = 1024 B) | — | 31 × 32 |
| Warps por mapa | sem limite fixo encontrado (lista na ROM) | lida direto da ROM | 15 |
| Pessoas / gatilhos por mapa | sem limite fixo encontrado; ID de evento 12 bits | registro de 8 B | 66 |
| Sprites usados por mapa | **16** (o Toolkit lê até 16) | lista com fim 0xFF | 12 |
| IDs de evento | **4096** (12 bits no registro de pessoa) | campo do spawn | 2288 |
| Scripts de evento | **4 MB** a partir da base | banco de 1 byte × 16 KB | ~110 KB |
| Medabots | **236** | Tinpet fixo em 236/237 | 120 |
| Personagens de retrato | **256** (9 expressões cada) | personagem de 1 byte no `<PORTRAIT>` | 176 |
| Largura da linha de diálogo | **208 px** antes da quebra (212 visíveis) | rotina `0x080402B4` | — |
| ROM | **32 MB** | espaço de endereços do GBA | 8 MB (expandida: 16 MB) |

Observações: 64×64 é o tamanho da tela de BG; o buffer em RAM onde as camadas são descomprimidas
(ponteiro em `0x3006D14`) ainda não foi medido, então o maior tamanho **comprovado** é 62×64 (3968 tiles).
Os limites "sem limite fixo encontrado" dependem de quantos objetos o jogo aloca na RAM ao carregar o
mapa; até o maior uso original é seguro.

## 14. Texto, retratos e engenharia reversa

### 14.1 Quebra de linha do diálogo
- Rotina `0x080402B4`, estado em `0x03000440` (`+0x14` caneta x em u8, `+0x15` largura restante da
  palavra, `+0x19` flag de quebra). Fonte `0x084BFC6C` (itálica `0x084C59AC`), larguras `0x083B4E08` /
  `0x083B5008` (2 B por glifo; o 1º é o avanço).
- Antes de cada palavra o jogo mede até o espaço seguinte **inclusive** (para em código de controle); se
  `x + largura > 208` quebra: linha 1 → linha 2, linha 2 → caixa nova depois do botão.
- x e largura são u8: linha com mais de 256 px é desenhada por cima do começo; palavra com mais de 255 px é
  medida módulo 256 (e pode não quebrar). `<MEM:n>` (nome do jogador) é medido como uma palavra.
- Comprovado com a captura `cap_3` (RAM quadro a quadro) e a ROM `teste_linha`.

### 14.2 Retratos
- Tabelas `0x083AFE88` (personagem × 9 + expressão, ponteiros Malias2) e `0x083B1748` (1 paleta de 32 B
  por personagem). Lidas por `0x0803F41C`, `0x0803F480`, `0x0803F51C` (3 + 4 literais), **sem checagem de
  limite**.
- A expansão move as duas para 256 personagens, grava o marcador `MBXPORT` (em uso, capacidade) antes da
  tabela de imagens e reaponta os literais. Edições de retrato são reposicionadas pelo número do
  personagem na hora do Export, então editar antes de adicionar não perde nada.

### 14.3 Engenharia reversa (Ghidra)
- Símbolos versionados em `re/symbols/mtbe.csv` no repositório `jvsdm/Medabots-Romhacking-Toolkit`
  (branch `claude/medabots-project-ajyaeb`); scripts do Ghidra em `re/ghidra/`; roteiro em `re/HANDOFF.md`.
- **Divisão de trabalho**: a sessão na nuvem faz a análise estática pesada (decompilar, nomear, documentar
  no CSV e no HANDOFF); a sessão local faz o que precisa da máquina — Ghidra com GhidraMCP, mGBA e
  capturas, testes no jogo, editor WPF — e devolve o que confirmou para o repositório.
- GhidraMCP 1.4 (LaurieWired) instalado no Ghidra 12.1.4 (extensão com a versão ajustada; o release é para
  11.3.2); bridge em `tools/ghidra_mcp/bridge_mcp_ghidra.py`, servidor `http://127.0.0.1:8080/`.
- **mgba-mcp** (struktured-labs): cada chamada abre o mGBA com um script Lua, roda N quadros, lê memória / OAM /
  tira print e fecha. Precisa de `--script`, que só existe no mGBA 0.11 (build noturna portátil em
  `tools/mgba-nightly/mGBA/`; o 0.10.5 instalado não tem). Patch para Windows (sem xvfb, `taskkill`, janela sem foco)
  em `tools/mgba-mcp-windows.patch`. Janela minimizada gera print vazio.
- Banco do Simon (Rokusho E): portado para o CSV em 2026-10-10 (866 símbolos, tipos em `re/types/`); ver `re/HANDOFF.md`.

## 15. Esqueleto, moldes e animações de batalha

### 15.1 Árvore de moldes (esqueleto)
- Tabela `0x083D1D78`: 50 ponteiros para registros de 0x18 B. `+0x00` lista de artes (pares imagem/paleta),
  `+0x0F`/`+0x10` **dois moldes filhos** (percorridos recursivamente), `+0x11` paleta, `+0x12`/`+0x13`
  largura/altura em px, `+0x15` forma do sprite. `+0x04..+0x0E` parecem pontos de encaixe.
- Árvore: 1 → 2 → juntas 3-6 (na RAM: cabeça, braços, pernas, ligadas ao 1º molde de cada peça). Cabeça 7-13,
  braço direito 14-16, esquerdo 17-19, pernas 20-49 (seis variações por tipo).
- Peça do bot (u32): bits 0-5 molde, 6-12 arte (8 bits com o patch), bit 14 espelho, bit 15 variante.
- **Cada pedaço = um sprite de hardware: no máximo 64×64 px.** Bot maior/mais complexo = mais pedaços (moldes).

### 15.2 Animação de movimento
- Roteiro = `gMoveScriptTable[tipo×7 + gMoveStyle[bot das pernas]]` (59 roteiros). Por roteiro:
  `0x083D2018` duração de cada quadro-chave (bytes, fim 0; N = quantidade), `0x083D1E40` ângulo s16
  [molde][N] (1/65536 volta), `0x083D1F2C` deslocamento (s8 x, s8 y) [molde][N]. Os dados têm **49 linhas
  (moldes 1-49)**.
- O tocador (`0x08047428`) interpola para **63 moldes** no estado por molde (`0x020260D0`, 16 B por molde,
  0x410 B por lado = 65 posições).

### 15.3 Moldes novos (50-63) — resolvido (2026-10-10)
O bloqueio antigo não era o vetor de estado (ele tem 65 posições por lado), e sim **dados**:
1. tabela de moldes com 50 posições (logo depois vêm os ponteiros de ângulo);
2. **mapa de espelhamento** `0x08081490` (50 B; troca os braços para o lado direito): moldes ≥50 viravam 0 e
   sumiam do lado direito;
3. **roteiros de movimento** com 49 linhas: moldes ≥50 liam dados do bloco seguinte e as peças "voavam".
`tools/mold_lab.py` estende os três (64 / 64 / 63 linhas) e faz a cabeça do bot 0 usar clones 50/51: no
visualizador Motion do menu de debug o bot fica idêntico ao original, dos dois lados.

### 15.4 Bots do Navi com a geometria do Navi (2026-10-10)
- O Navi usa o mesmo sistema de moldes; os formatos que o Core não tem são braço 64×64 num pedaço (15/18, 33/34),
  cabeça 64×64 (30), cabeça 64×32+32×16 (11/12) e pernas num pedaço (24) ou longas (31/32, 35-37).
- **Linhas do Navi: cabeça e braços 16 B, pernas 32 B** (como no Core). A leitura antiga com 64 B juntava a perna do
  bot seguinte: as "peças extras" eram isso, não Medachange. `art/navi/` foi extraído de novo.
- `NaviImporter` (Toolkit, `src/Medabots.Rom/Expansion/NaviImporter.cs`): adiciona bots do Navi à ROM expandida.
  Peças com molde de mesmo formato no Core vão para ele (cabeças 7-10, braços de dois pedaços 13/14→14/15 e
  16/17→17/18, pernas bípedes 19-23→31-35, corrente 25-29); as outras ganham moldes novos (50+) com o registro do
  molde parecido do Core e tamanho/encaixe do Navi. Stats, mini sprite e estilo de movimento vêm de um bot do Core
  com a mesma geometria de pernas (Rokusho 31-35, Spidar/Totalizer 25-29, Noctobat/Churlybear/Armorparadeen um
  pedaço); o tipo de perna do jogo é o do Navi.
- Lote de teste (21 bots, IDs 182-202) em `build/Medabots_expanded+navi+debug.gba` + `.sav` com todas as peças.
- As formas de Medachange do Navi ainda não foram encontradas (não estão nas linhas dos bots).

### 15.5 Montagem fiel do Navi: tabela de moldes do Navi + hooks (2026-10-10)
- Problema: com os registros do Core, braços e pernas saíam do lado trocado e mal encaixados. Causa principal: no
  Core a linha do **braço direito usa os moldes 17-19 e a do esquerdo 14-16**; o mapa antigo mandava o braço
  direito do Navi (13/14) para 14/15 (lado esquerdo do Core). Agora: direito 13/14→17/18, esquerdo 16/17→14/15,
  64×64 15/33→novos com base 19, 18/34→base 16. Além disso o Navi guarda o deslocamento do membro na **junta**
  (templates 0x4BFBE4/FC, 0x4BFC14/2C: direito (-4,-7), esquerdo (-4,+7)) e o Core no **primeiro pedaço**
  (17: -8, 14: +8, juntas zeradas), e os pivôs dos moldes de mesmo formato diferem.
- Solução: `NaviImporter` grava uma **segunda tabela de moldes** (64 ponteiros, depois do marcador `MBXNAVIT`) com
  os registros do Navi nos IDs do Core (o movimento do Core continua servindo) e o offset da junta somado ao
  primeiro pedaço de cada linha (a junta da RAM aponta para a 1ª entrada da linha; não é o filho fixo do template).
  Entradas do Navi levam o **bit 16** (o jogo não usa).
- `inject/src/navi_molds.c`: hooks em PartPreviewPiece 0x08045E6C, PlacePiece 0x080465B8 e CopyArmGraphics
  0x08046034. Cada um escolhe a tabela do Navi se a peça tem bit 16 e roda o original (prólogo de 16 B copiado num
  trampolim). `build.py` acha o marcador e liga os hooks sozinho (`gNaviMoldTable` via `--defsym`).
- Lado 1 (espelhado): no Core toda peça sem rotação leva hflip; no Navi não leva se tiver **bit 15** (segunda arte
  já desenhada para o lado 1, teste em 0x0801A7F4). O importador grava essa segunda arte já invertida, e o hflip
  do Core a desvira. Isso consertou Lady Outer, Giraffe Chief e Fearer.
- **Cintura:** o molde 2 do Navi (raiz do tronco, pai das juntas da cabeça e dos braços) tem deslocamento (−4,0);
  o do Core é zero, e a junta das pernas fica no molde 1. Sem isso cabeça e pernas ficavam 4 px mais próximas.
  O importador soma esse deslocamento às raízes da cabeça e dos braços.
- **Linha do chão:** na pose parada (tela de montagem, sem movimento) os bots do Navi ficavam com os pés 2-17 px
  acima da linha do Core (Flying Of 13 px abaixo). O importador mede o pixel mais baixo de cada bot (registros +
  arte, todas as juntas num ponto) e compara com a mediana dos bots do Core; a diferença vai nos bits 24-31 da
  1ª entrada da perna, e o hook do PlacePiece desloca o bot inteiro na raiz (molde 1). Com perna trocada (Tinpet)
  o ajuste some junto. Conferido: pés dos 21 em y=94 na tela de montagem (Core 93-95).
- Moldes novos que começam um membro usam registro/movimento da raiz do Core (cabeça 7, direito 17, esquerdo 14,
  pernas 20).
- **Peças misturadas** (Tinpet quando a peça quebra, ou peças trocadas entre bots): com o mapa antigo o braço de
  Tinpet (molde 14) colidia com o braço direito do Navi no mesmo molde e pegava o registro do Navi (braço solto).
  Corrigido pelo mapa por lado. No lado 1 o hook do PlacePiece escolhe a tabela pela peça do molde espelhado (é a
  geometria usada ali), e o do CopyArmGraphics pula a troca de arte entre braços quando os tamanhos diferem ou a
  peça do Navi tem arte própria do lado 1 (bit 15).
- Resultado: 21 bots conferidos nos dois lados contra o Medapedia; bots do Core idênticos com e sem os hooks;
  Granbeetle com braço do Core e Lady Outer com braço do Core encaixam nos dois lados.
  Pendentes: Hophop menor que a referência (perna sem paleta própria); braço do Navi de uma peça 64×64 sem arte do
  lado 1 (asa do Umbrebat) num bot do Core sai deslocado no lado 1.

## Créditos e referências

- [Medabots-Romhacking-Toolkit](https://github.com/STulling/Medabots-Romhacking-Toolkit), [Medabots-Randomizer](https://github.com/STulling/Medabots-Randomizer), [MedabotsLib](https://github.com/STulling/MedabotsLib) — STulling
- [mdr-navi-i18n](https://github.com/caribbeansc/mdr-navi-i18n) — charset do Medarot Navi e codec Malias (MIT)
- [Medapedia](https://medarot.meowcorp.us/) — listas de Medarots, medalhas e ações
- Malias2 — Normmatt
