# IV: inventário de peças, layout do save e base do roster (GH-IV, 2026-10-14)

Status: CONFIRMADO no código (decompile Ghidra, somente leitura; disassembly Capstone da ROM EU, python -I).
Não testado com savestate (sem emulador nesta tarefa). Nenhum `.sav` lido. Ghidra não salvo nem fechado; nenhum
rename aplicado no projeto (os nomes já existentes batem com o código).

Substitui IV onde houver diferença: PS.md (campo 1 = "equipado/uso" virou **equipadas**), SV2.md (já estava certo no
mapa; aqui fica a fórmula fechada), DC4.md/PB.md (conflito do roster resolvido).

## 1. Layout de 0x03004C40 (inventário de peças, IWRAM)

- Base: literal 0x080435AC (= 0x03004C40; mesmo valor em 0x08043584 e 0x080440EC).
- 960 B = 4 tipos x 120 bots x 2 campos. Tipos 0..3 = cabeça, braço D, braço E, perna (ordem de BT2 e do
  extrator; ver 6 para o que é provável). Bot 0..0x77 (120) e 0x78 = sentinela "sem peça" (checado em
  `PartSelect_CycleNextValid` e `Parts_*`: `bot < 0x78`).

```
inv(t, b, c) = 0x03004C40 + t*0xF0 + b*2 + c     // t 0..3, b 0..119, c 0..1
c = 0  -> QUANTIDADE possuída (byte). Teto 99 na soma.
c = 1  -> EQUIPADAS (quantas unidades desse bot estão montadas num Medabot). Sem piso/teto na subtração.
```

Campo 1 (equipadas), provas:
- `Parts_BuildEquipTable` 0x08007034: para cada um dos 4 slots do bot montado, se o bot < 0x78 chama
  `AdjustInventoryPartCount(bot, t, 1, 0, 1)` = **equipadas += 1**.
- `Parts_PrepareFourPartSlots` 0x08005A60: no mesmo caso chama `AdjustInventoryPartCount(bot, t, 1, 1, 1)` =
  **equipadas -= 1** (desmontar).
- `PartSelect_CycleNextValid` 0x080436AC: uma peça só é selecionável se `qty != 0` e `qty != equipadas`
  (o laço para na condição de sucesso; leitura direta do decompile). Ou seja: não se oferece peça com todas as
  unidades já montadas.

Quem escreve quantidade (c = 0):
- `Shop_CommitBuySellTransaction` 0x08062614: compra soma `gShopQuantities[i]` em `inv(tipo, bot, 0)`; venda subtrai.
  Sem piso na venda (checar se o jogo impede venda acima do possuído: pendente).
- `AdjustInventoryPartCount` 0x08043560 (modo 0 soma com teto 99; modo != 0 subtrai sem piso, para `c` dado).

Quem lê quantidade:
- `Shop_InitPartSelectionScreen` 0x080619B0: `gShopItems + i*8 + 2 = inv(tipo, bot, 0)` (lista da loja/venda).
- `Stats_DrawSummaryScreen` 0x0805D860: conta os bytes de paridade par (c = 0) dos 480 pares = peças distintas com
  qtd != 0 (o contador usa passo 2 e 0x1E0 voltas).

Não exaustivo: a varredura de literais de código (ver 4) achou também usuários em 0x0800EC44, 0x0800ED10,
0x0800EDA0, 0x0800EE34, 0x0805B0D8 (bolsa de key items), 0x08044B28, 0x080420C4, 0x0804213C, 0x080421B4 que o Ghidra
não tem como função nomeada. Não foram decompilados um a um; não afetam as conclusões acima.

`Check_PairsAllEqual` 0x080440D4: percorre os 480 pares e devolve 1 se TODOS têm qtd == equipadas. Chamador não
achado nesta rodada (pendente).

## 2. Save: conversão byte -> u16 e offset de cada par

- `SaveLoad_SaveGame` 0x0805E010 e `SaveLoad_LoadGame` 0x0805DF18 usam os pools 0x0805E0F8 (= 0x3BF / 960),
  0x0805E100 (= 0x03004C40), 0x0805E104 (= 0x7AC). Pools relidos na ROM nesta rodada: 0x0805E0F4 = 0x03000BE0,
  0x0805E100 = 0x03004C40, 0x0805E104 = 0x7AC. O corpo do laço (`*(ushort*)dst = (ushort)*(byte*)src`, 960 vezes)
  é o do SV2.md (decompile + asm), não relido aqui.
- Consequência: cada byte da RAM vira um u16 (lo = valor, hi = 0) no save. Tamanho 960 x 2 = 0x780, em +0x7AC.

```
save_off(t, b, c) = 0x7AC + 2*(t*0xF0 + b*2 + c)
                  = 0x7AC + t*0x1E0 + b*4 + c*2
qty_save(t,b) = u16 @ save_off(t, b, 0)   // lo byte = quantidade; hi = 0
eq_save(t,b)  = u16 @ save_off(t, b, 1)   // lo byte = equipadas; hi = 0
```

Exemplo: tipo 2, bot 5, quantidade -> 0x7AC + 0x3C0 + 0x14 = 0xB84; equipadas -> 0xB86.

## 3. Extrator: divergência exata (somente leitura, não editado)

`pc/extract/extract_save.py` linhas 34-36 e 79-86: `INVENTORY = 0x7AC`, `SLOTS = 240`, `at = INVENTORY + kind_i*SLOTS*2 + bot*2`,
`qty, eq = buf[at], buf[at+1]`.

Erros:
1. O passo por bot é 2 bytes, mas o correto é 4 (2 u16). O extrator lê o bot 1 como se fosse o eq do bot 0.
2. `SLOTS = 240` está errado: são 120 bots por tipo. Os bots 120..239 do extrator leem o inventário de outro tipo.
3. `eq = buf[at+1]` é sempre a parte alta (0) do u16 de quantidade, ou o eq de outro bot. Não é o eq do bot.
4. O stride por tipo `kind_i*SLOTS*2 = kind_i*480` está certo (0x1E0). Só o stride por bot e o número de bots estão errados.

Pseudocódigo correto:

```python
def u16(buf, off):          # buf = save já desinvertido (SV2)
    return buf[off] | (buf[off+1] << 8)

for t, kind in enumerate(KINDS):            # 4 tipos
    for b in range(120):                    # 120 bots, não 240
        off = 0x7AC + t*0x1E0 + b*4         # passo de 4 bytes (2 u16)
        qty = u16(buf, off)     & 0xFF      # quantidade (lo byte; hi = 0)
        eq  = u16(buf, off + 2) & 0xFF      # equipadas
        if qty == 0 and eq == 0:
            continue
        parts[kind][b] = {"qty": qty, "equipped": eq}
```

Remover o hack `if eq > qty: qty = eq` e o tratamento de `0xFF`: não existem no original; eram artefatos do layout expandido.

## 4. Base do roster de medalhas: 0x03000BE0 (resolvido)

- Literal 0x08041D5C = **0x03000BE0**. `Slot_FindByIdPlus1` 0x08041D3C varre 30 registros de 0x40 a partir daí
  (teste: byte0 & 0x80 e byte +1 == id) e devolve **índice + 1** (0 = não achou).
- Literal 0x08014F3C = **0x03000BA0** = roster - 0x40. `Fight_CopyBotSlotRecords` 0x08014EE8 faz
  `Mem_CopyBytes(0x03000BA0 + Slot_FindByIdPlus1(id)*0x40, bloco_do_bot, 0x40)`. Como o retorno é idx+1:
  `0x03000BA0 + (idx+1)*0x40 = 0x03000BE0 + idx*0x40`. Mesma memória; não há segundo roster.
- O save confirma: pool 0x0805E0F4 = 0x03000BE0, 30 x 0x40 em save +0x2C (SV2).
- Sentido: `Mem_CopyBytes(SRC=roster, DST=bloco do bot, 0x40)` (DC4 confirmado, mesmo sentido em PB.md).
- Conclusão: **DF.md (0x03000BE0) está certo; PB.md:21/29 (0x03000BA0) é a mesma base com índice 1-based**.
  DC4 em aberto deixa de existir.

## 5. Renomeações e símbolos

Ver `IV_simbolos.csv`. Nenhum nome novo foi aplicado no Ghidra: os nomes existentes (AdjustInventoryPartCount,
PartSelect_CycleNextValid, Slot_FindByIdPlus1, Shop_CommitBuySellTransaction, etc.) já batem com o código.

## 6. O que continua provável ou aberto

- Ordem dos tipos 0..3 = cabeça, braço D, braço E, perna: vem de BT2 e do extrator; no código, Parts_BuildEquipTable
  usa t = 0..3 para os 4 bytes bot+0x98..0x9B. Os nomes exatos dos tipos não foram confirmados pelo texto.
- Usuários de literal não decompilados um a um (ver 1): podem haver outras escritas de equipadas via ponteiro calculado.
  Não encontrei nenhuma além de `Parts_BuildEquipTable` e `Parts_PrepareFourPartSlots`.
- Venda acima do possuído (piso de quantidade): não verificado.
- Validação com savestate do save real: pendente (regra de não usar emulador nesta tarefa).
