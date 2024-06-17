namespace rvfun.UnitTests;
using rvfun;
using static rvfun.Mnemonics;

[TestFixture]
public class EmulatorTests
{
    private Memory memory;
    private Emulator emu;

    [SetUp]
    public void Setup()
    {
        this.memory = new Memory(new byte[1024]);
        this.emu = new Emulator(this.memory);
    }

    private void RunTest(Action<Assembler> testBuilder)
    {
        var asm = new Assembler(memory);
        testBuilder(asm);
        emu.exec();
    }

    [Test]
    public void RiscVEmu_addi()
    {
        RunTest(m =>
        {
            m.asm(addi, 2, 0, 42);
        });
        Assert.AreEqual(42, emu.Registers[2]);
    }
}