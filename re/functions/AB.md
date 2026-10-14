# AB: despachantes B..G do roteiro de acao (opcodes 0x04, 0x05, 0x07, 0x08, 0x09, 0x19, 0x1A) e pendencias do AS (GH-AB, 2026-10-14)

Leitura estatica: ROM original so leitura (python -I, tabelas) + decompile/disasm do Ghidra (sem salvar). Sem emulador.

## Mapa opcode -> despachante -> tabela (ROM)

| op | wrapper | despachante | tabela (4 B/entrada) | entradas |
|----|---------|-------------|----------------------|----------|
| 0x04 | 0x080339F4 | A | 0x083AD5CC | 0x30 |
| 0x05 | 0x08033A10 | B | 0x083AD68C | 9 |
| 0x07 | 0x08033A78 | C | 0x083AD6B0 | 8 |
| 0x08 | 0x08033A94 | D | 0x083AD6D0 | 8 |
| 0x09 | 0x08033AB0 | E | 0x083AD6F0 | 16 |
| 0x19 | 0x0803402C | F | 0x083AD730 | 8 |
| 0x1A | 0x08034048 | G | 0x083AD750 | 16 |

Stub (0x20 B, padrao de HA1): zera +0xB5 do bloco 0x03001800 e chama `table[+0xAA]` (trampolim 0x0807CCF0). Cada handler roda
enquanto o subestado (+0xAA) e o seu indice; "+1" avanca, "hold" so poe +0xB5 = 1 (ou espera). Os wrappers 0x04..0x1A NAO
avancam o cursor do roteiro; ele so avanca quando o handler do opcode ve +0xB5 = 1 (padrao de 0x51/0x59).

Placeholders (0x08000841 em B04..08, C04..07, D04..07, E0C..0F, F03..07, G07..0F). Nao sao handler. Hipotese: subestados
que nenhum fluxo alcanca (nao testado no emulador).

Alvo generico 0x080305E1 (Thumb de 0x080305E0, `ActionFamily_StatusCheckFamily_Handler`): chama
`Fight_RunSubstateDispatch(+0xA6)` (sub-maquina de mensagem, FH.md); se der != 0, +1. Alvo 0x08030609 (Thumb de 0x08030608,
`ActionFamily_TrapFamily_Handler`): igual, mas com `Fight_RunSubstateDispatchB()`. Usado em B01, C01 (TrapFamily) e
B02, C02, F01, E0A (StatusCheck).

## Q1: efeito de cada despachante (como tecnica)

Papel comum: +0xA6 = codigo de mensagem (indexa 0x083C6744, FH/A6); +0xAB = codigo de resultado (0, 1 ou 2) que o opcode 0x03
usa para escolher o byte do roteiro (Q2).

### 0x04 = A (apresentacao de dano/ROS e resultado), resumo
Sequencia ja documentada em HA1: pose e tiles (04/22, 06, 1A/1C), numero (0A), subtrai armadura da peca (0C), backup de stats
(0D), slot de valor (1D/21/24), encerramento com +0xAB = 0 (A2D), 1 (A2E) ou 2 (A2F). Efeito de tecnica: aplica o dano na peca e
devolve resultado 0/1/2 para o roteiro ramificar. Confianca: provavel.

### 0x05 = B (ramo pela peca-fonte), confirmado no fluxo
- B00 (0x0801D318): indice = bot+0xDD-1; se a flag da peca-fonte (0x03000590 + slot*0x100 + 0x59 + 0x18*indice) e 0:
  +0xA6 = 5 e +1 (vai para B01). Senao: +0xB5 = 1 e +0xAB = 0 (fim, ramo 0).
- B01 (TrapFamily): roda a sub-maquina de mensagem; quando termina, +1.
- B02 (StatusCheck): roda a mensagem por +0xA6 (= 5); quando termina, +1.
- B03 (0x0801D36C): +0xB5 = 1, +0xAB = 1 (fim, ramo 1).
Tecnica: "se a peca que originou a acao esta quebrada, mostra a mensagem 5 e segue pelo ramo 1; senao, ramo 0".
Confianca: confirmado (escritas lidas); semantica "quebrada" = provavel (o nome da flag e inferencia).

### 0x07 = C (ramo pela peca-alvo), confirmado no fluxo
Igual ao B, mas testa a peca do ALVO (bot+0x88 * 0x100 + 0x59 + 0x18*(+0x8B)). Flag 0: +0xA6 = 0x0C, +1 (C01 TrapFamily,
C02 StatusCheck, C03 = ramo 1). Senao: ramo 0 com +0xB5 = 1. Tecnica: "se a peca do alvo esta quebrada, ramo 1".
Confianca: confirmado (fluxo); semantica provavel.

### 0x08 = D (apresentacao de sprite de um bot de apoio)
- D00 (0x0801D434): CpuFastSet de dados (ponteiros de literal nao lidos), +1.
- D01 (0x0801D45C): Tile_ClearSlots(9), posiciona dois bots, monta slot 9 com a lista do literal, +1.
- D02 (0x0801D524): Fight_StepSlotTowardTarget(9) ate chegar; +1.
- D03 (0x0801D544): roda anim do slot 9 ate -1, limpa slot 9, +0xB5 = 1 (+0xAB nao mexido).
Tecnica: entrada visual de um sprite no slot 9. Sem efeito em dano nem ramo. Confianca: confirmado (fluxo); papel provavel.

### 0x09 = E (follow-up: bot aliado entra e age encadeado)
- E00 (0x0801D5A8): acha fonte de acao aliada; grava 1-based em fight+0xDE e tipo em +0xDF; +1.
- E01 (0x0801D6A4): carga de grafico conforme o tipo +0xDF (0x1B/0x1C/0x1D = CpuFastSet); +1.
- E02 (0x0801D738): copia 4 blocos de 0x40 do bot de +0xDE para buffers; +1.
- E03 (0x0801D878): anima a entrada do bot, 60 quadros (contador 0x030004C4), sfx 0x40; +1 ao chegar.
- E04 (0x0801D95C): anda o slot ate o ponto; +1 so ao chegar.
- E05 (0x0801D9DC): desliza o Y do bot ate 0x78; +1 ao chegar (retorno normal, nao tabela; JT).
- E06 (0x0801DAEC): sobe o slot, sfx 0x41 e composite; +1.
- E07 (0x0801DC74): espera contador 0x1E, cria slot 0x21; +1.
- E08 (0x0801DD30): espera byte 0x03003478 chegar a 0x40, sfx 0x30; +1.
- E09 (0x0801DD74): roda anim do slot 0x21 ate -1; grava +0xA6 = 0x16; +1.
- E0B (0x0801DDDC): marca o alvo (bot/parte) e +0xB5 = 1 (hold).
Tecnica: "companheiro entra e age junto": escolhe o aliado, carrega e anima o sprite, marca o alvo. O dano em si vem depois
pelo roteiro (opcodes 0x06..0x35). Confianca: confirmado (fluxo de cada passo); nomes = provavel.

### 0x19 = F (copia de dados da peca)
- F00 (0x0801DE44): copia dois blocos de 14 unidades (indice por +0xA7*0x18 e +0x87*0x100, offsets +0x4A/+0x49, tabela de
  ponteiros nao decodificada) para destino e destino+0x14; +0xA6 = 0x17; +1.
- F01 (0x080305E1): StatusCheck, roda mensagem 0x17; +1 ao terminar.
- F02 (0x0801DED4): so +0xB5 = 1 (hold).
Tecnica: prepara os dados da peca para a acao; sem efeito direto. Confianca: confirmado (copias, +0xA6, +1); conteudo copiado provavel.

### 0x1A = G (cena de fundo de tecnica, preview)
- Antes, o wrapper 0x08034048 grava 1 em DAT_0803406C+0x130 (flag de preview).
- G00 (0x0801DF0C): zera paleta de fundo e preenche tiles (fill, sem espera); +1.
- G01 (0x0801DF60): 10 quadros, desliza sprites laterais (contador 0x030004C4); ao fim flag +0x1AE = 1; +1.
- G02 (0x0801DFE0): carga de fundo em 3 etapas por quadro (CpuFastSet); +1 so na etapa 2.
- G03 (0x0801E09C): 2 etapas: BGs e cenario, depois graficos de bot para OBJ e blend; +1 na etapa 1.
- G04 (0x0801E200): reset dos sprites laterais; +1.
- G05 (0x0801E248): 10 quadros, entrada dos sprites laterais; +1 ao fim.
- G06 (0x0801E2B8): um passo dos sprites laterais; +0xB5 = 1 (hold).
Tecnica: monta e anima a cena visual da tecnica (fundo e sprites). Nao muda dano nem ramo. Confianca: confirmado (fluxo).

## Q2: opcode 0x03 (LEN 4): CONSOME os 4 bytes (confirmado)

Disasm/decompile de 0x08033974 `Fight_WaitForActionScriptChoiceByteMatch`:
1. `Fight_ReadActionScriptInlineArgs` 0x08034EB8 copia LEN[0x03] = 4 bytes do roteiro para +0xAC..+0xAF.
2. `Fight_AdvanceActionScriptCursor` 0x08034F08: cursor (+0xBC) += 1 + 4 = 5. Os 4 bytes sao lidos e pulados.
3. Laco: compara o byte +0xAC + (+0xAB) (um dos 4, escolhido por +0xAB) com o byte do roteiro no cursor; se nao casa,
   avanca o cursor por 1 + LEN[op] e repete; se casa, avanca e sai.
4. Ao sair, +0xB5 = 1 (fim do passo).
Resumo: 0x03 e um switch de 4 ramos. +0xAB (0/1/2 dos despachantes) escolhe o byte esperado. Confianca: confirmado (disasm).
Divergencia: DM.md §5 dizia LEN[0x03] = 1; ROM e este disasm dizem 4 (ja apontado em AS3a).

## Q3: opcode 0x01 (0x080338DC): grava 8 em 0x03002B8C e 0x40 em 0x0300137C

- Decompile (confirmado): `ActionScript_Op01_SetSceneExitState`: `*0x03002B8C = 8; *0x0300137C = 0x40;` e retorna.
  Nao chama AdvanceCursor e nao poe +0xB5. Como o laco nao avanca, o efeito de saida precisa vir da troca de cena/estado.
- Comparacao: op 0x3F (0x08034644, AS2) grava cena 8 e GS 0 ("saida do combate") se +0xB5 e +0xAB != 1. O 0x01 faz o mesmo
  com GS 0x40. Todo segmento de medaforca termina em 0x01 (MF2 §2), entao 0x01 e o fim do roteiro.
- Quem le: 0x03002B8C (cena) tem leituras em MainState0/1/2/3 (xref). 0x0300137C (GS) tem leitura em
  Dispatch_CallIndexedHandler 0x0800255a e Dispatch_ScrollTickThenHandler 0x08068de2. A leitura exata que dispara a saida
  da cena de acao NAO foi localizada.
- Pendente: confirmar no emulador (savestate de luta + medaforca) o que acontece apos 0x01. MN.md diz que cena 8 e batalha,
  o que conflita com "saida". Nao resolvido.
Confianca: escritas confirmadas; papel de fim = provavel; leitores = parcial.

## Q4: quem zera o medidor de Medaforca B+0xC1 (0x03000651 + bot*0x100): NAO ACHADO

Verificado nesta rodada:
- 0x49 (0x08034910) e inline (AS2): metade da armadura do bot ativo; nao escreve B+0xC1.
- 0x59 = `ActionScript_Op59_CallBA` -> `Fight_DispatchOnSubstateAA_BA`. BA_08 (0x0802A534, `AddGaugeToAllies`) soma em bot+0x10
  dos aliados via safe_add; nao escreve B+0xC1.
- BR_49 (0x08031918) e 0x080155A8/0x0803628C: ja sabido (MF2 §6); nao zeram B+0xC1.
- NAO lidos ainda: 0x51 (0x08034BBC -> `Fight_DispatchOnSubstateAA_AS`), BA_00..BA_07 e BA_SetMessage54 (0x0802A0E8..0x0802A694).
Proximo passo: decompile de BA_SetMessage54 (0x0802A694) e de `Fight_DispatchOnSubstateAA_AS`; procurar escrita no offset 0xC1
do bot. Antes, confirmar que o medidor de Medaforca e mesmo +0xC1 (MF2 diz; R1 diz que o medidor comum e +0xBB).

## Pendencias
1. Q4: BA_00..07, BA_SetMessage54 e AS (0x51) nao lidos.
2. Q3: leitor de GS 0x40 (saida da cena de acao) e teste no emulador.
3. Semantica de +0xA6 para as mensagens 5, 0x0C, 0x16, 0x17: nao decodificada.
4. Placeholders 0x08000841: nao sao handler; nao testados.
5. Ghidra: nenhuma renomeacao nova nesta rodada.
