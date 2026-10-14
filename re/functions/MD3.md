# MD3: leitores de bot+0xAA e bot+0xAD, e ação normal x medaforça (GH-MD3, 2026-10-14, PARCIAL)

Leitura estática: ROM original (só leitura, `python -I`, capstone Thumb) + decompile do Ghidra (sem renomear, sem
salvar). Continua MD2.md (§4 e §5). Símbolos em `MD3_simbolos.csv`.

## 1. Quem lê bot+0xAA (b2 do descritor)

Confirmado (decompile):
- `Fight_DispatchOnSubstateAA_BR_45_SetupMedaforceSlots` 0x08031704: `fi+0x51 = B[fi+0x87]+0xAA` (B = 0x03000590 +
  bot*0x100; literal em 0x08031854). É o **único leitor de B+0xAA achado**. Ou seja, o b2 do descritor vira
  `fi+0x51`. Confiança: confirmado no decompile. Uso posterior de fi+0x51 (lado/índice em HA1): pendente.
  Como b2 chega a 0x50 e fi+0x51 é usado com valores pequenos, o uso de fi+0x51 depois precisa ser lido antes de
  dar significado a b2 (pode ser chance/custo ou só índice de passo de animação).

Refutado (não são leitores de bot+0xAA):
- `Fight_DispatchOnSubstateAA_AS` 0x0802793C e `Fight_DispatchOnSubstateAA_BA` 0x0802A0E8: são stubs que zeram
  fi+0xB5 e fazem `CallViaR0(table[fi+0xAA])`. Leem **fi+0xAA** (substado da luta), não o bot. Os despachantes
  AS..BB (0x0802793C..0x0802FE50) são todos desse tipo (fight+0xAA); não há leitura de bot+0xAA neles.
- `Fight_DispatchOnSubstateAA_BF_SelectAlliesAndStat` 0x0802B810: o hit do scan em 0x0802B82A era **fi+0xAA**
  (`fi+0xAA++` no fim), não B+0xAA. Falso positivo.

## 2. Quem lê bot+0xAD (b5)

Nenhum leitor achado. Varredura por literal 0x03000590 (477 referências Thumb, janela linear de ~90 instruções,
rastreio de registrador) não encontrou ldr/ldrb/str com offset 0xAD nem `add #0xAD`. Não há literal 0x0300063D
na ROM (0x03000590+0xAD). Hipótese que sobra: o campo é lido por função que recebe o ponteiro do bot como parâmetro
(como MD2 viu com +0xAB/+0xAC), e o scan não cobre isso. Pendente.

## 3. Como a ação normal (peça) grava fight+0xA8 e bot+0xA8..0xAD

- `Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart` 0x08030A24 (decompile): depois de validar a peça,
  `fi+0xA8 = B[fi+0x87]+(B+0xDD-1)*0x18+0x49` (byte de técnica da peça; B+0xDD = índice da peça, k = dd-1, offset
  0x49 do bloco de 0x18 por peça; ver TQ.md). Também grava `B+0xA7 = B+0xDD - 1` (bot, não fi) e `fi+0x164 = 1`. **Não grava
  nenhum byte de bot+0xA8..0xAD.**
- Ou seja, na ação normal o índice do roteiro (fi+0xA8) vem da técnica da peça (0..59), e a medaforça (0x50+id)
  é que vem de bot+0xA8 (copiado por `Fight_SelectMedaforceActionSource` 0x08015668, validado no scan: o único
  acesso a +0xA8 com `Mem_CopyBytes_Dup`).
- bot+0xAB e bot+0xAC na ação normal: nenhum acesso encontrado no caminho da ação normal (BR_14 não toca). Os
  leitores conhecidos (0x0803769C, 0x080376F4 de MD2) recebem o ponteiro do bot; quem os chama na ação normal não
  foi lido (pendente).
- Leitor de bot+0xA8 na ação normal/BF: `Fight_DispatchOnSubstateAA_BF_SelectAlliesAndStat` 0x0802B810 lê
  `B[fi+0x87]+0xA8` (só se for 0x1F/0x21/0x23/0x25) e grava `fi+0x51` = 0..3; depois `fi+0xAA++`. Este é um
  leitor de bot+0xA8 que vale para os dois caminhos (pendente confirmar se é só medaforça).

## 4. Resumo do que mudou

- B+0xAA: lido SÓ por BR_45 (-> fi+0x51). AS..BB não leem bot+0xAA.
- B+0xAD: sem leitor achado.
- Ação normal: fi+0xA8 vem de B+(k*0x18)+0x49 (técnica da peça), não de bot+0xA8; não escreve bot+0xA8..0xAD.

## 5. Pendências

1. Uso de fi+0x51 depois de BR_45 (quais passos de HA1 leem; se é chance/custo ou índice).
2. Leitor de bot+0xAD (b5): precisa varredura por ponteiro de parâmetro (chamadores de 0x0803769C e 0x080376F4).
3. Quem chama 0x0803769C/0x080376F4 no caminho da ação normal (bot+0xAB/AC).
4. Confirmar em runtime (proibido nesta tarefa).

Limites do método: a varredura de literal 0x03000590 só segue o registrador carregado no mesmo trecho linear.
Não cobre ponteiro passado por parâmetro nem por struct copiada. Falsos positivos (ex. fi+0xAA) aparecem.
