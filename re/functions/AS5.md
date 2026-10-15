# AS5: efeito fechado de Op54, Op56, Op53 e passos AU/AO/AP; tabela Medaforce id -> roteiro -> efeito (GH-AS5)

Leitura: ROM original (so leitura) com `python -I` e decompile Ghidra dos handlers. Sem emulador, sem texto do jogo
(medaforcas so por id). **(corrige AS4)** marca correcoes ao AS4.

## (1) Op54 -> AV: status em todos os adversarios, por id de Medaforce
- Despachante AV (0x08028404), passo AV_03 (0x080287DC): id = B+0xA8 do bot ativo (fight+0x87). Indice da tabela de
  handlers = id - 0x0C, valido se < 0x1D. **(corrige AS4)**: a faixa valida vai de 0x0C a 0x28, nao ate 0x1C.
- Tabela de handlers: ponteiro literal 0x08028850 -> 0x08028854 (29 entradas de 4 bytes).
- Cada handler de status percorre os bots adversarios validos (+0x40 != 0, +0x59 != 0, com peca sem tecnica 16),
  pula o bot que ja tem o status (FightPreview_IsPartTypeFlagSet) e chama
  Fight_ApplyPartStatus(bot, codigo, fight+0xE8, fight+0xEC). Marca fight+0xD2+k = 1 nos atingidos. Efeito de estado no BOT.
- Entradas sem status apontam para 0x080289E4 (so avanca o laco).

| id Medaforce | handler | codigo de status | efeito |
|---|---|---|---|
| 0x0C | AV_0D 0x080288C8 | 0x0D | status 0x0D nos adversarios validos |
| 0x0E | AV_10 0x08028908 | 0x10 | status 0x10 |
| 0x10 | AV_12 0x08028944 | 0x12 | status 0x12 |
| 0x12 | AV_13 0x08028980 | 0x13 | status 0x13 |
| 0x28 | AV_2C 0x080289BC | 0x2C | status 0x2C |
| demais 0x0C..0x28 | 0x080289E4 | nenhum | so avanca o laco |

Os ids que usam Op54 nos roteiros sao exatamente 0x0C, 0x0E, 0x10, 0x12 e 0x28 (ver tabela 4).
Parametros do status (fight+0xE8 e fight+0xEC, shorts): valor/duracao nao decodificados.

## (2) Op56 -> AX: so limpa status, NAO cura armadura nem HP
- AX_00 SetupCureBanner (0x080294DC): so sprite/banner; fight+0x51=0.
- AX_02 FlagAlliesWithStatus (0x08029600): marca aliados com +0xC9 != 0 (byte de status do bot).
- AX_04 SpawnCureEffects (0x08029680) e AX_05 WaitCureEffects (0x0802976C): efeito visual e espera.
- AX_06 ClearAllyStatus (0x080297EC): para cada aliado marcado chama FightPreview_ClearPartFields(bot,1), que zera +0xC9..+0xCC.
- AX_07 SetCureMessage (0x08029828): grava codigo de mensagem 0x53 em +0xA6.
- Nenhuma escrita em peca, armadura ou HP no AX. Cura de armadura do time e Op57 (AY); pecas quebradas, Op58 (AZ).

## (3) Op53 -> AU, e passos nao lidos
- AU_06 SlideIcon (0x080282E0): desliza icone +-1 por quadro (sentido pelo bit0 de B+0xF8). Sem estado de jogo.
- AU_07 BlinkThenClear (0x08028350): pisca icone (bit 0x8000 em +0x840) e limpa objetos 0x21 ao fim. Sem estado de jogo.
- AU_08 SetActorReactionFlag2 (0x080283C0): seta bit 0x02 em bot_ativo+0xC0 (marca de reacao).
- AU_09 SetStepCode64 (0x080283EC): fight+0xA6 = 0x64.
- Conclusao: Op53 = marca de reacao do proprio bot + animacao. Nao altera status nem HP.
- AO_02 ClampPartCountsToLimit (0x080268D4): limita as 4 pecas ao teto fight+0xEC. Sem dano.
- AO_03 ShowFourPartValues (0x08026924): so exibicao dos 4 valores (sfx 0x5B).
- AO_10 QueueHeadBreakIfDefenderHeadZero (0x08026CBC): se o defensor (fight+0x88) tem +0x59 == 0, chama
  Fight_QueueBrokenBotForResolution(defensor).
- AP_01 WaitUnlessFlagB8Bit1 (0x08026D1C): espera (animacao) enquanto B+0xB8 bit 0x02 nao esta setado.
- AP_18 SetupHitEffectSprite (0x08026D98): prepara sprite de acerto (sfx 0x2A) e salta por ponteiro (destino nao lido).
  Sem escrita de HP.

## (4) Tabela Medaforce id -> roteiro -> opcodes -> efeito
Roteiro = gTechniqueActionScriptPtrTable 0x083C6CA8[0x50+id]; cursor += 1+LEN[op], LEN em 0x083C6EB8 (LEN nao-zero:
02=1, 03=4, 90..bf e outros, dados); terminador 0x01. Lido com `python -I` (scratchpad). Medaforce 0x00..0x33.

Legenda de opcodes (nomes Ghidra; "pend." = efeito nao lido):
- 40 RollPartValues; 41 PickRandomOpponentTargetPart; 42 RollPartValueAndEffect; 43 AddActiveBotPartBonusToEC;
  44 DamageRandomTargetPart; 45 IncrementUsageCounter; 46 SumPartDamageOfTargetBot; 47 SideSpritesCopyTarget;
  48 RepairActorPart; 49 HalfArmorSelfBot; 4A CountBrokenAllyParts (pend.); 4B FindBotWithPartAndPickPart;
  4C ArmorOfTargetPart; 4D RollDamageFromTwoDice (pend. efeito); 4E FindActiveBotWithGroupAndPickPart;
  4F SetEffect10IfNoGroupMatch (pend.).
- 50 CallAR (dano em peca aleatoria do adversario); 51 SelfStatusByMedaforceId (AS); 52 TeamSupportBonus (AT);
  53 ReactionMarkSelf (AU); 54 StatusAllOpponents (AV); 55 TeamProtectStatus (AW); 56 TeamCureStatus (AX);
  57 TeamRepairArmor (AY); 58 TeamRestoreParts (AZ); 59 CallBA gauge do time; 5A TechniqueNoDamage (BB).
- 5B..6A CallBC..CallBR: despachantes BC..BR (efeito pend.; ver AS2.md).
- 06, 08, 1A, 1B, 1C, 20, 21: apresentacao/subestado (08 = DispatchOnSubstateAA_D, sem efeito lido; 06 sem nome).
- 1D ApplyDamageToTargetPart: dano em peca do alvo.

| id | script (opcodes) | efeito |
|---|---|---|
| 00 | 06 08 1A 1B 1C 40 20 45 21 | rola valores de peca do bot ativo (40); 06/08 pend.; sem dano direto |
| 01 | 42 50 45 | rola valor (42) + dano em peca aleatoria dos adversarios (50) |
| 02 | 41 42 43 08 1A 1B 1C 1D 44 20 45 21 | escolhe peca adversaria, rola (42), bonus EC (43), dano (44) |
| 03 | 41 42 46 08 1A 1B 1C 1D 44 20 45 21 | como 02, mas dano usa soma de dano do alvo (46) |
| 04-09 | 51 45 | estado no proprio bot por id (Op51, AS; ver AS4) |
| 0A | 42 52 45 | rola (42) + bonus de apoio do time (52) |
| 0B | 53 45 | marca de reacao do proprio bot (53) |
| 0C | 42 54 45 | status 0x0D nos adversarios (Op54/AV_0D) |
| 0D | 55 45 | protecao contra status do time (55) |
| 0E | 42 54 45 | status 0x10 nos adversarios (AV_10) |
| 0F | 55 45 | protecao do time (55) |
| 10 | 42 54 45 | status 0x12 nos adversarios (AV_12) |
| 11 | 55 45 | protecao do time (55) |
| 12 | 42 54 45 | status 0x13 nos adversarios (AV_13) |
| 13 | 55 45 | protecao do time (55) |
| 14 | 41 42 08 1A 1B 1C 1D 44 47 1B 1C 48 20 45 21 | dano em peca aleatoria (44) + copia alvo (47) + reparo de peca do proprio ator (48) |
| 15 | 56 45 | limpa status dos aliados (56); sem cura de HP/armadura |
| 16 | 42 59 45 | rola (42) + gauge do time (CallBA, 59) |
| 17 | 5A 45 | tecnica sem dano (CallBB, 5A) |
| 18 | 42 5B 45 | rola (42) + CallBC (5B) |
| 19 | 42 5C 45 | rola (42) + CallBD (5C) |
| 1A | 42 57 45 | rola (42) + reparo de armadura do time (57) |
| 1B | 41 42 08 49 1A 1B 1C 1D 44 20 45 21 | metade da armadura do proprio bot (49) + dano (44) |
| 1C | 42 58 45 | rola (42) + restaura pecas quebradas do time (58) |
| 1D | 41 42 08 4A 1A 1B 1C 1D 44 20 45 21 | conta pecas quebradas aliadas (4A, pend.) + dano (44) |
| 1E | 42 5D 45 | rola + CallBE (5D) |
| 1F | 42 5E 45 | rola + CallBF (5E) |
| 20 | 42 5D 45 | rola + CallBE (5D) |
| 21 | 42 5E 45 | rola + CallBF (5E) |
| 22 | 42 5F 45 | rola + CallBG (5F) |
| 23 | 42 5E 45 | rola + CallBF (5E) |
| 24 | 42 60 45 | rola + CallBH (60) |
| 25 | 42 5E 45 | rola + CallBF (5E) |
| 26 | 42 61 45 | rola + CallBI (61) |
| 27 | 42 62 21 45 | rola + CallBJ (62) |
| 28 | 42 54 45 | status 0x2C nos adversarios (AV_2C) |
| 29 | 42 63 45 | rola + CallBK (63) |
| 2A | 42 64 21 45 | rola + CallBL (64) |
| 2B | 4B 42 08 4C 1A 1B 1C 1D 44 20 45 21 | acha bot com peca (4B), rola (42), armadura da peca alvo (4C), dano (44) |
| 2C | 42 65 21 45 | rola + CallBM (65) |
| 2D | 66 45 | CallBN (66), sem rolagem |
| 2E | 42 67 45 | rola + CallBO (67) |
| 2F | 68 45 | CallBP (68) |
| 30 | 69 45 | CallBQ (69) |
| 31 | 6A 45 | CallBR (6A) |
| 32 | 41 42 08 4D 1A 1B 1C 1D 44 20 45 21 | dado duplo (4D, pend.) + dano (44) |
| 33 | 4E 42 08 4F 1A 1B 1C 1D 44 20 45 21 | acha bot com grupo (4E), 4F pend., dano (44) |

Resumo: status no adversario so em 0x0C, 0x0E, 0x10, 0x12, 0x28 (Op54). Limpeza de status do time so em 0x15 (Op56,
sem cura de HP/armadura). Os ids 0x18..0x2D e 0x31 dependem dos despachantes BC..BR, que nao foram lidos aqui.

## Renames no Ghidra
Nenhum novo: os nomes existentes ja batem com o efeito lido (AV_0D..AV_2C, AX_06, AU_06..09, AO_02/03/10, AP_01/18).
O stub 0x080289E4 nao tem funcao propria (so label; ver CSV).

## Pendente
- Efeitos dos despachantes BC..BR (opcodes 0x5B..0x6A); opcodes 4A, 4D, 4F; 06 e 08 (interior).
- Valores dos parametros de status (fight+0xE8/0xEC) e como Op51 escolhe o subpasso AS para ids 0x04..0x09.
