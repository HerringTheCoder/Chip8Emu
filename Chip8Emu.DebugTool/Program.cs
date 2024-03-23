// See https://aka.ms/new-console-template for more information

using Chip8Emu.Core;

Console.WriteLine("Hello, World!");

var emulator = new Emulator();

var stream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "IBM_Logo.ch8"));
var debugBuffer = new byte[256];
var debugStream = new MemoryStream(debugBuffer, true);
stream.CopyTo(debugStream);
stream.Position = 0;
emulator.LoadProgram(stream);
stream.Dispose();

await emulator.RunProgram(500, CancellationToken.None);
