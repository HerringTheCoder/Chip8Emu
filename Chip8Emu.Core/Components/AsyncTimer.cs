using System.Threading.Channels;

namespace Chip8Emu.Core.Components;

public class AsyncTimer
{
    public PeriodicTimer Clock { get; }
    private readonly object _actionRequestedLock = new();

    private bool _isActionRequested; 
    public bool IsActionRequested
    {
        get
        {
            lock (_actionRequestedLock)
            {
                return _isActionRequested;
            }
        }
        set
        {
            lock (_actionRequestedLock)
            {
                _isActionRequested = value;
            }
        }
    }
    
    private readonly object _counterLock = new();

    private byte _counter = 0;

    public byte Counter
    {
        get
        {
            lock (_counterLock)
            {
                return _counter;
            }
        }
        set
        {
            lock (_counterLock)
            {
                _counter = _counter > 0x0 ? value : (byte)0;
            }
        }
    }
    
    

    internal AsyncTimer(int tickRate = 60)
    {
        Clock = new PeriodicTimer(TimeSpan.FromSeconds(1.0/tickRate));
    }
}