# Texto (traducao PT-BR)

| Arquivo | Uso |
|---|---|
| `dump_text_v3.py` | `python dump_text_v3.py ROM.gba game_text.json`: extrai as 50 tabelas de texto (mantem traducoes ja feitas) |
| `insert_text_v3.py` | `python insert_text_v3.py ROM_toolkit.gba game_text.json ROM_saida.gba --reflow`: grava as traducoes |
| `medtext.py` | tabela de caracteres, larguras da fonte (com a extensao do Toolkit), medida e quebra de caixas |
| `glossario.md` | termos e nomes usados na traducao |

Fluxo:
1. Toolkit: abrir a ROM (E), aba Font com "Use extended font" ligado, Export ROM.
2. `insert_text_v3.py` sobre essa ROM com o JSON traduzido.

Regras de texto (as mesmas do preview de mensagens do Toolkit):
- caixa de dialogo: 208 px de largura, 2 linhas; `<NL>` quebra a linha, `<NB>` abre outra caixa (o retrato continua);
- o jogo mede cada palavra (com o espaco seguinte) e, se passar de 208 px, pula de linha sozinho; da linha 2 vai
  para uma caixa nova depois do botao. `--reflow` faz essa quebra antes, para as caixas ficarem previsiveis;
- `<MEM:n>` (nome do jogador etc.) conta como 8 letras W (64 px);
- so existem os caracteres de `medtext.CHARS` (sem `;`, aspas curvas, travessao, `+`); `medtext.clean` troca
  pontuacao tipografica pela do jogo;
- tags (`<PORTRAIT:a,b,c>`, `<END:n>`, `<MEM:n>`, `<I>`, `<?><0xNN>`...) ficam iguais ao original.

O JSON com o texto do jogo nao vai para o repositorio.
