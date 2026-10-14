# SN4: conflito de nomes no tick de som (0x08072818 / 0x08072F7C / 0x08073010) e família mal rotulada

Escopo: resolver o conflito apontado no SN3 (seção 2) e varrer os snd_* de 0x08072000..0x08073800 atrás de
chamador de código de objeto (gObjectSlots 0x03002C30). Fonte: Ghidra (decompile, xrefs, renames), bytes da ROM
original lidos com python -I (só leitura), varredura de BL e ponteiros de ROM. mtbe.csv não foi editado.

## Veredito do conflito: é som, não objeto

Argumento principal: a struct passada ao tick (r0=r4) não é gObjectSlots. Os campos usados vão até +0x69 (objeto
tem 0x40 bytes). Os campos são de canal/trilha: +0x24 cursor do comando, +0x2C/+0x30 duração e contador,
+0x40/+0x42/+0x44 pitch, +0x50 flags, +0x5B..+0x5E envelope, +0x61/+0x62 fase e gate, +0x66 nível, +0x69 valor
de controle.

A origem é uma tabela de 13 registros de 12 bytes `{update_fn, tick_fn, tag}` em **0x083ABD60**, lida em
`snd_track_init_all` 0x08074A04 (ref 0x08074A70). Entradas de tick:
- tags 00..02 e FF00..FF02: `0x08072FF4` (antes Bar_TickWithSoundModulation);
- FF03: `0x08073010` (antes Obj_TickWaitCountdown);
- FF04/FF05: `0x08073044` (snd_tick_vibrato_and_pitch_slide).

Decisões:
- **0x08072818 snd_pitch_slide_tick: nome mantido.** Rampa de pitch com flag +0x50|=2 consumida por
  `snd_channel_flush_freq_scaled`. Chamadores: só ticks de som.
- **0x08072F7C snd_track_cmd_ext: nome mantido.** Decodifica comandos da trilha (Dn volume, E1, E2, FF). Único
  chamador: `snd_track_tick_wait` (FF03). Confirmado por decompile.
- **0x08073010: renomeado para snd_track_tick_wait** (provável). Lógica pelos bytes: se cursor +0x24 != 0,
  decrementa +0x30; ao zerar, chama snd_track_cmd_ext; senão snd_pitch_slide_tick. snd_track_cmd_ext recarrega
  +0x30 a partir de +0x2C, o que confirma o contador de espera. O Ghidra não decompila a função (halt_baddata).

## Família mal rotulada: Bar_* é envelope de som

Os 13 "Bar_" de 0x080723F8..0x0807266C (rotulados como "barra animada" no mtbe.csv) são a **máquina de envelope
de canal**:
- Entrada só por código de som: `snd_cmd_envelope` 0x08072CB0 e `snd_cmd_note`/`FC`/`FD` chamam
  `snd_env_reset_phase1`; a tabela 0x083ABD60 chama `snd_env_tick_by_state`. Nenhum BL nem ponteiro de ROM
  vem de código de objeto.
- Lê 4 bytes do cursor da trilha para +0x5B..+0x5E (envelope, o mesmo tipo de dado que as notas), usa gate +0x62
  e fase +0x61 (1..5).
- A fase 2 escreve +0x66 (nível, 0..15) e seta a flag +0x50|=2. Essa flag dispara `snd_channel_flush_freq_scaled`,
  que multiplica o pitch por +0x66. Ou seja, o envelope controla o volume/tom do canal.

Renomeados no Ghidra: snd_env_reset, snd_env_step_start, snd_env_step_decrement, snd_env_step_cond,
snd_env_update, snd_env_step_level3, snd_env_step_level2, snd_env_reset_phase1, snd_env_tick_phase1..4,
snd_env_tick_by_state.

## Outros rótulos Obj_ que são som (renomeados)

- Obj_FlushFlag8ToWord 0x080732C8 -> snd_channel_flush_flag8 (flag bit 8: grava +0x54 no registrador)
- Obj_FlushFlag2ScaledValue 0x080733BC -> snd_channel_flush_freq_scaled (flag bit 2: freq = +0x40*+0x66 & máscara)
- Obj_FlushFlag1Value69 0x080734F4 -> snd_channel_flush_ctrl69 (flag bit 1: grava +0x69|0x8000)
Chamadores: snd_channel_update_1..4 e snd_update_frame.

## snd_track_test_flag2 0x08074540 -> snd_driver_test_flag_bit1 (provável)

Decompile: `return byte([0x03007014] + 0x72C) >> 1 & 1`. O 0x72C é do driver, não da trilha (o nome antigo estava
errado). Chamadores: snd_update_frame (0x08073150..0x080731B2), snd_track_request_update (0x08074642), e os quatro
`snd_update_channelN_if_track_flag`/`Snd_TrackUpdateFlagged`. Ou seja: é gate global de atualização de canal.
O significado exato do bit 1 ainda não está fechado.

## Varredura: snd_* de 0x08072000..0x08073800 com chamador de objeto

Resultado: **nenhum**. Todos os BL e ponteiros de ROM que apontam para essa faixa vêm de código de som
(snd_*, Snd_*, Bar_*, Obj_Flush*, snd_track_*), da tabela 0x083ABD60, de `VBlankIrqHandler` 0x080007E0 (chama
snd_frame_tick) ou de `Scene_InitSoundAndStartSong0` 0x0800058C (chama snd_driver_init_block). Não há gObjectSlots
por trás de nenhum snd_*. Ficou fora de escopo: Palette_FadeStepWithSfx 0x08072080,
FightPreview_TickIntroAndBuildLayer 0x08072188, Preview_InitCompositeAndMotion 0x08072234 e
Preview_StepSfxSequenceAndMotion 0x080722B0 (nomes não-snd na mesma faixa; não examinados aqui).

## Extra

- Ghidra: `snd_cmd_set_flag62_on` 0x08072CB0 renomeado para `snd_cmd_envelope` (nome do mtbe.csv, confirmado
  pelo decompile).

## Pontos em aberto

- Significado de bit 1 de driver+0x72C (snd_driver_test_flag_bit1).
- mtbe.csv ainda tem os nomes antigos (Bar_*, Obj_TickWaitCountdown, Obj_FlushFlag*, snd_track_test_flag2)
  e o comentário "barra animada". Fica para a fusão, como o SN3 pediu.
- Preview_*/FightPreview_* na faixa 0x08072080..0x08072234: não checados.
