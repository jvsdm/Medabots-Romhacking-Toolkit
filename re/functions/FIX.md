# FIX: correções de notas antigas (GH-FIX, 2026-10-13)

Fontes: JT.md (saltos indiretos), HC1..HC5.md e HC*_simbolos.csv (continuações de evento), HE4 e HC5.md (0x76, 0x7A, 0x7F).
Conferido no Ghidra só o nome e a função de 0x0804FC7C (Event_Op7F_Cont_Copy_Tracked_Actor_Records) e 0x0804DC34 (Event_Op00_Cont_NoOp).

## Mudanças

1. "Tail-call/salto por ponteiro não recuperado" trocado por retorno normal (JT.md):
   - HA1.md, seção `0x0801D9DC E/05` (confiança e fluxo) e seção "Em aberto" item (2).
   - HA2.md, seção E05 (0x0801DADE), seção G02 (0x0801DFE0, etapas e confiança), bloco "Redo" (linhas ~459-463).
   - HA3.md, seção H:0x3E (0x0801F214, retorno em 0x0801F284..28A), seção L:0x0B (0x0802036C, retorno em 0x0802040A..0C), seção "Em aberto".
   - Próximos passos corretos (JT.md): E05 -> E06 (ao chegar), G02 -> G03 (só na etapa 2), H:0x3E -> 0x3F, L:0x0B -> 0x0C.
2. HA3.md, seção J:0x02 (0x0801FBC4): é tabela real de 5 entradas em 0x0801FBEC, seletor 0x03000579; 0x0801FD70 é retorno, não alvo (JT.md). Seção "Em aberto" corrigida.
3. HE4.md, seção 0x70/0x71/0x72/0x73/0x76: 0x76 checa estado 7 uma vez, não espera (HC5.md, 0x0804FB1C).
4. HE4.md, seção 0x7A: grava 1 em 0x03005349 (0x03005310+0x39) se 0x0804B738 != 0; não zera 0x03005310 (HC5.md, 0x0804FC1C).
5. HE4.md, seção 0x7F: fontes 0x03003FF0+arg0*0x40 e 0x03004A70+arg0*0x40, destinos 0x03003830 e 0x030042B0 (HC5.md, 0x0804FC7C). Deixou de citar "0x03002C30 e E+0xC0C".
6. HE2.md, seção 0x32: a ordem sim/não vem do literal da chamada (FUN_08048000(1)), não de E+0xCB5 (HC3.md, 0x0804E860).
7. HE1.md, seção 0x02: a continuação 0x0804DCF8 só chama Event_EndOpcode (HC1.md); a caixa do 0x02 fica a confirmar no emulador.
8. HC.md, tabela: para os 55 alvos documentados em HC1..HC5 (HC_lista.txt), coluna "já é função?" = sim e nome = nome de HCn_simbolos.csv, com o nome mtbe anterior entre parênteses. Linha de resumo atualizada (85 alvos únicos; 30 já eram função; 55 nomeados nesta rodada). Ainda sem alvo fora de função.

## Não tocado
- mtbe.csv, commits, emulador, ROMs.
- Pendências de conteúdo (ex.: 0x0804F5C8..0x0804F6E0 e demais "chute" nas HC) continuam como estavam.
