# SU2: passos pendentes de SU.md (+0xAA) decompilados no Ghidra

Data: 2026-10-14. Fonte: decompile do Ghidra (somente leitura; nada renomeado, nada salvo). Gancho: 0x03001800 +0xAA = passo; +0xB5 = handler concluído.
Terminação: a sequência só fecha quando o passo chega ao SetDone (0x0803080C = `Fight_DispatchOnSubstateAA_Shared_SetDone`, grava +0xB5=1). Passos SetDone de cada despachante: S 03/12/19/29, U 03/0F, V 03/08, W 03/0A, X 03, Y 03, AC 03, AD 03 (e demais). Nenhum passo pendente grava +0xB5 diretamente; "Encerra" abaixo = o passo leva ao SetDone só por cadeia (não direto).

Correções ao SU.md (confirmadas no decompile):
- S 0x00 não chama `Fight_ApplyStatusCostIfFlag`; chama `Fight_ApplyVirusBugRosPenalty` e `Fight_SpendRos20ReturnsExhausted`.
- AC 0x00 e AD 0x00 com ROS ok vão para +0xAA=4 (não +1). AD 0x00 tem bloqueio (msg 0x39) antes disso.
- S 0x0C fracasso em modo != 2 vai para 0x0E (com SFX 0x3E), não "0x14".
- X 0x09/0x0A/0x0B/0x0D e Y 0x0C: SFX do X 0x0C é 0x4A (Y 0x0C = X 0x0C com SFX 0x4B, segundo SU.md).

Ressalva geral: S 0x0F, S 0x16, S 0x26 (Fight_DispatchOnSubstateAA_S_0F, endereço 0x08021418) e S 0x10, S 0x17, S 0x27 (0x08021564) fazem +0xAA+1 e depois chamam um ponteiro de tabela não recuperado pelo Ghidra (0x0802155C, "Too many branches"). O +1 é o que o passo grava; se a função chamada reescrever +0xAA, o destino muda. Não resolvido.

## Passos pendentes (decompilados)

| Passo | Endereço | Próximo +0xAA | Condição | Encerra? | Nota |
|---|---|---|---|---|---|
| S 0x00 | 0x08020D24 | 0x04 (passo ok); +1 (0x01) se ROS esgotado | ROS esgotado -> msg 0x1A | não | Define modo +0x51 pela técnica (10->0, 11->1, 12->2, outros->3) e +0x52 = técnica |
| S 0x04 | 0x08020DF8 | +1 (0x05) | fixo | não | gfx + sprite do alvo, SFX 0x29 |
| S 0x05 | 0x08020EDC | +1 (0x06) | fixo (os dois ramos somam 1) | não | anima sprite |
| S 0x06 | 0x08020FB8 | +1 (0x07) | fixo | não | gfx + Slot_SetupFromRecordList(lista+0x8C, 0x21) |
| S 0x07 | 0x08021000 | +1 (0x08) se anim == -1; senão fica | condicional | não | espera slot 0x21 |
| S 0x08 | 0x08021038 | +1 (0x09) | fixo | não | gfx por modo (0/1/2/3), timer 0x3C |
| S 0x09 | 0x080211C8 | +1 (0x0A) quando byte chega a 0x40; senão fica | condicional | não | timer = 1 (modo 3) ou 0x3C |
| S 0x0A | 0x0802121C | modo<3: +1 (0x0B); modo 3: 0x20 (S 0x20, com SFX 0x52); senão fica | condicional (timer) | não | |
| S 0x0B | 0x080212A0 | +1 (0x0C) se byte == 0x10; senão move 1 px e fica | condicional | não | |
| S 0x0C | 0x08021300 | flag ok: +1 (0x0D); falha: modo 2 -> 0x14; senão SFX 0x3E -> 0x0E | 3 vias | não | |
| S 0x0D | 0x08021370 | byte==0x10: modo 2 -> 0x14, senão SFX 0x3E -> 0x0E; byte!=0x10: byte ±8, depois 0x0E | 2 vias | não | |
| S 0x0E | 0x080213E4 | +1 (0x0F) quando byte == 8 (limpa slot 0x21); senão -8 e fica | condicional | não | |
| S 0x0F | 0x08021418 | +1 (0x10) | fixo + ressalva do ponteiro | não | registro de dano no alvo (ver S) |
| S 0x10 | 0x08021564 | +1 (0x11) | fixo | não | msg 0x1E/1F/20/21 por modo; +0x53 != 0 -> 0x71 |
| S 0x14 | 0x080215C8 | +1 (0x15) | fixo | não | byte de estado = 4, zera +0x870, SFX 0x40 |
| S 0x15 | 0x08021608 | +1 (0x16) quando o passo de coordenada chega no limite; senão fica | condicional | não | limpa slot 0x21 ao chegar |
| S 0x16 | 0x08021418 | +1 (0x17) | fixo + ressalva | não | igual S 0x0F |
| S 0x17 | 0x08021564 | +1 (0x18) | fixo | não | igual S 0x10 |
| S 0x20 | 0x08021638 | +1 (0x21) com timer 0x3C quando anim == -1; senão fica | condicional | não | |
| S 0x21 | 0x0802167C | +1 (0x22) quando timer zera (copia tiles, posiciona sprites 0x58/0x40/0x28); senão fica | condicional | não | |
| S 0x22 | 0x08021734 | +1 (0x23) quando os 3 slots (0x21..0x23) retornam != 0; senão fica | condicional | não | |
| S 0x23 | 0x08021774 | flag ok: +1 (0x24); falha: SFX 0x3E e 0x25 | 2 vias | não | |
| S 0x24 | 0x080217D8 | +1 (0x25) | fixo | não | move sprite 8 px; SFX 0x3E se byte == 0x10 |
| S 0x25 | 0x0802186C | +1 (0x26) quando byte == 8 (limpa slots 0x21..0x23); senão -8 e fica | condicional | não | |
| S 0x26 | 0x08021418 | +1 (0x27) | fixo + ressalva | não | igual S 0x0F |
| S 0x27 | 0x08021564 | +1 (0x28) | fixo | não | igual S 0x10 |
| U 0x04 | 0x08020DF8 | +1 (0x05) | fixo | não | = S 0x04 (compartilhado) |
| U 0x05 | 0x08020EDC | +1 (0x06) | fixo | não | = S 0x05 |
| U 0x06 | 0x08020FB8 | +1 (0x07) | fixo | não | = S 0x06 |
| U 0x07 | 0x08021000 | +1 (0x08) quando anim == -1; senão fica | condicional | não | = S 0x07 |
| U 0x08 | 0x08021EA0 | +1 (0x09) | fixo | não | gfx do efeito; bits 0x200 (+0x1000 se +0xF8&1) no slot; timer 0x3C |
| U 0x0A | 0x08021F90 | +1 (0x0B) ao chegar no alvo; senão fica | condicional | não | rola BG 0x40/quadro |
| U 0x0B | 0x08022010 | +1 (0x0C) | fixo | não | zera +0xC9..+0xCC do alvo (status), SFX 0x37 |

Os passos de U 0x0C, 0x0D e 0x0E, de S 0x11/0x18 etc. já estavam resolvidos em SU.md; não reconferidos.

Demais pendentes (V e AB/AC/AD compartilhados):

| Passo | Endereço | Próximo +0xAA | Condição | Encerra? | Nota |
|---|---|---|---|---|---|
| W 0x07 | 0x08022748 | +1 (0x08) | fixo | não | ator: bot+0xBD |= 8; +0xDF = técnica da peça; +0xD8 = 0 e +0xD8 += +0xEC (clamp 0xFF) |
| X 0x04..0x07 | (= S 04..07) | +1 / cond. | — | não | compartilhados |
| X 0x0E | 0x08022C28 | +1 (0x0F) | fixo | não | alvo: +0xC4 = 0x1F (status), +0xC5 = 0, +0xC6 = clamp(+0xE8), +0xC7 = clamp(+0xEC) |
| Y 0x04..0x07 | (= S 04..07) | +1 / cond. | — | não | compartilhados |
| Y 0x0E | 0x08022F50 | +1 (0x0F) | fixo | não | alvo +0x40 = 3; FightPreview_SplitCountByCapacity; cabeça +0x59 = ctx+0xC8; +0xBA = 1; reposiciona bots |
| AC 0x00 | 0x08023AF0 | ROS ok: 0x04; ROS esgotado: +1 (0x01) com msg 0x1A | 2 vias | não | sem teste de técnica e sem custo de status |
| AC 0x04..0x07 | (= S 04..07) | +1 / cond. | — | não | compartilhados |
| AC 0x08 | 0x08023B44 | +1 (0x09) | fixo | não | gfx; +0x84 = 0x45; Slot_SetupFromRecordList(lista+0x114); timer 0x3C |
| AD 0x00 | 0x08023CB8 | ROS ok e checagem Fight_HasNoTechnique16Part == 0: msg 0x39, +1 (0x01); ROS ok e passou: 0x04; ROS esgotado: msg 0x1A, +1 | 3 vias | não | bloqueio pela técnica 16 (no alvo e no próprio) |
| AD 0x04..0x07 | (= S 04..07) | +1 / cond. | — | não | compartilhados |
| AD 0x08 | 0x08023D44 | +1 (0x09) | fixo | não | gfx; +0x84 = 0x42; slot lista+0x108; timer 0x3C |
| AD 0x0D | 0x08023E50 | +1 (0x0E) quando byte == 8 (Slot_ClearAttrMask(0x21,1)); senão -8 e fica | condicional | não | |
| AD 0x0E | 0x08023E94 | +1 (0x0F) | fixo | não | gfx; +0x84 = 0x42; slot lista+0x108; timer 0x3C |
| AD 0x12 | 0x08023F80 | +1 (0x13) | fixo | não | +0x84 = 0x43; slot lista+0x10C; SFX 0x4E |

## Efeito de cada técnica (3 linhas: o que muda nos bots)

- **S (0x29), dano**: ROS e custo do ator; grava o registro de dano do alvo (ctx+0x2A..+0x2D, slot pelo fi+0x8A) com +0xE8 (clamp 0xFF) e +0xEC + defesa do alvo (byte +0xD5) — dano acumulado para aplicar depois. Modo 0..3 só muda a mensagem (0x1E..0x21) e o sprite.
- **U (0x2B), cura de status**: ROS; zera os bytes de status do alvo (+0xC9..+0xCC) e liga bits no slot do alvo com SFX 0x37. Mensagem final 0x23. Sem dano e sem revive.
- **V (0x2C), buff visual sem efeito confirmado**: ROS; 4 orbes giram em volta do ator e depois U06 alterna o flag +0x58 do bloco (msg 0x72/0x73). Sem dano, status ou revive escritos no que foi lido.
- **W (0x2D), buff de carga no ator**: ROS (variante pela técnica da peça +0x49); bot+0xBD |= 8, +0xDF = técnica da peça, +0xD8 = +0xEC (acumula até 0xFF). Efeito no próximo golpe não lido (fora do SU).
- **X (0x2E), status no alvo**: ROS; passo 0x0E grava status 0x1F no alvo (+0xC4) com +0xC6/+0xC7 = valores clampados de +0xE8/+0xEC. Bloqueio (0x39) se o alvo tem peça com técnica 0x16 (passo 0x00 = fluxo de bloqueio).
- **Y (0x2F), revive**: ROS; só age se a cabeça do alvo tem armadura 0 (+0x59). Passo 0x0E: alvo +0x40 = 3, cabeça +0x59 recebe ctx+0xC8, +0xBA = 1 e reposiciona os bots; 0x0F copia 0xE bytes do bot alvo para o buffer (FUN_0803F63C(bot alvo, dado, 0xE); M_05 copia o sentido inverso, tabela -> buffer; DC4) e grava msg 0x28.
- **AC (0x33), flag do bot**: AC 0x00 faz ROS sem custo de status; AC 0x0E escreve 1 no byte +0xBF do bot indicado por fi+0x88 (flag "carga ultra rápida"); AC 0x0F grava código 0x2B em +0xA6. Dano não escrito no que foi lido.
- **AD (0x34), status compartilhado e absorção**: AD 0x00 bloqueia (msg 0x39) se a técnica 16 aparece no alvo ou no próprio; AD 0x15 (ver SU.md) escreve 0x29 em +0xC9..+0xCC e zera +0xC4..+0xC7 dos dois bots; AD 0x16 grava código 0x2D ("absorveu tempo do inimigo").

## Renomeação (não feita)

Nenhum símbolo foi renomeado nesta rodada, para não alterar o programa aberto no Ghidra. Padrão sugerido para os 56 passos com handler próprio: Fight_DispatchOnSubstateAA_<D>_<idx>_<Passo>. Não há CSV neste lote.

## Em aberto

- Ponteiro indireto em 0x0802155C (usado por S 0x0F/0x16/0x26): não recuperado pelo Ghidra. Destino do passo depois do +1 não confirmado.
- Efeito de V (flag +0x58), W (carga no próximo golpe), AC (flag 0xBF) e AD 0x15 (dois bots) precisa de teste no emulador; não foi rodado (fora do escopo).
- Passos com handler em 0x0803080C, 0x08030608, 0x080305E0 e 0x08030674 usados em todos os despachantes: já documentados em SU.md, não reconferidos.
