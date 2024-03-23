// ReSharper disable InconsistentNaming
namespace Chip8Emu.Core.Components;

public class Registers
{
    /// <summary>
    /// V0 - VF registers. VF register is a carry flag.
    /// </summary>
    public readonly byte[] V = new byte[16];
    public ushort I { get; set; }

    internal Registers()
    {
    }
}