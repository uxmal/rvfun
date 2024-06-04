namespace rvfun;

public class Memory
{
    private readonly byte[] bytes;

    public Memory(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public byte ReadByte(int address)
    {
        return bytes[address];
    }

    public void WriteByte(int address, byte value)  
    {
        this.bytes[address] = value;
    }


    public int ReadLeWord32(int address)
    {
        var b0 = this.bytes[address];
        var b1 = this.bytes[address + 1];
        var b2 = this.bytes[address + 2];
        var b3 = this.bytes[address + 3];
        return b0 + (b1 * 256) + (b2 * 65536) + (b3 * 256 * 65536);
    }

    public void WriteLeWord32(int address, int value)
    {
        var b3 = (byte) (value / (256 * 65536));
        var b2 = (byte)(value / 65536);
        var b1 = (byte)(value / 256);
        var b0 = (byte) value;
        this.bytes[address] = b0;
        this.bytes[address+ 1] = b1;
        this.bytes[address + 2] = b2;
        this.bytes[address + 3] = b3;
    }

}