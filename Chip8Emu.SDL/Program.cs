using Chip8Emu.Core;
using Chip8Emu.SDL;
using Chip8Emu.SDL.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDL2;
using static SDL2.SDL;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var emulatorSettings = new EmulatorSettings();
configuration.GetSection("EmulatorSettings").Bind(emulatorSettings);
var logger = LoggerHelper.GetLogger<Emulator>(emulatorSettings.LogLevel);

if (SDL_Init(SDL_INIT_VIDEO) < 0)
{
    logger.LogCritical("There was an issue initializing SDL. {SDL_GetError()}", SDL_GetError());
}

var window = SDL_CreateWindow("Chip8Emu",
    SDL_WINDOWPOS_UNDEFINED,
    SDL_WINDOWPOS_UNDEFINED,
    w: emulatorSettings.WindowWidth,
    h: emulatorSettings.WindowHeight,
    SDL_WindowFlags.SDL_WINDOW_SHOWN);

if (window == IntPtr.Zero)
{
    logger.LogCritical("There was an issue creating the window. {SDL_GetError()}", SDL_GetError());
}

var renderer = SDL_CreateRenderer(window,
    -1,
    SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
    SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

if (renderer == IntPtr.Zero)
{
    logger.LogCritical("There was an issue creating the renderer. {SDL_GetError()}", SDL_GetError());
}

if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
{
    logger.LogCritical("There was an issue initializing SDL2_Image {SDL_image.IMG_GetError()}",
        SDL_image.IMG_GetError());
}

var emulator = new Emulator(tickRate: emulatorSettings.GpuTickRate, logger);
var keyboardMapper = new KeyboardMapper(emulatorSettings.Keymap);
var file = emulatorSettings.Filename;
var stream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Roms", file));
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

// Main loop for the program
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

// Clean up
SDL_DestroyRenderer(renderer);
SDL_DestroyWindow(window);
SDL_Quit();