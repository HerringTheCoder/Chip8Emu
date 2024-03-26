using Chip8Emu.Core;
using Chip8Emu.SDL;
using Chip8Emu.SDL.Configuration;
using Microsoft.Extensions.Configuration;
using static SDL2.SDL;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var emulatorSettings = new EmulatorSettings();
configuration.GetSection("EmulatorSettings").Bind(emulatorSettings);
var logger = LoggerHelper.GetLogger<Emulator>(emulatorSettings.LogLevel);

var (renderer, window) = Renderer.InitializeRendererAndWindow(logger, emulatorSettings);
var emulator = new Emulator(tickRate: emulatorSettings.GpuTickRate, logger);
var keyboardMapper = new KeyboardMapper(emulatorSettings.Keymap);
var romPath = Path.IsPathRooted(emulatorSettings.RomPath)
    ? Path.Combine(emulatorSettings.RomPath)
    : Path.Combine(Directory.GetCurrentDirectory(), emulatorSettings.RomPath);

var stream = File.OpenRead(romPath);
emulator.LoadProgram(stream);

var cts = new CancellationTokenSource();
bool running = true;
emulator.DisplayUpdated += (_, _) => { Renderer.RenderStates(renderer, emulator.Display, window); };
emulator.ExceptionOccured += (_, _) =>
{
    cts.Cancel();
    running = false;
};

Task.Run(() => emulator.RunAsync(emulatorSettings.CyclesPerSecond, emulatorSettings.OperationsPerCycle, cts.Token));

// Main SDL loop
while (running && !cts.IsCancellationRequested)
{
    while (SDL_PollEvent(out SDL_Event e) == 1)
    {
        switch (e.type)
        {
            case SDL_EventType.SDL_QUIT:
                cts.Cancel();
                running = false;
                break;
            case SDL_EventType.SDL_KEYDOWN:
                emulator.PressedKeyValue =
                    keyboardMapper.TryGetChip8Key(e.key.keysym.sym, out var result)
                        ? result
                        : null;
                break;
            case SDL_EventType.SDL_KEYUP:
                emulator.PressedKeyValue = null;
                break;
        }
    }
}

Renderer.Destroy(renderer, window);