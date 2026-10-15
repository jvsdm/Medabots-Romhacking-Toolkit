# OB2: roteiro de ação com byte 04, passos A e quem leva +0xAA a 0x22

Lido no Ghidra (decompile) e na ROM original via python -I (tabelas de ponteiros, LEN, roteiros decodificados). Sem emulador.
Renames feitos no Ghidra (em memória, não salvei): 0x080688E0 -> `Fx_StepTableBUntilDone` (confirmado).
Scratch em `scratchpad/ob2/` (apagar no fim).

## (1) Quem executa o despachante A na Robattle

Cadeia (confirmada): `Fight_RunActionScriptLoop` 0x08033840 lê o byte do roteiro (`Fight_LookupActionScriptByte`);
enquanto o byte é >= 0x80 avança o cursor (`Fight_AdvanceActionScriptCursor`); senão chama
`0x083AF180[op]`. O laço é `do { flag=0; op(); } while (flag != 0)`, com flag = `+0xB5` (provável, pelo offset e pelo
Op04). Assim: Op04 zera +0xB5 via stub A; se o passo A terminou (handler põe +0xB5=1), Op04 avança o cursor e o laço
continua no mesmo quadro com o próximo opcode. Se não terminou, o laço para e o próximo quadro repete Op04 no mesmo byte.

Logo, o despachante A roda **uma vez por quadro, enquanto o roteiro estiver no byte 04**. Não é "por turno" nem
"abertura" por si só: o byte 04 é o que escolhe o laço A. Quem põe o roteiro no 04 é o início do roteiro da ação
(ver 2). Em aberto: o valor inicial de +0xAA em cada início de ação (provável zerado pelo setup, não verificado).

## (2) Roteiros com byte 04 (decodificados, LEN 0x083C6EB8 = só op 02 (1 byte) e op 03 (4 bytes) têm argumento)

Tabela de ponteiros 0x083C6CA8 (u32), índice = técnica 0..59 ou medaforça 0x50+id (80..95). Walk linear com cursor += 1+LEN[op],
bytes >= 0x80 pulados de 1 em 1.

- Stream compartilhado **0x083C6A04**: começa com `04`, depois `05 03(80 81 81 81) 06 07 ... 21 01`. Ponteiros: técnicas
  0-9, 13-20, 34, 35 e também 60-79 apontam para ele (conforme a tabela; 60-79 não confirmei o uso).
- **0x083C6B32** (técnica 52, `gActionScript_TeamAttack`): começa com `04`, depois `05 03 ... 3B 3C 3D 3E 3F ... 01`.
- Nenhum outro roteiro (medaforças 80-95 e demais técnicas) contém 04 como opcode. Confirmado na decodificação linear.

Ops do roteiro compartilhado depois do 04 (linear): 05, 06, 07, 08, 09, 0A, 0B..0D, 0E..10, 11..18, 19..1C, 1D..21, 22, 23, 24,
25, 26, 27, 28, 29..2D, 2E, 2F, 30..3F... (ver walk); o único desvio é o 03 (escolha por fight+0xAB, LEN 4).
Ops 0x22 e 0x2x aparecem como wrappers do tipo ActionScript_OpXX; só o 04 chama o stub A (xref 0x080339F6).

## (3) Quem leva +0xAA a 0x22 (cadeia A, pela tabela 0x083AD5CC)

Confirmado pelos decompiles (+0xAA = state; cada handler +1 ou valor fixo):
- **A/17** (0x0801CD41) grava +0xAA = **0x20** (fixo). É o único escritor literal de 0x20 encontrado.
  Entrada: A/16 = StatusCheck (0x080305E1), que faz +1 quando `Fight_RunSubstateDispatch(+0xA6)` retorna != 0.
  Anterior: A/15 (0x0801CD29) grava +0xA6=0x15 e +1 (-> 0x16).
- **A/20** (0x0801CF79, DrainTargetValueStep) -> +1 para 0x21 (ou 0x2D / 0x2E, em outros ramos).
- **A/21** (0x0801CFFD, LoadSideTilesThenJump) -> **+1 = 0x22**. Esse é o passo que leva a A/22 (SetupBotAndSprites).
- Então: 0x16 -> 0x17 -> 0x20 -> 0x21 -> **0x22** (A/22, grava 0x03002C30+0x848.. e +1 -> 0x23 = A/05).
- Entradas alternativas do início: A/00 (0x0801C525, `Fight_HandleRangeMismatchFailure`): escolhe família por +0x89/+0x2B
  e grava +0xA6 = 0x0E/0x0F/0x10/0x11 (+0x51 = 0..3) e +1; senão 0x2D.
- Ramos de terminação: A/1D (0x0801CE95) grava +0xAA = 0x0E (fixo). A/1B espera `Fight_TickSlotScriptsUntilDone(0x21,4)` e só então +1.

Não rastreei o caminho COMPLETO de 0x00 até 0x16 (vários handlers e a jump table de A/21 não recuperada). Em aberto.

## (4) Condição 0x080688E0 (fx_step_table_b, agora Fx_StepTableBUntilDone)

Decompile: `r = tableB[*DAT_08068908](...)`; se r != 0 zera o contador `*DAT_0806891C`; senão incrementa `*DAT_0806890C`;
devolve (r != 0). Quem chama (Fx_ClearIwramBlockMaskDispcntDisarm 0x08068EA8) desarma 0x030017F0 quando r != 0.
Ou seja: a condição é "o handler de efeito da tabela B terminou". Confirmado.
(Correção ao OB.md: a condição não é "modo"; é o resultado do handler B.)

## Renames e comentários no Ghidra

- 0x080688E0: fx_step_table_b -> `Fx_StepTableBUntilDone` (confirmado) + comentário.
- Nomes já corretos nesta rodada: 0x0801CD41 A_17_JumpTo20, 0x0801CF79 A_20_DrainTargetValueStep, 0x0801CFFD A_21_LoadSideTilesThenJump,
  0x0801CE95 A_1D_FinishAndLoadSideTiles, 0x0801CDB5 A_1B, 0x0801CDD5 A_1C, 0x0801CD29 A_15, 0x080305E1 ActionFamily_StatusCheckFamily_Handler.
  Renomear 0x0801C525 (A/00) para Fight_DispatchOnSubstateAA_A_00_RangeMismatchFamilySelect fica pendente (não mexi).

## Em aberto

- Valor inicial de +0xAA ao começar um roteiro (quem zera: setup 0x083AEEA0[tec] ou o op anterior). Não verificado.
- Caminho completo A 0x00 -> 0x16 e a jump table tail-call de A/21 (0x0801CFFD, "could not recover jumptable").
- Ramo 0x03 (escolha por +0xAB): mapa dos 4 argumentos 0x80..0x8A não lido.
- Técnicas 60-79 apontam para 0x083C6A04 (por que; sem uso confirmado).
