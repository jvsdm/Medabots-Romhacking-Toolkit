# RD8 - revisao de rotulos de dados (roteiros de tecnica)

Escopo: 36 linhas de `RD8_lista.txt`, todas kind=data. Nenhuma era label de codigo.

## Trocas
- Nenhum nome trocado. Nenhum rename_data feito no Ghidra. Os nomes `gActionScript_<Tecnica>` batem com os ids
  de tecnica confirmados (ver abaixo), entao foram mantidos.

## Achados
- Enderecos da lista estavam com base errada: `0x103C6xxx`. A ROM so existe em `0x08000000`+. Os bytes confirmam
  `0x083C6xxx` (ex.: 0x083C6AA4 = `06/31 01`, roteiro de Conceal). O CSV usa `0x083C6xxx`.
- Ponteiros: `gTechniqueActionScriptPtrTable` 0x083C6CA8[id] aponta para cada rotulo (ids 22..33, 36..59).
  Entao id = indice da tabela, nao a ordem da lista.
- Cada roteiro termina em 0x01 (bytes conferidos no fim de cada segmento).
- HealChange (id 59) tem 2 bytes (3F 01), nao 278: o segmento para em 0x083C6B92, que e o inicio do roteiro de
  medaforca (ptr[80]). O fim 0x00 em 0x083C6CA6 fica depois dessa regiao.
- Cinco roteiros sao identicos (Change, ItrpChange, AttkChange, DEFChange, HealChange = `3F 01`); NoEscape e
  NoDefense sao `06 08 39 01`. A distincao entre essas tecnicas deve estar em setup/validacao, nao no roteiro.
- Divergencia: Strengthen = id 53 (GH-DM), mas G1_damage.md fala em "tecnica 5" para strengthen. Nao resolvido;
  deixado como confirmado por GH-DM e marcado no CSV.
- Sem doc para os ids 22..35, 38..50, 54..59: nome mantido pela ordem da tabela e pelos ids ancora (36, 37, 51..53).
  Confianca `provavel`.
