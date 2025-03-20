namespace rvfun.UnitTests;

#pragma warning disable NUnit2005

[TestFixture]
public class AssemblerTests
{
    private const int op1 = 0b10001;
    private const int op2 = 0b10011;
    private const int op3 = 0b10101;
    private const int op4 = 0b10111;

    private Assembler Assemble(Action<Assembler> asmClient)
    {
        var m = new Assembler(new Logger());

        asmClient(m);
        return m;
    }


    private void RunTest(uint uInstrExpected, Action<Assembler> testBuilder)
    {
        var m = new Assembler(new Logger());

        testBuilder(m);

        var bytes = m.Section.GetAssembledBytes();
        var memory = new Memory();
        memory.Allocate(0, bytes, AccessMode.RX);

        var uInstr = memory.ReadLeWord32(0);
        if (uInstrExpected != uInstr)
        {
            Assert.AreEqual(ToBinary(uInstrExpected, 32), ToBinary(uInstr, 32));
        }
    }

    private static string ToBinary(uint value, int bitsize)
    {
        return Convert.ToString(value, 2).PadLeft(bitsize, '0');
    }

    [Test]
    public void RiscvAsm_mul()
    {
        RunTest(
            0b0000001_11111_10101_000_10001_0110011u,
            m => m.mul(17, 21, 31));
    }

    [Test]
    public void RiscvAsm_slti()
    {
        RunTest(
            0b111111111110_10101_010_10001_0010011u,
            m => m.slti(17, 21, -2));
    }

    [Test]
    public void RiscvAsm_lb()
    {
        RunTest(
            0b111111111110_10101_000_10001_0000011u,
            m => m.lb(17, 21, -2));
    }

    [Test]
    public void RiscvAsm_lh()
    {
        RunTest(
            0b111111111110_10101_001_10001_0000011u,
            m => m.lh(17, 21, -2));
    }

    [Test]
    public void RiscvAsm_lw()
    {
        RunTest(
            0b111111111110_10101_010_10001_0000011u,
            m => m.lw(17, 21, -2));
    }

    [Test]
    public void RiscvAsm_lbu()
    {
        RunTest(
            0b111111111110_10101_100_10001_0000011u,
            m => m.lbu( 17, 21, -2));
    }

    [Test]
    public void RiscvAsm_lhu()
    {
        RunTest(
            0b111111111110_10101_101_10001_0000011u,
            m => m.lhu( 17, 21, -2));
    }

    [Test]
    public void RiscvAsm_sb()
    {
        RunTest(
            0b1010101_10001_10101_000_01010_0100011u,
            m => m.sb( 17, 21, -1366));
    }

    [Test]
    public void RiscvAsm_sh()
    {
        RunTest(
            0b1010101_10001_10101_001_01010_0100011u,
            m => m.sh(17, 21, -1366));
    }

    [Test]
    public void RiscvAsm_sw()
    {
        RunTest(
            0b1010101_10001_10101_010_01010_0100011u,
            m => m.sw(17, 21, -1366));
    }

    [Test]
    public void RiscvAsm_beq()
    {
        RunTest(
        // imm[12|10:5] rs2 rs1 000 imm[4:1|11] 1100011 beq
        0b1010101_10101_10011_000_01011_1100011u,
        m => m.beq(op2, op3, -1366));
    }

    [Test]
    public void RiscvAsm_bne()
    {
        //     imm[12|10:5] rs2 rs1 001 imm[4:1|11] 1100011 
        RunTest(
            0b1010101_10101_10001_001_01011_1100011u,
            m => m.bne(17, 21, -1366));
    }

    [Test]
    public void RiscvAsm_jal()
    {
        RunTest(
            // imm[20|10:1|11|19:12] rd 1101111 
            0b10101010101111111111_00000_1101111u,
            m => m.jal(0, -1366));
    }

    [Test]
    public void RiscvAsm_jalr()
    {
        RunTest(
            // imm[11:0] rs1 000 rd 1100111 JALR
            0b000000000000_00001_000_00000_1100111u,
            m => m.jalr(0, 1, 0));
    }

    [Test]
    public void RiscvAsm_ecall()
    {
        RunTest(
            // 000000000000 00000 000 00000 1110011 ECALL
            0b000000000000_00000_000_00000_1110011u,
            m => m.ecall());
    }

    [Test]
    public void RiscvAsm_lui()
    {
        RunTest(
            // imm[31:12] rd 0110111 LUI
            0b00010010001101000101_00001_0110111u,
            m => m.lui(1, 0x12345, 0));
    }

    [Test]
    public void RiscvAsm_auipc()
    {
        RunTest(
            // imm[31:12] rd 0010111 AUIPC
            0b00010010001101000101_00001_0010111u,
            m => m.auipc(1, 0x12345));
    }


    [Test]
    public void RiscvAsm_blt()
    {
        RunTest(
        // imm[12|10:5] rs2 rs1 100 imm[4:1|11] 1100011 blt
        0b1010101_10101_10011_100_01011_1100011u,
        m => m.blt(op2, op3, -1366));
    }


    [Test]
    public void RiscvAsm_bge()
    {
        RunTest(
        // imm[12|10:5] rs2 rs1 101 imm[4:1|11] 1100011 bge
        0b1010101_10101_10011_101_01011_1100011u,
        m => m.bge(op2, op3, -1366));
    }

    [Test]
    public void RiscvAsm_bltu()
    {
        RunTest(
        // imm[12|10:5] rs2 rs1 110 imm[4:1|11] 1100011 bltu
        0b1010101_10101_10011_110_01011_1100011u,
        m => m.bltu(op2, op3, -1366));
    }

    [Test]
    public void RiscvAsm_bgeu()
    {
        RunTest(
        // imm[12|10:5] rs2 rs1 111 imm[4:1|11] 1100011 bgeu
        0b1010101_10101_10011_111_01011_1100011u,
        m => m.bgeu(op2, op3, -1366));
    }


    [Test]
    public void RiscvAsm_addi()
    {
        RunTest(
        // imm[11:0] rs1 000 rd 0010011 addi
        0b111111111110_10011_000_10001_0010011u,
        m => m.addi(op1, op2, -2));
    }

    [Test]
    public void RiscvAsm_li()
    {
        RunTest(
        // imm[11:0] 00000 000 rd 0010011 addi
        0b111111111110_00000_000_10001_0010011u,
        m => m.li(op1, -2));
    }

    [Test]
    public void RiscAsm_symbol()
    {
        var asm = Assemble(m =>
        {
            m.li(4, 9);
            m.label("mylabel");
            m.li(2, 10);
        });
        Assert.That(asm.Symbols["mylabel"].Address, Is.EqualTo(4u));
    }

        [Test]
    public void RiscAsm_symbol_redefinition()
    {
        var asm = Assemble(m =>
        {
            m.li(4, 9);
            m.label("mylabel");
            m.li(2, 10);
            m.label("mylabel");

        });
        Assert.That(asm.Logger.Errors.Count, Is.EqualTo(1));
    }

    [Test]
    public void RiscAsm_reloc_added()
    {
        var asm = Assemble(m =>
        {
            m.blt(1, 2, "mylabel");
        });
        Assert.That(asm.Relocations.Count, Is.EqualTo(1));
        var rel = asm.Relocations[0];
        Assert.That(rel.SymbolName, Is.EqualTo("mylabel"));
        Assert.That(rel.Rtype, Is.EqualTo(RelocationType.B_PcRelative));
    }



    [Test]
    public void RiscvAsm_sltiu()
    {
        RunTest(
        // imm[11:0] rs1 011 rd 0010011 sltiu
        0b111111111110_10011_011_10001_0010011u,
        m => m.sltiu(op1, op2, -2));
    }


    [Test]
    public void RiscvAsm_xori()
    {
        RunTest(
        // imm[11:0] rs1 100 rd 0010011 xori
        0b111111111110_10011_100_10001_0010011u,
        m => m.xori(op1, op2, -2));
    }


    [Test]
    public void RiscvAsm_ori()
    {
        RunTest(
        // imm[11:0] rs1 110 rd 0010011 ori
        0b111111111110_10011_110_10001_0010011u,
        m => m.ori(op1, op2, -2));
    }


    [Test]
    public void RiscvAsm_andi()
    {
        RunTest(
            // imm[11:0] rs1 111 rd 0010011 andi
            0b111111111110_10011_111_10001_0010011u,
            m => m.andi(op1, op2, -2));
    }

    [Test]
    public void RiscvAsm_srli()
    {
        RunTest(
            // 0000000 shamt rs1 101 rd 0010011 srli
            0b0000000_10101_10011_101_10001_0010011u,
            m => m.srli(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_srai()
    {
        RunTest(
        // 0100000 shamt rs1 101 rd 0010011 srai
        0b0100000_10101_10011_101_10001_0010011u,
        m => m.srai(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_add()
    {
        RunTest(
        // 0000000 rs2 rs1 000 rd 0110011 add
        0b0000000_10101_10011_000_10001_0110011u,
        m => m.add(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sub()
    {
        RunTest(
        // 0100000 rs2 rs1 000 rd 0110011 sub
        0b0100000_10101_10011_000_10001_0110011u,
        m => m.sub(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sll()
    {
        RunTest(
        // 0000000 rs2 rs1 001 rd 0110011 sll
        0b0000000_10101_10011_001_10001_0110011u,
        m => m.sll(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_slt()
    {
        RunTest(
        // 0000000 rs2 rs1 010 rd 0110011 slt
        0b0000000_10101_10011_010_10001_0110011u,
        m => m.slt(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sltu()
    {
        RunTest(
        // 0000000 rs2 rs1 011 rd 0110011 sltu
        0b0000000_10101_10011_011_10001_0110011u,
        m => m.sltu(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_xor()
    {
        RunTest(
        // 0000000 rs2 rs1 100 rd 0110011 xor
        0b0000000_10101_10011_100_10001_0110011u,
        m => m.xor(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_srl()
    {
        RunTest(
        // 0000000 rs2 rs1 101 rd 0110011 srl
        0b0000000_10101_10011_101_10001_0110011u,
        m => m.srl(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sra()
    {
        RunTest(
        // 0100000 rs2 rs1 101 rd 0110011 sra
        0b0100000_10101_10011_101_10001_0110011u,
        m => m.sra(op1, op2, op3));
    }

    [Test]
    public void RiscvAsm_or()
    {
        RunTest(
        // 0000000 rs2 rs1 110 rd 0110011 or
        0b0000000_10101_10011_110_10001_0110011u,
        m => m.or(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_and()
    {
        RunTest(
        // 0000000 rs2 rs1 111 rd 0110011 and
        0b0000000_10101_10011_111_10001_0110011u,
        m => m.and(op1, op2, op3));
    }


    [Test]
    [Ignore("Save for later")]
    public void RiscvAsm_fence_tso()
    {
        /*
        RunTest(
        // 1000 0011 0011 00000 000 00000 0001111 fence.tso
        0b1000_0011_0011_00000_000_00000_0001111u,
        m => m.fence_tso(op1, op2, op3));
        */
    }


    [Test]
    [Ignore("Save for later")]
    public void RiscvAsm_pause()
    {
/*        RunTest(
        // 0000 0001 0000 00000 000 00000 0001111 pause
        0b0000_0001_0000_00000_000_00000_0001111u,
        m => m.pause(op1, op2, op3));
*/
    }


    [Test]
    public void RiscvAsm_ebreak()
    {
        RunTest(
        // 000000000001 00000 000 00000 1110011 ebreak
        0b000000000001_00000_000_00000_1110011u,
        m => m.ebreak(op1, op2, op3));
    }



    [Test]
    public void RiscvAsm_lwu()
    {
        RunTest(
        // imm[11:0] rs1 110 rd 0000011 lwu
        0b111111111110_10011_110_10001_0000011u,
        m => m.lwu(op1, op2, -2));
    }


    [Test]
    public void RiscvAsm_ld()
    {
        RunTest(
        // imm[11:0] rs1 011 rd 0000011 ld
        0b111111111110_10011_011_10001_0000011u,
        m => m.ld(op1, op2, -2));
    }


    [Test]
    public void RiscvAsm_sd()
    {
        RunTest(
        // imm[11:5] rs2 rs1 011 imm[4:0] 0100011 sd
        0b1010101_10101_10011_011_01010_0100011u,
        m => m.sd(op3, op2, -1366));
    }


    [Test]
    public void RiscvAsm_slli()
    {
        RunTest(
        // 000000 shamt rs1 001 rd 0010011 slli
        0b000000_10101_10011_001_10001_0010011u,
        m => m.slli(op1, op2, op3));
    }

    [Test]
    [Ignore("Test this on 64-bit processor")]
    public void RiscvAsm_srli_64()
    {
        RunTest(
        // 000000 shamt rs1 101 rd 0010011 srli
        0b000000_110101_10011_101_10001_0010011u,
        m => m.srli(op1, op2, 0b110101));
    }


    [Test]
    [Ignore("Test this on 64-bit processor")]
    public void RiscvAsm_srai_64()
    {
        RunTest(
        // 010000 shamt rs1 101 rd 0010011 srai
        0b010000_110101_10011_101_10001_0010011u,
        m => m.srai(op1, op2, 0b110101));
    }


    [Test]
    public void RiscvAsm_addiw()
    {
        RunTest(
        // imm[11:0] rs1 000 rd 0011011 addiw
        0b111111111110_10011_000_10001_0011011u,
        m => m.addiw(op1, op2, -2));
    }


    [Test]
    public void RiscvAsm_slliw()
    {
        RunTest(
        // 0000000 shamt rs1 001 rd 0011011 slliw
        0b0000000_10101_10011_001_10001_0011011u,
        m => m.slliw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_srliw()
    {
        RunTest(
        // 0000000 shamt rs1 101 rd 0011011 srliw
        0b0000000_10101_10011_101_10001_0011011u,
        m => m.srliw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sraiw()
    {
        RunTest(
        // 0100000 shamt rs1 101 rd 0011011 sraiw
        0b0100000_10101_10011_101_10001_0011011u,
        m => m.sraiw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_addw()
    {
        RunTest(
        // 0000000 rs2 rs1 000 rd 0111011 addw
        0b0000000_10101_10011_000_10001_0111011u,
        m => m.addw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_subw()
    {
        RunTest(
        // 0100000 rs2 rs1 000 rd 0111011 subw
        0b0100000_10101_10011_000_10001_0111011u,
        m => m.subw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sllw()
    {
        RunTest(
        // 0000000 rs2 rs1 001 rd 0111011 sllw
        0b0000000_10101_10011_001_10001_0111011u,
        m => m.sllw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_srlw()
    {
        RunTest(
        // 0000000 rs2 rs1 101 rd 0111011 srlw
        0b0000000_10101_10011_101_10001_0111011u,
        m => m.srlw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sraw()
    {
        RunTest(
        // 0100000 rs2 rs1 101 rd 0111011 sraw
        0b0100000_10101_10011_101_10001_0111011u,
        m => m.sraw(op1, op2, op3));
    }



    //rv32/rv64 zifencei standard/extension
    [Test]
    [Ignore("not yet")]
    public void RiscvAsm_fence_i()
    {
        /*
        RunTest(
        // imm[11:0] rs1 001 rd 0001111 fence.i
        0b111111111110_10011_001_10001_0001111u,
        m => m.fence_i(op1, op2, op3));
        */
    }



    // rv32/rv64 zicsr standard/extension
    [Test]
    public void RiscvAsm_csrrw()
    {
        RunTest(
        // csr rs1 001 rd 1110011 csrrw
        0b111100001111_10011_001_10001_1110011u,
        m => m.csrrw(op1, op2, 0xF0F));
    }


    [Test]
    public void RiscvAsm_csrrs()
    {
        RunTest(
        // csr rs1 010 rd 1110011 csrrs
        0b111100001111_10011_010_10001_1110011u,
        m => m.csrrs(op1, op2, 0xF0F));
    }


    [Test]
    public void RiscvAsm_csrrc()
    {
        RunTest(
        // csr rs1 011 rd 1110011 csrrc
        0b111100001111_10011_011_10001_1110011u,
        m => m.csrrc(op1, op2, 0xF0F));
    }


    [Test]
    public void RiscvAsm_csrrwi()
    {
        RunTest(
        // csr uimm 101 rd 1110011 csrrwi
        0b111100001111_10011_101_10001_1110011u,
        m => m.csrrwi(op1, op2, 0xF0F));
    }


    [Test]
    public void RiscvAsm_csrrsi()
    {
        RunTest(
        // csr uimm 110 rd 1110011 csrrsi
        0b111100001111_10011_110_10001_1110011u,
        m => m.csrrsi(op1, op2, 0xF0F));
    }


    [Test]
    public void RiscvAsm_csrrci()
    {
        RunTest(
        // csr uimm 111 rd 1110011 csrrci
        0b111100001111_10011_111_10001_1110011u,
        m => m.csrrci(op1, op2, 0xF0F));
    }



    // rv32m standard/extension

    [Test]
    public void RiscvAsm_mulh()
    {
        RunTest(
        // 0000001 rs2 rs1 001 rd 0110011 mulh
        0b0000001_10101_10011_001_10001_0110011u,
        m => m.mulh(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_mulhsu()
    {
        RunTest(
        // 0000001 rs2 rs1 010 rd 0110011 mulhsu
        0b0000001_10101_10011_010_10001_0110011u,
        m => m.mulhsu(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_mulhu()
    {
        RunTest(
        // 0000001 rs2 rs1 011 rd 0110011 mulhu
        0b0000001_10101_10011_011_10001_0110011u,
        m => m.mulhu(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_div()
    {
        RunTest(
        // 0000001 rs2 rs1 100 rd 0110011 div
        0b0000001_10101_10011_100_10001_0110011u,
        m => m.div(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_divu()
    {
        RunTest(
        // 0000001 rs2 rs1 101 rd 0110011 divu
        0b0000001_10101_10011_101_10001_0110011u,
        m => m.divu(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_rem()
    {
        RunTest(
        // 0000001 rs2 rs1 110 rd 0110011 rem
        0b0000001_10101_10011_110_10001_0110011u,
        m => m.rem(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_remu()
    {
        RunTest(
        // 0000001 rs2 rs1 111 rd 0110011 remu
        0b0000001_10101_10011_111_10001_0110011u,
        m => m.remu(op1, op2, op3));
    }



    // rv64m standard extension (in addition to/rv32m)
    [Test]
    public void RiscvAsm_mulw()
    {
        RunTest(
        // 0000001 rs2 rs1 000 rd 0111011 mulw
        0b0000001_10101_10011_000_10001_0111011u,
        m => m.mulw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_divw()
    {
        RunTest(
        // 0000001 rs2 rs1 100 rd 0111011 divw
        0b0000001_10101_10011_100_10001_0111011u,
        m => m.divw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_divuw()
    {
        RunTest(
        // 0000001 rs2 rs1 101 rd 0111011 divuw
        0b0000001_10101_10011_101_10001_0111011u,
        m => m.divuw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_remw()
    {
        RunTest(
        // 0000001 rs2 rs1 110 rd 0111011 remw
        0b0000001_10101_10011_110_10001_0111011u,
        m => m.remw(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_remuw()
    {
        RunTest(
        // 0000001 rs2 rs1 111 rd 0111011 remuw
        0b0000001_10101_10011_111_10001_0111011u,
        m => m.remuw(op1, op2, op3));
    }


#if NOT_YET

    //rv32a standard/extension
    [Test]
    public void RiscvAsm_lr_w()
    {
        RunTest(
        // 00010 aq rl 00000 rs1 010 rd 0101111 lr.w
        0b00010_1_1_00000_10011_010_10001_0101111u,
        m => m.lr_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_sc_w()
    {
        RunTest(
        // 00011 aq rl rs2 rs1 010 rd 0101111 sc.w
        0b00011_1_1_10101_10011_010_10001_0101111u,
        m => m.sc_w(op1, op2, op3));
    }

#region AMO
/*

    [Test]
    public void RiscvAsm_amoswap_w()
    {
        RunTest(
        // 00001 aq rl rs2 rs1 010 rd 0101111 amoswap.w
        0b00001_1_1_10101_10011_010_10001_0101111u,
        m => m.amoswap_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amoadd_w()
    {
        RunTest(
        // 00000 aq rl rs2 rs1 010 rd 0101111 amoadd.w
        0b00000_1_1_10101_10011_010_10001_0101111u,
        m => m.amoadd_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amoxor_w()
    {
        RunTest(
        // 00100 aq rl rs2 rs1 010 rd 0101111 amoxor.w
        0b00100_1_1_10101_10011_010_10001_0101111u,
        m => m.amoxor_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amoand_w()
    {
        RunTest(
        // 01100 aq rl rs2 rs1 010 rd 0101111 amoand.w
        0b01100_1_1_10101_10011_010_10001_0101111u,
        m => m.amoand_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amoor_w()
    {
        RunTest(
        // 01000 aq rl rs2 rs1 010 rd 0101111 amoor.w
        0b01000_1_1_10101_10011_010_10001_0101111u,
        m => m.amoor_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amomin_w()
    {
        RunTest(
        // 10000 aq rl rs2 rs1 010 rd 0101111 amomin.w
        0b10000_1_1_10101_10011_010_10001_0101111u,
        m => m.amomin_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amomax_w()
    {
        RunTest(
        // 10100 aq rl rs2 rs1 010 rd 0101111 amomax.w
        0b10100_1_1_10101_10011_010_10001_0101111u,
        m => m.amomax_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amominu_w()
    {
        RunTest(
        // 11000 aq rl rs2 rs1 010 rd 0101111 amominu.w
        0b11000_1_1_10101_10011_010_10001_0101111u,
        m => m.amominu_w(op1, op2, op3));
    }


    [Test]
    public void RiscvAsm_amomaxu_w()
    {
        RunTest(
        // 11100 aq rl rs2 rs1 010 rd 0101111 amomaxu.w
        0b11100_1_1_10101_10011_010_10001_0101111u,
        m => m.amomaxu_w(op1, op2, op3));
    }


[Test]
public void RiscvAsm_lr_d()
{
    RunTest(
    // 00010 aq rl 00000 rs1 011 rd 0101111 lr.d
    0b00010_1_1_00000_10011_011_10001_0101111u,
    m => m.lr_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_sc_d()
{
    RunTest(
    // 00011 aq rl rs2 rs1 011 rd 0101111 sc.d
    0b00011_1_1_10101_10011_011_10001_0101111u,
    m => m.sc_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amoswap_d()
{
    RunTest(
    // 00001 aq rl rs2 rs1 011 rd 0101111 amoswap.d
    0b00001_1_1_10101_10011_011_10001_0101111u,
    m => m.amoswap_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amoadd_d()
{
    RunTest(
    // 00000 aq rl rs2 rs1 011 rd 0101111 amoadd.d
    0b00000_1_1_10101_10011_011_10001_0101111u,
    m => m.amoadd_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amoxor_d()
{
    RunTest(
    // 00100 aq rl rs2 rs1 011 rd 0101111 amoxor.d
    0b00100_1_1_10101_10011_011_10001_0101111u,
    m => m.amoxor_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amoand_d()
{
    RunTest(
    // 01100 aq rl rs2 rs1 011 rd 0101111 amoand.d
    0b01100_1_1_10101_10011_011_10001_0101111u,
    m => m.amoand_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amoor_d()
{
    RunTest(
    // 01000 aq rl rs2 rs1 011 rd 0101111 amoor.d
    0b01000_1_1_10101_10011_011_10001_0101111u,
    m => m.amoor_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amomin_d()
{
    RunTest(
    // 10000 aq rl rs2 rs1 011 rd 0101111 amomin.d
    0b10000_1_1_10101_10011_011_10001_0101111u,
    m => m.amomin_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amomax_d()
{
    RunTest(
    // 10100 aq rl rs2 rs1 011 rd 0101111 amomax.d
    0b10100_1_1_10101_10011_011_10001_0101111u,
    m => m.amomax_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amominu_d()
{
    RunTest(
    // 11000 aq rl rs2 rs1 011 rd 0101111 amominu.d
    0b11000_1_1_10101_10011_011_10001_0101111u,
    m => m.amominu_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_amomaxu_d()
{
    RunTest(
    // 11100 aq rl rs2 rs1 011 rd 0101111 amomaxu.d
    0b11100_1_1_10101_10011_011_10001_0101111u,
    m => m.amomaxu_d(op1, op2, op3));
}
*/

#endregion
#endif


#region RV32F
//rv32f standard/extension/
[Test]
public void RiscvAsm_flw()
{
    RunTest(
    // imm[11:0] rs1 010 rd 0000111 flw
    0b111111111110_10011_010_10001_0000111u,
    m => m.flw(op1, op2, -2));
}


[Test]
public void RiscvAsm_fsw()
{
    RunTest(
    // imm[11:5] rs2 rs1 010 imm[4:0] 0100111 fsw
    0b1010101_10001_10011_010_01010_0100111u,
    m => m.fsw(op1, op2, -1366));
}


[Test]
public void RiscvAsm_fmadd_s()
{
    RunTest(
    // rs3 00 rs2 rs1 rm rd 1000011 fmadd.s
    0b10111_00_10101_10011_010_10001_1000011u,
    m => m.fmadd_s(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fmsub_s()
{
    RunTest(
    // rs3 00 rs2 rs1 rm rd 1000111 fmsub.s
    0b10111_00_10101_10011_010_10001_1000111u,
    m => m.fmsub_s(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmsub_s()
{
    RunTest(
    // rs3 00 rs2 rs1 rm rd 1001011 fnmsub.s
    0b10111_00_10101_10011_010_10001_1001011u,
    m => m.fnmsub_s(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmadd_s()
{
    RunTest(
    // rs3 00 rs2 rs1 rm rd 1001111 fnmadd.s
    0b10111_00_10101_10011_010_10001_1001111u,
    m => m.fnmadd_s(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fadd_s()
{
    RunTest(
    // 0000000 rs2 rs1 rm rd 1010011 fadd.s
    0b0000000_10101_10011_010_10001_1010011u,
    m => m.fadd_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsub_s()
{
    RunTest(
    // 0000100 rs2 rs1 rm rd 1010011 fsub.s
    0b0000100_10101_10011_010_10001_1010011u,
    m => m.fsub_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fmul_s()
{
    RunTest(
    // 0001000 rs2 rs1 rm rd 1010011 fmul.s
    0b0001000_10101_10011_010_10001_1010011u,
    m => m.fmul_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fdiv_s()
{
    RunTest(
    // 0001100 rs2 rs1 rm rd 1010011 fdiv.s
    0b0001100_10101_10011_010_10001_1010011u,
    m => m.fdiv_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsqrt_s()
{
    RunTest(
    // 0101100 00000 rs1 rm rd 1010011 fsqrt.s
    0b0101100_00000_10011_010_10001_1010011u,
    m => m.fsqrt_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsgnj_s()
{
    RunTest(
    // 0010000 rs2 rs1 000 rd 1010011 fsgnj.s
    0b0010000_10101_10011_000_10001_1010011u,
    m => m.fsgnj_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjn_s()
{
    RunTest(
    // 0010000 rs2 rs1 001 rd 1010011 fsgnjn.s
    0b0010000_10101_10011_001_10001_1010011u,
    m => m.fsgnjn_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjx_s()
{
    RunTest(
    // 0010000 rs2 rs1 010 rd 1010011 fsgnjx.s
    0b0010000_10101_10011_010_10001_1010011u,
    m => m.fsgnjx_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmin_s()
{
    RunTest(
    // 0010100 rs2 rs1 000 rd 1010011 fmin.s
    0b0010100_10101_10011_000_10001_1010011u,
    m => m.fmin_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmax_s()
{
    RunTest(
    // 0010100 rs2 rs1 001 rd 1010011 fmax.s
    0b0010100_10101_10011_001_10001_1010011u,
    m => m.fmax_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_w_s()
{
    RunTest(
    // 1100000 00000 rs1 rm rd 1010011 fcvt.w.s
    0b1100000_00000_10011_010_10001_1010011u,
    m => m.fcvt_w_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_wu_s()
{
    RunTest(
    // 1100000 00001 rs1 rm rd 1010011 fcvt.wu.s
    0b1100000_00001_10011_010_10001_1010011u,
    m => m.fcvt_wu_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fmv_x_w()
{
    RunTest(
    // 1110000 00000 rs1 000 rd 1010011 fmv.x.w
    0b1110000_00000_10011_000_10001_1010011u,
    m => m.fmv_x_w(op1, op2, op3));
}


[Test]
public void RiscvAsm_feq_s()
{
    RunTest(
    // 1010000 rs2 rs1 010 rd 1010011 feq.s
    0b1010000_10101_10011_010_10001_1010011u,
    m => m.feq_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_flt_s()
{
    RunTest(
    // 1010000 rs2 rs1 001 rd 1010011 flt.s
    0b1010000_10101_10011_001_10001_1010011u,
    m => m.flt_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fle_s()
{
    RunTest(
    // 1010000 rs2 rs1 000 rd 1010011 fle.s
    0b1010000_10101_10011_000_10001_1010011u,
    m => m.fle_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fclass_s()
{
    RunTest(
    // 1110000 00000 rs1 001 rd 1010011 fclass.s
    0b1110000_00000_10011_001_10001_1010011u,
    m => m.fclass_s(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_s_w()
{
    RunTest(
    // 1101000 00000 rs1 rm rd 1010011 fcvt.s.w
    0b1101000_00000_10011_010_10001_1010011u,
    m => m.fcvt_s_w(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_s_wu()
{
    RunTest(
    // 1101000 00001 rs1 rm rd 1010011 fcvt.s.wu
    0b1101000_00001_10011_010_10001_1010011u,
    m => m.fcvt_s_wu(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fmv_w_x()
{
    RunTest(
    // 1111000 00000 rs1 000 rd 1010011 fmv.w.x
    0b1111000_00000_10011_000_10001_1010011u,
    m => m.fmv_w_x(op1, op2, op3));
}


// rv64f standard extension (in addition to/rv32f)/
[Test]
public void RiscvAsm_fcvt_l_s()
{
    RunTest(
    // 1100000 00010 rs1 rm rd 1010011 fcvt.l.s
    0b1100000_00010_10011_010_10001_1010011u,
    m => m.fcvt_l_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_lu_s()
{
    RunTest(
    // 1100000 00011 rs1 rm rd 1010011 fcvt.lu.s
    0b1100000_00011_10011_010_10001_1010011u,
    m => m.fcvt_lu_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_s_l()
{
    RunTest(
    // 1101000 00010 rs1 rm rd 1010011 fcvt.s.l
    0b1101000_00010_10011_010_10001_1010011u,
    m => m.fcvt_s_l(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_s_lu()
{
    RunTest(
    // 1101000 00011 rs1 rm rd 1010011 fcvt.s.lu
    0b1101000_00011_10011_010_10001_1010011u,
    m => m.fcvt_s_lu(op1, op2, op3, rm:0b010));
}

#endregion


//rv32d standard/extension/

#region RV32D
[Test]
public void RiscvAsm_fld()
{
    RunTest(
    // imm[11:0] rs1 011 rd 0000111 fld
    0b111111111110_10011_011_10001_0000111u,
    m => m.fld(op1, op2, -2));
}


[Test]
public void RiscvAsm_fsd()
{
    RunTest(
    // imm[11:5] rs2 rs1 011 imm[4:0] 0100111 fsd
    0b1010101_10001_10011_011_01010_0100111u,
    m => m.fsd(op1, op2, -1366));
}


[Test]
public void RiscvAsm_fmadd_d()
{
    RunTest(
    // rs3 01 rs2 rs1 rm rd 1000011 fmadd.d
    0b10111_01_10101_10011_010_10001_1000011u,
    m => m.fmadd_d(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fmsub_d()
{
    RunTest(
    // rs3 01 rs2 rs1 rm rd 1000111 fmsub.d
    0b10111_01_10101_10011_010_10001_1000111u,
    m => m.fmsub_d(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmsub_d()
{
    RunTest(
    // rs3 01 rs2 rs1 rm rd 1001011 fnmsub.d
    0b10111_01_10101_10011_010_10001_1001011u,
    m => m.fnmsub_d(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmadd_d()
{
    RunTest(
    // rs3 01 rs2 rs1 rm rd 1001111 fnmadd.d
    0b10111_01_10101_10011_010_10001_1001111u,
    m => m.fnmadd_d(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fadd_d()
{
    RunTest(
    // 0000001 rs2 rs1 rm rd 1010011 fadd.d
    0b0000001_10101_10011_010_10001_1010011u,
    m => m.fadd_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsub_d()
{
    RunTest(
    // 0000101 rs2 rs1 rm rd 1010011 fsub.d
    0b0000101_10101_10011_010_10001_1010011u,
    m => m.fsub_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fmul_d()
{
    RunTest(
    // 0001001 rs2 rs1 rm rd 1010011 fmul.d
    0b0001001_10101_10011_010_10001_1010011u,
    m => m.fmul_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fdiv_d()
{
    RunTest(
    // 0001101 rs2 rs1 rm rd 1010011 fdiv.d
    0b0001101_10101_10011_010_10001_1010011u,
    m => m.fdiv_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsqrt_d()
{
    RunTest(
    // 0101101 00000 rs1 rm rd 1010011 fsqrt.d
    0b0101101_00000_10011_010_10001_1010011u,
    m => m.fsqrt_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsgnj_d()
{
    RunTest(
    // 0010001 rs2 rs1 000 rd 1010011 fsgnj.d
    0b0010001_10101_10011_000_10001_1010011u,
    m => m.fsgnj_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjn_d()
{
    RunTest(
    // 0010001 rs2 rs1 001 rd 1010011 fsgnjn.d
    0b0010001_10101_10011_001_10001_1010011u,
    m => m.fsgnjn_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjx_d()
{
    RunTest(
    // 0010001 rs2 rs1 010 rd 1010011 fsgnjx.d
    0b0010001_10101_10011_010_10001_1010011u,
    m => m.fsgnjx_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmin_d()
{
    RunTest(
    // 0010101 rs2 rs1 000 rd 1010011 fmin.d
    0b0010101_10101_10011_000_10001_1010011u,
    m => m.fmin_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmax_d()
{
    RunTest(
    // 0010101 rs2 rs1 001 rd 1010011 fmax.d
    0b0010101_10101_10011_001_10001_1010011u,
    m => m.fmax_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_s_d()
{
    RunTest(
    // 0100000 00001 rs1 rm rd 1010011 fcvt.s.d
    0b0100000_00001_10011_010_10001_1010011u,
    m => m.fcvt_s_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_d_s()
{
    RunTest(
    // 0100001 00000 rs1 rm rd 1010011 fcvt.d.s
    0b0100001_00000_10011_010_10001_1010011u,
    m => m.fcvt_d_s(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_feq_d()
{
    RunTest(
    // 1010001 rs2 rs1 010 rd 1010011 feq.d
    0b1010001_10101_10011_010_10001_1010011u,
    m => m.feq_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_flt_d()
{
    RunTest(
    // 1010001 rs2 rs1 001 rd 1010011 flt.d
    0b1010001_10101_10011_001_10001_1010011u,
    m => m.flt_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fle_d()
{
    RunTest(
    // 1010001 rs2 rs1 000 rd 1010011 fle.d
    0b1010001_10101_10011_000_10001_1010011u,
    m => m.fle_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fclass_d()
{
    RunTest(
    // 1110001 00000 rs1 001 rd 1010011 fclass.d
    0b1110001_00000_10011_001_10001_1010011u,
    m => m.fclass_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_w_d()
{
    RunTest(
    // 1100001 00000 rs1 rm rd 1010011 fcvt.w.d
    0b1100001_00000_10011_010_10001_1010011u,
    m => m.fcvt_w_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_wu_d()
{
    RunTest(
    // 1100001 00001 rs1 rm rd 1010011 fcvt.wu.d
    0b1100001_00001_10011_010_10001_1010011u,
    m => m.fcvt_wu_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_d_w()
{
    RunTest(
    // 1101001 00000 rs1 rm rd 1010011 fcvt.d.w
    0b1101001_00000_10011_010_10001_1010011u,
    m => m.fcvt_d_w(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_d_wu()
{
    RunTest(
    // 1101001 00001 rs1 rm rd 1010011 fcvt.d.wu
    0b1101001_00001_10011_010_10001_1010011u,
    m => m.fcvt_d_wu(op1, op2, op3, rm:0b010));
}



//rv64d standard extension (in addition to/rv32d)/
[Test]
public void RiscvAsm_fcvt_l_d()
{
    RunTest(
    // 1100001 00010 rs1 rm rd 1010011 fcvt.l.d
    0b1100001_00010_10011_010_10001_1010011u,
    m => m.fcvt_l_d(op1, op2, 0,rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_lu_d()
{
    RunTest(
    // 1100001 00011 rs1 rm rd 1010011 fcvt.lu.d
    0b1100001_00011_10011_010_10001_1010011u,
    m => m.fcvt_lu_d(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fmv_x_d()
{
    RunTest(
    // 1110001 00000 rs1 000 rd 1010011 fmv.x.d
    0b1110001_00000_10011_000_10001_1010011u,
    m => m.fmv_x_d(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_d_l()
{
    RunTest(
    // 1101001 00010 rs1 rm rd 1010011 fcvt.d.l
    0b1101001_00010_10011_010_10001_1010011u,
    m => m.fcvt_d_l(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_d_lu()
{
    RunTest(
    // 1101001 00011 rs1 rm rd 1010011 fcvt.d.lu
    0b1101001_00011_10011_010_10001_1010011u,
    m => m.fcvt_d_lu(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fmv_d_x()
{
    RunTest(
    // 1111001 00000 rs1 000 rd 1010011 fmv.d.x
    0b1111001_00000_10011_000_10001_1010011u,
    m => m.fmv_d_x(op1, op2, op3));
}

#endregion

//rv32q standard/extension/

#region RV32Q
[Test]
public void RiscvAsm_flq()
{
    RunTest(
    // imm[11:0] rs1 100 rd 0000111 flq
    0b111111111110_10011_100_10001_0000111u,
    m => m.flq(op1, op2, -2));
}


[Test]
public void RiscvAsm_fsq()
{
    RunTest(
    // imm[11:5] rs2 rs1 100 imm[4:0] 0100111 fsq
    0b1010101_10001_10011_100_01010_0100111u,
    m => m.fsq(op1, op2, -1366));
}


[Test]
public void RiscvAsm_fmadd_q()
{
    RunTest(
    // rs3 11 rs2 rs1 rm rd 1000011 fmadd.q
    0b10111_11_10101_10011_010_10001_1000011u,
    m => m.fmadd_q(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fmsub_q()
{
    RunTest(
    // rs3 11 rs2 rs1 rm rd 1000111 fmsub.q
    0b10111_11_10101_10011_010_10001_1000111u,
    m => m.fmsub_q(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmsub_q()
{
    RunTest(
    // rs3 11 rs2 rs1 rm rd 1001011 fnmsub.q
    0b10111_11_10101_10011_010_10001_1001011u,
    m => m.fnmsub_q(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmadd_q()
{
    RunTest(
    // rs3 11 rs2 rs1 rm rd 1001111 fnmadd.q
    0b10111_11_10101_10011_010_10001_1001111u,
    m => m.fnmadd_q(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fadd_q()
{
    RunTest(
    // 0000011 rs2 rs1 rm rd 1010011 fadd.q
    0b0000011_10101_10011_010_10001_1010011u,
    m => m.fadd_q(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsub_q()
{
    RunTest(
    // 0000111 rs2 rs1 rm rd 1010011 fsub.q
    0b0000111_10101_10011_010_10001_1010011u,
    m => m.fsub_q(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fmul_q()
{
    RunTest(
    // 0001011 rs2 rs1 rm rd 1010011 fmul.q
    0b0001011_10101_10011_010_10001_1010011u,
    m => m.fmul_q(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fdiv_q()
{
    RunTest(
    // 0001111 rs2 rs1 rm rd 1010011 fdiv.q
    0b0001111_10101_10011_010_10001_1010011u,
    m => m.fdiv_q(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsqrt_q()
{
    RunTest(
    // 0101111 00000 rs1 rm rd 1010011 fsqrt.q
    0b0101111_00000_10011_010_10001_1010011u,
    m => m.fsqrt_q(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fsgnj_q()
{
    RunTest(
    // 0010011 rs2 rs1 000 rd 1010011 fsgnj.q
    0b0010011_10101_10011_000_10001_1010011u,
    m => m.fsgnj_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjn_q()
{
    RunTest(
    // 0010011 rs2 rs1 001 rd 1010011 fsgnjn.q
    0b0010011_10101_10011_001_10001_1010011u,
    m => m.fsgnjn_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjx_q()
{
    RunTest(
    // 0010011 rs2 rs1 010 rd 1010011 fsgnjx.q
    0b0010011_10101_10011_010_10001_1010011u,
    m => m.fsgnjx_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmin_q()
{
    RunTest(
    // 0010111 rs2 rs1 000 rd 1010011 fmin.q
    0b0010111_10101_10011_000_10001_1010011u,
    m => m.fmin_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmax_q()
{
    RunTest(
    // 0010111 rs2 rs1 001 rd 1010011 fmax.q
    0b0010111_10101_10011_001_10001_1010011u,
    m => m.fmax_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_s_q()
{
    RunTest(
    // 0100000 00011 rs1 rm rd 1010011 fcvt.s.q
    0b0100000_00011_10011_010_10001_1010011u,
    m => m.fcvt_s_q(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_q_s()
{
    RunTest(
    // 0100011 00000 rs1 rm rd 1010011 fcvt.q.s
    0b0100011_00000_10011_010_10001_1010011u,
    m => m.fcvt_q_s(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_d_q()
{
    RunTest(
    // 0100001 00011 rs1 rm rd 1010011 fcvt.d.q
    0b0100001_00011_10011_010_10001_1010011u,
    m => m.fcvt_d_q(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_q_d()
{
    RunTest(
    // 0100011 00001 rs1 rm rd 1010011 fcvt.q.d
    0b0100011_00001_10011_010_10001_1010011u,
    m => m.fcvt_q_d(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_feq_q()
{
    RunTest(
    // 1010011 rs2 rs1 010 rd 1010011 feq.q
    0b1010011_10101_10011_010_10001_1010011u,
    m => m.feq_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_flt_q()
{
    RunTest(
    // 1010011 rs2 rs1 001 rd 1010011 flt.q
    0b1010011_10101_10011_001_10001_1010011u,
    m => m.flt_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fle_q()
{
    RunTest(
    // 1010011 rs2 rs1 000 rd 1010011 fle.q
    0b1010011_10101_10011_000_10001_1010011u,
    m => m.fle_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fclass_q()
{
    RunTest(
    // 1110011 00000 rs1 001 rd 1010011 fclass.q
    0b1110011_00000_10011_001_10001_1010011u,
    m => m.fclass_q(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_w_q()
{
    RunTest(
    // 1100011 00000 rs1 rm rd 1010011 fcvt.w.q
    0b1100011_00000_10011_010_10001_1010011u,
    m => m.fcvt_w_q(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_wu_q()
{
    RunTest(
    // 1100011 00001 rs1 rm rd 1010011 fcvt.wu.q
    0b1100011_00001_10011_010_10001_1010011u,
    m => m.fcvt_wu_q(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_q_w()
{
    RunTest(
    // 1101011 00000 rs1 rm rd 1010011 fcvt.q.w
    0b1101011_00000_10011_010_10001_1010011u,
    m => m.fcvt_q_w(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_q_wu()
{
    RunTest(
    // 1101011 00001 rs1 rm rd 1010011 fcvt.q.wu
    0b1101011_00001_10011_010_10001_1010011u,
    m => m.fcvt_q_wu(op1, op2, 0, rm:0b010));
}



//rv64q standard extension (in addition to/rv32q)/
[Test]
public void RiscvAsm_fcvt_l_q()
{
    RunTest(
    // 1100011 00010 rs1 rm rd 1010011 fcvt.l.q
    0b1100011_00010_10011_010_10001_1010011u,
    m => m.fcvt_l_q(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_lu_q()
{
    RunTest(
    // 1100011 00011 rs1 rm rd 1010011 fcvt.lu.q
    0b1100011_00011_10011_010_10001_1010011u,
    m => m.fcvt_lu_q(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_q_l()
{
    RunTest(
    // 1101011 00010 rs1 rm rd 1010011 fcvt.q.l
    0b1101011_00010_10011_010_10001_1010011u,
    m => m.fcvt_q_l(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_q_lu()
{
    RunTest(
    // 1101011 00011 rs1 rm rd 1010011 fcvt.q.lu
    0b1101011_00011_10011_010_10001_1010011u,
    m => m.fcvt_q_lu(op1, op2, 0, rm:0b010));
}

#endregion

//rv32zfh standard/extension/

#region RV32zfh
[Test]
public void RiscvAsm_flh()
{
    RunTest(
    // imm[11:0] rs1 001 rd 0000111 flh
    0b111111111110_10011_001_10001_0000111u,
    m => m.flh(op1, op2, -2));
}


[Test]
public void RiscvAsm_fsh()
{
    RunTest(
    // imm[11:5] rs2 rs1 001 imm[4:0] 0100111 fsh
    0b1010101_10001_10011_001_01010_0100111u,
    m => m.fsh(op1, op2, -1366));
}


[Test]
public void RiscvAsm_fmadd_h()
{
    RunTest(
    // rs3 10 rs2 rs1 rm rd 1000011 fmadd.h
    0b10111_10_10101_10011_010_10001_1000011u,
    m => m.fmadd_h(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fmsub_h()
{
    RunTest(
    // rs3 10 rs2 rs1 rm rd 1000111 fmsub.h
    0b10111_10_10101_10011_010_10001_1000111u,
    m => m.fmsub_h(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmsub_h()
{
    RunTest(
    // rs3 10 rs2 rs1 rm rd 1001011 fnmsub.h
    0b10111_10_10101_10011_010_10001_1001011u,
    m => m.fnmsub_h(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fnmadd_h()
{
    RunTest(
    // rs3 10 rs2 rs1 rm rd 1001111 fnmadd.h
    0b10111_10_10101_10011_010_10001_1001111u,
    m => m.fnmadd_h(op1, op2, op3, op4, rm:0b010));
}


[Test]
public void RiscvAsm_fadd_h()
{
    RunTest(
    // 0000010 rs2 rs1 rm rd 1010011 fadd.h
    0b0000010_10101_10011_010_10001_1010011u,
    m => m.fadd_h(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsub_h()
{
    RunTest(
    // 0000110 rs2 rs1 rm rd 1010011 fsub.h
    0b0000110_10101_10011_010_10001_1010011u,
    m => m.fsub_h(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fmul_h()
{
    RunTest(
    // 0001010 rs2 rs1 rm rd 1010011 fmul.h
    0b0001010_10101_10011_010_10001_1010011u,
    m => m.fmul_h(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fdiv_h()
{
    RunTest(
    // 0001110 rs2 rs1 rm rd 1010011 fdiv.h
    0b0001110_10101_10011_010_10001_1010011u,
    m => m.fdiv_h(op1, op2, op3, rm:0b010));
}


[Test]
public void RiscvAsm_fsqrt_h()
{
    RunTest(
    // 0101110 00000 rs1 rm rd 1010011 fsqrt.h
    0b0101110_00000_10011_010_10001_1010011u,
    m => m.fsqrt_h(op1, op2, 0b010));
}


[Test]
public void RiscvAsm_fsgnj_h()
{
    RunTest(
    // 0010010 rs2 rs1 000 rd 1010011 fsgnj.h
    0b0010010_10101_10011_000_10001_1010011u,
    m => m.fsgnj_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjn_h()
{
    RunTest(
    // 0010010 rs2 rs1 001 rd 1010011 fsgnjn.h
    0b0010010_10101_10011_001_10001_1010011u,
    m => m.fsgnjn_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fsgnjx_h()
{
    RunTest(
    // 0010010 rs2 rs1 010 rd 1010011 fsgnjx.h
    0b0010010_10101_10011_010_10001_1010011u,
    m => m.fsgnjx_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmin_h()
{
    RunTest(
    // 0010110 rs2 rs1 000 rd 1010011 fmin.h
    0b0010110_10101_10011_000_10001_1010011u,
    m => m.fmin_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fmax_h()
{
    RunTest(
    // 0010110 rs2 rs1 001 rd 1010011 fmax.h
    0b0010110_10101_10011_001_10001_1010011u,
    m => m.fmax_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_s_h()
{
    RunTest(
    // 0100000 00010 rs1 rm rd 1010011 fcvt.s.h
    0b0100000_00010_10011_010_10001_1010011u,
    m => m.fcvt_s_h(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_h_s()
{
    RunTest(
    // 0100010 00000 rs1 rm rd 1010011 fcvt.h.s
    0b0100010_00000_10011_010_10001_1010011u,
    m => m.fcvt_h_s(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_d_h()
{
    RunTest(
    // 0100001 00010 rs1 rm rd 1010011 fcvt.d.h
    0b0100001_00010_10011_010_10001_1010011u,
    m => m.fcvt_d_h(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_h_d()
{
    RunTest(
    // 0100010 00001 rs1 rm rd 1010011 fcvt.h.d
    0b0100010_00001_10011_010_10001_1010011u,
    m => m.fcvt_h_d(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_q_h()
{
    RunTest(
    // 0100011 00010 rs1 rm rd 1010011 fcvt.q.h
    0b0100011_00010_10011_010_10001_1010011u,
    m => m.fcvt_q_h(op1, op2, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_h_q()
{
    RunTest(
    // 0100010 00011 rs1 rm rd 1010011 fcvt.h.q
    0b0100010_00011_10011_010_10001_1010011u,
    m => m.fcvt_h_q(op1, op2, rm:0b010));
}


[Test]
public void RiscvAsm_feq_h()
{
    RunTest(
    // 1010010 rs2 rs1 010 rd 1010011 feq.h
    0b1010010_10101_10011_010_10001_1010011u,
    m => m.feq_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_flt_h()
{
    RunTest(
    // 1010010 rs2 rs1 001 rd 1010011 flt.h
    0b1010010_10101_10011_001_10001_1010011u,
    m => m.flt_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fle_h()
{
    RunTest(
    // 1010010 rs2 rs1 000 rd 1010011 fle.h
    0b1010010_10101_10011_000_10001_1010011u,
    m => m.fle_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fclass_h()
{
    RunTest(
    // 1110010 00000 rs1 001 rd 1010011 fclass.h
    0b1110010_00000_10011_001_10001_1010011u,
    m => m.fclass_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_w_h()
{
    RunTest(
    // 1100010 00000 rs1 rm rd 1010011 fcvt.w.h
    0b1100010_00000_10011_010_10001_1010011u,
    m => m.fcvt_w_h(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_wu_h()
{
    RunTest(
    // 1100010 00001 rs1 rm rd 1010011 fcvt.wu.h
    0b1100010_00001_10011_010_10001_1010011u,
    m => m.fcvt_wu_h(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fmv_x_h()
{
    RunTest(
    // 1110010 00000 rs1 000 rd 1010011 fmv.x.h
    0b1110010_00000_10011_000_10001_1010011u,
    m => m.fmv_x_h(op1, op2, op3));
}


[Test]
public void RiscvAsm_fcvt_h_w()
{
    RunTest(
    // 1101010 00000 rs1 rm rd 1010011 fcvt.h.w
    0b1101010_00000_10011_010_10001_1010011u,
    m => m.fcvt_h_w(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_h_wu()
{
    RunTest(
    // 1101010 00001 rs1 rm rd 1010011 fcvt.h.wu
    0b1101010_00001_10011_010_10001_1010011u,
    m => m.fcvt_h_wu(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fmv_h_x()
{
    RunTest(
    // 1111010 00000 rs1 000 rd 1010011 fmv.h.x
    0b1111010_00000_10011_000_10001_1010011u,
    m => m.fmv_h_x(op1, op2, op3));
}



//rv64zfh standard extension (in addition to/rv32zfh)
[Test]
public void RiscvAsm_fcvt_l_h()
{
    RunTest(
    // 1100010 00010 rs1 rm rd 1010011 fcvt.l.h
    0b1100010_00010_10011_010_10001_1010011u,
    m => m.fcvt_l_h(op1, op2, 0, rm:0b010));
}



//rv64zfh standard extension (in addition to/rv32zfh)/
[Test]
public void RiscvAsm_fcvt_lu_h()
{
    RunTest(
    // 1100010 00011 rs1 rm rd 1010011 fcvt.lu.h
    0b1100010_00011_10011_010_10001_1010011u,
    m => m.fcvt_lu_h(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_h_l()
{
    RunTest(
    // 1101010 00010 rs1 rm rd 1010011 fcvt.h.l
    0b1101010_00010_10011_010_10001_1010011u,
    m => m.fcvt_h_l(op1, op2, 0, rm:0b010));
}


[Test]
public void RiscvAsm_fcvt_h_lu()
{
    RunTest(
    // 1101010 00011 rs1 rm rd 1010011 fcvt.h.lu
    0b1101010_00011_10011_010_10001_1010011u,
    m => m.fcvt_h_lu(op1, op2, 0, rm:0b010));
}

#endregion

//zawrs standard/extension/
#region zawrs
[Test]
public void RiscvAsm_wrs_nto()
{
    RunTest(
    // 000000001101 00000 000 00000 1110011 wrs.nto
    0b000000001101_00000_000_00000_1110011u,
    m => m.wrs_nto());
}


[Test]
public void RiscvAsm_wrs_sto()
{
    RunTest(
    // 000000011101 00000 000 00000 1110011 wrs.sto
    0b000000011101_00000_000_00000_1110011u,
    m => m.wrs_sto());
}

#endregion

}