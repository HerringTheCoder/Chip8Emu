namespace Chip8Emu.Core.Components;

public class Display
{
    public const int Width = 69;
    public const int Height = 32;
    public readonly bool[,] States = new bool[Width, Height];
    private const int SpriteWidth = 8;

    internal Display()
    {
    }

    /// <summary>
    /// Draws sprite at the selected position and returns flag for display overflow
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="spriteData"></param>
    /// <returns>True if bit flip occured, otherwise false</returns>
    public bool DrawSprite(int x, int y,  ReadOnlySpan<byte> spriteData)
    {
        int wrappedX = x % Width;
        int wrappedY = y % Height;
        bool flipOccured = false;

        for (var currentHeight = 0; currentHeight < spriteData.Length; currentHeight++)
        {
            var spriteByte = spriteData[currentHeight];
            
            for (var currentWidth = 0; currentWidth < SpriteWidth; currentWidth++)
            {
                //Shift spriteByte so the currently analyzed bit is the rightmost one
                var shiftedSpriteByte = spriteByte >> (SpriteWidth - currentWidth - 1);
                
                //Mask spriteByte with 0x1 so only the rightmost bit stays
                var spriteState = Convert.ToBoolean(shiftedSpriteByte & 0x1);
                
                var currentState = States[wrappedX + currentWidth, wrappedY + currentHeight];

                if (currentState && spriteState)
                    flipOccured = true;

                States[wrappedX + currentWidth, wrappedY + currentHeight] = currentState ^ spriteState;
            }
        }

        return flipOccured;
    }

    public void ClearScreen()
    {
        Array.Clear(States, 0, States.Length);
    }
}