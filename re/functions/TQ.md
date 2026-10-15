# TQ: técnicas de peça (0..59) — setup, validação e roteiro (GH-TQ, 2026-10-14, em andamento)

Leitura: ROM original `Medabots - Metabee (Europe).gba` (só leitura, python -I) e decompile do Ghidra.
Convenção: endereço da tabela = `0x08000000 + offset`; entrada thumb tem bit 1 (mostrado sem o bit).
Nomes de família são **inferidos do nome em inglês da técnica** (exceto onde o código confirma, marcado).

## 1. Como a técnica é escolhida (confirmado)

- Peça k (0..3) do bot b: registro de 0x18 B no bloco do bot `0x03000590 + b*0x100 + 0x48 + 0x18*k`
  (0x48 = medalha, **0x49 = técnica**, 0x4A = especialidade, 0x4C = armadura máx, ...). Ver `DM.md` §P(b,k).
- Despacho (confirmado, decompile de `Fight_DispatchPartPreparation` 0x08031A70 e `Fight_DispatchPartExecution` 0x08031ABC):
  - byte técnica = `ROM/RAM[0x03000590 + fight[+0x5C]*0x100 + fight[+0x5D]*0x18 + 0x49]` (fight = 0x03001800; b = +0x5C, k = +0x5D).
  - preparação: `gTechniqueActionSetupHandlerTable` 0x083AEEA0[técnica] (chamada em 0x08031A70).
  - validação/execução: `gTechniqueActionValidationHandlerTable` 0x083AEF90[técnica] (chamada em 0x08031ABC).
  - Entrada 0x08000840 (thumb 0x08000841) = `NoOpActionHandler` (só `return`). Técnicas sem setup ou sem validação própria usam essa entrada.
- Setup e validação não gravam a técnica; ela fica em `F+0xA8` quando a ação entra no roteiro (BR_14, `BT.md`).

## 2. Roteiro de ação ligado à técnica (confirmado no código; mapa de conteúdo parcial)

- `Fight_RunActionScriptLoop` 0x08033840 lê o opcode com `FUN_08034E94` 0x08034E94 = `ROM[ ptr[F+0xA8] + F+0xBC ]`,
  onde `ptr` = **`gTechniqueActionScriptPtrTable` 0x083C6CA8** (4 B por entrada; índice = F+0xA8 = técnica 0..59;
  medaforça usa 0x50+id = 80..129, ver `MF.md`). Opcode ≥ 0x80 = controle (`AS1.md`); despacho por
  `gActionScriptOpcodeHandlerTable` 0x083AF180[opcode] (`AS1.md`, `AS2.md`).
- Os ponteiros 0..59 apontam para **um único fluxo de bytes** (cada roteiro começa em um ponto dele e segue até o fim
  do fluxo). Vinte técnicas compartilham o mesmo início 0x083C6A04 (ver §4). Confirmado por leitura da tabela;
  como o fluxo acaba em cada caso não foi percorrido inteiro.
- Opcodes de técnica de dano/efeito que aparecem nos roteiros (pelos nomes em `AS2.md`): 0x41 (alvo aleatório),
  0x42 (sorteio de valor), 0x43, 0x46 (soma armadura máx−atual), 0x49 (metade da armadura), 0x4C, 0x4D (sorteio),
  0x50 (dano em peça aleatória), 0x51 (estado por técnica 4..8), 0x52..0x54 (suporte de time / marca / estado),
  0x55..0x5A (time: proteção, cura, reparo, restauração, gauge, técnica sem dano). 0x5B..0x6A: pendentes.

## 3. Famílias, handlers e regras

Setup (`gTechniqueActionSetupHandlerTable`): nomes no Ghidra já aplicados. Cada setup escolhe o alvo (`fight+0x5E`
= slot do alvo, `fight+0x5F` = linha de status sorteada, `fight+0x60` = lado, `fight+0x61..63` = candidatos) e
termina em `Fight_SetPartActionMetadata(bot, k)`. Quando não acha candidato, relaxa o filtro em passos (ex.: 1º filtro
com regra da técnica; 2º sem ela; depois aleatório no lado oposto/aliado).

| Setup (endereço) | Nome no Ghidra | Regra de candidato (lida no decompile) | Técnicas |
|---|---|---|---|
| 0x08000840 | NoOpActionHandler | nenhuma | ver tabela §5 |
| 0x08031AFC | Fight_SetupTargetSelectionBySpeciality | bot com peça de especialidade igual à da peça atacante (`Fight_HasActivePartWithSpeciality`) | 2..9, 42, 49, 52 |
| 0x08031BCC | Fight_SetupTargetSelectionRequireStatus64 | bot vivo com `+0x91 == 'd'` (100 = perna voadora) | 34 (Anti-Air) |
| 0x08031D7C | Fight_SetupTargetSelectionRequireStatus6A | bot vivo com `+0x91 == 'j'` (106 = perna aquática) | 35 (Anti-Sea) |
| 0x08031F2C | Fight_SetupTargetSelectionRequirePendingEffect | bot vivo, sem técnica 16 (`Fight_HasNoTechnique16Part`), `+0xC9 != 0` (efeito pendente) | 22 |
| 0x08032100 | Fight_SetupTargetSelectionRequireDamagedPart | bot vivo com peça danificada | 30 |
| 0x080322C8 | Fight_ChooseActionTargetSlot | escolha de slot do alvo (sem filtro próprio) | 31, 38 |
| 0x08032478 | Fight_SetupTargetSelectionRequireBrokenPart | bot com peça quebrada | 32 |
| 0x08032640 | Fight_SetupTargetSelectionRequireStateBA2 | bot com estado BA2 (`Fight_*` a confirmar) | 33 |
| 0x0803279C | Fight_SetupTargetSelectionExcludeSelf | exclui o próprio bot do lado aliado | 39 |
| 0x080328B8 | Fight_SetupTargetSelectionRequireNoTechnique16AndClearPending | sem técnica 16 + limpa pendência | 41, 44, 46, 47 |
| 0x08032A68 | Fight_SetupTargetSelectionRequireUsableHeadAction | ação de cabeça utilizável | 45 |
| 0x08032C08 | Fight_SetupTargetSelectionRequireMedaforceToken | alvo com medaforça/token (`+0x..`, a confirmar) | 50 |
| 0x08032DA0 | Fight_SetupPartTargetSelectionRequireNoTechnique16AndClearPending | como 41, mas escolhe peça | 43 |

Validação (`gTechniqueActionValidationHandlerTable`): só **um** handler distinto, 0x080332EC (função desde o sync de 2026-10-14 (nome: Fight_ValidatePart_ClearPartByte0F);
decodificado à mão, 0x080332EC..0x0803330C, 9 instruções, sem BL):
- lê `k = fight[+0x5D]`, `b = fight[+0x5C]`, calcula `0x03000590 + b*0x100 + k*0x18` e **zera o byte +0x0F do registro**
  (endereço Ghidra `+0x57` da peça; campo não identificado, provavelmente contador/flag de uso). Retorna.
- Usado por 22, 31, 33, 38, 39, 41, 42, 44, 46, 47, 50. Nome sugerido: `Fight_ValidatePart_ClearPartByte0F` (chute, não aplicado).

Regras de validação comuns (BR_13/BR_14, `BT.md`; confirmadas só nos ramos listados):
- `B+0xBD` bit 0 → erro (A6 = 4) antes de tudo (BR_13).
- `B+0xDD == 0` → sem peça selecionada (GS = 0x10).
- `B+0xDD >= 4` → medaforça (slot = B+0xDD − 4), não é técnica.
- Anti-ar (34) só acerta alvo com perna `'d'`; anti-mar (35) só `'j'` (`IsAntiAir` 0x08035880, `IsAntiSea` 0x080358B8, `DM.md`).
- Missile (6) e Napalm (7) não erram por ROS zero (`DM.md`, `Fight_ApplyEvasionReductionToRos`).
- Quebra/erro de peça: códigos 0x04..0x0C em BR_14 (não decompilado nesta rodada).
- Pernas (k = 3): +1 é **tipo de perna 100..106**, não técnica; não passam por estas tabelas (a confirmar).

## 4. Tabela técnica -> família -> setup -> validação -> roteiro

Família = inferida do nome (ver legenda ao fim). `ptr` = 0x083C6CA8[id]. Grupo de roteiro: `A04` = 0x083C6A04.

| id | técnica (nome da ROM, só id) | família | setup | validação | ptr roteiro |
|---|---|---|---|---|---|
| 0 | Sword | golpe corpo a corpo | 0x08000840 (nada) | nenhuma | 0x083C6A04 (A04) |
| 1 | Hammer | golpe corpo a corpo | 0x08000840 (nada) | nenhuma | A04 |
| 2 | Rifle | tiro | 0x08031AFC BySpeciality | nenhuma | A04 |
| 3 | Chain Gun | tiro | 0x08031AFC BySpeciality | nenhuma | A04 |
| 4 | Laser | tiro de energia (DM: +poder) | 0x08031AFC BySpeciality | nenhuma | A04 |
| 5 | Beam | tiro de energia (DM: +poder) | 0x08031AFC BySpeciality | nenhuma | A04 |
| 6 | Missile | tiro com explosão (exceção ROS zero) | 0x08031AFC BySpeciality | nenhuma | A04 (ver §6) |
| 7 | Napalm | tiro de área/fogo (exceção ROS zero) | 0x08031AFC BySpeciality | nenhuma | A04 |
| 8 | Break | golpe de quebra (DM: +sucesso) | 0x08031AFC BySpeciality | nenhuma | A04 |
| 9 | Press | golpe de esmagamento | 0x08031AFC BySpeciality | nenhuma | A04 |
| 10 | Grap Trap | armadilha | 0x08000840 | nenhuma | 0x083C6A57 |
| 11 | Shot Trap | armadilha | 0x08000840 | nenhuma | 0x083C6A59 |
| 12 | Attack Clr | limpeza de efeito no ataque (provável) | 0x08000840 | nenhuma | 0x083C6A5B |
| 13 | Bug | status | 0x08000840 | nenhuma | A04 |
| 14 | Virus | status | 0x08000840 | nenhuma | A04 |
| 15 | Thunder | elemental | 0x08000840 | nenhuma | A04 |
| 16 | Freeze | elemental/status; **id usado como guarda** (“sem técnica 16”) | 0x08000840 | nenhuma | A04 |
| 17 | Wave | elemental de área (provável) | 0x08000840 | nenhuma | A04 |
| 18 | Hold | status (prende) | 0x08000840 | nenhuma | A04 |
| 19 | Fire | elemental | 0x08000840 | nenhuma | A04 |
| 20 | Melt | elemental | 0x08000840 | nenhuma | A04 |
| 21 | Break Form | status/quebra de forma (provável) | 0x08000840 | nenhuma | 0x083C6A65 |
| 22 | Stability | suporte; alvo com efeito pendente | 0x08031F2C RequirePendingEffect | 0x080332EC | 0x083C6A67 |
| 23 | Force Bind | status (prende) | 0x08000840 | nenhuma | 0x083C6A6B |
| 24 | CancelOptic | cancela efeito (óptico) | 0x08000840 | nenhuma | 0x083C6A6D |
| 25 | CancelBomb | cancela efeito (bomba) | 0x08000840 | nenhuma | 0x083C6A6E |
| 26 | CancelGrav | cancela efeito (gravidade) | 0x08000840 | nenhuma | 0x083C6A6F |
| 27 | Defence | defesa | 0x08000840 | nenhuma | 0x083C6A70 |
| 28 | Full Block | defesa total | 0x08000840 | nenhuma | 0x083C6A72 |
| 29 | Half Block | defesa parcial | 0x08000840 | nenhuma | 0x083C6A74 |
| 30 | Recovery | cura (alvo com peça danificada) | 0x08032100 RequireDamagedPart | nenhuma | 0x083C6A76 |
| 31 | AutoRecover | cura automática | 0x080322C8 ChooseActionTargetSlot | 0x080332EC | 0x083C6A87 |
| 32 | Repair | reparo (alvo com peça quebrada) | 0x08032478 RequireBrokenPart | nenhuma | 0x083C6A8B |
| 33 | Reactivate | reativa peça (estado BA2) | 0x08032640 RequireStateBA2 | 0x080332EC | 0x083C6A9C |
| 34 | Anti-Air | anti-ar (alvo perna 'd') | 0x08031BCC RequireStatus64 | nenhuma | A04 |
| 35 | Anti-Sea | anti-mar (alvo perna 'j') | 0x08031D7C RequireStatus6A | nenhuma | A04 |
| 36 | Scout | reconhecimento | 0x08000840 | nenhuma | 0x083C6AA2 |
| 37 | Conceal | ocultação/defesa (provável) | 0x08000840 | nenhuma | 0x083C6AA4 |
| 38 | Boost Chrg | carga de medidor (+) | 0x080322C8 ChooseActionTargetSlot | 0x080332EC | 0x083C6AA6 |
| 39 | Rapid Chrg | carga de medidor, exclui si | 0x0803279C ExcludeSelf | 0x080332EC | 0x083C6AAA |
| 40 | Accel Chrg | carga de medidor | 0x08000840 | nenhuma | 0x083C6AAE |
| 41 | Drain Chrg | carga/dreno de medidor | 0x080328B8 RequireNoTech16AndClearPending | 0x080332EC | 0x083C6AAF |
| 42 | Pushover | golpe (especialidade) | 0x08031AFC BySpeciality | 0x080332EC | 0x083C6AB3 |
| 43 | Impair | status (peça) | 0x08032DA0 (peça, NoTech16+ClearPending) | nenhuma | 0x083C6AB8 |
| 44 | Confusion | status | 0x080328B8 RequireNoTech16AndClearPending | 0x080332EC | 0x083C6ABC |
| 45 | Use Drain | dreno de uso | 0x08032A68 RequireUsableHeadAction | nenhuma | 0x083C6AC0 |
| 46 | No Escape | status (impede fuga) | 0x080328B8 RequireNoTech16AndClearPending | 0x080332EC | 0x083C6AC4 |
| 47 | No Defense | status (ignora defesa) | 0x080328B8 RequireNoTech16AndClearPending | 0x080332EC | 0x083C6AC8 |
| 48 | Destroy | ataque direto | 0x08000840 | nenhuma | 0x083C6ACC |
| 49 | Sacrifice | sacrifício (especialidade) | 0x08031AFC BySpeciality | nenhuma | 0x083C6AFF |
| 50 | Forcedrain | dreno forçado (token de medaforça) | 0x08032C08 RequireMedaforceToken | 0x080332EC | 0x083C6B2C |
| 51 | Team Form | suporte de time (forma) | 0x08000840 | nenhuma | 0x083C6B30 |
| 52 | TeamAttack | ataque de time (DM: usa Team Form) | 0x08031AFC BySpeciality | nenhuma | 0x083C6B32 |
| 53 | Strengthen | bônus de poder nas peças 0..2 (confirmado, `DM.md`) | 0x08000840 | nenhuma | 0x083C6B85 |
| 54 | CntrAttk | contra-ataque | 0x08000840 | nenhuma | 0x083C6B86 |
| 55 | Change | troca de estado | 0x08000840 | nenhuma | 0x083C6B88 |
| 56 | ItrpChange | interrompe e troca | 0x08000840 | nenhuma | 0x083C6B8A |
| 57 | AttkChange | troca o ataque do alvo (provável) | 0x08000840 | nenhuma | 0x083C6B8C |
| 58 | DEF Change | troca a defesa do alvo (provável) | 0x08000840 | nenhuma | 0x083C6B8E |
| 59 | HealChange | troca de cura (provável) | 0x08000840 | nenhuma | 0x083C6B90 |

Legenda: "A04" = 0x083C6A04; "nenhuma" = validação 0x08000840 (sem regra própria além do BR_13/BR_14).
Famílias: golpe, tiro, elemental, status, armadilha, cancelamento/limpeza, defesa, cura/reparo, carga, dreno, suporte de time,
anti (ar/mar), reconhecimento, troca, sacrifício. A família é do nome; só as marcadas "confirmado" no texto foram lidas no código.

## 5. Pendências e conflitos

- **Conflito (não resolvido):** 20 técnicas (0..9, 13..20, 34, 35) apontam para o mesmo início de roteiro 0x083C6A04. Se o
  índice fosse só a técnica, Missile (6) e Sword (0) rodariam a mesma animação, e B15/B16 (efeito de explosão só na técnica 6)
  diriam o contrário. Hipóteses: o efeito vem de outro campo (gObjectSlots/motion), ou o índice real é outro. Validar em runtime.
- Validação 0x080332EC não tem função no Ghidra (criar função não estava no pedido); nome sugerido não aplicado.
- Setup de 33 (RequireStateBA2), 45, 50 e de 43: decompile não lido por completo (só o nome e a regra principal).
- Roteiros: não percorridos até o fim; opcodes 0x5B..0x6A e 0x47/0x4A pendentes (`AS2.md`).
- Pernas k=3 (+1 = tipo 100..106): fora de 0..59; caminho de preparação delas não localizado.
- BR_14 (códigos de erro 0x04..0x0C) e efeito de `+0xBD` bit 0 não decompilados aqui.

