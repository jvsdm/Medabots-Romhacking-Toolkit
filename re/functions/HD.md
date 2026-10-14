# HD: interface da Robattle (HUD) — GH-HD

Escopo: barras de armadura das peças, medidor de carga (gauge) e de Medaforça, painéis de peça, ícones de ação,
cursor 3x3, letreiros (CRITICAL HIT / LEG DEFENSE) e números de dano.

Convenções: endereços da ROM EU (Ghidra). RAM do bot: `0x03000590 + bot*0x100` (offsets do Ghidra; a pesquisa do PC
usa `0x030005D0`, offset Ghidra = offset da pesquisa + 0x40). `fight_information` = `0x03001800`.
Status: **PENDENTE** até cada seção ser decompilada e confirmada.

## Mapa de elementos

| Elemento | Função principal | Estado |
|---|---|---|
| Gauge de barra 12 tiles | `Gauge_DrawBar12Tiles` 0x0803EA38 | confirmado (R2, reconferir) |
| Gauge vazio / seleção cheia/vazia | `Gauge_DrawEmpty12Tiles` 0x0803EAD8, `Gauge_DrawByFlag4` 0x0803EB0C | confirmado (R2) |
| Gauge do bot (8 células, ação) | `Fight_DrawBotGaugeTilemap` 0x08015C40 | provável (BT), pendente |
| Painel de peça | `BattlePanel_*` 0x08062A10.. | pendente |
| Animação de descida das barras | pendente | pendente |
| Letreiros | `Fight_SpawnInlineEffectTextObjects` 0x08036FF8 | pendente |
| Números de dano | `Screen_RevealStepShowNumber` 0x080028AC | pendente |

## Seções

### 1. Gauge do bot (barra de 8 células) — `Fight_DrawBotGaugeTilemap` 0x08015C40 (confirmado no decompile + literais da ROM)

- Entrada: `param_1` = índice do bot (0..5). `param_1 < 3` = lado esquerdo; `>= 3` = lado direito.
- Valor: byte da RAM `0x03000590 + bot*0x100 + 0xC1` (Ghidra; = offset 0x81 na pesquisa do PC, NÃO confundir com o medidor
  +0xBB da pesquisa, que é outro byte). Verificar no PC antes de usar o nome "carga".
- Conversão: `Fight_ComputeGaugeFillFixed` 0x08015DD0 calcula `v * (0x40/0x50)` em soft-float, devolve
  `(inteiro >> 3) << 16 | (inteiro & 7)` (u32: alto = células cheias, baixo = resto 0..7). Se `v != 0` e o resultado
  deu 0, força resto 1 (barra nunca some com valor > 0).
- Desenho: uma `FillBgTilemapRectSequential(1,1,x,0x11, mapa, tile)` por célula, linha y = 0x11 (17).
  - Lado esquerdo: x = 1..8. Lado direito: x = 0x15..0x1C (21..28).
  - Células cheias: tile `0xD208` (literal 0x08015D08). Resto parcial: `0xD200 + resto` (1..7).
    Vazias: `0xD200`.
  - `mapa` = `*DAT_08015D04` (literal 0x0400000A, ou seja, o ponteiro lido é a base do tilemap via o registrador de BG).
    Procedência exata do destino (BG1 ou outro) ainda não confirmada.
- Atenção: o laço de células cheias não tem limite; se o valor passar de 0x50 (`v*0.8 > 63`) escreve além das 8 células.
- Chamada por: não rastreada nesta passagem (usar `get_function_xrefs`).
- Confiança: confirmado (código + literais); destino BG e significado do byte +0xC1 provável.

### 2. Valor da barra de gauge — `Fight_TickPartGaugePhases` 0x08014B34 (confirmado no decompile)

- Base `fight_information` = 0x03001800 (literal 0x08014C10). Dois lados com fase própria: fi+0x51 (lado A) e fi+0x53 (lado B).
- A fase é o índice do bot da fila (0..2; valor 3 = concluído). A cada 2 quadros (paridade de fi-timer 0x03000440+0x86,
  halfword) a fase avança e o bit 0 de `byte 0x08014C1C+base` inverte.
- Registro de saída: halfword em 0x03005016 (literal 0x03005010 + 6), formato: byte da parte `0x03000590+bot*0x100+0xDD`
  (+0x100 se bit de paridade), bit 15 marca o registro ativo.
- Quando o byte de bit 15 do lado tem o bit 8 diferente do estado (fi+0x1B0), copia a parte para `+0xDD` do bot (lado) e
  avança a fase.
- Quando as duas fases são 3, incrementa o contador em 0x0300137C (literal 0x08014C2C) e zera o timer.
- Confiança: confirmado (código); significado de "gauge cheia" = "pode agir" ainda não confirmado (lacuna do BT.md).

### 3. Normalização do desenho (`Gauge_*`, R2.md, confirmados)

- `Gauge_DrawBar12Tiles` 0x0803EA38: barra de 12 tiles com valor 0..0x60 em fi+0x1A0 (`>>3` cheias, `&7` parcial).
- `Gauge_DrawEmpty12Tiles` 0x0803EAD8: 12 tiles vazios (linha 0x12, colunas 7..18).
- `Gauge_DrawByFlag4` 0x0803EB0C: bit 0x04 do halfword +0x86 escolhe cheia/vazia.
- `Gauge_SetCounterFrom0x68` 0x0803EB30: contador = 0x68 - valor, piso 0xC.
- `Gauge_DispatchByStepIndex` 0x0803EB5C: índice = valor>>4 para tabela de 7 handlers.
- Este conjunto é a barra de 12 tiles da ação/carga (lado do ataque), distinta da barra de 8 células do bot (item 1).

### 4. Medidor do bot (animação do sprite) — `Fight_UpdateBotMeterDisplayObject` 0x08019544 (confirmado)

- Base do bot: literal 0x080195BC = 0x03000590. Byte do medidor: `bot*0x100 + 0xBB` (0x0300064B para o bot 0).
- Velocidade do sprite composto: `((0x20 - medidor) >> 1) + 1` (`SetCompositeBattleSpriteAnimationSpeed`, objeto `bot*4+0x5A`).
  Se o bit 0x200 de `gObjectSlots`-like do objeto (`(bot*4+0x5B)*0x40 + 0x03002C30`) está ligado, grava a velocidade no
  byte +0x24 do objeto `bot*4+0x5D` e chama `Sprite_UpdateComposite4Motion`.
- Divergência a checar: a pesquisa do PC (README, "medidor do bot = byte +0xBB, teto 0x1F") não diz se o +0xBB é contado
  de 0x03000590 ou de 0x030005D0. Pelo código, é 0x03000590 + 0xBB. Conferir no emulador antes de portar.
- Confiança: confirmado (código); o significado físico do medidor (carga/ação) é provável.

### 5. Armadura por peça (dado da barra) — confirmado

- Armadura atual da peça k do bot: `0x03000590 + bot*0x100 + 0x59 + 0x18*k` (Ghidra). Equivale a +0x0D da pesquisa
  (peças a partir de +0x0C na pesquisa, +0x4C no Ghidra). Lido em `Fight_CopyPartArmorToDisplayValues` 0x08036B84,
  que copia para `fi+0xC8`, `fi+0xCC` (u16) e grava `fi+0xD1 = 1`.
- Valor exibido limitado por `Fight_ClampDisplayValue1To99` 0x08036E54 (1..99, zero vira 1).
- Chamadores: `Fight_DispatchOnSubstateAA_H_07_LoadStageGfx` 0x0801E60E, `Fight_DispatchOnSubstateAA_H_20_ApplyPartDamage` 0x0801ED04.
- Desenho da barra de 4 peças (descida animada): NÃO localizado nesta passagem. Candidatas não verificadas:
  `Fight_RefreshBotDisplayedPartSprites` 0x08015A00, `Fight_BuildBotPartDisplayStats` 0x0801520C,
  `Fight_RefreshBotPartSlotDisplays` 0x0801528C (só monta metadados de ação, não desenha barra; descartada).

### 6. Ícones de status por bot — confirmado

- `Fight_DrawActiveBotsOverlay` 0x080195C0: para os 6 bots ativos (flag em `0x03000590+bot*0x100+0x40` != 0), chama
  `Fight_DrawSecondaryEffectIcon`, `Fight_DrawPendingEffectIcon`, `Fight_DrawPartStatusIcon`. Limpa a máscara de atributo
  OAM `Slot_ClearAttrMask(0x46, 0x12)` antes.
- `Fight_DrawPartStatusIcon` 0x08019B40: bits de `bot+0xBD`: 0x08 = modo de ícone A, 0x10 = modo B. Objeto de ícone
  `0x48 + 3*bot` (OBJ). Tipo do ícone = byte `bot+0xDF` (valores 0x1B, 0x1C, 0x1D escolhem registro de sprite diferente).
  Posição: `Fight_PlaceBotStatusIconObject(bot, 2, 2, bot+0x174B + 4)`.
- Tabela de sprites de objeto: base 0x03002C30 (gObjectSlots).

### 7. Painel de 6 dígitos — `Overlay_DrawSixDigitRow` 0x0803E8EC (confirmado no código)

- 6 linhas de dígitos, `FillBgTilemapRectSequential(7,2, x=4, y=2,4,...,12, mapa, tile)`.
- Tile do dígito = `0x3380 + dígito*0x0E` (cálculo `byte*0xE0000 + 0x33800000 >> 16`), onde o byte vem da tabela
  apontada por `DAT_0803E92C`. Não é o "-N" de dano da batalha (é painel numérico de overlay, uso não confirmado).

### 8. Revelação por passos com número — `Screen_RevealStepShowNumber` 0x080028AC (confirmado no código)

- Contador de passo: byte `DAT_08002A04 + 0x84` (estado) de 0 a 0x12. Copia 4 faixas 12x2 (linhas 1, 3, 5, 7; x=0x12),
  cada uma de tabela de ponteiros, índices `passo+0x13/+0x1E/+0x29/+0x34`. Passos 10 e 12 descomprimem tiles e chamam
  `LoadTilesWithMalias` (0x15, 9, ..., 0x120).
- Após o passo 0x12: `Num_FormatDecimalToGlyphs` no número, `Text_LayoutGlyphSprites(…,10,0x17,10,5,0xF,2,0)` e
  `Screen_LoadSpriteRowA`.
- Não é o dano de batalha. Provável tela de resultado (XP/dinheiro). Confirmar por xref antes de dar nome final.

### 9. Letreiros de efeito — `Fight_SpawnInlineEffectTextObjects` 0x08036FF8 (confirmado no código)

- Argumentos: `(slot0, x, y, n)`. Limpa `n` entradas de objeto (`ClearObjectAttributeEntries`), e para cada uma: tile
  inicial `DAT_0803709C` (halfword), prioridade/atributo fixo `+0x0E=0`, `+0x05=7`, `+0x0B=0xF0`, `+0x16 = i*8`.
  Posição: `x + tabela[i].x` (máscara `DAT_080370A4`), `y + tabela[i].y` (`& 0xFF`). Monta com `Slot_SetupFromRecordList`.
- Espaçamento: passo de 8 no campo `+0x16`. Textos reais (CRITICAL HIT / LEG DEFENSE) não confirmados: o texto vem do
  registro passado, não do corpo.

### Pendente nesta passagem
- Desenho exato da barra de armadura das 4 peças e a descida animada (candidatas acima, não lidas).
- Cursor 3x3 da tela de prévia (escolha de peça): `HandleSelectedMedabotPreviewCursorInput` 0x080183A4, único chamador `UpdateSelectedMedabotPreviewScreen` 0x08013E84 (QA3).
- Texto dos letreiros e destino BG dos gauges (BG1? CBB/SBB).
- Renomes no Ghidra: nenhum novo nesta passagem (os nomes desta seção já estavam aplicados).
