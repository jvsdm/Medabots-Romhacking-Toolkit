# AS6: tabela Medaforce id -> efeito, ids 0x18..0x2D e 0x31 (despachantes BC..BR)

Leitura: ROM original (so leitura) via `python -I` (tabelas) e decompile/disassembly Ghidra. Sem emulador, sem texto do jogo.
Continua AS5 (secao 4). Confianca: confirmado = lido no decompile/literal; provavel = nome + contexto; aberto = nao fechado.

## 1. Cadeia dos despachantes (confirmada)

- Stub `ActionScript_Op5B..Op6A_CallBx` (0x08034CD4 + 0x1C*k): chama `Fight_DispatchOnSubstateAA_Bx` e, se fight+0xB5 != 0, avanca o cursor.
- `Fight_DispatchOnSubstateAA_Bx` (0x0802AAD0..0x0802FE50): zera fight+0xB5, depois `CallViaR0(tabela[fight+0xAA])`.
- Tabela de passos = literal em dispatcher+0x24 (ex.: BC -> 0x083AE800, BD -> 0x083AE840, ..., BR -> 0x083AEC98).
- Passos listados em ordem de indice ate o primeiro NoOp (0x08000840). Entradas sao ponteiros thumb (bit 0 setado).
- Ressalva: alguns dispatchers tem passos de dano DEPOIS do NoOp (BI 0x10..0x1C, BL 0x0A..0x14). Eles so sao alcancados
  por salto de passo (fight+0xAA escrito por outro handler), que nao rastreei. Esses casos estao marcados "aberto".
- Campos recorrentes: fight+0xD2+k (alvo marcado), fight+0xD6 (slot por alvo), fight+0xEC (EC = valor do efeito),
  fight+0xC8 (valor de dreno), fight+0x87 (indice do bot ativo), fight+0x60 (base do lado), fight+0xA6 (codigo de mensagem).

## 2. Tabela final id -> efeito (uma linha por id)

| id | despachante | efeito |
|---|---|---|
| 0x18 | BC | soma EC no contador do proprio bot (bot+0xD1, teto 255); so visual e mensagem |
| 0x19 | BD | soma EC no contador do proprio bot (bot+0xD0, teto 255); so visual e mensagem |
| 0x1E | BE | sorteia status por alvo (0x0D/0x10/0x11/0x13) nos adversarios marcados (provavel: lado adversario) |
| 0x1F | BF | buff: +EC no byte de atributo bot+0x4D dos aliados marcados |
| 0x20 | BE | igual ao 0x1E |
| 0x21 | BF | buff: +EC no byte de atributo bot+0x4E dos aliados marcados |
| 0x22 | BG | ganho de medidor: transfere EC do slot fight+0x8A para o slot fight+0x89 (fight+0x28, teto 0xA0) (provavel) |
| 0x23 | BF | buff: +EC no byte de atributo bot+0x50 dos aliados marcados |
| 0x24 | BH | ganho de medidor: +EC em bot+0xB9 dos aliados marcados (teto 120) e -EC em bot+0xB9 dos adversarios marcados |
| 0x25 | BF | buff: +EC no byte de atributo bot+0x51 dos aliados marcados |
| 0x26 | BI | visual de slot central + mensagem 0x59; dano de cabeca so pelos passos pos-NoOp (aberto) |
| 0x27 | BJ | dano em armadura da peca 0 (cabeca) dos adversarios marcados (valor = slot D6); quebra se cabeca zerar |
| 0x28 | AV (Op54) | status 0x2C nos adversarios (ja em AS5) |
| 0x29 | BK | status pendente 0x2B nos adversarios marcados (bot+0xC9), valores EC/E8 (exibicao); mensagem 0x5E |
| 0x2A | BL | dano em peca aleatoria de cada adversario marcado: dano fixo 200 e -EC no valor da peca; passo 0x12 pos-NoOp (aberto) |
| 0x2B | sem despachante | ja em AS5 (4B/4C: acha bot com peca, armadura da peca alvo, dano) |
| 0x2C | BM | projeteis nos adversarios e depois dano em peca aleatoria de cada marcado (valor slot D6), com dreno fight+0xC8 do valor da peca |
| 0x2D | BN | marca bit 0x08 em bot+0xC0 dos aliados vivos; so visual (sem HP) |
| 0x2E | BO | +EC no contador do proprio ator (bot+0xD5, teto 255); visual e mensagem 0x51 |
| 0x2F | BP | dano na peca 3 (perna) dos adversarios com valor pendente (slot D6); -fight+0xC8 no valor da peca |
| 0x30 | BQ | marca bit 0x01 em bot+0xC0 do proprio ator; visual; mensagem 0x4F |
| 0x31 | BR | visual de 4 slots; marca bit 0x04 em bot+0xC0 do ator; mensagem 0x6A; sem HP |

Resumo por tipo:
- Dano em grupo (peca aleatoria dos marcados): 0x2A (BL), 0x2C (BM). Dano em peca 3 de marcados: 0x2F (BP). Dano em cabeca (armadura) de marcados: 0x27 (BJ).
- Cura: nenhuma neste conjunto (cura de time e Op56..58 ja em AS5).
- Status: 0x1E/0x20 (sorteio 0x0D/0x10/0x11/0x13), 0x29 (0x2B pendente). 0x28 vem de AV (0x2C).
- Ganho de medidor/contador: 0x18, 0x19, 0x2E (contadores do ator bot+0xD1/D0/D5); 0x22 e 0x24 (medidor de aliados e adversarios).
- Buff de atributo: 0x1F, 0x21, 0x23, 0x25 (bytes +0x4D/+0x4E/+0x50/+0x51 dos aliados).
- So marca de reacao/visual: 0x2D, 0x30, 0x31 (bits em bot+0xC0: 0x08, 0x01, 0x04).

## 3. Campos do bot mexidos (0x03000590 + i*0x100)

- +0x4D/+0x4E/+0x50/+0x51: atributos (aliados, 0x1F..0x25).
- +0xB9: medidor (aliados +EC teto 120; adversarios -EC) (0x24).
- +0xC0: flags de reacao (bit 0x01 em 0x30, 0x04 em 0x31, 0x08 em 0x2D).
- +0xC9..+0xCC: status e valores de exibicao (0x29; 0x1E/0x20 pelo passo de status).
- +0xD0 / +0xD1 / +0xD5: contadores do ator (0x19 / 0x18 / 0x2E).
- +0xD2+k (fight), +0xD6 (fight, slot por alvo): marcas e valores de dano pendentes.
- +0x59 + k*0x18 (peca k): armadura/valor da peca (dano em 0x2A, 0x2C, 0x2F; armadura em 0x27).
- Fight+0xEC (EC) e fight+0xC8 (dreno) sao os parametros de todos os efeitos acima.

## 4. Opcodes 4A, 4D, 4F (fechados)

- 4A `ActionScript_Op4A_CountBrokenAllyParts` (0x0803496C): para os aliados com bot+0xBA != 0, cada peca com valor 0 soma 10 em fight+0x8C. Nao causa dano.
- 4D `Fight_RollDamageFromTwoDice` (0x08034AA0): dois sorteios de 0..15 em tabela; grava fight+0x188 e outro byte; fight+0xEC = (d1+d2)*10. Define o EC que o efeito seguinte usa (id 0x32 e demais "dado duplo").
- 4F `ActionScript_Op4F_SetEffect10IfNoGroupMatch` (0x08034B68): se nenhum adversario vivo casa o campo 0x91 (grupo) do ator, fight+0xEC = 10 (valor de fallback).

## 5. Op51 e o subpasso AS (ids 0x04..0x09)

- `ActionScript_Op51` (0x08034BBC) chama `Fight_DispatchOnSubstateAA_AS` (0x0802793C), tabela 0x083AE580[fight+0xAA].
- Passos AS: 00 SelectVariantByTechnique, 01 LoadIconAtActor, 02 SpinIcon, 03 SetupSecondSprite, 04 ShrinkSecondSprite, 05 WaitTimerOrSkip, 06 ShrinkAndClearSprites, 07 SetStatusFlagByTechnique, 08 SetStepCodeByTechnique, 09 controle (0x080305E1), 0A controle, depois NoOp.
- Passo 07 (0x08027E1C): id = bot ativo+0xA8. Para ids 04..08 (id-4 < 5) seta um bit em bot+0xD6:
  04 -> 0x01, 05 -> 0x08, 06 -> 0x02, 07 -> 0x10, 08 -> 0x04. Qualquer outro id (ex. 09) -> bit 0x20.
- Passo 08 (0x08027EDC): fight+0xA6 = codigo de mensagem: 04 -> 0x4A, 05 -> 0x61, 06 -> 0x4B, 07 -> 0x62, 08 -> 0x4C; outro -> 0x63.
- Portanto Op51 para 0x04..0x09 e so marca de estado no proprio bot (bit em +0xD6) + mensagem. Nao mexe em HP.
- Os passos 00..06 sao visual (icone/sprites). Valores de variante (passo 00) nao lidos.

## 6. Renames no Ghidra

- `Fight_DispatchOnSubstateAA_BO_09_AddValueToUserByte05` -> `Fight_DispatchOnSubstateAA_BO_09_AddValueToUserByteD5` (literal 0x03000665 = bot+0xD5).
- `Fight_DispatchOnSubstateAA_BP_08_ApplyTeamPendingDamage` -> `Fight_DispatchOnSubstateAA_BP_08_ApplyPendingDamagePart3` (dano na peca 3, nao no time).
- Nomes confirmados sem mudanca: BC/BD_AddValueToBot*, BE_*, BF_*, BH_BoostAllies..., BJ/BI commit armor, BK_0E, BL_12, BM_0F, BN_08, BQ_09, BR_07 (ver nota: nome Bit2 = valor 0x04).
- `Fight_DispatchOnSubstateAA_BH_CommitArmorCheckHeads` esta compartilhado com BI/BJ, mas o prefixo BH e so do nome; nao renomeei.

## 7. Pendente

- BI (0x26) e BL (0x2A): passos de dano depois do NoOp (BI 0x10..0x1C, BL 0x0A..0x14). Falta achar o handler que escreve fight+0xAA para eles.
- Quem preenche o slot D6 (valor de dano) antes de BJ/BM/BP e o valor fight+0xC8 (dreno): nao lido.
- Campos +0x4D/+0x4E/+0x50/+0x51 e +0xD0/+0xD1/+0xD5 (contadores) e fight+0x28 (medidor) ainda sem nome de uso: confirmei a escrita, nao o significado.
- Os bits 0x01/0x02/0x04/0x08/0x10/0x20 de +0xD6 (AS passo 07) nao foram cruzados com quem os le.
