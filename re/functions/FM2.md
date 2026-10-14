# FM2: menu de campo, continuação (GH-FM2, 2026-10-14, PARCIAL)

Leitura: ROM original (só leitura; bytes via python -I), Ghidra (disassemble_function funciona em endereço
arbitrário, mesmo fora de função: devolve o trecho até o retorno). Nada salvo, nada renomeado no Ghidra.
Símbolos: `FM2_simbolos.csv`. Código sem função: `FM2_lista.txt`. Continua de `FM.md`.
Confiança: **confirmado** (lido no código), **provável**, **pendente**.

## 1. Tecla B e o ramo que abre o menu (sub 2 do mapa, 0x080510C0)

- Entrada: sub-estado 2 do mapa (0x03005311 = 2). Teclas: `0x03002B64` (held) e `0x0300058C` (keys_pressed = novas).
- 0x0805114C: literal 0x0805118C = 0x03002B64, bit 0 (A segurado). **confirmado** (lido).
- 0x080511BA..0x080511C4: literal 0x080511FC = 0x0300058C; bit 0 (A novo) -> 0x080511C6 (chama 0x08052284 e 0x08052624).
- 0x08051204: **bit 1 de 0x0300058C = B novo**. Só entra no ramo de menu se `E+0xC33 != 0` (E = 0x030057E0; literal 0x08051298 = 0xC33).
  **confirmado (lido)**. Se E+0xC33 == 0 o ramo não faz nada; cai em 0x080512EC (andar com 0x08051CB8).
- Ramo B (0x08051210..0x08051290), na ordem: chama 0x08047E4C; grava gMainState 0x0300178C = 0; gamestate_menu
  0x03002B8C = 1; gamestate_subroutine 0x0300137C = 0; copia posições (0x03005000 / 0x03004C30 etc.); chama 0x080505FC;
  grava **E+0xCBA = 2**; volta a 0x0805132C. **confirmado (lido); o significado de gMainState=0/menu=1 aqui é
  pendente**: MN.md diz que `MainState0[1]` é a cena 1 (demo), o que não bate com "abre o menu". Não resolvi.
- Quem grava `gamestate_menu=2` com `gMainState=1`: **não é este ramo** (ele grava 1). Continua **pendente**.
- R novo (bit 8 de 0x0300058C) com E+0xC5C == 1 chama 0x08047BA8 (som); ver KI.md (E+0xC5C).

## 2. Handlers do sub-estado do mapa (tabela 0x08050F80, byte 0x03005311, ponteiro base 0x03005310)

Despacho `08050F5C`: `ldrb sub; cmp 4; bls; lsl 2; ldr tabela; mov pc,r0`. **confirmado**. Tabela (5 entradas, sem bit Thumb):

| sub | handler | o que faz (lido no código) | confiança |
|---|---|---|---|
| 0 | 0x08050F94 | chegada ao tile (E+0xC56/C57 via 0x08052C68). Com gatilho no tile -> sub 1 (0x08051026). Sem gatilho -> sub 2 (0x0805106A), e se `[0x03006D38]+3 == 4` chama 0x08047BA8(0). | confirmado |
| 1 | 0x08051078 | espera objeto (0x08051988 no byte do objeto 0x03002C30+0xBE0 e E+0xC58 == 0); depois sub += 1 (= 2). | confirmado |
| 2 | 0x080510C0 | **controle normal do mapa** (andar, A, B, R). Guardas: E+0xC58, E+0xC88, 0x08052400, 0x080521A8. Tile com gatilho -> sub 3 (0x08051118). Senão 0x08051CB8 (andar). Ver §1. | confirmado (estrutura), provável (nomes) |
| 3 | 0x0805134C | chama 0x08049E08(0x7FFF); quando != 0 -> sub 4. | confirmado |
| 4 | 0x08051368 | chama 0x08052E88; sub = 0; grava gMainState=1 (0x0300178C), gamestate_menu=1 (0x03002B8C), gamestate_subroutine=1 (0x0300137C), E+0xCBA = 1. Ou seja: **volta ao mapa normal** (saída do evento/menu). | confirmado |

Observação: E+0xCBA é um byte de modo do mapa: o ramo B grava 2, o sub 4 grava 1 (valor de volta). **provável** que 2 = "menu de campo aberto".

## 3. Despachante 0x0805716C e subs 1 e 2 do menu de escolha

- 0x0805716C não tem função no Ghidra. `disassemble_function` de 0x080574D5 também falha (o endereço com bit Thumb
  é 0x080574D4, que também não tem função). Sub 1 (0x08057440) tem função; sub 2 foi lido por halfwords.
- **Sub 1 = 0x08057440 Map_TickChoiceListOpen (confirmado no código)**: zera [0x030064A0+0x66]; se
  [0x030064A0+0x6F] <= 15 incrementa e desenha a janela (0x080492A8, altura 0x1E-contador); senão limpa
  [+0x65], chama 0x08048864(0x7F...), 0x08057FD4, 0x08047C00 e 0x0804A210 (fim de lista).
- **Sub 2 = 0x080574D4 (sem função no Ghidra, lido em halfwords)**: incrementa [0x030064A0+0x67]; chama 0x08057FD4 e
  0x08057F1C; lê a linha selecionada [+0x65]; chama 0x08057D80(linha) (**Overworld_CheckStepAllowed**: 1 = ativa);
  se inativa, sai; se ativa, toca som 0x08047BA8(6) e faz um switch em `r4-1` (0..5) pela tabela 0x08057534.

Ações por linha (tabela 0x08057534, ordem 0..5; o 0 e o 5 caem em blocos comuns):

| idx | destino | ação lida | confiança |
|---|---|---|---|
| 0 | 0x080575D4 | bloco comum de saída (epílogo) | provável |
| 1 | 0x0805754C | zera [0x030064A0+0x70]; **gamestate_subroutine = 0x0B** (0x0300137C); vai para o bloco comum | confirmado (lido) |
| 2 | 0x08057564 | **gamestate_subroutine = 0x0D** | confirmado (lido) |
| 3 | 0x08057570 | **gamestate_subroutine = 0x0F** | confirmado (lido) |
| 4 | 0x08057570 | igual ao idx 3 (sub 0x0F) | confirmado (lido) |
| 5 | 0x08057570 | igual ao idx 3 (sub 0x0F) | confirmado (lido) |

Bloco comum (0x080575AC..): `0x080575C2: mov r0,#0x7F; BL 0x08048888`; grava [base+0x6F] = 0x10; `BL 0x0804A210`
(fim = volta ao mapa?). **provável**.

Mapeamento linha -> opção: **pendente**. As linhas do menu (0..3 ativas por 0x08057D80) e os índices 0..5 do switch
não batem diretamente (o switch tem 6 casos, o menu 4 linhas). Não confirmei qual idx é MEDAMATCH/ITEM/VEÍCULO/SALVAR.
Hipótese (provável, não testada): sub 0x0B/0x0D/0x0F são subestados das telas de MEDAMATCH/ITEM/SALVAR.
A tabela 0x083D5FF4 só foi lida até sub 2; 0x0B, 0x0D e 0x0F estão além do que conheço => **pendente**: ler a tabela até 0x0F.

## 4. Quem grava gamestate_menu=4 com gMainState=2 (bolsa de key items)

- **Pendente.** Não rodei xref de 0x03002B8C (131 pontos de literal, custo alto). Nada novo confirmado nesta rodada.
- Lado da bolsa (já em KI.md): MainState2 [4] = Dispatch_StateTableFrameCount 0x0805CB50.

## 5. Pendente

1. Gravador de `gamestate_menu=2` com gMainState=1 (menu de campo de 4 linhas). Não é o ramo B de §1 (grava 1).
2. Significado de gMainState=0/gamestate_menu=1 do ramo B (§1): ver MS0[1] e 0x080505FC.
3. Tabela 0x083D5FF4 além de sub 2 (valores 0x0B, 0x0D, 0x0F) e mapa linha -> opção.
4. Gravador de gamestate_menu=4 (bolsa).
5. Função de 0x0805716C (ainda sem função no Ghidra).

## 6. Renomeações

Nenhuma no Ghidra nesta rodada (os handlers do mapa e o sub 2 do menu não têm função própria no Ghidra;
criar função não foi feito por não estar confirmado o bastante). Nomes propostos ficam no CSV.
