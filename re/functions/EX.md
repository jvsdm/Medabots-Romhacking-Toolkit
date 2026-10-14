# EX - divergencias do extrator de eventos (analise, nada alterado)

Fonte: agente GH-EX (2026-10-14). Contagem por mapa em EX_contagem.csv.

- Entrada do loader: Map_LoadEntitySpawnsForCurrentMap = 0x080513C8 (0x08051416 e miolo; EV.md corrigido no CSV).
- Ponteiros: NAO ha tabela por grupo. O laco de 0x20 passes (grupo 0..31, byte +5 == passe) relê a mesma lista;
  ponteiro = u32[0x08483234 + 4*gCurrentMapId] (0x03005FDE). O extrator le o mesmo ponteiro: perda 0.
- Filtro do jogo por registro: grupo < 32; mascara (u16 +6 >> capitulo) & 1, capitulo = byte 0x03005FDF
  (capitulo >= 16 nunca carrega; o C# IsVisibleInChapter devolve true nesse caso: corrigir); flag de evento feito
  EventFlag_IsSetByPackedId 0x080531E0 (id = u16 +2 & 0xFFF; par -> bit 7, impar -> bit 3 de [0x030057D8 + ((id&0xFFF)>>1)] (EF)).
- Modo E+0xCBA (0x0300649A): 0 e 1 aceitam o mesmo conjunto; 2 so regrava entradas antigas com bit 7.
- Mapas 0..191: 2188 registros, 2116 ids; no capitulo 0 o jogo carrega 196 (o extrator inclui ~1920 a mais).
  Mapas 192+ nao sao listas validas.
- Proposta (nao aplicada): people_event_ids(rom, map, chapter=None, done=None) filtrando por grupo/mascara/flag;
  default chapter=None mantem o comportamento atual (superset).
