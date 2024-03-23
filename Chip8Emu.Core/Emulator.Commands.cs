using System.Diagnostics;
using Chip8Emu.Core.Extensions;
using Chip8Emu.Core.Resources;
using static System.Byte;

// ReSharper disable InconsistentNaming

namespace Chip8Emu.Core;

public partial class Emulator
{
    public IDictionary<byte, Action<Operation>> Commands => new Dictionary<byte, Action<Operation>>
    {
        { 0x0, SpecialCommand },
        { 0x1, JumpToAddressNNN },
        { 0x2, CallSubroutine },
        { 0x3, SkipNextInstructionIfVxEqNN },
        { 0x4, SkipNextInstructionIfVxNeqNN },
        { 0x5, SkipNextInstructionIfVxEqVy },
        { 0x6, SetVxRegister },
        { 0x7, AddNNToVxNoCarry },
        { 0x8, SetVx },
        { 0x9, SkipNextInstructionIfVxNeqVy },
        { 0xA, SetIRegisterToNNN },
        { 0xB, JumpToAddressV0PlusNNN },
        { 0xC, SetVxToRandAndNN },
        { 0xD, DisplaySprite },
        { 0xE, SkipNextInstructionBasedOnKeyState },
        { 0xF, HandleFGroupOperation }
    };

    private void SpecialCommand(Operation op)
    {
        if (op.NN == 0xEE)
        {
            ReturnFromSubroutine();

            void ReturnFromSubroutine()
            {
                ProgramCounter = Stack.Pop();
            }
        }
        else if (op.Y == 0xE)
        {
            Display.ClearScreen();
        }
    }

    private void JumpToAddressNNN(Operation op) 
    {
        //Compensate for PC step
        ProgramCounter = op.NNN.Subtract(ProgramCounterStep);
    }

    private void CallSubroutine(Operation op)
    {
        Stack.Push(ProgramCounter);
        //Compensate for the global PC increment, because we want to specifically call NNN on the next cycle
        ProgramCounter = op.NNN.Subtract(ProgramCounterStep);
    }

    private void SkipNextInstructionIfVxEqNN(Operation op)
    {
        if (Registers.V[op.X] == op.NN)
            SkipNextInstruction();
    }

    private void SkipNextInstructionIfVxNeqNN(Operation op)
    {
        if (Registers.V[op.X] != op.NN)
            SkipNextInstruction();
    }

    private void SkipNextInstructionIfVxEqVy(Operation op)
    {
        if (Registers.V[op.Y] == Registers.V[op.X])
            SkipNextInstruction();
    }

    private void SetVxRegister(Operation op)
    {
        Registers.V[op.X] = op.NN;
    }

    private void AddNNToVxNoCarry(Operation op)
    {
        Registers.V[op.X] += op.NN;
    }

    private void SetVx(Operation op)
    {
        byte registerVy = Registers.V[op.Y];

        switch (op.N)
        {
            case 0:
                Registers.V[op.X] = registerVy;
                break;
            case 1:
                Registers.V[op.X] |= registerVy;
                break;
            case 2:
                Registers.V[op.X] &= registerVy;
                break;
            case 3:
                Registers.V[op.X] ^= registerVy;
                break;
            case 4:
                Registers.V[0xF] = Registers.V[op.X].WillAdditionOverflow(registerVy);
                Registers.V[op.X] += registerVy;
                break;
            case 5:
                Registers.V[0xF] = Registers.V[op.X].WillSubtractionUnderflow(registerVy);
                Registers.V[op.X] -= registerVy;
                break;
            case 6:
                Registers.V[op.X] >>= 1;
                break;
            case 7:
                Registers.V[0xF] = registerVy.WillSubtractionUnderflow(Registers.V[op.X]);
                Registers.V[op.X] -= registerVy;
                break;
            case 0xE:
                Registers.V[op.X] <<= 1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(op.N),
                    $"Unsupported 0x000N value: '{op.N:X}'" +
                    $"on instruction: '{op.Instruction}'");
        }
    }

    private void SkipNextInstructionIfVxNeqVy(Operation op)
    {
        if (Registers.V[op.X] != Registers.V[op.Y])
            SkipNextInstruction();
    }

    private void SetIRegisterToNNN(Operation op)
    {
        Registers.I = op.NNN;
    }

    private void JumpToAddressV0PlusNNN(Operation op)
    {
        if (Registers.V[0].WillAdditionOverflow(op.NNN) == 0x1)
            Debug.WriteLine(
                $"Overflow detected on instruction : {op.Instruction} in method {nameof(JumpToAddressV0PlusNNN)}");

        ProgramCounter = ((ushort)(Registers.V[0] + op.NNN)).Subtract(ProgramCounterStep);
    }

    private void SetVxToRandAndNN(Operation op)
    {
        Registers.V[op.X] = (byte)Random.Shared.Next(MinValue + op.NN, MaxValue + op.NN);
    }

    private void DisplaySprite(Operation op)
    {
        Registers.V[0xF] = 0;
        var sprite = Memory.GetSprite(Registers.I, op.N);

        if (Display.DrawSprite(Registers.V[op.X], Registers.V[op.Y], sprite))
            Registers.V[0xF] = 1;

        DelayTimer.IsActionRequested = true;
    }

    private void SkipNextInstructionBasedOnKeyState(Operation op)
    {
        switch (op.NN)
        {
            case 0x9E when PressedKeyValue == Registers.V[op.X]:
            case 0xA1 when PressedKeyValue != Registers.V[op.X]:
                SkipNextInstruction();
                break;
        }
    }

    private void HandleFGroupOperation(Operation op)
    {
        switch (op.NN)
        {
            case 0x07:
                Registers.V[op.X] = DelayTimer.Counter;
                break;
            case 0x0A:
                var key = ' ';
                while (!KeyConfiguration.AllowedKeys.Contains(key))
                {
                    key = Console.ReadKey().KeyChar;
                }

                break;
            case 0x15:
                DelayTimer.Counter = Registers.V[op.X];
                break;
            case 0x18:
                var vx = Registers.V[op.X];
                SoundTimer.Counter = vx;
                if (vx > 0)
                {
                    SoundTimer.IsActionRequested = true;
                }

                break;
            case 0x1E:
                Registers.I += Registers.V[op.X];
                break;
            case 0x29:
                //Actual multiplier depends on the system font position.
                //*5 assumes there are 5 bytes for each character and the font starts at address 0 
                Registers.I = (ushort)(Registers.V[op.X] * 5);
                break;
            case 0x33:
                var vxValue = Registers.V[op.X];
                for (int i = 0; i < 3; i++)
                {
                    byte digit = (byte)(vxValue % 10);
                    Memory.WriteByte((ushort)(Registers.I + i), digit);
                }

                break;
            case 0x55:
                for (int i = 0; i < op.X; i++)
                {
                    Memory.WriteByte((ushort)(Registers.I + i), Registers.V[i]);
                }

                break;
            case 0x65:
                for (int i = 0; i < op.X; i++)
                {
                    Registers.V[i] = Memory.ReadByte((ushort)(Registers.I + 1));
                }

                break;
        }
    }
}