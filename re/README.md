# Engenharia reversa - Medabots Metabee (E)

Simbolos e scripts do Ghidra versionados em texto. O projeto do Ghidra (`.gpr`/`.rep`) e a ROM nao entram no git.

| Caminho | Conteudo |
|---|---|
| `symbols/mtbe.csv` | enderecos conhecidos: funcoes, tabelas, variaveis de RAM, registradores de IO |
| `ghidra/MedabotsSetup.java` | prepara o programa: blocos de RAM, limpeza de codigo falso, aplica o CSV |
| `ghidra/MedabotsExportSymbols.java` | grava os nomes dados no Ghidra de volta no CSV |

Enderecos no CSV sao de memoria GBA (`0x08...` = ROM, offset de arquivo + `0x08000000`).

## Formato do CSV

`address,name,kind,count,comment`

| kind | Efeito |
|---|---|
| `func_thumb` | desmonta em Thumb e cria funcao |
| `func_arm` | desmonta em ARM e cria funcao |
| `ptr` | cria `count` ponteiros de 4 bytes |
| `data` | limpa `count` bytes de codigo falso e cria o label |
| `label` | so cria o nome |

## Uso

Importacao (uma vez):

1. File > Import File > ROM Metabee (E).
2. Format: Raw Binary. Language: `ARM:LE:32:v4t` (ARM, v4t, 32, little, default).
3. Options: Block Name `ROM`, Base Address `08000000`.
4. Nao rode a analise automatica ainda.

Scripts (uma vez por maquina):

1. Window > Script Manager.
2. Icone "Manage Script Directories" (abre o Bundle Manager) > `+` verde > selecione a pasta `re/ghidra` deste repo.
3. Os scripts aparecem na categoria `Medabots` e no menu Tools > Medabots.

Ciclo:

1. Tools > Medabots > Setup.
2. Analysis > Auto Analyze. Desmarque `Aggressive Instruction Finder`, `ARM Aggressive Instruction Finder` e `Non-Returning Functions - Discovered`.
3. Tools > Medabots > Setup de novo (limpa o que a analise criou na area de dados e reaplica os nomes).
4. Trabalhe no Ghidra normalmente.
5. Tools > Medabots > Export Symbols antes de cada commit.

O Setup e idempotente: pode rodar quantas vezes quiser.

## Limpeza de codigo falso

A partir de `0x08080000` a ROM vira o bloco `ROM_DATA`, sem execute, para os analisadores nao criarem codigo ali. Funcoes acima desse endereco sem chamada vinda de codigo abaixo desse endereco sao removidas, depois de confirmacao. Funcoes listadas no CSV nunca sao removidas. O limite fica em `DATA_START` no topo de `MedabotsSetup.java`.
