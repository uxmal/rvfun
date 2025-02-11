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

    [Test]
    public void RiscVEmu_DisallowIfNotExecutable()
    {
        // Disallow execution if the memory area is not executable.
        this.memory = new Memory();
        this.memory.Allocate(0, new byte[1024], AccessMode.Read);
        this.osemu = new OsEmulator(memory);
        this.emu = new Emulator(this.memory, osemu);
        this.exitCode = null;

        try
        {
            RunTest(m =>
            {
                m.addi(0, 0, 0);
            });
            Assert.Fail();
        }
        catch (InvalidOperationException)
        {

        }

    }
    private void RunTest(Action<Assembler> testBuilder)
    {
        var asm = new Assembler(memory, new Logger());
        testBuilder(asm);
        exitCode = emu.exec();
    }

    [Test]
    public void RiscVEmu_addi()
    {
        RunTest(m =>
        {
            m.addi(2, 0, 42);
        });
        Assert.AreEqual(42, emu.Registers[2]);
    }

    [Test]
    public void RiscVEmu_addi_x0()
    {
        RunTest(m =>
        {
            m.addi(0, 0, 42);
        });
        Assert.AreEqual(0, emu.Registers[0]);
    }

    [Test]
    public void RiscVEmu_addi_minus2()
    {
        RunTest(m =>
        {
            m.addi(2, 0, -2);
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

            m.add(4, 5, 6);
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

            m.mul(4, 5, 6);
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

            m.slti(4, 5, 6);
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

            m.slti(4, 5, 5);
        });
        Assert.AreEqual(0, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_sltiu_true()
    {
        RunTest(m =>
        {
            emu.Registers[4] = -4;
            emu.Registers[5] = 5;

            m.sltiu(4, 5, -2);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_sltiu_false()
    {
        RunTest(m =>
        {
            emu.Registers[4] = -2;
            emu.Registers[5] = 5;

            m.slti(4, 5, 5);
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

            m.lb(4, 5, 0x4);
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

            m.lbu(4, 5, 0x4);
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

            m.lh(4, 5, 0x4);
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

            m.lhu(4, 5, 0x4);
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

            m.lw(4, 5, 0x4);
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

            m.sb(4, 5, 0x124);
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

            m.sh(4, 5, 0x124);
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

            m.sw(4, 5, 0x124);
        });
        Assert.AreEqual(0x12345678, memory.ReadLeWord32(0x128));
    }

    [Test]
    public void RiscVEmu_jal_x0()
    {
        RunTest(m =>
        {
            m.addi(4, 0, 1);
            m.jal(0, 8);
            m.addi(4, 0, -1);
            m.addi(0, 0, 0);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_bne()
    {
        RunTest(m =>
        {
            m.addi(4, 0, 1);
            m.bne(4, 0, 8);
            m.addi(4, 0, -1);
            m.addi(0, 0, 0);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_bge()
    {
        RunTest(m =>
        {
            m.addi(4, 0, 1);
            m.bge(4, 0, 8);
            m.addi(4, 0, -1);
            m.addi(0, 0, 0);
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

            m.jalr(0, 1, 0);
            m.addi(10, 0, 0x42);

            m.addi(11, 10, 0);
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

            m.ecall(0, 0, 0);
            m.addi(10, 0, 0x42);
        });
        Assert.AreEqual(1, emu.Registers[10]);
        Assert.AreEqual(1, exitCode!.Value);
    }

    [Test]
    public void RiscVEmu_lui()
    {
        RunTest(m =>
        {
            m.lui(1, 0x12345, 0);
            m.addi(1, 1, 0x678);
        });
        Assert.AreEqual(0x12345678, emu.Registers[1]);
    }

    [Test]
    public void RiscVEmu_auipc()
    {
        RunTest(m =>
        {
            m.auipc(1, 0x12345);
            m.addi(1, 1, 0x678);
        });
        Assert.AreEqual(0x12345678, emu.Registers[1]);
    }

    [Test]
    public void RiscVEmu_beq()
    {
        RunTest(m =>
        {
            m.addi(4, 0, 0);
            m.beq(4, 0, 8);
            m.addi(4, 0, -1);
            m.addi(0, 0, 0);
        });
        Assert.AreEqual(0, emu.Registers[4]);
    }

    [Test]
    public void RiscVEmu_blt()
    {
        RunTest(m =>
        {
            m.addi(4, 0, -2);
            m.blt(4, 0, 8);
            m.addi(4, 0, -1);
            m.addi(0, 0, 0);
        });
        Assert.AreEqual(-2, emu.Registers[4]);
    }





    [Test]
    public void RiscVEmu_bltu()
    {
        RunTest(m =>
        {
            m.addi(3, 0, -2);
            m.addi(4, 0, 1);
            m.bltu(4, 3, 8);
            m.addi(4, 0, -1);
            m.addi(0, 0, 0);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }


    [Test]
    public void RiscVEmu_bgeu()
    {
        RunTest(m =>
        {
            m.addi(3, 0, -2);
            m.addi(4, 0, 1);
            m.bgeu(3, 4, 8);
            m.addi(4, 0, -1);
            m.addi(0, 0, 0);
        });
        Assert.AreEqual(1, emu.Registers[4]);
    }



    [Test]
    public void RiscVEmu_xori()
    {
        RunTest(m =>
                {
                    emu.Registers[4] = 0x55555555;
                    m.xori(5, 4, -1);
                });
        Assert.AreEqual(unchecked((int)0xAAAAAAAA), emu.Registers[5]);
    }


    [Test]
    public void RiscVEmu_ori()
    {
        RunTest(m =>
                {
                    emu.Registers[4] = 0x55555555;
                    m.ori(5, 4, -1);
                });
        Assert.AreEqual(unchecked((int)0xFFFFFFFF), emu.Registers[5]);
    }


    [Test]
    public void RiscVEmu_andi()
    {
        RunTest(m =>
                {
                    emu.Registers[4] = 0x55555555;
                    m.andi(5, 4, 0xF);
                });
        Assert.AreEqual(5, emu.Registers[5]);
    }


    [Test]
    public void RiscVEmu_slli()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 2);           // 0000010
                    m.slli(5, 4, 1);
                });
        Assert.AreEqual(4, emu.Registers[5]);
    }


    [Test]
    public void RiscVEmu_srli()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 4);           // 0000100
                    m.srli(5, 4, 1);
                });
        Assert.AreEqual(2, emu.Registers[5]);
    }


    [Test]
    public void RiscVEmu_srai()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, -1);

                    m.srai(5, 4, 4);
                });
        Assert.AreEqual(-1, emu.Registers[5]);
    }



    [Test]
    public void RiscVEmu_sub()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 2);
                    m.addi(5, 0, 3);
                    m.sub(3, 4, 5);
                });
        Assert.AreEqual(-1, emu.Registers[3]);
    }


    [Test]
    public void RiscVEmu_sll()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 2);
                    m.addi(3, 0, 3);
                    m.sll(5, 4, 3);
                });
        Assert.AreEqual(0x10, emu.Registers[5]);
    }


    [TestCase(2, 3, 1)]
    [TestCase(3, 3, 0)]
    public void RiscVEmu_slt(int reg1, int reg2, int result)
    {
        RunTest(m =>
                {
                    m.addi(4, 0, reg1);
                    m.addi(5, 0, reg2);
                    m.slt(6, 4, 5);
                });
        Assert.AreEqual(result, emu.Registers[6]);
    }


    [TestCase(2u, 0xFFFFFFF3u, 1)]
    [TestCase(0xFFFFFFF3u, 2u, 0)]
    public void RiscVEmu_sltu(uint reg1, uint reg2, int result)
    {
        RunTest(m =>
                {
                    m.addi(4, 0, (int)reg1);
                    m.addi(5, 0, (int)reg2);
                    m.sltu(6, 4, 5);
                });
        Assert.AreEqual(result, emu.Registers[6]);
    }


    [Test]
    public void RiscVEmu_xor()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 0xAAA);
                    m.addi(5, 0, 0xFFF);
                    m.xor(6, 4, 5);
                });
        Assert.AreEqual(0x555, emu.Registers[6]);
    }


    [Test]
    public void RiscVEmu_srl()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 6);
                    m.addi(5, 0, 1);
                    m.srl(6, 4, 5);
                });
        Assert.AreEqual(3, emu.Registers[6]);
    }


    [Test]
    public void RiscVEmu_sra()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, -20);
                    m.addi(5, 0, 10);
                    m.sra(6, 4, 5);
                });
        Assert.AreEqual(-1, emu.Registers[6]);
    }


    [Test]
    public void RiscVEmu_or()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 0x2AA);
                    m.addi(5, 0, 0x0F0);
                    m.or(6, 4, 5);
                });
        Assert.AreEqual(0x2FA, emu.Registers[6]);
    }


    [Test]
    public void RiscVEmu_and()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 0xAAA);
                    m.addi(5, 0, 0x0F0);
                    m.and(6, 4, 5);
                });
        Assert.AreEqual(0x0A0, emu.Registers[6]);
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fence_tso()
    {
        RunTest(m =>
           {
               m.asm(fence_tso, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_pause()
    {
        RunTest(m =>
                {

                    m.asm(pause, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }



    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_ebreak()
    {
        RunTest(m =>
                {

                    m.ebreak(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }



    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_lwu()
    {
        RunTest(m =>
                {

                    m.lwu(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_ld()
    {
        RunTest(m =>
                {

                    m.ld(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sd()
    {
        RunTest(m =>
                {

                    m.sd(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_addiw()
    {
        RunTest(m =>
                {

                    m.addiw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_slliw()
    {
        RunTest(m =>
                {

                    m.slliw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_srliw()
    {
        RunTest(m =>
                {

                    m.srliw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sraiw()
    {
        RunTest(m =>
                {

                    m.sraiw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_addw()
    {
        RunTest(m =>
                {

                    m.addw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_subw()
    {
        RunTest(m =>
                {

                    m.subw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sllw()
    {
        RunTest(m =>
                {

                    m.sllw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_srlw()
    {
        RunTest(m =>
                {

                    m.srlw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sraw()
    {
        RunTest(m =>
                {

                    m.sraw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }





    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fence_i()
    {
        RunTest(m =>
           {
               m.asm(fence_i, -1, -1, -1);
           });
    }



    // rv32/rv64 zicsr standard/extension
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_csrrw()
    {
        RunTest(m =>
                {

                    m.csrrw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_csrrs()
    {
        RunTest(m =>
                {

                    m.csrrs(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_csrrc()
    {
        RunTest(m =>
                {

                    m.csrrc(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_csrrwi()
    {
        RunTest(m =>
                {

                    m.csrrwi(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_csrrsi()
    {
        RunTest(m =>
                {

                    m.csrrsi(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_csrrci()
    {
        RunTest(m =>
                {

                    m.csrrci(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }




    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_mulh()
    {
        RunTest(m =>
                {

                    m.mulh(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_mulhsu()
    {
        RunTest(m =>
                {

                    m.mulhsu(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_mulhu()
    {
        RunTest(m =>
                {

                    m.mulhu(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_div()
    {
        RunTest(m =>
                {

                    m.div(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_divu()
    {
        RunTest(m =>
                {

                    m.divu(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_rem()
    {
        RunTest(m =>
                {

                    m.rem(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_remu()
    {
        RunTest(m =>
                {

                    m.remu(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }



    // rv64m standard extension (in addition to/rv32m)
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_mulw()
    {
        RunTest(m =>
                {

                    m.mulw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_divw()
    {
        RunTest(m =>
                {

                    m.divw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_divuw()
    {
        RunTest(m =>
                {

                    m.divuw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_remw()
    {
        RunTest(m =>
                {

                    m.remw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_remuw()
    {
        RunTest(m =>
                {

                    m.remuw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }



    //rv32a standard/extension
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_lr_w()
    {
        RunTest(m =>
           {
               m.asm(lr_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sc_w()
    {
        RunTest(m =>
           {
               m.asm(sc_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amosw_p_w()
    {
        RunTest(m =>
           {
               m.asm(amoswap_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoadd_w()
    {
        RunTest(m =>
           {
               m.asm(amoadd_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoxor_w()
    {
        RunTest(m =>
           {
               m.asm(amoxor_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoand_w()
    {
        RunTest(m =>
           {
               m.asm(amoand_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoor_w()
    {
        RunTest(m =>
           {
               m.asm(amoor_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomin_w()
    {
        RunTest(m =>
           {
               m.asm(amomin_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomax_w()
    {
        RunTest(m =>
           {
               m.asm(amomax_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amominu_w()
    {
        RunTest(m =>
           {
               m.asm(amominu_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomaxu_w()
    {
        RunTest(m =>
           {
               m.asm(amomaxu_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_lr_d()
    {
        RunTest(m =>
           {
               m.asm(lr_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sc_d()
    {
        RunTest(m =>
           {
               m.asm(sc_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoswap_d()
    {
        RunTest(m =>
           {
               m.asm(amoswap_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoadd_d()
    {
        RunTest(m =>
           {
               m.asm(amoadd_d, -1, -1, -1);
           });
    }

    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoand_d()
    {
        RunTest(m =>
           {
               m.asm(amoand_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoxor_d()
    {
        RunTest(m =>
           {
               m.asm(amoxor_d, -1, -1, -1);
           });
    }





    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoor_d()
    {
        RunTest(m =>
           {
               m.asm(amoor_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomin_d()
    {
        RunTest(m =>
           {
               m.asm(amomin_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomax_d()
    {
        RunTest(m =>
           {
               m.asm(amomax_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amominu_d()
    {
        RunTest(m =>
           {
               m.asm(amominu_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomaxu_d()
    {
        RunTest(m =>
           {
               m.asm(amomaxu_d, -1, -1, -1);
           });
    }



    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flw()
    {
        RunTest(m =>
                {

                    m.asm(flw, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsw()
    {
        RunTest(m =>
                {

                    m.asm(fsw, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_s()
    {
        RunTest(m =>
           {
               m.asm(fmadd_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_s()
    {
        RunTest(m =>
           {
               m.asm(fmsub_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_s()
    {
        RunTest(m =>
           {
               m.asm(fnmsub_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_s()
    {
        RunTest(m =>
           {
               m.asm(fnmadd_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_s()
    {
        RunTest(m =>
           {
               m.asm(fadd_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_s()
    {
        RunTest(m =>
           {
               m.asm(fsub_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_s()
    {
        RunTest(m =>
           {
               m.asm(fmul_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_s()
    {
        RunTest(m =>
           {
               m.asm(fdiv_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_s()
    {
        RunTest(m =>
           {
               m.asm(fsqrt_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_s()
    {
        RunTest(m =>
           {
               m.asm(fsgnj_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_s()
    {
        RunTest(m =>
           {
               m.asm(fsgnjn_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_s()
    {
        RunTest(m =>
           {
               m.asm(fsgnjx_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_s()
    {
        RunTest(m =>
           {
               m.asm(fmin_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_s()
    {
        RunTest(m =>
           {
               m.asm(fmax_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_s()
    {
        RunTest(m =>
           {
               m.asm(fcvt_w_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_s()
    {
        RunTest(m =>
           {
               m.asm(fcvt_wu_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_x_w()
    {
        RunTest(m =>
           {
               m.asm(fmv_x_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_s()
    {
        RunTest(m =>
           {
               m.asm(feq_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_s()
    {
        RunTest(m =>
           {
               m.asm(flt_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_s()
    {
        RunTest(m =>
           {
               m.asm(fle_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_s()
    {
        RunTest(m =>
           {
               m.asm(fclass_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_w()
    {
        RunTest(m =>
           {
               m.asm(fcvt_s_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_wu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_s_wu, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_w_x()
    {
        RunTest(m =>
           {
               m.asm(fmv_w_x, -1, -1, -1);
           });
    }


    // rv64f standard extension (in addition to/rv32f)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_s()
    {
        RunTest(m =>
           {
               m.asm(fcvt_l_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_s()
    {
        RunTest(m =>
           {
               m.asm(fcvt_lu_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_l()
    {
        RunTest(m =>
           {
               m.asm(fcvt_s_l, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_lu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_s_lu, -1, -1, -1);
           });
    }






    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fld()
    {
        RunTest(m =>
                {

                    m.asm(fld, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsd()
    {
        RunTest(m =>
                {

                    m.asm(fsd, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_d()
    {
        RunTest(m =>
           {
               m.asm(fmadd_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_d()
    {
        RunTest(m =>
           {
               m.asm(fmsub_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_d()
    {
        RunTest(m =>
           {
               m.asm(fnmsub_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_d()
    {
        RunTest(m =>
           {
               m.asm(fnmadd_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_d()
    {
        RunTest(m =>
           {
               m.asm(fadd_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_d()
    {
        RunTest(m =>
           {
               m.asm(fsub_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_d()
    {
        RunTest(m =>
           {
               m.asm(fmul_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_d()
    {
        RunTest(m =>
           {
               m.asm(fdiv_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_d()
    {
        RunTest(m =>
           {
               m.asm(fsqrt_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_d()
    {
        RunTest(m =>
           {
               m.asm(fsgnj_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_d()
    {
        RunTest(m =>
           {
               m.asm(fsgnjn_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_d()
    {
        RunTest(m =>
           {
               m.asm(fsgnjx_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_d()
    {
        RunTest(m =>
           {
               m.asm(fmin_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_d()
    {
        RunTest(m =>
           {
               m.asm(fmax_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_d()
    {
        RunTest(m =>
           {
               m.asm(fcvt_s_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_s()
    {
        RunTest(m =>
           {
               m.asm(fcvt_d_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_d()
    {
        RunTest(m =>
           {
               m.asm(feq_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_d()
    {
        RunTest(m =>
           {
               m.asm(flt_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_d()
    {
        RunTest(m =>
           {
               m.asm(fle_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_d()
    {
        RunTest(m =>
           {
               m.asm(fclass_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_d()
    {
        RunTest(m =>
           {
               m.asm(fcvt_w_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_d()
    {
        RunTest(m =>
           {
               m.asm(fcvt_wu_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_w()
    {
        RunTest(m =>
           {
               m.asm(fcvt_d_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_wu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_d_wu, -1, -1, -1);
           });
    }



    //rv64d standard extension (in addition to/rv32d)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_d()
    {
        RunTest(m =>
           {
               m.asm(fcvt_l_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_d()
    {
        RunTest(m =>
           {
               m.asm(fcvt_lu_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_x_d()
    {
        RunTest(m =>
           {
               m.asm(fmv_x_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_l()
    {
        RunTest(m =>
           {
               m.asm(fcvt_d_l, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_lu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_d_lu, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_d_x()
    {
        RunTest(m =>
           {
               m.asm(fmv_d_x, -1, -1, -1);
           });
    }




    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flq()
    {
        RunTest(m =>
                {

                    m.asm(flq, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsq()
    {
        RunTest(m =>
                {

                    m.asm(fsq, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_q()
    {
        RunTest(m =>
           {
               m.asm(fmadd_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_q()
    {
        RunTest(m =>
           {
               m.asm(fmsub_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_q()
    {
        RunTest(m =>
           {
               m.asm(fnmsub_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_q()
    {
        RunTest(m =>
           {
               m.asm(fnmadd_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_q()
    {
        RunTest(m =>
           {
               m.asm(fadd_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_q()
    {
        RunTest(m =>
           {
               m.asm(fsub_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_q()
    {
        RunTest(m =>
           {
               m.asm(fmul_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_q()
    {
        RunTest(m =>
           {
               m.asm(fdiv_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_q()
    {
        RunTest(m =>
           {
               m.asm(fsqrt_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_q()
    {
        RunTest(m =>
           {
               m.asm(fsgnj_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_q()
    {
        RunTest(m =>
           {
               m.asm(fsgnjn_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_q()
    {
        RunTest(m =>
           {
               m.asm(fsgnjx_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_q()
    {
        RunTest(m =>
           {
               m.asm(fmin_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_q()
    {
        RunTest(m =>
           {
               m.asm(fmax_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_q()
    {
        RunTest(m =>
           {
               m.asm(fcvt_s_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_s()
    {
        RunTest(m =>
           {
               m.asm(fcvt_q_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_q()
    {
        RunTest(m =>
           {
               m.asm(fcvt_d_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_d()
    {
        RunTest(m =>
           {
               m.asm(fcvt_q_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_q()
    {
        RunTest(m =>
           {
               m.asm(feq_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_q()
    {
        RunTest(m =>
           {
               m.asm(flt_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_q()
    {
        RunTest(m =>
           {
               m.asm(fle_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_q()
    {
        RunTest(m =>
           {
               m.asm(fclass_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_q()
    {
        RunTest(m =>
           {
               m.asm(fcvt_w_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_q()
    {
        RunTest(m =>
           {
               m.asm(fcvt_wu_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_w()
    {
        RunTest(m =>
           {
               m.asm(fcvt_q_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_wu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_q_wu, -1, -1, -1);
           });
    }



    //rv64q standard extension (in addition to/rv32q)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_q()
    {
        RunTest(m =>
           {
               m.asm(fcvt_l_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_q()
    {
        RunTest(m =>
           {
               m.asm(fcvt_lu_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_l()
    {
        RunTest(m =>
           {
               m.asm(fcvt_q_l, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_lu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_q_lu, -1, -1, -1);
           });
    }





    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flh()
    {
        RunTest(m =>
                {

                    m.asm(flh, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsh()
    {
        RunTest(m =>
                {

                    m.asm(fsh, -1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_h()
    {
        RunTest(m =>
           {
               m.asm(fmadd_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_h()
    {
        RunTest(m =>
           {
               m.asm(fmsub_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_h()
    {
        RunTest(m =>
           {
               m.asm(fnmsub_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_h()
    {
        RunTest(m =>
           {
               m.asm(fnmadd_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_h()
    {
        RunTest(m =>
           {
               m.asm(fadd_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_h()
    {
        RunTest(m =>
           {
               m.asm(fsub_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_h()
    {
        RunTest(m =>
           {
               m.asm(fmul_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_h()
    {
        RunTest(m =>
           {
               m.asm(fdiv_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_h()
    {
        RunTest(m =>
           {
               m.asm(fsqrt_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_h()
    {
        RunTest(m =>
           {
               m.asm(fsgnj_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_h()
    {
        RunTest(m =>
           {
               m.asm(fsgnjn_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_h()
    {
        RunTest(m =>
           {
               m.asm(fsgnjx_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_h()
    {
        RunTest(m =>
           {
               m.asm(fmin_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_h()
    {
        RunTest(m =>
           {
               m.asm(fmax_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_h()
    {
        RunTest(m =>
           {
               m.asm(fcvt_s_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_s()
    {
        RunTest(m =>
           {
               m.asm(fcvt_h_s, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_h()
    {
        RunTest(m =>
           {
               m.asm(fcvt_d_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_d()
    {
        RunTest(m =>
           {
               m.asm(fcvt_h_d, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_h()
    {
        RunTest(m =>
           {
               m.asm(fcvt_q_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_q()
    {
        RunTest(m =>
           {
               m.asm(fcvt_h_q, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_h()
    {
        RunTest(m =>
           {
               m.asm(feq_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_h()
    {
        RunTest(m =>
           {
               m.asm(flt_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_h()
    {
        RunTest(m =>
           {
               m.asm(fle_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_h()
    {
        RunTest(m =>
           {
               m.asm(fclass_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_h()
    {
        RunTest(m =>
           {
               m.asm(fcvt_w_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_h()
    {
        RunTest(m =>
           {
               m.asm(fcvt_wu_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_x_h()
    {
        RunTest(m =>
           {
               m.asm(fmv_x_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_w()
    {
        RunTest(m =>
           {
               m.asm(fcvt_h_w, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_wu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_h_wu, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_h_x()
    {
        RunTest(m =>
           {
               m.asm(fmv_h_x, -1, -1, -1);
           });
    }



    //rv64zfh standard extension (in addition to/rv32zfh)
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_h()
    {
        RunTest(m =>
           {
               m.asm(fcvt_l_h, -1, -1, -1);
           });
    }



    //rv64zfh standard extension (in addition to/rv32zfh)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_h()
    {
        RunTest(m =>
           {
               m.asm(fcvt_lu_h, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_l()
    {
        RunTest(m =>
           {
               m.asm(fcvt_h_l, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_lu()
    {
        RunTest(m =>
           {
               m.asm(fcvt_h_lu, -1, -1, -1);
           });
    }



    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_wrs_nto()
    {
        RunTest(m =>
           {
               m.asm(wrs_nto, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_wrs_sto()
    {
        RunTest(m =>
           {
               m.asm(wrs_sto, -1, -1, -1);
           });
    }
}