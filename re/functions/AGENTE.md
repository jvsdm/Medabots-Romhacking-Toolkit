# Resumo para agentes (ler só isto)

Medabots Metabee (EU), GBA Thumb. Ghidra MCP aberto: NÃO fechar nem salvar (outros agentes em paralelo).
ROM só leitura: `D:/claude-brain/Medabots/Medabots - Metabee (Europe).gba` (offset = addr - 0x08000000).
Pasta: `D:/claude-brain/Medabots/git/Medabots-Romhacking-Toolkit/re/functions/`.

## Regras
- Escreva só os seus arquivos `XX.md` e `XX_simbolos.csv` (e `XX_criar.txt` se pedido). Sem commit, não edite
  mtbe.csv, sem emulador, não copie texto do jogo.
- Buscas em `../symbols/mtbe.csv` e `D:/claude-brain/Medabots/pc/research/README.md` só com `python -I` (nunca
  leia inteiros; o grep do shell está quebrado).
- Economize contexto: `get_function_by_address` antes de decompilar; não decompile o mesmo endereço duas vezes;
  pare perto de 120k de contexto num ponto limpo (o que já está no CSV vale).
- Escreva o CSV À MEDIDA QUE AVANÇA (append por endereço), não só no fim.

## CSV
`addr,nome,kind,comentario` — kind = `func_thumb` (código) ou `label`/`data`/`table`.
Comentário começa com a confiança: `confirmado` (efeito visto no decompile), `provavel` (nome deduzido do
contexto), `chute`. Se trocar um nome existente, ponha `(antes: X)`.
Bytes começando com `xx B5` (push {...,lr}) = início de função, mesmo que o Ghidra mostre halt_baddata.

## Nomes
Inglês, `Subsystem_VerbObject` (Fight_, Event_, Map_, Overworld_, Menu_, Parts_, Medal_, Snd_, Gfx_, Obj_,
Save_, Text_). Prefixos de batalha: `Fight_DispatchOnSubstateAA_<Desp>_<Passo>_<Verbo>`.

## Endereços-chave
- Bot de batalha `0x03000590 + i*0x100`. fight_information `0x03001800` (+0xAA passo, +0xB5 feito, +0xA6 código).
- ctx de tela `0x03000440`; GS `0x0300137C`; gMainState `0x0300178C`; gGameStateMenu `0x03002B8C`.
- Bloco de evento E `0x030057E0`; flags `0x030057D8`; capítulo `0x03005FDF`; dinheiro `0x03002B70`.
- gObjectSlots `0x03002C30` (0x40 B). Medalhas `0x03000BE0` (30 x 0x40). Inventário de peças `0x03004C40`.
- Utilitários: Mem_CopyBytes 0x0803F63C (src,dst,n); LZ77 0x080009BC; div 0x0807CD2C; rand 0x080424B8;
  call_r1 0x0807CCF4; StateMachine_DispatchStep 0x0803FB3C; textos de batalha gTextGameMessages 0x083C6744.
- "Could not recover jumptable" seguido de `pop {rN}; bx rN` é só retorno, não tabela.

## Relatório final (curto, ≤15 linhas)
Quantos feitos, quantos renomeados, divergências com nomes antigos, o que ficou aberto.

## Opcodes de evento (Event_OpXX)
O handler primário só lê argumentos e liga um pedido/flag; o efeito real fica na continuação (tabela HC
0x083D2630). Não renomeie Event_OpXX pelo corpo do primário: mantenha o nome do opcode e anote no comentário.
