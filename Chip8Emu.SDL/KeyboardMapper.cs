namespace Chip8Emu.SDL;
using static SDL2.SDL;

public class KeyboardMapper
{
    private readonly IDictionary<SDL_Keycode, byte> _keymap;
    
    public KeyboardMapper(IDictionary<SDL_Keycode, byte> keymap)
    {
        _keymap = keymap;
    }

    public bool TryGetChip8Key(SDL_Keycode keyValue, out byte result)
    {
        return _keymap.TryGetValue(keyValue, out result);
    }
}