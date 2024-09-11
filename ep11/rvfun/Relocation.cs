namespace rvfun;

public class Relocation
{
    public Relocation(uint address, RelocationType rtype, string symbolName)
    {
        Address = address;
        Rtype = rtype;
        SymbolName = symbolName;
    }

    public uint Address { get; }
    public RelocationType Rtype { get; }
    public string SymbolName { get; }
}

public enum RelocationType{

    None,

    J_PcRelative,
    B_PcRelative
}