# DC - correções aplicadas em documentos antigos (GH-DC, 2026-10-14)

Cada correção cita a fonte (agente posterior) entre parênteses. Só os arquivos abaixo foram editados.

| Arquivo : seção | Mudança |
|---|---|
| DM.md §5 (cursor do roteiro) | LEN real: LEN[0x02]=1, LEN[0x03]=4, LEN[0x13]=0, demais 0 (AS3a/AS3b/TQ2). Antes dizia 0x13 e 0x03 = 1 byte. |
| AS2.md, "Ponto importante" | Marcado como superado: handlers 0x36..0x6A agora são funções, nomes em AS3c.md (AS3c). Tabela da seção ainda diz "sem função"; ler AS3c. |
| AS2.md, LEN | Nada a corrigir: AS2 não tem a tabela LEN (ela está só em DM.md §5 e AS3a/AS3b). |
| FI.md, tabela 1, +0x028 e +0x02A | Um só array de 8 B em fi+0x2A + 8*k; +0x028 não é campo separado (FI2, Conflito 2). |
| FI.md, tabela 1, +0x084 "contador de slots" | Linha trocada por nota: não é fi+0x84; é ctx 0x03000440+0x84 (FI2, Conflito 7). |
| FI.md, Conflitos | "+0x028 x +0x02A" marcado resolvido; "+0x0A8" resolvido: id da técnica/roteiro, índice de 0x083C6CA8 (FI2, Conflito 3). |
| HA4.md, S_09 / S_0A / (passo com timer) | "fi+0x86 (timer)" -> timer 0x03000440+0x86 (ctx) (FI, FI2). |
| HD.md | Nada a corrigir: HD.md não contém "fi+0x3C". O estado do cursor em 0x03000440+0x3C/0x3D já está em HD2.md (HD2). |
| RW.md, §3/§6 (fi+0x9C) | Vencedor = 0x030004DC (T+0x9C, ctx), não fi+0x9C (DF). Valor exato segue em aberto. |
| IN.md, Shop_ItemListInputStep e FUN_08048960 | FUN_08048960 = KeyItem_CanUseHere, não classificador de batalha; Shop_* de itens = bolsa de key items (IN2). |
| OW.md, tabela de endereços 0x03006D3A | Índice na tabela de velocidade 0x083D2A40, não multiplicador (OW2). |
| OW.md, colisão de tile e "quem chama CheckStepAllowed" | Overworld_CheckStepAllowed é do menu de 4 linhas (OW2, FL7), não da colisão. |
| EV.md, carga de spawns | Entrada = 0x080513C8; 0x08051416 é o miolo; sem tabela de ponteiros por grupo; ponteiro = u32[0x08483234 + 4*gCurrentMapId] (EX). |
| MS.md, FF 1/4 | ctx+4++ do mesmo ctx 0x03000440, sem SFX (MS2). |
| MS.md, Text_TypewriterRowStep | Nada a mudar: MS.md já diz que não é a impressora (MS). |
| SN.md, cabeçalho da amostra PCM | Byte +6 = nota-raiz (0x3C); +7 = flag 0 (SN2). |
| AI.md, Fight_IsMedaforceSelectionBlockedByBattleState 0x08015648 | Nome atual Fight_IsMedaforceBlockedByChapter; bloqueia se bot >= 3 e gCurrentChapter (0x03005FDF) <= 2 (AI2). Antes dizia > 2. |
