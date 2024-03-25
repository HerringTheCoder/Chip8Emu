using System.Diagnostics.Contracts;

namespace Chip8Emu.Core.Extensions;

public static class ByteExtensions
{
    [Pure]
    public static byte WillAdditionOverflow(this byte b, int val)
    {
        return Convert.ToByte(b + val > byte.MaxValue);
    }

    [Pure]
    public static byte WillSubtractionUnderflow(this byte b, int val)
    {
        return Convert.ToByte(b - val < byte.MinValue);
    }
}