# CS: sprites de personagem do mapa (overworld) - GH-CS

Status: PARCIAL (2026-10-14). Estático (ROM original só leitura + Ghidra). Sem emulador. Nada extraído em imagem.

## 1. Carga dos blocos (confirmado no disasm 0x08048288)
- Loop: `i = 0..`; `id = Map_GetSpriteListEntry(i)` (0x08048684) até 0xFF.
- Lista do mapa: `u8 ROM[ u32 ROM[0x08413160 + 4*gCurrentMapId] + i ]`, gCurrentMapId = byte 0x03005FDE.
- `id` -> `Map_ResolveSpriteId` (0x08048210): id 0 -> byte indireto (0x083D2B26 + IWRAM[0x0300643C]); 0xD0+ -> lista RAM 0x03006418+k (escrita pelo evento 0x53); 0xC0..0xCF -> (id-0xC0)*2 + IWRAM[0x03006D3E] + 0x28. Entre 0x00 e 0xBF o id segue direto.
- Bloco: `ptr = ROM[0x083F7E80 + 4*id]`; se 0, usa id 1. LZ77 (bios 0x0807CCD8) para **VRAM 0x06010000 + i*0x900**. Tamanho 0x900 = 2304 B = 72 tiles 4bpp de 8x8 (32 B cada) se o bloco for 4bpp 1D.
- O índice da VRAM é a POSIÇÃO i na lista do mapa, não o id.

## 2. Id de sprite -> grupo e direção (confirmado)
- `Obj_FrameBaseFromSprite` 0x080502C8: frame base = (sprite>>4)*12 + (sprite&3)*3. Grava em +0x06 (frame) no spawn e no modo sem animação.
- `Map_InitObjectSprite` (OW.md 3f): +0x2B = sprite>>4 (grupo); +0x20 = sprite&3 (facing 0 N,1 S,2 W,3 E).
- Cada grupo tem 12 quadros (4 direções x 3 quadros).

## 3. Roteiro de animação (confirmado: disasm 0x08049440 + dump da ROM)
- Anim = facing + 4*grupo (`Overworld_GetObjectAnimationIndexForFacing` 0x08050300). Roteiro = `ROM[0x083F7B80 + 4*anim]` (ptr do roteiro); +0x24 do slot = anim.
- Passo = 8 bytes: `u16 op, u16 a1, u16 a2, u16 a3`. +0x22 = índice do passo (x8), +0x23 = contador de ticks.
- Ops: `op + 0xFFFF0006 <= 5` seleciona o caso (ou seja, op 0xFFFA..0xFFFF = casos 0..5 da tabela em 0x0804948C); demais ops = default:
  - 0xFFFA: quadros a3 + byte(+0x2F), mantém a2 ticks, avança.
  - 0xFFFB e demais (default): frame(+6) = a3, mantém a2 ticks, avança passo.
  - 0xFFFC: carrega LZ77 do registro a1 (tabela 0x080AEB74, 16 B) na 1ª vez, depois default.
  - 0xFFFD: SFX a1, avança e repete.
  - 0xFFFE: volta ao passo a1 (laço).
  - 0xFFFF: fim.
- Dump (ROM): anim 0 = quadros 1,0,2,0 cada 8 ticks; FFFE volta ao 0. Anim 1 = 4,3,5,3; anim 2 = 7,6,8,6; anim 3 = A,9,B,9 ... Padrão: passo parado = base+0, andar = base+1 / base+2, com base = 3*anim. Confere com 12*grupo+3*facing.
- Map_ObjectAnimStep 0x080508B4 (OW.md 3b) chama Map_TickObjectAnimation uma vez por tick até a2... (leitura anterior, não reconferida).

## 4. OAM
- `BuildShadowOamFromObjectState` 0x08001520: varre 0x80 entradas de 0x40 B (ativas = bit 0x8000 do halfword +0), monta lista, ordena por SortActiveObjectIndicesByPriority 0x08002060 (shell sort ascendente pelo byte +0x0B; menor = escrito primeiro na OAM). Obs.: o nome Ghidra fala em "priority"; NÃO é ordenação por Y.
- Campo +0x06 (frame): byte baixo = índice do molde (entrada de tabela de moldes); byte alto = tabela de moldes (0 usado nos roteiros de mapa). Molde = contagem u16 + partes de 8 B (y, atributo, tile, x). Atributos de flip em bits 0x10/0x20 do halfword de forma.
- Tamanho 16x32? NÃO confirmado: depende dos moldes da tabela (não lida).

## 5. Paletas
- `Map_LoadCurrentEventObjectPalettes` 0x08048708 (prólogo 0xB570): não decompilado nesta passada. Pendente.

## 6. Comparação com pc/extract
- pc/extract/extract_objects.py e extract_map.py (extract_all.py) não usam 0x083F7E80, 0x083F7B80 nem 0x08413160 (grep). Logo o extrator não tem a lista por mapa nem o roteiro de animação; divergência: o extrator precisa desse mapeamento para montar os quadros de NPC.
- Não comparei imagens (regra: só números).

## Pendente
- Tamanho do molde/OBJ (16x32?) e tabela de moldes do mapa (byte alto do +0x06).
- Origem de +0x2F (usado pelo 0xFFFA) e do byte IWRAM 0x03006D3E.
- Paletas OBJ dos personagens (0x08048708).
- Mapeamento tile = slot*72 + molde (suposto, não confirmado).
- Quem grava +0x0B (prioridade) e se algum objeto usa Y como chave.
- Tamanho da tabela 0x083F7B80 (não contado).
- Passo do spawn +4 (byte da lista de spawn): não rastreado nesta passada.
