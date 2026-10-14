# RD7 - revisao de rotulos de dados/tabelas (RD7_lista.txt)

## Trocas de nome (rename_data no Ghidra)
- 0x08785FB4: gGfx_Shop_PartSelect_Obj -> gGfx_PartSelect_Obj (usado tambem pelo BattlePanel; GF3/GF4).
- 0x08765EC4: event_operation_list -> gEventDb_HeaderPad (zeros, sem referencia; nao e lista).
- 0x084C8034: nome do dado = gObjTextFontGfx (Malias 'Le' +2=0x09C0). A linha label gGfx_Obj_Malias_09C0 e duplicata: remover do mtbe.csv.

## Kind corrigido (label -> data/table)
- data: gFontPalette, sPartScreenObjPaletteBlockA/B/C, sCompositeBattleSpritePaletteSet, gPartListTilemap, gTextDump, gStarterData.
- table: gEventDb (header 2288 eventos), gEventPointerTable (2288 x u16).

## Achados
- Linhas 0x103C6A04/57/59/5B/65 estao com endereco invalido (0x10..., fora da ROM). Provavel typo de 0x083C6A..; bytes confirmam roteiro de action-script em 0x083C6A04 e vizinhos. 0x083C6A04 ja e gTechniqueScriptStream_Generic (RD5): AntiSea e alias, nao renomeado.
- gMiniSpritePalettes: mtbe diz 128 B, GF2 diz 0x40 words. Conferir.
- gStarterData: sem xref; Bot_InitStarterRecord (0x08044B58) escreve os bytes direto, nao le esta area. Nome mantido como chute.
- Titulo: 0x087BB7E0 sem cabecalho LZ77 (anomalia ja registrada em GF4); destino VRAM divergente.

## Em aberto
- Trocas de mtbe.csv (kind e remocao da duplicata) ficam para o Opus; este agente nao edita mtbe.csv.
