# DS: peças Destroy/Sacrifice medidas no emulador (2026-10-15)

Fontes: `rec_12` (gravação do usuário: Sacrifice acerta) e o `.ss2` do usuário (Unicorn × urso de alta esquiva; A, ←
no início = Sacrifice, que erra). Watch de escrita na armadura (bot+0x59+0x18k). Confiança: **confirmado**.

- Ao acertar: a peça do alvo vai direto a 0 (perna do inimigo 50→0 na rec_12), não é dano proporcional.
- **Sempre**, acertando ou errando: a peça usada pelo atacante quebra (braço 15→0; rec_12 f2334, ss2 k=1011), com o
  código de mensagem fi+0xA6 = 24 (antes 23). No ss2 o urso desviou e não perdeu armadura, mas o braço quebrou igual.
- Escritor: `Fight_DispatchOnSubstateAA_M_04` 0x08020728 → `Fight_SubtractPartArmorClampZero(fi+0x87, fi+0xA7, 0xFF)`
  (pc 0x0803518E). fi+0x87 = bot que agiu, fi+0xA7 = peça usada. Se a peça usada é a cabeça (0), chama
  Fight_QueueBrokenBotForResolution (o bot cai). O nome/comentário antigo ("peça do defensor") estava errado.
- O despachante M roda depois do resultado do golpe, por isso independe do acerto.

Aberto: o passo que zera a peça do alvo no acerto (não capturado: o watch da rec_12 no alvo veio vazio) e o id de
técnica do Sacrifice/Destroy.
Obs.: `rec_14` não é reproduzível (o usuário recarregou o estado durante a gravação; o braço "volta" de 0 para 15).
