namespace rvfun.UnitTests;
using rvfun;
using static rvfun.Mnemonics;

#pragma warning disable NUnit2005

[TestFixture]
public class EmulatorTests
{
    private Memory memory;
    private Emulator emu;
    private OsEmulator osemu;
    private int? exitCode;

    [SetUp]
    public void Setup()
    {
        this.memory = new Memory(new byte[1024]);
        this.osemu = new OsEmulator(memory);
        this.emu = new Emulator(this.memory, osemu);
        this.exitCode = null;
    }

    private void RunTest(Action<Assembler> testBuilder)
    {
        var asm = new Assembler(memory);
        testBuilder(asm);
        exitCode = emu.exec();
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
    public void RiscVEmu_addi_x0()
    {
        RunTest(m =>
        {
            m.asm(addi, 0, 0, 42);
        });
        Assert.AreEqual(0, emu.Registers[0]);
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

    [Test]
    public void RiscVEmu_lb()
    {
        RunTest(m =>
        {
            memory.WriteByte(0x8, 0x82);
            emu.Registers[4] = 0x4;
            emu.Registers[5] = 0x4;

            m.asm(lb, 4, 5, 0x4);
        });
        Assert.AreEqual(unchecked((int)0xFFFFFF82), emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_lbu()
    {
        RunTest(m =>
        {
            memory.WriteByte(0x8, 0x82);
            emu.Registers[4] = 0x4;
            emu.Registers[5] = 0x4;

            m.asm(lbu, 4, 5, 0x4);
        });
        Assert.AreEqual((int)0x00000082, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_lh()
    {
        RunTest(m =>
        {
            memory.WriteByte(0x8, 0x65);
            memory.WriteByte(0x9, 0x87);
            emu.Registers[4] = 0x4;
            emu.Registers[5] = 0x4;

            m.asm(lh, 4, 5, 0x4);
        });
        Assert.AreEqual(unchecked((int)0xFFFF8765), emu.Registers[4]);
    }


    [Test]
    public void RiscVEmu_lhu()
    {
        RunTest(m =>
        {
            memory.WriteByte(0x8, 0x65);
            memory.WriteByte(0x9, 0x87);
            emu.Registers[4] = 0x4;
            emu.Registers[5] = 0x4;

            m.asm(lhu, 4, 5, 0x4);
        });
        Assert.AreEqual((int)0x00008765, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_lw()
    {
        RunTest(m =>
        {
            memory.WriteByte(0x8, 0x78);
            memory.WriteByte(0x9, 0x56);
            memory.WriteByte(0xA, 0x34);
            memory.WriteByte(0xB, 0x12);
            emu.Registers[4] = 0x4;
            emu.Registers[5] = 0x4;

            m.asm(lw, 4, 5, 0x4);
        });
        Assert.AreEqual((int)0x12345678, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_sb()
    {
        RunTest(m =>
        {
            emu.Registers[4] = 0x12345678;
            emu.Registers[5] = 0x4;

            m.asm(sb, 4, 5, 0x124);
        });
        Assert.AreEqual(0x78, memory.ReadLeWord32(0x128));
    }

    [Test]
    public void RiscVEmu_sh()
    {
        RunTest(m =>
        {
            emu.Registers[4] = 0x12345678;
            emu.Registers[5] = 0x4;

            m.asm(sh, 4, 5, 0x124);
        });
        Assert.AreEqual(0x5678, memory.ReadLeWord32(0x128));
    }

    [Test]
    public void RiscVEmu_sw()
    {
        RunTest(m =>
        {
            emu.Registers[4] = 0x12345678;
            emu.Registers[5] = 0x4;

            m.asm(sw, 4, 5, 0x124);
        });
        Assert.AreEqual(0x12345678, memory.ReadLeWord32(0x128));
    }

    [Test]
    public void RiscVEmu_jal_x0()
    {
        RunTest(m => 
        {
            m.asm(addi, 4, 0, 1);
            m.asm(jal, 0, 8, 0);
            m.asm(addi, 4, 0, -1);
            m.asm(addi, 0, 0, 0);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }
    
    [Test]
    public void RiscVEmu_bne()
    {
        RunTest(m => 
        {
            m.asm(addi, 4, 0, 1);
            m.asm(bne, 4, 0, 8);
            m.asm(addi, 4, 0, -1);
            m.asm(addi, 0, 0, 0);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_bge()
    {
        RunTest(m => 
        {
            m.asm(addi, 4, 0, 1);
            m.asm(bge, 4, 0, 8);
            m.asm(addi, 4, 0, -1);
            m.asm(addi, 0, 0, 0);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_jalr()
    {
        RunTest(m =>
        {
            emu.Registers[1] = 0x8;
            emu.Registers[10] = 1;

            m.asm(jalr, 0, 1, 0);
            m.asm(addi, 10, 0, 0x42);

            m.asm(addi, 11, 10, 0);
        });
        Assert.AreEqual(1, emu.Registers[11]);
    }

    [Test]
    public void RiscVEmu_ecall()
    {
        RunTest(m =>
        {
            emu.Registers[17] = 0;
            emu.Registers[10] = 1;

            m.asm(ecall, 0, 0, 0);
            m.asm(addi, 10, 0, 0x42);
        });
        Assert.AreEqual(1, emu.Registers[10]);
        Assert.AreEqual(1, exitCode!.Value);
    }

    [Test]
    public void RiscVEmu_lui()
    {
        RunTest(m => 
        {
            m.asm(lui, 1, 0x12345, 0);
            m.asm(addi, 1, 1, 0x678);
        });
        Assert.AreEqual(0x12345678, emu.Registers[1]);
    }

    [Test]
    public void RiscVEmu_auipc()
    {
        RunTest(m => 
        {
            m.asm(auipc, 1, 0x12345, 0);
            m.asm(addi, 1, 1, 0x678);
        });
        Assert.AreEqual(0x12345678, emu.Registers[1]);
    }

}