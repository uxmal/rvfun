namespace rvfun.UnitTests;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

[TestFixture]
public class LinkerTests
{
    [SetUp]
    public void Setup()
    {
    }

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

}