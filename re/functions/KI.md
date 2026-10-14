# GH-KI: key items (bolsa, uso, eventos) — Medabots Metabee EU

Status: EM ANDAMENTO (2026-10-14). Salvo a cada passo. Continua IN.md / IN2.md.

## Passo 0: ponto de partida
- Bolsa de key items = Map64 0x03002BA0 (64 pares), texto gTextKeyItems 0x08483ED8 (64 ponteiros) (IN2, confirmado).
- Pendente vindo de IN2: (1) miolo de KeyItemBag_ListInputStep (0x0805D2C8); (2) FUN_08048960 (conflito: IN diz
  classificador de batalha, DT3 diz que lê gMapArea[mapa] e devolve 2 para passo negado); (3) abertura pelo menu de
  campo (base 0x08483594); (4) eventos 0x4B.

## Passo 1: FUN_08048960 (decompile, confirmado no código)
- NAO e classificador de batalha. Switch sobre o ARG = id do key item (mesmos ids da tabela 0x0805D2C8: 1,5,12,13,19,39,40).
  Devolve: 0 = id sem regra (default); 1 = pode usar; 2 = bloqueado pela área do mapa (0x0A em gMapArea[mapa], lido
  com DAT_08048a40+mapa); 3 = caso geral (flag 0x20 em DAT_08048a4c); 4 = bloqueio por flag/limite (ids 1,5,C,D,13h,27h,28h).
- Chamador (Shop_ItemListInputStep): 0 -> SFX 6, nada; 1 -> SFX 4 e despacho; 2..4 -> SFX 6 + estado 9 (mensagem de recusa).
- Portanto o nome FUN_08048960 vira "KeyItem_CanUseHere" (provável). Confirmar literais DAT_08048a40..a80 (passo 2).

## Passo 2: miolo de KeyItemBag_ListInputStep (0x0805CFB4, lido no disasm, literais lidos na ROM)
- CORRECAO de IN2: tabela 0x0805D2C8 tem 41 entradas (0..0x28). Entrada 0x28 (id 40) = 0x0805D3FC (não e palavra solta).
- Após FUN_08048960 == 1 e id <= 0x28: SFX 4, depois despacho pela entrada do id.
- Caminhos (literais: E = 0x030057E0 bloco de evento; GS = 0x0300137C; ctx = 0x03000440):
  * id 0 (0x0805D36C): zera o byte apontado por [0x03006F74]->+0 (struct do cursor da bolsa) e GS = 0x0B. Sem efeito de evento.
  * id 1 (0x0805D384): E+0xC5C = 3 ; id 5 (0x0805D398): E+0xC5C = 4 ; id 12 (0x0805D3AC): E+0xC5C = 5
  * id 13 (0x0805D3C0): E+0xC5C = 8 ; id 19 (0x0805D3D4): E+0xC5C = 6 ; id 39 (0x0805D3E8): E+0xC5C = 7
  * id 40 (0x0805D3FC): E+0xC5C = 0x0D
  * Comum a 1,5,12,13,19,39,40 (0x0805D418): ctx+0x84 (0x030004C4) = 0 ; GS = 0x0D (estado de uso de key item, a confirmar).
  * demais ids 2..4, 6..11, 14..18, 20..38 (0x0805D410): MainState_RequestModeTransition(4) = sai da bolsa sem efeito.
- Valores 3,4,5,6,7,8,0x0D em E+0xC5C = modo do uso (provavel: qual comportamento de campo/evento). Quem le E+0xC5C: pendente.
- Literais: 0x0805D2C4 = 0x0805D2C8 (tabela); 0x0805D088 -> struct 0x03006F74; 0x0805D08C etc. (cursor/estado da bolsa).

## Passo 3: como a bolsa abre pelo menu (parcial)
- Tabela 0x08483594 = MainState2 por gamestate_menu (confirmado no ROM): [3]=0x0805C0D4, [4]=0x0805CB50 (despachante da bolsa),
  [5]=0x0805D704, [6]=0x0805E404, [7]=0x0805F104, [8]=0x0805FC58, [9]=0x08060280, [10]=0x08060688, [11]=0x08060D6C, [12]=0x08061958.
  Indice 4 = 0x084835A4 (= base + 0x10: por isso "0x084835A4" aparece em IN2 e e o MESMO menu 4 da MainState2).
- Dispatch_StateTableFrameCount 0x0805CB50 indexa por gamestate_subroutine (0x0300137C) a tabela 0x08483634:
  [2]=0x0805CDAC KeyItemBag_InitListScreen ; [5]=0x0805CFB4 KeyItemBag_ListInputStep (confirmado pelas duas tabelas).
- Quem grava gamestate_menu=4 com gMainState=2 (a entrada do campo para a bolsa): NAO ACHADO. Xrefs de 0x03002B8C:
  escritas em AgbMain, SoftResetSequence, MainState2 (0x0805BC12 = gravacao do menu 10 da falha de link) e
  Screen_ResetGfxAndState 0x08044200. GameState_StartFlowByCommand (0x08047878) NAO aparece como escritor direto;
  a lista de cmd (MN.md 2.1) nao tem cmd que resulte em MainState2 menu 4. PENDENTE.
- Campo: o menu do START nao foi decodificado aqui (MainState1 menu 1 = EventScript_RunCurrent).

## Passo 4: eventos que checam key item (op 0x4B = Jump_If_Item_Count_Below_Amount, args Byte id, Byte qty, Byte salto)
- Varredura (pc/extract/extract_events.py importado sem edicao; decode_event BFS, 2288 eventos). Script:
  Script de varredura apagado (rascunho temporario). Salta se contagem de id < qty (qty 0 = checa só posse; ver IN.md).
- Ids de key item usados (id: número de usos em eventos distintos):
  0x02:2 0x03:1 0x04:3 (qty 2,3,4) 0x06:2 0x09:1 0x0A:2 0x0B:1 0x0E:1 0x10:3 0x11:1 0x14:1 0x15:1 0x16:1
  0x1A:1 0x1B:2 0x1D:1 0x20:1 0x21:1 0x24:2 (qty 1,2) 0x29:1 0x2A:1 0x2C:6
- Ressalva: a varredura cobre só o que o BFS alcança a partir de cada evento; eventos com opcode desconhecido podem ter cortado o caminho.
- Ids acima de 0x28 (40) existem nos scripts (0x2C): não passam pelo despacho de uso (cai em MainState_RequestModeTransition(4)).

## Arquivos
- Correcao do classificador (ASM 0x08048960): case 0 usa E+0xC5D bit 0x20 e gMapArea; ids 1,5,C,D,13h,27h,28h usam E+0xC5D bit 0 e E+0xC5C.
- KI_simbolos.csv (nomes/comentarios), KI_lista.txt (miolo sem funcao no Ghidra). IN2.md: correcao da palavra 0x0805D3FC (e id 40).
- Pendente: quem le E+0xC5C (0x0300643C) e o que faz GS=0x0D (estado de uso); entrada da bolsa pelo menu de campo (gamestate_menu=4 em MainState2); escritores de gamestate_menu=4.
