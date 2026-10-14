# BI: pontas da abertura da Robattle (E/F/G/H em HA2.md)

Lido no Ghidra (decompile e xrefs) e no pool da ROM original. Sem emulador. Não editei mtbe.csv nem HA2.md.

Método: cada stub `Fight_DispatchOnSubstateAA_<Desp>` zera `+0xB5` a cada chamada e chama `table[+0xAA]`.
O laço que avança é o op de script: `ActionScript_Op09` (E) e `ActionScript_Op19` (F). Ambos chamam o stub e, se
`+0xB5 != 0`, chamam `Fight_AdvanceActionScriptCursor` (zera `+0xAA` e avança o cursor do script).

## (1) Inicializador do byte 0x03003478 (espera do E08, até 0x40)

- Inicializador: `Fight_DispatchOnSubstateAA_SetupBotAndSprites` (0x0801D0A8, confirmado). Grava 0x03003478 = **8**,
  0x03003479 = 0x40, 0x0300347A = 6, 0x0300347B = 0xF0 (constantes lidas da pool 0x0801D0E4..0x0801D0F4).
- Com 8 de início, o E08 leva cerca de 50 frames para chegar a 0x40 (HA2 dizia ~48 a partir de 0, valor não verificado).
- Quem chama esse setup: não identificado nesta rodada.
- Outros escritores do mesmo endereço (xref): 0x08021168 (S_08), 0x0801DCE6 (E07), 0x08022D7A (Y_08), 0x08023D74 (AD_08),
  0x08023184 (Z_07), 0x08024140 (AE_08), 0x0802253E (W_04), 0x0801C994 (TickCounterSfx). É um contador compartilhado.
- Confiança: confirmado (init 8 pela pool e decompile). Provável: o E08 compara 0x3478 com 0x40, como diz o HA2.

## (2) Consumidor do timer 0x030004C6 (= 0x03000440+0x86) gravado pelo H18 com 0x3C

- Consumidor: **H19 = `Fight_DispatchOnSubstateAA_H_19_WaitTimer60ClearSlot10` (0x08030624)**, antes
  `ActionFamily_HealChange_Handler`. Renomeado no Ghidra nesta rodada (decompile confirmado).
- Lógica: se `byte 0x030004C9 != 0` ou o flag de `0x08030654` tem bit 0 zerado, decrementa o halfword até 0. Ao zerar:
  `ClearObjectAttributeEntries(10,1)` e `+0xAA += 1` (-> H1A).
- Outros leitores com o mesmo padrão: `Fight_DispatchOnSubstateAA_Shared_WaitTimerOrSkip` (0x08030680) e
  `Fight_DispatchOnSubstateAA_AG_09_ScrollSlotWaitTimer` (0x080307xx). Não lidos: rotinas de sobreposição 0x08014E24,
  0x08014B72, 0x080135A0.
- Confiança: confirmado (H19). Provável: a base 0x03000440, pelo padrão dos outros leitores.

## (3) Ponteiro 0x08043E49 gravado em 0x030017F0 pelo G03 (0x0801E09C)

- G03 (etapa 1) grava 0x08043E49 em 0x030017F0 (escrita em 0x0801E1C8).
- Quem chama: `VBlankIrqHandler` (0x080007E0). O handler faz `CallViaR0(*0x030017F0)` a cada VBlank e depois repõe o
  slot com `DAT_08000818`. É um callback de um quadro, desarmado logo após a chamada.
- Outras escritas em 0x030017F0: `Fight_J02_Step4` (0x0801FD3C, grava 0x08044101), `Fight_SceneStep53_ExitBattleScene`
  (0x0803133C, grava nop), `Gfx_ResetBgLayersAndSong` e outros.
- Confiança: provável (o `*DAT_08000810` do VBlank é o 0x030017F0, pelo literal e pelo padrão de escrita). Chute: o
  conteúdo de 0x08043E48.

## (4) Quem zera +0xB5 (0x030018B5) depois dos holds F02, G06, H15, e o evento que libera

- Não há laço separado zerando +0xB5. Cada stub zera `+0xB5 = 0` ANTES de chamar o handler (decompile do stub em
  0x0801C506). F02 (0x0801DED4), G06 (0x0801E2B8) e H15 (0x0801EAC4) escrevem `+0xB5 = 1`.
- Quem lê e avança: `ActionScript_Op09` (E, 0x08033AB0) e `ActionScript_Op19` (F, decompile). Com `+0xB5 != 0`, chamam
  `Fight_AdvanceActionScriptCursor`. O cursor avança no mesmo quadro e `+0xAA` volta a 0.
- Correção ao HA2: F02, G06 e H15 não seguram o script. Marcam o fim do passo. Nenhum evento externo (toque, animação)
  os libera aqui.
- Confiança: confirmado para E e F (decompile). Provável para G e H (padrão; op não decompilado).

## (5) Sequência E00..H19 (passo -> o que aparece na tela)

Visual inferido dos handlers (HA2/HA1). "gen" = handler genérico (0x080305E1 ou 0x08030624, não documentado aqui).

| Passo | Handler | O que aparece na tela |
|---|---|---|
| E00 | 0x0801D5A8 FindAllyFollowupActionSource | nada (busca fonte aliada; ver HA1) |
| E01 | 0x0801D6A4 | carga de gráfico da fase (sem visual novo) |
| E02 | 0x0801D738 | sprite do bot da vez montado |
| E03 | 0x0801D878 | entrada animada do bot (60 frames) |
| E04 | 0x0801D95C | bot anda até o alvo |
| E05 | 0x0801D9DC | bot desliza no eixo Y até a linha 0x78 |
| E06 | 0x0801DAEC | sobe o slot; aparecem ícones/partes do bot |
| E07 | 0x0801DC74 | espera 30 frames; surge o efeito (slot 0x21) |
| E08 | 0x0801DD30 | espera 0x3478 chegar a 0x40 (~50 frames); som 0x30 |
| E09 | 0x0801DD74 | roda a animação do efeito; copia dados do bot |
| E0A | gen | (não documentado) |
| E0B | 0x0801DDDC | marca alvo selecionado (sem visual); +B5=1 |
| F00 | 0x0801DE44 | copia dados da parte; msg 0x17 |
| F01 | gen | (não documentado) |
| F02 | 0x0801DED4 | sem visual; +B5=1 (fim do passo) |
| G00 | 0x0801DF0C | limpa paleta e tiles de fundo |
| G01 | 0x0801DF60 | sprites laterais de fundo deslizam (10 frames) |
| G02 | 0x0801DFE0 | carga do fundo em 3 etapas |
| G03 | 0x0801E09C | monta cenário (BG e OBJ/blend) em 2 etapas; grava callback 0x08043E49 |
| G04 | 0x0801E200 | zera e reinicia sprites laterais |
| G05 | 0x0801E248 | sprites laterais entram (10 frames) |
| G06 | 0x0801E2B8 | passo das sequências laterais; +B5=1 |
| H00 | 0x0801E300 | carga de sprites de luta e ícones de parte |
| H01 | 0x0801E3C0 | escolhe caminho da ação (bits de +0xB7/+0xB8) |
| H02 | 0x0801E43C | ramo pelo bit 0x40 do bot (0x06 ou H03) |
| H03 | 0x0801E474 | carga de gráfico e objeto de luta; som 0x72 |
| H04 | 0x0801E54C | espera slot chegar a 10; timer 0x3C |
| H05 | gen | (não documentado) |
| H06 | 0x0801E578 | ramo H07 ou 0x0A (bit 0x20) |
| H07 | 0x0801E5DC | carga de gráfico por etapa |
| H08 | 0x0801E830 | avança offset do slot; timer 0x3C |
| H09 | gen | (não documentado) |
| H0A | 0x0801E864 | monta 4 sprites do bot |
| H0B | 0x0801E8DC | espera timer dos sprites (10) |
| H0C | gen | (não documentado) |
| H0D | 0x0801E918 | dígitos do bot e linha de sprites; som 0x5B |
| H0E | 0x0801E9B0 | espera passo de sprite; timer 0x3C |
| H0F | gen | (não documentado) |
| H10 | 0x0801E9E0 | som 0x2F |
| H11 | 0x0801E9FC | linhas de ícones de parte; timer 0x3C |
| H12 | gen | (não documentado) |
| H13 | 0x0801EA5C | se +0xD1==0 avança; senão pula para H16 |
| H14 | 0x0801EA94 | passo por orientação |
| H15 | 0x0801EAC4 | sem visual; +B5=1 (fim do passo) |
| H16 | 0x0801EAD4 | sprites de parte na posição do bot |
| H17 | 0x0801EB5C | anima slot 10 até acabar; sprites de duas camadas |
| H18 | 0x0801EBB8 | arma timer 0x3C em 0x030004C6 |
| H19 | 0x08030624 | espera o timer; limpa slot 10; avança para H1A |

## Em aberto

- Quem chama o SetupBotAndSprites (0x0801D0A8) e em que fase.
- Conteúdo do callback 0x08043E48 (G03).
- Ops de script de G e H não decompilados (confirmar o padrão de +B5).
- Handlers genéricos 0x080305E1 e 0x08000841 não lidos.
- Nomes de simbolos novos ficam em BI_simbolos.csv (não fundidos no mtbe.csv).
