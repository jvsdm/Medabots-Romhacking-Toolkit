# CANCEL: Force Bind (23) e passivos Cancel 24/25/26 (GH-CANCEL, 2026-10-18)

Leitura estática no Ghidra (MCP, só leitura) + varredura da ROM (Python, só leitura). Sem emulador.
Confiança: "confirmado" = decompile lido; "provável" = padrão; "aberto" = não fechado.

## 1. Force Bind (23): FECHADO (roteiro e efeito)

Roteiro: `gActionScript_ForceBind` 0x083C6A6B (TQ2_fluxo.txt). Bytes `2C 01`: um único opcode.

| Passo | Função (entrada) | O que faz | Confiança |
|---|---|---|---|
| op 0x2C | `ActionScript_Op2C_CallSubstateV` 0x0803440C | chama `Fight_DispatchOnSubstateAA_V` 0x08022154 (despacha por `+0xAA`, actionSubstate) | confirmado |
| V 0x00 | `Fight_DispatchOnSubstateAA_V_00_CheckRos` 0x0802217C | teste de acerto (ROS). Falha: `+0xA6=0x1A`, `+1`. Passa: `+0xAA=4` | confirmado (HN1) |
| V 0x04 | `..._V_04_SpawnFourOrbsOnActor` 0x080221C8 | 4 sprites no ator (efeito visual), `+1` | confirmado |
| V 0x05 | `..._V_05_SpiralOrbsAroundActor` 0x080222A8 | gira os sprites; ao fim, `+1` | confirmado |
| V 0x06 | `..._V_06_ToggleFlag58Message` 0x08022380 | **alterna F+0x58** (0→1 com msg 0x72; 1→0 com msg 0x73) | confirmado |

- Cadeia de substates 0 → 4 → 5 → 6 confere com o `+1` de cada handler.
- Erro de acerto (ROS zero) não alterna o flag: é o mesmo teste das Interrupt (`disrupt.lua` `M.hits`).
- Efeito: F+0x58 = `medaforceEligible` (`gFight_MedaforceEligible` 0x03001858). Flag global da luta, não por lado.

Consumidores de F+0x58 (todos lidos por decompile):
- `Fight_TrySelectMedaforceAction` 0x080155A8 (bloqueia se != 0; MF.md §2): leitura em 0x080155BE.
- `Fight_DispatchOnSubstateAA_BR_40_CheckMedaforceRequirements` 0x08031614: se F+0x58 != 0, mensagem 0x79 (bloqueio), senão checa nível/medidor.
- HUD `Fight_UpdateBotInfoPanel` (só leitura).

Logo: **Force Bind ligado = F+0x58 != 0 = nenhuma medaforça (dos dois lados) até um segundo Force Bind desligar** (alterna).

## 2. Correção de nome: 0x08015648

Ghidra: `Fight_IsMedaforceBlockedByChapter` (não "BattleState" como em MF.md/mtbe).
Corpo: retorna 1 se `r0 >= 3` (bot inimigo) **e** o byte de capítulo (`gEventBlock` via literal 0x0801565C/0x08015660) `<= 2`; senão 0.
Ou seja: medaforça de inimigo é barrada nos capítulos iniciais. Isto é independente do flag F+0x58.

## 3. Passivos Cancel 24/25/26: ABERTO (não achado no código)

- Roteiros: 24 em 0x083C6A6D, 25 em 0x083C6A6E, 26 em 0x083C6A6F, todos só `01` (ActionScript_Op01_pendente).
  Ou seja, **não há script de ação**: o efeito, se existir, está num teste de defesa, não num roteiro.
- Conjunto anulado (REF_MEDAPEDIA, descrição de jogo, não ROM): 24→4,5 (Laser/Beam); 25→6,7 (Missile/Napalm); 26→8,9 (Break/Press).
- Busca feita:
  - Varredura Thumb `cmp rN,#imm8` (bytes `imm, 0x28|N`) em 0x08012000–0x0803F000 para imm 0x17..0x1A: 0x17 e 0x19 = **zero** acertos; 0x18 e 0x1A só em telas de ícone/preview (0x08016E7A, 0x0802FB7C, 0x08030188, 0x08037BD8..0x08038888, todos fora do teste).
  - `sub rN,#imm` + comparação seguinte, `mov rX,#imm` + `cmp rA,rX`: zero acertos úteis.
  - `Fight_AccumulateBaseDamage` 0x08033D30 (compara técnica do atacante 4/5, `attackerFlags`) e `Fight_ComputeTotalRos` 0x08038BC0: sem passivo do defensor.
  - Nomes: nenhuma função com "Cancel" ou "Passive" no Ghidra.
- Hipótese (provável, não fechada): o teste usa tabela indexada pela técnica (ou máscara), não comparação com imediato. Próximo passo: achar o leitor de `B+0x49+0x18k` com índice variável (loop k=0..2 sobre as peças do defensor) na rotina de dano/defesa (perto de `Fight_ResolveDefenderRosReductions` 0x08038E54 ou de `Fight_SplitDamageByPartArmor` 0x08034F68).

## 4. Consequência para o porte

- `guard.lua` `M.cancels`: conjuntos continuam **chute** (descrição de jogo). Não há regra real confirmada.
- Force Bind: porte fechado pelo roteiro acima.
