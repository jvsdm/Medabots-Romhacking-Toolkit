# GHX: música da Robattle, opcodes 0x5D/0x5E, leitores de E+0xC33 / E+0xC5D (2026-10-18)

Ghidra MCP (sem emulador). Endereços = entrada de função ou literal lido. Confiança: confirmado (decompile/disasm),
provavel (nome ou contexto), chute.

## 1. Música da Robattle (id para o porte)

- Entrada: `Battle_IntroSetupStep` 0x0806D9A8 pede `snd_song_request(0)` (silêncio). `Battle_InitScreenAndSlots`
  0x0806DCDE também pede 0.
- Intro: `Screen_IntroTimeline` 0x0806DAB4, passo 0, pede **`snd_song_request(2)`** (confirmado no decompile).
  Id 2 no índice do porte (`pc/cache/music.lua`) é um **stream** (`streams[2]`, `sound/s077.wav`, loop, 23,3 s), não
  uma trilha de `tracks`. Provável música de luta (nome do stream sugere Medaforça). Confiança: provavel.
- Jingles/cues durante a luta (não são a música de fundo): `Fight_SceneStep43_HoldBannerThenJingle` 0x08030FEC
  pede 0x1C (banner); `Battle_GaugeTickStep` 0x08010B54 pede 0xE/0xF; `Fight_CyclePartFrameTableA` 0x08011078 pede
  0x10; `Fight_FadeOutSoundStep` 0x080114E0 pede fade 8.
- Opção "ROBATTLE BGM" do título (3 escolhas, confirmado):
  - Byte salvo em `0x03000B94` (linha 2 de `Options_ThreeRowMenuStep` 0x0805C574; o CSV antigo chamava SAVE_OPT_C).
  - Tabela na ROM **0x080AF8CA** = `09 0A 0B` (ids 9, 10, 11), indexada por esse byte.
  - Usos: (a) prévia ao mudar a opção, no próprio menu (`snd_song_request(tbl[v])`); (b) resultado pós-luta em
    `Fight_PlayResultCueByState` 0x08018A54 (chamada por `Fight_PlayResultCueStep` 0x0801425E): se `+0x89==1` pede 9;
    se high nibble de `gPostBattleModeFlags` = 0 pede `tbl[opção]`; 0x10 pede 0xC; outro pede 0xD.
  - Ids 9-11 não têm stream nem trilha no índice do porte: viram silêncio. Não é a música de fundo da luta.
- Decisão para o porte (aplicada em `pc/game/src/battle.lua`):
  - `BATTLE_SONG = 2` (era nil).
  - Início da luta: `music.play(BATTLE_SONG)` (antes `music.playTrack`, que só toca trilhas e daria silêncio para id 2).
  - Fim: `music.stop()` já existente (linha ~918) continua valendo.
  - Teste: `lovec.exe pc/game --scene battle --battleId 1 --frames 60` sem erro (exit 0, nenhum "music:" no log).
  - Não testado com som real (headless). Não confirmei por ouvido que o id 2 é a música de luta.
- Para o resultado (prévia/cue pós-luta com 9/10/11): não implementado; não há áudio para esses ids no índice.

## 2. Opcodes 0x5D / 0x5E (E = `EVENT_STATE_E` 0x030057E0)

Numeração: o CSV e os comentários dizem 0x5C/0x5D, mas os nomes e os endereços abaixo seguem a numeração pedida
(0x5D = `Event_Op5D_Update_Overworld_Event_Mode_Flags` 0x0804CE90; 0x5E = `Event_Op5E_Set_Overworld_Event_Mode_Bit0`
0x0804CEF8). Nomes mantidos (regra do AGENTE). Literais: `E+0xC94` = byte de argumento (Script_ReadBytes /
EventScript_ReadNextByte), `E+0xC33` = 0x03006413, `E+0xC5D` = 0x0300643D (gOverworldEventModeFlags),
`E+0xC5E` = 0x0300643E (backup).

- **5D arg b** (confirmado na disasm):
  - bit 0x80 = 0: `E[0xC33] = b & 0xF`. Não mexe em 0x0300643D.
  - bit 0x80 = 1 e `b & 0xF == 1`: `0x0300643D |= 0x20`.
  - bit 0x80 = 1 e outro nibble: `0x0300643D &= ~0x20`.
- **5E arg** (confirmado): arg == 1 → `0x0300643D |= 0x01`; outro → `&= ~0x01`.
- Ev 185 (mapa 23): `5D 00` → E[C33]=0 (sem efeito em 0x20); `5E 01` → bit 0x01 ligado.

### Leitores de E+0xC33 (0x03006413). Único escritor: 5D.
- `Overworld_UpdateMapSceneStateMachine` 0x08050F5C, em 0x08051212: se `E[C33] != 0`, chama
  `Overworld_NoOpStateHandler` 0x08047E4C (vazio) e grava `gMainState` 0x0300178C = r4. r4 vem de `and r4,r1` com
  `[0x0300058C]` em 0x080511C0 (não decodifiquei o valor exato). Efeito: a máquina de estados do mapa troca de estado
  (provável: sai do andar livre, por exemplo para uma cena ou menu). Confiança: provavel.
- `Screen_RedrawAndDispatchStep` 0x08058C0C, em 0x08058E96: lê C33 (uso não decodificado). chute.

### Leitores de E+0xC5D (0x0300643D)
- `Fight_ClassifyActionState` 0x08048960 (nome CSV "KI", item-chave), em 0x08048A2E: item-chave id 0 com bit 0x20
  ligado → classe 3 (senão checa mapa e devolve 2 ou 1). Bits 0x01 (ids 1,5,C,D,13h,27h,28h → classe 3, pelo CSV) em 0x08048A54.
  Confirmado para 0x20; 0x01 vem do comentário do CSV, não reconferido. Efeito: como o item-chave é usado/classificado no
  menu. Não é música nem encontro.
- `Overworld_CheckStepAllowed` 0x08057D80: máscaras 0x02 (dir 0), 0x10 (dir 1), 0x04 (dir 3), 0x08 (dir 3, 2º teste).
  Bloqueia o passo. Não lê 0x01 nem 0x20. Confirmado.
- `Map_FocusEventObjectRenderSlot` 0x0804D9F8, em 0x0804DB10: copia o byte original para E+0xC5E (backup) e depois
  **grava 0xDF** no byte (instrução `strb` com 0xDF, sem AND: zera o 0x20 e liga os outros bits; confirmado nos halfwords
  0x20DF/0x7008). `Map_RestoreFocusedEventModeFlags` 0x0804DB80 restaura do backup. Ou seja, durante o foco de objeto de
  evento o byte vale 0xDF. Se o porte quiser o efeito exato, use 0xDF, não "bit 0x20 = 0".
- `Event_Op2C_SetActorPriority` 0x0804C364: liga/desliga 0x10 (não é 0x01/0x20). Confirmado.
- `Screen_ReturnEpilogue`/literal 0x0805942C: máscara 0x40 (decode parcial). chute.
- Não há leitor de 0x0300643D no código de som (`SND_PolicyRequest` usa `currentSongId`, não E+C5D). Música NÃO
  depende dos bits 0x01/0x20/0x10.
- Não há leitor de encontro aqui: encontro usa 0x0300640C (opcode 0x5F), fora deste escopo.
- Literais absolutos 0x083D6284 (0x0300643D) e 0x083D629C (0x03006413) ficam numa tabela de dados (0x080EC9EE+). Leitor
  não rastreado. chute.

## 3. O que o porte precisa (não editado: script.lua / world.lua)

- `pc/game/src/script.lua`: os opcodes 0x5D/0x5E hoje são pulados (ver comentário "Ops desconhecidos com next são pulados",
  linha 26; ev 185 "pula 0x5D/0x5E" em README ST1). Falta:
  - estado persistente `modeByte` (E+C33, valor b&0xF) e `modeFlags` (0x0300643D, bits 0x01 e 0x20). Se for salvo,
    entra no bloco E / save (E+0xC33 e E+0xC5D fazem parte do bloco E copiado para o save, `EVENT_STATE_E` 0xCC0 B).
  - handlers: 5D: regra acima; 5E: regra acima (no dispatch do script, perto da chamada `call(host, "shop", ...)`
    linha ~119, onde estão os outros ops; a tabela exata não conferi).
  - `Map_FocusEventObjectRenderSlot` equivalente: salvar modeFlags em backup, gravar 0xDF ao focar objeto de evento,
    restaurar do backup ao sair (`world.lua`, onde o foco de objeto de evento for tratado; não localizei o ponto exato).
- `pc/game/src/world.lua` (máquina do mapa): consumir `modeByte != 0` → trocar o estado do mapa como acima. Não
  implementado: o alvo do gMainState (r4) não foi decodificado; o efeito concreto precisa de teste na ROM de debug.
- Item-chave (classificação com bit 0x20/0x01): se o porte tiver o menu de item, a regra de `Fight_ClassifyActionState`
  para o id 0 (bit 0x20 → classe 3) deve ser espelhada. Localização no porte não verificada.
- `battle.lua`: já feito (seção 1).

## Em aberto
- Confirmar por ouvido/mGBA que id 2 é a música de luta (e não só a intro). Olhar o ponto de volta ao mapa.
- Valor exato de r4 em 0x080511C0 (gMainState que o 5D seta).
- Leitor do bit 0x01 para ids 1,5,C,D,13h,27h,28h e do literal 0x0805942C (máscara 0x40).
- Tabela 0x083D6284/0x083D629C (dados) — quem a lê.
