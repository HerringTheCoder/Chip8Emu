using Chip8Emu.Core;
using Chip8Emu.SDL;
using SDL2;

if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
{
    Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
}

var window = SDL.SDL_CreateWindow("Chip8Emu", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 640*2, 480*2, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

if (window == IntPtr.Zero)
{
    Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
}

var renderer = SDL.SDL_CreateRenderer(window, 
                                        -1, 
                                        SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | 
                                        SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

if (renderer == IntPtr.Zero)
{
    Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
}

if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
{
    Console.WriteLine($"There was an issue initilizing SDL2_Image {SDL_image.IMG_GetError()}");
}

var emulator = new Emulator(tickRate: 60);

var file = "glitchGhost.ch8";
// var file = "test_opcode.ch8";

var stream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Roms", file));
emulator.LoadProgram(stream);
var cts = new CancellationTokenSource();
bool running = true;
emulator.DisplayUpdated += EmulatorOnDisplayUpdated;
emulator.ExceptionOccured += (sender, eventArgs) =>
{
    cts.Cancel();
    running = false;
};

void EmulatorOnDisplayUpdated(object? sender, EventArgs e)
{
    // Console.Clear();
    // Console.WriteLine("\x1b[3J");
    // for (int y = 0; y < 32; y++)
    // {
    //     for (int x = 0; x < 64; x++)
    //     {
    //         Console.Write(emulator.Display.States[x, y] ? "#" : " ");
    //     }
    //
    //     Console.WriteLine();
    // }
    
    Renderer.RenderStates(renderer, emulator.Display);
}

Task.Run(() => emulator.RunAsync(cyclesPerSecond:1000, operationsPerCycle:1, cts.Token));

// Main loop for the program
while (running && !cts.IsCancellationRequested)
{
    while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_QUIT:
                cts.Cancel();
                running = false;
                break;
            case SDL.SDL_EventType.SDL_KEYDOWN:
                emulator.PressedKeyValue = (byte?)(SDL.SDL_GetKeyName(e.key.keysym.sym)[0] - 48);
                break;
            case SDL.SDL_EventType.SDL_KEYUP:
                emulator.PressedKeyValue = null;
                break;
        }
    }
}

// Clean up the resources that were created.
SDL.SDL_DestroyRenderer(renderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();