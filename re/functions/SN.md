# SN: motor de som (Medabots Metabee EU)

Escopo: motor de som proprio da Medabots (nao e m4a: magic 0x68736D53 ausente). Fonte: decompile no Ghidra, leitura
da ROM original (python -I) e pc/extract/extract_music.py (so leitura). Nenhum audio extraido. Nomes novos no
Ghidra (confirmados): snd_cmd_E1_set_track_2c, snd_cmd_E2_set_track_69, snd_cmd_EA_set_track_6f. Simbolos em
SN_simbolos.csv.

## 1. Driver e estado global

- Ponteiro do driver: `[0x03007014]` = `0x03002430` (confirmado; usado por todas as rotinas snd_*).
- Trilhas: 13 estruturas de 0x74 bytes a partir de `driver+k*0x74` (k = 0..12). A trilha 12 (`driver+0x570`) e
  especial: iniciada com o ponteiro de `[driver+0x428]` quando a mascara do cabecalho tem bit 0x004.
- Lista circular de trilhas: cabeca em `[driver+0x5E4]`, proximo ponteiro em `track+4`.

| Offset | Tipo | Uso (confirmado no codigo, salvo nota) |
|---|---|---|
| +0x428 | ptr | dado/fluxo da trilha 12 (especial) |
| +0x688 | ptr | tabela de musicas (1 ponteiro de 4 B por id) |
| +0x68C | ptr | tabela de efeitos sonoros (sfx), mesmo formato do cabecalho de musica |
| +0x72C | u8 | flags: bit0 = escreve SOUNDCNT_L; bit1 = "pausa/fade" (so atualiza tipos de trilha fora de {0,1,2,3,8,9,0xC}) |
| +0x72D / +0x72E | u8 | volume esquerdo (<<4) / direito, gravados em 0x04000080 com 0xFF00 |
| +0x72F | u8 | id da musica atual (snd_song_start) |
| +0x730 | u16 | contador de guarda: snd_frame_tick so roda quando e 0; sfx e musica incrementam/decrementam em volta |

- Tick por quadro: `VBlankIrqHandler` 0x080007FC -> `snd_frame_tick` 0x08073274 (xref confirmado, unica chamada).
  `snd_frame_tick` -> `snd_update_frame` 0x08073094, que: (1) avanca as trilhas da lista (`snd_track_update`,
  com filtro do bit1 de +0x72C); (2) atualiza os 4 canais PSG (`snd_channel_update_1..4`, 0x080736A0..0x080737E8);
  (3) renderiza grupos `driver+0x690` e `driver+0x6D8` (`snd_render_group_a/b`); (4) `snd_ring_write` nas trilhas
  0..2 (buffer ciclico de 48 B); (5) `snd_master_ctrl_write` 0x080739F4.

## 2. Tabela de musicas

- Endereco: `0x080B012C` (ponteiro em `driver+0x688`), 4 B por entrada, ids 0..53 (SND2). Entrada 0 = `0x080B0204`,
  entrada 1 = `0x080B0238` (lido na ROM).
- Musica de mapa: `gMapMusic` `0x0847E644` (192 B; id por mapa, SND1/SND11). Mapa 23 -> id 26 @0x080B0510 (SND11).
- Pedidos: `Sound_RequestSong` 0x08047BB4 (wrapper) -> `FUN_0804842C(1,id)` -> `snd_song_request` 0x08074B88 ->
  `snd_song_start` 0x08074A8C.
- `snd_song_start` (decompile): para as trilhas (`snd_track_stop_callback`, 7 slots), le a mascara u16 do cabecalho
  e, para cada bit setado (bit 0x800 = trilha 0 ... bit 0x001 = trilha 11), chama `snd_track_init(track, ptr)`,
  grava a prioridade em `track+0x6D` e `track+0x70` = parametro (1 para musica). Grava o id em `[driver+0x72F]`.

### Cabecalho de musica e de sfx (confirmado)

    +0x00 u16  mascara de trilhas (0x800 = trilha 0 ... 0x001 = trilha 11; 0x004 = trilha 12)
    +0x02 u8   prioridade (copiada para track+0x6D)
    +0x03 u8   0
    +0x04 u32  ponteiro da trilha, um por bit setado, em ordem decrescente

## 3. Efeitos sonoros (sfx)

- Tabela: `driver+0x68C` (= `[0x03002430+0x68C]`), mesmo formato do cabecalho de musica.
- `snd_sfx_request(id)` 0x08074DE0: incrementa a guarda +0x730 (ate 0xFFFE), chama `snd_sfx_start(id, 0)`, decrementa.
- `snd_sfx_start` 0x08074D50 (decompile): para cada trilha da mascara do sfx, so inicializa se
  `track+0x6D <= prio_do_sfx`. Logo: **sfx de prioridade igual ou maior substitui a trilha; menor e ignorado**.
  Grava `track+0x4E` = id do sfx e `track+0x70` = 0.
- Pendente: o codigo lido nao zera `track+0x6D` ao fim da trilha (ver Pendente).

## 4. Formato dos comandos de trilha (confirmado no interpretador)

Interpretador: `Snd_TrackStepInterpreter` 0x08072D68 (le 1 byte em `track+0x14`, cursor++). Tabela da switch:

| Byte | Handler | Operandos (bytes apos o opcode) | Efeito |
|---|---|---|---|
| 0x00-0x7F | snd_cmd_note 0x08072910 | 1 (duracao; 2 se modo FE=1) | nota MIDI; PSG key = nota + E8 - 0x24 (clamp 0..0x47); dur = (FA<<8) + b + 1 |
| 0x80-0x8F | snd_cmd_rest 0x08072AC0 | 1 | pausa b+1 ticks (nao usa FA) |
| 0x90-0x9F | (NOP) | 0 | 1 byte, sem efeito |
| 0xA0-0xAF | (NOP) | 0 | 1 byte, sem efeito |
| 0xB0-0xBF | (NOP) | 0 | 1 byte, sem efeito |
| 0xC0-0xCF | (NOP) | 0 | 1 byte, sem efeito |
| 0xD0-0xDF | snd_cmd_volume 0x08072AFC | 0 | volume = n (track+0x65); ramo extra se FE=1 e sub=3 |
| 0xE0 | snd_cmd_rest | 1 | igual 0x80 |
| 0xE1 | snd_cmd_E1_set_track_2c | 1 | track+0x2C = xx |
| 0xE2 | snd_cmd_E2_set_track_69 | 1 | track+0x69 = xx; flags |= 0x51 |
| 0xE3 | snd_cmd_voice 0x08072B34 | 1 | voz/amostra = xx (track+0x67); em trilhas PCM = indice da amostra |
| 0xE4-0xE7 | snd_set_voice(0,0x40,0x80,0xC0) | 0 | grava voz/flag de controle fixo |
| 0xE8 | snd_cmd_transpose 0x08072B64 | 1 (com sinal) | track+0x6A, somado a nota |
| 0xE9 | snd_cmd_E9 0x08072B74 | 1 | track+0x6B = min(xx, 0x30) (GUESS volume/expressao) |
| 0xEA | snd_cmd_EA_set_track_6f | 1 | track+0x6F = xx; flags |= 0x40 |
| 0xEB | snd_cmd_EB 0x08072BAC | 0 | track+0x4C += 1 (GUESS contador) |
| 0xEC-0xEF | (sem case: default -> snd_cmd_note) | ver nota | cai no ramo de nota, nao e comando (Ghidra) |
| 0xF0 / F1 | loop1 start / end | 1 / 0 | contador track+0x52; inicio track+0x18 |
| 0xF2 / F3 | loop2 start / end | 1 / 0 | contador track+0x53; inicio track+0x1C |
| 0xF4 | snd_cmd_jump 0x08072C10 | 4 (a32) | cursor = a (JMP) |
| 0xF5 | snd_cmd_call 0x08072C20 | 4 (a32) | track+0x20 = retorno; cursor = a (CALL) |
| 0xF6 | snd_cmd_return 0x08072C34 | 0 | cursor = track+0x20 |
| 0xF7 | snd_cmd_F7 0x08072C3C | 1 | track+0x54 (0 -> 8) (GUESS vibrato/ataque) |
| 0xF8 | snd_cmd_F8 0x08072C64 | 1 | track+0x55 (GUESS) |
| 0xF9 | snd_cmd_lfo 0x08072C74 | 2 | track+0x56 = aa; +0x57 = bb&F; +0x58 = bb>>4 (usado por snd_calc_vibrato) |
| 0xFA | snd_cmd_dur_hi 0x08072CA0 | 1 | track+0x6E = byte alto da proxima duracao |
| 0xFB | snd_cmd_envelope 0x08072CB0 | 4 | track+0x5B..0x5E; flag 0x62=1 (GUESS envelope) |
| 0xFC / 0xFD | snd_cmd_FC / FD | 0 | track+0x62 = 1 / 0 (GUESS) |
| 0xFE | snd_cmd_mode 0x08072D4C | 1 | track+0x63 = modo da nota (0 normal; 1 = nota com operando extra) |
| 0xFF | snd_cmd_end 0x08072D5C | 0 | zera cursor; chama handler track+0x0C |

Notas do decompile de `snd_cmd_note`:
- Modo 0: le a duracao do byte seguinte. Sub-modo em track+0x64: 0..2 = nota com transposicao (+E8-0x24, clamp
  0..0x47, `snd_freq_lookup`); 3 = `snd_set_note_freq` direto; 4/5 = nota PCM crua em +0x48.
- Modo 1 (FE 1): nao le duracao; le 1 byte extra (vai para o byte alto de +0x46). Extrator nao trata (divergencia 4).
- Modo 2: le a duracao e grava em `+0x28`; sub-modo >= 3 usa a tabela de ponteiros `[DAT_08072a98]+0x728`.
- `snd_cmd_rest` nao consome nem zera FA (+0x6E); a nota seguinte consome e zera.

Canais: a ligacao trilha -> canal PSG/PCM e feita pelas funcoes `snd_channel_update_1..4` e pelos grupos
(driver+0x690 / +0x6D8). Mapeamento exato: pendente.

## 5. Mixagem, DMA e PCM

- PSG (canais 1-4): escrito em REG_SOUND* (0x04000060..0x04000084) por `snd_channel_update_1..4`. Frequencia pela
  tabela `0x083ABB50` (74 halfwords; Hz = 131072/(2048-v)). SOUNDCNT_L 0x04000080 via `snd_master_ctrl_write`
  (`(+0x72E) | ((+0x72D)<<4) | 0xFF00`, so se bit0 de +0x72C).
- PCM (Direct Sound): FIFO A `0x040000A0` e FIFO B `0x040000A4` (literais em 0x08486E54/58, confirmados). DMA1 com
  origem dentro da amostra (SND13: watch 0x040000BC, enderecos 0x0819A020/0x0819CA80 para a amostra 58).
  Timer que dita a taxa: nao confirmado (pendente).
- Amostras: tabela de ponteiros `0x080B16E4` (128 entradas). Cada entrada aponta para cabecalho
  {u32 tamanho, u16 taxa, u8 flag 0, u8 nota-raiz (0x3C), u32 flag de laco, u32 inicio do laco} seguido de int8
  a partir de +0x10. Confirmado pelo extrator e pelo replay (SND10/SND13).
- Taxa de PCM por nota: `snd_pcm_rate_set` 0x08073C18 (limite 0x78000), `snd_pcm_rate_from_note` 0x08073C3C.

## 6. Divergencias: pc/extract/extract_music.py x Ghidra (so leitura, nada alterado)

1. **0x90, 0xA0, 0xB0, 0xC0** estao em `ONE_BYTE` (o extrator le 2 bytes). No Ghidra sao NOP de 1 byte
   (o interpretador retorna sem consumir operando). Divergencia real: desalinha tudo depois. Corrigir: tirar do
   ONE_BYTE (tratar como pc += 1).
2. **0x81-0x8F**: o extrator so trata 0x80 e 0xE0 como pausa; 0x81-0x8F cairia em "byte sem tratamento". No Ghidra
   todos os 0x80-0x8F sao snd_cmd_rest (1 operando). Corrigir: `elif 0x80 <= b <= 0x8F`.
3. **0xEC-0xEF**: o extrator os trata como comandos de 1 operando (ONE_BYTE). No Ghidra caem em snd_cmd_note (nota
   com duracao). Provavel erro do extrator. Confirmar com replay antes de mudar.
4. **FE 01 (modo 1)**: a nota consome 2 bytes e nao le duracao. O extrator ignora o modo, logo desalinha se algum
   trecho usar FE 1. Verificar se alguma musica usa FE.
5. **E4-E7**: o Ghidra grava voz/flag; o extrator so pula o byte. Mesmo tamanho; `smp` nao muda. Ignorar ate confirmar.
6. **E2**: tamanho correto (1 byte), efeito (track+0x69) nao modelado. Sem impacto na posicao.
7. **0xD0-0xDF**: o extrator guarda so o volume. Ghidra tem ramo extra com FE=1 e sub=3. Sem impacto se FE 1 nao for usado.
8. **FA**: consistente (nem o Ghidra nem o extrator zeram FA no rest).

Tamanhos sem divergencia: E0/80 (rest), E1, E3, E8, E9, EA, EB, F0-F3, F4/F5 (a32), F6, F7, F8, F9 (2 bytes),
FA, FB (4 bytes), FC, FD, FE (1 byte), FF.

## 7. Prioridade de sfx (resumo)

- Cada trilha guarda a prioridade atual em track+0x6D.
- Sfx novo entra se `track+0x6D <= prio_novo`. Musica usa o mesmo caminho (snd_track_init).
- A guarda +0x730 evita reentrar no frame durante um pedido.

## Pendente

- Timer da taxa PCM e sequencia exata da DMA (quem liga timer e DMA; registradores). Nao lido.
- Quem zera `track+0x6D` ao terminar a trilha; reset em `snd_track_reset_state` 0x0807490C.
- Efeito exato de E4-E7, E2 (track+0x69), E9, F7, F8, FB (envelope), FC/FD e modo FE 1/2 no sintetizador.
- `snd_channel_update_1..4` (PSG): registradores exatos e envelope.
- Mapeamento trilha -> grupo de render (driver+0x690 / +0x6D8) e papel da trilha 12.
- Conteudo da tabela de sfx (quais ids e ponteiros): so o mecanismo foi lido, nada extraido.
- `snd_hw_init` 0x08073A88: escritas em 0x04000084/0x04000064 nao decodificadas.
- Confirmar EC-EF (nota ou comando) com replay.
