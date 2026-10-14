# TR: tabela técnica -> onde guarda -> duração -> quem consome -> quando limpa (GH-TR, 2026-10-14)

Fecha os pontos abertos de SS2.md (§3, §4, §5, §6) e de FI2 (fi+0x2A..0x3A). Ghidra só leitura (decompile, xrefs, desmonte);
nada salvo, nada renomeado nesta rodada (os nomes necessários já existiam). Varredura da ROM: literais de 32 bits
(python -I, sem emulador). Não é varredura completa de escritas por offset: o restante fica em "Pendente".

Legenda: C = confirmado (decompile/literal lido nesta rodada) | P = provável | A = aberto.

## Tabela técnica

| Técnica / efeito | Onde guarda | Duração (quanto tempo) | Quem consome | Quando limpa | Conf |
|---|---|---|---|---|---|
| 0x2E No Escape | alvo bot+0xBC bit 0x04 (bypass de esquiva) | bit fica até a próxima ação do ALVO (BR_10 dele) ou derrota. Duração do pendente em fi+0xE8 (1..99, `SetPendingPartStatus` 0x08036E64) (P: não ligado ao bit) | `Fight_ApplyEvasionReductionToRos` 0x08038F84 (pula o cálculo de esquiva) | `Fight_ResetBotPartFlags` 0x0801B60C (BC=0), `Fight_ResetBotPartState` 0x08036FA0 (derrota) | C (escrita 0x08033C20; leitor 0x08038F84); duração P |
| 0x2F No Defense | alvo bot+0xBC bit 0x08 (bypass de defesa) | igual ao 0x2E | `Fight_ApplyDefenseReductionToRos` 0x08039068 (pula redução de defesa) | igual ao 0x2E | C / P |
| 0x0F/0x10 Thunder/Freeze | alvo bot+0xBC bits 0x0C | igual | 0x08038F84 e 0x08039068 | igual | C |
| Atacante com bit 1 / 2 | ator bot+0xBC |= 0x04 (se &1) / 0x08 (se &2) | igual | igual (BR_10 do ator zera) | C (0x08033C20) |
| Fonte do 0x2E/0x2F | roteiro de ação: opcode 0x0E de gActionScriptOpcodeHandlerTable 0x083AF180 (entrada 14 = 0x083AF1B8) | roda no passo do roteiro | - | - | C (despachante 0x08033840, literal 0x080338D8) |
| Escudo/bloqueio de status (0x0D/0x10/0x12/0x13) | bot+0xCF (máscara 0x01/0x08/0x02/0x04) | - | `FightPreview_IsPartTypeFlagSet` 0x08036D64 chamado por AV 0x080287DC, BE 0x0802B550, I_00 0x0801F51A | escrita: AW_09 0x08029354 (liga) | C (SS2 §1); limpeza A |
| Scout (36) | registro de 8 B fi+0x28+8k, k = fi+0x89 (fase). Byte +0x28 = bônus | 1 fase: fi+0x89 = (fi+0x45-1)/3 (`Fight_UpdatePhaseFromCounter` 0x0801A640) | E8 do acerto (DM, leitor não lido nesta rodada) | fase muda; T_11 zera +2A..+2D (k=fi+0x89) | +0x28 consumo P; escritor do +0x28 A |
| Suporte do time (AT 0x52/0x0B) | registro fi+0x2A+8k, k = fi+0x8A (base 0x2A = byte +2 do registro de 0x28) | até T_11 (k=fi+0x89) ou Op3D (k=fi+0x8A) limpar | AT_CheckSupportAlreadyActive 0x08028078 (ação se +2A==0 ou +2B!='Z'); Team Form E8 (+2C, P) | `Fight_DispatchOnSubstateAA_T_11_ClearRecordSlot` 0x08021DAC zera +2A..+2D; `ActionScript_Op3D_ClearSupportRecord` 0x080345E8 zera +2C..+2F | C (escrita AT 0x08028130; zeragens C) |
| Marcador de suporte ativo | fi+0x2B = 0x5A ('Z') | = registro | AT 0x08028078 | igual ao registro | C |
| Carga (38-40 Boost/Rapid/Accel) | ator bot+0xBD bit 0x08; fi+0xDF = técnica da peça | até o início da próxima ação do DONO (BR_10 zera BD&=0xE5) ou peça selecionada quebrada (BD&=0xE7) | E00 `Fight_FindAllyFollowupActionSource` 0x0801D5A8 (P) e ícone `Fight_DrawPartStatusIcon` 0x08019B40 (C) | `Fight_ResetBotPartFlags` 0x0801B60C; `Fight_SelectBotActivePart` 0x0801B37C | escrita C (W_07 0x08022748, SS2); ligação 38-40 P |
| Modo do ícone (0x10) | ator bot+0xBD bit 0x10 | até a próxima ação do dono | ícone estático, `Fight_DrawPartStatusIcon` | `Fight_ResetBotPartFlags` (0xE5 zera 0x10) | C |
| Armadilha (10/11 Grap/Shot) | não tem estado próprio: é o roteiro gActionScript_GrapTrap/ShotTrap (0x083C6A57/59) rodado pelo laço 0x08033840 | duração do roteiro | - | fim do roteiro | C (passo 01 = Dispatch B + AA++, 0x08030608) |
| Conceal (37) | só mensagem fi+0xA6 = 0x1D (0x08023718) | - | - | - | C (só mensagem); estado de esquiva A |
| Bot+0xDE (peça do ícone, 1..3) | bot+0xDE = bot+0xDD | enquanto a peça for a escolhida | `Fight_DrawPartStatusIcon` (armadura em (DE-1)*0x18+0x59) | regravado por `Fight_SelectBotActivePart` | C (escritor 0x0801B37C, BR_50 0x08030E8E) |
| Bot+0xDF (técnica da peça escolhida) | bot+0xDF = técnica (+0x49 da peça) | igual a +0xDE | ícone | regravado em SelectBotActivePart | C |
| Reset total de bot+0xBC | bot+0xBC = 0 | - | - | `Fight_ResetBotPartFlags` (ator, início de ação, só chamador BR_10 0x080308BC passo 0 com fi+0x87); `Fight_ResetBotPartState` (derrota, só K_00 0x0801FE14) | C |

## Escritores/limpezas confirmados (decompile)

- `Fight_ResetBotPartFlags` 0x0801B60C: bot+0xBC = 0; bot+0xBD &= 0xE5 (tira 0x02, 0x08, 0x10); zera blocos +0x00,+0x20,+0x40,+0x50,+0xBC. Chamador único: `Fight_DispatchOnSubstateAA_BR_10_ActionSelectLoadGfx` 0x080308BC (passo 0, ator = fi+0x87).
- `Fight_SelectBotActivePart` 0x0801B37C: bot+0xBD &= 0xFE; bot+0xDE = bot+0xDD; bot+0xDF = byte +0x49 da peça (se 1..3); limpa +0x56.. (4 peças) e BD &= 0xE7 quando a peça de +0xDE tem +0x59 == 0. Chamador: BR_50 0x08030E8E.
- `Fight_DispatchOnSubstateAA_AT_ApplyTeamSupportBonus` 0x08028130: escreve +2A=1, +2B=0x5A, +2C=0, +2D=0 e soma (ramo novo) ou +2C+=E8>>1 (ramo ativo). Confirma FI2 (base 0x2A).
- `Fight_DispatchOnSubstateAA_AT_CheckSupportAlreadyActive` 0x08028078 (lido em FI2): decide ação pelo registro.
- Interpretação: a tabela de registros é de 8 B em fi+0x28 (byte 0 = bônus de Scout), e o flag/marcador/acumuladores ficam em +0x2A..+0x2D. A nota de DM.md ("+0x2B == 51") está em outro sentido: o byte +0x2B é 'Z' no AT. Trate DM.md §6 como errado nesse ponto (P).

## Pendente (A)

1. Escritor do byte fi+0x28+8k (bônus do Scout): nenhum achado (literais 0x03001828/0x0300182A não aparecem como literal; a base 0x03001800 é usada por ~40 funções, varredura por código não feita).
2. Leitor do Scout (E8 += +0x28): não decompilado nesta rodada (P).
3. Ligação exata de 38/39/40 a W_07 e leitor de E00 (E00 0x0801D5A8) (P).
4. Duração do pendente 0x2E/0x2F (fi+0xE8) x limpeza do bit em bot+0xBC: hoje o bit cai na próxima ação do alvo, não ao fim da contagem. Decidir com replay (A).
5. Conceal: estado de esquiva (A). Estado da armadilha: sem estado extra além do roteiro (C), mas o pendente de armadilha em AD/AF (0x36) não foi lido.
6. Escritores de fi+0x2E..0x3A e de bot+0xBC bits 0x20/0x40 (mantidos fora desta rodada).
7. I_00 0x0801F51A e os quatro SetPending de BE: tipos não conferidos (P).
