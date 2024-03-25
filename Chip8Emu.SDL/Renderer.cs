using System.Diagnostics;
using Chip8Emu.Core.Components;

namespace Chip8Emu.SDL;

public static class Renderer
{
    private const int SizeMultiplier = 1;
    private const int ScreenWidth = 128;
    private const int ScreenHeight = 64;
    private const int WindowWidth = ScreenWidth * SizeMultiplier;
    private const int WindowHeight = ScreenHeight * SizeMultiplier;

    // Constants for colors
    private static readonly SDL2.SDL.SDL_Color ColorOn = new SDL2.SDL.SDL_Color() { r = 255, g = 255, b = 255, a = 255 }; // White
    private static readonly SDL2.SDL.SDL_Color ColorOff = new SDL2.SDL.SDL_Color() { r = 0, g = 0, b = 0, a = 255 };    // Black

    // Function to render the emulator states on the screen
    public static void RenderStates(nint renderer, Display display)
    {
        try
        {
            SDL2.SDL.SDL_SetRenderDrawColor(renderer, ColorOff.r, ColorOff.g, ColorOff.b, 255); // Clear the renderer
            SDL2.SDL.SDL_RenderClear(renderer);
            SDL2.SDL.SDL_RenderSetLogicalSize(renderer, WindowWidth, WindowHeight);

            // Draw the emulator display contents
            for (int y = 0; y < ScreenHeight; y++)
            {
                for (int x = 0; x < ScreenWidth; x++)
                {
                    if (display.States[x, y])
                    {
                        SDL2.SDL.SDL_SetRenderDrawColor(renderer, ColorOn.r, ColorOn.g, ColorOn.b, ColorOn.a); // Set color to white
                        SDL2.SDL.SDL_RenderDrawPoint(renderer, x * SizeMultiplier, y * SizeMultiplier); // Draw a point
                    }
                }
            }

            SDL2.SDL.SDL_RenderPresent(renderer); // Render the frame;
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
        
    }
}