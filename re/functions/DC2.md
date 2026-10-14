# DC2: aplicação dos vereditos do QA3 (GH-DC2, 2026-10-14)

Sem commit, sem emulador. Leitura da ROM só com python -I. README.md e mtbe.csv NÃO editados (Opus faz).
DC.md não existia: nada a não duplicar.

| Arquivo:linha | Mudança | Fonte |
|---|---|---|
| HD2.md §2 (título e linhas 58-59) | cursor 3x3 passa a ser "da tela de prévia (escolha de peça)"; "da luta" retirado | QA3 item 1 |
| HD2.md:29 | "cursor da luta" -> "cursor 3x3 da tela de prévia" (endereço 0x08018534 NÃO conferido, ver abertos) | QA3 item 1 |
| HD.md:119 | "Cursor 3x3 da luta" -> cursor 3x3 da tela de prévia, chamador 0x08013E84 | QA3 item 1 |
| CF.md:10 | "E o cursor da previa, nao o da luta." -> "E o cursor 3x3 da tela de prévia" | QA3 item 1 |
| DM.md:106-107 | LEN[0x03]=4, LEN[0x13]=0, LEN[0x02]=1; sai "antes dizia..." e "confirmado só para os bytes lidos" | QA3 item 2 |
| HC1.md:4 | gate = E+0xC88 = 0x03006468; E+0xC89 = 0x03006469 = byte de estado | QA3 item 3 |
| LK.md:112-113 | pendente = par 0x03006468 + 0x03006469 (era só 0x03006469) | QA3 item 3 |
| GF4.md:17 | destino de 0x087BB2A0 = VRAM 0x06007000 (pool 0x0806D86C); trace IN1 (0x06006D00) não usado como prova | QA3 item 4 |
| GF3.md:127 | 0x087B67B0: destino NÃO é 0x06008000 nem 0x06000000 por literal; VRAM = r1 do chamador; 0x06008000 é destino de DMA3 (pool 0x0806D850) | DC2 (achado extra) |
| FI.md:59 | +0x0D2: "bits0..6 = índice" -> peça 1/2/3 mapeada para 2/3/1 em BK_0E; "valor direto" marcado como não conferido | QA3 item 6 |
| FI.md:132 | +0x0D2 = mapeamento HN7 confirmado só em BK_0E; "valor direto" retirado | QA3 item 6 |

## Achado extra (item 4): GF3.md:127
Pool e código de Gfx_LoadScreenLZ77Assets 0x0806D7A0 (lido na ROM, `push` na entrada):
- 0x0806D7A2: `ldr r0 <- 0x0806D844` (= 0x087B67B0), `bl bios_lz77` 0x0807CCD8 (0x0806D7A8). O r1 não é carregado por literal: é o argumento do chamador.
- Literais próximos: 0x0806D844 = 0x087B67B0, 0x0806D848 = 0x040000D4 (registro DMA3 SAD), 0x0806D850 = 0x06008000 com 0x0806D854 = 0x84000090 (controle DMA3).
- Não há 0x06000000 na janela 0x0806D7A0..0x0806D900 da ROM. Então a alternativa (0x06000000) também não é o par.
Conclusão: 0x06008000 é o destino de DMA, não do LZ77 de 0x087B67B0. O destino desse LZ77 vem do chamador. Não é possível fechar o valor só com o literal pool; falta ver o chamador (fora do escopo desta tarefa).

## Abertos (para o Opus)
- HD2.md:29 cita `HandleSelectedMedabotPreviewCursorInput` em 0x08018534, mas a função está em 0x080183A4 (QA3 não cobriu). Confirmar ou corrigir o endereço.
- PS.md, PS2.md e R1.md:570 ("cursor 3x3 da tela de luta") ainda chamam o cursor de "luta" (QA3 pediu conferir PS/PS2 se citarem o cursor). Não editados.
- HD2.md:71 e :105 ainda dizem "cursor de escolha de peça/alvo não achado": o cursor 3x3 da prévia é o de escolha de peça, mas o cursor de alvo continua aberto.
- LK.md:18 diz que Event_Op76 grava `1` em 0x03006469. QA3 não cobriu isso; não verificado.
- DM.md:104 e 107 não mudei além do QA3.
