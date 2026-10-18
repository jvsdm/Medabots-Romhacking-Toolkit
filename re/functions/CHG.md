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

## 7. Rodada CHG2 (Ghidra, 2026-10-17)

Endereços de entrada: ver `CHG_simbolos.csv` (linhas CHG2). Porte: `pc/game/src/fight/timing.lua` (`meterOfFrame`,
`motionSpeed`, `isFrozen`) e testes `pc/game/tests/test_timing.lua` (OK 32).

### 7.0 Correções ao texto acima
- **A1 = bot+0xA1 = armadura atual da perna** (`parts[3].currentArmor`; 0x03000631 = bot+0xA1). "+6 se A1 == 0" =
  **perna destruída** soma +6 ao medidor (0x080191C0). Confirmado (decompile).
- Bases das peças (stride 0x18): cabeça bot+0x48, braço D +0x60, braço E +0x78, perna +0x90. Logo: +0x49 técnica,
  +0x4E power, +0x50/+0x51 usos/radiação da **cabeça**; braço D +0x68/+0x69; braço E +0x80/+0x81.
  A seleção só lê os bytes de **braço** (ver 7.2); a cabeça seleciona +6 fixo. Confiança: alta (bate com 0x03000631 e
  0x03000651/0x0300064B).
- **GS 76 (0x08014548, `Fight_UpdateBotMeterMotionStep`)**: para bot com presença e bot+0xBF != 0 faz **bot+0xBB = 8**
  antes de `Fight_UpdateBotScreenXFromMeter`. Logo nesse estado o andar é **(0x20−8)/16 = 1,5 px/q**, sem depender do
  medidor real. Confirmado (decompile). Que situações passam por GS 76 (ida/volta?) = ABERTO; a medida de volta (1,3–1,45
  px/q) fica perto de 1,5.

### 7.1 Espera em casa: +8/+9 da peça viram tempo? (1)
- **Quem lê +8 (usos/carga) e +9 (radiação):** nos decompiles lidos, só `Fight_TickSelectedPartCue` (0x08019108). Para a
  peça selecionada (braço) e viva, soma **+usos (+8)** ao medidor, ou **+radiação (+9)** se `reactionStatusFlags & 4`.
  Confirmado (código). Não achei outro leitor desses bytes; a busca não foi exaustiva (só decompiles lidos).
- **Como vira tempo:** +8/+9 entra no medidor do quadro → a velocidade de carga cai para (0x20 − medidor)/16 px/q. Ou seja,
  cada ponto de medidor é uma parcela do tempo de carga, não o tempo da espera. Confirmado.
- **Espera em casa (172 q): NÃO fechada.** O que foi lido:
  - Chegada em casa: `Fight_CheckBotYAtRestPos` (0x08019FBC) → X = 0x68 (bots 0..2) ou 0x88 (3..5) e sem flag 4 →
    `Fight_QueueBotEvent` com o **param = propulsão da perna**. A fila (`fi+0x45`, 6 pares) é **ordenada pelo param**
    (`Fight_SortBotEventQueue` 0x0801A5D8) e consumida por `Fight_BotEventQueueStep` (0x08014314). Logo a propulsão aqui é
    **chave de ordem**, não contador de tempo. Confirmado.
  - Não é o medidor: quando o bot está em casa, X já está no limite e não anda.
  - Contadores de espera achados: `Fight_CountWaitTimerUp16ThenAdvanceStep` (0x08014AB4, sobe até 0x10) e
    `Fight_WaitTimerDownThenAdvanceStep` (0x08014E54, desce até 0). São de 16 q, não de 172. Quem carrega o valor de
    struct+0x86 no caso de 172 = **ABERTO**.
  - Hipótese da radiação: **não sustentada** pelo que foi lido (+9 só entra no medidor). Não testada.
- Conclusão: a espera de 172 q segue como constante medida (`COOLDOWN_DEFAULT`). A fórmula não é do medidor.

### 7.2 Termos positivos do medidor (+0xBB) (2)
| Termo | Função | Peso | Confiança |
|---|---|---|---|
| zera | 0x080190B8 | BB = 0 a cada quadro | confirmado |
| cabeça viva, selecionada (src 1) | 0x08019108 | +6 | confirmado |
| braço D/E vivo, selecionado (src 2/3) | 0x08019108 | +usos (+8) ou +radiação (+9) se flag 4 | confirmado |
| selecionado src ≥ 4 | 0x08019108 | +12 fixo (sem checar peça) | confirmado (quem é o id 4+ = ABERTO) |
| perna destruída (A1 = 0) | 0x080191C0 | +6 | confirmado |
| tabela `[(bot+0x91−100)*11 + fi+0x38 + k]` | 0x080191E4 | + byte da tabela | código confirmado; **tabela não lida, fi+0x38 ABERTO** |
| pendente tipo 0x11/0x12/0x29 | 0x0801922C | +CC do pendente | confirmado |
| nível (B+5) /10 | 0x08019264 | −(nível/10) | confirmado |
| propulsão da perna /20 | 0x0801928C | −(propulsão/20) | confirmado |
| Boost (C4 = 0x26 ou 0x29) | 0x080192B8 | −C7 | confirmado |
| Accel (técnica 0x28 em parte 0..2 viva) | 0x080192E8 | −power da peça | confirmado (loop só cobre 0..2, não a perna) |
| teto 0x1F | 0x0801932C | limite final | confirmado |
Ordem: somas (cap 255) antes das subtrações (piso 0 em cada uma, equivale a clamp final). Porte: `M.meterOfFrame`.
Sem data de tabela, o medidor não é exato: o termo 0x080191E4 fica em `bot.table_add` (default 0).

### 7.3 Técnicas de carga 38, 39, 40 (3)
- **38 Boost Chrg (0x26):** `Fight_CommitBoostChargeSecondaryEffectToMarkedTargets` (0x0802CB34) grava bot+0xC4 = 0x26,
  +0xC5 = 0, **+0xC6 = clamp(ROS total fi+0xE8, 1..99) (duração)**, **+0xC7 = clamp(attackAccum fi+0xEC, 1..99) (valor)**.
  Depois, a cada quadro, −C7 no medidor (0x080192B8). **Não zera a carga**. Confirmado (escrita e medidor). Onde C6 cai
  a cada quadro = ABERTO (provável no tick de status).
- **40 Accel Chrg (0x28):** passiva por peça. Se uma **cabeça ou braço** (partes 0..2) com técnica 0x28 está viva, −power a
  cada quadro (0x080192E8). **Não mexe em C7 nem zera**. A perna não conta. Confirmado.
- **39 Rapid Chrg (0x27):** **não achado**. Nenhuma função de medidor testa 0x27 (0x0801922C, 0x080192B8, 0x080192E8). Nome
  "Charge/Chrg" só retorna Boost. 0x0802A824 (`MarkOpponentsSetGauge`) põe BB = 0x14 nos adversários, mas ligado ao 0x5A
  (não 39). ABERTO. Hipótese: efeito fora do medidor (ROS/dano). Não confirmada.
- **"Zera a carga"?** Nenhuma função lida zera BB além do início do quadro (0x080190B8) e de GS 76 (que põe 8). Confirmado.

### 7.4 Propulsão: qual perna? (4)
- 0x0801928C lê `parts[3].propulsion` (= bot+0xA2) **sem checar armadura**: é a propulsão da perna que está no slot
  (a da perna atual). Perna destruída **continua** descontando (propulsão) e ainda soma +6 (A1 = 0). Confirmado.
- Trocar a perna muda a propulsão a partir do próximo quadro (o byte é relido a cada quadro). Confirmado pelo código.
  Confiança média em "trocar = relido": não testado no emulador.

### 7.5 Para o Opus
- `battle.lua` (não editado) passa `b.propulsion` e `b.pending_type`. Para a fórmula completa, o bot precisa de
  `selected`, `reaction_flags`, `parts[k].hp/uses/radiation/power/technique`, `level` e `boost` (C4/C6/C7). Sem isso,
  `meterOfFrame` cai para 0 nos termos de seleção.
- `chargeSpeed` continua (0x20 − medidor)/16. `motionSpeed()` = 1,5 px/q (GS 76). `cooldownFrames` = 172 (ABERTO).

## 9. Rodada B4 WAIT (2026-10-18): espera em casa e ritmo do tick de status

Método: Ghidra (leitura, sem salvar) + replay headless de rec_2 com watch em 0x0300137C (GS da frame de luta),
0x03000440+0xC0 (ctx T), 0x03000590+0x1370 (bots + fight). Porte: `timing.lua` (`HOME_WAIT_REC2/REC5`), `status.lua`
(`TICK_FRAMES`, `ticksToEnd`, `ticksInFrames`, `M.tick`), testes `test_timing` (OK 34) e `test_status` (OK 94).

### 9.1 Ritmo do tick de status (confirmado no decompile + watch)
- `Fight_StepBattleFrame` 0x0801427C: se fi+0x55 == 0 chama `Fight_TickSecondaryEffectTimers` 0x08018C58.
- `Fight_TickSecondaryEffectTimers`: `fi+0x56 = (fi+0x56 + 1) & 0x1F`; quando vira 0, se `fi+0x57` (timeUp) != 0:
  `timeUp -= 1`, sobe o relógio de exibição e chama `Fight_TickActiveBotStatusEffects` 0x08018CBC.
- `Fight_TickActiveBotStatusEffects`: para cada bot com presença: reparo (se +0xC4 != 0), depois
  `Fight_TickPendingEffectDurationAndEmitResult` 0x08018DF4 (se +0xC9 != 0). Não pula 0x0F/0x10 (Stop também leva tick).
- 0x08018DF4: se `dur - 4 < 1` → limpa o pendente, **sem aplicar efeito neste tick**; senão `dur -= 4` e, só para
  0x13/0x14 (Fire/Melt), aplica o dano.
- **Ritmo:** 1 tick a cada 32 quadros em que a frame de luta roda (GS 0x44). NÃO é por volta nem por segundo.
  Medido em rec_2: fi+0x56 anda a cada quadro em GS 0x44 (2381..2400), fica parado durante a espera (GS 4..9, 2401..2574),
  e vira 0 em 2576 — o mesmo quadro em que fi+0x57 cai de 38 para 37 e em que o bot sai para a nova carga.
  Logo: **zero ticks durante a espera em casa**; a duração de Stop/Bind/Burn congela enquanto o bot está em casa.
- Efeito do porte: `M.tick` agora segue a ROM (expira antes do efeito); Wave/Hold/Drain não somam medidor no tick
  (a ROM soma +CC por quadro em 0x0801922C, já em `timing.meterOfFrame`). Duração 5 → 2 ticks; 20 → 5 ticks (4 com dano).
- Chute antigo do porte ("1 tick por volta", "1 por segundo no Stop") está **errado**: o tick é 1/32 quadros de GS 0x44.

### 9.2 Espera em casa: o que o watch mostrou (GS 0x0300137C)
- Chegada: GS 4 em 2405 (rec_2). Sequência medida em rec_2 (quadros): 4..6 = 22 | **7 = 108** | 8 = 20 | 9 = 16 |
  0x38, 0x0D, 0x0E, 0x45, 0x44 = 6 → nova carga em 2576. Soma 22+108+20+16+6 = **172**.
- Os 20 q do estado 8 e os 16 q do estado 9 aparecem no ctx T +0x84 (0x030004C4): byte que anda 1 por quadro e
  zera ao passar de 0x13 (estado 8), e chega a 0x10 no fim do estado 9. O contador de 16 em +0x86 (0x030004C6,
  `Fight_CountWaitTimerUp16ThenAdvanceStep` 0x08014AB4 / `Fight_WaitTimerDownThenAdvanceStep` 0x08014E54) **não** anda
  nessa janela: o contador 0x08014AB4/0x08014E54 não é o da espera.
- **Não é constante entre bots:** rec_5 mostra 4..6 = 21 | 7 = 101 | 8 = 20 | 9 = 16 e, antes da carga, 0x40..0x42
  (94 q) que não aparece em rec_2 (evento a mais, não explicado). O estado 7 é o que muda (108 vs 101). Hipótese (não
  testada): o estado 7 espera uma animação/pose do bot (duração de script), porque o estado 7 não gasta tick nem medidor.
- Não foi achado o quadro de chegada/ida do estado 7 nem quem escreve 0x38/0x0D/0x0E no GS (são escritos por handlers
  de cena compartilhados com outras telas). **ABERTO**: fórmula da espera. `cooldownFrames` continua 172 (medido, rec_2).
- Radiação (+9) e carga (+8) não entram nesses números: o medidor não governa a espera (confirmado pelo watch: bot
  fica com X fixo, D7 segue andando sem efeito).

### 9.3 Para o Opus (integração em battle.lua, não editado)
- Chamar `status.tick(bot)` **uma vez por cada 32 quadros de GS 0x44** (contador próprio por luta, não por volta).
  Enquanto o bot está em casa (estados 4..9), não chamar.
- Duração do pendente em ticks: `status.ticksToEnd(bot.pending.duration)`.
- Espera: `timing.cooldownFrames(bot, k)` (172) até fechar o estado 7.
