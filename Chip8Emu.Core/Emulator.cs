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
    public byte? PressedKeyValue = null;
    public bool IsKeyLockActive = false;
    public AsyncTimer DelayTimer { get; }
    public AsyncTimer SoundTimer { get; }

    public event EventHandler DisplayUpdated;

    protected virtual void OnDisplayUpdated(EventArgs e)
    {
        DisplayUpdated?.Invoke(this, e);
    }

    public event EventHandler ExceptionOccured;

    protected virtual void OnExceptionOccured(EventArgs e)
    {
        ExceptionOccured.Invoke(this, e);
    }

    public Emulator(int tickRate)
    {
        DelayTimer = new AsyncTimer(tickRate);
        SoundTimer = new AsyncTimer(tickRate);
        ProgramCounter = Memory.UserSpace.Start;
    }

    public void LoadProgram(Stream stream)
    {
        Memory.LoadProgram(stream);
        Debug.WriteLine("Program loaded successfully");
    }

    public async Task RunAsync(int operationsPerSecond, CancellationToken cancellationToken)
    {
        var pauseInterval = TimeSpan.FromSeconds(1.0 / operationsPerSecond);

        try
        {
            await Task.WhenAll(
                MainLoopTask(pauseInterval, cancellationToken),
                RefreshDisplayTask(cancellationToken),
                UpdateSoundStateTask(cancellationToken));
        }
        catch(AggregateException ex)
        {
            Debug.WriteLine(ex.InnerExceptions);
            throw;
        }
    }

    public async Task MainLoopTask(TimeSpan pauseInterval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!IsKeyLockActive)
                {
                    var instruction = Memory.ReadWord(ProgramCounter);
                    DecodeAndExecuteInstruction(instruction);
                    ProgramCounter += ProgramCounterStep;
                    Debug.WriteLine($"Moving ProgramCounter to address: {ProgramCounter}");
                }

                await Task.Delay(pauseInterval, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Main loop exception catched with message {Exception}", ex);
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
                Debug.WriteLine("DisplayTask exception catched!");
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
                Debug.WriteLine("SoundStateTask exception catched!");
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
        Debug.WriteLine($"Invoking command: {command.Method.Name} with instruction: {instruction:X} {operation}");
        command.Invoke(operation);
    }

    private void SkipNextInstruction()
    {
        ProgramCounter = ProgramCounter.Add(ProgramCounterStep);
    }
}