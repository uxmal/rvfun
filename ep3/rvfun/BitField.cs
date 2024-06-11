namespace rvfun;

public struct BitField
{
    private readonly int mask;
    private readonly int bitpos;

    public BitField(int bitpos, int bitwidth)
    {
        this.mask = (1 << bitwidth) - 1;
        this.bitpos = bitpos;
    }

    public int Extract(int bitvector)
    {
        return (bitvector >> bitpos) & mask;
    }
}
