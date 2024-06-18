
namespace rvfun;

public class Memory
{
    private readonly byte[] bytes;

    public Memory(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public byte ReadByte(uint address)
    {
        return bytes[address];
    }

    public void WriteByte(uint address, byte value)  
    {
        this.bytes[address] = value;
    }


    public uint ReadLeWord32(uint address)
    {
        var b0 = this.bytes[address];
        var b1 = this.bytes[address + 1];
        var b2 = this.bytes[address + 2];
        var b3 = this.bytes[address + 3];
        return b0 + 
            (uint)(b1 * 256) + 
            (uint)(b2 * 65536) +
            (uint)(b3 * 256 * 65536);
    }

    public void WriteLeWord32(uint address, uint value) => WriteLeWord32(address, (int)value);

    public void WriteLeWord32(uint address, int value)
    {
        var b3 = (byte) (value >> 24);
        var b2 = (byte)(value >> 16);
        var b1 = (byte)(value >> 8);
        var b0 = (byte) value;
        this.bytes[address] = b0;
        this.bytes[address+ 1] = b1;
        this.bytes[address + 2] = b2;
        this.bytes[address + 3] = b3;
    }

    public bool IsValidAddress(uint iptr)
    {
        return 0 <= iptr && iptr < this.bytes.Length;
    }
}