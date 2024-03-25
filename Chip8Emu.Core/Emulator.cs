using System.Diagnostics;
using Chip8Emu.Core.Components;
using Chip8Emu.Core.Extensions;

namespace Chip8Emu.Core;

public partial class Emulator
{
    public Display Display { get; } = new();
    public Memory Memory { get; } = new();
    public Registers Registers { get; } = new();
    public Stack<ushort> Stack { get; } = new();
    public ushort ProgramCounter { get; private set; }
    public const ushort ProgramCounterStep = 2;
    public AsyncTimer DelayTimer { get; }
    public AsyncTimer SoundTimer { get; }
    public byte? PressedKeyValue = null;
    public event EventHandler DisplayUpdated = null!;

    protected virtual void OnDisplayUpdated(EventArgs e)
    {
        DisplayUpdated?.Invoke(this, e);
    }

    public event EventHandler ExceptionOccured = null!;

    protected virtual void OnExceptionOccured(EventArgs e)
    {
        ExceptionOccured.Invoke(this, e);
    }

    public Emulator(int tickRate)
    {
        DelayTimer = new AsyncTimer(tickRate);
        SoundTimer = new AsyncTimer(tickRate);
        ProgramCounter = Memory.UserSpace.Start;

        Commands = new Dictionary<byte, Action<Operation>>
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
    }

    public void LoadProgram(Stream stream)
    {
        Memory.LoadProgram(stream);
        Debug.WriteLine("Program loaded successfully");
    }

    public async Task RunAsync(int cyclesPerSecond, int operationsPerCycle, CancellationToken cancellationToken)
    {
        try
        {
            await Task.WhenAll(
                MainLoopTask(cyclesPerSecond, operationsPerCycle, cancellationToken),
                RefreshDisplayTask(cancellationToken),
                UpdateSoundStateTask(cancellationToken));
        }
        catch (AggregateException ex)
        {
            Debug.WriteLine(ex.InnerExceptions.Select(x => x.Message));
            throw;
        }
        catch (Exception ex)
        { 
            Debug.WriteLine(ex.Message);
        }
    }

    public async Task MainLoopTask(int cyclesPerSecond, int operationsPerCycle, CancellationToken cancellationToken)
    {
        var cpuInterval = TimeSpan.FromSeconds(1.0 / cyclesPerSecond);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                for (int i = 0; i < operationsPerCycle; i++)
                {
                    var instruction = Memory.ReadWord(ProgramCounter);
                    DecodeAndExecuteInstruction(instruction);
                    ProgramCounter += ProgramCounterStep;
                    Debug.WriteLine($"Moving ProgramCounter to address: {ProgramCounter}");
                }

                await Task.Delay(cpuInterval, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Main loop exception caught with message {Exception}", ex.Message);
                OnExceptionOccured(new ErrorEventArgs(ex));
                throw;
            }
        }
    }

    public async Task RefreshDisplayTask(CancellationToken cancellationToken)
    {
        while (await DelayTimer.Clock.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                if (DelayTimer.IsActionRequested)
                {
                    DelayTimer.IsActionRequested = false;
                    DelayTimer.Counter--;
                    OnDisplayUpdated(EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DisplayTask exception caught!");
                throw;
            }
        }
    }

    public async Task UpdateSoundStateTask(CancellationToken cancellationToken)
    {
        while (await SoundTimer.Clock.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                if (SoundTimer is { Counter: > 0, IsActionRequested: true })
                {
                    SoundTimer.IsActionRequested = false;
                    Console.Beep(440, SoundTimer.Counter * (int)SoundTimer.Clock.Period.TotalMilliseconds);
                }

                SoundTimer.Counter--;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SoundStateTask exception caught!");
                throw;
            }
        }
    }


    private void DecodeAndExecuteInstruction(ushort instruction)
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

        var command = Commands[operation.OpCode];
        Debug.WriteLine($"{instruction:X} - Invoking command: {command.Method.Name} with operation - {operation}");
        command.Invoke(operation);
    }

    private void SkipNextInstruction()
    {
        ProgramCounter = ProgramCounter.Add(ProgramCounterStep);
    }
}