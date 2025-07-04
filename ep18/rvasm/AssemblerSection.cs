
namespace rvfun.asm;

public class AssemblerSection
{
    private readonly MemoryStream bytes;

    public AssemblerSection()
    {
        this.bytes = new();
    }

    public uint Position => (uint) bytes.Position;
     
    public byte[] GetAssembledBytes()
    {
        return bytes.ToArray();
    }

    public void WriteBytes(uint offset, byte[] bytes)
    {
        this.bytes.Position = offset;
        this.bytes.Write(bytes, 0, bytes.Length);
    }

    public void WriteLeWord32(uint offset, uint value)
    {
        this.bytes.Position = offset;
        var b3 = (byte)(value >> 24);
        var b2 = (byte)(value >> 16);
        var b1 = (byte)(value >> 8);
        var b0 = (byte)value;
        this.bytes.WriteByte(b0);
        this.bytes.WriteByte(b1);
        this.bytes.WriteByte(b2);
        this.bytes.WriteByte(b3);
    }

    public void WriteLeWord32(uint offset, int n) => WriteLeWord32(offset, (uint)n);
}