# Notas para fundir os nomes no mtbe.csv

Regra: quando o mtbe.csv já tem um nome que descreve melhor, ele vence e o Ghidra volta para ele.

| Endereço | Ghidra (agente) | mtbe.csv | Decisão |
|---|---|---|---|
| 0x08035754 | Fight_SubClampZero | safe_subtract | manter mtbe |
| 0x08035814 | Fight_ApplyStatusCostIfFlag | Fight_ApplyVirusBugRosPenalty | mtbe |
| 0x08035880 | Fight_CheckPairCondition | Fight_IsAntiAirAgainstAirTarget | mtbe |
| 0x0803579C | Fight_AccumRosPart | Fight_GetPartProficiencyBase | revisar |
| 0x08035740 | Fight_AddByteClampToMax | safe_add | mtbe |
| 0x08034F68 | Fight_ApplyDamageToPart | do_damage_on_part | revisar |
| 0x080183A4 | HandleSelectedMedabotPreviewCursorInput | (idem) | renomear: é o cursor 3x3 da luta |
| 0x08044A3C | MotionPlayStep | site_MoveStyleRead | Ghidra (confirmado G9) |
| comentário de Fight_TrySelectActionFromSpecialityCycle | — | "4 cai no head" | errado: limpa a fonte e devolve 0 |
| comentário de 0x08072D5C snd_cmd_end | — | cita FUN_0807495c | errado: é outra função |
| tabela 0x083D222C | — | SND2/SND11 dizem 0xD8..0xFF | errado: indexada por opcode 0x00..0x88 (H.md) |
| 0x08043C94 | Encounter_RollPercent0To99 | generate_random__ | Ghidra (randbyte*100/0xFF) |
| 0x0804538C | LayoutCompositePreviewEntryOamGrid | ShiftCompositePreviewTemplateEntryX | Ghidra (posiciona a grade de OAM) |
