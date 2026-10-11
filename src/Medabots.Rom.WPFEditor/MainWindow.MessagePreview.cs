using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Images;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Preview of a message as the game shows it: one 240x160 screen per box (&lt;NB&gt; starts a new box), with
/// the dialogue font and its width table, two lines per box (&lt;NL&gt;), and the portrait of
/// &lt;PORTRAIT:side, character, expression&gt; (side 1 = left, 0 = right; it stays until changed).
/// Font located per ROM by Text.ExtendedFont.Locate (Metabee (E): font 0x4BFC6C, italic 0x4C59AC, widths
/// 0x3B4E08 / 0x3B5008; text code at 0x40144-0x40504) (128 glyphs of 8x16, 4bpp, colour 1 = background;
/// 2 bytes per glyph in the width tables, the first is the advance). Glyph order: tools' ENCODING / EuropeanNameEncoding.
/// Automatic wrapping (routine 0x080402B4, state at 0x3000440): before each word, the game measures it up to and
/// including the space after it (stopping at a control code) and, if pen x + that width &gt; 208, moves to line 2, or
/// from line 2 to a new box after the button. Pen x and the measured width are bytes: a line past 256 px is drawn
/// over its own start, and a word over 255 px is measured modulo 256 (so it may not wrap).
/// </summary>
public partial class MainWindow
{
    private const int GbaScreenWidth = 240;
    private const int GbaScreenHeight = 160;
    // Metabee (E) layout, used until a ROM is located
    private static readonly Medabots.Rom.Text.FontLayout DefaultFontLayout = new(0x4BFC6C, 0x3B4E08);
    private static Medabots.Rom.Text.FontLayout s_fontLayout = DefaultFontLayout;
    private static byte[]? s_fontLayoutRom;

    // Widths 0x4F-0x7F with the extension; null when the ROM cannot take it
    private static (byte[] Main, byte[] Italic)? s_extendedWidths;
    private const int BoxTop = 104;
    private const int TextLeft = 16;
    private const int TextRight = 228;
    private const int WrapWidth = 208;

    // <MEM:n> is the player's name, measured by the game as one word; up to 8 letters, so at most 8 x 'W' (8 px).
    private const string PlayerNamePlaceholder = "WWWWWWWW";
    private const int PortraitSize = 64;

    // Preview draws the extended glyphs and widths (Text.ExtendedFont) when the project enables them.
    private static bool s_useExtendedFont = true;

    private const string MessageGlyphs =
        " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789·" +
        ".,'-/:?!\"()♥£&%õ" +
        "ÄäÁáÂâÈèÉéÊêËëÎî" +
        "ÏïÍíÖöÔôÓóÜüÛûÙù" +
        "ÚúßÇçÑñ¡¿ÀàÆæÃãÕ";

    private static readonly Regex MessageToken = new(@"<([^<>]*)>|.", RegexOptions.Singleline);

    private sealed record MessageRun(string Text, bool Italic);

    private sealed class MessagePage
    {
        public (int Side, int Character, int Expression)? Portrait { get; set; }

        public List<List<MessageRun>> Lines { get; } = [[]];

        // Lines the game moved here by itself (automatic wrap), and whether the box itself comes from one.
        public HashSet<int> WrappedLines { get; } = [];

        public bool WrappedBox { get; init; }
    }

    private List<MessagePage> _messageBoxes = [];
    private int _messageBoxIndex;

    private void OnMessageTextEditorTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => RebuildMessagePreview(keepBox: true);

    private void OnMessagePreviewPreviousClicked(object sender, RoutedEventArgs e) => ShowMessageBox(_messageBoxIndex - 1);

    private void OnMessagePreviewNextClicked(object sender, RoutedEventArgs e) => ShowMessageBox(_messageBoxIndex + 1);

    private void RebuildMessagePreview(bool keepBox)
    {
        if (MessagePreviewImage is null)
        {
            return;
        }

        s_useExtendedFont = _project.UseExtendedFont;
        UpdateFontLayout(_session?.RomFile.Data);
        _messageBoxes = WrapMessageBoxes(ParseMessageBoxes(MessageTextEditor?.Text ?? ""), _session?.RomFile.Data);
        ShowMessageBox(keepBox ? _messageBoxIndex : 0);
    }

    // Boxes split by <NB>; lines by <NL>; <PORTRAIT> carries over to the next boxes; <I> toggles italics.
    private static List<MessagePage> ParseMessageBoxes(string text)
    {
        var boxes = new List<MessagePage> { new() };
        var italic = false;
        (int, int, int)? portrait = null;
        foreach (Match match in MessageToken.Matches(text))
        {
            var box = boxes[^1];
            if (!match.Groups[1].Success)
            {
                var ch = match.Value;
                if (ch is "\r" or "\n")
                {
                    continue;
                }

                var line = box.Lines[^1];
                if (line.Count > 0 && line[^1].Italic == italic)
                {
                    line[^1] = line[^1] with { Text = line[^1].Text + ch };
                }
                else
                {
                    line.Add(new MessageRun(ch, italic));
                }

                continue;
            }

            var tag = match.Groups[1].Value.Trim();
            var name = tag.Split(':')[0].Trim().ToUpperInvariant();
            switch (name)
            {
                case "PORTRAIT":
                {
                    var numbers = tag.Contains(':') ? tag[(tag.IndexOf(':') + 1)..].Split(',').Select(p => int.TryParse(p.Trim(), out var n) ? n : 0).ToArray() : [];
                    if (numbers.Length >= 3)
                    {
                        portrait = (numbers[0], numbers[1], numbers[2]);
                        box.Portrait = portrait;
                    }

                    break;
                }

                case "NB":
                    boxes.Add(new MessagePage { Portrait = portrait });
                    break;
                case "NL":
                    box.Lines.Add([]);
                    break;
                case "I":
                    italic = !italic;
                    break;
                case "MEM":
                    box.Lines[^1].Add(new MessageRun(PlayerNamePlaceholder, italic));
                    break;
                case "END" or "ENDLST":
                    return boxes;
            }

            box.Portrait ??= portrait;
        }

        return boxes;
    }

    // The game's automatic wrap, applied to the boxes written by hand (see the class summary).
    private static List<MessagePage> WrapMessageBoxes(List<MessagePage> written, byte[]? rom)
    {
        var result = new List<MessagePage>();
        foreach (var source in written)
        {
            var page = new MessagePage { Portrait = source.Portrait };
            result.Add(page);
            for (var lineIndex = 0; lineIndex < source.Lines.Count; lineIndex++)
            {
                if (lineIndex > 0)
                {
                    page.Lines.Add([]);
                }

                var x = 0;
                foreach (var run in source.Lines[lineIndex])
                {
                    for (var i = 0; i < run.Text.Length; i++)
                    {
                        // A word starts the run or follows a space; it is measured with its trailing space.
                        if (i == 0 || run.Text[i - 1] == ' ')
                        {
                            var end = run.Text.IndexOf(' ', i);
                            var word = run.Text[i..(end < 0 ? run.Text.Length : end + 1)];
                            var width = word.Sum(ch => GlyphAdvance(rom, ch, run.Italic)) & 0xFF;
                            if (x > 0 && x + width > WrapWidth)
                            {
                                if (page.Lines.Count < 2)
                                {
                                    page.Lines.Add([]);
                                }
                                else
                                {
                                    page = new MessagePage { Portrait = page.Portrait, WrappedBox = true };
                                    result.Add(page);
                                }

                                page.WrappedLines.Add(page.Lines.Count - 1);
                                x = 0;
                            }
                        }

                        var line = page.Lines[^1];
                        if (line.Count > 0 && line[^1].Italic == run.Italic)
                        {
                            line[^1] = line[^1] with { Text = line[^1].Text + run.Text[i] };
                        }
                        else
                        {
                            line.Add(new MessageRun(run.Text[i].ToString(), run.Italic));
                        }

                        x = (x + GlyphAdvance(rom, run.Text[i], run.Italic)) & 0xFF;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Writes the game's automatic wrap into the text: where a word would not fit, the space before it becomes
    /// &lt;NL&gt; (on line 1) or &lt;NB&gt; (on line 2), and an &lt;NL&gt; typed on line 2 becomes &lt;NB&gt;.
    /// Words wider than a line are left as they are (the preview warns about them).
    /// </summary>
    private static string ApplyMessageLineBreaks(string text, byte[]? rom)
    {
        if (rom is null)
        {
            return text;
        }

        var tokens = MessageToken.Matches(text).Select(m => (Tag: m.Groups[1].Success ? m.Groups[1].Value.Trim().Split(':')[0].Trim().ToUpperInvariant() : null, m.Value)).ToList();
        var output = new System.Text.StringBuilder();
        int x = 0, line = 0;
        var italic = false;
        var wordStart = true;
        for (var i = 0; i < tokens.Count; i++)
        {
            var (tag, value) = tokens[i];
            if (tag is not null)
            {
                switch (tag)
                {
                    case "NL" when line == 1:
                        output.Append("<NB>");
                        (x, line) = (0, 0);
                        break;
                    case "NL":
                        output.Append(value);
                        (x, line) = (0, 1);
                        break;
                    case "NB":
                        output.Append(value);
                        (x, line) = (0, 0);
                        break;
                    case "END" or "ENDLST":
                        output.Append(value);
                        for (var rest = i + 1; rest < tokens.Count; rest++)
                        {
                            output.Append(tokens[rest].Value);
                        }

                        return output.ToString();
                    case "MEM":
                    {
                        // The player's name: a word of its own, at its widest.
                        var width = PlayerNamePlaceholder.Sum(ch => GlyphAdvance(rom, ch, italic));
                        if (x > 0 && x + width > WrapWidth)
                        {
                            BreakBefore();
                        }

                        output.Append(value);
                        x = (x + width) & 0xFF;
                        break;
                    }

                    default:
                        italic ^= tag == "I";
                        output.Append(value);
                        break;
                }

                wordStart = true;                    // a control code ends the game's word measurement
                continue;
            }

            if (value is "\r" or "\n")
            {
                output.Append(value);
                continue;
            }

            if (wordStart)
            {
                // The word up to and including its trailing space, stopping at a control code.
                var width = 0;
                for (var k = i; k < tokens.Count && tokens[k].Tag is null; k++)
                {
                    if (tokens[k].Value is "\r" or "\n")
                    {
                        continue;
                    }

                    width += GlyphAdvance(rom, tokens[k].Value[0], italic);
                    if (tokens[k].Value == " ")
                    {
                        break;
                    }
                }

                if (x > 0 && x + (width & 0xFF) > WrapWidth)
                {
                    BreakBefore();
                }
            }

            output.Append(value);
            x = (x + GlyphAdvance(rom, value[0], italic)) & 0xFF;
            wordStart = value == " ";
        }

        return output.ToString();

        // The space before the word becomes the break.
        void BreakBefore()
        {
            var end = output.Length;
            while (end > 0 && output[end - 1] == ' ')
            {
                end--;
            }

            output.Length = end;
            output.Append(line == 0 ? "<NL>" : "<NB>");
            line = line == 0 ? 1 : 0;
            x = 0;
        }
    }

    private static int GlyphIndex(char ch) => MessageGlyphs.IndexOf(ch) is var glyph and >= 0 ? glyph : MessageGlyphs.IndexOf('?');

    private static int GlyphAdvance(byte[]? rom, char ch, bool italic)
    {
        var glyph = GlyphIndex(ch);
        if (s_useExtendedFont && s_extendedWidths is { } ext && glyph >= Medabots.Rom.Text.ExtendedFont.FirstWidthGlyph)
        {
            var extendedAdvance = (italic ? ext.Italic : ext.Main)[(glyph - Medabots.Rom.Text.ExtendedFont.FirstWidthGlyph) * 2];
            return extendedAdvance is > 0 and <= 16 ? extendedAdvance : 6;
        }

        var widths = (italic ? s_fontLayout.ItalicWidths : s_fontLayout.Widths) + glyph * 2;
        var advance = rom is not null && widths < rom.Length ? rom[widths] : 6;  // the second byte is not spacing
        return advance is > 0 and <= 16 ? advance : 6;
    }

    private void ShowMessageBox(int index)
    {
        if (_messageBoxes.Count == 0)
        {
            return;
        }

        _messageBoxIndex = Math.Clamp(index, 0, _messageBoxes.Count - 1);
        var box = _messageBoxes[_messageBoxIndex];
        var (bitmap, warnings) = RenderMessageBox(box);
        MessagePreviewImage.Source = bitmap;
        var portrait = box.Portrait is { } p ? $"portrait: character {p.Character}, expression {p.Expression}, {(p.Side == 1 ? "left" : "right")}" : "no portrait";
        var wrapped = box.WrappedBox ? "  |  new box from automatic wrap" : box.WrappedLines.Count > 0 ? "  |  line 2 from automatic wrap" : "";
        MessagePreviewLabel.Text = $"Box {_messageBoxIndex + 1} of {_messageBoxes.Count}  |  {portrait}{wrapped}" + (warnings.Count > 0 ? "  |  " + string.Join("; ", warnings) : "");
        MessagePreviewLabel.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, warnings.Count > 0 ? "Theme.Changed" : "Theme.TextMuted");
    }

    private (BitmapSource Bitmap, List<string> Warnings) RenderMessageBox(MessagePage box)
    {
        var pixels = new uint[GbaScreenWidth * GbaScreenHeight];
        var warnings = new List<string>();
        Fill(pixels, 0, 0, GbaScreenWidth, GbaScreenHeight, 0xFF3A3F47);           // stand-in for the map

        // Portrait above the box, on its side.
        if (box.Portrait is { } portrait && TryGetPortraitImage(portrait.Character, portrait.Expression) is { } image)
        {
            var left = portrait.Side == 1 ? 8 : GbaScreenWidth - 8 - PortraitSize;
            var colours = Bgr555Colours(image.PaletteBytes);
            for (var y = 0; y < Math.Min(image.Height, PortraitSize); y++)
            {
                for (var x = 0; x < Math.Min(image.Width, PortraitSize); x++)
                {
                    // The portraits face left; on the left side the game mirrors them to face the box.
                    var sourceX = portrait.Side == 1 ? Math.Min(image.Width, PortraitSize) - 1 - x : x;
                    var index = image.PixelIndices[TilePixel(image, sourceX, y)];
                    if (index != 0)
                    {
                        pixels[(BoxTop - PortraitSize + y) * GbaScreenWidth + left + x] = colours[index];
                    }
                }
            }
        }

        // The box: purple/blue frame around a yellow panel, as in the game.
        Fill(pixels, 2, BoxTop, GbaScreenWidth - 4, GbaScreenHeight - BoxTop - 2, 0xFF5A62B8);
        Fill(pixels, 4, BoxTop + 3, GbaScreenWidth - 8, GbaScreenHeight - BoxTop - 7, 0xFF9AA4E6);
        Fill(pixels, 6, BoxTop + 6, GbaScreenWidth - 12, GbaScreenHeight - BoxTop - 13, 0xFFFFF25A);

        var rom = _session?.RomFile.Data;
        for (var lineIndex = 0; lineIndex < box.Lines.Count; lineIndex++)
        {
            // The pen is a byte, as in the game: past 256 px the line is drawn over its own start.
            var width = 0;
            var top = BoxTop + 10 + lineIndex * 16;
            foreach (var run in box.Lines[lineIndex])
            {
                foreach (var ch in run.Text)
                {
                    if (MessageGlyphs.IndexOf(ch) < 0 && !warnings.Contains($"no glyph for '{ch}'"))
                    {
                        warnings.Add($"no glyph for '{ch}'");
                    }

                    DrawGlyph(pixels, rom, GlyphIndex(ch), run.Italic, TextLeft + (width & 0xFF), top);
                    width += GlyphAdvance(rom, ch, run.Italic);
                }
            }

            if (width > TextRight - TextLeft)
            {
                warnings.Add(width > 0xFF
                    ? $"line {lineIndex + 1} is {width} px wide: past 256 it is drawn over its start (a word with no spaces does not wrap)"
                    : $"line {lineIndex + 1} is {width} px wide, the box shows {TextRight - TextLeft}");
                Fill(pixels, TextRight, top, 1, 16, 0xFFE11D48);
            }
        }

        if (box.Lines.Count > 2)
        {
            warnings.Add($"{box.Lines.Count} lines: the box shows 2");
        }

        var bytes = new byte[pixels.Length * 4];
        Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
        var bitmap = BitmapSource.Create(GbaScreenWidth, GbaScreenHeight, 96, 96, PixelFormats.Bgra32, null, bytes, GbaScreenWidth * 4);
        bitmap.Freeze();
        return (bitmap, warnings);
    }

    // Draws an 8x16 glyph (two 4bpp tiles, colour 0/1 = background), clipped to the visible part of the line.
    private static void DrawGlyph(uint[] pixels, byte[]? rom, int glyph, bool italic, int left, int top)
    {
        var font = italic ? s_fontLayout.ItalicFont : s_fontLayout.Font;
        if (rom is null || font + 128 * 64 > rom.Length)
        {
            return;
        }

        for (var tile = 0; tile < 2; tile++)
        {
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var value = GlyphPixel(rom, glyph, italic, x, tile * 8 + y);
                    var px = left + x;
                    var py = top + tile * 8 + y;
                    if (value > 1 && px >= TextLeft && px < TextRight + 2 && py < GbaScreenHeight)
                    {
                        // 2-F are anti-aliasing levels over the background (F = full ink).
                        pixels[py * GbaScreenWidth + px] = Blend(0xFFFFF25A, 0xFF3A2A10, (value - 1) / 14.0);
                    }
                }
            }
        }
    }

    private IndexedImage? TryGetPortraitImage(int character, int expression)
    {
        try
        {
            return GetPreviewPortraitAsset(character, expression).Image;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IndexOutOfRangeException or InvalidDataException or KeyNotFoundException)
        {
            return null;
        }
    }

    private static int TilePixel(IndexedImage image, int x, int y) => ((y / 8) * image.TileWidth + x / 8) * 64 + (y % 8) * 8 + x % 8;

    private static uint[] Bgr555Colours(byte[] palette)
    {
        var colours = new uint[16];
        for (var i = 0; i < 16 && i * 2 + 1 < palette.Length; i++)
        {
            var c = palette[i * 2] | palette[i * 2 + 1] << 8;
            colours[i] = 0xFF000000u | (uint)((c & 31) << 3) << 16 | (uint)((c >> 5 & 31) << 3) << 8 | (uint)((c >> 10 & 31) << 3);
        }

        return colours;
    }

    private static void Fill(uint[] pixels, int x, int y, int width, int height, uint colour)
    {
        for (var py = Math.Max(0, y); py < Math.Min(GbaScreenHeight, y + height); py++)
        {
            for (var px = Math.Max(0, x); px < Math.Min(GbaScreenWidth, x + width); px++)
            {
                pixels[py * GbaScreenWidth + px] = colour;
            }
        }
    }

    // Locates the font once per ROM image; keeps the Metabee (E) layout when not found.
    private static void UpdateFontLayout(byte[]? rom)
    {
        if (rom is null || ReferenceEquals(rom, s_fontLayoutRom))
        {
            return;
        }

        s_fontLayout = Medabots.Rom.Text.ExtendedFont.Locate(rom) ?? DefaultFontLayout;
        s_extendedWidths = Medabots.Rom.Text.ExtendedFont.CanApply(rom, out var layout)
            ? Medabots.Rom.Text.ExtendedFont.BuildWidths(rom, layout)
            : null;
        s_fontLayoutRom = rom;
    }

    // Palette index (0-F) of a glyph pixel; extended glyphs replace the blank slots when enabled.
    private static int GlyphPixel(byte[] rom, int glyph, bool italic, int x, int y)
    {
        var offset = (y / 8) * 32 + (y % 8) * 4 + x / 2;
        var extended = s_useExtendedFont && s_extendedWidths is not null ? Medabots.Rom.Text.ExtendedFont.GetGlyph(glyph, italic) : default;
        var b = extended.IsEmpty ? rom[(italic ? s_fontLayout.ItalicFont : s_fontLayout.Font) + glyph * 64 + offset] : extended[offset];
        return (x & 1) == 1 ? b >> 4 : b & 15;
    }

    private static uint Blend(uint a, uint b, double t)
    {
        uint Channel(int shift) => (uint)Math.Round(((a >> shift) & 0xFF) * (1 - t) + ((b >> shift) & 0xFF) * t) << shift;
        return 0xFF000000u | Channel(16) | Channel(8) | Channel(0);
    }
}
