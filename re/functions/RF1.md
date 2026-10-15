# RF1: revisão da lista RF1_lista.txt (34 itens, todos tipo ram)

Resultado: nenhum item era função. Nenhum renomeado no Ghidra. Os nomes foram mantidos, exceto um.

## Trocas
- 0x030018D6: `fight_slot_roll_halfword` -> `gFight_TargetDamage` (antes: fight_slot_roll_halfword). Confiança: confirmado.
  Base: `Fight_DispatchOnSubstateAA_AR_ApplyTeamPartDamage` (0x080274EE) lê três u16 em fight_information+0xD6 (índice i*2) e subtrai da parte. Confirma o conflito que estava em AS2/RM2.
- 0x03000536: kind `func_thumb` -> `data` (antes: IWRAM_PlayerSpecialtyBase). Byte de dado em ctx+0xF6, não base de tabela.

## Achados
- 0x0300381A (`gPlayerSpeedIndex`) = gObjectSlots[0x2F]+0x2A, por conta: 0x03002C30 + 0x2F*0x40 + 0x2A = 0x0300381A. O nome é só atalho. Confiança: provavel.
- 0x030005D0 = bloco do bot 0 (0x03000590) +0x40, destino de Fight_CopyBotIdentityRecords.
- 0x030005D3 (+0x43) e 0x03000637 (+0xA7): sem escritor achado. Continuam chute.

## Em aberto
- Trap, DPAD, eventos (0x030064xx/0x030065xx), `gOverworldAuxBufferPtr`: continuam chute, sem efeito visto.
- `gOverworldAuxBufferPtr` pode ser o ponteiro do buffer de atores (stride 0x300, SC4). Não renomeei por falta de confirmação.
- Registradores GBA e sp_usr/sp_irq/INTR_VECTOR: confirmados, sem troca.

Detalhes item a item em `RF1_simbolos.csv`.
