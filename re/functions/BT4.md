# BT4: nomes dos passos sem nome, quem grava GS=35, saida da Robattle

Ferramenta: Ghidra MCP (sem salvar). Varreduras: python -I na ROM original. Relacionado: BT3.md (tabela GS 0x083AD080).

## (1) Passos sem nome da tabela GS

Os 5 endereços NÃO são funções no Ghidra (decompile e get_function devolvem "No function found"). Os bytes NÃO começam com push (xx B5); são trechos leaf de 3 a 6 instruções, terminando em `bx lr`, e não há entrada no BT4_criar.txt. Por isso não há rename no Ghidra; os nomes ficam em BT4_simbolos.csv.

| endereco | idx na tabela | nome proposto | efeito (confirmado por leitura do Thumb) |
|---|---|---|---|
| 0x08014AB4 | 147, 163, 166, 168 | Fight_CountWaitTimerUp16ThenAdvanceStep | u16 gCtx_WaitTimer (0x030004C6): se 0x10 faz GS+=1; senao timer+=1 |
| 0x08014CE4 | 153 | Fight_SetGsTo40Step | GS = 0x40 |
| 0x08014D84 | 169 | Fight_IncrementGsStep | GS += 1 |
| 0x08014E08 | 175 | Fight_ClearLinkStateField6SetGs45Step | u16 0x03005016 (gLinkStateBlock+6) = 0; GS = 0x45 |
| 0x08014E54 | 174 | Fight_WaitTimerDownThenAdvanceStep | se gCtx_WaitTimer==0 GS+=1; senao timer-=1 |

Obs.: 0x08014AB4 e 0x08014E54 são o par contador-para-cima / contador-para-baixo do mesmo timer (ctx+0x86).

## (2) Quem grava GS = 35 (0x23) para entrar na Robattle: NÃO ACHADO

Varreduras feitas (scripts em scratchpad, descartados):
- Literal pool com 0x0300137C: 200+ ocorrências. Só ldr-literal + str com mov rX,#0x23 na janela de ±40 meia-palavras: 4 candidatos, nenhum grava 0x23 em GS: 0x0800D0AA e 0x0800D5F6 têm mov #0x23 numa rotina de DMA/paleta, sem store para GS com 0x23; 0x08031576 usa 0x23 como argumento de bl; 0x0805D800 grava 0x23 em ctx+6 (strh [r1,#6]), não em GS.
- Controle: o mesmo scanner acha os escritores conhecidos de GS=0x40 (0x08014CE4) e GS=0x45 (0x08014E08), então o método funciona.
- Todos os 757 escritores de GS na ROM (ldr-literal + str na janela de 14 meia-palavras): nenhum com imediato 0x23.
- Escritores com base por offset (mov #0x23 + str [base,#off] com base vindo de literal): zero casos resolvem para 0x0300137C.
- Funções curtas que gravam GS com o valor vindo de argumento (96 candidatas): nenhum bl precedido de mov rX,#0x23 em até 12 meia-palavras.

Hipóteses não testadas: valor de GS vindo de tabela/byte de ROM; `movs` + soma ou `mov rX,rY` antes do store; escrita por rotina genérica de estado chamada com registrador computado. Próximo passo sugerido: achar quem chama Fight_BattleSceneInit 0x08013B8C (idx 35) e ver o ponto de entrada antes do primeiro passo.

## (3) Saída da Robattle para o mapa: parcial

- Laço: Battle_FrameDispatchOrExit 0x08012970 (decompile confirmado): chama Battle_CheckExitShortcut; se 0 faz CpuFastSet quando *0x03000430 == 2 e chama table[GS].
- Battle_CheckExitShortcut (decompile confirmado; endereço contendo 0x0803EEE0, Ghidra não tem entrada exata neste ponto). Condição: byte gScreenCtx(0x03000440)+0x89 == 1 E (u32 gLinkStateBlock(0x03005010)+0x1c & 0x00FFFF00) != 0. Efeito: gMainState(0x0300178C) = 0; gGameStateMenu(0x03002B8C) = 10; GS(0x0300137C) = 0x10. Retorna 1.
- Escritores de gMainState (0x0300178C) na faixa de código de batalha 0x08012900..0x08015000: nenhum (varredura de 77 escritores na ROM). O único dentro da faixa do exit é o do shortcut acima.
- Ainda não se sabe a saída normal (fim de resultado sem atalho). Os passos de resultado são 64-66 e 68; a última transição não foi fechada. Pendente: qual passo grava GS final e de onde vem a troca de estado do mapa após o shortcut.
- Observação: GS=0x10 cai no passo idx 16 (Fight_RunEntryAnimStep 0x0801303C); ou seja, o atalho reentra na sequência de entrada em vez de sair direto. Confirmar antes de usar.

## Lacunas abertas
- (2) quem grava GS=35 para entrar na Robattle: não achado com os métodos acima.
- (3) saída normal da batalha para o mapa: não fechada.
- Nomes de (1) só existem no CSV (sem função no Ghidra). Se o Ghidra precisar deles, criar a função antes.
