using Medabots.Rom.Text;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class MedabotsTextCodecTests
{
    private readonly MedabotsTextCodec _codec = new();

    [Fact]
    public void EncodeAndDecode_RoundTripsSupportedCommands()
    {
        const string text = "<PORTRAIT:1, 2, 3>Hello<NL>World<NB><I><END:0>";

        var encoded = _codec.Encode(text);
        var decoded = _codec.Decode(encoded);

        Assert.Equal(
            [0xFB, 0x01, 0x02, 0x03, 0x08, 0x1F, 0x26, 0x26, 0x29, 0xFD, 0x17, 0x29, 0x2C, 0x26, 0x1E, 0xFC, 0xF8, 0xFF, 0x00],
            encoded);
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void EncodeAndDecode_RoundTripsExtendedGlyphs()
    {
        const string text = "Ãõã Õ àÆ";

        var encoded = _codec.Encode(text);

        Assert.Equal([0x7D, 0x4F, 0x7E, 0x00, 0x7F, 0x00, 0x7A, 0x7B], encoded);
        Assert.Equal(text, _codec.Decode(encoded));
    }

    [Fact]
    public void ReadEncodedMessage_StopsAtEndCommand()
    {
        byte[] romData = [0x01, 0x02, 0xFF, 0x00, 0x55, 0x66];

        var encoded = _codec.ReadEncodedMessage(romData, 0);

        Assert.Equal([0x01, 0x02, 0xFF, 0x00], encoded);
    }
}
