# Saltos indiretos das notas FS1/FS7 (GH-JT7, 2026-10-14)

Escopo: 0x08004644 (ponteiro DAT_08004678), 0x08004CA4 (DAT_08004DAC), 0x0803E120 (tabela 0x0803E168).
Método: disassemble no Ghidra (leitura), palavras e literais da ROM original (python -I, só leitura).
Retorno = `pop {rN}; bx rN` com o LR empilhado no prólogo. Tabela real = `ldr rB,[literal]` + `ldr rX,[rB+idx*4]` + `mov pc,rX`.

## Resumo

| Endereço | Hospedeira | Resultado | Tabela |
|---|---|---|---|
| 0x08004644 | PartEditor_DispatchSubmodeStep | TABELA REAL, 5 entradas | 0x0800467C |
| 0x08004CA4 | TeamPick_CompareSelectionStep | RETORNO (FS1 errado) | nenhuma |
| 0x0803E120 | Anim_DrawSegmentThenJumpPtr | RETORNO (FS7 errado) | nenhuma |

## 1. 0x08004644 PartEditor_DispatchSubmodeStep: tabela real

- Padrão: `ldr r0,[0x08004670]` (= ctx 0x03000440); `add r0,#0x5a; ldrb r0` (índice = byte 0x0300049A);
  `cmp r0,#4; bhi 0x08004750` (fora de 0..4 vai para o retorno); `lsl r0,#2`; `ldr r1,[0x08004678]`
  (valor do literal = 0x0800467C, a tabela); `ldr r0,[r1+r0]`; `mov pc,r0` (em 0x0800466C).
- Tamanho do índice: 5 entradas (0..4), 20 bytes em 0x0800467C..0x0800468F.
- Alvos lidos da ROM (bit 0 do Thumb tirado, confere com o disassembly):
  - 0 -> 0x08004690
  - 1 -> 0x080046C4
  - 2 -> 0x080046E8
  - 3 -> 0x08004708
  - 4 -> 0x08004728
- O prólogo do corpo é `push {r4,lr}`, mas os alvos não têm `push`: são labels internos do mesmo corpo, não funções.
- Antes do despacho a função zera u8 0x030004D6 (ctx+0x96), zera u32 0x03000430 e limpa 0x80 entradas de OAM (0x08040CD0 = ClearObjectAttributeEntries).
- O que cada caso faz (r4 = 0x0300047C em todos; os demais literais: 0x03002B8C e 0x0300137C são os de estado e subestado):
  - Caso 0: chama Mem_FillBytes(r4,0x14,0) (0x0803F624), r4[0]=1, r4[1]=ctx+0x5B, chama 0x080070F0 (Parts_CountInventoryMatches, o retorno é descartado), grava 4 em 0x03002B8C e 8 em 0x0300137C, depois retorna.
  - Casos 1..4: mesmo Mem_FillBytes; r4[0]=1; r4[1]=byte ctx+0x5C (1), +0x5D (2), +0x5E (3), +0x5F (4); r4[2]=0 (caso 1), 1 (caso 2), 2 (caso 3), 3 (caso 4).
  - Cauda comum (0x08004744): grava 5 em 0x03002B8C e 0x10 em 0x0300137C, depois retorna.
  - Conclusão: os casos 1..4 são quatro variantes de "inicializa sub-bloco de 0x14 bytes com o byte de seleção e avança o estado". O caso 0 é a variante que conta peças do inventário.
- Nomes dados no Ghidra (rename_data, tentativa; confirmar): 0x0800467C PartEditor_SubmodeJumpTable; 0x08004690 PartEditor_SubmodeCase0_CountParts; 0x080046C4 PartEditor_SubmodeCase1_Init; 0x080046E8 PartEditor_SubmodeCase2_Init; 0x08004708 PartEditor_SubmodeCase3_Init; 0x08004728 PartEditor_SubmodeCase4_Init. Comentário em 0x08004666.
- Confiança: confirmado (código + 5 palavras da ROM batem com o disassembly). Os significados de 0x03002B8C/0x0300137C no contexto de peças são prováveis: no mtbe.csv eles têm outros nomes (gGameStateMenu, gMainSubStateCounter), que não batem com este uso. Não foram alterados.

## 2. 0x08004CA4 TeamPick_CompareSelectionStep: NÃO é tabela

- O "ponteiro DAT_08004DAC" de FS1 é o `bx r0` de 0x08004DAC, precedido de `pop {r0}` (0x08004DAA): é o retorno do prólogo `push {lr}`.
- O que há de fato no meio é um if/else: `ldrb` de +0x51 com `cmp #0 / cmp #1 / else` (ramos 0x08004CC8, 0x08004CE4, 0x08004CF8), e depois um teste de bit de um u16 por +0x52 com `cmp #0 / cmp #1 / else`. A comparação final (0x08004D6E..0x08004D9A) é uma cadeia de `if`, sem indireção.
- Confiança: confirmado. FS1 deve trocar a frase "decide por jump table (ponteiro DAT_08004DAC)" por "decide por if/else; retorno em 0x08004DAC".

## 3. 0x0803E120 Anim_DrawSegmentThenJumpPtr: NÃO é tabela

- Prólogo `push {lr}`; em 0x0803E166 `pop {r0}` e em 0x0803E168 `bx r0`. É o retorno. Não há tabela em 0x0803E168 (são 2 bytes de código da função, não dados).
- O corpo lê um byte de script (base 0x0803E158 + 0x19A + byte em base+0x1A2, via `ldrb`) e compara com 4: se igual, incrementa o u32 apontado por 0x0803E15C; se diferente, grava 0x48 no u32 apontado por 0x0803E16C. Antes disso incrementa o u16 em +0x86 (ponteiro 0x0803E154). Isso é o que FS7 descreve como "escolhe um destino". Não há salto indireto.
- O ponteiro 0x083AFD64 de FS7 é o ponteiro de função chamador (não é a tabela).
- Confiança: confirmado. FS7 deve corrigir a linha "chama um ponteiro de salto não recuperado" e "Pendente: jump table em 0x0803E168".

## Ghidra

- Comentários: 0x08004666 (tabela real), 0x08004CAC (retorno, não tabela), 0x0803E168 (retorno, não tabela).
- Renomeações tentadas (rename_data): tabela e 5 labels acima. A chamada devolveu "attempted"; falta confirmar que pegou nos labels de código.
- Nada foi salvo no Ghidra.

## Em aberto

- Confirmar no Ghidra que os 5 labels aparecem com nome (rename_data em endereço de código pode não ter efeito).
- Decidir se 0x03002B8C/0x0300137C no contexto de peças devem ganhar nome próprio (hoje são gGameStateMenu/gMainSubStateCounter em mtbe.csv, possivelmente errado para este uso).
- Corrigir FS1 (0x08004CA4, linhas de pendências) e FS7 (0x0803E120) conforme os itens 2 e 3.
