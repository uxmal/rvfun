namespace rvfun.UnitTests;

using rvfun;
using rvfun.asm;
using rvfun.lib;

#pragma warning disable NUnit2005

[TestFixture]
public class EmulatorTests
{
    private int? exitCode;
    private int[] registers = default!;
    private Memory memory = default!;

    [SetUp]
    public void Setup()
    {
        this.exitCode = null;
        this.registers = new int[32];
    }

    [Test]
    public void RiscVEmu_DisallowIfNotExecutable()
    {
        // Disallow execution if the memory area is not executable.
        this.exitCode = null;

        try
        {
            RunTest(m =>
            {
                m.addi(0, 0, 0);
            }, AccessMode.Read);
            Assert.Fail("Expected an emulator exception b/c the memory is not marked executable.");
        }
        catch (InvalidOperationException)
        {
        }

    }
    private void RunTest(Action<Assembler> testBuilder, AccessMode accessMode = AccessMode.RWX)
    {
        var asm = new Assembler(new Logger());
        testBuilder(asm);
        this.memory = new Memory();
        var asmBytes = asm.Section.GetAssembledBytes();
        var bytes = new byte[2048];
        Array.Copy(asmBytes, bytes, asmBytes.Length);

        memory.Allocate(0, bytes, accessMode);
        var osemu = new OsEmulator(this.memory);
        var emu = new Emulator(this.memory, osemu, this.registers, 0);

        exitCode = emu.exec();
    }

    [Test]
    public void RiscVEmu_addi()
    {
        RunTest(m =>
        {
            m.addi(2, 0, 42);
        });
        Assert.AreEqual(42, this.registers[2]);
    }

    [Test]
    public void RiscVEmu_addi_x0()
    {
        RunTest(m =>
        {
            m.addi(0, 0, 42);
        });
        Assert.AreEqual(0, this.registers[0]);
    }

    [Test]
    public void RiscVEmu_addi_minus2()
    {
        RunTest(m =>
        {
            m.addi(2, 0, -2);
        });
        Assert.AreEqual(-2, this.registers[2]);
    }

    [Test]
    public void RiscVEmu_add()
    {
        RunTest(m =>
        {
            this.registers[4] = 4;
            this.registers[5] = 5;
            this.registers[6] = 6;

            m.add(4, 5, 6);
        });
        Assert.AreEqual(11, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_mul()
    {
        RunTest(m =>
        {
            this.registers[4] = 4;
            this.registers[5] = 5;
            this.registers[6] = 6;

            m.mul(4, 5, 6);
        });
        Assert.AreEqual(30, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_slti_true()
    {
        RunTest(m =>
        {
            this.registers[4] = 4;
            this.registers[5] = 5;

            m.slti(4, 5, 6);
        });
        Assert.AreEqual(1, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_slti_false()
    {
        RunTest(m =>
        {
            this.registers[4] = 4;
            this.registers[5] = 5;

            m.slti(4, 5, 5);
        });
        Assert.AreEqual(0, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_sltiu_true()
    {
        RunTest(m =>
        {
            this.registers[4] = -4;
            this.registers[5] = 5;

            m.sltiu(4, 5, -2);
        });
        Assert.AreEqual(1, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_sltiu_false()
    {
        RunTest(m =>
        {
            this.registers[4] = -2;
            this.registers[5] = 5;

            m.slti(4, 5, 5);
        });
        Assert.AreEqual(0, this.registers[4]);
    }


    [Test]
    public void RiscVEmu_lb()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x4;
            this.registers[5] = 0x4;

            m.lb(4, 5, 0x4);
            m.dw(0);
            m.dw(0x82);
        });
        Assert.AreEqual(unchecked((int)0xFFFFFF82), this.registers[4]);
    }

    [Test]
    public void RiscVEmu_lbu()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x4;
            this.registers[5] = 0x4;

            m.lbu(4, 5, 0x4);
            m.dw(0);
            m.dw(0x82);
        });
        Assert.AreEqual((int)0x00000082, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_lh()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x4;
            this.registers[5] = 0x4;

            m.lh(4, 5, 0x4);
            m.dw(0);
            m.dw(0x00008765);
        });
        Assert.AreEqual(unchecked((int)0xFFFF8765), this.registers[4]);
    }


    [Test]
    public void RiscVEmu_lhu()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x4;
            this.registers[5] = 0x4;

            m.lhu(4, 5, 0x4);
            m.dw(0);
            m.dw(0x00008765);
        });
        Assert.AreEqual((int)0x00008765, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_lw()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x4;
            this.registers[5] = 0x4;

            m.lw(4, 5, 0x4);
            m.dw(0);
            m.dw(0x12345678);
        });
        Assert.AreEqual((int)0x12345678, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_sb()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x12345678;
            this.registers[5] = 0x4;

            m.sb(4, 5, 0x124);
            m.dw(0, 0x128);
        });
        Assert.AreEqual(0x78, memory.ReadLeWord32(0x128));
    }

    [Test]
    public void RiscVEmu_sh()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x12345678;
            this.registers[5] = 0x4;

            m.sh(4, 5, 0x124);
            m.dw(0, 0x128);
        });
        Assert.AreEqual(0x5678, memory.ReadLeWord32(0x128));
    }

    [Test]
    public void RiscVEmu_sw()
    {
        RunTest(m =>
        {
            this.registers[4] = 0x12345678;
            this.registers[5] = 0x4;

            m.sw(4, 5, 0x124);
            m.dw(0, 0x128);
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
        Assert.AreEqual(1, this.registers[4]);
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
        Assert.AreEqual(1, this.registers[4]);
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
        Assert.AreEqual(1, this.registers[4]);
    }

    [Test]
    public void RiscVEmu_jalr()
    {
        RunTest(m =>
        {
            this.registers[1] = 0x8;
            this.registers[10] = 1;

            m.jalr(0, 1, 0);
            m.addi(10, 0, 0x42);

            m.addi(11, 10, 0);
        });
        Assert.AreEqual(1, this.registers[11]);
    }

    [Test]
    public void RiscVEmu_ecall()
    {
        RunTest(m =>
        {
            this.registers[17] = 0;
            this.registers[10] = 1;

            m.ecall();
            m.addi(10, 0, 0x42);
        });
        Assert.AreEqual(1, this.registers[10]);
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
        Assert.AreEqual(0x12345678, this.registers[1]);
    }

    [Test]
    public void RiscVEmu_auipc()
    {
        RunTest(m =>
        {
            m.auipc(1, 0x12345);
            m.addi(1, 1, 0x678);
        });
        Assert.AreEqual(0x12345678, this.registers[1]);
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
        Assert.AreEqual(0, this.registers[4]);
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
        Assert.AreEqual(-2, this.registers[4]);
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
        Assert.AreEqual(1, this.registers[4]);
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
        Assert.AreEqual(1, this.registers[4]);
    }



    [Test]
    public void RiscVEmu_xori()
    {
        RunTest(m =>
                {
                    this.registers[4] = 0x55555555;
                    m.xori(5, 4, -1);
                });
        Assert.AreEqual(unchecked((int)0xAAAAAAAA), this.registers[5]);
    }


    [Test]
    public void RiscVEmu_ori()
    {
        RunTest(m =>
                {
                    this.registers[4] = 0x55555555;
                    m.ori(5, 4, -1);
                });
        Assert.AreEqual(unchecked((int)0xFFFFFFFF), this.registers[5]);
    }


    [Test]
    public void RiscVEmu_andi()
    {
        RunTest(m =>
                {
                    this.registers[4] = 0x55555555;
                    m.andi(5, 4, 0xF);
                });
        Assert.AreEqual(5, this.registers[5]);
    }


    [Test]
    public void RiscVEmu_slli()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 2);           // 0000010
                    m.slli(5, 4, 1);
                });
        Assert.AreEqual(4, this.registers[5]);
    }


    [Test]
    public void RiscVEmu_srli()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, 4);           // 0000100
                    m.srli(5, 4, 1);
                });
        Assert.AreEqual(2, this.registers[5]);
    }


    [Test]
    public void RiscVEmu_srai()
    {
        RunTest(m =>
                {
                    m.addi(4, 0, -1);

                    m.srai(5, 4, 4);
                });
        Assert.AreEqual(-1, this.registers[5]);
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
        Assert.AreEqual(-1, this.registers[3]);
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
        Assert.AreEqual(0x10, this.registers[5]);
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
        Assert.AreEqual(result, this.registers[6]);
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
        Assert.AreEqual(result, this.registers[6]);
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
        Assert.AreEqual(0x555, this.registers[6]);
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
        Assert.AreEqual(3, this.registers[6]);
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
        Assert.AreEqual(-1, this.registers[6]);
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
        Assert.AreEqual(0x2FA, this.registers[6]);
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
        Assert.AreEqual(0x0A0, this.registers[6]);
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fence_tso()
    {
        /*
        RunTest(m =>
           {
               m.fence_tso(-1, -1, -1);
           });
           */
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_pause()
    {
        /*
        RunTest(m =>
                {

                    m.pause(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
        */
    }



    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_ebreak()
    {
        /*
        RunTest(m =>
                {

                    m.ebreak();
                });
                */
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
        /*
        RunTest(m =>
           {
               m.fence_i(-1, -1, -1);
           });
           */
           Assert.Fail("Not implemented");
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
        /*
        RunTest(m =>
           {
               m.lr_w(-1, -1, -1);
           });
           */
           Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sc_w()
    {
        /*
        RunTest(m =>
           {
               m.sc_w(-1, -1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amosw_p_w()
    {
        /*
        RunTest(m =>
           {
               m.amoswap_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoadd_w()
    {
        /*
        RunTest(m =>
           {
               m.amoadd_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoxor_w()
    {
        /*
        RunTest(m =>
           {
               m.amoxor_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoand_w()
    {
        /*
        RunTest(m =>
           {
               m.amoand_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoor_w()
    {
        /*
        RunTest(m =>
           {
               m.amoor_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomin_w()
    {
        /*
        RunTest(m =>
           {
               m.amomin_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomax_w()
    {
        /*
        RunTest(m =>
           {
               m.amomax_w(-1, -1, -1);
           });*/
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amominu_w()
    {
        /*
        RunTest(m =>
           {
               m.amominu_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomaxu_w()
    {
        /*
        RunTest(m =>
           {
               m.amomaxu_w(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_lr_d()
    {
        /*
        RunTest(m =>
           {
               m.lr_d(-1, -1, -1);
           });
           */ 
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_sc_d()
    {
        // RunTest(m =>
        //    {
        //        m.sc_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoswap_d()
    {
        /*
        RunTest(m =>
           {
               m.amoswap_d(-1, -1, -1);
           });
           */
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoadd_d()
    {
        // RunTest(m =>
        //    {
        //        m.amoadd_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }

    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoand_d()
    {
        // RunTest(m =>
        //    {
        //        m.amoand_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoxor_d()
    {
        // RunTest(m =>
        //    {
        //        m.amoxor_d(-1, -1, -1);
        //    });
           Assert.Fail();

    }





    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amoor_d()
    {
        // RunTest(m =>
        //    {
        //        m.amoor_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomin_d()
    {
        // RunTest(m =>
        //    {
        //        m.amomin_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomax_d()
    {
        // RunTest(m =>
        //    {
        //        m.amomax_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amominu_d()
    {
        // RunTest(m =>
        //    {
        //        m.amominu_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_amomaxu_d()
    {

        // RunTest(m =>
        //    {
        //        m.amomaxu_d(-1, -1, -1);
        //    });
           Assert.Fail();
    }



    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flw()
    {
        RunTest(m =>
                {

                    m.flw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsw()
    {
        RunTest(m =>
                {

                    m.fsw(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_s()
    {
        RunTest(m =>
           {
               m.fmadd_s(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_s()
    {
        RunTest(m =>
           {
               m.fmsub_s(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_s()
    {
        RunTest(m =>
           {
               m.fnmsub_s(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_s()
    {
        RunTest(m =>
           {
               m.fnmadd_s(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_s()
    {
        RunTest(m =>
           {
               m.fadd_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_s()
    {
        RunTest(m =>
           {
               m.fsub_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_s()
    {
        RunTest(m =>
           {
               m.fmul_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_s()
    {
        RunTest(m =>
           {
               m.fdiv_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_s()
    {
        RunTest(m =>
           {
               m.fsqrt_s(-1, -1, -1 ,-1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_s()
    {
        RunTest(m =>
           {
               m.fsgnj_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_s()
    {
        RunTest(m =>
           {
               m.fsgnjn_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_s()
    {
        RunTest(m =>
           {
               m.fsgnjx_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_s()
    {
        RunTest(m =>
           {
               m.fmin_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_s()
    {
        RunTest(m =>
           {
               m.fmax_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_s()
    {
        RunTest(m =>
           {
               m.fcvt_w_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_s()
    {
        RunTest(m =>
           {
               m.fcvt_wu_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_x_w()
    {
        RunTest(m =>
           {
               m.fmv_x_w(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_s()
    {
        RunTest(m =>
           {
               m.feq_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_s()
    {
        RunTest(m =>
           {
               m.flt_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_s()
    {
        RunTest(m =>
           {
               m.fle_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_s()
    {
        RunTest(m =>
           {
               m.fclass_s(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_w()
    {
        RunTest(m =>
           {
               m.fcvt_s_w(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_wu()
    {
        RunTest(m =>
           {
               m.fcvt_s_wu(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_w_x()
    {
        RunTest(m =>
           {
               m.fmv_w_x(-1, -1, -1);
           });
    }


    // rv64f standard extension (in addition to/rv32f)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_s()
    {
        RunTest(m =>
           {
               m.fcvt_l_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_s()
    {
        RunTest(m =>
           {
               m.fcvt_lu_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_l()
    {
        RunTest(m =>
           {
               m.fcvt_s_l(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_lu()
    {
        RunTest(m =>
           {
               m.fcvt_s_lu(-1, -1, -1, -1);
           });
    }






    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fld()
    {
        RunTest(m =>
                {

                    m.fld(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsd()
    {
        RunTest(m =>
                {

                    m.fsd(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_d()
    {
        RunTest(m =>
           {
               m.fmadd_d(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_d()
    {
        RunTest(m =>
           {
               m.fmsub_d(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_d()
    {
        RunTest(m =>
           {
               m.fnmsub_d(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_d()
    {
        RunTest(m =>
           {
               m.fnmadd_d(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_d()
    {
        RunTest(m =>
           {
               m.fadd_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_d()
    {
        RunTest(m =>
           {
               m.fsub_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_d()
    {
        RunTest(m =>
           {
               m.fmul_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_d()
    {
        RunTest(m =>
           {
               m.fdiv_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_d()
    {
        RunTest(m =>
           {
               m.fsqrt_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_d()
    {
        RunTest(m =>
           {
               m.fsgnj_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_d()
    {
        RunTest(m =>
           {
               m.fsgnjn_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_d()
    {
        RunTest(m =>
           {
               m.fsgnjx_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_d()
    {
        RunTest(m =>
           {
               m.fmin_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_d()
    {
        RunTest(m =>
           {
               m.fmax_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_d()
    {
        RunTest(m =>
           {
               m.fcvt_s_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_s()
    {
        RunTest(m =>
           {
               m.fcvt_d_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_d()
    {
        RunTest(m =>
           {
               m.feq_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_d()
    {
        RunTest(m =>
           {
               m.flt_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_d()
    {
        RunTest(m =>
           {
               m.fle_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_d()
    {
        RunTest(m =>
           {
               m.fclass_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_d()
    {
        RunTest(m =>
           {
               m.fcvt_w_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_d()
    {
        RunTest(m =>
           {
               m.fcvt_wu_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_w()
    {
        RunTest(m =>
           {
               m.fcvt_d_w(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_wu()
    {
        RunTest(m =>
           {
               m.fcvt_d_wu(-1, -1, -1, -1);
           });
    }



    //rv64d standard extension (in addition to/rv32d)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_d()
    {
        RunTest(m =>
           {
               m.fcvt_l_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_d()
    {
        RunTest(m =>
           {
               m.fcvt_lu_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_x_d()
    {
        RunTest(m =>
           {
               m.fmv_x_d(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_l()
    {
        RunTest(m =>
           {
               m.fcvt_d_l(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_lu()
    {
        RunTest(m =>
           {
               m.fcvt_d_lu(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_d_x()
    {
        RunTest(m =>
           {
               m.fmv_d_x(-1, -1, -1);
           });
    }




    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flq()
    {
        RunTest(m =>
                {

                    m.flq(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsq()
    {
        RunTest(m =>
                {

                    m.fsq(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_q()
    {
        RunTest(m =>
           {
               m.fmadd_q(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_q()
    {
        RunTest(m =>
           {
               m.fmsub_q(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_q()
    {
        // RunTest(m =>
        //    {
        //        m.fnmsub_q(-1, -1, -1);
        //    });
           Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_q()
    {
        RunTest(m =>
           {
               m.fnmadd_q(-1, -1, -1, -1, -1);
           });
        Assert.Fail();
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_q()
    {
        RunTest(m =>
           {
               m.fadd_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_q()
    {
        RunTest(m =>
           {
               m.fsub_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_q()
    {
        RunTest(m =>
           {
               m.fmul_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_q()
    {
        RunTest(m =>
           {
               m.fdiv_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_q()
    {
        RunTest(m =>
           {
               m.fsqrt_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_q()
    {
        RunTest(m =>
           {
               m.fsgnj_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_q()
    {
        RunTest(m =>
           {
               m.fsgnjn_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_q()
    {
        RunTest(m =>
           {
               m.fsgnjx_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_q()
    {
        RunTest(m =>
           {
               m.fmin_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_q()
    {
        RunTest(m =>
           {
               m.fmax_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_q()
    {
        RunTest(m =>
           {
               m.fcvt_s_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_s()
    {
        RunTest(m =>
           {
               m.fcvt_q_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_q()
    {
        RunTest(m =>
           {
               m.fcvt_d_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_d()
    {
        RunTest(m =>
           {
               m.fcvt_q_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_q()
    {
        RunTest(m =>
           {
               m.feq_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_q()
    {
        RunTest(m =>
           {
               m.flt_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_q()
    {
        RunTest(m =>
           {
               m.fle_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_q()
    {
        RunTest(m =>
           {
               m.fclass_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_q()
    {
        RunTest(m =>
           {
               m.fcvt_w_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_q()
    {
        RunTest(m =>
           {
               m.fcvt_wu_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_w()
    {
        RunTest(m =>
           {
               m.fcvt_q_w(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_wu()
    {
        RunTest(m =>
           {
               m.fcvt_q_wu(-1, -1, -1, -1);
           });
    }



    //rv64q standard extension (in addition to/rv32q)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_q()
    {
        RunTest(m =>
           {
               m.fcvt_l_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_q()
    {
        RunTest(m =>
           {
               m.fcvt_lu_q(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_l()
    {
        RunTest(m =>
           {
               m.fcvt_q_l(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_lu()
    {
        RunTest(m =>
           {
               m.fcvt_q_lu(-1, -1, -1, -1);
           });
    }





    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flh()
    {
        RunTest(m =>
                {

                    m.flh(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsh()
    {
        RunTest(m =>
                {

                    m.fsh(-1, -1, -1);
                });
        Assert.Fail("Not implemented");
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmadd_h()
    {
        RunTest(m =>
           {
               m.fmadd_h(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmsub_h()
    {
        RunTest(m =>
           {
               m.fmsub_h(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmsub_h()
    {
        RunTest(m =>
           {
               m.fnmsub_h(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fnmadd_h()
    {
        RunTest(m =>
           {
               m.fnmadd_h(-1, -1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fadd_h()
    {
        RunTest(m =>
           {
               m.fadd_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsub_h()
    {
        RunTest(m =>
           {
               m.fsub_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmul_h()
    {
        RunTest(m =>
           {
               m.fmul_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fdiv_h()
    {
        RunTest(m =>
           {
               m.fdiv_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsqrt_h()
    {
        RunTest(m =>
           {
               m.fsqrt_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnj_h()
    {
        RunTest(m =>
           {
               m.fsgnj_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjn_h()
    {
        RunTest(m =>
           {
               m.fsgnjn_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fsgnjx_h()
    {
        RunTest(m =>
           {
               m.fsgnjx_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmin_h()
    {
        RunTest(m =>
           {
               m.fmin_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmax_h()
    {
        RunTest(m =>
           {
               m.fmax_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_s_h()
    {
        RunTest(m =>
           {
               m.fcvt_s_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_s()
    {
        RunTest(m =>
           {
               m.fcvt_h_s(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_d_h()
    {
        RunTest(m =>
           {
               m.fcvt_d_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_d()
    {
        RunTest(m =>
           {
               m.fcvt_h_d(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_q_h()
    {
        RunTest(m =>
           {
               m.fcvt_q_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_q()
    {
        RunTest(m =>
           {
               m.fcvt_h_q(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_feq_h()
    {
        RunTest(m =>
           {
               m.feq_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_flt_h()
    {
        RunTest(m =>
           {
               m.flt_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fle_h()
    {
        RunTest(m =>
           {
               m.fle_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fclass_h()
    {
        RunTest(m =>
           {
               m.fclass_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_w_h()
    {
        RunTest(m =>
           {
               m.fcvt_w_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_wu_h()
    {
        RunTest(m =>
           {
               m.fcvt_wu_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_x_h()
    {
        RunTest(m =>
           {
               m.fmv_x_h(-1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_w()
    {
        RunTest(m =>
           {
               m.fcvt_h_w(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_wu()
    {
        RunTest(m =>
           {
               m.fcvt_h_wu(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fmv_h_x()
    {
        RunTest(m =>
           {
               m.fmv_h_x(-1, -1, -1);
           });
    }



    //rv64zfh standard extension (in addition to/rv32zfh)
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_l_h()
    {
        RunTest(m =>
           {
               m.fcvt_l_h(-1, -1, -1, -1);
           });
    }



    //rv64zfh standard extension (in addition to/rv32zfh)/
    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_lu_h()
    {
        RunTest(m =>
           {
               m.fcvt_lu_h(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_l()
    {
        RunTest(m =>
           {
               m.fcvt_h_l(-1, -1, -1, -1);
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_fcvt_h_lu()
    {
        RunTest(m =>
           {
               m.fcvt_h_lu(-1, -1, -1, -1);
           });
    }



    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_wrs_nto()
    {
        RunTest(m =>
           {
               m.wrs_nto();
           });
    }


    [Test]
    [Ignore("nyi")]
    public void RiscVEmu_wrs_sto()
    {
        RunTest(m =>
           {
               m.wrs_sto();
           });
    }
}