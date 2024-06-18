namespace rvfun;

public struct BitField
{
    private readonly uint mask;
    private readonly int bitpos;
    private readonly int bitwidth;

    public BitField(int bitpos, int bitwidth)
    {
        this.mask = (1u << bitwidth) - 1;
        this.bitpos = bitpos;
        this.bitwidth = bitwidth;
    }


    public uint ExtractUnsigned(uint bitvector)
    {
        return (bitvector >> bitpos) & mask;
    }

    public int ExtractSigned(uint bitvector)
    {
        int n = (int)((bitvector >> bitpos) & mask);
        n = (n << (32 - bitwidth)) >> (32-bitwidth);
        return n;
    }
}
