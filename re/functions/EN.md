# EN: de onde vêm os times inimigos (GH-EN, 2026-10-14)

Leitura estática (Ghidra + ROM, só leitura; `python -I`). ROM = 0x08000000 + offset. Estado de evento/mapa: base E =
0x030057E0 (OW.md, HE2.md). Template da batalha = `fight_information.template` em 0x03001800 (0x28 B, copiado do ROM).
Confiança: "confirmado" = decompile/disassembly + literal; "provável" = padrão de uso; "medido" = contagem nos dados da ROM.

ATENÇÃO (decompiler): os nomes `DAT_08011dd8` e parecidos são literais; em `Fight_BeginEncounterTemplateSelection`
o decompile mostra escritas em 0x040000D4..0x040000DC (registros DMA3), não lógica de template. Não confiar no corpo.

## 1. Fluxo

```
(a) Batalha aleatória: passos (E+0xC2B > 16 e randbyte&0x1F == 0x1F ou passos > 31) -> encontro
    Overworld_ShouldTriggerRandomEncounter 0x080438C4 (OW.md, pc/research/README.md)
    -> reset_step_and_get_encounter_flag 0x0804387C -> Fight_ResetTemplateAndPalette 0x08010540
    -> Fight_BeginEncounterTemplateSelection 0x08011DB0 (xref confirmado)
(b) Batalha de história: evento 0x33 Start_Battle(id) 0x0804C53C (HE2.md)
    -> E+0xC2A (0x0300640A) = id; E+0xC89 = 1; continuação 0x0804E8C4 (ainda sem o caminho até (c) lido)
(c) Fight_SelectTemplateForCurrentEncounter 0x08011E20 (confirmado no decompile):
    se E+0xC2D (0x0300640D) == 0: sorteia um dos 4 bytes de gMapEncountersExtra[gCurrentMapId] e grava em E+0xC2A;
    copia 0x28 B de gEncounterTemplatePtrs[E+0xC2A] (0x083C1B80 + id*4) para 0x03001800;
    se template+3 == 0: re-sorteia as peças (Fight_SelectTierPartId 0x08011F20). MEDIDO: todos os 246 templates
    têm byte +3 = 1, então este ramo NÃO é usado pelos dados da ROM.
(d) Fight_SetupBattleBotStates 0x080129F2 (confirmado): se ctx+0x89 != 1: Fight_ClearBattleBuffer,
    Fight_UpdateBotActiveFlagCount, Fight_RefreshBotPartRecords, Fight_InitializeEnemyBotStates 0x080150C0.
    Fight_InitializeEnemyBotStates: para i = 0 .. (fight+0x5B - 1): Fight_InitializeBotFromTemplate(i) (confirmado).
(e) Recompensa e derrota: §6 (aberto).
```

## 2. Batalha aleatória (gMapEncountersExtra)

- `gMapEncountersExtra` 0x083BF210: 192 entradas x 4 B, índice = gCurrentMapId (0x03005FDE; OW.md).
  Medido: 64 mapas têm quarteto; 128 são zero. Mapa 1..9: `9B F5 9B A7`. Mapa 23 (New Game) = zero (sem encontro).
  Ids usados: 58 distintos, todos com ponteiro válido (faixa 30..245).
- Sorteio `Encounter_RollPercent0To99` 0x08043C94 (confirmado): r < 40 -> byte 0 (40%); r < 70 -> byte 1 (30%);
  r < 90 -> byte 2 (20%); senão byte 3 (10%). Confirmado no código (0x08011E20 e 0x08011F20).
- O byte escolhido é **id de template** (0..245), gravado em E+0xC2A. O nome "gEncounterSlotIndex" do OW.md está
  errado: é o id, não índice 0..3.

## 3. Batalha de história (Start_Battle 0x33)

- Byte 1 do argumento = id -> E+0xC2A = **id de template direto**. Mesma tabela (§4), sem sorteio se E+0xC2D != 0.
- Quem grava E+0xC2D = 1 nessa rota: não achado (aberto). Ver §7.

## 4. Tabela de templates gEncounterTemplatePtrs 0x083C1B80

- 246 ponteiros u32 (todos válidos). Índice = id de template (0..245). Csv: `gEncounterTemplatePtrs`
  (Simon: list_of_all_fights). Cada ponteiro -> template de 0x28 B.

### 4.1 Template (0x28 B) — formato confirmado no código, nomes provável
| Offset | Campo | Uso | Confiança |
|---|---|---|---|
| +0x00 (u8) | character_id | índice de gTextPreBattleMessage (fala antes da luta; TX/mtbe) | confirmado (leitura), uso = provável |
| +0x03 (u8) | template_flags | 0 = re-sorteia peças; 1 em todos os 246 templates da ROM | confirmado / medido |
| +0x04 (u8) | peça da cabeça (id) | lido por Fight_InitializeBotFromTemplate como índice de 0x083BA658 | confirmado |
| +0x05..+0x07 | demais peças (id) | não lidos em (c) e (d); leitor aberto | aberto |
| +0x08 (u8) | medalha (id) | gMedals 0x083B62D8 + id*0x10 | confirmado |
| +0x09 (u8) | nível do bot | copiado para bot+5 | confirmado |
| +0x0A..+0x0D | 4 bytes de especialidade (nibbles) | bot+0x18..+0x1F, crus; não usados no cálculo final de §5 | confirmado (cópia) |
| +0x0C e +0x18 | registros 1 e 2 (mesma forma) | bots 4 e 5 | confirmado |
| +0x24..+0x27 | 4 bytes | zerados nos exemplos; não lidos nestas funções | aberto |

- Os +4..+7 são 4 bytes por registro. Em (c), com flags == 0, eles são um quarteto de **candidatos por raridade**
  (Fight_SelectTierPartId: r<40 a, <70 b, <90 c, senão d). Com flags == 1 (o caso da ROM) não há sorteio.
- Contagem de bots (medida nos 246 templates): registros com nível > 0 -> 3 bots: 198; 2: 34; 1: 4; 0: 10.
  Provável: a contagem usada por Fight_InitializeEnemyBotStates (fight+0x5B) sai disso; não confirmado no código.

## 5. Fight_InitializeBotFromTemplate 0x080150FC (confirmado no disassembly, 0x08015100..0x080151FA)

Entrada: k (0..2) = registro. Bot inimigo = slot 3+k; bloco RAM = 0x03000590 + (3+k)*0x100.
- Medalha: copia gMedals[rec+8] (0x10 B) para scratch 0x0300052E.
- Cabeçalho: bot+0 = 0x80; bot+1 = medal[0]; bot+4 = medal[1]; bot+5 = rec+9 (nível); bot+6 = medal[2]; bot+7 = medal[7];
  bot+9 = rec+0xE (= byte 2 do registro seguinte para k < 2; aberto).
- Cabeça: copia 0x0E B de `ptr 0x083BA658[rec+4]` para enemy_block + 0x0A (0x0300089A + k*0x100).
  **Conflito aberto**: pc/research/README.md diz peça = bloco + 0x4C (Ghidra). Não resolvido aqui.
- Especialidade final bot+0x20..+0x27 (k = 0..7): `T[(bot+4)*8 + k] * (bot+5 >> 1)`, T = tabela de 8 B em 0x083AD380
  (ponteiro 0x080151E8); se 0 -> 1; se > 100 -> 100. Confirmado.

## 6. Recompensa e derrota (aberto)

- `won_bodypart_type` 0x03001968 e `won_bodypart_id` 0x03001969 (mtbe.csv; BT1 em pc/research/README.md) = slot
  sorteado (rand&3) e id do bot. Quem grava não lido neste passo. Dinheiro e derrota: não medidos.

## 7. Pendências

- Quem põe E+0xC2D = 1 na história (e se a história sempre usa id direto).
- Peças +0x05..+0x07 (quem as lê), bytes +0x24..+0x27, rec+0xE.
- Conflito do offset da cabeça (+0x0A x +0x4C).
- Fight_UpdateBotActiveFlagCount (origem de fight+0x5B) e o significado de ctx+0x89 == 1 em Fight_SetupBattleBotStates.
- Recompensa e dinheiro (§6).
- Ghidra: nenhum nome novo confirmado neste passo (os nomes de §5 e §2 já existem); nada foi renomeado.
