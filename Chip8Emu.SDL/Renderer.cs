using System.Diagnostics;
using Chip8Emu.Core;
using Chip8Emu.Core.Components;
using Chip8Emu.SDL.Configuration;
using Microsoft.Extensions.Logging;
using SDL2;
using static SDL2.SDL;

namespace Chip8Emu.SDL;

public static class Renderer
{
    private const int ScreenWidth = 64;
    private const int ScreenHeight = 32;

    // Constants for colors
    private static readonly SDL_Color ColorOn = new() { r = 255, g = 255, b = 255, a = 255 }; // White
    private static readonly SDL_Color ColorOff = new() { r = 0, g = 0, b = 0, a = 255 };    // Black

    // Function to render the emulator states on the screen
    public static void RenderStates(nint renderer, Display display, nint window)
    {
        try
        {
            SDL_GetWindowSize(window, out int windowWidth, out int windowHeight);

            // Calculate scaling factors for width and height
            float scaleX = (float)windowWidth / ScreenWidth;
            float scaleY = (float)windowHeight / ScreenHeight;

            // Determine the smaller scaling factor to maintain aspect ratio
            float scale = Math.Min(scaleX, scaleY);

            // Calculate scaled window size
            int scaledWidth = (int)(ScreenWidth * scale);
            int scaledHeight = (int)(ScreenHeight * scale);

            SDL_SetRenderDrawColor(renderer, ColorOff.r, ColorOff.g, ColorOff.b, 255); // Clear the renderer
            SDL_RenderClear(renderer);
            SDL_RenderSetLogicalSize(renderer, scaledWidth, scaledHeight);

            // Draw the emulator display contents
            for (int y = 0; y < ScreenHeight; y++)
            {
                for (int x = 0; x < ScreenWidth; x++)
                {
                    if (display.States[x, y])
                    {
                        SDL_SetRenderDrawColor(renderer, ColorOn.r, ColorOn.g, ColorOn.b, ColorOn.a); // Set color to white
                        SDL_Rect rect = new SDL_Rect
                        {
                            x = (int)(x * scale),
                            y = (int)(y * scale),
                            w = (int)scale,
                            h = (int)scale
                        };
                        SDL_RenderFillRect(renderer, ref rect); // Draw a rectangle
                    }
                }
            }

            SDL_RenderPresent(renderer); // Render the frame;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }

    public static (nint renderer, nint window) InitializeRendererAndWindow(ILogger<Emulator> logger, EmulatorSettings emulatorSettings)
    {
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

        return (renderer, window);
    }

    public static void Destroy(IntPtr renderer, IntPtr window)
    {
        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
    }
}