namespace Chip8Emu.Core;

public partial class Emulator
{
    public IDictionary<byte, Action<Operation>> Commands => new Dictionary<byte, Action<Operation>>
    {
        { 0x0, SpecialCommand },
        { 0x1, JumpToAddressNNN },
        { 0xA, SetIRegister },
        { 0x6, SetVxRegister },
        { 0xD, DisplaySprite },
        { 0x7, AddNNToVx},
    };

    public void SpecialCommand(Operation operation)
    {
        if (operation.Y == 0xE)
        {
            Display.ClearScreen();
        }
    }

    public void JumpToAddressNNN(Operation operation)
    {
        ProgramCounter = operation.NNN;
    }

    public void SetIRegister(Operation operation)
    {
        Registers.I = operation.NNN;
    }

    public void SetVxRegister(Operation operation)
    {
        Registers.VRegisters[operation.X] = operation.NN;
    }

    public void DisplaySprite(Operation operation)
    {
        Registers.VRegisters[0xF] = 0;
        var sprite = Memory.GetSprite(Registers.I, operation.N);

        if (Display.DrawSprite(Registers.VRegisters[operation.X], Registers.VRegisters[operation.Y], sprite))
            Registers.VRegisters[0xF] = 1;
        
        // if (Display.DrawSprite(sprite, Registers.VRegisters[operation.X], Registers.VRegisters[operation.Y], operation.N))
        //     Registers.VRegisters[0xF] = 1;
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Console.Write(Display.States[x, y] ? "#" : " ");
            }
            Console.WriteLine();
        }
    }

    public void AddNNToVx(Operation operation)
    {
        Registers.VRegisters[operation.X] += operation.NN;
    }
}