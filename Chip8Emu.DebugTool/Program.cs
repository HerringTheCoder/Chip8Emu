// See https://aka.ms/new-console-template for more information

using Chip8Emu.Core;
using Chip8Emu.DebugTool;

Console.WriteLine("Hello, World!");

var emulator = new Emulator(tickRate: 60, LoggerHelper.GetLogger<Emulator>());

// var file = "IBM_Logo.ch8";
// var file = "pong.ch8";
// var file = "chip8_audio.ch8"; No sound is played
// var file = "keshaWasBiird.ch8"; 64Kb rom
var file = "glitchGhost.ch8";

var stream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Roms", file));
var debugBuffer = new byte[4096];
var debugStream = new MemoryStream(debugBuffer, true);
stream.CopyTo(debugStream);
stream.Position = 0;
emulator.LoadProgram(stream);
emulator.DisplayUpdated += EmulatorOnDisplayUpdated;

void EmulatorOnDisplayUpdated(object? sender, EventArgs e)
{
    Console.Clear();
    Console.WriteLine("\x1b[3J");
    for (int y = 0; y < 32; y++)
    {
        for (int x = 0; x < 64; x++)
        {
            Console.Write(emulator.Display.States[x, y] ? "#" : " ");
        }
    
        Console.WriteLine();
    }
}

stream.Dispose();
await emulator.RunAsync(cyclesPerSecond: 500, operationsPerCycle: 5, CancellationToken.None);
