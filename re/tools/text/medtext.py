"""Texto do Medabots (E): tabela de caracteres, larguras da fonte e quebra de caixas como o jogo faz."""
import re
import unicodedata

CHARS = (" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789·.,'-/:?!\"()♥£&%õ"
         "ÄäÁáÂâÈèÉéÊêËëÎîÏïÍíÖöÔôÓóÜüÛûÙùÚúßÇçÑñ¡¿ÀàÆæÃãÕ")
CODE = {c: i for i, c in enumerate(CHARS)}
WRAP = 208
TAG = re.compile(r'<[^<>]*>')


# Avancos (px) dos glifos 0x00-0x7F com a fonte estendida do Toolkit (aba Font):
# 0x00-0x4E da ROM (E); 0x4F-0x7F calculados pelos pixels (ExtendedFont.MeasureGlyph)
W_MAIN = [
    4, 6, 5, 6, 6, 5, 5, 6, 6, 4, 6, 5, 5, 7, 6, 7,
    5, 7, 6, 6, 6, 6, 6, 8, 6, 6, 6, 6, 5, 5, 5, 6,
    4, 5, 5, 2, 4, 5, 2, 6, 5, 6, 4, 5, 4, 4, 4, 4,
    5, 6, 6, 5, 5, 6, 5, 6, 6, 6, 5, 5, 6, 5, 6, 4,
    4, 3, 2, 6, 5, 4, 6, 4, 4, 6, 6, 8, 8, 8, 8, 6,
    6, 6, 6, 6, 6, 6, 5, 6, 5, 6, 5, 6, 5, 6, 4, 4,
    4, 4, 4, 4, 7, 6, 7, 6, 7, 6, 6, 4, 6, 4, 6, 4,
    6, 4, 5, 6, 5, 6, 5, 4, 6, 6, 6, 8, 8, 7, 6, 7,
]
W_ITAL = [
    4, 6, 5, 6, 6, 6, 6, 6, 6, 3, 5, 6, 4, 7, 6, 6,
    6, 6, 6, 6, 5, 6, 5, 7, 7, 5, 6, 6, 5, 6, 6, 5,
    5, 7, 5, 4, 5, 5, 4, 7, 6, 5, 6, 6, 4, 5, 4, 5,
    5, 7, 6, 6, 5, 6, 4, 6, 6, 6, 6, 6, 5, 6, 6, 4,
    4, 4, 1, 6, 7, 5, 5, 4, 4, 4, 5, 8, 8, 8, 8, 6,
    6, 5, 5, 6, 6, 5, 6, 5, 6, 5, 6, 4, 6, 4, 4, 4,
    4, 4, 3, 4, 6, 5, 6, 5, 6, 5, 6, 5, 6, 5, 6, 5,
    6, 5, 5, 6, 5, 6, 5, 4, 5, 5, 5, 8, 7, 6, 5, 6,
]
REPL = {'\u2019': "'", '\u2018': "'", '\u201c': '"', '\u201d': '"', '\u2026': '...',
        '\u2014': '-', '\u2013': '-', '\u00a0': ' '}


def clean(s):
    # Unicode composto e pontuacao tipografica para o charset do jogo
    s = unicodedata.normalize('NFC', s)
    for a, b in REPL.items():
        s = s.replace(a, b)
    return s


# Rotulos (nomes de medalha, arma, perna, item...) sao desenhados como sprites com a fonte 8x8 da ROM
# (0x084C8034, so 0x01-0x4E): letra acentuada le quadros fora da tabela e desenha lixo.
LABEL_TABLES = {'MedalNames', 'MedalAbilityNames', 'AttackTypeNames', 'WeaponTypeNames', 'LegTypes',
                'BattlefieldNames', 'MenuLabels', 'KeyItems', 'CharacterNames', 'StatusStrings', 'MedaforceNames'}


def fold_accents(s):
    # Tira acentos (á -> a, Ç -> C); mantem ♥ · £
    out = []
    for c in s:
        d = unicodedata.normalize('NFD', c)
        out.append(d[0] if len(d) > 1 and d[0].isascii() else c)
    return ''.join(out)


def bad_chars(s):
    return sorted({c for c in TAG.sub('', s) if c not in CODE})


def tags(s):
    return [t for t in TAG.findall(s) if t != '<NL>']


def text_width(s, italic=False):
    # Largura em px; <MEM:n> = 8 x 'W'; <I> alterna italico
    x = 0
    for m in re.finditer(r'<[^<>]*>|.', s, re.S):
        t = m.group(0)
        if t == '<I>':
            italic = not italic
        elif t.startswith('<MEM'):
            x += 8 * (W_ITAL if italic else W_MAIN)[CODE['W']]
        elif len(t) == 1 and t in CODE:
            x += (W_ITAL if italic else W_MAIN)[CODE[t]]
    return x


def reflow(s, width=WRAP, lines_per_box=2):
    # Refaz as quebras de linha de cada caixa do zero (enche a linha ate 208 px, como o jogo quebra palavras);
    # as caixas (<NB>) do texto ficam, e a que nao couber em 2 linhas vira mais caixas
    end = ''
    m = re.search(r'(<END:\d+>|<ENDLST>)$', s)
    if m:
        end = m.group(1)
        s = s[:m.start()]
    boxes = s.split('<NB>')
    out = []
    italic = False
    for box in boxes:
        box = re.sub(r'\s*<NL>\s*', ' ', box)
        box = re.sub(r' {2,}', ' ', box)
        lead = ''.join(re.findall(r'^(?:<PORTRAIT:[^>]*>)*', box)[:1])
        body = box[len(lead):].strip()
        words = body.split(' ') if body else []
        lines, cur, x = [], [], 0
        for w in words:
            ww = text_width(w, italic)
            sw = (W_ITAL if italic else W_MAIN)[0]
            # o jogo mede a palavra com o espaco seguinte
            if cur and x + sw + ww + sw > width:
                lines.append(' '.join(cur))
                cur, x = [w], ww
            else:
                x += (sw if cur else 0) + ww
                cur.append(w)
            italic ^= w.count('<I>') % 2 == 1
        if cur or not lines:
            lines.append(' '.join(cur))
        chunks = [lines[i:i + lines_per_box] for i in range(0, len(lines), lines_per_box)]
        for k, ch in enumerate(chunks):
            out.append((lead if k == 0 else '') + '<NL>'.join(ch))
    return '<NB>'.join(out) + end
