# RM4: revisao de nomes (RM4_lista.txt)

Trocas: nenhuma. Nenhum rename_data feito no Ghidra.

Achado: os 43 enderecos da lista sao registradores de IO do GBA (0x04000042-0x04000208), nao variaveis de RAM.
Os nomes REG_* batem com o padrao do hardware e com o mtbe.csv. Uso confirmado no Ghidra em funcoes de link,
teclas (get_player_inputs), interrupcao (IntrMain), DMA3 (lib_dma3_copy), blend e som.

Sem nenhum xref ou literal na ROM (chute): REG_SOUNDBIAS, REG_DMA0DAD, REG_DMA0CNT, REG_DMA1SAD, REG_DMA2CNT,
REG_SIOMULTI2, REG_SIOMLT_SEND. Podem ser acessados por base+offset, sem literal separado.
