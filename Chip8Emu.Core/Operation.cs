namespace Chip8Emu.Core;

public record Operation
(
    ushort Instruction,
    byte OpCode,
    byte X,
    byte Y,
    byte N,
    byte NN,
    ushort NNN
    );