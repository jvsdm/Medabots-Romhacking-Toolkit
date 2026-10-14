# SN3: pontas do motor de som (Medabots Metabee EU)

Escopo: fechar as pontas de SN.md, SN2.md e FS10.md (itens 1 a 4 da tarefa). Fonte: Ghidra (só leitura, mais
um rename de dado), bytes da ROM original lidos com python -I (nada modificado, nada extraído), e os docs
do próprio toolkit. mtbe.csv e pc/research/README.md foram consultados só via python -I.

## 1. snd_block_tick 0x08073E24 x snd_channel_set_state3 0x08073E1C: duas funções (confirmado)

- `0x08073E24` snd_block_tick (corpo 08073E24-08073E87). Confere com o CSV e com a nota do SND12 em
  pc/research: é o tick de PCM/tempo do bloco driver+0x690, não o sequenciador. Chama 0x08073DF4 (no início),
  0x08073C3C (snd_pcm_rate_from_note), 0x0807CEC8 (divisão sem sinal), 0x08073D70, 0x08072734 e 0x08073DB8.
  Ponteiro a partir do argumento. **Nome e linha do CSV corretos.**
- `0x08073E1C` snd_channel_set_state3 (corpo 08073E1C-08073E23, 4 instruções):
  `add r0,#0x3E; mov r1,#3; strb r1,[r0]; bx lr`. Grava o valor 3 em `[arg+0x3E]`. É folha, não tem BL.
  Já tinha nome no Ghidra; não renomeei. Chamado por BL em 0x08073956 (snd_channel_render) e referenciado
  pelas tabelas de ponteiro (FS10: 0x080748xx), que o xref do Ghidra não lista.
- Decisão: são duas coisas diferentes, não alias. O CSV de `snd_block_tick` está certo; faltava a linha de
  `snd_channel_set_state3` (adicionada em SN3_simbolos.csv). mtbe.csv não foi editado (regra da tarefa); fica
  para a fusão.

## 2. Ramos finais de snd_update_channel2..4 e else de 0x08070778 e 0x08073010

Confiança: provável (decodificado à mão a partir dos bytes; Ghidra não tem código nesses endereços).

**snd_update_channel2_if_track_flag 0x08074780** (ptr do driver = `[0x03007014]` = 0x03002430):
```
push {r4,lr}; r4 = &[0x03007014]
snd_channel_update_2(driver+0x244)            ; BL 0x08073718
r0 = snd_track_test_flag2()                   ; BL 0x08074540
if r0 != 0: pop {r4,pc}                       ; ramo bne 0x080747AC
else: byte[driver+0xC4] |= 0x5F
      snd_channel_update_2(driver+0x74)       ; BL 0x08073718 de novo, arg = trilha 1
      pop {r4,pc}
```
**snd_update_channel3_if_track_flag 0x080747B8**: igual; chama `snd_channel_update_3(driver+0x2B8)` (BL
0x08073784). Else: `byte[driver+0x138] |= 0x7F`; `snd_channel_update_3(driver+0xE8)`; pop.

**snd_update_channel4_if_track_flag 0x080747F4**: igual; `snd_channel_update_4(driver+0x32C)` (BL 0x080737E8).
Else: `byte[driver+0x1AC] |= 0x5F`; `snd_channel_update_4(driver+0x15C)`; pop.

Atenção: os argumentos do ramo else (driver+0x74, +0xE8, +0x15C) são bases de trilha (k*0x74), não as bases de
canal do ramo principal (+0x244, +0x2B8, +0x32C). Isso não bate com o desenho óbvio. Eu deixo registrado como
está no código, sem interpretar. Os bytes OR-ados (0x5F, 0x7F) também não têm significado conhecido.

**0x08070778 Obj_AdvanceCounterCap47**: o ramo else que o FS10 deixou aberto agora está decodificado:
```
slot = 0x03002C30 + (r0&0xFF)*0x40
if [slot+0x21] <= 0x47: [slot+0x21]++
else:
  if (s16)[slot+2] <= 0x168: [slot+2] = [slot+2] + 6      ; halfword
  else: [slot+2] = 0; [slot+0x21] = 0
retorna 1 sempre (os dois ramos terminam em mov r0,#1)
```
Ou seja: depois do teto 0x47 o slot anda em X (+2) de 6 em 6 até 0x168 e reinicia. Confiança: provável.

**0x08073010 Obj_TickWaitCountdown** (0x08073010-0x0807303F):
```
push {r4,lr}; r4 = r0
if [r4+0x24] == 0: tail 0x08072818(r4); pop; ret
loop:
  if [r4+0x30] != 0: [r4+0x30]--
  if [r4+0x30] != 0: 0x08072818(r4); pop; ret
  0x08072F7C(r4)                         ; ramo do passo
  if [r4+0x24] != 0: volta a loop
  else: 0x08072818(r4); pop; ret
```
**Conflito de nomes, não resolvido:** no mtbe.csv, `0x08072818` = `snd_pitch_slide_tick` e `0x08072F7C` =
`snd_track_cmd_ext` (ambos marcados R4). Aqui eles são chamados com `r0 = r4`, a struct do próprio
`Obj_TickWaitCountdown`. Ou os nomes do R4 estão errados, ou o 0x08073010 não é objeto (está colado no bloco de
som, logo antes de 0x08073044). Não renomeei nada; a decisão fica para quem revisa.

## 3. DAT_08071058 e a tabela de 12 bytes em 0x08071BD0

- **DAT_08071058 não é contador de som.** É um literal (bytes `7C 13 00 03` = 0x0300137C), ou seja, aponta para
  o gMainSubStateCounter (GS, nome no mtbe.csv 0x0300137C). `Scene_StartSong4AndSlots` 0x08071030 (decompile):
  `snd_song_request(4); slot_start_v2(1,0,0); slot_start_v2(2,0xB,0); *GS += 1`. Confirmado. O "contador
  desconhecido" do FS10 está resolvido.
- **Tabela 0x08071BD0**: 12 bytes `14 01 0B 08 30 2C 03 00 0E 05 00 00`, lidos da ROM. O uso (decompile de
  `Obj_RunSpriteSequenceTable12` 0x08071B64): `lib_memcpy` de 12 bytes para a pilha; compara `[slot+0x1A]`
  (halfword) com `tab[[slot+0x21]]`; se igual, `[slot+2]++` e `[slot+0x21]++`. Quando o índice chega a 8 e
  `[slot+0x22]` != -1, cria o slot filho (0x0C se k=0xB, senão k+1) via `Map_InitializeObjectRenderSlot` e
  `slot_start_v2(4,...)`. Com índice >= 12, zera `[slot+0x22]`; se k=0xB, mexe em bits 0x800 do OAM. Retorna 1
  enquanto roda, 0 ao fim.
- Os bytes 4..11 coincidem com literais usados pela própria função (0x03002C30 = gObjectSlots; 0x0000050E como 3º
  argumento). Ou seja, a tabela está dentro do pool de literais. Os valores (20,1,11,8,48,44,3,0,14,5,0,0) não têm
  significado confirmado; são os valores que `+0x1A` deve bater.
- Ghidra: renomeado o dado `0x08071BD0` para `ObjSeq_CompareTable12` (provável).

## 4. BL que caem em dado (falsos positivos de varredura): confirmado que são dado

- **Origem 0x081B43FC** (BL pair `F6F4 FAF9`, alvo 0x080A89F2): cai numa região de valores pequenos empacotados
  (0x081B43E0..0x081B4406: `FDF9 0201 00FF 00FF FF00 F7FA ... 0801 F2FD F6F4 FAF9 0200 0000 00FF 0001`). Sem
  função no Ghidra e sem xref. É dado.
- **Alvo 0x080A89F2**: começa com `B494 BFF4`. `BFxx` é a codificação de IT, que não existe no ARM7TDMI (ARMv4T).
  Seguido de `D6B4 F04C 0000 0000 24F8 4168 4168 56BC 56BC`, que é tabela/literal. É dado.
- **Origem 0x080FC3F6** (BL pair `F7F0 FAFA`, alvo 0x080EC9EE): está em 0x080FC3D8..0x080FC400, região de bytes
  empacotados (`0E0E 0006 F1F6 FBF6 0503 ... ECEE EAE7 F7F0 FAFA FBFA FB00 00FB 0E06`). Depois do BL vêm `FBFA` e
  `FB00` como sufixos de BL soltos (11111 sem prefixo). É dado.
- **Alvo 0x080EC9EE**: começa com `B5EB` (push válido), mas o próximo é `CFC9` (LDMIA r7!, {r0,r3,r6,r7}, base na
  lista com writeback, improvável em compilador), e depois `E5DA E0EB 03F9 0002 2614 1F05 2F33 5935` (dado). Veredito:
  dado, provável (não confirmado 100%).

Conclusão: os dois BL são falsos positivos da varredura. Origem e alvo são dado. Não há função a documentar.

## Pontos em aberto

- Conflito de nomes 0x08072818 / 0x08072F7C (seção 2) entre o R4 e o chamador de Obj_TickWaitCountdown.
- Argumentos do ramo else de snd_update_channel2..4 (bases de trilha, não de canal): não explicados.
- Significado dos 12 bytes de 0x08071BD0 (seção 3).
- snd_track_test_flag2 0x08074540 continua "chute" no mtbe.csv (não verificado aqui).
- Xrefs do Ghidra não listam os chamadores por tabela de ponteiro (0x083ABDxx etc.); só os BL diretos.
