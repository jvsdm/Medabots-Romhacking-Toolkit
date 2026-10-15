# INDEX: mapa de leitura de re/functions/

Para quem chega sem contexto. Organizado por subsistema; dentro de cada um, a ordem de leitura sugerida.
Cada linha diz o que o arquivo tem e, quando existe, se um arquivo posterior corrigiu ou fechou o conteúdo.
Arquivos `*_simbolos.csv`, `*_lista.txt` e `*_progresso.txt` são apoio: não são lidos como texto.
Endereços são da ROM EU (`Medabots - Metabee (Europe).gba`). Offset da pesquisa do PC = Ghidra - 0x40 no bloco de batalha.

Convenção de confiança nos arquivos: "confirmado" = lido no código/decompile ou nos bytes da ROM; "provável" = padrão de uso; "chute" = nome sem prova.

## 0. Antes de tudo (método e correções gerais)

- README.md: formato das seções e faixas de funções (R0..R4); dicas de contexto e aritmética da BIOS.
- HANDOFF_DISCOVERY.md: handoff do discovery; o que falta e onde retomar.
- QA3.md: vereditos de conflitos entre documentos; ler antes de confiar em nomes ou endereços citados em dois lugares.
- QA1.md: conferência das 18 funções criadas pelo MedabotsSync.
- QA2.md: endereços que caem no meio de outra função no Ghidra (limites de função).
- DC.md, DC2.md, DC3.md: aplicação de correções em documentos antigos (DC3: sentido de `Mem_CopyBytes`, origem é o 1º argumento).
- FIX.md: correções de notas antigas (JT, HA3, HE4, HC5).
- CF.md: decisões de conflito de nome entre mtbe.csv e o Ghidra.
- NM.md, NM2.md: limpeza de nomes em português (NM2 continua NM).
- MERGE_NOTAS.md, MERGE_RELATORIO.md, MERGE_RELATORIO2.md: fusão dos `*_simbolos.csv` no mtbe.csv (quais nomes entraram; conflitos).
- CH3.md: revisão dos "chutes" (EM ANDAMENTO: R3 parcial, R4 não começou).
- NX1.md: funções novas do sync (EXTRA_criar); corrige o próprio EXTRA_criar (o driver não chama 0x080196C0).

## 1. Sistema (laço, boot, cenas, save, som, gráficos, link, debug, menus, título)

Laço e boot:
- MN.md: laço principal (VBlank, teclas, `gMainStateTable`), estados globais, escolha de fluxo por `GameState_StartFlowByCommand`.
- TL.md: boot até o primeiro passo no mapa (logo, título, New Game/Continue, evento 185). Nome antigo do despachante corrigido no Ghidra. Abertos: GS 0x0805C8EC e 0x0805C950.
- SC.md: comandos de cena (`Event_RunSceneCommandSequence`, switch 0x00..0x20). Usa MN.md como base; abertos: nome dos menus 0x10..0x16.
- MU.md: catálogo estado -> menu -> função -> tela. Próxima rodada: identificar as telas marcadas "pendente".
- ST.md: tabelas de passos de cena (0x083ACE54, 0x083ACEF8, 0x083AFCA0, 0x083AFD08, 0x083D212C).
- ST2.md: continuação de ST (passo 0x0800EB34, modo em 0x03002D59). Corrige/complementa ST.md.
- TX.md: textos pré/pós-batalha e a tabela logo após `gActionScriptOpcodeHandlerTable`.

Menus e telas:
- FM.md: menu de campo (abre no mapa com B) e a bolsa; PARCIAL.
- FM2.md: continuação do menu de campo (gravador de `gamestate_menu=2` ainda aberto). Complementa FM.
- OP.md: menu de opções (`Options_ThreeRowMenuStep`); nome do A6 em dúvida.
- IN.md: itens, loja, inventário e dinheiro.
- IN2.md: loja e key items (continuação de IN).
- KI.md: key items (bolsa, uso, eventos).
- DB.md: menus de debug (Motion, opções, Item, Mdlpic).
- DB2.md: telas de debug restantes (Tinpet, Item, Face, Mdlpic, Rnd-tk, Battle, Sound). Corrige nomes do sync para estas telas.
- SV.md: sistema de save; inventário provavelmente lido errado pelo extrator (ver nota da seção).
- LK.md: link (cabo, SIO). Troca de peças por link não achada.
- LK2.md: continuação do link (menu 10, menu 0x15, comando 0xEF00).
- LK3.md: comandos de link (tabela 0x080AFCC0), passos do menu 0x15. Corrige/complementa LK2.

Som e gráficos:
- SN.md: motor de som (tracks, prioridade de SFX, envelopes).
- SN2.md: continuação de SN (comandos FB e outros; tabela de comandos).
- GF.md: catálogo de gráficos (carregadores e assets).
- GF2.md: continuação de GF (chamadores de `decompress_and_store` e `LoadTilesWithMalias`).
- GF3.md: catálogo de gráficos parte 3 (overworld, retratos, menu de campo, status). Correção de endereços de GF2.
- GF4.md: catálogo de gráficos parte 4 (título, grade de peças, batalha, número, peça). Corrige 0x087BB7E0 (sem cabeçalho LZ77).
- IC.md: ícones de peça e medalha, descritores do sprite grande de peça.
- IC2.md: continuação de IC (struct de tipo do sprite grande, ícone de medalha/perna). Corrige a forma OBJ 64x32 aberta.

Opcodes de script (mapa e eventos): ver seção 2.

## 2. Mapa (movimento, colisão, warps, sprites, eventos, progresso)

Mapa e movimento:
- MP.md: formatos de dados do mapa como o jogo lê (LZ77, tilesets, mapas). Ver GF3 para o tileset (0x083F8A80).
- MP2.md: pendências do mapa (animação de tiles, `Map_UpdateBackgroundAnimations`).
- OW.md: sistema de mapa (passo, colisão, `Overworld_CheckStepAllowed` 0x08057D80, warps, encontro). CU.md §3c corrige o passo do objeto descrito aqui (§5).
- OW2.md: continuação (tabela de velocidade, tabela de warps 0x08412E60, escala do passo). Abertos: quem põe 0x03006D3A.
- CS.md: sprites de personagem do mapa (carga dos blocos). PARCIAL.
- CS2.md: continuação de CS (paletas dos personagens).

Eventos e objetos do mapa:
- EV.md: máquina de eventos do mapa (visão geral). PARCIAL; abertos na §7. Corrigido pelo EX.md.
- EV2.md: uso real dos opcodes de evento (varredura da ROM). Corrigido em parte por E2F.md.
- E2F.md: tamanho real do opcode 0x2F e 0x7D. N do 0x2F ainda heurística (confirmar no emulador).
- EX.md: divergências do extrator de eventos (sem alteração); entrada do loader 0x080513C8.
- EX_contagem.csv: contagem de opcodes por mapa (apoio de EX.md).
- HE1.md: handlers de opcode de evento 0x00..0x22.
- HE2.md: handlers 0x23..0x45. Complementa HE1 (0x23..0x45).
- HE3.md: handlers 0x46..0x68 (0x51 "jump" sem avaliar no PC).
- HE4.md: handlers 0x69..0x88; contagem de argumentos por `Script_ReadBytes`.
- H.md: handlers chamados por ponteiro (despachantes e tabelas de eventos). Corrige nota de SND11 (0x083D222C).
- HC.md: tabela de continuação dos opcodes de evento (0x083D2630 + 4*op).
- HC1.md, HC2.md, HC3.md, HC4.md, HC5.md: continuações de opcodes de evento por lote (FIX.md cita HC5 para 0x76, 0x7A, 0x7F).
- HN6.md, HN7.md: handlers de opcode de evento 0x24..0x41 e 0x72..0x76 (parte de HN, mas de eventos).
- EN.md: de onde vêm os times inimigos (`gMapEncountersExtra` -> templates). Ponte entre mapa e Robattle; recompensa em aberto (§6).
- PR.md: progresso da história (capítulo 0x03005FDF, flags de evento, campo de 3 bits).
- DT3.md: tabelas `gMapArea`, `gMapAreaB`, `gBotNames`, `gTechniqueNames` e `gMoldTable` (parte de DT).
- DT.md: inventário de tabelas de dados (peças, medalhas, técnicas, mapa, música). PARCIAL.
- DT2.md: tabelas de handlers pendentes (ActionScript e evento). Corrige nota de SND2/SND11.

Mapa: tabelas e tabelas auxiliares nos arquivos de dados acima.

## 3. Robattle (batalha)

Fluxo geral e início:
- BT.md: visão geral da Robattle (do início ao fim); liga R1, FL7, JT, HN9, HN10, SD, FH e A6.
- BT2.md: fechamento das lacunas do fluxo da Robattle (entrada menu 7/8; quem indexa 0x083AD190/0x083AD10C em aberto). Complementa BT.
- FI.md: tabela completa do `fight_information` (0x03001800). Legenda de confiança; CONFLITO entre documentos listados.
- FI2.md: tabelas 2 e 3 do fight_information (contexto 0x03000440 e bloco E 0x030057E0). Complementa FI.
- FH.md: helpers de batalha chamados por muitos handlers (`Fight_RunSubstateDispatch`). Mapa código -> roteiro ainda aberto.
- A6.md: `fight+0xA6` -> roteiro de mensagem (`gTextGameMessages` 0x083C6744; formato dos caracteres e FA/FC/FD/FF). Fecha o mapa de FH.
- MS.md: motor de mensagens e texto (`Text_RenderGlyphStep`). Complementa A6.
- MS2.md: pendências do motor de mensagem + handlers 9..22. Corrige alvo de FF 1/4 (MsgFF_AdvanceOtherObj).
- TP.md: escolha do time antes da Robattle (até 3 bots); escolha aberta.
- PB.md: montagem dos bots do jogador no início da luta e retorno ao save; caller 0x08004A2A não achado.
- SD.md: tabelas dos despachantes de sub-estado de batalha (`Fight_DispatchOnSubstateAA_*`), visão geral.
- JT.md: saltos indiretos não recuperados pelo Ghidra (1ª rodada); J:0x02, G:0x02 em aberto.
- JT2.md: despachantes por tabela/índice (0x0803FB3C, 0x08040974, 0x08071630).
- JT3.md: despachantes por tabela (continuação de JT2; 0x0805DB20 sem nome).
- JT4.md: saltos indiretos ainda abertos; corrige retorno de FL5 (Screen_IntroTimeline 0x0806DAA4).
- JT5.md: saltos indiretos restantes; corrige FL5 (Anim_SlotScriptTick 0x0803C680).

Despachantes de passo da batalha (`Fight_DispatchOnSubstateAA_<letra>`), em ordem:
- HA1.md: handlers dos despachantes (lista HA1_lista). Base dos despachantes; HA2..HA8 detalham.
- HA2.md: despachantes E, F, G, H, AO. Corrigido em parte por JT (E05 e G02).
- HA3.md: 42 handlers decompilados e renomeados (despachantes J..); redo que resolveu saltos de JT.
- HA4.md: 42 entradas; +0xAA de fight_information = action_sequence_state. Base do HA7.
- HA5.md: despachante T (parcial: 9 de 42).
- HA6.md: despachantes Z, AA..AE.
- HA7.md: despachantes AE, AF, AG, AH. Corrige "tabela 0x080240B4" do R1.md (é o slot).
- HA8.md: tabela de despachantes AH..AP (42 entradas). 42 entradas batem com HA8_lista.
- AD.md: despachantes AF, AG, AM..AQ (passos já cobertos em HA/HN; 35 renomeados).
- SU.md: despachantes S, U, V, W, X, Y, AC, AD.
- SU2.md: passos pendentes de SU decompilados (S 0x00; ponteiro 0x0802155C não recuperado).
- HN1.md, HN2.md (AR..AV), HN3.md (AV..BA), HN4.md (BA..BG), HN5.md (BG, BH), HN8.md (BL..BP), HN9.md (BP..BR), HN10.md (handlers que NÃO são do BR; passos 0x41..0x66 da cena 8). Ler HN10 antes de citar BR.
- HN6.md: 3 handlers de batalha (AG e compartilhado) + 38 de evento. Ver seção 2.
- HN7.md: 18 handlers de batalha (BK, BL) + 22 de evento.

Ações, técnicas e roteiros:
- AS1.md: opcodes do roteiro de ação 0x01..0x35 (parcial; tabela `gActionScriptOpcodeHandlerTable` 0x083AF180). Continua em AS3a/AS3b.
- AS2.md: opcodes 0x36..0x6A (wrappers 0x08034524+). Pendências 0x47 e 0x4A (ver AS3c).
- AS3a.md: opcodes 0x01..0x1A. Correção de LEN (usada por DM §5).
- AS3b.md: opcodes 0x1B..0x35. Corrige LEN (LEN[0x02]=1, LEN[0x03]=4, LEN[0x13]=0) e DM §5 e AS2.
- AS3c.md: handler -> despachante -> efeito para 0x36..0x6A. Fecha 0x47 e 0x4A (provável no campo).
- AB.md: despachantes B..G do roteiro de ação (0x04, 0x05, 0x07, 0x08, 0x09, 0x19, 0x1A) e pendências do AS.
- AS_criar.txt: não é documento (lista de funções a criar; apoio).
- DM.md: fórmula de dano, ROS, acerto e crítico ponta a ponta. LEN corrigido por AS3b e TQ2.
- TQ.md: técnicas de peça 0..59 (setup, validação, roteiro). Conflito: 20 técnicas compartilham o início 0x083C6A04.
- TQ2.md: roteiros 0..59 como um fluxo linear; o conflito não se separa pelo roteiro. Corrige LEN do DM §5. Fluxo em TQ2_fluxo.txt.
- TA.md: onde a técnica muda o comportamento (PARCIAL); corrigido por TA2 e TA3 (quem grava bot+0xA8).
- TA2.md: leitores de bot+0xA8 (técnica ativa), decisão por técnica. Corrige TA.md (Op1B não grava).
- TA3.md: quem grava bot+0xA8 = `Fight_SelectMedaforceActionSource` 0x08015668. Risco: ação normal pode não gravar.
- AN.md: animação (motion) dos bots grandes em batalha (`MotionPlayStep`, `gMoveScriptTable`).
- HO1.md: handlers do ObjectScript (roteiro de objeto da batalha). Valores 0x80+ são rótulos, não opcodes.
- HO2.md: ObjectScript opcodes 0x1E–0x3A; 19 de 29 endereços em aberto.
- AI.md: IA dos bots (escolha de ação e alvo). Correção de R0a (10/30/60).
- AI2.md: fechamento da IA: tabela de ordens de fallback 0x083AD470 fechada; driver 0x08012B94 (chamador em 0x080322C8 aberto).

Medaforça:
- MF.md: Medaforça de ponta a ponta (medidor B+0xC1, escolha, roteiro). EM ANDAMENTO; efeito por id em aberto (§9).
- MF2.md: efeito de cada Medaforça (PARCIAL); pendentes 0x41..0x6A e 0x06..0x35.
- MG3.md: medidor de Medaforça (B+0xC1) e fim de roteiro. Corrige o quanto o medidor desce.
- MD.md: medalhas (`gMedals` 0x083B62D8) e fórmula ROS -> dano (ler com DM). Nota: 0x35 confirmado no disasm; o mtbe diz 0x05 (errado).
- MD2.md: significado dos bytes do descritor de Medaforça (`desc6` 0x083B7E54). PARCIAL.
- MD3.md: leitores de bot+0xAA e bot+0xAD (ação normal x Medaforça). PARCIAL.
- EN.md: (ver seção 2) times inimigos e template da batalha.

Estado, dano e peças:
- SS.md: sistema de status da Robattle (pendente de status: bloco +0xC9..+0xCC; AutoRecover +0xC4; proteção AW +0xCF). Aberto: quem liga +0xCF.
- DF.md: derrota do jogador e bloco do bot (bot+0xBA = status). Corrige BT ("+0x40 = b...").
- DF2.md: derrota de fato e peças dos inimigos; concluído com pendências.
- R1.md: funções 0x08016288..0x08017F9C (PARCIAL, peças, medidor e reparo). Ver também R1a/R1b.

Recompensa e fim de batalha:
- RW.md: recompensa, XP e fim de batalha (sorteio de peça, `Fight_TryAssignBotPartByRoll` 0x080121CC).
- XP.md: experiência e nível das medalhas (XP = soma do campo +6 das peças inimigas). Correção de especialidades (+0x20..+0x27). Aberto: chamador de `Fight_AwardPartExperience`.
- EN.md (seção 6): recompensa e quem põe E+0xC2D na história em aberto.

Interface (HUD e telas de peças):
- HD.md: HUD da Robattle (interface, barras, descida animada). Pendentes na última seção.
- HD2.md: pendências da HUD (complemento; corrige endereço de HandleSelectedMedabotPreviewCursorInput, que é 0x080183A4).
- PS.md: telas de peças, montagem e grade de Tinpets (grade 3x3; regras em 0x080183A4).
- PS2.md: continuação de PS (quem lê 0x083AC880). Corrige "cursor de luta" em R1.md:570.

Movimento e cena de batalha (corte, ataque):
- AN.md: já listado na seção Ações, acima.

## 4. Funções por faixa de endereço e lacunas (sem subsistema fixo)

Faixas de funções (lidas no Ghidra, em ordem de endereço). Quase tudo abaixo mistura subsistemas; use o índice do mtbe.csv para achar.
- R0.md (faixa 0, grande; funções de luta e boot). R0a.md (25 endereços da luta: bots, peças, seleção de ação). R0b.md (0x080156F8..0x08016154).
- R1.md (faixa 1, PARCIAL, 61 de 301, continuar em 0x08018074). R1a.md (0x0802C644..0x08034E94). R1b.md (0x08034EB8..0x08035880).
- R2.md (faixa 2; 302 endereços; movimento, script, animação). R2a.md (0x08043B7C..0x080455E0). R2b.md (0x08045680..0x08047E08; `Overworld_StartMapTransitionEffect` não é transição).
- R3.md (faixa 3, 141 KB; uma seção por função). Ver CH3 para revisão dos chutes.
- R4.md (faixa 4, 127 KB). Tem nota de correção sobre o caso 10/4 (ver o arquivo).
- FL.md: "FUN_" que sobraram (lista geral, 434 candidatas). FL1..FL10: lotes dessas funções (cada lote tem sua _lista.txt). Ler só o lote do endereço procurado.
- FR1.md, FR2.md, FR3.md: pendências de FL2..FL8 (rodada de fechamento). FR2 é de FL4; FR3 de FL5, FL6, FL8.
- FN1.md (< 0x08030000), FN2.md (0x08030000..0x08058000), FN3.md (>= 0x08058000). FN2 cita Fight_RollStatusRowRandomEntry_Wrapper.
- G.md: lacunas espalhadas (23 endereços).
- N1.md: funções de 10 alvos de tabelas de ponteiro. N2.md: funções da tabela 0x08483634 e cópias.
- Tabelas de ponteiro: H.md (ver seção 2) e DT*.md (ver seção 2).
- TX.md (ver seção 1).

## Abertos importantes (15 maiores pendências que pedem emulador ou análise)

Ordem por impacto no entendimento. "E" = pede emulador (mGBA/replay); "A" = análise estática (Ghidra/ROM).

1. Conflito das 20 técnicas com o mesmo início de roteiro (0x083C6A04): TQ.md §5 (A). TQ2 mostra que o roteiro não separa.
2. Driver da CPU 0x08012B94 e seu chamador 0x080322C8 (breakpoint no emulador): AI2.md "Pendente" (E).
3. Quem chama `Fight_AwardPartExperience` (vitória ou derrota) e quem zera fi+0x16C: XP.md "Pendências" (E).
4. Derrota: se o resultado 2 (T+0x9C) impede a recompensa; onde o resultado é gravado: RW.md §6 (E).
5. Quem grava won_bodypart_type (0x03001968) e won_bodypart_id (0x03001969) na história: EN.md §6 (E).
6. Opcode 0x2F: N (tamanho da tabela) é heurística; confirmar com um 2F de N=4 no emulador: E2F.md §6 (E).
7. Quem liga +0xCF (proteção) no bloqueio de status e a regra de "imune" por variante: SS.md §7 (E).
8. Escritor de 0x03006D3A (índice que faz o jogador correr): OW2.md "Pendente" (E).
9. Quem grava +0x32 do pisca e a cadeia FUN_08048684 -> FUN_08048210 (pose em +0x30): CU.md §5 (A).
10. Quem grava 0x03002D59 (slot 4 +0x29) e qual evento põe 0x0300137C = 66 (entrada da tela de detalhe): ST2.md §6 (E).
11. Se a ação normal não grava bot+0xA8: leitores AS/AV/AW/BF/BE (técnicas 4..40) lendo valor antigo: TA3.md §4 (E).
12. Escolha de até 3 bots antes da Robattle (candidatos 0x0806B8CE, 0x0806C7E4): TP.md §2 (A/E).
13. Troca de peças e medalhas por link (não achada; busca por "Trade"/"Serial" vazia): LK.md §6 (E).
14. Quem liga/desliga "removido" nos eventos (opcodes 0x12 e 0x13): PR.md §4 (A).
15. ObjectScript: 19 de 29 opcodes (0x1E..0x3A) sem leitura completa: HO2.md "Em aberto" (A/E).

Observação: SN.md (timer PCM e DMA) e MN/TL (GS 0x0805C8EC, 0x0805C950) ficaram de fora da lista de 15 por serem mais de infraestrutura; estão em SN.md "Pendente" e TL.md §8.

- (2026-10-14) Emulador: watch de escrita em 0x03000649 (Conceal, bot 0) durante a técnica 37 e em 0x03001828 (Scout) durante a 36; TR2..TR5 esgotaram a análise estática.
- (2026-10-14) Emulador: watch de escrita em GS 0x0300137C ao entrar numa Robattle (quem grava 0x23) e ao sair dela (volta ao mapa); BT3..BT6 esgotaram a analise estatica.
- (2026-10-14) Conceal/Scout: escritores achados no AS6 (Medaforces 0x22/0x24 gravam bot+0xB9 e fight+0x28). Falta ver se as tecnicas 36/37 tambem gravam (emulador ainda util para duracao/limpeza).
