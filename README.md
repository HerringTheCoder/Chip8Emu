# Chip8 emulator written in C#
The aim of this project is to study retro emulation and low level hardware architectures.
 
Current version supports about 30 CPU instructions, multi-threaded timers and SDL renderer for video.
So far it boots into most of the games I've tested, except for 'modern' titles that require >4Kb of memory.

The current goals:
- Bugfixing
- User input
- Sound output
- Rom loading via desktop application

Links that I found particularly useful:
- https://tobiasvl.github.io/blog/write-a-chip-8-emulator  
  Probably the best no-code Chip8 guide for anyone that prefers to figure out most of the stuff.  
Gives a quick perspective into Chip8 architecture with some useful insight.
- http://devernay.free.fr/hacks/chip8/C8TECH10.HTM  
 A similar, albeit more focused technical reference. Helped me understand some CPU opcodes behavior.
- https://www.wikiwand.com/en/CHIP-8#Opcode_table  
Yet another useful opcode table. Provides some pseudocode instead of pure assembler.
