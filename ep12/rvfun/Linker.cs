namespace rvfun;

public class Linker
{
    private readonly  Memory memory;
    private readonly Dictionary<string, Symbol> symbols;
    private readonly List<Relocation> relocations;
    private readonly Logger logger;

    public Linker(
        Memory memory,
         Dictionary<string, Symbol> symbols,
          List<Relocation> relocations,
        Logger logger)
    {
    this.memory = memory;
    this.symbols = symbols;
    this.relocations = relocations;
    this.logger = logger;
    }

    public void Relocate()
    {
        foreach (var rel in this.relocations)
        {
            Relocate(rel);
        }
    }

    private void Relocate(Relocation rel)
    {
        if (!symbols.TryGetValue(rel.SymbolName, out var symbol))
        {
            logger.ReportError($"Unknown symbol {rel.SymbolName}");
            return;
        }
        var uInstr = memory.ReadLeWord32(rel.Address);
        int displacement = (int) symbol.Address - (int) rel.Address;
        switch (rel.Rtype)
        {
            case RelocationType.J_PcRelative:
                uInstr |= AsmEncoder.EncodeJdisplacement(displacement);
                memory.WriteLeWord32(rel.Address, uInstr);
                break;
            case RelocationType.B_PcRelative:
                uInstr |= AsmEncoder.EncodeBdisplacement(displacement);
                memory.WriteLeWord32(rel.Address, uInstr);
                break;
            case RelocationType.W32_Absolute:
                memory.WriteLeWord32(rel.Address, symbol.Address);
                break;
            default:
                throw new NotImplementedException($"Unimplemented relocation type {rel.Rtype}.");
        }
    }
}