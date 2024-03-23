namespace Chip8Emu.Core.Extensions;

public static class ByteExtensions
{
    public static byte WillAdditionOverflow(this byte b, int val)
    {
        return Convert.ToByte(byte.MaxValue - b < val);
    }

    public static byte WillSubtractionUnderflow(this byte b, int val)
    {
        return Convert.ToByte(b - byte.MinValue < val);
    }
}