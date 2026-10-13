# HA2: handlers dos despachantes Fight_DispatchOnSubstateAA_* (E, F, G, H, AO)

Escopo: os 42 endereços de `HA2_lista.txt`. Cada despachante é um stub de 0x20 bytes que zera `+0xB5` do bloco
`0x03001800` (fight_information) e chama `table[+0xAA]` pelo trampolim `0x0807CCF0`. O byte `+0xAA` é o subestado.
Tabela de cada stub (literal no stub): E `0x083AD6F0`, F `0x083AD730`, G `0x083AD750`, H `0x083AD790`,
I `0x083AD8D0` (fim da H = 0x50 entradas, 0x00..0x4F). Entradas conferidas na ROM: as 41 da lista batem com o
índice (0 divergências). AO (0x0801EBD4, índice 0x0E) não tem stub identificado nesta rodada.

Convenções de leitura: "+1" = handler faz `+0xAA += 1` (ldrb/adds #1/strb sobre `0x03001800+0xAA`; confirmado no
código lido). "hold" = handler não escreve +0xAA e só põe `+0xB5 = 1` ou chama rotina e volta (o subestado fica).
Alvos genéricos da tabela (fora desta lista): `0x080305E1` (E0A, F01, H05, H09...), `0x08000841` (E0C..0F, F03..07,
G07, H25..27...), `0x080306B9`, `0x08030625`. Não documentados aqui.

Nomes (redo desta rodada): os handlers agora são funções no Ghidra e foram renomeados no projeto com
`Fight_DispatchOnSubstateAA_<Desp>_<idx>_<Passo>`. Os nomes do CSV seguem o mesmo padrão. Na rodada anterior
ainda não havia função: os textos antigos de "chute" foram reescritos após a leitura do decompile.

## 0x0801D6A4 Fight_DispatchOnSubstateAA_E (idx 0x01)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_E_01_LoadPhaseGfx(void)`
- Passo: carga de gráfico da fase (descompacta ROM para VRAM) antes da animação de entrada. A fase vem do byte
  `bloco+0xDF` (bloco de luta, literal no pool).
- O que faz: fase 0x1B, 0x1C e 0x1D cada uma chama `decompress_and_store(ROM, dest)` (`0x080009BC`) e depois
  `bios_cpufastset` com o destino da fase (0x1C: ROM `0x084CAE20` -> VRAM `0x06014800`). Fase 0x1D também usa
  `decompress_and_store` + cpufastset. Outras fases: não carrega nada. Ao final, SEMPRE `+0xAA += 1`.
- Subestado: +1 (-> E02). A nota antiga ("não escreve +0xAA", "cauda não decodificada") estava errada: o +1 é
  incondicional no fim da função.
- Confiança: confirmado (código lido: +1 e fases), provável (endereços exatos das cargas por literal).

## 0x0801D738 Fight_DispatchOnSubstateAA_E (idx 0x02)
- Passo: copia dados de 4 blocos do bot ativo para buffers de trabalho.
- O que faz: `+0xDE-1` escolhe o bot; `0x0803F63C` (cópia) lê `0x030042B0` + `0x40/0x80/0xC0` + offsets fixos `0xFFFFF1C0..0xFFFFF280` e grava `0x03000440`=0 (r1); chama `0x08040D0C`, `0x08015838`, `0x0804305C`, `0x08043240`.
- Subestado: +1 (-> E03).
- Confiança: provável (cópia), confirmado (+1).

## 0x0801D878 Fight_DispatchOnSubstateAA_E (idx 0x03)
- Passo: preparação de objetos de luta (OAM/posições).
- O que faz: chama `0x080432A0`, `0x08040D0C`, `0x08040D44`, `0x08074DE0`, `0x08035694`; escreve `0x03002C30` (+0x30 halfword, 0x1664/0x1665 nos bytes de controle), `0x03000440`.
- Subestado: +1 (-> E04).
- Confiança: provável (objetos), confirmado (+1).

## 0x0801D95C Fight_DispatchOnSubstateAA_E (idx 0x04)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_E_04_StepBotSlot(void)` (Ghidra: FUN_0801d95c)
- Passo: anda com o slot de objeto do bot ativo (bot = `+0xDE`) e espera chegar.
- O que faz: chama `Fight_StepSlotCoordDownInRange(bot*4+0x59, -0x20, -0x10)`. Se retorna 0 (ainda andando):
  `Sprite_UpdateComposite4Motion((bot-1)*4+0x5a, bot-1, flag)` + `Fight_UpdateFollowerObjectSlot(bot*4+0x59, bot+0x71)`.
  Se retorna não-zero (chegou): faz as mesmas duas chamadas e depois `+0xAA += 1`. `flag` = bit0 de `+0xF8` invertido.
- Subestado: hold enquanto anda; +1 (-> E05) só quando chega. (Nota antiga "+1 incondicional" estava errada.)
- Confiança: confirmado (código lido, condição e +1), provável (semântica de "chegou").

## 0x0801D9DC Fight_DispatchOnSubstateAA_E (idx 0x05)
- Assinatura: `void Fight_DispatchOnSubstateAA_E_05_SlideBotIntoPlace(void)` (Ghidra: FUN_0801d9dc)
- Passo: desliza o objeto do bot no eixo Y até a linha 0x78 e, ao chegar, prepara os atributos do sprite.
- O que faz: escolhe o sentido pelo bit0 de `+0xF8` do bot. Sentido 0: Y -= 2 por frame, para quando o campo
  (halfword em `bot*0x100 + base + 0x1642`) passa de 0x78. Sentido 1: Y += 2, para quando fica < 0x78. Chama
  `Sprite_UpdateComposite4Motion`. Se chegou: escreve bytes/halfwords no bloco de objetos (`bot*0x100+base+0x30/+2`,
  `(bot+0x71)*0x40+base+2/+4` copiando o do bot `+0x87`), `+0xAA += 1`. Depois faz tail-call por ponteiro
  (jump table não recuperada pelo Ghidra; é o alvo do próximo passo, não decodificado).
- Subestado: hold enquanto desliza; +1 (-> E06) ao chegar.
- Confiança: confirmado (condição de chegada e +1), provável (sentido do movimento e base `0x03002C30`, do
  nome do literal). Tail-call não decodificado.

## 0x0801DAEC Fight_DispatchOnSubstateAA_E (idx 0x06)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_E_06_ShowPartsHud(void)` (Ghidra: FUN_0801daec)
- Passo: sobe o slot do bot até a posição-alvo e, ao chegar, monta os ícones/partes do bot na tela.
- O que faz: `Fight_StepSlotCoordUpInRange(bot*4+0x59, ...)` (alvo lido do bloco do bot `+0x87`, halfword `+0x1740`).
  Enquanto não chega: `Sprite_UpdateComposite4Motion` + `Fight_UpdateFollowerObjectSlot`. Ao chegar: mesmas duas,
  depois `snd_sfx_request(0x41)`, `Fight_GetDisplayedPartIdsOrBrokenGlyphs(bot-1, ...)`,
  `Sprite_WriteComposite4Objects(...)` (4 objetos de parte, posição do bot), zera 2 campos do bot (`+0x15D8`,
  `+0x1618`), `Fight_HideObjectSlot(bot+0x71)`, zera `DAT_0801dc34+0x84`, `+0xAA += 1`.
- Subestado: hold enquanto sobe; +1 (-> E07) ao chegar.
- Confiança: confirmado (condição, +1, chamadas), provável (significado das partes/ícones pelos nomes das rotinas).

## 0x0801DC74 Fight_DispatchOnSubstateAA_E (idx 0x07)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_E_07_SpawnSlot21(void)` (Ghidra: FUN_0801dc74)
- Passo: espera ~30 frames e então cria o slot de objeto 0x21 a partir da lista de registros do bot.
- O que faz: contador em `bloco+0x84` (literal). Enquanto != 0x1E: incrementa o contador (hold). Quando == 0x1E:
  `Tile_ClearSlots(0x21, 1)`, grava X/Y do bot (`+0x1642`, `+0x1644 - 4`) em um objeto (`+0x840`, `+0x8`, `+0x5`,
  `+0x9` com valores 6, 0x80, 0x80, 0xF0), `Slot_SetupFromRecordList(ptr+0x70, 0x21)`, `+0xAA += 1`.
- Subestado: hold ~30 frames; +1 (-> E08) quando o contador chega a 0x1E.
- Confiança: confirmado (código lido: contador 0x1E, slot 0x21, +1), provável (espera de 30 = aparição do efeito).

## 0x0801DD30 Fight_DispatchOnSubstateAA_E (idx 0x08)
- Passo: disparo de efeito (id 0x30) no meio da luta.
- O que faz: `0x08074DE0(0x30)`; toca `0x03002C30`.
- Subestado: +1 (-> E09).
- Confiança: provável (id), confirmado (+1).

## 0x0801DD74 Fight_DispatchOnSubstateAA_E (idx 0x09)
- Assinatura: `void Fight_DispatchOnSubstateAA_E_09_RunSlot21Anim(void)` (Ghidra: FUN_0801dd74)
- Passo: roda o script de animação do slot 0x21 a cada frame; quando termina, copia dados do bot e avança.
- O que faz: `Anim_RunSlotScript(ptr+0x70, 0x21)` todo frame. Se o byte de estado do slot == -1 (fim): `FUN_0803F63C(
  bot*0x100 + base, literal, 0xE)` (cópia de 14 unidades), `+0xA6 = 0x16` (código de mensagem), `Tile_ClearSlots(0x21, 1)`,
  `+0xAA += 1`.
- Subestado: hold enquanto a animação roda; +1 (-> E0A, genérico 0x080305E1) ao fim.
- Confiança: confirmado (código lido), provável (o significado de 0x16 em +0xA6).

## 0x0801DDDC Fight_DispatchOnSubstateAA_E (idx 0x0B)
- Assinatura: `void Fight_DispatchOnSubstateAA_E_0B_SetSelectedTarget(void)` (Ghidra: FUN_0801dddc)
- Passo: marca o alvo escolhido (bot e parte) e termina o handler sem avançar.
- O que faz: `Fight_SetSelectedTargetBot(bloco+0x87, bot)` e `Fight_SetSelectedTargetPart(bloco+0x87, parte do bot
  anterior, bot-1 * 0x100 + base + 0xDE)`; põe `+0xB5 = 1`.
- Subestado: hold (não escreve +0xAA). `+0xB5 = 1` só encerra este handler; quem avança é o despachante seguinte.
- Confiança: confirmado (código lido, chamadas e +0xB5). O nome da rotina de alvo, pelo Ghidra.

## 0x0801DE44 Fight_DispatchOnSubstateAA_F (idx 0x00)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_F_00_CopyPartDataPair(void)` (Ghidra: FUN_0801de44)
- Passo: copia dois blocos de dados de parte do bot (cada um escolhido por tabela de ponteiros).
- O que faz: o índice vem de `+0xA7` (*0x18) e `+0x87` (*0x100) do bloco, mais offsets `+0x4A` e `+0x49`, usados
  para ler um ponteiro de tabela (base `DAT_0801dec0` e `DAT_0801ded0`). Chama `FUN_0803F63C` (cópia, 14 unidades)
  duas vezes: o 1º para o destino literal, o 2º para `destino + 0x14`. Depois `+0xA6 = 0x17`, `+0xAA += 1`.
- Subestado: +1 (-> F01). A nota antiga ("sem +0xAA") estava errada.
- Confiança: confirmado (código lido: cópias, +0xA6 = 0x17, +1). A função da tabela de ponteiros não foi decodificada.

## 0x0801DED4 Fight_DispatchOnSubstateAA_F (idx 0x02)
- Passo: flag de espera.
- O que faz: só `+0xB5 = 1` e retorno (5 instruções).
- Subestado: hold.
- Confiança: confirmado (código lido).

## 0x0801DF0C Fight_DispatchOnSubstateAA_G (idx 0x00)
- Passo: carga de paleta e tiles de cena (CpuFastSet via `0x0807CCC0`).
- O que faz: copia para paleta `0x05000120` e VRAM `0x06004120`; usa `0x01000008` / `0x11111111`.
- Subestado: +1 (-> G01).
- Confiança: provável (CpuFastSet pelo README), confirmado (+1).

## 0x0801DF60 Fight_DispatchOnSubstateAA_G (idx 0x01)
- Passo: configuração de registrador de fundo (BG).
- O que faz: chama `0x0803F15C`; escreve `0x04000008` (BGxCNT) e `0x03002C30`; flag `[r2]=1`.
- Subestado: +1 (-> G02).
- Confiança: provável, confirmado (+1).

## 0x0801DFE0 Fight_DispatchOnSubstateAA_G (idx 0x02)
- Assinatura: `void Fight_DispatchOnSubstateAA_G_02_LoadBgTilesStaged(void)` (Ghidra: FUN_0801dfe0)
- Passo: carga de fundo em 3 etapas (tiles/mapa por CpuFastSet), uma por frame, com contador próprio.
- O que faz: contador de etapa em `DAT_0801dff8+DAT_0801dffc` (byte). Etapa 0: três `bios_cpufastset` de 0x200
  unidades (paleta/tiles de fundo). Etapa 1: uma cópia de 0x800 unidades. Etapa 2: escreve um ponteiro de
  `DAT_0801e090` e `Tile_ClearSlots(0, 0x80)`, zera o contador e faz `+0xAA += 1` no bloco. Toda etapa termina com
  tail-call por ponteiro (jump table não recuperada).
- Subestado: hold nas etapas 0 e 1; +1 (-> G03) só na etapa 2.
- Confiança: confirmado (código lido: etapas, +1 na etapa 2), provável (conteúdo VRAM pelos literais). Tail-call não decodificado.

## 0x0801E09C Fight_DispatchOnSubstateAA_G (idx 0x03)
- Passo: montagem do cenário de batalha (fundo/efeito).
- O que faz: maior handler do grupo (157 instruções); chama `0x08043CA8`, `0x080009BC`, `0x0803EF8C`, `0x08043B7C`, `0x0807CCC0`, `0x08043DF8`, `0x0801BC5C`, `0x0801BCA0`, `0x08035C3C`; VRAM `0x06017400`; lê `0x03000590`, `0x030017F0`.
- Subestado: +1 (-> G04).
- Confiança: provável (cenário), confirmado (+1).

## 0x0801E200 Fight_DispatchOnSubstateAA_G (idx 0x04)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_G_04_ResetSideSprites(void)` (Ghidra: FUN_0801e200)
- Passo: zera e reinicia os sprites laterais da cena de luta.
- O que faz: `bios_cpufastset(&zero, ...)` (limpa uma palavra de 0 para o bloco de destino), depois
  `FightPreview_InitializeSideSprites()`, `FightPreview_UpdateSingleLayerSideSprites()`,
  `FightPreview_ResetSequenceStepFlags()`, zera o contador `+0x84` do bloco e faz `+0xAA += 1`.
- Subestado: +1 (-> G05), sem espera.
- Confiança: confirmado (código lido). Os nomes `FightPreview_*` vêm do Ghidra/mtbe, não confirmados pelo uso.

## 0x0801E248 Fight_DispatchOnSubstateAA_G (idx 0x05)
- Assinatura: `void Fight_DispatchOnSubstateAA_G_05_SlideSideSpritesIn(void)` (Ghidra: FUN_0801e248)
- Passo: animação de entrada dos sprites laterais em 10 frames.
- O que faz: a cada frame: `FightPreview_UpdateSingleLayerSideSprites`, `FightPreview_AdvanceLeftSideSequenceStep`,
  `FightPreview_AdvanceRightSideSequenceStep`, e duas chamadas `FUN_0803EF8C(0x1E, 1, 0, X, Y, 0)` com X = `9 - c`
  (lado esquerdo) e X = `c + 10` (lado direito), onde `c` = contador `+0x84` do bloco. Se `c == 9`: `+0xAA += 1`;
  senão `c += 1`.
- Subestado: hold por 10 frames; +1 (-> G06) quando `c` chega a 9.
- Confiança: confirmado (código lido: contador 0..9, +1), provável (sentido "entrada" pelo deslocamento).

## 0x0801E2B8 Fight_DispatchOnSubstateAA_G (idx 0x06)
- Passo: espera de animação.
- O que faz: chama `0x08035DE8`, `0x080361C4`, `0x08036218`; `+0xB5 = 1`.
- Subestado: hold.
- Confiança: provável (hold, código lido).

## 0x0801E300 Fight_DispatchOnSubstateAA_H (idx 0x00)
- Passo: carga de sprites e tiles de luta (OBJ).
- O que faz: `0x080009BC`, `0x0807CCC0`, `0x080362C4`, `0x08036340`, `0x08036370`, `0x080364A8`; VRAM `0x06006400`, `0x06006520`, `0x06006600`, `0x06006800`; paleta OBJ `0x05000140`, `0x05000100`, `0x05000160`.
- Subestado: +1 (-> H01).
- Confiança: provável, confirmado (+1).

## 0x0801E3C0 Fight_DispatchOnSubstateAA_H (idx 0x01)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_01_ChooseActionPath(void)` (Ghidra: FUN_0801e3c0)
- Passo: escolhe o caminho da ação pelos bits de flags do bloco (`+0xB7`, `+0xB8`); zera `+0xB9`.
- O que faz, por ordem de teste:
  1. `+0xB7 & 1` != 0 -> `+0xAA = 0x28`.
  2. `+0xB7 & 3` != 0 -> `Fight_AddMedaforceChargeFromAction()`, `+0xAA = 0x0A`.
  3. `+0xB8 & 2` != 0 -> `+0xAA = 0x30`.
  4. `+0xB8 & 4` != 0 -> `+0xAA = 0x38`.
  5. senão -> `Fight_IncrementActionUsageCounter_08036270()`, `Fight_AddMedaforceChargeFromAction()`, `+0xAA += 1` (-> H02).
- Subestado: ramificação para 0x28 / 0x0A / 0x30 / 0x38 / +1.
- Confiança: confirmado (código lido: testes de bit, valores e chamadas). Significado dos bits: chute.

## 0x0801E43C Fight_DispatchOnSubstateAA_H (idx 0x02)
- Assinatura: `void Fight_DispatchOnSubstateAA_H_02_CheckBotFlag40(void)` (Ghidra: FUN_0801e43c)
- Passo: ramo curto pelo bit 0x40 do byte `+0xBC` do bot selecionado por `bloco+0x88`.
- O que faz: bit 0x40 == 0 -> `+0xAA = 0x06`; bit 0x40 != 0 -> `+0xAA += 1` (-> H03).
- Subestado: ramificação 0x06 ou +1.
- Confiança: confirmado (código lido, condição e valores). Significado do bit 0x40: chute.

## 0x0801E474 Fight_DispatchOnSubstateAA_H (idx 0x03)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_03_LoadGfxAndInitObj(void)` (Ghidra: FUN_0801e474)
- Passo: carga de gráfico (descompacta da ROM) e inicialização de um objeto de luta.
- O que faz: `decompress_and_store(literal ROM, literal dest)` (`0x080009BC`) + `bios_cpufastset(..., 8)`. Depois
  grava campos do objeto (base literal): halfword `+0x280`, bytes `+0x29C = 0`, `+0x?? = 0x0E`, `+0x?? = 0x80`,
  halfword `= 0x1F00`, byte `= 0`. Se o bot (literal) tem `+0xF8 & 2 == 0` copia halfword de `+0x158` para
  destino e usa `+0x15A`; senão usa `+0x154`/`+0x156`. Grava `+0x284`, `snd_sfx_request(0x72)`, `+0xAA += 1`.
- Subestado: +1 (-> H04). A nota antiga ("não escreve +0xAA") estava errada: o +1 é incondicional no fim.
- Confiança: confirmado (código lido: +1, som 0x72, cargas), chute (offsets exatos dos campos do objeto, pois os literais não foram resolvidos).

## 0x0801E54C Fight_DispatchOnSubstateAA_H (idx 0x04)
- Assinatura: `void Fight_DispatchOnSubstateAA_H_04_WaitSlotOffset10(void)` (Ghidra: FUN_0801e54c)
- Passo: espera o deslocamento de slot chegar a 10 e então pausa 60 frames.
- O que faz: `Fight_StepSlotAnimOffsetTable(10)`. Se retorna não-zero: `+0x86 = 0x3C` (timer de espera de 60
  frames, o mesmo `+0x86` que `0x08030674` consome) e `+0xAA += 1`. Se zero: hold.
- Subestado: hold enquanto anda; +1 (-> H05, genérico) ao chegar, com 60 frames de espera.
- Confiança: confirmado (código lido: timer 0x3C, +1 condicional). A nota antiga (grava 0x03000440) estava errada.

## 0x0801E578 Fight_DispatchOnSubstateAA_H (idx 0x06)
- Assinatura: `void Fight_DispatchOnSubstateAA_H_06_CheckHitOrSkip(void)` (Ghidra: FUN_0801e578)
- Passo: ramo sem chamadas, pelo registro de parte do bot e pelo bit 0x20 de `+0xBC`.
- O que faz: se o byte `+0x49` do registro do bot (`bot*0x18 + bot*0x100 + base`, bot de `+0xA7`/`+0x87`) == '0' ->
  `+1`. Senão, se `+0xBC` do bot (bot de `+0x88`) & 0x20 == 0 -> `+0xAA = 0x0A`. Senão -> `+1`.
- Subestado: +1 (-> H07) ou 0x0A (-> H0A, ramo que pula H07..H09).
- Confiança: confirmado (código lido, condições e valores). Significado do bit 0x20 e do byte `+0x49`: chute.

## 0x0801E5DC Fight_DispatchOnSubstateAA_H (idx 0x07)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_07_LoadStageGfx(void)` (Ghidra: FUN_0801e5dc)
- Passo: carga de gráfico por etapa do objeto (etapas no byte `bloco+0x8B`) e configuração do objeto.
- O que faz: se o byte `+0x49` (índice do bot) for '0': caso especial, `FUN_08036B84`, descompacta gráfico,
  cpufastset, escreve 0x2100 e toca som 0x54. Senão, pela etapa (`+0x8B` = 0..3): descompacta um gráfico próprio e
  escreve um halfword de controle (0x1B00, 0x1C00, 0x1D00, 0x1E00); etapa fora de 0..3: som 6. Depois grava campos
  do objeto (`+0x280`, `+0x29C = 0`, `+0x28A = 0x0E`, etc.), posição X conforme lado e etapa, `+0x284`, e
  `+0xAA += 1`.
- Subestado: +1 (-> H08) em todos os caminhos (a nota antiga "não escreve +0xAA" estava errada).
- Confiança: confirmado (código lido: +1, etapas, sons 0x54 e 6), chute (significado das etapas).

## 0x0801E830 Fight_DispatchOnSubstateAA_H (idx 0x08)
- Assinatura: `void Fight_DispatchOnSubstateAA_H_08_AdvanceSlotOffsetByStage(void)` (Ghidra: FUN_0801e830)
- Passo: avança o deslocamento do slot pela etapa do objeto e pausa 60 frames ao terminar.
- O que faz: `FightPreview_AdvanceSlotOffset(10, bloco+0x8B)`. Se não-zero: `+0x86 = 0x3C` e `+0xAA += 1`. Senão hold.
- Subestado: hold enquanto anda; +1 (-> H09, genérico 0x080305E1) ao terminar.
- Confiança: confirmado (código lido). A chamada `0x080366DC` da nota antiga não aparece neste handler.

## 0x0801E864 Fight_DispatchOnSubstateAA_H (idx 0x0A)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_0A_SetupFourSprites(void)` (Ghidra: FUN_0801e864)
- Passo: carrega gráfico e monta 4 sprites do bot na etapa atual.
- O que faz: `decompress_and_store(literal ROM, literal dest)` + `bios_cpufastset(..., 8)`; escolhe a posição do bot
  (`+0x158` se `+0xF8 & 2 == 0`, senão `+0x154`), chama `FightPreview_SetupFourSprites(10, etapa bloco+0x8B, X, Y)`
  e `+0xAA += 1`.
- Subestado: +1 (-> H0B).
- Confiança: confirmado (código lido: +1, chamada com 10 e etapa). A chamada `0x08036784(0x0A,0xAA)` da nota antiga
  era um erro de leitura: a rotina real é `FightPreview_SetupFourSprites`.

## 0x0801E8DC Fight_DispatchOnSubstateAA_H (idx 0x0B)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_0B_TickSpriteTimers(void)` (Ghidra: FUN_0801e8dc)
- Passo: copia gráfico (CpuFastSet) e espera o timer dos sprites chegar a zero.
- O que faz: `bios_cpufastset(literal, literal, 8)`; `FightPreview_TickSpriteTimers(10)`. Se não-zero:
  `+0x86 = 10` (espera curta de 10 frames) e `+0xAA += 1`. Senão hold.
- Subestado: hold enquanto o timer roda; +1 (-> H0C, genérico) ao terminar.
- Confiança: confirmado (código lido). A chamada `0x0803684C` da nota antiga não aparece neste handler.

## 0x0801E918 Fight_DispatchOnSubstateAA_H (idx 0x0D)
- Passo: fase de animação com som/efeito (id 0x5B).
- O que faz: `0x080009BC`, `0x0807CCC0`, `0x0801A28C`, `0x080368C4`, `0x08074DE0(0x5B)`; VRAM `0x06010000`.
- Subestado: +1 (-> H0E).
- Confiança: provável (id 0x5B), confirmado (+1).

## 0x0801E9B0 Fight_DispatchOnSubstateAA_H (idx 0x0E)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_0E_WaitSpriteStep10(void)` (Ghidra: FUN_0801e9b0)
- Passo: avança um passo de sprite (10) e pausa 60 frames ao terminar.
- O que faz: `FUN_08036B1C(bloco, 10)`. Se não-zero: halfword `+0x86 = 0x3C` (60 frames, nota: o endereço é
  base-0x68, ou seja `+0x86` do bloco de objeto) e `+0xAA += 1`. Senão hold.
- Subestado: hold; +1 (-> H0F, genérico) ao terminar.
- Confiança: confirmado (código lido: 0x3C e +1). Nota: não há "efeito 0x3C" nem `0x0300052E` neste handler; o 0x3C é o timer.
  Também é handler de AP/AQ/P/R pela coluna extra da lista (não reconferido).

## 0x0801E9E0 Fight_DispatchOnSubstateAA_H (idx 0x10)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_10_SfxStep2F(void)` (Ghidra: FUN_0801e9e0)
- Passo: toca o som 0x2F e avança.
- O que faz: `snd_sfx_request(0x2F)`; `+0xAA += 1`.
- Subestado: +1 (-> H11), sem espera.
- Confiança: confirmado (código lido). O id 0x2F é de som (não de efeito visual).

## 0x0801E9FC Fight_DispatchOnSubstateAA_H (idx 0x11)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_11_WaitPartIconRows(void)` (Ghidra: FUN_0801e9fc)
- Passo: desenha as linhas de ícones de parte e espera a rotina de espera terminar.
- O que faz: `FUN_08036BB8(bloco, byte -0x40, byte -0x3D)` (retorna flag de fim). Chama
  `FightPreview_LoadPartIconRows` e `FightPreview_DrawPartIconPairs` a cada frame (lado = bit1 de `+0x30`). Se a
  flag de fim é não-zero: `+0x86 = 0x3C` (60 frames) e `+1` no campo de estado do bloco (base-0x1E).
- Subestado: hold enquanto a flag está zero; +1 (-> H12) ao terminar, com 60 frames de espera.
- Confiança: confirmado (código lido: flag, timer 0x3C, +1), provável (sentido de lado/ícones).

## 0x0801EA5C Fight_DispatchOnSubstateAA_H (idx 0x13)
- Assinatura: `void Fight_DispatchOnSubstateAA_H_13_BackupStatsOrSkip(void)` (Ghidra: FUN_0801ea5c)
- Passo: decide entre avançar um passo ou pular dois passos, conforme o byte `+0xD1` do bloco.
- O que faz: se `+0xD1 == 0`: `+0xAA += 1`. Senão: `Fight_BackupHalvedStatsIfFlagClear(bloco+0x88)` e `+0xAA += 3`.
- Subestado: +1 (-> H14) se `+0xD1 == 0`; +3 (-> H16, pula H14 e H15) caso contrário.
- Confiança: confirmado (código lido). A chamada `0x08035194` da nota antiga é `Fight_BackupHalvedStatsIfFlagClear`.

## 0x0801EA94 Fight_DispatchOnSubstateAA_H (idx 0x14)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_14_StepByOrientation(void)` (Ghidra: FUN_0801ea94)
- Passo: chama uma rotina de parte com a orientação do bot e avança.
- O que faz: `FUN_08036BF4(bit2-clear de +0xF8 do bot)`; `+0xAA += 1`.
- Subestado: +1 (-> H15), sem espera.
- Confiança: confirmado (código lido). A chamada `0x08036BF4` estava certa; o nome é provisório.

## 0x0801EAC4 Fight_DispatchOnSubstateAA_H (idx 0x15)
- Passo: espera (flag).
- O que faz: só `+0xB5 = 1` (5 instruções).
- Subestado: hold.
- Confiança: confirmado (código lido).

## 0x0801EAD4 Fight_DispatchOnSubstateAA_H (idx 0x16)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_16_SetupPartSpritesAt(void)` (Ghidra: FUN_0801ead4)
- Passo: carrega gráfico de partes e monta os sprites de parte na posição do bot.
- O que faz: `decompress_and_store(literal ROM, literal dest)` + `bios_cpufastset(..., 8)`; lê a posição do bot
  (`+0x158` ou `+0x154`, conforme `+0xF8 & 2`); zera o timer `+0x86`; `FUN_08036C20(10, etapa bloco+0x8B, X, Y, 0x1E)`;
  `+0xAA += 1`.
- Subestado: +1 (-> H17), sem espera (o timer de 0x1E fica para a rotina chamada).
- Confiança: confirmado (código lido: +1, chamadas, timer 0x1E), provável (sentido do timer 0x1E).

## 0x0801EB5C Fight_DispatchOnSubstateAA_H (idx 0x17)
- Assinatura: `void Fight_DispatchOnSubstateAA_H_17_RunSlot10Anim(void)` (Ghidra: FUN_0801eb5c)
- Passo: roda a animação do slot 0x0A até acabar; ao acabar, limpa o slot e avança.
- O que faz: `Anim_RunSlotScript(ptr+0x78, 10)` todo frame. Se o byte de estado do slot == -1: `Tile_ClearSlots(10, 1)`
  e `+0xAA += 1`. Independente disso, o contador de frames `+0x86` (halfword) sobe a cada frame; quando chega a 0x12,
  chama `FightPreview_UpdateDualLayerSideSprites()`.
- Subestado: hold enquanto a animação roda; +1 (-> H18) quando termina.
- Confiança: confirmado (código lido: fim de animação, +1, 0x12). Ver nota: o `+0x86` aqui é contador de frames, não timer.

## 0x0801EBB8 Fight_DispatchOnSubstateAA_H (idx 0x18)
- Passo: avanço simples.
- O que faz: sem chamadas; grava `0x03000440`.
- Subestado: +1 (-> H19, genérico).
- Confiança: confirmado (+1).

## 0x0801EBD4 Fight_DispatchOnSubstateAA_AO (idx 0x0E)
- Assinatura: `void Fight_DispatchOnSubstateAA_AO_0E_UpdateSpritesWait60(void)` (Ghidra: FUN_0801ebd4)
- Passo: atualiza sprites de duas camadas e pausa 60 frames. Dispatcher AO sem stub identificado.
- O que faz: `FightPreview_UpdateDualLayerSideSprites()`; `+0x86 = 0x3C`; `+0xAA += 1`.
- Subestado: +1 (-> AO 0x0F) após 60 frames de espera (timer 0x3C consumido pelo despachante).
- Confiança: confirmado (código lido). A chamada `0x08035F20` da nota antiga é `FightPreview_UpdateDualLayerSideSprites`, provável.
  Também handler de AP (coluna extra): não reconferido.

## 0x0801EBF8 Fight_DispatchOnSubstateAA_H (idx 0x1A)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_1A_QueueBrokenBotOrSkip(void)` (Ghidra: FUN_0801ebf8)
- Passo: se o bot da vez (`bloco+0x88`) não está "quebrado", enfileira-o para resolução; senão pula passos.
- O que faz: se o byte do bot em `bot*0x100 + base + 0x59` == 0: `Fight_QueueHeadBrokenBotForResolution(bot)` e
  `+0xAA += 1`. Senão: `+0xAA += 3`.
- Subestado: +1 (-> H1B) ou +3 (-> H1D) conforme a condição. A nota antiga ("dois pontos de +1") estava errada.
- Confiança: confirmado (código lido: condição, +1 e +3). Semântica "quebrado" provável pelo nome da rotina.

## 0x0801EC3C Fight_DispatchOnSubstateAA_H (idx 0x1D)
- Assinatura: `undefined4 Fight_DispatchOnSubstateAA_H_1D_WaitOrAdvance(void)` (Ghidra: FUN_0801ec3c)
- Passo: espera enquanto a condição do bot não fecha; depois avança ou vai para o estado 0x20.
- O que faz: bot = `bloco+0x87`; registro do bot em `bot*0x100 + base + (bot2*0x18)`. Se o byte `+0x49` do registro
  é '0': decrementa o contador `+0xEC` (halfword) do bloco; se ainda != 0, `+0xAA = 0x20`; se chegou a 0, `+1`.
  Senão: se (`+0xD6 & 2` do bot ou byte `+0x4F` do registro != 0) e `+0xCA` do bloco != 0: `+0xAA = 0x20`; caso
  contrário `+1`.
- Subestado: 0x20 (espera, entrada H20 fora da lista) ou +1 (-> H1E).
- Confiança: confirmado (código lido: valores e ramos). Significado dos campos `+0xEC`/`+0xCA`: chute.

## Máquina de estados (esboço, despachante H, bloco 0x03001800)

    (redo confirmado no código lido; H00 e H0D ainda não redescritos)
    H01 -> 0x28 | 0x0A | 0x30 | 0x38 | H02 (decisão por bits de +0xB7/+0xB8)
    H02 -> 0x06 | H03
    H03 -> H04 (+1 incondicional)
    H04 -> H05 (+1 só quando o slot chega a 10; timer 0x3C)
    H06 -> H07 | 0x0A (decisão)
    H07 -> H08 (+1) ; H08 -> H09 (+1 após timer 0x3C)
    H0A -> H0B (+1) ; H0B -> H0C (+1 após timer 10)
    H0E -> H0F (+1 após timer 0x3C) ; H10 -> H11 (+1, sfx 0x2F)
    H11 -> H12 (+1 após timer 0x3C) ; H13 -> H14 | H16 (+1 ou +3)
    H14 -> H15 (+1) ; H16 -> H17 (+1) ; H17 -> H18 (+1 ao fim da anim do slot 10)
    H1A -> H1B | H1D (+1 ou +3) ; H1D -> H1E | 0x20 (decisão)
    Holds: H15 (só +B5), H1D (0x20 espera; sai só quando a condição fecha).
    Não-chute: E0B e F02 são hold (+B5=1).

Dispatchers E e F e G: E01 -> E02 (+1, cargas de fase) ; E04 -> E05 (+1 ao chegar) ; E05 -> E06 (+1 ao chegar) ;
E06 -> E07 (+1 ao chegar) ; E07 -> E08 (+1 após 30 frames) ; E09 -> E0A (+1 ao fim da anim) ; E0B hold (+B5=1).
F00 -> F01 (+1, cópia de 2 blocos) ; F02 hold. G02 -> G03 (+1 na 3ª etapa) ; G04 -> G05 (+1) ; G05 -> G06 (+1 após 10 frames).

## Pendências

- Entradas H 0x1E..0x4F (fora da lista): a tabela `0x083AD790` continua com ponteiros de código válidos
  (ex.: 0x20 -> 0x0801ECC1, 0x24 -> 0x0801EED5, 0x28 -> 0x0801EEE5, 0x30 -> 0x0801F019, 0x38 -> 0x0801F0F1,
  0x3E -> 0x0801F215, 0x4A -> 0x0801ED51). A lista de HA2 parou em 0x1D. Recomenda-se estender HA2_lista.txt.
- Redo (esta rodada): as seções marcadas "chute" foram relidas com o Ghidra. Renomeadas com o padrão
  `Fight_DispatchOnSubstateAA_<Desp>_<idx>_<Passo>`. As notas antigas "sem +1 no trecho lido" (E01, F00, G02, H03,
  H07, H11, H16) estavam erradas: esses handlers somam +1 (ou +3) no código.
- Tail-calls por ponteiro (E05 e G02) não recuperados pelo Ghidra ("UNRECOVERED_JUMPTABLE"): o próximo passo
  desses dois handlers fica em aberto.
- Não redescritos nesta rodada (sem "chute" na confiança): H00 (E300), H0D (E918), G00/G01/G03/G06, E02/E03/E08.

Pendente redo: 12 seções sem "chute" na confiança ainda não redescritas (só conferidas por grep): E02 (0x0801D738), E03 (0x0801D878), E08 (0x0801DD30), F02 (0x0801DED4, já confirmada), G00 (0x0801DF0C), G01 (0x0801DF60), G03 (0x0801E09C, 157 instr.), G06 (0x0801E2B8), H00 (0x0801E300), H0D (0x0801E918), H15 (0x0801EAC4, já confirmada), H18 (0x0801EBB8, já confirmada). Pendente também: tail-calls de E05 e G02 (jump table não recuperada) e a lista HA2_lista.txt termina em 0x1D.
