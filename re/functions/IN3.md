# GH-IN3: efeito de cada item da bolsa (jump table 0x0805D2C8) — Medabots Metabee EU

Status: CONCLUIDO (2026-10-14). Ghidra: leitura + 7 renames. ROM: so leitura via python -I (literais e tabelas).
Sem emulador, sem commit, mtbe.csv nao editado, pc/research/README.md nao editado (escopo da tarefa).
Simbolos: `IN3_simbolos.csv`. Codigo sem funcao: `IN3_criar.txt`.

## 1. Indice e tamanho da tabela [confirmado]

- Id do item: halfword (u16) em 0x02040000 + ([ptr 0x03006F74]+0x0C)*4. A leitura fica em Shop_ItemListInputStep
  0x0805D230..0x0805D23C (o id vai para KeyItem_CanUseHere em r0).
- Guarda: `cmp r0,#0x28 / bls` (0x0805D2B4). Logo ids 0..0x28 = **41 entradas** (nao 40). A tabela vai de
  0x0805D2C8 a 0x0805D36B; a palavra em 0x0805D36C ja e codigo (Thumb 0x6801 0x4803), e tambem e o alvo do id 0.
  IN.md/IN2.md/mtbe.csv dizem "0x28 entradas": o limite esta certo, a contagem e 41.
- Alvos (ROM, 41 ponteiros): 0->0x0805D36C; 1->0x0805D384; 5->0x0805D398; 12->0x0805D3AC; 13->0x0805D3C0;
  19->0x0805D3D4; 39->0x0805D3E8; 40->0x0805D3FC; 2-4, 6-11, 14-18, 20-38 -> 0x0805D410.
- **0x0805D410 (default) nao e alcancado.** Quem chama KeyItem_CanUseHere so vai para a tabela quando o retorno e 1,
  e o retorno 1 so sai para id 0 e para os ids 1,5,12,13,19,39,40 (ver §2). Logo o default e codigo morto nesta
  rotina. A conclusao de KI.md ("default = MainState_RequestModeTransition(4), sai sem efeito") estava errada: a saida
  por MainState_RequestModeTransition(4) acontece no FIM do fade do uso (§3, estado 0x0E).

## 2. Classificador KeyItem_CanUseHere 0x08048960 (retorno em r0) [confirmado, asm + literais]

Ordem exata (corrige a linha de mtbe.csv 2682, que inverte a ordem do id 0):
- **id 0**: se gMapArea[gCurrentMapId] == 0x0A -> 2; senao se E+0xC5D & 0x20 -> 3; senao -> 1.
  gMapArea = ROM 0x083D2B6B + E+0x7FE (gCurrentMapId).
- **ids 1,5,12,13,19,39,40**: se E+0xC5D & 0x01 -> 3; senao se o byte E+0xC5C esta em {1,2} -> 4; senao -> 1.
  (teste: (byte)(E+0xC5C - 1) > 1 -> 1, senao 4.)
- **demais ids** (2-4, 6-11, 14-18, 20-38 e tudo > 0x28): default -> 0.
- Literais usados: 0x08048A40 = 0x083D2B6B (gMapArea), 0x08048A44 = E (0x030057E0), 0x08048A48 = 0x7FE,
  0x08048A4C = 0xC5D, 0x08048A78/7C/80 = E, 0xC5D, 0xC5C. Ghidra/mtbe chamam E+0xC5D de gOverworldEventModeFlags
  (0x0300643D); E+0xC5C = gMapPlayerWalkState (0x0300643C).

## 3. Efeito por id [confirmado no asm do miolo de Shop_ItemListInputStep = KeyItemBag_ListInputStep 0x0805CFB4]

Caminho comum (retorno 1): SFX 4 (0x08074DE0) -> despacho pela tabela.
Retorno 0: so SFX 6 (0x0805D430), sem troca de estado. Retorno 2..4: SFX 6, limpa bit 0x4000 de dois atributos
OBJ do cursor (0x03004070 e 0x030040B0, provavel), GS=9 (0x0805D2A0..0x0805D290). Retorno 2..4 nao leva id de
mensagem: o codigo 2/3/4 fica so em r0 e 0x0803FB0C (Obj_ResetStateFields) nao recebe argumento. A mensagem de
recusa, se existir, sai do estado 9 (Flow_StepCounterTickB, que chama StateMachine_DispatchStep no objeto
DAT_0805D59C+0x20). Id da mensagem: NAO CONFIRMADO.

| id | Retorno que chega ao despacho | Efeito | RAM/flags tocados | Mensagem |
|---|---|---|---|---|
| 0 | 1 (usa) | NAO muda E+0xC5C. Zera o byte em [0x03006F74]->+0 (estado do cursor). GS=0x0B -> sub 0x0B = Fx_SetBldcntAndAdvance (REG_BLDCNT=0x2FF, avanca). Sub 0x0C = 0x0805D5F1, sem funcao no Ghidra: fim NAO confirmado. | 0x03006F74, GS 0x0300137C, REG_BLDCNT | — |
| 0 | 2 | recusa: mapa com gMapArea==0x0A (SFX 6, GS=9) | ROM 0x083D2B6B | codigo 2 (nao e id) |
| 0 | 3 | recusa: flag E+0xC5D bit 0x20 (SFX 6, GS=9) | 0x0300643D | codigo 3 |
| 1 | 3 / 4 / 1 | 1: E+0xC5C=**3**. 3 se bit 0x01 de E+0xC5D; 4 se E+0xC5C in {1,2} (recusa) | E+0xC5C 0x0300643C; E+0xC5D; ctx+0x84 | nenhuma (1 nao mostra) |
| 5 | 3 / 4 / 1 | 1: E+0xC5C=**4** (mesma condicao de recusa do id 1) | idem | idem |
| 12 | 3 / 4 / 1 | 1: E+0xC5C=**5** | idem | idem |
| 13 | 3 / 4 / 1 | 1: E+0xC5C=**8** | idem | idem |
| 19 | 3 / 4 / 1 | 1: E+0xC5C=**6** | idem | idem |
| 39 | 3 / 4 / 1 | 1: E+0xC5C=**7** | idem | idem |
| 40 | 3 / 4 / 1 | 1: E+0xC5C=**0x0D** | idem | idem |
| 2-4, 6-11, 14-18, 20-38 | 0 | so SFX 6. Nada muda (sem saida, sem troca de estado) | — | — |

Caminho de uso (ids 1,5,12,13,19,39,40), depois de gravar E+0xC5C:
1. ctx+0x84 (0x030004C4) = 0; GS = 0x0D.
2. Sub 0x0D = StateTable_WaitSubThenAdvance_B 0x0805D658: espera a sub-maquina de cena e avanca.
3. Sub 0x0E = Screen_TickFadeOutAndClear 0x0805D674 [provavel]: fade de 9 passos (contador ctx+0x84), limpa VRAM.
4. Fim do fade: MainState_RequestModeTransition(4) 0x080476E8 — sai da bolsa.
Na saida, o campo passa a ler o novo E+0xC5C. Quem le E+0xC5C e como (velocidade/animacao) segue no VA3.

Modos gravados (3,4,5,6,7,8,0x0D): ja estao nas tabelas de VA3 (§4).

## 4. O que o modo muda (VA3, conferido nesta rodada)

- Tabela A (velocidade por modo, VA3 0x083D2A88). Ghidra/mtbe citam 0x083D2A30 para o mesmo indice (Move_Player,
  HE1). Confere-se na ROM: as duas sao iguais exceto no indice 11 (0x02 em 0x083D2A88, 0x00 em 0x083D2A30).
  Os modos dos itens (3..8 e 0x0D) valem 0 nas DUAS tabelas. Ou seja: se A for mesmo velocidade, esses modos
  parariam o jogador; isso nao bate com "item de movimento". Consumidor de 0x0300381A (gPlayerSpeedIndex) a conferir.
- Tabela B (passabilidade) e indexada por E+0xC28, nao por E+0xC5C: os itens NAO mudam passabilidade direto.
- Os modos 1 e 2 sao os dois que os toggles de VA3 alternam; a recusa do id 1..40 (retorno 4) acontece justo quando
  o jogador ja esta em 1 ou 2.

## 5. Renames aplicados no Ghidra (confirmado: "Function renamed successfully")

| Antes | Depois | Motivo |
|---|---|---|
| Shop_ItemListInputStep 0x0805CFB4 | KeyItemBag_ListInputStep | entrada 5 da tabela 0x08483634 (bolsa) |
| Shop_InitItemListScreen 0x0805CDAC | KeyItemBag_InitListScreen | entrada 2 da mesma tabela |
| Shop_UpdateSlotAndDispatchState 0x0805CF58 | KeyItemBag_UpdateSlotAndDispatchState | entrada 4 da mesma tabela |
| Shop_BuildItemList64 0x0805AE4C | KeyItemBag_BuildOwnedList | copia so os itens possuidos (bit 0x80) do Map64 |
| Shop_DrawItemListPage 0x0805AED0 | KeyItemBag_DrawListPage | chamado por InitListScreen e ListInputStep |
| Shop_DrawSelectedNameText 0x0805B594 | KeyItemBag_DrawSelectedNameText | chamado por ListInputStep (provavel) |
| Shop_HandleItemListAndCounts 0x0800C804 | Debug_KeyItemCountEditorStep | so na tabela de debug 0x083ACAC0 (idx 12) |

Mantidos com "Shop_" (sao de loja de verdade ou nao confirmados): Shop_SetSlotHighlight 0x0805CB90 (usado tambem
fora da bolsa, nao verificado), Shop_TickBuyQuantityMenu, Shop_CommitBuySellTransaction, Shop_InitPartSelectionScreen,
Shop_RefreshPartListScreen, Shop_FadeInStep e os demais de compra/venda.
Obs.: o buffer gShopItems (0x02000000, espelho 0x02040000) e reaproveitado pela bolsa para a lista de ids. O nome
"gShopItems" e enganoso para esse uso: sugestao de rename futuro (nao feito).

## 6. Pendencias

- Mensagens de recusa: nenhum id de mensagem confirmado (§3). Falta seguir StateMachine_DispatchStep no objeto
  DAT_0805D59C+0x20 ou a funcao que monta o texto no estado 9.
- Sub 0x0C (0x0805D5F1, continuacao do id 0): sem funcao no Ghidra (ver IN3_criar.txt).
- Significado de E+0xC5D bits 0x01 e 0x20 (flags de evento/mapa): so o uso foi mapeado, nao a origem.
- Consumidor de E+0xC5C 3..8 e 0x0D (velocidade/animacao): confirmar com tabela A (§4).
- Pedidos de correcao em mtbe.csv (nao editado): linha 3194/3195 (41 entradas, nao 0x28); linha 2682 (ordem id 0);
  linha 3190/3193 (renames aplicados, nomes ainda Shop_ no CSV); 0x0805D36C (ItemListSwitch_Table -> KeyItemBag_Case_Id0).
