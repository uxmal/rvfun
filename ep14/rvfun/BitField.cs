namespace rvfun;

#pragma warning disable IDE0290

public readonly struct BitField
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


    public readonly uint ExtractUnsigned(uint bitvector)
    {
        return (bitvector >> bitpos) & mask;
    }

    public readonly int ExtractSigned(uint bitvector)
    {
        int n = (int)((bitvector >> bitpos) & mask);
        n = (n << (32 - bitwidth)) >> (32-bitwidth);
        return n;
    }

    public static int ExtractSigned(uint bitvector, params BitField[] fields)
    {
        uint result = 0;
        int cbits = 0;
        for (int i = 0; i < fields.Length; ++i)
        {
            var f = fields[i];
            result = result << f.bitwidth;
            result |= f.ExtractUnsigned(bitvector);
            cbits += f.bitwidth;
        }
        uint m = 1u << (cbits - 1);
        result = (result ^ m) - m;      // Nifty hack that sign extends at bit position cbits.
        return (int)result;
    }
}
