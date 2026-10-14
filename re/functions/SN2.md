# SN2: motor de som, continuacao (Medabots Metabee EU)

Continua `SN.md`. Fonte: decompile do Ghidra (confirmado no codigo salvo nota). Nada de audio extraido.

## 1. Efeitos de comando (fechados no decompile)

| Byte | Handler | Operandos | Efeito confirmado |
|---|---|---|---|
| E9 xx | snd_cmd_E9 | 1 | track+0x6B = xx, limitado a 0x30 (mesmo que SN.md) |
| F7 xx | snd_cmd_F7 | 1 | track+0x54 = xx; se 0 -> 8; flags track+0x50 \|= 0x08 |
| F8 xx | snd_cmd_F8 | 1 | track+0x55 = xx (so grava) |
| FB a b c d | snd_cmd_envelope | 4 | track+0x5B..0x5E = a,b,c,d; flag track+0x62 = 1; se sub-modo (+0x64) < 3 chama `Bar_ResetPhase1` (nome do Ghidra, provavelmente errado: e reset de fase do canal) |
| FC | snd_cmd_FC | 0 | track+0x62 = 1; mesmo ramo de reset de fase de FB |
| FD | snd_cmd_FD | 0 | track+0x62 = 0; mesmo ramo de reset de fase |
| FE xx | snd_cmd_mode | 1 | track+0x63 = xx (modo da nota). Ver secao 2 |

Nota: `+0x64` (sub-modo de nota) e `+0x63` (modo FE) sao campos distintos. Confirmado no decompile de snd_cmd_mode.

## 2. Modo FE (1 e 2) em snd_cmd_note

- FE 0: nota normal, le duracao no byte seguinte (SN.md).
- FE 1: a nota consome um operando extra (grava no byte alto de +0x46). Sem duracao lida aqui (SN.md, divergencia 4).
- FE 2: le a duracao e grava em `+0x28`; sub-modo >= 3 usa a tabela de ponteiros `[DAT_08072a98]+0x728` (SN.md).
- Pendente: semantica do operando extra do FE 1 no sintetizador.

## 3. Registradores PSG (snd_channel_update_1..4) e envelope

Enderecos lidos dos literais (ROM, python -I):

| Canal | Funcao de update | Registradores (literais) | Nota |
|---|---|---|---|
| CH1 | snd_channel_update_1 0x080736A0 | 0x04000060 (SOUND1CNT_L, via FUN_080732C8); reg_value em 0x04000062 (SOUND1CNT_H) + byte 0x04000065; freq_write_a em 0x04000064 (SOUND1CNT_X) | ramo de nota: note_write(0x62, 0x64) |
| CH2 | snd_channel_update_2 0x08073718 | reg_value 0x04000068 (SOUND2CNT_L) + byte 0x0400006D; freq_write_a 0x0400006C (SOUND2CNT_H) | ramo de nota: note_write(0x68, 0x6C) |
| CH3 (onda) | snd_channel_update_3 0x08073784 | Snd_UpdateChannelFreqEnv 0x04000072 (SOUND3CNT_H, volume); freq_write_b 0x04000074 (SOUND3CNT_X) | ramo de nota: note_write(0x72, 0x74) |
| CH4 (ruido) | snd_channel_update_4 0x080737E8 | FUN_080733BC 0x04000078 (SOUND4CNT_L, envelope); FUN_080734F4 0x0400007C (SOUND4CNT_H) | sem ramo de nota; sempre ctrl_write |

Helpers confirmados:
- Condicao de escrita COMPLETA (canais 1-3): `track+8 == 0` OU `track+0x6B == 0` OU `track+0x66 != 0` OU `track+0x50 & 0x10`.
  Senao so `snd_channel_note_write` (nota vinda do buffer em anel, `snd_ring_read`; -1 = soltar/silencio; set bit 0x8000 de gatilho).
- `snd_channel_freq_write_a`: se `track+0x50 & 0x10` -> gatilho 0x8000 e limpa 0x10. Se `track+0x50 & 1`: valor = clamp(`+0x46` + `+0x55` + (`+0x38`>>4), 0, 0x7FF) (11 bits), limpa bit 1. Logo **F8 (`+0x55`) e um deslocamento de altura somado a frequencia** (nao so "GUESS").
- `snd_channel_note_write`: freq = `snd_freq_lookup(nota)` + `+0x55`, clamp 0..0x7FF. Modo `+0x64 == 2` usa tabela da ROM 0x083ABBE2 em `+0x40*3 >> 12` e escreve byte alto de 0x04000..; demais usam `+0x67` (voz) como byte e `+0x40*3`.
- `snd_channel_ctrl_write` (0x08073604): se `track+0x50 & 0x40`: nivel = `track+0x70` (se 0) senao `track+0x6F` (EA) ou, no modo 2, byte da tabela `[driver+0x728]+4 + +0x48`; escreve em **0x04000081** (byte alto de SOUNDCNT_L, painel/mascara do PSG) com mascara `table(0x083ABBF2)[+0x64] & old | table(0x083ABBF6)[nivel + +0x64*4]`; limpa bit 0x40. Se `[driver+0x72C] & 4` setado, nivel = 3 (fixo).
- Portanto: **EA (`+0x6F`) = nivel de volume/envelope do canal** usado no registrador de controle (0x81), e `+0x70` (0 musica/sfx) so troca a fonte do nivel.

Envelope FB/FC/FD: grava `+0x5B..+0x5E` e `+0x62` (1 = envelope ativo / 0 desligado). Os 4 bytes de FB sao os parametros do envelope (ataque/decaimento/sustento/soltura, nao confirmado pelo nome). Nao ha `snd_envelope_tick` ainda lido (pendente).

## 4. Hardware de som (snd_hw_init 0x08073A88)

- `*0x04000084 = 0x80` (SOUNDCNT_X: liga o controlador de som, confirmado).
- `*0x04000082 = 0x400E` (SOUNDCNT_H: volume PSG/DMA; bits 1-3 setados).
- `*0x04000070 = 0x00C0` (SOUND3CNT_L: liga canal 3 / banco de onda; confirmado no literal).
- `snd_hw_channel_reset(0)` (0x08073A48) e `snd_cmd_push(7,7)` chamados; efeito de snd_cmd_push nao lido.
- Escritas de canal ficam em `snd_channel_*`; DMA/timer: ver secao 6.

## 5. Fim de trilha: track+0x6D

- `snd_track_reset_state` 0x0807490C zera `+0x6D` (prioridade atual), `+0x14` (cursor), `+0x54`, `+0x66`, `+0x6B`, `+0x58`, `+0x38`, `+0x44`; `+0x48 = 0xFF`; `+0x4E = -1` (sem sfx); `+0x50 |= 0x0A`. **Logo quem zera track+0x6D e esta funcao** (ver xrefs na secao 7).
- Snd_TrackStepInterpreter 0x08072D68 e a unica rotina lida que le `+0x14` como cursor.

## 6. Tabela completa de opcodes do interpretador (0x08072D68; Ghidra + disassembly capstone da ROM)

Despacho (confirmado no disassembly): op < 0xE0 e tratado em 0x08072F04 com `op & 0xF0`; op >= 0xE0 usa a tabela de
32 ponteiros em 0x08072D94 (ponteiro lido do literal 0x08072D90).

| Byte(s) | Handler (endereco) | Operandos a seguir | Bytes totais | Efeito |
|---|---|---|---|---|
| 0x00-0x7F | snd_cmd_note 0x08072910 | 1 duracao (FE 0); 1 extra (FE 1, sem duracao); FE 2: duracao em +0x28 | 2 (FE 0) | nota = op; key = op + transp(+0x6A) - 0x24, clamp 0..0x47; dur = (FA<<8)+b+1 |
| 0x80-0x8F | snd_cmd_rest 0x08072AC0 | 1 | 2 | pausa b+1 ticks (`op & 0xF0 == 0x80`, confirmado) |
| 0x90-0x9F | NOP | 0 | 1 | sem efeito (confirmado: `op & 0xF0 == 0x90` volta direto) |
| 0xA0-0xAF | NOP | 0 | 1 | sem efeito |
| 0xB0-0xBF | NOP | 0 | 1 | sem efeito |
| 0xC0-0xCF | NOP | 0 | 1 | sem efeito |
| 0xD0-0xDF | snd_cmd_volume 0x08072AFC | 0 | 1 | volume = n (`op & 0xF`) em +0x65/+0x66; ramo extra se FE=1 e sub-modo=3 |
| 0xE0 | snd_cmd_rest | 1 | 2 | igual 0x80 |
| 0xE1 | snd_cmd_E1_set_track_2c | 1 | 2 | +0x2C = xx |
| 0xE2 | snd_cmd_E2_set_track_69 | 1 | 2 | +0x69 = xx; flags \|= 0x51 |
| 0xE3 | snd_cmd_voice 0x08072B34 | 1 | 2 | +0x67 = xx (amostra em PCM) |
| 0xE4-0xE7 | snd_set_voice (0, 0x40, 0x80, 0xC0) | 0 | 1 | grava voz/flag fixo em +0x67 e marca flag |
| 0xE8 | snd_cmd_transpose 0x08072B64 | 1 (com sinal) | 2 | +0x6A = xx (transposicao) |
| 0xE9 | snd_cmd_E9 | 1 | 2 | +0x6B = min(xx, 0x30) |
| 0xEA | snd_cmd_EA_set_track_6f | 1 | 2 | +0x6F = xx (nivel de envelope/volume, usado em ctrl_write); flag 0x40 |
| 0xEB | snd_cmd_EB | 0 | 1 | +0x4C += 1 (contador; uso nao confirmado) |
| 0xEC-0xEF | snd_cmd_note (cai no ramo de nota) | 1 duracao | 2 | **nota** (op como nota), nao comando |
| 0xF0 / F1 | loop1 start / end | 1 / 0 | 2 / 1 | contador +0x52; inicio +0x18 |
| 0xF2 / F3 | loop2 start / end | 1 / 0 | 2 / 1 | contador +0x53; inicio +0x1C |
| 0xF4 | snd_cmd_jump 0x08072C10 | 4 (a32) | 5 | cursor = a |
| 0xF5 | snd_cmd_call 0x08072C20 | 4 (a32) | 5 | +0x20 = retorno; cursor = a |
| 0xF6 | snd_cmd_return 0x08072C34 | 0 | 1 | cursor = +0x20 |
| 0xF7 | snd_cmd_F7 | 1 | 2 | +0x54 = xx (0 -> 8); flag +0x50 \|= 0x08 |
| 0xF8 | snd_cmd_F8 | 1 | 2 | +0x55 = xx: deslocamento de altura somado a frequencia (confirmado) |
| 0xF9 | snd_cmd_lfo 0x08072C74 | 2 | 3 | +0x56 = aa; +0x57 = bb&F; +0x58 = bb>>4 (vibrato, snd_calc_vibrato) |
| 0xFA | snd_cmd_dur_hi 0x08072CA0 | 1 | 2 | +0x6E = byte alto da proxima duracao |
| 0xFB | snd_cmd_envelope 0x08072CB0 | 4 | 5 | +0x5B..0x5E = a,b,c,d; +0x62 = 1; reset de fase se sub-modo < 3 |
| 0xFC | snd_cmd_FC 0x08072D04 | 0 | 1 | +0x62 = 1; reset de fase se sub-modo < 3 |
| 0xFD | snd_cmd_FD 0x08072D28 | 0 | 1 | +0x62 = 0; mesmo ramo |
| 0xFE | snd_cmd_mode 0x08072D4C | 1 | 2 | +0x63 = xx (modo da nota: 0; 1 = operando extra; 2 = duracao em +0x28) |
| 0xFF | snd_cmd_end 0x08072D5C | 0 | 1 | snd_track_stop_callback: se cursor != 0, snd_track_reset_state e chama callback +0x0C |

Nota sobre FB/FC/FD: o Ghidra chama `Bar_ResetPhase1` (nome de outro modulo, provavelmente errado). Efeito real nao
decodificado: so age quando sub-modo +0x64 < 3. Pendente.

## 7. Trilha -> canal/grupo (confirmado pelas chamadas em snd_update_frame 0x08073094)

Trilha k = driver + k*0x74 (0x74 = 116 B; k = 0..12; 0x570 = trilha 12). Mapeamento:

| Trilha | Offset | Papel |
|---|---|---|
| 0 | +0x000 | ch1 PSG (fonte do buffer em anel, snd_ring_write) |
| 1 | +0x074 | ch2 PSG (anel) |
| 2 | +0x0E8 | ch3 onda (anel) |
| 3 | +0x15C | ch4 ruido (padrao) |
| 4 / 5 / 6 | +0x1D0 / +0x244 / +0x2B8 | alternativas de ch1 / ch2 / ch3 (flag do driver +0x1E4 / +0x254 / +0x2CC != 0) |
| 7 | +0x32C | alternativa de ch4 (flag do driver +0x340 != 0) |
| 8 / 10 | +0x3A0 / +0x488 | grupo A de render (snd_render_group_a); 10 e alternativa (flag +0x49C) |
| 9 / 11 | +0x414 / +0x4FC | grupo B de render (snd_render_group_b); 11 e alternativa |
| 12 | +0x570 | canal 4 especial (ramo de ch4 quando as condicoes de +0x584/+0x14/+0xFC do driver nao batem); iniciado com ponteiro de +0x428 com mascara 0x004 |

Registradores PSG reais (literais confirmados): ch1 0x04000060-0x65, ch2 0x04000068-0x6D, ch3 0x04000070-0x74,
ch4 0x04000078-0x7C; mestre/panning 0x04000080-0x81.
Os grupos de render (trilhas 8-11) sao o caminho PCM (snd_channel_render); detalhe nao decodificado.

## 8. Timer da taxa PCM e DMA

- Confirmado: snd_hw_init 0x08073A88 grava 0x04000084 = 0x80 (SOUNDCNT_X), 0x04000082 = 0x400E (SOUNDCNT_H),
  0x04000070 = 0x00C0 (SOUND3CNT_L). Nao escreve timer nem DMA.
- Confirmado no codigo (funcao em 0x08073AB8, sem nome no Ghidra; o Ghidra nao a separou do literal pool): grava em
  0x040000B0 + 12*(r0-1) (bloco DMA: SAD +0, DAD +4 vindo da tabela 0x08486E54, controle +8). Nome sugerido
  snd_dma_channel_setup (provavel). Chamadores nao lidos. Literal 0x040000B0 tambem aparece em 0x0800127A, 0x08043AE2,
  0x08044F8C, 0x080450EC (nao confirmados como som).
- Timer: literal 0x04000100 (TM0CNT_L) so em 0x0807C92C (sem funcao no Ghidra). Nao confirmado.
- Taxa PCM: snd_pcm_rate_set 0x08073C18 = snd_pitch_scale((taxa * ushort[hdr+4]) >> 12, byte[hdr+6]), limite 0x78000.
  Confirmado: **byte +6 do cabecalho da amostra e a nota-raiz** (SN.md dizia "flag 0"; corrigir).
- Pendente: quem liga timer e DMA em tempo real e o valor de controle (0x040000C6 e vizinhos).

## 9. Tabela de sfx (mecanismo; sem extracao)

- snd_sfx_start 0x08074D50: ponteiro = [driver+0x68C][id*4], sem checagem de limite. Cabecalho igual ao da musica
  (mascara u16, prioridade, ponteiros de trilha). Para cada bit: se track+0x6D <= prio, chama snd_track_init,
  grava prio em +0x6D, +0x4E = id, +0x70 = 0.
- Quantos ids: nao contado (pendente). Para contar: ler a tabela da ROM ate o primeiro ponteiro invalido.

## 10. Divergencias do extrator (pc/extract/extract_music.py, so leitura) CONFIRMADAS

Trecho lido: linhas 47-48 (ONE_BYTE/NO_BYTE) e 96-170 (flatten_track).

1. **0x90/A0/B0/C0**: CONFIRMADO ERRO. O extrator cai em `elif b in ONE_BYTE` e faz pc += 2; no Ghidra sao NOP de
   1 byte. Desalinha tudo depois. Correcao: tratar como pc += 1 sem evento.
2. **0x81-0x8F**: CONFIRMADO ERRO. Nenhum ramo casa e o extrator aborta a trilha ("byte sem tratamento").
   Correcao: `elif 0x80 <= b <= 0x8F` (pausa b+1, pc += 2).
3. **0xEC-0xEF**: CONFIRMADO ERRO DE EFEITO, tamanho igual (2 bytes). Extrator: sem evento. Ghidra: **nota** com
   duracao. Correcao: tratar como nota. Confirmar por replay antes de mudar.
4. **FE 01**: CONFIRMADO que FE 1 faz a nota consumir 2 bytes (sem duracao). O extrator ignora o modo; so desalinha se
   alguma musica usar FE 1. NAO verificado (pendente: varredura).
5. **E4-E7**: tamanho 1 ok, efeito nao modelado. Ignorar.
6. **E2**: tamanho ok, efeito +0x69 nao modelado. Ignorar.
7. **0xD0-0xDF**: ok para FE 0.
8. **FA**: ok.
9. **F9 (3 bytes), FB (5), FC/FD (1), FF (1), E0**: conferem.

Unica discrepancia de tamanho real: item 1 (0x90-0xC0 com 1 byte, nao 2).

## Pendente

- Timer PCM e DMA em tempo real (secao 8).
- Quem chama snd_track_stop_callback no fim de cada trilha (FF) e quem zera +0x6D sem sfx: unico chamador de
  snd_track_reset_state achado: 0x08074968, dentro de snd_track_stop_callback 0x0807495C.
- Semantica do operando de FE 1 e FE 2 no sintetizador; FB (envelope) e snd_envelope_tick nao lidos.
- Grupos PCM (trilhas 8-11) e snd_channel_render: nao decodificados.
- Bar_ResetPhase1 (chamado por FB/FC/FD): nome suspeito; efeito nao lido.
- Tabela de sfx: numero de ids e ponteiros nao contados.
- FE 1 em alguma musica: nao verificado.
