using Microsoft.Extensions.Logging;
using static SDL2.SDL;

namespace Chip8Emu.SDL.Configuration;

public class EmulatorSettings
{
    public int CyclesPerSecond { get; set; }
    public int OperationsPerCycle { get; set; }
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public int GpuTickRate { get; set; }
    public string Filename { get; set; } = null!;
    public IDictionary<SDL_Keycode, byte> Keymap { get; set; } = null!;
    public LogLevel LogLevel { get; set; }
}