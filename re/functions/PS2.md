# PS2: telas de peças (continuação do PS.md) — GH-PS2 (2026-10-14)

Status: PARCIAL. Confirmado: os 4 InitScreen (nomes novos no Ghidra), PartDetailScreen_Init, Tinpet_InitIds (campo +7).
Pendente: quem lê a tabela 0x083AC880 e a base da tabela 0x083AD0F0, regras de borda do cursor (não reli 0x080183A4 nesta rodada), escritor da flag +0x40.
Ghidra: 4 funções renomeadas (não salvo; não fechado).

## 1. As 4 funções chamadas PartsGrid_InitScreen (decompile lido)

Padrão comum: DISPCNT = máscara & atual; registradores de BG (0x500); `decompress_and_store` de 4 a 8 blobs; `LoadTilesWithMalias` em 4 bancos de 0x200; `bios_cpufastset` para zerar VRAM/paleta; `REG_DISPCNT |= 0x1F60`; `SetGpuBlendMode(5,0x3F,0x20)`; zera ctx+0x84; incrementa contador. Muda o conjunto de desenhos.

| Endereço | Nome antigo | Nome novo (Ghidra) | Blobs | Desenhos (chamadas) | Confiança |
|---|---|---|---|---|---|
| 0x08007A04 | PartsGrid_InitScreen | PartsGrid_InitScreen_CellGraphics | 4 | UploadSelectedTile, DrawSelectedGraphic, DrawSelectedLabel, DrawCellIconLayer, DrawCellGraphicSmall, DrawCellGraphicMedium, DrawCellTileIndex | provável (grade de células com gráfico pequeno/médio) |
| 0x08007E38 | PartsGrid_InitScreen | PartsGrid_InitScreen_DetailRows | 5 | UploadSelectedTile, DrawSelectedGraphic, DrawSelectedLabel, DrawCellIconLayer, Sprite_InitPairC, DrawDetailRows, DrawDetailByMode, Sprite_SetPairSizeC; ctx+0x46=0 | provável (grade com linhas de detalhe e modo de detalhe) |
| 0x08008234 | PartsGrid_InitScreen | PartsGrid_InitScreen_StatusPreview | 7 + paleta gMiniSpritePalettes | UploadSelectedTile, DrawSelectedGraphic, DrawSelectedLabel, DrawCellIconLayer, DrawSlotIconRow, DrawCellStatusBar, DrawPartRegion, DrawPartPreview, DrawCellPartStatValue | provável (lista de peças com barra de status e prévia) |
| 0x0800DA34 | PartsGrid_InitScreen | MedabotGrid_InitScreen | 7 + 4 bancos | se ctx+0x42==1: DrawLargeDisplayIfOwned; se ctx+0x3C==4: snd_song_request(7); BgMap_CopyRectWithOffset (linha = ctx+0x3E); Num_DrawDecimal6BigTiles(ctx+0x3F+1); SplitCursorIndexRowCol; DrawFourEntryText, DrawFourIdCells(B), DrawFourPartIcons; SetupCursorSpritePair; Menu_SetupCursorSprite; ctx+0x44=0; se ctx+0x42!=1: InitScreenBuffers | provável (tela 3x3 dos Medabots: número do slot 1..9 e cursor linha/coluna) |

Ctx comum: IWRAM 0x03000440 (+0x3C/+0x3D/+0x3E/+0x3F, +0x42, +0x44, +0x46, +0x84).

### Cruzamento com a tabela 0x083AC880
- Registros de 3 words (stride 0xC), endereços Thumb. Endereço da linha = 0x083AC880 + 0xC*k:
  - k=0 @880: 080079CC, 08000840, 08007998
  - k=1 @88C: **08007A04** (col0), 08007BD8, 08007BFC
  - k=2 @898: 08007C20, 08007C4C, 08007D04
  - k=3 @8A4: 08007BD8, 08007D28, 08000840
  - k=4 @8B0: 08000840 x3
  - k=5 @8BC: 08007D04, 08007BD8, 08007DDC
  - k=6 @8C8: 08000840, **08007E38** (col1), 0800801C
  - k=7 @8D4: 08008050, 08008084, 080080C8
  - k=11 @904: col2 = **08008234** (@90C)
  - k=92 @CD0: col1 = **0800DA34** (@CD4); o pool 0x0800DA2C aponta para 0x083ACCD4, lido pela função em 0x0800DA06 (sem nome no Ghidra)
- Não há coluna "init" fixa: as 4 funções aparecem em colunas diferentes. Hipótese (não confirmada): cada registro é {?, init, tick}, e a coluna depende do registro.
- Nenhuma palavra da ROM vale 0x083AC880 nem os endereços das linhas: quem lê a tabela ainda é desconhecido.

## 2. Tabela 0x083AD0F0 e a tela MEDABOTS
- Stride 4; entrada 9 = UpdateSelectedMedabotPreviewScreen 0x08013E30 (palavra em 0x083AD114 = 0x08013E31); entrada 10 = Fight_SelectScreenFrame 0x08013FA8 (palavra em 0x083AD118). Confirmado pela busca de palavras na ROM.
- NENHUMA palavra da ROM vale 0x083AD0F0. O código que a indexa calcula a base de outra forma: PENDENTE.
- Pools próximos (0x083AD064, 0x083AD06A, 0x083AD070, 0x083AD080) pertencem a outras tabelas. 0x083AD080 é a tabela de `Battle_FrameDispatchOrExit` 0x0801299A (`CallViaR0(tabela[byte*4])`); não é esta.
- Entrada pelo menu de campo: PENDENTE (não rastreado nesta rodada).

## 3. Tela de detalhe da peça
- `PartDetailScreen_Init` 0x0800E020 (nome já existia; decompilado): limpa 4 pares de slots; se ctx+0x3C é 0 ou 4, `PartsGrid_DrawLargeDisplayIfOwned` e blobs do modo grande; senão blobs do modo pequeno (+ `PartPreview_InitScreenBuffers`, `PartsGrid_RedrawLargeDisplay`). Depois: `GetPartStats(ctx+0x3D, ctx+0x3E, ctx+0xEE)`, desenha nome, atributo, distintivo de especialidade, descrição, distintivo de gênero, rótulos e valores de status, conta peças disponíveis para o filtro de atributo (`PartDetail_CountAvailablePartsForCurrentAttributeFilter`), cursor (`PartDetail_SetupCursorSpritePair`); ctx+0x42=1; contador++.
- `PartDetail_SceneStep` 0x0800EA2C: passo por quadro (ver FL5.md). Não relido nesta rodada.
- Confiança: confirmado (decompile do init). Papel de ctx+0x3D/+0x3E como índice de bot e tipo de peça: provável.

## 4. Flag de célula ocupada (+0x40 de 0x03000590 + k*0x100) e Tinpet
- `Tinpet_InitIds` 0x080421A0: grava o byte +7 de cada registro de 8 B do TIME (base 0x030017A0, confirmado pelo literal 0x080421B0) com 0..8. NÃO escreve o +0x40 de nenhum bloco 0x03000590.
- Logo a origem da flag "ocupada" fica ABERTA. Não confirmar sem achar o escritor.
- Regras de borda do cursor 3x3 (0x080183A4): não reli nesta rodada; o resumo do PS.md segue como provável.

## Pendente
1. Quem lê 0x083AC880 e o significado das colunas (init/tick/exit).
2. Base de cálculo da tabela 0x083AD0F0 e a entrada pelo menu de campo.
3. Escritor de +0x40 do bloco 0x03000590 + k*0x100.
4. Regras de borda de 0x080183A4 (reler com decompile).
5. PartDetail_SceneStep 0x0800EA2C (relido).
