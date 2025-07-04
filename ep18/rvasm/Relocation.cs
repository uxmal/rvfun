namespace rvfun.asm;

public class Relocation
{
    public Relocation(uint address, RelocationType rtype, string symbolName)
    {
        Offset = address;
        Rtype = rtype;
        SymbolName = symbolName;
    }

    public uint Offset { get; }
    public RelocationType Rtype { get; }
    public string SymbolName { get; }
}

public enum RelocationType{

    None,

    J_PcRelative,
    B_PcRelative,
    W32_Absolute,
    U_PcRelative_Hi20,
    I_PcRelative_Lo12
}