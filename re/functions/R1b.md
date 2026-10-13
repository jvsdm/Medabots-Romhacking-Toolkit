# R1b: funções 0x08034EB8..0x08035880 (34 funções, Medabots Metabee EU)

Documentação feita no Ghidra (MCP). Nomes já existentes no Ghidra ou no mtbe.csv foram reaproveitados. Renomeei no Ghidra
28 funções que ainda estavam como FUN_. Ponteiros DAT_ não foram resolvidos um a um: offsets são relativos ao registro do
bot (base = 0x03000590 + bot*0x100) ou ao slot de objeto (stride 0x40).

## 0x08034EB8 Fight_ReadActionScriptInlineArgs
- Assinatura: `void f(void)`
- O que faz: copia os bytes de argumento inline do opcode atual do script de ação para fight_information (+0xAC..).
  Tamanho vem da tabela de opcodes; posição do cursor em +0xBC.
- Chama: não verificado. Chamada por: não verificado.
- Memória: lê script de ação via +0xA8, +0xBC; escreve +0xAC.. (fight_information).
- Para o Lua: sim (interpretador do script de ação).
- Confiança: confirmado.

## 0x08034F08 Fight_AdvanceActionScriptCursor
- Assinatura: `void f(void)`
- O que faz: zera o byte de repetição +0xAA e avança o cursor +0xBC em 1 + tamanho do argumento do opcode atual.
- Memória: escreve +0xAA, +0xBC; lê tabela de opcodes.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08034F44 Fight_SideHasPartCode15
- Assinatura: `int f(int side)`
- O que faz: varre 3 slots de parte do lado (base side*0x100 + 0x49, stride 0x18); retorna 1 se algum byte for 0x15.
  O significado do código 0x15 não foi confirmado.
- Chama: não verificado. Chamada por: não verificado.
- Memória: lê registro do bot (+0x49, +0x61, +0x79).
- Para o Lua: sim, se a regra for portada (confirmar o significado de 0x15 antes).
- Confiança: estrutura confirmada; significado chute.

## 0x08034F68 Fight_ApplyDamageToPart
- Assinatura: `int f(int side, int part, int dmg)`
- O que faz: compara o dano com a armadura atual da parte (+0x59 + part*0x18). Se o dano for menor, aplica tudo
  (grava em fight_information +0xC8/+0xCC, zera +0xD1). Se for maior ou igual, grava a armadura como aplicada e o
  excedente em +0xCA, com +0xD1=1.
- Chama: não verificado. Chamada por: não verificado.
- Memória: lê +0x59+part*0x18; escreve fight_information +0xC8..+0xD1.
- Para o Lua: sim (núcleo do dano por parte).
- Confiança: provável (nome antigo do CSV: do_damage_on_part).

## 0x08034FC8 Fight_SetBotSlotPosFromDirection
- Assinatura: `void f(int bot, uint flags, int mode)`
- O que faz: move o slot de objeto do bot (índice bot*4+0x5D, stride 0x40) em X (mode 0) ou Y (mode != 0) a partir da
  posição do bot (+0x13C/+0x13E), com passo ±1/±2 conforme bits 1-2 de flags. Depois chama Fight_PlaceBotMarkerSlotsAroundBase.
- Chama: Fight_PlaceBotMarkerSlotsAroundBase (0x080350E0).
- Memória: escreve slot de objeto (tabela via DAT_08035024); lê +0x13C/+0x13E do bot.
- Para o Lua: não precisa se o Lua tiver sprite próprio; senão sim (posição de sprite).
- Confiança: provável.

## 0x080350E0 Fight_PlaceBotMarkerSlotsAroundBase
- Assinatura: `void f(int bot)`
- O que faz: posiciona três slots auxiliares (bot*4+0x5A, 0x5B, 0x5C) em volta do slot-base do bot: um 8 px acima e um
  8 px para cada lado. Marcadores/sombras.
- Chama: nenhuma. Chamada por: Fight_SetBotSlotPosFromDirection (0x08034FC8), Fight_SetBotBaseSlotAndMarkers (0x0803512C).
- Memória: escreve slots de objeto (stride 0x40).
- Para o Lua: não precisa (apresentação).
- Confiança: confirmado (estrutura lida).

## 0x0803512C Fight_SetBotBaseSlotAndMarkers
- Assinatura: `int f(int bot)`
- O que faz: copia a posição do bot (+0x13C, +0x13E+4) para o slot-base do bot (bot*4+0x5D) e chama
  Fight_PlaceBotMarkerSlotsAroundBase.
- Chama: Fight_PlaceBotMarkerSlotsAroundBase. Chamada por: não verificado.
- Memória: lê +0x13C/+0x13E do bot; escreve slot de objeto.
- Para o Lua: não precisa (apresentação).
- Confiança: provável.

## 0x08035164 Fight_SubtractPartArmorClampZero
- Assinatura: `void f(int side, int part, int amount)`
- O que faz: subtrai amount do byte +0x59 + part*0x18 do registro do bot, sem descer abaixo de zero. Pelo uso em
  Fight_QueueHeadBrokenBotForResolution (+0x59 = armadura da cabeça), o byte é a armadura atual da parte.
- Chama: nenhuma. Chamada por: não verificado.
- Memória: escreve +0x59+part*0x18 do bot.
- Para o Lua: sim (dano em parte).
- Confiança: provável.

## 0x08035194 Fight_BackupHalvedStatsIfFlagClear
- Assinatura: `void f(int side)`
- O que faz: se o byte +0xA1 do lado for 0, copia para +0xA2..+0xA6 os bytes +0x95..+0x99 divididos por 2.
- Memória: lê +0x95..+0x99, +0xA1; escreve +0xA2..+0xA6.
- Para o Lua: sim, se a lógica de modificador for portada (significado de +0x95..+0x99 não confirmado).
- Confiança: provável (estrutura confirmada; semântica chute).

## 0x080351E8 Fight_CopyStatsIfFlagSet
- Assinatura: `void f(int side)`
- O que faz: se +0xA1 do lado for diferente de 0, copia +0x95..+0x99 para +0xA2..+0xA6 sem dividir. Par de
  Fight_BackupHalvedStatsIfFlagClear.
- Memória: lê +0x95..+0x99, +0xA1; escreve +0xA2..+0xA6.
- Para o Lua: sim (mesma lógica).
- Confiança: provável.

## 0x0803526C Fight_StepSlotCoordUpInRange
- Assinatura: `u64 f(int slot, short min, short max)`
- O que faz: avança a coordenada do slot (+4) pelo acumulador (+0x25, bits 0-1 mantidos), com o incremento vindo da
  tabela DAT_080352C4 indexada por +0x30. Se o resultado sair de [min, max], soma o passo (+0x24) ao alvo +0x30 (teto
  0xFF) e retorna 0; senão fixa em min e retorna 1.
- Memória: lê/escreve slot (stride 0x40): +0x04, +0x24, +0x25, +0x30.
- Para o Lua: não precisa (animação de sprite), salvo se for movimento de bot.
- Confiança: provável.

## 0x080352EC Fight_StepSlotCoordDownInRange
- Assinatura: `u64 f(int slot, int min, short max)`
- O que faz: versão decrescente de Fight_StepSlotCoordUpInRange (usa 0xFF - alvo e subtrai). Mesmo retorno.
- Memória: slot (stride 0x40): +0x04, +0x24, +0x25, +0x30.
- Para o Lua: não precisa (sprite).
- Confiança: provável.

## 0x08035234 Fight_QueueHeadBrokenBotForResolution
- Assinatura: `void f(int bot)`
- O que faz: se a armadura da cabeça do bot (+0x59) for zero, grava bot+1 no primeiro slot livre (3 entradas) da fila
  de cabeça quebrada em fight_information.
- Memória: lê +0x59 do bot; escreve fila (3 bytes) em fight_information.
- Para o Lua: sim (resolução de KO por cabeça).
- Confiança: confirmado.

## 0x08035478 Fight_SelectLowestPriorityTargetBot
- Assinatura: `void f(uint side)`
- O que faz: escolhe o alvo. Para side < 3: procura o menor valor (ushort +2) entre 5 entradas ativas (flag +0x540).
  Para os outros lados: procura o maior valor entre 3 entradas. Grava índice+1 em +0xE0 do bot atual.
- Memória: lê tabelas de prioridade (DAT_080354C0 / DAT_0803551C); escreve +0xE0.
- Para o Lua: sim (escolha de alvo da IA).
- Confiança: provável.

## 0x08035528 Fight_SelectRandomTargetPartForSelectedBot
- Assinatura: `void f(int bot)`
- O que faz: se o bot tem alvo (+0xE0 != 0), pega a linha com Fight_GetStatusRowIndex(alvo-1), sorteia 0..7
  (randbyte & 7) e grava +1 em +0xE1 (parte-alvo).
- Chama: Fight_GetStatusRowIndex, randbyte. Chamada por: não verificado.
- Memória: lê +0xE0; escreve +0xE1.
- Para o Lua: sim (escolha de parte-alvo).
- Confiança: provável.

## 0x0803536C Fight_SetFacingSlotX
- Assinatura: `void f(int slot, uint flags, short x)`
- O que faz: grava o X do slot (+2) a partir de x, com deslocamento +1 ou -1 conforme bits 1-2 de flags (tabelas
  diferentes por direção). Sem deslocamento, grava x direto.
- Memória: escreve slot (stride 0x40) +0x02.
- Para o Lua: não precisa (sprite).
- Confiança: provável.

## 0x080353AC (mantido FUN_080353AC)
- Assinatura: `void f(uint flags)`
- O que faz: grava 0 ou 2 no campo +2 de um slot fixo (ponteiros DAT_080353c8 / DAT_080353bc) conforme bit 1 de flags.
  Não ficou claro qual slot é nem por que o deslocamento é 2.
- Memória: escreve slot fixo +0x02.
- Para o Lua: não precisa se for só apresentação.
- Confiança: chute (nome mantido como FUN_).

## 0x080353CC Fight_StepSlotAnimOffsetTable
- Assinatura: `int f(int slot)`
- O que faz: se o slot está ativo (bit 0x8000 em +0x00) e o índice de passo (+0x23) não chegou a 0x20, soma a tabela
  de passos (DAT_08035408[índice]) a +0x04, incrementa o índice e retorna 0. Caso contrário retorna 1 (fim).
- Memória: lê/escreve slot (+0x00, +0x04, +0x23).
- Para o Lua: não precisa (animação de sprite); se o Lua reproduzir a animação, usar a tabela original.
- Confiança: provável.

## 0x0803556C Fight_RollStatusRowRandomEntry
- Assinatura: `u8 f(void)` (o decompile não mostra o parâmetro do bot; o valor vem do registro)
- O que faz: obtém a linha com Fight_GetStatusRowIndex e devolve tabela[linha*8 + (randbyte & 7)].
- Chama: Fight_GetStatusRowIndex, randbyte. Chamada por: não verificado.
- Memória: lê tabela de sorteio (ponteiro DAT_0803558c).
- Para o Lua: sim, junto com a tabela (dados).
- Confiança: provável.

## 0x08035590 Fight_GetStatusRowIndex
- Assinatura: `int f(int bot)`
- O que faz: retorna uma linha 0..9 para a tabela de sorteio, somando: 1 se +0x71 != 0 (sinal), +3 se +0x89 != 0,
  +5 se +0xA1 != 0.
- Memória: lê +0x71, +0x89, +0xA1 do bot.
- Para o Lua: sim (usa os mesmos flags de status).
- Confiança: provável (flags não nomeados).

## 0x080355C4 Fight_CopySelectedTargetToFightInfo
- Assinatura: `void f(int bot)`
- O que faz: copia o alvo escolhido (+0xE0, +0xE1, base 1) para fight_information +0x88 e +0x8B, em base 0.
- Memória: lê +0xE0/+0xE1; escreve fight_information +0x88/+0x8B.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x080355F0 Fight_StepSlotTowardTarget
- Assinatura: `u64 f(int slot)`
- O que faz: se o slot já está na posição alvo (+0x30, +0x32) retorna 1. Senão calcula a direção com FUN_080442b4,
  grava em +0x25, chama FUN_0804422c e Fight_SlotWithin2OfTarget. Dentro de 2 px, cola no alvo e retorna 1; senão 0.
- Chama: FUN_080442b4, FUN_0804422c, Fight_SlotWithin2OfTarget.
- Memória: lê/escreve slot (+0x02, +0x04, +0x25, +0x30, +0x32).
- Para o Lua: não precisa (movimento de sprite), salvo se o Lua movimentar objeto.
- Confiança: provável.

## 0x08035658 Fight_SlotWithin2OfTarget
- Assinatura: `int f(int slot)`
- O que faz: retorna 1 se X e Y do slot estão a até ±2 px do alvo (+0x30, +0x32); senão 0.
- Memória: lê slot +0x02, +0x04, +0x30, +0x32.
- Para o Lua: não precisa.
- Confiança: confirmado.

## 0x08035694 Fight_ShowObjectSlot
- Assinatura: `void f(int slot)`
- O que faz: liga o bit 0x200 de +0x00 do slot e fixa +0x08/+0x09 em 0x40.
- Memória: escreve slot +0x00, +0x08, +0x09.
- Para o Lua: não precisa (apresentação).
- Confiança: provável.

## 0x080356B4 Fight_HideObjectSlot
- Assinatura: `void f(int slot)`
- O que faz: limpa bits de +0x00 do slot com a máscara DAT_080356c8 (esconde o slot).
- Memória: escreve slot +0x00.
- Para o Lua: não precisa.
- Confiança: provável.

## 0x080356CC Fight_UpdateFollowerObjectSlot
- Assinatura: `int f(int leader_slot, uint follower_slot)`
- O que faz: se o líder está com Y < 0xA0 e o seguidor está dentro da faixa do líder, liga o bit 15 do seguidor
  (Slot_SetFlagBit15) e ajusta +0x08/+0x09 pela distância; senão limpa a máscara (Slot_ClearAttrMask).
- Chama: Slot_SetFlagBit15, Slot_ClearAttrMask.
- Memória: lê/escreve slots (+0x04, +0x08, +0x09, flags).
- Para o Lua: não precisa.
- Confiança: provável.

## 0x08035740 Fight_AddByteClampToMax
- Assinatura: `void f(u8 *p, int add, uint max)`
- O que faz: *p = *p + add, limitado a max (se passar, grava max).
- Memória: escreve *p.
- Para o Lua: sim (utilitário de stat).
- Confiança: confirmado. Antigo nome do CSV: safe_add.

## 0x0803572C Fight_SubClampZeroByte
- Assinatura: `void f(u8 *p, int amount)`
- O que faz: *p = *p - amount, sem descer abaixo de zero. Versão byte de Fight_SubClampZero.
- Memória: escreve *p.
- Para o Lua: sim (utilitário).
- Confiança: confirmado.

## 0x08035754 Fight_SubClampZero (nome já existente)
- Assinatura: `void f(short *p, int amount)` (confirmado pelo uso em Fight_ApplyStatusCostIfFlag).
- O que faz: subtrai amount sem descer abaixo de zero (16 bits). O mtbe.csv chama de safe_subtract.
- Para o Lua: sim (utilitário).
- Confiança: confirmado.

## 0x0803576C Fight_GetMedalPartCompatibilityBonus
- Assinatura: `int f(int bot)`
- O que faz: soma o byte +7 das 4 partes cujo byte +4 (medal_compatibility) coincide com a classe de medalha do bot.
- Chama: nenhuma. Chamada por: Fight_AccumRosPart (0x0803579C), Fight_AccumRosBase (0x08035844).
- Memória: lê registro do bot (+0x48 + k*0x18, +4, +7).
- Para o Lua: sim (cálculo de ROS).
- Confiança: confirmado (bate com mtbe.csv).

## 0x0803579C Fight_AccumRosPart (nome já existente no Ghidra)
- Assinatura: `void f(int bot, int part)`
- O que faz: acumula o termo de ROS da parte: byte +0x4D da parte, bônus de medalha (0x0803576C), rand&7, nível de
  habilidade (tabela indexada pelo skill id da parte) e bônus de alcance (+0xA5 ou +0xA6, conforme FUN_08042b80).
  Acumulador em +0xE8.
- Chama: Fight_GetMedalPartCompatibilityBonus, randbyte, FUN_08042b80. Chamada por: não verificado.
- Memória: lê parte e tabelas; escreve acumulador (+0xE8).
- Para o Lua: sim (fórmula do ROS).
- Confiança: confirmado (lido). O mtbe.csv chama de Fight_GetPartProficiencyBase (mesmo código).

## 0x08035814 Fight_ApplyStatusCostIfFlag (nome já existente no Ghidra)
- Assinatura: `void f(int bot)`
- O que faz: se o efeito pendente do bot (+0xC9) for 0x0D ou 0x0E, subtrai +0xCC do acumulador (DAT_08035840) com
  Fight_SubClampZero.
- Chama: Fight_SubClampZero. Chamada por: não verificado.
- Para o Lua: sim.
- Confiança: confirmado. O mtbe.csv chama de Fight_ApplyVirusBugRosPenalty; 0x0D/0x0E = Bug/Virus confirma o CSV.

## 0x08035844 Fight_AccumRosBase (nome já existente no Ghidra)
- Assinatura: `void f(int bot)`
- O que faz: acumula em +0xEE: byte +5 (legs success/propulsion), bônus de medalha, rand&7.
- Chama: Fight_GetMedalPartCompatibilityBonus, randbyte. Chamada por: não verificado.
- Para o Lua: sim.
- Confiança: confirmado (lido). O mtbe.csv chama o mesmo termo de "defender ROS reduction".

## 0x08035880 Fight_CheckPairCondition (nome já existente no Ghidra)
- Assinatura: `int f(int attacker, int part, int defender)`
- O que faz: retorna 1 se o byte de técnica da parte (+0x49 + part*0x18) for 0x22 e o estado de movimento do defensor
  (+0x91) for 0x64 ('d'); senão 0.
- Para o Lua: sim (regra Anti-Air contra alvo no ar, se for essa a leitura).
- Confiança: provável. O mtbe.csv chama de Fight_IsAntiAirAgainstAirTarget; o código bate com essa descrição.

## Conflitos de nome para o merge (mtbe.csv não foi editado)
- 0x08035754: Ghidra "Fight_SubClampZero" vs mtbe.csv "safe_subtract".
- 0x08035814: Ghidra "Fight_ApplyStatusCostIfFlag" vs mtbe.csv "Fight_ApplyVirusBugRosPenalty" (o CSV descreve melhor).
- 0x08035880: Ghidra "Fight_CheckPairCondition" vs mtbe.csv "Fight_IsAntiAirAgainstAirTarget" (o CSV descreve melhor).
- 0x0803579C: Ghidra "Fight_AccumRosPart" vs mtbe.csv "Fight_GetPartProficiencyBase" (mesmo código).
- 0x08035740: mtbe.csv "safe_add"; Ghidra agora "Fight_AddByteClampToMax".
- 0x08034F68: mtbe.csv "do_damage_on_part"; Ghidra agora "Fight_ApplyDamageToPart".
