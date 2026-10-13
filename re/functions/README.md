# Funções da ROM (Medabots Metabee EU) — documentação função por função

Lista de partida: alvos de BL no código (0x08000000-0x080B0000), 1507 funções, divididas em 5 faixas
(`R0_lista.txt` .. `R4_lista.txt`). Funções chamadas só por ponteiro (tabelas de opcode, callbacks) entram quando
aparecem na leitura.

Cada faixa tem:
- `Rk.md`: uma seção por função (formato abaixo), na ordem de endereço.
- `Rk_simbolos.csv`: `address,name,kind,comment` para fundir depois no `re/symbols/mtbe.csv`.
- `Rk_progresso.txt`: último endereço documentado (para continuar de onde parou).

Formato de cada seção:

    ## 0x08012345 nome_sugerido
    - Assinatura: `int nome(int a, u8 *b)`
    - O que faz: 1-3 frases.
    - Chama: ... / Chamada por: ...
    - Memória: RAM/IO/ROM lida e escrita (endereços).
    - Para o Lua: o que precisa ser portado (ou "não precisa": BIOS, DMA, hardware).
    - Confiança: confirmado (lido no código) | provável | chute.

## Dicas para economizar contexto (medidas nas primeiras rodadas)

- Antes de decompilar, `get_function_by_address` (barato): se o endereço cai no meio de outra função, o
  decompile devolve a função inteira que o contém (já custou ~12k tokens em 3 cópias). Anote "miolo de FUN_x" e siga.
- Endereço sem função no Ghidra: `disassemble_function` só se precisar; quase sempre é literal pool ou dado.
- `FUN_0807CCC0` = wrapper do SWI 0x0C (CpuFastSet). Wrappers de BIOS em 0x0807CC..: uma linha cada.
- Não leia `pc/research/README.md` nem `mtbe.csv` inteiros: use grep pelo endereço.
- Auxiliares de aritmética (confirmados na R4): `0x0807CD2C` = divisão com sinal (NÃO é multiplicação nem seno —
  notas antigas em R0/R2/R4 que dizem isso estão erradas), `0x0807CDF8` = resto com sinal, `0x0807CEC8` = divisão
  sem sinal, `0x0807CF40` = resto sem sinal (provável), `0x0807CCF4` = trampolim `bx r1` (o efeito é do alvo em r1).
- Nomes: inglês, estilo `Subsistema_VerboObjeto` (como o mtbe.csv).
- Bloco do bot na batalha (R1, confirmado no literal pool 0x0801593C): base = 0x03000590 + bot*0x100. A pesquisa do PC
  usa 0x030005D0 = base + 0x40, então offset Ghidra = offset da pesquisa + 0x40 (peças: Ghidra +0x4C = pesquisa +0x0C).
- `FUN_08041D04` = memcpy de bytes (não é som).
- Jump tables: o literal do pool aponta PARA a tabela (ex.: literal 0x0801A7B0 -> tabela em 0x0801A7B4, 9 ponteiros
  de handler). Ler as palavras na ROM é barato; vale recuperar quando a tabela tem poucas entradas (≤ 12).
