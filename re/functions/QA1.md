# QA1: conferência das 18 funções criadas pelo MedabotsSync (Metabee EU)

Critério: real = código Thumb válido E (é chamada por BL/B OU é alvo de ponteiro na ROM).
Método: varredura da ROM original (`Medabots - Metabee (Europe).gba`, só leitura, `python -I`) procurando
chamadas BL (Thumb), B (tail), palavras ARM BL/BLX, e palavras alinhadas iguais a endereço|1 (ponteiros).
Ghidra: `get_function_by_address` confirmou que existe função em todos os 18 e que todos têm nome (nenhum FUN_).
`get_xrefs_to` voltou VAZIO em todos, inclusive no 0x08045A40, que tem 3 BL confirmados. Logo o xref do Ghidra
nesta sessão não serve de prova; a decisão é pela ROM.

| endereço | nome mtbe | real? | motivo | ação |
|---|---|---|---|---|
| 0x08000000 | entry | NÃO (ARM) | cabeçalho da ROM: primeira palavra 0xEA00002E é ARM (B), não Thumb; ninguém a chama por BL (entrada do BIOS) | REMOVER (kind func_thumb; se quiser, manter como rótulo de cabeçalho) |
| 0x0800B890 | DebugMenu_Dispatch | SIM | Thumb válido (push/ldr/bl/pop/bx); ponteiro em 0x083AC3C4 | manter |
| 0x0800BA74 | DebugMenu_Exit | SIM | Thumb válido; ponteiro em 0x083ACA64 | manter |
| 0x08010C50 | Fight_HandleRewardGrantOrSkipByPostBattleFlags | SIM | Thumb válido; ponteiro em 0x083ACF00 | manter |
| 0x08030860 | Fight_SceneMain | SIM | Thumb válido; ponteiro em 0x083AC2E8; já tem nome no Ghidra | manter |
| 0x08033840 | Fight_RunActionScriptLoop | SIM | Thumb válido; ponteiro em 0x083AC2EC | manter |
| 0x08033C00 | Fight_ComputeTotalRosPhase | SIM | Thumb válido (2 BL + pop/bx); ponteiro em 0x083AF1B0 | manter |
| 0x08033C20 | Fight_ApplyPendingEffectStatusFlags | SIM | Thumb válido; ponteiro em 0x083AF1B8 | manter |
| 0x08033CE0 | Fight_UpdateActionArgSelectorPhase | SIM | Thumb válido; ponteiro em 0x083AF1C0 | manter |
| 0x08033D10 | Fight_SeedPreDamageFromTotalRos | SIM | Thumb válido; ponteiro em 0x083AF1CC | manter |
| 0x08045858 | EmitCompositePreviewTemplateEntriesSortedByY | NÃO CONFIRMADA (Thumb válido, sem chamador) | começa com B (tail para 0x08045870); zero BL, B, ponteiro ou palavra ARM apontando para ele | REMOVER (candidato; código órfão) |
| 0x0804589C | InitCompositePreviewTemplatePointerList | NÃO CONFIRMADA (Thumb válido, sem chamador) | corpo Thumb válido (str/movs/bx lr); zero referências na ROM | REMOVER (candidato; código órfão) |
| 0x080458A4 | AppendCompositePreviewTemplatePointer | NÃO CONFIRMADA (Thumb válido, sem chamador) | corpo Thumb válido; zero referências na ROM | REMOVER (candidato; código órfão) |
| 0x080459A4 | OffsetCompositePreviewTemplateEntry | NÃO CONFIRMADA (Thumb válido, sem chamador) | prólogo push/mov válido; zero referências; corpo no Ghidra com 0 bytes | REMOVER (candidato; código órfão) |
| 0x08045A40 | IsCompositePreviewPointOnScreen | SIM | 3 BL reais (0x08045B24, 0x08045BB0, 0x08045C70); Thumb válido, bx lr em 0x08045A5E. CF.md dizia "pool/dado": ERRADO | manter nome; corrigir CF.md; Ghidra tem corpo de 1 byte (desmontagem quebrada: refazer no Ghidra pelo usuário) |
| 0x08045AEC | QueueVisibleCompositePreviewTemplateEntry | NÃO CONFIRMADA (Thumb válido; só chama 0x08045A40) | sem chamador externo à família; zero referências de fora | REMOVER (candidato; código órfão) |
| 0x08045B68 | QueueVisibleCompositePreviewTemplateEntryWithBounds | NÃO CONFIRMADA (Thumb válido; só chama 0x08045A40) | sem chamador externo à família | REMOVER (candidato; código órfão) |
| 0x08045C30 | QueueVisibleCompositePreviewTemplateEntryWithAffineBounds | NÃO CONFIRMADA (Thumb válido; só chama 0x08045A40) | sem chamador externo à família | REMOVER (candidato; código órfão) |

## Resumo
- Reais (10): 0x0800B890, 0x0800BA74, 0x08010C50, 0x08030860, 0x08033840, 0x08033C00, 0x08033C20, 0x08033CE0, 0x08033D10, 0x08045A40.
- REMOVER (8): 0x08000000 (cabeçalho ARM) + 7 órfãos da família "CompositePreview" (0x08045858, 0x0804589C, 0x080458A4, 0x080459A4, 0x08045AEC, 0x08045B68, 0x08045C30).
- Os 7 órfãos têm código Thumb válido. Podem ser código morto de um build antigo, ou ser chamados a partir de um
  endereço copiado para IWRAM em tempo de execução (a varredura só enxerga a ROM). Por isso são candidatos, não
  falsos: a decisão é do usuário.
- Nenhum FUN_ entre os 18: nenhum rename feito.
- Não apaguei nada, não editei mtbe.csv, não toquei no Ghidra (sem salvar nem fechar), sem commit.
- Ação extra sugerida: 0x08045A40 precisa de desmontagem refeita no Ghidra (corpo de 1 byte); CF.md linha 12 e 31 e
  a linha do mtbe.csv (comentário "[CF: nao e funcao; literal pool/dado]") devem ser corrigidas.
