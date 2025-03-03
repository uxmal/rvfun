using Reko.ImageLoaders.LLVM;

namespace rvfun;

public class Linker
{
    private readonly  Memory memory;
    private readonly Dictionary<string, Symbol> symbols;
    private readonly List<Relocation> relocations;
    private readonly Dictionary<uint, Relocation> pcrel_hi20s;
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
    this.pcrel_hi20s = relocations
        .Where(r => r.Rtype == RelocationType.U_PcRelative_Hi20)
        .ToDictionary(r => r.Address);
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
            case RelocationType.U_PcRelative_Hi20:
                displacement = displacement + 0x800;    // According to Risc-V docs
                uInstr |= AsmEncoder.EncodeUdisplacement(displacement >> 12);
                memory.WriteLeWord32(rel.Address, uInstr);
                break;
            case RelocationType.I_PcRelative_Lo12:
                displacement = DetermineHiDisplacement(rel);
                uInstr |= AsmEncoder.EncodeIdisplacement(displacement);
                memory.WriteLeWord32(rel.Address, uInstr);
                break;
            default:
                throw new NotImplementedException($"Unimplemented relocation type {rel.Rtype}.");
        }
    }

    private int DetermineHiDisplacement(Relocation relLo)
    {
        // The symbol RelLo refers to is the location of the corresponding relHi
        if (!symbols.TryGetValue(relLo.SymbolName, out var symHi))
        {
            logger.ReportError($"Unknown symbol {relLo.SymbolName}.");
            return 0;
        }
        // Now find a pcrel_hi20 relocation at that symbol's address.
        if (!pcrel_hi20s.TryGetValue(symHi.Address, out var relHi))
        {
            logger.ReportError($"No {RelocationType.U_PcRelative_Hi20} relocation found at location {symHi.Address}.");
            return 0;
        }
        if (!symbols.TryGetValue(relHi.SymbolName, out var sym))
        {
            logger.ReportError($"Unknown symbol {relLo.SymbolName}.");
            return 0;
        }
        return (int)sym.Address - (int) relHi.Address;
    }
}
