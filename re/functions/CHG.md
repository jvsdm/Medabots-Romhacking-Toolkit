# CHG: tempo da Robattle (carga do bot até a linha central, volta, espera em casa)

Escopo: quem anda o bot no campo, de onde vem a velocidade, o que é a espera, e como 38–41 e o status 15/16 entram.
Fonte: decompile já no CSV + desmonte da ROM EU (`py -3` com capstone, helper do scratchpad; Ghidra MCP estava
recusando conexão em 8080 nesta rodada, então nada foi renomeado nem salvo). Data: 2026-10-17.
Ver também `pc/game/src/fight/timing.lua` (porte) e `pc/game/tests/test_timing.lua` (OK 15).

## 1. Quem anda o bot (confirmado)

Loop por quadro: `Fight_StepBattleFrame` 0x0801427C (GS 68 da tabela de passos) chama, em ordem, `Fight_TickActiveBotsExceptStatusTypes0F10`
0x0801902C, `Fight_DrawActiveBotsOverlay`, ... (0x080142E8). O laço 0x0801902C faz, para cada um dos 6 bots:

1. Se o bot está ativo (+0x40 != 0) **e** o tipo pendente (+0xC9) não é 0x0F nem 0x10: roda
   `Fight_TickBotSubsystems` 0x080190B8 (zera +0xBB e soma/subtrai), `FUN_08019348` (= `Fight_UpdateBotScreenXFromMeter`,
   move X) e `Fight_UpdateBotMeterDisplayObject` 0x08019544 (só exibição).

Ou seja: **a caminhada é o próprio medidor +0xBB recalculado a cada quadro**. Não há acumulação de medidor entre quadros:
0x080190B8 zera +0xBB no começo.

### Movimento (desmonte de 0x08019348, confirmado)
```
D7 = (D7 + 0x20 - BB) & 0xFF      ; +0xD7, acumulador
DA = D7 >> 4                       ; +0xDA, passos de X neste quadro (0..2)
D7 = D7 & 0xF
X += DA (sentido pelo bit 4 de +0xBD), com limite: bots 0..2 entre 0x28 e 0x68; bots 3..5 entre 0x88 e 0xC8
```
Como BB ∈ 0..31 (clamp 0x0801932C), DA ∈ {0,1,2} e a **média é (0x20 − BB)/16 px por quadro**:
BB=0 → 2 px/q; BB=9 → 1.44 px/q; BB=31 → 1/16 px/q.

### Sentido (confirmado)
- 0x080143A0 `Fight_ToggleBotFlagAdvanceStep` inverte o bit 4 de +0xBD do bot ativo (chamado no início do retorno).
- A volta usa a mesma regra (`Fight_UpdateBotScreenXFromMeterInverted` 0x08019440, mesmo cálculo, sinal invertido).
  Bate com M2: "retorno = andar de volta na mesma velocidade da carga" (~1.3–1.45 px/q).

### Termos que mexem em +0xBB (todos lidos no código, por quadro)
| Termo | Função | Efeito | Confiança |
|---|---|---|---|
| zera | 0x080190B8 (início) | BB = 0 | confirmado |
| +6 se byte +0xA1 == 0 | 0x080191C0 | sobe (mais lento) | confirmado (código); significado de A1 = ABERTO |
| +6 peça 1 selecionada / +0x50 ou +0x51 da peça; +12 se DD >= 4 | 0x08019108 | sobe (seleção) | confirmado código; fonte exata dos +0x50/+0x51 ABERTO |
| +CC do efeito pendente, se tipo 0x11, 0x12 ou 0x29 | 0x0801922C | sobe (Wave/Hold/Drain Chrg) | confirmado código |
| tabela (bot+0x91, nível, +0x38) | 0x080191E4 | sobe | parcial (tabela não lida) |
| −(byte +5)/10 | 0x08019264 | desce (nível, ver nota) | confirmado código; nome "nível" provável |
| −(byte +0xA2)/20 | 0x0801928C | desce | **propulsão** confirmado (BattleBot_Legs_Propulsion = 0x03000632 = bot+0xA2) |
| −(+0xC7) se efeito +0xC4 = 0x26 ou 0x29 | 0x080192B8 | desce (Boost Chrg / Drain Chrg) | confirmado código |
| −(peça +0x4E) se peça +0x49 == 0x28 e viva | 0x080192E8 | desce | confirmado código; nome do +0x49 ABERTO |
| teto 0x1F | 0x0801932C | limita | confirmado |

Consequência para o porte: **propulsão acelera a carga** (desconta /20 do medidor) e a regra de andar é a do item acima.
Nível (+5/10) também desconta; é o que o porte deve usar no lugar do chute `30 + propulsão*0.6`.

## 2. Congelar: status 15/16 (confirmado, nome ABERTO)
0x0801902C pula qualquer bot cujo +0xC9 é 0x0F ou 0x10 (SS.md: Thunder/Freeze, ids 15/16 no README). Nesses quadros
o bot não anda, não recalcula medidor e não atualiza exibição. É o "Stop" do roteiro do PT: **Stop = congela a carga**.
(Nome do status: a tabela SS.md diz Thunder/Freeze; o README/roteiro chama de Stop. Mesmo id, mesmo efeito de código.)

## 3. Técnicas de carga 38–41
- **41 Drain Chrg** = status 0x29 `)`: +CC ao medidor (0x0801922C) e −C7 no medidor (0x080192B8). Sem tick próprio.
  Confiança: confirmado (código); o valor de CC/C7 vem do efeito (`fi+0xE8`/`fi+0xEC`, ver MF4 0x61).
- **Boost Chrg** (`Fight_CommitBoostChargeSecondaryEffectToMarkedTargets` 0x0802CB34, via BI 0x0802C644 / id 0x61):
  grava +0xC4 = 0x26, +0xC6 = duração (fi+0xE8), +0xC7 = valor (fi+0xEC). O valor é descontado do medidor
  (0x080192B8) → **acelera a carga** enquanto dura. Confiança: confirmado (escrita) / provável (que 0x26 = "carga boost").
- **Rapid/Accel Chrg** (38–40): não localizei o handler que grava +0xC4/+0xBB para elas. **ABERTO**.
- Técnica 0x5A (`Fight_DispatchOnSubstateAA_BB_MarkOpponentsSetGauge` 0x0802A824): põe bot+0xBB = 0x14 nos
  adversários vivos. Confirmado (escrita de 0x14 em +0xBB). É o caso de "medidor de técnica cheio de 20" do README.

## 4. Espera (resfriamento) — ABERTO
- Medido (M2, rec_2): chegada em casa em 2404, nova carga em 2576 (2576 − 2404 = 172).
  **Espera em casa = 172 quadros** (uma amostra, bot Metabee, peças do Kabuto/Metabee).
- A espera **não** é o +0xBB (o bot em casa já tem X fixo no limite, o medidor não governa a espera).
- Hipótese (NÃO confirmada): radiação (+9 da cabeça/braço) entra no tempo de espera, já que o README chama
  +9 de radiação e o roteiro fala em "resfriamento". Não testado; sem outra amostra não dá para fechar a fórmula.
- Estado de GS na espera: fi+0x55 bits 1..4 → GS 0x60/0x68/0x70/0x78 (BT.md). Não localizei o contador que conta 172.

## 5. Confiança geral
- Fórmula de caminhada (2 px/q em BB=0; (0x20−BB)/16 média): **confirmada** no desmonte; BB real durante a carga
  do Metabee ≈ 9 (fecha com M2 1.45 px/q), mas a soma exata dos termos do bot de M2 não foi conferida no emulador.
- Propulsão e nível no medidor: **confirmados** (sinal e divisor).
- Espera 172 q: **medida**, fórmula **ABERTO**.

## 6. Para o Opus (integração)
- `battle.lua` tem `walkSpeed = 30 + prop*0.6` e `COOLDOWN_FRAMES = 172` (chutes). Trocar por
  `timing.chargeSpeed(bot)` (px/q) e `timing.walkStep(state, meter)` (passos inteiros, igual ao jogo) e
  `timing.cooldownFrames(bot, k)` (172 ABERTO).
- `timing.isFrozen(bot)` = status 15/16 (congela carga).
- Não integrado: `battle.lua` não foi editado.
