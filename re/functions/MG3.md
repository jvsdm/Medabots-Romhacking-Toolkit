# MG3: medidor de Medaforce (B+0xC1) e fim de roteiro (GH-MG3, 2026-10-14, EM ANDAMENTO)

Leitura estática: ROM original (só leitura, `python -I`) + Ghidra (sem renomear/salvar até confirmar). Bloco do bot
B = 0x03000590 + bot*0x100 (Ghidra); B+0xC1 do bot 0 = 0x03000651.

## Passo 1: quem escreve B+0xC1 (varredura da ROM)
- Literal 0x03000651 na ROM: 0 hits (o código calcula B + 0xC1 a partir de 0x03000590 ou do ponteiro do bot).
- Varredura Thumb: `add rX,#0xC1` / `mov rX,#0xC1` seguido de ldrb/strb com base rX, em 0x08000000..0x0803FFFF.
  Escritas (strb) encontradas:
  - 0x0803628C região (+0xC1 soma com teto 0x50): 0x080362AC = STRB (ja confirmado, MF2 §6).
  - 0x0802EB0E: `add r0,#0xC1` + `strb r1,[r0]` (dentro/perto de BM_0F 0x0802EAD8): candidato a zerar o medidor do inimigo.
  - 0x0801AF84 e 0x0801AFD0 (+ strb 0x0801AF8E e 0x0801AFDA): perto de Fight_StepBotVisualDown 0x0801AFB8, que
    decrementa +0xC1 (DF.md). Candidato a "consumo" visual.
  - 0x0872DBBA / 0x08634798 / 0x0875B550: fora da área de código (dados), ignorados.
- Leituras (ldrb, sem escrita): 0x0801546E, 0x080155CC (gate), 0x08015C56, 0x08015D12, 0x08015E76, 0x08015ECE,
  0x0801741C, 0x0801747E, 0x080174E2, 0x080178A6, 0x080178DC, 0x08017922, 0x0801AD1A, 0x08025B64, 0x08025C94,
  0x08025CCA, 0x08025CE6, 0x0802E4A0 (BM_00), 0x08030D0A, 0x080316DA, 0x080332D6, 0x0803367E.
- Ainda NÃO lidos no Ghidra: handler 0x51 (0x0802793C, `Fight_DispatchOnSubstateAA_AS`), BA_00..BA_07,
  BA_SetMessage54 (0x0802A694).

## Passo 1 (Ghidra, confirmado no decompile)
- **Escritor 1 (zera): `Fight_DispatchOnSubstateAA_BM_0F_DrainMedaforceAsDamage` 0x0802EAD8** (nome do Ghidra já
  existente). Para cada um dos 3 bots do lado (`fight+0x60 + k`) com byte `+0xD2` != 0: grava **0** em
  `B+0xC1` (`(bot*0x100) + 0x03000590 + 0xC1`), depois aplica dano na peça (`Fight_ApplyDamageToPart`) e
  zera o contador. Ou seja: a técnica BM_0F "drena" a medaforça (o medidor vai a 0) e converte em dano.
  Por bot: só se `+0xD2` marcado. Confiança: confirmado (decompile).
- **Decrementador (visual): `Fight_StepBotVisualDown` 0x0801AFB8** (DF.md): se `fight+0x51` != 0 e `B+0xC1` != 0,
  faz `B+0xC1 -= 1` e `fight+0x51 -= 1`, redesenha o medidor e o número. Confirmado (decompile). Não zera de uma
  vez: desce 1 por chamada.
- Escritor de soma: `Fight_AddMedaforceChargeFromAction` 0x0803628C (MF2 §6), confirmado.
- Não é `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction` (zera B+0xE0/E1, não o medidor).
- **Decrementador do consumo (confirmado): BR_46 `Fight_DispatchOnSubstateAA_BR_46_WaitMedaforceAnim` 0x0803186A**
  chama `Fight_StepBotVisualDown` a cada quadro (bot = fight+0x87) enquanto a animação de medaforça roda. Quando a
  animação acaba (`-1`), reseta contadores visuais (`Fight_ResetBotVisualCounters` 0x0801AEB4, só zera fight+0x9C..0xA0,
  não mexe em B+0xC1). Ou seja, o medidor cheio desce de 0x50 para 0 com a animação. Chamador de StepBotVisualDown
  também: AJ_0E 0x08025D7C (`StepBotVisualDownWaitSlot`).
- Incrementador: `Fight_StepBotVisualUp` 0x0801AF84 (confirmado): `B+0xC1 += 1` até 'P' (0x50), `fight+0x51 -= 1`.
- Resposta curta ao passo 1: quem zera B+0xC1 ao usar medaforça é o BR_46 (decremento até 0 pela animação); quem zera
  de uma vez é BM_0F (técnica de drenar, lado inimigo). Passo 1 da varredura: nenhum outro `add/mov #0xC1` + store.
  Limite: a varredura só pega `add/mov rX,#0xC1` seguido de byte load/store; escritas com 0xC1 vindo de literal ou de
  soma de registradores não foram cobertas.

## Passo 2: op 0x01 (0x080338DC) -> cena 8 / GS 0x40 (confirmado no decompile + ROM)
- `ActionScript_Op01_SetSceneExitState` 0x080338DC: `*0x03002B8C = 8; *0x0300137C = 0x40;` (só isso).
- Cena 8 = `Fight_SceneMain` 0x08030860 (MN.md): chama `CallViaR0(tabela 0x083AECD8[GS])`, com GS = 0x0300137C.
  Entrada 0x40 da tabela = ponteiro 0x08030E78 = `Fight_DispatchOnSubstateAA_BR_50_CheckLeadersDefeated` (decompile).
- BR_50 (confirmado): `Fight_StepBotPartTimer`/`SelectBotActivePart` no bot 0; se `bot0+0x40 == 0` então resultado 2,
  fade 4, SFX 0x6F, GS++; senão se `bot3+0x40 == 0` então resultado 1, fade, SFX 0x6F, GS++; senão **GS = 0x48**
  (a batalha continua).
- **Resposta:** o 0x01 NÃO sai do combate. Ele devolve o controle à cena de batalha principal (cena 8) no passo de
  checagem de fim de ação (BR_50). Dali: se um líder (bot 0 ou bot 3) está com +0x40 zerado, a batalha termina
  (fade + SFX de vitória/derrota, resultado 1/2); senão volta ao laço (GS 0x48). Por isso a hipótese "fim de segmento
  = saída" está errada: 0x01 é o fim do roteiro, a volta é para a batalha. A parte de "bot+0x40 = líder derrotado"
  é provável (o nome do handler e os dois ramos), não confirmada no emulador.
- O 0x3F (AS2) faz o mesmo com GS 0 (ponto de entrada do laço da cena 8, sub 0 = 0x08030891).

## Correção ao passo 1: o quanto o medidor desce (confirmado no decompile)
- `Fight_DispatchOnSubstateAA_BR_26_BuildPartObjects` 0x08030DB4: grava `fight+0x51` = **0x28** por padrão, ou **0x50**
  se `B+0xC0 & 8` (mesma flag que dobra a carga em `Fight_AddMedaforceChargeFromAction`).
- O BR_46 desce `B+0xC1` 1 por quadro enquanto `fight+0x51 != 0`. Então o consumo é `min(fight+0x51, B+0xC1)`:
  com a flag, 0x50 zera o medidor; sem a flag, desce só 0x28 (fica em 0x28). Ou seja, "zerar" só vale no caso com
  flag; no caso comum o medidor cai pela metade. Provável; falta confirmar no emulador (medidor cheio, técnica
  de medaforça sem flag, ler B+0xC1 antes e depois).
- Ainda sem certeza de que BR_26 é o caminho de toda medaforça (BR_20 gate `C1 == 0x50` antes dele). Pendente.

## Passo 3: mensagens +0xA6 (gTextGameMessages 0x083C6744), sentido em poucas palavras
- 0x05: "a Medapart da ação está quebrada" (falha por peça-fonte quebrada; produtor B00 0x0801D318). Confirmado pelo texto.
- 0x0C: "a Medapart alvo está quebrada" (produtor C00 do ramo do alvo; AB.md). Confirmado pelo texto.
- 0x16: "<X> protegido" (proteção de um bot; produtor E09 0x0801DD74 no follow-up de aliado). Confirmado pelo texto.
- 0x17: "<X> ação <Y>" (resultado de uma ação com nome do bot/técnica; produtor F00 0x0801DE44, preparo de dados).
  Sentido exato de <Y> (MEM:2) não confirmado; provável "resultado".

## Status final
- Passo 1: fechado (quem zera/desce B+0xC1). Passo 2: fechado (0x01 volta a batalha no BR_50). Passo 3: fechado (sentido das 4 mensagens).
- Renomeação no Ghidra: nenhuma nova; as funções-chave já tinham nome. Nada salvo.
- Pendente: confirmar no emulador o consumo com flag B+0xC0&8 vs sem flag; BR_20 vs BR_26 na ordem da medaforça; significado de bot+0x40 (líder derrotado, provável).
