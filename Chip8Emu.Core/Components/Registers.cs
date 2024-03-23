// ReSharper disable InconsistentNaming
namespace Chip8Emu.Core.Components;

public class Registers
{
    public byte[] VRegisters = new byte[16];
    //carry register
    public ushort I { get; set; }

    internal Registers()
    {
    }
}