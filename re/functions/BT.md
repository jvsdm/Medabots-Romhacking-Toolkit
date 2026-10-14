# BT: visão geral da Robattle (GH-BT, 2026-10-14)

Fluxo de alto nível de uma batalha, do início ao fim, ligando os pedaços já documentados em R1, FL7, JT/JT2,
HN9, HN10, SD, FH e A6. Confirmações desta rodada: decompile no Ghidra e literais lidos na ROM EU
(`Medabots - Metabee (Europe).gba`, só leitura). Nenhum emulador foi usado.

Confiança: "confirmado" = lido no decompile ou nos literais; "provável" = padrão de uso ou nome de doc antiga;
"lacuna" = não fechado.

## Variáveis de estado (quem é quem)

| Endereço | Nome | Papel | Confiança |
|---|---|---|---|
| 0x03002B8C | gamestate_menu (GM) | índice da cena em `menu_list` (0x083AC2C8) | confirmado (mtbe, decompile de 0x08012970) |
| 0x0300137C | gamestate_subroutine (GS) | passo dentro da cena; índice da tabela da cena | confirmado (literal 0x080129C4) |
| 0x03000440 | T (contexto de cena/mensagem) | +0x86 timer, +0x89 flag, +0x9C vencedor (1/2) | confirmado (literais em FL7, HN9) |
| 0x03000579 | seletor de cena (= 0x03000440+0x139) | índice de `Dispatch_ByteStateIndexTable` (0..5) | confirmado (JT, JT2) |
| 0x0300057A (0x03000440+0x13A) | byte de estado (passo) | índice de `Fight_RunStateDispatch` (0..8) | confirmado (literal 0x0801A7AC) |
| 0x03001800 | fight_information (F) | +0x87 bot ativo, +0x88 alvo, +0xA1/+0xA2 sub-estado e código, +0xA6 código de mensagem, +0xAA passo, +0xB5 handler feito, +0xE8 ROS, +0xEC valor de ação | confirmado (FH, SD, A6) |
| 0x03000590 + bot*0x100 | bloco do bot (B) | +0xC1 medaforce atual, +0xDD peça escolhida, +0x40 usado no fim | confirmado (HN9, FH) |
| 0x03002C30 | gObjectSlots (S) | sprites de animação; slots de efeito | confirmado (FH, SD) |
| 0x03001360 | gMessageBusyFlag | 0 = pode disparar mensagem | confirmado (A6) |
| 0x03002BA0 | inventário (64 pares) | fora do fluxo de batalha | confirmado (Key facts) |

## Diagrama em texto (estado -> próximo estado)

Legenda: `[cena m]` = `gamestate_menu` igual a m; `(GS n)` = passo da cena; `(sub n)` = `fight+0xAA` ou
`0x03000440+0x13A`.

```
ENTRADA DA CENA (do mapa: quem grava gamestate_menu = 8 ou 7 é LACUNA)
  Fight_BattleSceneInit 0x08013B8C
     - monta UI: decompress x7, LoadTilesWithMalias x4, CpuFastSet paleta, painel dos bots,
       preview do Medabot selecionado. Confirmado: NÃO grava o byte 0x13A.
     - entrada 0 de 0x083AD10C (tabela não achada por literal; lacuna)
  |
  v
  Dispatch_ByteStateIndexTable 0x08013904
     - índice = byte 0x03000579 (cena 0..5), tabela 0x080138A8 (JT2)
     - cena 3 = Fight_SelectScreenFrame 0x08013FA8 (seleção/preview, GpuBlend, espera 0x3C)
  |
  v
  Fight_RunStateDispatch 0x0801A788  (índice = 0x03000440+0x13A, 0..8; >8 sai)
     state0 0x0801A7D8 (4 gfx) -> state1 0x0801A818 (4 gfx) -> state2 0x0801A85C (5 gfx)
     -> state3 0x0801A8BC (5 gfx) -> state4 0x0801A910 (3 gfx) -> state5 0x0801A950 (repassa 0x9D)
     -> state6 0x0801A958 (3 gfx) -> state7 0x0801A998 (preenchimento) -> state8 0x0801A9C8 (salta 0x0801AADE)
     Quem grava 0x13A para cada passo: LACUNA (só leitores achados).

LOOP DE QUADRO DA LUTA (menu_list[7] = 0x08012970 Battle_FrameDispatchOrExit)
  Battle_FrameDispatchOrExit 0x08012970 (confirmado)
     - Battle_CheckExitShortcut; se sair, nada.
     - se *0x03000430 == 2: limpa 0x200 halfwords (BIOS).
     - CallViaR0(table_0x083AD080[GS]) : tabela 0x083AD080 (entradas 0x08012A20, 0x08012B40, 0x08012B94,...)
  Fight_StepBattleFrame 0x0801427C (confirmado no decompile; entrada 0 da tabela 0x083AD190)
     - se byte fi+0x55 (0x03001855, literal 0x080142A4 = 0x03001800) == 0: Fight_TickSecondaryEffectTimers, Fight_UpdateCounterDigitPositions.
     - bits de fi+0x55: bit1 -> espera 0x60; bit2 -> 0x68; bit3 -> 0x70; bit4 -> 0x78 (gravado em *0x0300137C).
     - senão: Fight_TickActiveBotsExceptStatusTypes0F10, Fight_DrawActiveBotsOverlay,
       Fight_UpdateSixBotSprites, Fight_UpdateBotPositionEvents; se 0, sai; se != 0 avança.
     - Quem indexa 0x083AD190: LACUNA (não há literal apontando para a tabela).

ESCOLHA DE AÇÃO (por bot, antes da execução) — nomes do mtbe, confiança provável/confirmado por descrição
  Fight_SelectActionsForBotGroup 0x0801531C  (3 bots a partir de first_bot)
     -> Fight_SelectActionFromCycleOrPendingOverride 0x08015358
          -> Fight_TrySelectActionFromSpecialityCycle 0x08015390  (speciality_action_table[ciclo])
          -> Fight_SelectFallbackActionFromPriorityList 0x08015464  (se o ciclo falhar)
     -> Fight_SelectActionSource 0x08015564  (1..3 = peça cabeça/direita/esquerda; grava B+0xDD)
     -> Fight_TrySelectMedaforceAction 0x080155A8 -> Fight_SelectMedaforceActionSource 0x08015668
        (bloqueio: Fight_IsMedaforceSelectionBlockedByBattleState 0x08015648; nível da medal)
  Gauge (barra do bot):
     Fight_TickPartGaugePhases 0x08014B34 (fases 0..3 de ctx+0x51/+0x53; provável)
     Fight_DrawBotGaugeTilemap 0x08015C40 (barra de 8 células; provável)
     Fight_ComputeGaugeFillFixed 0x08015DD0 (a/b*c em soft-float; provável)
     Ligação entre "gauge cheia" e "pode agir": LACUNA (não verificada).

EXECUÇÃO DA ROBATTLE (menu_list[8] = 0x08030860 Fight_SceneMain, passos GS 0x00..0x66)
  Fight_SceneMain 0x08030860 (confirmado por mtbe/HN10): chama 0x083AECD8[GS] se não há saída.
  Passos principais (HN9/HN10, confirmados nos decompiles listados):
    GS 0x00 BR_10 0x08030890: reset de flags, gfx
    GS 0x01..0x02 BR_11/BR_12: slot 3 e espera
    GS 0x03 BR_13_ActionSelectCheckPart 0x080309C8: B+0xBD bit0 -> A6=4, GS=0x20;
            B+0xDD == 0 -> GS=0x10; senão GS++
    GS 0x04 BR_14_ActionSelectValidatePart 0x08030A24: erro -> A6=0x04..0x0C, GS=0x20;
            medaforce -> GS=0x30; ok -> F+0xA8 = técnica, GM=9 (vai para RunActionScriptLoop)
    GS 0x10 BR_20 0x08030CFC: bot+0xC1 == 0x50 -> GS++; senão GS=0x14
    GS 0x11/0x20/0x31 BR_21 -> Fight_RunSubstateDispatchB até != 0; GS++
    GS 0x12 BR_22 -> Fight_RunSubstateDispatch(0) até != 0; GS++
    GS 0x14/0x34 BR_24 -> Fight_RunSubstateDispatchC(0) até != 0; GS++
    GS 0x16 BR_26 0x08030DB4: SFX 0x51, F+0x51 = 0x28/0x50, monta objetos de peça
    GS 0x17 BR_27 0x08030E14: anima objetos; InitBattleSprites; GS++
    GS 0x21/0x32/0x38 BR_31 0x08030E54: Fight_RunSubstateDispatch(F+0xA6) até != 0 (MENSAGEM)
    GS 0x30 BR_40 0x08031614: checa F+0x58, nível F+0xA7 contra bot+5 (10/30/60), B+0xC1 == 0x50;
            A6 = 0x79/0x36/0x37 ou GS = ...
    GS 0x40 BR_50 0x08030E78: bot0 e bot3 +0x40 == 0 -> T+0x9C = 2 (bot0) ou 1 (bot3);
            fade, SFX 0x6F, GS++; senão GS = 0x48 (provável: +0x40 = bot sem peças/derrota)
    GS 0x41..0x66 (passos de fim/level-up): ver Fim e Level-up.
  Execução da ação (scripts de ação, FL/A2):
    Fight_RunActionScriptLoop 0x08033840 (menu_list[9], confirmado no decompile):
       Battle_CheckExitShortcut; busca opcode (FUN_08034E94); enquanto >= 0x80 chama
       Fight_AdvanceActionScriptCursor; CallViaR0(tabela[op]); repete até o byte ctx+DAT_080338D0
       ficar 0 (um handler marca 1 = "feito neste quadro").
       Tabela de handlers: 0x083AF180 (gActionScriptOpcodeHandlerTable; opcodes 0x50..0x54 =
       AR..AV, KEY facts).
    Passos de sub-estado de ação: Fight_DispatchOnSubstateAA (+0xAA) com tabela por família
       (BR = 0x083AEC98 via Fight_DispatchOnSubstateAA_BR 0x0802FE50; AA/AB/AC/AD = cálculo de ROS e dano).
    Cálculo de valor: Fight_RollPartValue 0x0803769C -> Fight_ComputeEffectValueFromRos 0x080376F4
       (confirmado, FH). Dano por peça: Fight_ScaleAttackValueByPartScalar 0x08037280 (FH).
    Gasto de ROS: Fight_SpendRos20ReturnsExhausted 0x0803725C (retorno 1 = sem ROS -> A6=0x1A, FH).
  Mensagens (fight+0xA6):
    ActionFamily_StatusCheckFamily_Handler 0x080305E0 -> Fight_RunSubstateDispatch(F+0xA6)
    -> Fight_TryUiActionByIndex 0x0801AC24 -> StateMachine_DispatchStep 0x0803FB3C
    -> gTextGameMessages (0x083C6744), índice = código de A6 (confirmado, A6).
  Dano, peça quebrada, derrota:
    Dano aplicado a peça: 0x030018CC (amount_of_damage_to_part), 0x030018D1 (part_gets_broken_from_damage),
       0x030018EC (damage), 0x030018CA (leftover_damage), 0x030018C8 (part_damage_counter) (mtbe, provável no uso).
    Aplicação de dano pendente: Fight_ApplyPendingEffectDamageToPart 0x08018FF0 (mtbe).
    Cabeça quebrada = derrota do bot: LACUNA (não verificada neste passo; ver Fight_TickActiveBotStatusEffects 0x08018CBC).
    Vencedor por lado: Fight_EvaluateTeamWinState 0x0801B53C (confirmado): compara
       Fight_CountBotsWithStatus2(lado 0) x (lado 3), depois Fight_CountEmptyPartSlots, depois
       +0x31 x +0x29 do contexto 0x03000440; empate -> randbyte. Grava T+0x9C = 1 ou 2.
  Medaforce: Fight_TrySelectMedaforceAction 0x080155A8 (checa nível), Fight_SelectMedaforceActionSource
       0x08015668 (grava o descritor), passo GS 0x30 (BR_40) e cena de medaforce (BR_45/BR_46).

FIM DE BATALHA E SAÍDA
  Fight_SceneStep48_CheckTimeUpOrLevelUp 0x08031120 (HN10): F+0x57 == 0 -> Fight_EvaluateTeamWinState;
     senão F+0x165 ? 0x60 : 0x4F.
  Fight_SceneStep45_GotoEndBattle 0x08030CA0: passos 0x45/0x4D -> sub = 0x50 (confirmado).
  Fight_SceneStep50_EndBattleBranchLink 0x08031298 (HN10, confirmado):
     T+0x89 == 1 (link) -> 0x58; senão fade de música, sub++.
  Fight_SceneStep51_CommitPalettes 0x080312CC -> Fight_SceneStep52_FadePalettesToWhite 0x080312F0.
  Fight_SceneStep53_ExitBattleScene 0x0803133C (HN10, confirmado):
     0x030017F0 = nop; T+0x132 = 1; link -> gamestate_menu = 10 (sub 0x20);
     senão gamestate_menu = 6 (sub 0x30).
  Fim com passos de nocaute/tempo: 0x41 LoadKoBanner 0x08030F10; 0x42/0x4A SlideBannerIn 0x08030FB4;
     0x43 HoldBannerThenJingle 0x08030FEC; 0x44 ShowLeaderDownWinnerText 0x080310A0;
     0x49 LoadTimeUpBanner 0x08031174; 0x4C ShowTimeUpWinnerText 0x08031218.
  Level-up (0x60..0x66, HN10; significado "level-up" é chute): 0x08031390, 0x08031438, 0x08031458,
     0x08031518, 0x08031538, 0x080315B0, 0x08031608 (volta a sub 0x4F).
  Recompensa: Fight_HandleRewardGrantOrSkipByPostBattleFlags 0x08010C50 (mtbe; gPostBattleModeFlags).
  Volta ao mapa: gamestate_menu = 10 (link) ou 6; menu 6 = 0x08010264 e menu 10 = 0x0803D538: NÃO NOMEADOS.
```

## Tabela dos estados principais de fight_information (0x03001800)

| Offset | Nome provável | Quem grava | Quem lê | Valores | Confiança |
|---|---|---|---|---|---|
| +0x55 | flags de espera do quadro | ? | Fight_StepBattleFrame 0x0801427C | bits 1..4 -> espera 0x60..0x78 | confirmado (leitura) |
| +0x87 | bot ativo | ? | quase todos os passos SD/HN | 0..5 | confirmado |
| +0x88 | alvo | ? | Fight_BotSideAnimStep, RunEntryVisualStep | | confirmado |
| +0x9C | índice da tabela C (passos de entrada do bot) | Gfx/Entry steps (SD) | Fight_RunSubstateDispatchC | 0..9 | confirmado |
| +0x9D | arg da tabela C | Fight_RunSubstateDispatchC | passos C | | confirmado |
| +0x9E | saída (1 = terminou) | passos C | Fight_RunSubstateDispatchC | 0/1 | confirmado |
| +0xA1 | índice da tabela A (sub-estado de UI) | passos A | Fight_RunSubstateDispatch | 0..8; 3 = pronto | confirmado (SD) |
| +0xA2 | código de mensagem recebido | Fight_RunSubstateDispatch | Fight_UiActionStep | = +0xA6 | confirmado (SD, FH) |
| +0xA6 | código de passo/mensagem | produtores SetStepCode (HA*) | ActionFamily_StatusCheckFamily_Handler | 0x1A, 0x1C, 0x1D, 0x25, 0x2B..0x2D, 0x39, 0x6A, ... | confirmado (A6) |
| +0xA7 | peça usada | ? | BR_40 (nível) | | provável |
| +0xA8 | técnica selecionada | BR_14 | | | confirmado (HN9) |
| +0xAA | passo de ação (substate) | cada handler (++ ou =) | Fight_DispatchOnSubstateAA_* | 0..0x3F | confirmado |
| +0xB5 | handler concluído | Fight_... (0x0803080C sets 1) | cadeias de passos | 0/1 | confirmado (Key facts) |
| +0xD2..D4 | bots afetados | ? | | | confirmado (Key facts) |
| +0xE8 | ROS (u16) | Fight_SpendRos20... / RollPartValue | | | confirmado (FH) |
| +0xEC | valor de ação (s16) | ScaleAttackValueByPartScalar / ComputeEffectValueFromRos | | | confirmado (FH) |
| +0x139 (0x03000579 é T+0x139) | seletor de cena | ? | Dispatch_ByteStateIndexTable | 0..5 | confirmado (leitura) |
| T+0x13A | byte de estado (passo) | ? | Fight_RunStateDispatch | 0..8 | confirmado (leitura) |

Obs.: no mapeamento de offsets, "+0x9C" aqui é da tabela C de fi (0x03001800+0x9C); "T+0x9C" é o vencedor
de 0x03000440. Não são o mesmo byte.

## Lacunas (não fechadas nesta rodada)

1. Quem grava o gamestate_menu = 8 e = 7 ao entrar na Robattle (vindo do mapa). Não achado.
2. Quem indexa a tabela 0x083AD190 (Fight_StepBattleFrame) e a 0x083AD10C (Fight_BattleSceneInit): não há
   literal que aponte para elas. Pode ser base calculada. Não resolvido.
3. Relação entre 0x083AD080 (Battle_FrameDispatchOrExit, por GS) e 0x083AD190 (Fight_StepBattleFrame):
   o frame de menu 7 usa 0x083AD080; onde entra 0x083AD190 não foi fechado.
4. Quem grava 0x13A (passo de estado) e 0x139 (cena): só achei leitores. Falta varrer os chamadores de
   Dispatch_ByteStateIndexTable e de Fight_RunStateDispatch com decompile.
5. Estados 1..4 e 6 de Fight_RunStateDispatch: sem a ordem exata de transição (só o que carregam).
6. state8 salta para 0x0801AADE: não lido.
7. Menu 6 (0x08010264) e menu 10 (0x0803D538): sem nome. Menu 10 é a saída "link".
8. Significado de bot+0x40 usado em BR_50 (provável: bot sem peças/derrotado). Não confirmado.
9. Cabeça quebrada = derrota: a condição exata não foi lida nesta rodada.
10. Gauge: relação "barra cheia" -> "pode agir" não verificada.
11. Medaforce: níveis 10/30/60 vêm de HN9 (confirmado por leitura, não testado no emulador).
12. Nada foi testado no emulador (regra da tarefa); tudo é leitura de código.
13. Ghidra: nenhum rename novo foi feito nesta rodada (não havia nome confirmado por decompile sem lacuna).
