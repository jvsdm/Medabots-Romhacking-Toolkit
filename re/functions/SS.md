# SS: sistema de status da Robattle (Metabee EU)

Status: PARCIAL. Endereços: Ghidra (IWRAM). Bloco do bot = `0x03000590 + bot*0x100` (offsets do Ghidra, como em DF.md).
Confiança: C = confirmado no decompile/literal; P = provável; A = aberto.
Nomes dos tipos: tabela `pc/cache/text.lua` (ver §6): o índice do tipo é o mesmo id que aparece em +0xC9.

## 1. Dois mecanismos no bloco do bot

| Mecanismo | Campos | Quem aplica | Tick | Cura / fim | Ícone |
|---|---|---|---|---|---|
| Efeito pendente (status de peça, um por bot) | +0xC9 tipo; +0xCA estágio; +0xCB duração (dec. 4/tick); +0xCC valor | `Fight_SetPendingPartStatus` 0x08036E64 (renomeada aqui; alias do Ghidra `FightPreview_SetPartStatsByType`), C | `Fight_TickPendingEffectDurationAndEmitResult` 0x08018DF4 (C) | fim quando +0xCB-4 < 1: zera +0xC9..+0xCC, +0xCE \|= 2, flag 0x55 \|= 4; cura por AX_06 (`FightPreview_ClearPartFields(bot,1)`, C) | `Fight_DrawPendingEffectIcon` 0x08019720 (C) |
| Reparo secundário (AutoRecover) | +0xC4 tipo (0x1F); +0xC6 contador (dec. 4); +0xC7 valor por tick; +0xC8 total reparado | `Fight_DispatchOnSubstateAA_X_0E_InflictStatus1F` 0x08022C28 (C) | `Fight_TickSecondaryRepairEffectAndEmitResult` 0x08018D0C (C) | contador <= 4: zera +0xC4..+0xC7, +0xCE \|= 1 (C) | ícone "secundário" (kind 0) de `Fight_PlaceBotStatusIconObject` (R1) |

Tick geral (C): `Fight_TickActiveBotStatusEffects` 0x08018CBC, para os 6 bots com +0x40 != 0: se +0xC4 != 0 chama o reparo;
se +0xC9 != 0 chama a duração do pendente. Zera fight+0x55 (0x03001800+0x55, P) antes do laço; os ticks setam 2/4/8/0x10 nessa flag (C).

Medidor (C): `Fight_TickPendingEffectMeterGain` 0x0801922C soma +0xCC ao medidor do bot (`Fight_AddBotMeterCapped255`) se o tipo é 0x11, 0x12 ou 0x29.

Fight_TickBotSubsystems (R1, C): para bots ativos cujo tipo NÃO é 0x0F/0x10, roda os subsistemas (0x0801902C; 0x08019078 só o passo 0x08019544).

Dano por tick (C): tipos 0x13/0x14 (Fire/Melt) aplicam dano +0xCC nas peças intactas (`Fight_FindNextPartWithArmorForPendingEffect` 0x08018F6C, `Fight_ApplyPendingEffectDamageToPart` 0x08018FF0), até 4 peças por tick; +0xCD recebe o total do tick, +0xCE |= 8, flag 0x55 |= 0x10.

## 2. Tabela: status -> bits -> aplica -> tick -> cura -> ícone

Id = valor de +0xC9. Nome = tabela de nomes (§6).

| Id | Nome | Bit de proteção (+0xCF, AW) | Aplica (quem) | Tick / efeito | Cura | Ícone (`Fight_DrawPendingEffectIcon`) |
|---|---|---|---|---|---|---|
| 0x0D / 0x0E | Bug / Virus | var 0 = 0x01 (AW_09, C) | AV 0x03 (0x0D, C); BE (sorteio, P) | sem tick próprio (duração comum) | AX_06 (C) | grupo A (0x0D-0x0E, C) |
| 0x0F / 0x10 | Thunder / Freeze | var 1 = 0x08 (C) | AV 0x03 (0x10, C) | 0x0F/0x10 ficam FORA dos subsistemas do bot (R1, C); efeito "congelar" = P | AX_06 | grupo D (0x0F-0x10, C) |
| 0x11 / 0x12 | Wave / Hold | var 2 = 0x02 (C) | AV 0x03 (0x12, C) | 0x11/0x12 somam +0xCC ao medidor (C) | AX_06 | grupo B (0x11-0x12, C) |
| 0x13 / 0x14 | Fire / Melt | var 3 = 0x04 (C) | AV 0x03 (0x13, C) | dano por tick nas peças (C, ver §1) | AX_06 | grupo C; 0x13 e 0x14 têm sprites diferentes (C) |
| 0x29 | Drain Chrg (")" ASCII) | - | técnica de carga (P) | soma +0xCC ao medidor (C) | - | grupo B (C) |
| 0x2B | Impair ("+") | - | BK 0x0E (`Fight_DispatchOnSubstateAA_BK_0E_SetPendingStatus2B` 0x0802DBD8, C) | trava a ação da peça (`Fight_IsPartActionLockedByPendingEffect` 0x080154E0, C) | AX_06 | "+" (C) |
| 0x2C | Confusion (",") | - | AV 0x03 (C) | `Fight_AssignBotTargetSlot` 0x0801B69C troca o alvo (C, nome do mtbe) | AX_06 | "," (C) |
| 0x2E | No Escape (".") | - | AV/técnica (A: não achado o aplicador) | BYPASS_EVASION (mtbe, C) | - | "." (C) |
| 0x2F | No Defense ("/") | - | A | A (efeito de defesa não lido) | - | "/" (C) |
| 0x1F | AutoRecover (+0xC4, não +0xC9) | - | X_0E 0x08022C28 (C) | reparo por tick (C) | fim por contador | secundário (R1) |

Ícones (C): `Fight_DrawPendingEffectIcon` 0x08019720 usa slot 0x47+3*bot (stride 0x40). Pelo que o despachante lê, o caractere
do ícone é o próprio tipo nos casos ASCII (0x2B..0x2F).

Não são status pendentes (C, pelo despachante de ícone e pelo tick): Scout (36), Conceal (37), Grap/Shot Trap (10/11),
Boost/Rapid/Accel Chrg (38-40). São técnicas: Scout soma bônus de suporte em E8 (DM, C); as armadilhas têm animação própria
(`TrapFamily_Handler`, HN5). "Speed up" não aparece na tabela de nomes: A.

## 3. Proteção contra status (AW, C)
- Variante da técnica (`Fight_DispatchOnSubstateAA_AW_00_SelectStatusTypeFromTechnique` 0x08028C68): +0xA8 = 0x0D -> 0, 0x0F -> 1, 0x11 -> 2, 0x13 -> 3 (grava em fi+0x51).
- `..AW_09_ApplyStatusGuardAndCure` 0x08029354: para cada aliado marcado, liga em bot+0xCF o bit da variante (0x01/0x08/0x02/0x04);
  se o pendente é o par da variante (0x0D/0x0E, 0x0F/0x10, 0x11/0x12, 0x13/0x14), zera com `FightPreview_ClearPartFields(bot,1)`.
- Leitor da máscara +0xCF (quem bloqueia o status novo): A (não achado).
- Cura de todos os aliados: AX_02 marca (cabeça inteira e +0xC9 != 0) e AX_06 `0x080297EC` limpa.
- Imunidade a AV 0x03: técnica 0x16 (HN2, C).

## 4. Flags de técnica no bot (+0xD6, bloco do bot, C)
- `Fight_DispatchOnSubstateAA_AS_SetStatusFlagByTechnique` 0x08027E1C: bot+0xD6 |= 0x01 (tec 4), 0x08 (5), 0x02 (6), 0x10 (7), 0x04 (8); 0x20 outras.
- DM (C): bits de +0xD6 são bônus do ATACANTE (bit0 +poder, bit1 sem "zero-miss", bit2 +sucesso). Não é status pendente.

## 5. Bits de +0xBD, +0xBF, +0xC0, +0xDE/+0xDF (ícone de peça)
- +0xBD bit 0x08 (C, HN1 e HD): ligado pelo efeito W no ator (`Fight_DispatchOnSubstateAA_W_07_ApplyActorChargeBuff` 0x08022748, mtbe).
  `Fight_DrawPartStatusIcon` 0x08019B40 (C): com 0x08 ou 0x10 ligado, desenha o ícone só se a armadura da peça (+0xDE-1) != 0; tipo +0xDF escolhe o sprite (0x1B/0x1C/0x1D têm sprites próprios; demais = padrão). Slot 0x48+3*bot.
- +0xBD bit 0x10 (C, HA8 e 0x08019B40): segundo caminho do mesmo ícone de peça (mesma checagem de armadura). Quem usa cada bit ainda não separado (A).
- +0xBD bit 0x04: espelha o indicador no eixo X conforme o lado (`Fight_ShouldMirrorReactionIndicatorX` 0x08015924, C).
- +0xDF (C, ícone): tipo da técnica da peça; 0x1B, 0x1C, 0x1D escolhem sprites diferentes no ícone de peça.
- +0xDE (C no decompile): índice 1-based da peça do ícone. DF.md cita +0xDD para a peça escolhida: conflito em aberto.
- +0xBF: escrito por HA6 (`0x03000590+bot*0x100+0xBF`, C na escrita); significado A.
- +0xC0: bit 2 = reaction_state_flags (HN2, C). HA8 dobra o limiar se o bit 0x08 de +0xC0 está ligado (P: HA8 diz 'bit 8').

## 6. Nomes dos tipos (tabela de texto)
A tabela `pc/cache/text.lua` (cabeçalho "WeaponTypeNames", ptr 0x003BE868) traz os nomes dos ids 13..47 de 0x0D a 0x2F:
13 Bug, 14 Virus, 15 Thunder, 16 Freeze, 17 Wave, 18 Hold, 19 Fire, 20 Melt, 31 AutoRecover, 41 Drain Chrg, 43 Impair,
44 Confusion, 46 No Escape, 47 No Defense; 36 Scout, 37 Conceal (técnicas). O cabeçalho diz "tipos de arma", mas os ids batem
com os tipos de status do decompile (ex.: 0x1F = AutoRecover, 0x2C = Confusion): usar com cuidado.

## 7. Aberto
1. Quem liga +0xCF no bloqueio (leitor de máscara) e a regra de "imune" por variante.
2. Aplicador de 0x2E (No Escape) e 0x2F (No Defense); efeito de 0x2F.
3. "Speed up" e "Scout/Conceal" como status: não achados.
4. Conflito +0xDD x +0xDE para a peça do ícone.
5. Fight_TickBotSubsystems e a diferença 0x0F/0x10 (congelar) ainda sem texto do efeito.

Arquivos: `SS_simbolos.csv`. Nada gravado no Ghidra, exceto a renomeação de 0x08036E64 (não salvo).
