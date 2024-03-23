namespace Chip8Emu.SDL;
using SDL2;

public static class Renderer
{
    private const int ScreenWidth = 64;
    private const int ScreenHeight = 32;

    // Constants for colors
    private static readonly SDL.SDL_Color ColorOn = new SDL.SDL_Color() { r = 255, g = 255, b = 255, a = 255 }; // White
    private static readonly SDL.SDL_Color ColorOff = new SDL.SDL_Color() { r = 0, g = 0, b = 0, a = 255 };    // Black

    // Function to render the emulator states on the screen
    public static void RenderStates(nint renderer, bool[,] states)
    {
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255); // Clear the renderer
        SDL.SDL_RenderClear(renderer);

        // Draw the emulator display contents
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                if (states[x, y])
                {
                    SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255); // Set color to white
                    SDL.SDL_RenderDrawPoint(renderer, x * 10, y * 10); // Draw a point
                }
            }
        }

        SDL.SDL_RenderPresent(renderer); // Render the frame
    }
}