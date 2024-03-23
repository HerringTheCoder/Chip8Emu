using Chip8Emu.Core.Resources;

namespace Chip8Emu.Core.Components;

public class Memory
{
    private readonly byte[] _currentMemory = new byte[0x1000]; //4Kb

    //First 512 bytes are empty for compatibility with older programs
    public readonly (ushort Start, ushort End) UserSpace = new(0x200, 0xFFF);
    private const int FontsStartingAddress = 0x0;

    internal Memory()
    {
        Buffer.BlockCopy(Fonts.Data, 0, _currentMemory, FontsStartingAddress, Fonts.Data.Length);
    }

    public byte[] GetSprite(ushort address, int offset)
    {
        return _currentMemory[new Range(address, address + offset)];
    }

    public void WriteByte(ushort address, byte value)
    {
        ValidateAddress(address);

        _currentMemory[address] = value;
    }

    public byte ReadByte(ushort address)
    {
        ValidateAddress(address);

        return _currentMemory[address];
    }

    public ushort ReadWord(ushort startIndex)
    {
        // Ensure that there are at least two bytes remaining in the array
        if (startIndex + 1 >= _currentMemory.Length)
        {
            throw new ArgumentException("The byte array does not contain enough elements to form a word value.");
        }
        
        ushort result = BitConverter.ToUInt16(new []{_currentMemory[startIndex + 1], _currentMemory[startIndex]});

        return result;
    }

    public void LoadProgram(Stream stream)
    {
        int bytesRead;
        int currentPosition = UserSpace.Start;
        while ((bytesRead = stream.Read(_currentMemory, UserSpace.Start, _currentMemory.Length - currentPosition)) > 0)
        {
            currentPosition += bytesRead;
        }
    }

    public void ResetMemory()
    {
        for (var i = UserSpace.Start; i <= UserSpace.End; i++)
        {
            _currentMemory[i] = 0x0;
        }
    }

    private void ValidateAddress(ushort address)
    {
        if (address <= UserSpace.Start || address > UserSpace.End)
            throw new ArgumentOutOfRangeException(
                nameof(address),
                address,
                $"User memory is located between " +
                $"{UserSpace.Start}/0x{UserSpace.Start:X} and" +
                $" {UserSpace.End}/0x{UserSpace.End:X} addresses.");
    }
}