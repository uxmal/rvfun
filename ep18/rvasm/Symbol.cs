namespace rvfun.asm;

public class Symbol
{
    public uint Address {get;}
    public string Name {get;}

    public Symbol(string sName, uint address)
    {
        this.Name = sName;
        this.Address = address;
    }
}