namespace rvfun.UnitTests;

using Reko.Arch.RiscV;

using NUnit.Framework;
using Reko.Core.Memory;
using Reko.Core;
using System.Diagnostics;

[TestFixture]
public class LinkerTests
{
    private Memory CreateMemory()
    {
        var bytes = new byte[1024];
        return new Memory(bytes);
    }

    private TestResult RunTest(
        Action<Assembler> noSymbolClient,
        Action<Assembler> symbolClient)
        {
            var logger = new Logger();
            var mem = CreateMemory();
            var asm = new Assembler(mem, logger);
            noSymbolClient(asm);

            var logger2 = new Logger();
            var mem2 = CreateMemory();
            var asm2 = new Assembler(mem2, logger2);
            symbolClient(asm2);

            var linker = new Linker(mem2, asm2.Symbols, asm2.Relocations, new Logger());
            linker.Relocate();
            return new TestResult(mem, asm, mem2, asm2);
        }


    private record TestResult(
        Memory noSymbolMemory,
        Assembler  noSymbolAssembler,
        Memory symbolMemory,
        Assembler symbolAssembler);

    [Test]
    public void RiscAsm_reloc_relocate()
    {
        var result = RunTest(
        m =>
        {
            m.li(4, 4);
            m.jal(0, -4);
        },
        m =>
        {
            m.label("mylabel");
            m.li(4, 4);
            m.jal(0, "mylabel");
        });
        var uInstrExpected = result.noSymbolMemory.ReadLeWord32(4);
        var uInstrActual = result.symbolMemory.ReadLeWord32(4);

        Assert.That(uInstrActual, Is.EqualTo(uInstrExpected));
    }

    [Test]
    public void RiscAsm_reloc_relocate_br()
    {
        var result = RunTest(m =>
        {
            m.li(4, 4);
            m.blt(3, 11, -4);
        },
        m =>
        {
            m.label("mylabel");
            m.li(4, 4);
            m.blt(3, 11, "mylabel");
        });


        var uInstrExpected = result.noSymbolMemory.ReadLeWord32(4);
        var uInstrActual = result.symbolMemory.ReadLeWord32(4);

        Assert.That(uInstrActual, Is.EqualTo(uInstrExpected));
    }

    [Test]
    public void RiscAsm_reloc_pcrel_positive_displacement()
    {
        var result = RunTest(m =>
        {
            m.auipc(4, 0x20);
            m.addi(4, 4, 4);
        },
        m =>
        {
            m.AddSymbol("sym", 0x20004);
            m.auipc(4, m.pcrel_hi("sym"));
            m.addi(4, 4, m.pcrel_lo(-4));
        });

        Dasm(result.symbolMemory, 0, 16);

        var uInstrExpected = result.noSymbolMemory.ReadLeWord32(4);
        var uInstrActual = result.symbolMemory.ReadLeWord32(4);

        Assert.That(uInstrActual.ToString("X8"), Is.EqualTo(uInstrExpected.ToString("X8")));
    }

    private void Dasm(Memory symbolMemory, int uAddr, int length)
    {
        var bytes = symbolMemory.GetSpan(uAddr, length, AccessMode.Read).ToArray();
        var mem = new ByteMemoryArea(Address.Ptr32((uint)uAddr), bytes);
        var arch = new RiscVArchitecture(null!, "riscv", new());
        var rdr = mem.CreateLeReader(0, length);
        var dasm = arch.CreateDisassembler(rdr);
        foreach (var instr in dasm)
        {
            uint uInstr =  rdr.PeekLeUInt32(-4);
            Console.WriteLine("{0}: {1:X8} {2}", instr.Address, uInstr, instr.ToString());
            Debug.Print("{0}: {1:X8} {2}", instr.Address, uInstr, instr.ToString());
        }
    }
}
