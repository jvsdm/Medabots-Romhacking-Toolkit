# SC4: efeito dos estados de cena (tabela 0x080542B0)

Confianca: **confirmado** = nome/chamada vista nos bytes (decodificados do .gba) e nome no mtbe. **provavel** = efeito
deduzido das chamadas e das palavras do stream. **chute** = so a forma (cadeia sf32, escrita de campo) sem corpo lido ate o fim.

## Metodo e limite

O Ghidra MCP so desmonta funcao inteira; os 45 alvos nao tem funcao (sync so desmontou). Por isso decodifiquei os bytes
da ROM com um decodificador Thumb proprio (`scratchpad/sc4/dis.py`, `python -I`), validado em 0x080567F0.
Cada alvo lido ate o primeiro retorno/salto (ate 45 instrucoes). Sem analise de quem chama a tabela.

## Modelo comum (lido em 0x080567FE, 0x080554EA, 0x0805676C)

- Estado em `gOverworldUiState` (0x03005350) +0xF4; passo/cursor do stream em +0xFC (indice de palavra).
- Argumentos: palavras de 32 bits do stream em r9; cada estado le `r9[passo]` e avanca +0xFC.
- Parametros vao para o registro de ator/aux (ptr 0x03006D30, stride 0x300) ou para gOverworldUiState.
- Cauda comum 0x080567FE: se [+0xF4] nao e 0/1/2/4, chama Scene_DispatchStateHandlerTail (bl 0x08054280, nome do
  mtbe; o SC3 cita 0x080542A0); senao zera +0xFC e +0xF8 e segue o passo.
- 0x080567F0 (funcao, idx 0x26..0x7F): zera +0xF4 e +0xFC e cai em 0x080567FE. Renomeado no Ghidra para
  `Overworld_ClearStateThenFinish` (antes: `Overworld_EnterSceneState`, nome do SC3 que nao bate com os bytes).

## Tabela indice -> efeito

| idx | alvo | rotulo | confianca / efeito |
|---|---|---|---|
| 00 / 04 / 25 | 0x080567FE | (miolo de 0x080567F0) | fim de passo (cauda 0x080567FE) |
| 01 | 0x080544E0 | SceneState_01_SetViewParams | provavel: parametros de camera/vista (4 palavras + 2 sf32) |
| 02 | 0x080545B4 | SceneState_02_UpdateActorLoop | chute: laco sobre atores |
| 03 | 0x080550E4 | SceneState_03_SetActorRecord | provavel: grava 4 palavras no registro do ator |
| 05 | 0x0805513C | SceneState_05_CondSetPos | chute: escrita condicional de posicao |
| 06 | 0x08055150 | SceneState_06_CondSetPos | chute: igual ao 05 |
| 07 | 0x080551D4 | SceneState_07_CondSetPos | chute: igual ao 05 |
| 08 | 0x08055258 | SceneState_08_CondSetPos | chute: igual ao 05 |
| 09 | 0x080552F0 | SceneState_09_SetFieldSf32 | provavel: campo <- sf32(palavra) (cauda 0x080554EA) |
| 0A | 0x0805531C | SceneState_0A_SetFieldSf32 | provavel: igual ao 09 |
| 0B | 0x0805534C | SceneState_0B_SetMapScratch | chute: gMapRenderScratch 0x03006D40 |
| 0C | 0x080553D8 | SceneState_0C_SetMapScratch | chute: igual ao 0B |
| 0D | 0x08055464 | SceneState_0D_SetFieldSf32Off | chute: cauda 0x080554E8 (campo com offset) |
| 0E | 0x08055494 | SceneState_0E_SetFieldSf32 | provavel: campo <- sf32 |
| 0F | 0x080554C4 | SceneState_0F_SetSf32Finish | chute: sf32 + fim de passo |
| 10..12 | 0x08055508 | SceneState_10_12_Branch | chute: alvo compartilhado, testa [+0xF4]==0x10 |
| 13 | 0x08055724 | SceneState_13_ScaleMul | chute: sf32 * valor |
| 14 | 0x0805579C | SceneState_14_ScaleMul | chute: igual ao 13 |
| 15 | 0x08055824 | SceneState_15_ScaleMulChain | chute: sf32 * valor * sf32 |
| 16 | 0x080558D4 | SceneState_16_ScaleMulChain | chute: igual ao 15 |
| 17 | 0x08055590 | SceneState_17_DivSf32 | chute: divisao sf32 (razao/interpolacao) |
| 18 | 0x08055618 | SceneState_18_DivSf32 | chute: igual ao 17 |
| 19 | 0x080556A0 | SceneState_19_DivSf32 | chute: igual ao 17 |
| 1A | 0x08055994 | SceneState_1A_MulSf32 | chute: sf32 * valor |
| 1B | 0x08055A28 | SceneState_1B_SetMultiWords | chute: varias palavras, usa 0x03005448 |
| 1C | 0x08055C90 | SceneState_1C_SetAuxRecord | chute: registro do aux buffer |
| 1D | 0x08055CF8 | SceneState_1D_SetAuxWord | provavel: palavra no aux (+0x208) |
| 1E | 0x08055D68 | SceneState_1E_SetAuxRecord | chute: registro do aux buffer |
| 1F | 0x08055DD0 | SceneState_1F_StoreWordCall | chute: chama 0x0805678C (grava 1 palavra) |
| 20 | 0x08055E24 | SceneState_20_StoreWordCall | chute: chama 0x0805678E |
| 21 | 0x08055E7C | SceneState_21_StoreWordCall | chute: chama 0x0805678E |
| 22 | 0x08055ED4 | SceneState_22_SetWordReg | chute: corpo nao lido |
| 23 | 0x08055F40 | SceneState_23_Sync | chute: fim de passo |
| 24 | 0x08055F8C | SceneState_24_SetAuxRecord | chute: aux buffer |
| 80 | 0x080560BC | SceneState_80_FadeReset | **confirmado** nome: chama Overworld_ResetAndStartScreenTransitionEffect (0x08049D90) |
| 81 | 0x0805612C | SceneState_81_FadeStart | **confirmado** nome: chama Overworld_StartScreenTransitionEffect (0x08049D18) |
| 82 | 0x0805619C | SceneState_82_SetFadeParams | provavel: byte em +0x208 e halfword em +0x20A (parametros de fade/tempo) |
| 83 | 0x08056280 | SceneState_83_SetParams | chute: corpo nao lido |
| 84 | 0x080563CC | SceneState_84_RebuildObjects | provavel: NPC snapshot, zera gObjectSlots e gTrackedActorHalfwords, rebuild de render slots |
| 85 | 0x080565B8 | SceneState_85_StoreWord | provavel: cauda 0x0805678E |
| 86 | 0x08055BB4 | SceneState_86_SetAuxRecord | chute: aux buffer |
| 87 | 0x08056604 | SceneState_87_StoreTwoWords | chute: cauda 0x0805676C (2 palavras) |
| 88 | 0x080567A4 | SceneState_88_UpdateEventObjects | provavel: Map_UpdateEventObjectDepthAndMarkerState (0x0804B1B0) |
| 89 | 0x08056660 | SceneState_89_SwitchMode | chute: 3 palavras, switch em 0/1/>1 |
| 8A | 0x08056724 | SceneState_8A_SetAuxRecord | chute: aux buffer, sem chamada |
| 8B | 0x080567D4 | SceneState_8B_InitPlayerSprites | provavel: Overworld_InitPlayerSprites (0x080517F0) + Overworld_SetActorAnimFrameByTable (0x08051E38) |
| 26..7F | 0x080567F0 | Overworld_ClearStateThenFinish | confirmado bytes: zera estado e passo, cai em 0x080567FE |

## Em aberto

- Ghidra: os 45 alvos nao sao funcoes, entao nao houve rename de rotulo (rename_function_by_address so pega funcao).
  Unica troca feita: 0x080567F0 -> Overworld_ClearStateThenFinish (antes: Overworld_EnterSceneState).
- Para fechar os chutes: criar a funcao em cada alvo no Ghidra (ou ler o corpo ate o fim) e ver as chamadas completas.
- Divergencia: SC3 cita o despachante em 0x080542A0; o nome do mtbe e 0x08054280 (bl da cauda 0x080567FE).
