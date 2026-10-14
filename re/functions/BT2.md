# BT2: fechamento das lacunas do fluxo da Robattle (GH-BT2, 2026-10-14)

Continuação de `BT.md` (não editado). Confiança: "confirmado" = lido no decompile ou nos bytes da ROM EU
(`Medabots - Metabee (Europe).gba`, só leitura); "provável" = padrão de uso; "lacuna" = não fechado.
Nenhum emulador usado. Nada commitado.

## Lacuna 2: tabelas 0x083AD190 e 0x083AD10C (CONFIRMADO o conteúdo, quem indexa = LACUNA)

- Busca de literal u32 em toda a ROM (python -I): `0x083AD190` e `0x083AD10C` (e as versões com bit thumb):
  **0 ocorrências**. Não existe ponteiro literal para essas tabelas.
- Bytes da ROM (file offset = addr - 0x08000000), lidos como ponteiros:
  - 0x083AD190 (Fight_StepBattleFrame): `0801427D, 08014315, 080143A1, 08014405, 08000841 x4, 08014549, 08000841`
    -> entrada 0 = Fight_StepBattleFrame 0x0801427C (já sabido); entrada 8 = 0x08014548 (BT.md).
  - 0x083AD10C (Fight_BattleSceneInit): `08013B8D, 08013DE5, 08013E31, 08013FA9, 08000841, 08013615, 08013651, 080136B1`
    -> entrada 0 = Fight_BattleSceneInit 0x08013B8C.
  - 0x083AD080 (Battle_FrameDispatchOrExit, indexada por GS): `080129C9, 08012A21, 08012B41, 08012B95, 08012BC1, ...`
- 0x08000841 é sentinela repetida (entrada vazia/nop), não função.
- Hipótese (não confirmada): as duas tabelas podem ser alcançadas como base 0x083AD080 + 0x110 (= 0x083AD190) e
  0x083AD080 + 0x8C (= 0x083AD10C), via soma com constante, sem literal direto. Não testado.
- Xrefs do Ghidra (get_xrefs_to) voltaram vazias para 0x083AD190, 0x083AD10C, 0x0801427C, 0x08013B8C e 0x08013904.
  O MCP de xrefs não resolve nesta sessão; usar busca de bytes/decompile.

## Menus 6 e 10 (CONFIRMADO o código; sem função no Ghidra, lido por desmonte da ROM)

- menu_list[6] = 0x08010265 (thumb de 0x08010264). Despachante genérico por GS: se 0x03000430 == 2 faz DMA de
  limpeza de 0x200 halfwords; CallViaR0(tabela 0x083ACE24[GS]) (helper 0x0807CCF0). No fim zera 0x03000440+0x85.
  Não chama Fight_*. Nome sugerido: `Scene_GsDispatch_Menu6`. Ainda NÃO é "volta ao mapa" comprovado.
- menu_list[10] = 0x0803D539 (thumb de 0x0803D538). Se (GS-0x10) > 0xF chama 0x0803EED8 (se der != 0, sai).
  Mesmo esquema: limpeza com 0x03000430 == 2, 0x0803E780, CallViaR0(tabela 0x083AFC78[GS]). Nome sugerido:
  `Scene_GsDispatch_Menu10_Link`. Coerente com "saída link" do BT.md, não confirmado.

## Despachante de estado da luta (CONFIRMADO)

- Fight_RunStateDispatch 0x0801A788: se byte > 8 sai; senão CallViaR0(tabela 0x0801A7B4[byte]).
- Tabela 0x0801A7B4 (lida da ROM): 0:0801A7D8, 1:0801A818, 2:0801A85C, 3:0801A8BC, 4:0801A910, 5:0801A950,
  6:0801A958, 7:0801A998, 8:0801A9C8. (A entrada 9 é outro dado, não é estado.)
- state8 0x0801A9C8 salta para 0x0801AADE (BT.md). Não decompilado nesta rodada.
- Ordem de transição 1..6: LACUNA. Só os handlers foram listados; o avanço do byte 0x13A não foi localizado.

## Quem grava gamestate_menu (0x03002B8C) = 7 ou 8 ao entrar na luta (PARCIAL)

- A busca de literal 0x03002B8C achou 131 pontos de referência na ROM (pool de literal).
- Gfx_ResetScreenBgAndBlend (decompile, literal 0x08011C80): grava `*(0x03002B8C) = 8` e `*(0x08011C7C) = 2`.
  É rotina de reset de tela. PROVÁVEL escritor de menu=8, mas NÃO confirmado que seja a entrada da luta vinda do
  mapa. Chamadores não achados (get_xrefs_to vazio).
- Ponto 0x08014388 (dentro do intervalo de Fight_StepBattleFrame): grava 8 em gamestate_menu e 0 em 0x0300137C.
  Não decompilado.
- Escritor de 7 (entrada da luta): NÃO fechado. 0x08007DAA aparece no desmonte com `movs r0,#7; str` mas o
  decompile de 0x08007DAA (Tile_ClearSlotsAndDispatchState) não mostra essa escrita. Conflito, não resolvido.
- Hipótese para testar: o evento 0x33 Start_Battle (README/eventos) chama a rotina de entrada. Não verificado.

## Lacunas que NÃO foram fechadas nesta rodada (Pendente)

- Lacuna 1 (entrada menu 7/8): parcial, ver acima.
- Lacuna 2 (quem indexa 0x083AD190 e 0x083AD10C): conteúdo confirmado, quem indexa = PENDENTE. 0 literais.
- Lacuna 3 (ligação 0x083AD080 x 0x083AD190): PENDENTE (hipótese base+0x110 / +0x8C, não testada).
- Lacuna 4 (quem grava 0x03000579 e 0x0300057A): PENDENTE. Tem 944 literais de 0x03000440; não varrido.
- Lacuna 5 (ordem de transição 1..6): PENDENTE.
- Lacuna 6 (state8 -> 0x0801AADE): PENDENTE (não decompilado).
- Lacuna 7 (menu 6 e 10): parcial, ver acima.
- Lacuna 8 (bot+0x40 / BR_50): PENDENTE.
- Lacuna 9 (cabeça quebrada = derrota): PENDENTE.
- Lacuna 10 (gauge cheio -> pode agir): PENDENTE.
- Renames no Ghidra: NENHUM. Os menus 6/10 não têm função no Ghidra (rename exigiria criar função) e o resto
  não foi confirmado.
