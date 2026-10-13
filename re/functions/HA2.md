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

Nomes: Ghidra não tem função nesses endereços (só os stubs), então nada foi renomeado no projeto. Os nomes do CSV
são propostos e valem só quando marcados provável.

## 0x0801D6A4 Fight_DispatchOnSubstateAA_E (idx 0x01)
- Assinatura: `void f(void)`
- Passo: carga de gráfico da fase (tiles) antes da animação de entrada; `+0xDF` seleciona a fase (cmp 0x1B/0x1C/0x1D).
- O que faz: para fase 0x1C copia gráfico de ROM `0x084CAE20` para VRAM `0x06014800` (rotina `0x080009BC`) e carrega ponteiro `0x0850F698`; cauda em `0x0801D6EA` não decodificada.
- Subestado: não escreve +0xAA no trecho lido. Próximo: desconhecido (parcial).
- Confiança: provável (cargas); chute (transição).

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
- Passo: atualização de posição/animação de objeto (sem nome claro).
- O que faz: chama `0x080352EC`, `0x080356CC`, `0x080432A0` (duas vezes).
- Subestado: +1 (-> E05).
- Confiança: chute (função), confirmado (+1).

## 0x0801D9DC Fight_DispatchOnSubstateAA_E (idx 0x05)
- Passo: ajuste de atributos de objeto (sprite).
- O que faz: escreve halfwords em `0x03002C30` (+2, +4), bytes 0x1664/0x1665 em `[r0]`, `[r0,#0x30]`; chama `0x080432A0`.
- Subestado: +1 (-> E06).
- Confiança: provável (escritas), confirmado (+1).

## 0x0801DAEC Fight_DispatchOnSubstateAA_E (idx 0x06)
- Passo: montagem da tela de luta (dados de parte/HUD).
- O que faz: chama `0x0803526C`, `0x080356CC`, `0x080432A0`, `0x08074DE0`, `0x08015838`, `0x0804305C`, `0x080356B4`; grava `0x03000440` e `0x03002C30`.
- Subestado: +1 (-> E07).
- Confiança: chute (conteúdo), confirmado (+1).

## 0x0801DC74 Fight_DispatchOnSubstateAA_E (idx 0x07)
- Passo: efeito/som de início de ação (chute).
- O que faz: chama `0x08040CD0` e `0x08040B90` com r1=0x21; lê `[r0,#0x70]` de objeto.
- Subestado: +1 (-> E08).
- Confiança: chute, confirmado (+1).

## 0x0801DD30 Fight_DispatchOnSubstateAA_E (idx 0x08)
- Passo: disparo de efeito (id 0x30) no meio da luta.
- O que faz: `0x08074DE0(0x30)`; toca `0x03002C30`.
- Subestado: +1 (-> E09).
- Confiança: provável (id), confirmado (+1).

## 0x0801DD74 Fight_DispatchOnSubstateAA_E (idx 0x09)
- Passo: chamada de efeito/animação (chute).
- O que faz: chama `0x08040BD8`, `0x0803F63C`, `0x08040CD0(r1=1)`.
- Subestado: +1 (-> E0A, genérico 0x080305E1).
- Confiança: chute, confirmado (+1).

## 0x0801DDDC Fight_DispatchOnSubstateAA_E (idx 0x0B)
- Passo: espera/troca de bot (sem avanço).
- O que faz: chama `0x08035928(+0x87, +0xDE)` e `0x08035944(+0xDE-1 ...)`; põe `+0xB5 = 1`.
- Subestado: hold (não escreve +0xAA). Quem sai é a rotina chamada (não lida).
- Confiança: provável (hold), chute (quem avança).

## 0x0801DE44 Fight_DispatchOnSubstateAA_F (idx 0x00)
- Passo: cópia de paleta/tile do bot (chute).
- O que faz: chama `0x0803F63C`; lê `0x03000590` (bloco do bot, +0x100*bot) e `0x030004DE`, `0x083B7048`.
- Subestado: não escreve +0xAA no trecho lido. Próximo: desconhecido.
- Confiança: chute.

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
- Passo: cópia de tilemap de fundo.
- O que faz: `0x0807CCC0`; grava VRAM `0x06002800`, `0x0600C800`, `0x06010800`... (blocos de mapa 0x0600xx00).
- Subestado: não escreve +0xAA no trecho lido. Próximo: desconhecido (provável +1 por padrão, não confirmado).
- Confiança: provável (VRAM), chute (transição).

## 0x0801E09C Fight_DispatchOnSubstateAA_G (idx 0x03)
- Passo: montagem do cenário de batalha (fundo/efeito).
- O que faz: maior handler do grupo (157 instruções); chama `0x08043CA8`, `0x080009BC`, `0x0803EF8C`, `0x08043B7C`, `0x0807CCC0`, `0x08043DF8`, `0x0801BC5C`, `0x0801BCA0`, `0x08035C3C`; VRAM `0x06017400`; lê `0x03000590`, `0x030017F0`.
- Subestado: +1 (-> G04).
- Confiança: provável (cenário), confirmado (+1).

## 0x0801E200 Fight_DispatchOnSubstateAA_G (idx 0x04)
- Passo: configuração de BG/objeto (chute).
- O que faz: `0x0807CCC0`, `0x08035C70`, `0x08035DE8`, `0x080361A8`; grava `0x03002C30`, `0x03000440`.
- Subestado: +1 (-> G05).
- Confiança: chute, confirmado (+1).

## 0x0801E248 Fight_DispatchOnSubstateAA_G (idx 0x05)
- Passo: animação de entrada (chute).
- O que faz: `0x08035DE8`, `0x080361C4`, `0x08036218`, `0x0803EF8C`; `0x04000008`.
- Subestado: +1 (-> G06).
- Confiança: chute, confirmado (+1).

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
- Passo: ramo de decisão (escolhe o caminho da luta).
- O que faz: chama `0x0803628C` e `0x08036270`; escreve +0xAA com valores diferentes por caminho: `0x28`, `0x0A`, `0x30`, `0x38`.
- Subestado: ramificação para 0x28 / 0x0A / 0x30 / 0x38 (confirmado nas escritas; a condição não foi decodificada).
- Confiança: confirmado (valores), chute (condição).

## 0x0801E43C Fight_DispatchOnSubstateAA_H (idx 0x02)
- Passo: ramo curto.
- O que faz: caminho A: +0xAA = 6; caminho B: +1 (-> H03).
- Subestado: ramificação 0x06 ou +1.
- Confiança: confirmado (código lido), condição chute.

## 0x0801E474 Fight_DispatchOnSubstateAA_H (idx 0x03)
- Passo: carga de gráfico (chute).
- O que faz: `0x080009BC`, `0x0807CCC0`; VRAM `0x06010000`; lê `0x0850F878`, `0x050003C0`, `0x03002C30`.
- Subestado: não escreve +0xAA no trecho lido.
- Confiança: chute.

## 0x0801E54C Fight_DispatchOnSubstateAA_H (idx 0x04)
- Passo: pequena atualização (chute).
- O que faz: `0x080353CC`; grava `0x03000440`.
- Subestado: +1 (-> H05, genérico).
- Confiança: chute, confirmado (+1).

## 0x0801E578 Fight_DispatchOnSubstateAA_H (idx 0x06)
- Passo: fim de contagem (ramo).
- O que faz: sem chamadas; lê `0x03000590`; caminhos com +1 e um com +0xAA = 0x0A.
- Subestado: ramificação +1 ou 0x0A.
- Confiança: confirmado (código lido), condição chute.

## 0x0801E5DC Fight_DispatchOnSubstateAA_H (idx 0x07)
- Passo: animação/efeito (chute).
- O que faz: `0x08036B84`, `0x080009BC`, `0x0807CCC0`, `0x08074DE0`; VRAM `0x06010000`; lê `0x084CDD68`, `0x0850F8B8`, `0x03002C30`.
- Subestado: não escreve +0xAA no trecho lido.
- Confiança: chute.

## 0x0801E830 Fight_DispatchOnSubstateAA_H (idx 0x08)
- Passo: avanço de ação (chute).
- O que faz: chama `0x080366DC`.
- Subestado: +1 (-> H09, genérico 0x080305E1).
- Confiança: chute, confirmado (+1).

## 0x0801E864 Fight_DispatchOnSubstateAA_H (idx 0x0A)
- Passo: efeito com parâmetro 0x0A/0xAA (chute).
- O que faz: `0x080009BC`, `0x0807CCC0`, `0x08036784(r0=0x0A, r1=0xAA)`; lê `0x084CDF48`, `0x0850F8D8`.
- Subestado: +1 (-> H0B).
- Confiança: chute, confirmado (+1).

## 0x0801E8DC Fight_DispatchOnSubstateAA_H (idx 0x0B)
- Passo: chamada de efeito (chute).
- O que faz: `0x0807CCC0`, `0x0803684C`; lê `0x0850F8D8`.
- Subestado: +1 (-> H0C, genérico).
- Confiança: chute, confirmado (+1).

## 0x0801E918 Fight_DispatchOnSubstateAA_H (idx 0x0D)
- Passo: fase de animação com som/efeito (id 0x5B).
- O que faz: `0x080009BC`, `0x0807CCC0`, `0x0801A28C`, `0x080368C4`, `0x08074DE0(0x5B)`; VRAM `0x06010000`.
- Subestado: +1 (-> H0E).
- Confiança: provável (id 0x5B), confirmado (+1).

## 0x0801E9B0 Fight_DispatchOnSubstateAA_H (idx 0x0E)
- Passo: fim de ação (efeito 0x3C).
- O que faz: `0x08036B1C`; `0x0300052E`; põe 0x3C no argumento.
- Subestado: +1 (-> H0F, genérico). Também usado como handler de AP, AQ, P, R (coluna extra da lista).
- Confiança: chute, confirmado (+1).

## 0x0801E9E0 Fight_DispatchOnSubstateAA_H (idx 0x10)
- Passo: efeito 0x2F (chute).
- O que faz: `0x08074DE0(0x2F)`.
- Subestado: +1 (-> H11).
- Confiança: chute, confirmado (+1).

## 0x0801E9FC Fight_DispatchOnSubstateAA_H (idx 0x11)
- Passo: espera de objetos (chute).
- O que faz: `0x08036BB8`, `0x08036370`, `0x080364A8`; lê `0x030018C8`.
- Subestado: não escreve +0xAA no trecho lido.
- Confiança: chute.

## 0x0801EA5C Fight_DispatchOnSubstateAA_H (idx 0x13)
- Passo: avanço após chamada de rotina (chute).
- O que faz: `0x08035194`.
- Subestado: +1 (-> H14).
- Confiança: chute, confirmado (+1).

## 0x0801EA94 Fight_DispatchOnSubstateAA_H (idx 0x14)
- Passo: avanço (chute).
- O que faz: `0x08036BF4`.
- Subestado: +1 (-> H15).
- Confiança: chute, confirmado (+1).

## 0x0801EAC4 Fight_DispatchOnSubstateAA_H (idx 0x15)
- Passo: espera (flag).
- O que faz: só `+0xB5 = 1` (5 instruções).
- Subestado: hold.
- Confiança: confirmado (código lido).

## 0x0801EAD4 Fight_DispatchOnSubstateAA_H (idx 0x16)
- Passo: carga de gráfico (chute).
- O que faz: `0x080009BC`, `0x0807CCC0`; VRAM `0x06010800`; lê `0x084CE3E8`, `0x0850F8F8`.
- Subestado: não escreve +0xAA no trecho lido.
- Confiança: chute.

## 0x0801EB5C Fight_DispatchOnSubstateAA_H (idx 0x17)
- Passo: efeito de objeto (chute).
- O que faz: `0x08040BD8`, `0x08040CD0`, `0x08035F20`; grava `0x03000440`, `0x03002C30`.
- Subestado: +1 (-> H18).
- Confiança: chute, confirmado (+1).

## 0x0801EBB8 Fight_DispatchOnSubstateAA_H (idx 0x18)
- Passo: avanço simples.
- O que faz: sem chamadas; grava `0x03000440`.
- Subestado: +1 (-> H19, genérico).
- Confiança: confirmado (+1).

## 0x0801EBD4 Fight_DispatchOnSubstateAA_AO (idx 0x0E)
- Passo: avanço (chute). Dispatcher AO sem stub identificado.
- O que faz: `0x08035F20`; grava `0x03000440`.
- Subestado: +1 (-> AO 0x0F).
- Confiança: chute, confirmado (+1). Também handler de AP (coluna extra).

## 0x0801EBF8 Fight_DispatchOnSubstateAA_H (idx 0x1A)
- Passo: espera com chamada (chute).
- O que faz: `0x08035234`.
- Subestado: +1 (-> H1B). Dois pontos de +1 no código.
- Confiança: chute, confirmado (+1).

## 0x0801EC3C Fight_DispatchOnSubstateAA_H (idx 0x1D)
- Passo: ramo de decisão.
- O que faz: caminho A: +0xAA = 0x20 (-> H20, entrada fora da lista); caminho B: +1.
- Subestado: ramificação 0x20 ou +1.
- Confiança: confirmado (valores), condição chute.

## Máquina de estados (esboço, despachante H, bloco 0x03001800)

    H00 -> H01 (carga de sprites)
    H01 -> 0x0A | 0x28 | 0x30 | 0x38 (decisão; 0x28/0x30/0x38 caem FORA da lista, entradas 0x28, 0x30, 0x38 da tabela)
    H02 -> H03 | 0x06 (decisão)
    H04 -> H05 (genérico) ; H06 -> +1 | 0x0A (decisão) ; H08 -> H09 (genérico)
    H0A -> H0B -> H0C (genérico) ; H0D -> H0E -> H0F (genérico) ; H10 -> H11 ; H13 -> H14 -> H15 (hold)
    H17 -> H18 -> H19 (genérico) ; H1A -> H1B ; H1D -> H20 | H1E
    Holds (sem +1): H15, H16?, H03?, H07?, H11?. Os '?' não foram confirmados no trecho lido.

Dispatchers E e F e G (mesmo padrão): E01 (parcial) -> E02 -> ... -> E09 -> E0A (genérico); E0B hold.
F00 (sem +1 lido) ; F02 hold. G00 -> G01 -> ... -> G06 (hold).

## Pendências

- Entradas H 0x1E..0x4F (fora da lista): a tabela `0x083AD790` continua com ponteiros de código válidos
  (ex.: 0x20 -> 0x0801ECC1, 0x24 -> 0x0801EED5, 0x28 -> 0x0801EEE5, 0x30 -> 0x0801F019, 0x38 -> 0x0801F0F1,
  0x3E -> 0x0801F215, 0x4A -> 0x0801ED51). A lista de HA2 parou em 0x1D. Recomenda-se estender HA2_lista.txt.
- Handlers sem +1 no trecho lido (E01, F00, G02, H03, H07, H11, H16): cauda ou rotina chamada não decodificada.
- Cauda de E01 (0x0801D6EA) não decodificada.
- Nenhum nome foi aplicado no Ghidra: os handlers não são funções lá.
