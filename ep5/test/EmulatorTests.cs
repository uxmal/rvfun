namespace rvfun.UnitTests;
using rvfun;
using static rvfun.Mnemonics;

#pragma warning disable NUnit2005

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

    [Test]
    public void RiscVEmu_addi_minus2()
    {
        RunTest(m =>
        {
            m.asm(addi, 2, 0, -2);
        });
        Assert.AreEqual(-2, emu.Registers[2]);
    }

    [Test]
    public void RiscVEmu_add()
    {
        RunTest(m =>
        {
            emu.Registers[4] = 4;
            emu.Registers[5] = 5;
            emu.Registers[6] = 6;

            m.asm(add, 4, 5, 6);
        });
        Assert.AreEqual(11, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_mul()
    {
        RunTest(m =>
        {
            emu.Registers[4] = 4;
            emu.Registers[5] = 5;
            emu.Registers[6] = 6;

            m.asm(mul, 4, 5, 6);
        });
        Assert.AreEqual(30, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_slti_true()
    {
        RunTest(m =>
        {
            emu.Registers[4] = 4;
            emu.Registers[5] = 5;

            m.asm(slti, 4, 5, 6);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }

        [Test]
    public void RiscVEmu_slti_false()
    {
        RunTest(m =>
        {
            emu.Registers[4] = 4;
            emu.Registers[5] = 5;

            m.asm(slti, 4, 5, 5);
        });
        Assert.AreEqual(0, emu.Registers[4]);
    }


        /*
m.asm(addi, 1, 0, 10);
m.asm(addi, 2, 0, 1);
m.asm(@goto, 5, 0, 0);

m.asm(mul, 2, 2, 1);    // 3: loop header
m.asm(addi, 1, 1, -1);

m.asm(sgti, 3, 1, 1);       // 5: check
m.asm(bnz, 3, 3, 0);    
    */
}