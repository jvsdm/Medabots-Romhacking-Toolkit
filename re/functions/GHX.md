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

## 4. Fechamento do modo de campo (2026-10-18, Ghidra + ROM original, confirmado salvo onde marcado)

Literais (lidos da ROM original, E = 0x030057E0): E+0xC33 = 0x03006413 (modeByte); E+0xC5C = 0x0300643C; E+0xC5D =
0x0300643D (modeFlags); E+0xC5E = 0x0300643E (backup); E+0xC94 = 0x03006474 = byte de argumento de 5D/5E; E+0xCBA = 0x0300649A.

**Correção de premissa**: `Overworld_CheckStepAllowed` 0x08057D80 NAO bloqueia o passo. Os callers são só de menu:
`Overworld_InitChoiceMenuScreen` (0x08057244/..330), `Map_CreateChoiceSprites` (0x08058080..150),
`FieldMenu_ChoiceSub2_SelectRow` (0x08057500) e `Screen_LoadTilesAndMapEntities` (0x0805782E..970). Retorna 0..6 por
linha do menu de escolha (param 0..3 = linha; 1 = ativa, 4 = bloqueada por flag, 5/6 = alvo/condição de tile).

- Param 0 (item 0x12, peça 0x55): bit 0x02 de 0x0300643D bloqueia (código 4); depois E+0xC5C == 2 → código 5.
- Param 1 (item 0x18): bit 0x10 bloqueia (4); depois E+0xC5C em 1..2 → 5.
- Param 2 (item 0x19, peça 0x0B, registro de encontro 1): bit 0x04 bloqueia (4); E+0xC5C em 1..2 → 5.
- Param 3 (item 0x26, peça 0x6B): bit 0x08 bloqueia (4); tile adjacente (0x080524C8) != 3 → 6. Por isso o tile 3 da
  colisão (W4) só passa com a ação de menu, não pelo passo.
- Bits 0x01 e 0x20 NÃO são lidos aqui.
- O passo do jogador (`tileBlocked` no porte; W2/W4 na ROM) só usa colisão de mapa. Nenhuma máscara de 0x0300643D
  entra nele. Portanto "bloqueio de passo pelas máscaras" NÃO existe; o porte não foi alterado nesse ponto.

**Leitor do bit 0x01** (confirmado no decompile de `Fight_ClassifyActionState` 0x08048960): itens-chave ids 1, 5,
0xC, 0xD, 0x13, 0x27, 0x28. Com bit 0x01 ligado devolvem classe 3 (sem o teste de mapa/ID). Com bit 0 desligado
devolvem 4 (se E+0xC5C-1 <= 1) ou 1. O id 0 com 0x20 ligado devolve 3 (já em 2).

**5D e 5E** (decompile, confirmados; ver `EVENT` no porte):
- 5D (0x0804CE90): arg b = `Script_ReadBytes(1)` gravado em E+0xC94. b & 0x80 == 0 → E+0xC33 = b & 0xF; b & 0x80 e
  b & 0xF == 1 → 0x0300643D |= 0x20; outro → &= ~0x20.
- 5E (0x0804CEF8): arg gravado em E+0xC94; arg == 1 → 0x0300643D |= 0x01; outro → &= ~0x01.

**Map_FocusEventObjectRenderSlot** 0x0804D9F8: chamado pelos dois ramos do 0x60 (`Event_Op60_Focus_Event_Object`,
0x0804CFD0 e 0x0804CFFE). Grava o byte original em E+0xC5E e depois 0xDF em 0x0300643D. Restaurado só pelo 0x61
(`Event_Op61_Restore_Focused_Event_Mode_Flags`, 0x0804D00E → 0x0804DB80).

**E+0xC33 != 0 (0x080511C0, `Overworld_UpdateMapSceneStateMachine` 0x08050F5C)** — corrigido: o r4 gravado em
gMainState é 0, não "r4 vindo de [0x0300058C]". Condições, todas no mesmo quadro:
- 0x0300058C bit 0x01 == 0 (senão o ramo vai para outra parte);
- 0x0300058C bit 0x02 == 1;
- E+0xC33 != 0.
Efeito: NoOp 0x08047E4C; gMainState (0x0300178C) = 0 (estado de campo, MainState 0); [0x03002B8C] = 1;
GS 0x0300137C = 0; E+0xCBA = 2; chamadas de cópia 0x080505FC. Ou seja, volta ao estado de campo e zera a
sequência de GS. O sentido de 0x0300058C bits 0x01/0x02 NÃO foi decodificado (em aberto).

**Porte (2026-10-18)**: `script.save.modeByte` (E+0xC33) e `script.save.modeFlags` (0x0300643D) gravados pelos ops
`mode_set` (5D) e `mode_flag` (5E) do `script.lua`; o foco do 0x60 grava 0xDF e o 0x61 restaura o backup
(`world.lua`). A reação do 0x0300058C (gMainState = 0) NÃO foi portada: o porte não tem 0x0300058C. Sem efeito visível
confirmado no porte até agora; o modeByte fica salvo e aparece no log como "modo de campo 0x5D".

E+0xCBA (0x0300649A) recebe 2 nesse ramo; nome não confirmado.

## 5. Música por mapa e por evento (CHAPENC, 2026-10-18, Ghidra + ROM)
- `Event_Op49_Play_Map_Music` 0x0804C9C4 = política 4 → `FUN_080483F8` (0x080483F8, confirmado no decompile): se
  E+0xC30 (0x03006410) == 0xFF devolve E+0xC31 (0x03006411, id da música persistente do 0x48); senão devolve
  `gMapMusic[E+0x7FE]` (ROM 0x0847E644). **Sem entrada de capítulo.** A troca de música por capítulo não existe.
- 0x45 Play_Music = id direto (política 1). 0x48 Play_Persistent_Music grava E+0xC31 e marca E+0xC30 = 0xFF.
- Porte: `world.loadMap` → `music.playMap(n)` = `gMapMusic[n]` (cache `mapMusic[23] = 26`, confere). Divergência aberta:
  o porte não aplica o override persistente (E+0xC30 == 0xFF) depois de um 0x48. Não alterado.
