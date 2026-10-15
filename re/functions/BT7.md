# BT7: entrada e saída da Robattle medidas no emulador (2026-10-14)

Fonte: replay de `data/records/rec_7` (ROM EU original; encontro com o garoto, vitória, volta ao mapa), headless,
watch de escrita em `0x0300178C` (gMainState), `0x03002B8C` (gGameStateMenu) e `0x0300137C` (GS), 4 bytes cada.
Confiança: **confirmado** (pc e quadro medidos).

**Correção de premissa (BT3..BT6):** GS não é global da batalha. É o passo **dentro do menu atual**, e cada menu
tem sua tabela. "Quem grava GS=0x23" não fazia sentido; o 0x23 que aparece é o último passo do menu 6 e do menu 3.

## Entrada (quadros 302..998)
| Quadro | pc | Efeito |
|---|---|---|
| 302 | 0x080479E0/E6/EA (GameState_StartFlowByCommand, lr 0x08051C5F = passo no mapa → cmd 0x21) | gMainState 1→0, menu →6, GS →0 |
| 303..503 | 0x080102FC..0x08010A6A (passos da tabela do menu 6, 0x083ACE24) | GS 0..0x0D, depois 0x20..0x23 (transição/flash do encontro) |
| 504 | 0x08010B40/B44 (Fight_ResetPartStateBlock, idx 0x23 do menu 6) | menu 6→3, GS 0 |
| 512..656 | 0x08003D2E..0x08004836 (tabela do menu 3) | GS 0,1,2, 0x20..0x23 (preview dos bots) |
| 657 | 0x0800485E/62 (BattlePreview_ClearStateStep) | menu 3→7, GS 0 |
| 658..971 | 0x08012A0E..0x0801430A (tabela do menu 7, 0x083AD080) | GS 0..9 → 0x38 (0x080165C2) → 0x0D → 0x40..0x45 (abertura da luta) |
| 972 | 0x08014390/94 (lr 0x08015A4F) | menu 7→8, GS 0 |
| 978..998 | 0x080308FC..0x08030C88 (tabela do menu 8) | GS 0..4 |
| 998 | 0x08030C84 | menu 8→9 (laço de roteiro) |

## Turno (repete a cada ação)
- Menu 9 = Fight_RunActionScriptLoop 0x08033840: GS é o cursor do roteiro (escrito em 0x08033890/0x080338A0).
- Fim do roteiro: 0x080338E4 (ActionScript_Op01_SetSceneExitState) menu 9→8; 0x08030CD4 menu 8→7;
  0x08014390 menu 7→8; 0x08030C84 menu 8→9. Ciclo medido 8 vezes (f1822..f7582).

## Saída (quadros 12113..12842)
| Quadro | pc | Efeito |
|---|---|---|
| 12113 | 0x0803135E (lr 0x08030883) | menu 8→6 (tela de resultado no menu 6: GS 0x30..0x5B) |
| 12825 | 0x080476B4/B8 = Menu_SetStateByArg01(1), chamado por Gfx_ResetBgLayersAndSong (lr 0x08011589) | gMainState 0→1, menu →1 (VM de eventos/mapa), GS 0x5B→0x0D |
| 12842 | — | GS 0x0E→0x05: mapa retomado (sub 5 = Map_TickSceneStateMachine) |

## Pendências
- Nomes dos passos 0x20..0x23 do menu 6 e do menu 3 (BT6 tinha a lista do menu 6).
- Scout/Conceal (técnicas 36/37): nenhuma gravação usa essas técnicas; precisa de nova gravação.
