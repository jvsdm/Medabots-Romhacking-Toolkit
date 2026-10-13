# HA3: handlers dos despachantes Fight_DispatchOnSubstateAA_* (42 handlers, lista HA3_lista.txt)

Método: bytes da ROM original (`Medabots - Metabee (Europe).gba`) + decodificador Thumb simples (corpo linear,
ramos seguidos, pools ignorados). O Ghidra NÃO tem função definida nestes endereços (get_function_by_address e
disassemble_function falham). Nada foi criado nem renomeado no Ghidra. Nomes sugeridos só no CSV.
Seções "resumo": a leitura veio de lista de constantes e chamadas, não do corpo completo; leia o corpo antes de usar.

## Mecanismo (confirmado na ROM)
- Cada despachante (A..Z, AA..BR) é um stub com literal `0x03001800` (bloco de combate) e ponteiro de tabela
  no pool. Stub: zera +0xB5 e chama `tabela[+0xAA]` (ver H.md e R1a.md).
- Conferido nesta rodada: para as 42 linhas de HA3_lista.txt, `tabela_do_despachante[índice] == endereço` (42/42).
- Quase todo handler termina com `+0xAA += 1` (leitura/escrita de 0x03001800+0xAA). Alguns gravam valor fixo
  (H:0x24 grava 0x0A) ou também mexem em +0xAB.
- Template comum (passo de apresentação): carga de gráfico LZ77 para VRAM (0x06010000, 0x06010800, 0x06014800,
  0x06004120), cópia de paleta OBJ (0x050003C0, 0x050003A0, 0x050002E0), OBJs em gObjectSlots (0x03002C30,
  offsets 0x280..0x2E2), e som/efeito. Não decifrei qual ataque/texto cada passo mostra.
- Helpers sem nome no mtbe.csv: 0x0800FB9C, 0x0800FBC0, 0x0800FC55, 0x0800FCEF, 0x0800FD5B (carga de gráfico);
  0x0804DD23, 0x0804DD47, 0x0804DDDC, 0x0804DE76, 0x0804DEE2 (cópia de paleta); 0x08049DBC, 0x08049DE0, 0x08049EC7,
  0x08049F5C (som/efeito); 0x0802A0FB, 0x0802A14C, 0x08029E51, 0x0802AC46 (por bot/lado). Dado 0x083B6100 aparece
  em vários handlers (tabela/texto, não confirmado).
- Chamador conhecido: H (0x0801E2D8) é chamado de 0x080341DE (UNCONDITIONAL_CALL); a função que contém 0x080341DE
  não existe no Ghidra. Quem escolhe o despachante (a fase da Robattle) continua em aberto.

## Handlers (despachante:índice; "também" = outros despachantes que apontam para o mesmo código)

## 0x0801ECC0 Fight_DispatchOnSubstateAA_H_Substate20 (H:0x20)
- Lê bloco de bot (0x03000590 + bot*0x100, byte +0x49 == 0x30?) e +0x87/+0x8B; chama helpers 0x0802A0FB/0x0802A14C e 0x0802AC46 ou 0x08029E51. Grava +0xD0.
- Sem gráfico. Passo provável: atualização de status por bot (chute). +0xAA += 1.
- Confiança: confirmado (corpo lido); significado: chute.

## 0x0801ED50 Fight_DispatchOnSubstateAA_H_Substate21 (H:0x21; também H:0x4A, P, R)
- Ramo por byte do bot == 0x30: gráfico 0x084CDD68 (VRAM 0x06010000) + paleta 0x0850F8B8; senão 0x084CBBBC + 0x0850F738.
- OBJs em gObjectSlots; som 0x08049DBC/0x08049DE0. Junta os ramos e faz +0xAA += 1.
- Confiança: confirmado; passo: provável apresentação do bot que age (chute).

## 0x0801EED4 Fight_DispatchOnSubstateAA_H_Substate24 (H:0x24)
- Só grava +0xAA = 0x0A. Salto direto para o subestado 0x0A.
- Confiança: confirmado.

## 0x0801EEE4 Fight_DispatchOnSubstateAA_H_Substate28 (H:0x28)
- Gráfico 0x084CB9C8 para VRAM 0x06010000; paleta 0x0850F718 para 0x050003C0; OBJ 0xC404 em gObjectSlots+0x280; som 0x08049EC7.
- Escolhe fonte por +0xF8 bit 1. +0xAA += 1.
- Confiança: confirmado; passo: provável efeito de ação (chute).

## 0x0801EFC8 Fight_DispatchOnSubstateAA_H_Substate29 (H:0x29)
- Espera: testa flag 0x03001780 bit0; incrementa halfword gObjectSlots+0x284 e compara com gObjectSlots+0x2B0.
- Quando igual: grava 0x03000440+0x86 = 60 e +0xAA += 1. Senão só espera.
- Confiança: confirmado (código lido); passo: provável espera até a animação acabar (chute).

## 0x0801F018 Fight_DispatchOnSubstateAA_H_Substate30 (H:0x30; também AP)
- Gráfico 0x084CDB98 + paleta 0x0850F898; OBJ 0xC404; som 0x08049F5C. Escolhe fonte por +0xF8 bit 1. +0xAA += 1.
- Confiança: confirmado (corpo lido); passo: chute.

## 0x0801F0F0 Fight_DispatchOnSubstateAA_H_Substate38 (H:0x38)
- Gráfico 0x084D2E8C + paleta 0x0850FA38; OBJ 0xB800 em gObjectSlots+0x280. +0xAA += 1.
- Confiança: confirmado (corpo lido); passo: chute.

## 0x0801F1C8 Fight_DispatchOnSubstateAA_H_Substate3B (H:0x3B)
- Só +0xAA += 1 (sem gráfico). Passo de transição ou espera.
- Confiança: confirmado (corpo curto).

## 0x0801F214 Fight_DispatchOnSubstateAA_H_Substate3E (H:0x3E; também AQ, P, R)
- Gráfico 0x084CF274 para VRAM 0x06010800; paleta 0x0850F938; lê 0x03001800; +0xAA += 1.
- Confiança: confirmado (resumo); passo: chute.

## 0x0801F290 Fight_DispatchOnSubstateAA_H_Substate3F (H:0x3F; também AQ, P, R)
- Usa dado 0x083B6100 e gObjectSlots+0x2A2; +0xAA += 1.
- Confiança: resumo; passo: chute.

## 0x0801F2DC Fight_DispatchOnSubstateAA_H_Substate41 (H:0x41; também AQ, P, R)
- Gráfico 0x084D2DC4 para VRAM 0x06010000; paleta 0x0850F598; lê 0x030018C8 e 0x0300052E. +0xAA += 1.
- Confiança: resumo; passo: chute.

## 0x0801F374 Fight_DispatchOnSubstateAA_H_Substate44 (H:0x44; também AQ, P, R)
- Só +0xAA += 1 (passo vazio ou espera).
- Confiança: confirmado (corpo curto).

## 0x0801F390 Fight_DispatchOnSubstateAA_H_Substate45 (H:0x45; também AQ, P, R)
- Lê 0x030018C8 e timer 0x03000440. No resumo não achei escrita em +0xAA (não confirmado).
- Confiança: parcial.

## 0x0801F3F0 Fight_DispatchOnSubstateAA_H_Substate47 (H:0x47)
- Lê bloco de bot 0x03000590 e 0x03001800; +0xAA += 1.
- Confiança: resumo.

## 0x0801F474 Fight_DispatchOnSubstateAA_H_Substate4D (H:0x4D)
- Só +0xAA += 1.
- Confiança: confirmado (corpo curto).

## 0x0801F4AC Fight_DispatchOnSubstateAA_I_Substate00 (I:0x00)
- Lê 0x03001800 e bloco de bot 0x03000590; +0xAA += 1.
- Confiança: resumo; passo: chute.

## 0x0801F5AC Fight_DispatchOnSubstateAA_I_Substate01 (I:0x01; também I)
- Só lê 0x03001800. Sem +0xAA no resumo.
- Confiança: parcial; ler o corpo antes de usar.

## 0x0801F5BC Fight_DispatchOnSubstateAA_I_Substate02 (I:0x02)
- Gráfico 0x084CBE3C (VRAM 0x06010000) + paleta 0x0850F758; OBJ 0xC624 e 0x289/0x28A/0x28B/0x286; +0xAA += 1.
- Confiança: resumo longo (46 linhas); passo: chute.

## 0x0801F780 Fight_DispatchOnSubstateAA_I_Substate03 (I:0x03)
- Dado 0x083B6100; OBJ 0x289 e 0x2E2; +0xAA += 1.
- Confiança: resumo.

## 0x0801F800 Fight_DispatchOnSubstateAA_I_Substate04 (I:0x04)
- Vários gráficos (0x084CC0F8, 0x084CC578, 0x084CC90C, 0x084CCC24) com paletas 0x050003C0; OBJ 0xC404 e 0x1701; +0xAA += 1.
- Maior handler da lista (39 linhas no resumo). Confiança: parcial.

## 0x0801F9B0 Fight_DispatchOnSubstateAA_I_Substate05 (I:0x05)
- Timer 0x03000440; +0xAA += 1.
- Confiança: resumo.

## 0x0801F9EC Fight_DispatchOnSubstateAA_I_Substate07 (I:0x07)
- Lê bloco de bot 0x03000590; +0xAA += 1.
- Confiança: resumo.

## 0x0801FA68 Fight_DispatchOnSubstateAA_J_Substate00 (J:0x00)
- Carga de tiles de BG: VRAM 0x06004120, paleta 0x05000120; +0xAA += 1.
- Confiança: resumo.

## 0x0801FADC Fight_DispatchOnSubstateAA_J_Substate01 (J:0x01)
- Mexe em registradores de BG (0x04000008), gObjectSlots/0x03002414, 0x030017F0; chama NoOpActionHandler 0x08000841; +0xAA += 1.
- Confiança: provável.

## 0x0801FBC4 Fight_DispatchOnSubstateAA_J_Substate02 (J:0x02)
- Registrador 0x04000008 e 0x03000430; grava 0x1901/0x1A0A/0x1B0B (tilemap). No resumo não achei +0xAA.
- Confiança: parcial.

## 0x0801FD88 Fight_DispatchOnSubstateAA_J_Substate03 (J:0x03)
- Registrador 0x04000008; +0xAA += 1.
- Confiança: resumo.

## 0x0801FE14 Fight_DispatchOnSubstateAA_K_Substate00 (K:0x00)
- Só +0xAA += 1.
- Confiança: resumo.

## 0x0801FE64 Fight_DispatchOnSubstateAA_K_Substate02 (K:0x02)
- Gráfico 0x084CF178 para VRAM 0x06014800; paleta 0x0850F918 para 0x050002E0; +0xAA += 1.
- Confiança: resumo.

## 0x0801FEC8 Fight_DispatchOnSubstateAA_K_Substate03 (K:0x03)
- Só +0xAA += 1.
- Confiança: resumo.

## 0x0801FEE8 Fight_DispatchOnSubstateAA_K_Substate04 (K:0x04)
- Gráfico 0x084CDF48 para VRAM 0x06014800; paleta 0x0850F8D8; OBJ 0xC004; dado 0x083B6100; +0xAA += 1.
- Confiança: parcial.

## 0x0801FF9C Fight_DispatchOnSubstateAA_K_Substate05 (K:0x05)
- Dado 0x083B6100; OBJ 0x862; +0xAA += 1.
- Confiança: resumo.

## 0x08020004 Fight_DispatchOnSubstateAA_K_Substate06 (K:0x06)
- Lê 0x0300059A e 0x030004DE. Sem +0xAA no resumo.
- Confiança: parcial.

## 0x08020040 Fight_DispatchOnSubstateAA_K_Substate08 (K:0x08)
- Só +0xAA += 1.
- Confiança: resumo.

## 0x08020078 Fight_DispatchOnSubstateAA_L_Substate00 (L:0x00)
- Escreve +0xAA += 1 e +0xAB += 1.
- Confiança: resumo.

## 0x080200A8 Fight_DispatchOnSubstateAA_L_Substate02 (L:0x02)
- Gráficos 0x084CB744 (VRAM 0x06010000) e 0x084D2614 (VRAM 0x06010800); paletas 0x0850F6F8 e 0x0850FA18; OBJ 0xC404 e 0x2AB/0x2CA/0x2CB/0x2C6; +0xAA += 1.
- Confiança: resumo.

## 0x080201DC Fight_DispatchOnSubstateAA_L_Substate03 (L:0x03)
- Dado 0x083B6100; OBJ 0x2AB; +0xAA += 1 e mexe em +0xAB.
- Confiança: parcial.

## 0x08020248 Fight_DispatchOnSubstateAA_L_Substate04 (L:0x04)
- Escreve +0xAB e +0xAA.
- Confiança: parcial.

## 0x08020288 Fight_DispatchOnSubstateAA_L_Substate06 (L:0x06)
- Gráfico 0x084CB9C8 para VRAM 0x06010000; paleta 0x0850F718; OBJ 0x2AB; +0xAA += 1.
- Confiança: resumo.

## 0x080202E0 Fight_DispatchOnSubstateAA_L_Substate07 (L:0x07)
- OBJ 0x2AB; timer 0x03000440; mexe em +0xAB e +0xAA.
- Confiança: parcial.

## 0x08020340 Fight_DispatchOnSubstateAA_L_Substate09 (L:0x09)
- Só +0xAA += 1.
- Confiança: resumo.

## 0x0802036C Fight_DispatchOnSubstateAA_L_Substate0B (L:0x0B)
- Gráfico 0x084D178C para VRAM 0x06010800; paleta 0x0850F9D8; OBJ 0xC404; dado 0x083B6100; +0xAA += 1.
- Confiança: resumo.

## 0x08020428 Fight_DispatchOnSubstateAA_L_Substate0C (L:0x0C)
- Dado 0x083B6100; OBJ 0x2A2; timer 0x03000440; +0xAA += 1.
- Confiança: resumo.

## Esboço da máquina de estados (parcial)
- Despachante = fase (letra). `+0xAA` = passo dentro da fase. Cada handler avança +1, ou grava valor fixo.
- H: apresentação com gráfico e OBJ, e uma espera (H:0x29). I, K, L: mistura de gráfico, OBJ e passos vazios.
  J: BG/tilemap (0x04000008). Nas fases H..L, passos vazios (só +0xAA += 1) são comuns.
- Só H tem chamador conhecido (0x080341DE). Qual evento escolhe a fase e o passo fica em aberto.

## Em aberto
- Criar funções no Ghidra nos 42 endereços para decompile limpo (hoje não existem).
- Nomes dos helpers listados acima: não estão no mtbe.csv.
- Passo real da Robattle (ataque, dano, troca, fim): não decifrado. Ver pc/research/R3_battle_state.md.
