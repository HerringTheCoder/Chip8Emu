# Chip8 emulator written in C#
The aim of this project is to study retro emulation and low level hardware architectures.
For the time being it does not aim for ease of use (though it's mostly configurable) or full compatibility.  

Current version includes:
- All 36 CPU instructions emulated along with correct memory, registers and stack behavior
- Multi-threaded timers
- GPU accelerated renderer and upscaling
- Configurable (although a bit cumbersome) keybinds, cpu speed, resolution
  
So far it seems to play most of the games I've tested, except for 'modern' titles that require >4Kb of memory.

Glitch Ghost rom  
<img width="400" alt="image" src="https://github.com/HerringTheCoder/Chip8Emu/assets/44547474/43b9ea94-a513-4bdc-a076-c8985760d4d9">

Cpu test rom  
<img width="400" alt="image" src="https://github.com/HerringTheCoder/Chip8Emu/assets/44547474/031c1968-ece6-49da-bc9b-6501bbeea274">


You can configure some of the parameters by modifying appsettings.json file:
- CyclesPerSecond (Max: 1000) and OperationsPerCycle (Max: Not yet defined, but I've tested it using value of 100000) control your execution speed.
Theoretically the speed of execution is equal to CyclesPerSecond * OperationsPerCycle.  
For best accuracy (however it is defined in Chip8) it is probably best to leave OperationsPerCycle=2.  
- GpuTickRate should be generally set to 60hz/60fps, but you can set it up to 1000, keep in mind it will only increase rendering refresh rate, not the in-game speed.  
- WindowWidth and WindowHeight can be set to whatever resolution you want and the image should be upscaled, 
unless you're trying to render it below the minimum of 64x32 pixels (x1, no scaling).  
- Filename can be either relative or absolute.
- Keymap relies on SDL Keycodes: https://wiki.libsdl.org/SDL2/SDL_Keycode
Keep in mind that chip8 specification has no standardized controls, so you need to configure key binding per game.
 
Default configuration I've used to test glitch ghost game:
```
{
  "EmulatorSettings": {
    "LogLevel": "Information",
    "CyclesPerSecond": 700,
    "OperationsPerCycle": 2,
    "WindowWidth": 1920,
    "WindowHeight": 1080,
    "GpuTickRate": 60,
    "Filename": "Roms/glitchGhost.ch8",
    "Keymap": {
      "SDLK_0": "0x0",
      "SDLK_1": "0x1",
      "SDLK_2": "0x2",
      "SDLK_3": "0x3",
      "SDLK_4": "0x4",
      "SDLK_UP": "0x5",
      "SDLK_SPACE": "0x6",
      "SDLK_LEFT": "0x7",
      "SDLK_DOWN": "0x8",
      "SDLK_RIGHT": "0x9",
      "SDLK_q": "0xA",
      "SDLK_w": "0xB",
      "SDLK_e": "0xC",
      "SDLK_r": "0xD",
      "SDLK_t": "0xE",
      "SDLK_y": "0xF"
    }
  }
}
``` 

Parts that could be improved:
- Rom loading and configuration via GUI
- Sound output, the code is mostly there, but I didn't bother coupling SDL Mixer yet
- Image flickering countermeasures

Links that I found particularly useful:
- https://tobiasvl.github.io/blog/write-a-chip-8-emulator  
  Probably the best no-code Chip8 guide for anyone that prefers to figure out most of the stuff.  
Gives a quick perspective into Chip8 architecture with some useful insight.
- http://devernay.free.fr/hacks/chip8/C8TECH10.HTM  
 A similar, albeit more focused technical reference. Helped me understand some CPU opcodes behavior.
- https://www.wikiwand.com/en/CHIP-8#Opcode_table  
Yet another useful opcode table. Provides some pseudocode instead of pure assembler.
