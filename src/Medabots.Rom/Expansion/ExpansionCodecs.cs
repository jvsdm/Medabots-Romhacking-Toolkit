namespace Medabots.Rom.Expansion;

/// <summary>Block sizes of the compressed formats the expansion copies, and a literal-only Malias writer.</summary>
internal static class ExpansionCodecs
{
    /// <summary>Malias ("Le") payload and the number of bytes the compressed block occupies.</summary>
    public static (byte[] Payload, int CompressedSize) MaliasDecompress(ReadOnlySpan<byte> data, int offset)
    {
        if (data[offset] != (byte)'L' || data[offset + 1] != (byte)'e')
        {
            throw new InvalidDataException($"No Malias block at 0x{offset:X}.");
        }

        var length = data[offset + 2] | data[offset + 3] << 8 | data[offset + 4] << 16;
        var src = offset + 6;
        var output = new List<byte>(length);
        var remaining = length;
        while (remaining > 0)
        {
            int command = data[src++];
            for (var op = 0; op < 4 && remaining > 0; op++)
            {
                switch (command & 3)
                {
                    case 0:
                    {
                        var value = data[src] | data[src + 1] << 8;
                        src += 2;
                        int distance = (value & 0xFFF) + 5, count = (value >> 12) + 3;
                        for (var i = 0; i < count; i++)
                        {
                            output.Add(output[^distance]);
                        }

                        remaining -= count;
                        break;
                    }
                    case 1:
                    {
                        int value = data[src++];
                        int distance = (value & 3) + 1, count = (value >> 2) + 2;
                        for (var i = 0; i < count; i++)
                        {
                            output.Add(output[^distance]);
                        }

                        remaining -= count;
                        break;
                    }
                    case 2:
                        output.Add(data[src++]);
                        remaining -= 1;
                        break;
                    default:
                        output.Add(data[src]);
                        output.Add(data[src + 1]);
                        output.Add(data[src + 2]);
                        src += 3;
                        remaining -= 3;
                        break;
                }

                command >>= 2;
            }
        }

        return (output.GetRange(0, length).ToArray(), src - offset);
    }

    /// <summary>Size of the BIOS LZ77 (0x10) block at <paramref name="offset"/>.</summary>
    public static int Lz77CompressedSize(ReadOnlySpan<byte> data, int offset)
    {
        if (data[offset] != 0x10)
        {
            throw new InvalidDataException($"No LZ77 block at 0x{offset:X}.");
        }

        var length = data[offset + 1] | data[offset + 2] << 8 | data[offset + 3] << 16;
        var src = offset + 4;
        var produced = 0;
        while (produced < length)
        {
            int flags = data[src++];
            for (var bit = 0; bit < 8 && produced < length; bit++)
            {
                if ((flags & (0x80 >> bit)) != 0)
                {
                    produced += (data[src] >> 4) + 3;
                    src += 2;
                }
                else
                {
                    produced++;
                    src++;
                }
            }
        }

        return src - offset;
    }

    public static int CompressedSize(ReadOnlySpan<byte> data, int offset) =>
        data[offset] == (byte)'L' && data[offset + 1] == (byte)'e'
            ? MaliasDecompress(data, offset).CompressedSize
            : Lz77CompressedSize(data, offset);

    /// <summary>A Malias block made of literals only (mode 3 = 3 bytes, mode 2 = 1 byte).</summary>
    public static byte[] MaliasStore(ReadOnlySpan<byte> payload)
    {
        var output = new List<byte> { (byte)'L', (byte)'e', (byte)payload.Length, (byte)(payload.Length >> 8), (byte)(payload.Length >> 16), 0 };
        var i = 0;
        while (i < payload.Length)
        {
            var command = 0;
            var body = new List<byte>(12);
            for (var op = 0; op < 4 && i < payload.Length; op++)
            {
                var count = payload.Length - i >= 3 ? 3 : 1;
                command |= (count == 3 ? 3 : 2) << (2 * op);
                for (var k = 0; k < count; k++)
                {
                    body.Add(payload[i + k]);
                }

                i += count;
            }

            output.Add((byte)command);
            output.AddRange(body);
        }

        return output.ToArray();
    }
}

/// <summary>Name encoding of the European release (the font's 0x00-0x7F glyphs; names end in 0xFE).</summary>
internal static class EuropeanNameEncoding
{
    private const string Glyphs =
        " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789·" +
        ".,'-/:?!\"()♥£&%õ" +
        "ÄäÁáÂâÈèÉéÊêËëÎî" +
        "ÏïÍíÖöÔôÓóÜüÛûÙù" +
        "ÚúßÇçÑñ¡¿ÀàÆæÃãÕ";

    public static byte[] Encode(string text)
    {
        var bytes = new byte[text.Length + 1];
        for (var i = 0; i < text.Length; i++)
        {
            var index = Glyphs.IndexOf(text[i], StringComparison.Ordinal);
            if (index < 0)
            {
                throw new InvalidDataException($"No glyph for '{text[i]}' in \"{text}\".");
            }

            bytes[i] = (byte)index;
        }

        bytes[^1] = 0xFE;
        return bytes;
    }
}
