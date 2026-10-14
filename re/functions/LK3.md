# LK3: comandos de link, tabela 0x080AFCC0 e passos do menu 0x15 (Metabee EU)

Feito pelo agente GH-LK3 (2026-10-14). Lido no Ghidra (decompile, xrefs, nomes) e na ROM original (só leitura, python -I).
Offset = endereço - 0x08000000. Sem emulador. Confiança: **confirmado** (lido no código ou no literal), **provável**, **chute**.
Símbolos em `LK3_simbolos.csv`. Código sem função no Ghidra em `LK3_lista.txt`. Renomeados no Ghidra: §5.

**Correções ao LK2.md** (feitas aqui):
- A tabela 0x080AFCC0 **não é indexada pelo comando**: o índice é `gamestate_subroutine` (0x0300137C).
- `0x08065ED0` (handler 1) é uma função de 0x08065ED0 a 0x0806622D (despachante de 6 subestados por obj+0x3A), não só
  "checa inventário".
- `0x080440B4` (`Count_Entries3InTable9`) **não** varre o inventário: conta bytes == 3 em 9 registros de 8 B a partir de
  **0x030017A0** (bloco de time). Só `0x080440D4` (`Check_PairsAllEqual`) varre **0x03004C40** (inventário).
- O "pool 0x08066860" não é o meio de uma função perto de 0x08066760: é o literal que a função `0x08066824` lê (ela
  começa em 0x08066824, depois do pool de 0x08066860 ser lido).

## 1. Quem indexa a tabela 0x080AFCC0 (item 1, confirmado)

- Literal 0x080AFCC0 aparece **uma vez** na ROM: pool em **0x08066860** (busca por bytes, única ocorrência).
- Literal 0x080AFC88 aparece **uma vez**: pool em **0x08065D3C** (lido por `Fight_CallHandlerWithArgCopy` 0x08065D10, LK2 §2).
- O pool 0x08066860 é lido pela função **0x08066824** (`Fight_CallHandlerCopyArgs52`, renomeada aqui para
  `Menu14_CallSubstateHandler`). Ela copia 0x34 B (13 ponteiros) de 0x080AFCC0 para a pilha e chama
  `CallViaR0(pilha[*0x0300137C])`. Depois, se `*0x03001780` tem bit 0, soma 1 em `*0x03001C04` e em `+2` (contador, provável).
- Quem chama 0x08066824: **não há BL**. É a entrada **0x14 da MainState2** (tabela `0x08483594`, +0x50 = 0x084835E4,
  valor 0x08066825). Ou seja, roda quando `gMainState == 2` e `gamestate_menu == 0x14`.
- Quem põe `gamestate_menu = 0x14`: `GameState_StartFlowByCommand` (0x08047878), **cmd 0x05** (MN.md §2.1). O cmd 5 é o
  comando de cena que vem **depois** do `StartFlow(4)` do evento 0x76 (LK.md §1, `Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05`).
- Ordem: evento 0x76 → menu 0x15 (passos 0..7, handshake no 5) → menu 0x14 (passos 0..12, §2).

## 2. Os 13 passos do menu 0x14 (item 2)

Tabela 0x080AFCC0 (ponteiros Thumb; o byte baixo é +1 do endereço). **A tabela é mista**: não é só "comandos de link".

| sub | Thumb | Função (Ghidra) | Papel | Confiança |
|---|---|---|---|---|
| 0 | 0x08065D44 | sem função no Ghidra (código: push r4-r6,lr; começa com `mov r0,#0`, `mov r1,#0xC0`) | pendente | pendente |
| 1 | 0x08065ED0 | `Screen_DispatchByState3A` (0x08065ED0..0x0806622D) | despachante de 6 subestados por obj 0x03006F74+0x3A (JT3, ramo A se `0x03000B90 != 0`, ramo B se == 0). Lê ID do aparelho (0x03000B90) e slots (0x03005010). Chama `Count_Entries3InTable9` (time) e `Check_PairsAllEqual` (inventário 0x03004C40). Provável: checagem de sincronia antes da troca/comando. | confirmado (código); papel provável |
| 2 | 0x08066240 | `Anim_CounterDoneInitObjectSlot` | se o contador de animação (0x03006F74+0x50) termina: `Obj_InitSlotAndLoad(1,0,1)`, sub++ (`0x03001C..`), liga bit 0x4000 | confirmado (código) |
| 3 | 0x08066280 | `Menu_SelectionFiveOptionsStep` (renomeada aqui `Link_SelectFiveOptionsStep`) | menu de **5 opções** (cursor obj+5 = 0..4; sobe/desce com 0x40/0x80). Se entrada remota tem 0xEF00 no halfword recebido, copia o byte baixo para obj+5 e liga obj+0x7A (escolha do peer). Se o jogador confirma (A), grava **0xEF00 \| cursor** no halfword de envio, `snd_sfx 4`. | confirmado (código); significado das 5 opções: pendente |
| 4 | 0x08066460 | `Link_CommandExchangeStep` (LK2 §4.2) | 0xEF00\|cmd, timer 0xFE0F, ecoa; se obj+5 == 4 vai ao estado 8 | confirmado (LK2) |
| 5 | 0x08066534 | `Save_WaitSfxThenFlash` | espera SFX 4 terminar, `snd_song_request(0)`, `snd_sfx_request(0)`, `Save_InvokeFlashOp`, sub += 2 | confirmado (código): **é tela de SALVAR** |
| 6 | 0x080665B0 | sem função no Ghidra (código: push lr; carrega ponteiro de objeto, soma 0x50 e chama uma função) | pendente | pendente (não lido por completo) |
| 7 | 0x08066574 | `Save_RunOptionsThenBlendFade` | até 4x `Site_RunSaveLoadOption(0)`, depois `Screen_InitBlendFade` | confirmado (código): **SALVAR** |
| 8 | 0x080665FC | `Fx_InitStringStateMachine` | monta string de 0x270 (0xFB... terminador -1), `StateMachine_DispatchStep`, `Fx_InitRecord8(1,0x16,7)` | confirmado (código): texto/efeito |
| 9 | 0x0806668C | sem função no Ghidra (código: push lr; chama uma função e testa o resultado) | pendente | pendente |
| 10 | 0x080666C8 | `Fx_TickStateThenInitRecord8` | tick de máquina de estado (0x48 do bloco 0x080666EC), depois `Fx_InitRecord8` | confirmado (código): efeito |
| 11 | 0x080666F4 | sem função no Ghidra (código: push lr; chama uma função; se o resultado é 0, incrementa um contador e grava) | pendente | pendente |
| 12 | 0x08066714 | `Text_BuildMessageAndStep` | monta mensagem (ponteiro em 0x03000... +0x3C, terminador -1) e avança a máquina; ao fim: se `+0x2A` == 0 usa 0xFF00 senão 0x... ; se ambos halfwords 0xFFxx e iguais, `Obj_InitTimerRecord(...,0xC0)` e sub=5; senão sub=3 e +5=6 | confirmado (código): mensagem final (ex.: resultado/salvar) |

**Leitura do item 2:** o menu 0x14 é a **sequência pós-link**: mensagens e efeitos (8, 10, 12), uma escolha de 5 opções
com envio ao peer (3, 4), e **salvar** (5, 7). Não há um handler de troca de peça ou medalha.

## 3. Troca de peças e medalhas por link (item 3)

- **Não existe função de troca por link** no código lido. Busca por nome ("Trade", "Exchange", "Link") não achou troca; os
  nomes achados são de transporte e de estado (LK.md, LK2.md).
- **Como os dados viajam** (confirmado): cada rodada leva **1 halfword por quadro** (`QueueLinkTransferPacket` 0x080022DC,
  payload = 1 hw, LK.md §3). O que é enviado nos passos lidos: **só um byte de comando/escolha** (0xEF00 | 0..4 ou
  0xEF00 | cmd). Nenhum passo lido monta um identificador de peça (bot, tipo, atributos) nem move `script.save.parts`
  ou o inventário 0x03004C40.
- **Consequência:** uma troca de peça exigiria várias rodadas (ex.: escolher a peça, enviar bot, tipo e id, confirmar).
  Isso não aparece no código lido. Se existe, está fora desta tabela (ou no menu 0x15 passo 6/7, ainda não lidos).
- Veredito: **não confirmado**. Candidato provável ainda aberto: passos 6 e 9 do menu 0x14 e os passos 0, 2, 6, 7 do
  menu 0x15 (sem função no Ghidra).

## 4. Passos 0..7 do menu 0x15 (item 4)

Tabela 0x080AFC88 (confirmada, `Fight_CallHandlerWithArgCopy` 0x08065D10 chama `[0x0300137C]`).

| sub | Thumb | Função | Papel | Confiança |
|---|---|---|---|---|
| 0 | 0x08065790 | sem função (LK2: chama 0x0803FAA0 e 0x08074CE0, toca obj 0x03006F74) | preparo (provável) | pendente |
| 1 | 0x080657C4 | `Sm_StepSfxRepeat3` | máquina de estado com **3 confirmações (A)**; cada uma toca SFX 0xD e reinicia o passo; ao 3º, marca o bit 2 do byte de controle e avança | confirmado (código): espera de texto com A x3 |
| 2 | 0x08065A80 | sem função (chama `StateMachine_DispatchSub` 0x08041CA4) | espera de texto (provável) | pendente |
| 3 | 0x08065A0C | `Battle_TickTwoStageSequence` | 2 estágios; A avança (SFX 0xD); no 2º fecha o objeto (`+0x84=0`, `+0x1B=0`) e decrementa `gamestate_subroutine` | confirmado (código): confirmação em duas etapas |
| 4 | 0x08065A94 | `Menu15_Sub4_WaitAThenAdvance` (renomeada aqui) | passo de estado (0x10 ou 0x14 do bloco, conforme +8). Se +8 == 0: conta e zera +0x3A; senão, A: SFX 0xD, limpa flags e subtrai 2 do contador | confirmado (código); papel exato: provável |
| 5 | 0x08065B0C | `Link_HandshakeFrameStep` | handshake por quadro com o peer (LK.md §4.1) | confirmado (LK.md) |
| 6 | 0x08065C98 | sem função no Ghidra | troca de comandos (LK2): toca 0x03005010 e 0x03000B90 | provável (LK2); não lido aqui |
| 7 | 0x08065CFC | sem função no Ghidra | fim da etapa (LK2: chama `StateMachine_DispatchSub` e 0x080476E8) | provável (LK2); não lido aqui |

**Leitura do item 4:** o menu 0x15 é a **sequência de conexão**: espera de texto (1), confirmação (3), passo de estado (4),
handshake (5), depois comandos (6) e fim (7). O handshake é o único passo confirmado de link.

## 5. Renomeados no Ghidra (nesta rodada)

- `Fight_CallHandlerCopyArgs52` (0x08066824) → `Menu14_CallSubstateHandler` (confirmado: CallViaR0 com índice 0x0300137C).
- `Menu_SelectionFiveOptionsStep` (0x08066280) → `Link_SelectFiveOptionsStep` (confirmado: grava 0xEF00 | cursor).
- `FUN_08065a94` → `Menu15_Sub4_WaitAThenAdvance` (confirmado no corpo; o papel exato é provável).
- `Count_Entries3InTable9` (0x080440B4): renomeei e **voltei** ao nome original (mtbe.csv usa esse nome). Registrado aqui:
  lê o bloco de time 0x030017A0, não o inventário.
- Os demais: só no CSV.

## 6. Pendente

- Sub 0, 2, 6, 7, 9 e 11 de 0x080AFCC0 e sub 0, 2, 6, 7 de 0x080AFC88: sem função no Ghidra (LK3_lista.txt). Criar a função é
  trabalho do MedabotsSync, fora deste escopo.
- O significado das 5 opções de `Link_SelectFiveOptionsStep` (cursor 0..4): confirmar com emulador (não usado nesta rodada).
- O papel exato de `Fight_..._Dispatch` 0x08065ED0 (subestados 0..5 com obj+0x3A) e de `Check_PairsAllEqual` (pares de
  bytes iguais em 0x03004C40): provável, não confirmado.
- Troca de peça/medalha por link: não achada. Se existir, deve estar nos handlers sem função acima.
