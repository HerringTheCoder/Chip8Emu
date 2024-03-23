namespace Chip8Emu.Core.Components;

public class Display
{
    public bool[,] GetStates()
    {
        lock (StatesLock)
        {
            return _states;
        }
    } 
    
    private readonly bool[,] _states = new bool[64, 32];
    private readonly object StatesLock = new object();
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
    public bool DrawSprite(int x, int y,  byte[] spriteData)
    {
        int wrappedX = x % 64;
        int wrappedY = y % 32;
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
                
                var currentState = _states[wrappedX + currentWidth, wrappedY + currentHeight];

                if (currentState && spriteState)
                    flipOccured = true;

                _states[wrappedX + currentWidth, wrappedY + currentHeight] = currentState ^ spriteState;
            }
        }

        return flipOccured;
    }

    public void ClearScreen()
    {
        Array.Clear(_states, 0, _states.Length);
    }
}