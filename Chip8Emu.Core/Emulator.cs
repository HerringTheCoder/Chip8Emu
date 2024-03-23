using System.Diagnostics;
using Chip8Emu.Core.Components;

namespace Chip8Emu.Core;

public partial class Emulator
{
    public Display Display { get; } = new();
    public Memory Memory { get; } = new();
    public Registers Registers { get; } = new();
    public Stack<ushort> Stack { get; } = new();
    public int ProgramCounter { get; private set; }

    public void LoadProgram(Stream stream)
    {
        Memory.LoadProgram(stream);
        Debug.WriteLine("Program loaded successfully");
    }

    public async Task RunProgram(int operationsPerSecond, CancellationToken cancellationToken)
    {
        var pauseInterval = TimeSpan.FromSeconds(1.0 / operationsPerSecond);
        ProgramCounter = Memory.UserSpace.Start;
        
        while (true)
        {
            var instruction = Memory.ReadWord(ProgramCounter);
            ProgramCounter += 2;
            DecodeAndExecuteInstruction(instruction);
            await Task.Delay(pauseInterval, cancellationToken);
        }
    }

    public void DecodeAndExecuteInstruction(ushort instruction)
    {
        var operation = new Operation(
            instruction,
            OpCode: (byte)((instruction & 0xF000) >> 12),
            X: (byte)((instruction & 0x0F00) >> 8),
            Y: (byte)((instruction & 0x00F0) >> 4),
            N: (byte)(instruction & 0x000F),
            NN: (byte)(instruction & 0x00FF),
            NNN: (ushort)(instruction & 0x0FFF)
        );

        Commands[operation.OpCode].Invoke(operation);
    }
}