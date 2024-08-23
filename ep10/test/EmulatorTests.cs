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

/*
/imm[31:12] rd 0110111/lui/
/imm[31:12] rd 0010111/auipc/
/imm[20|10:1|11|19:12] rd 1101111/jal/
/imm[11:0] rs1 000 rd 1100111/jalr/
/imm[12|10:5] rs2 rs1 000 imm[4:1|11] 1100011/beq/
/imm[12|10:5] rs2 rs1 001 imm[4:1|11] 1100011/bne/
/imm[12|10:5] rs2 rs1 100 imm[4:1|11] 1100011/blt/
/imm[12|10:5] rs2 rs1 101 imm[4:1|11] 1100011/bge/
/imm[12|10:5] rs2 rs1 110 imm[4:1|11] 1100011/bltu/
/imm[12|10:5] rs2 rs1 111 imm[4:1|11] 1100011/bgeu/
/imm[11:0] rs1 000 rd 0000011/lb/
/imm[11:0] rs1 001 rd 0000011/lh/
/imm[11:0] rs1 010 rd 0000011/lw/
/imm[11:0] rs1 100 rd 0000011/lbu/
/imm[11:0] rs1 101 rd 0000011/lhu/
/imm[11:5] rs2 rs1 000 imm[4:0] 0100011/sb/
/imm[11:5] rs2 rs1 001 imm[4:0] 0100011/sh/
/imm[11:5] rs2 rs1 010 imm[4:0] 0100011/sw/
/imm[11:0] rs1 000 rd 0010011/addi/
/imm[11:0] rs1 010 rd 0010011/slti/
/imm[11:0] rs1 011 rd 0010011/sltiu/
/imm[11:0] rs1 100 rd 0010011/xori/
/imm[11:0] rs1 110 rd 0010011/ori/
/imm[11:0] rs1 111 rd 0010011/andi/
/0000000 shamt rs1 001 rd 0010011/slli/
/0000000 shamt rs1 101 rd 0010011/srli/
/0100000 shamt rs1 101 rd 0010011/srai/
/0000000 rs2 rs1 000 rd 0110011/add/
/0100000 rs2 rs1 000 rd 0110011/sub/
/0000000 rs2 rs1 001 rd 0110011/sll/
/0000000 rs2 rs1 010 rd 0110011/slt/
/0000000 rs2 rs1 011 rd 0110011/sltu/
/0000000 rs2 rs1 100 rd 0110011/xor/
/0000000 rs2 rs1 101 rd 0110011/srl/
/0100000 rs2 rs1 101 rd 0110011/sra/
/0000000 rs2 rs1 110 rd 0110011/or/
/0000000 rs2 rs1 111 rd 0110011/and/
/1000 0011 0011 00000 000 00000 0001111/fence.tso/
/0000 0001 0000 00000 000 00000 0001111/pause/
/000000000000 00000 000 00000 1110011/ecall/
/000000000001 00000 000 00000 1110011/ebreak/

/imm[11:0] rs1 110 rd 0000011/lwu/
/imm[11:0] rs1 011 rd 0000011/ld/
/imm[11:5] rs2 rs1 011 imm[4:0] 0100011/sd/
/000000 shamt rs1 001 rd 0010011/slli/
/000000 shamt rs1 101 rd 0010011/srli/
/010000 shamt rs1 101 rd 0010011/srai/
/imm[11:0] rs1 000 rd 0011011/addiw/
/0000000 shamt rs1 001 rd 0011011/slliw/
/0000000 shamt rs1 101 rd 0011011/srliw/
/0100000 shamt rs1 101 rd 0011011/sraiw/
/0000000 rs2 rs1 000 rd 0111011/addw/
/0100000 rs2 rs1 000 rd 0111011/subw/
/0000000 rs2 rs1 001 rd 0111011/sllw/
/0000000 rs2 rs1 101 rd 0111011/srlw/
/0100000 rs2 rs1 101 rd 0111011/sraw/

//rv32/rv64 zifencei standard/extension
/imm[11:0] rs1 001 rd 0001111/fence.i/

// rv32/rv64 zicsr standard/extension
/csr rs1 001 rd 1110011/csrrw/
/csr rs1 010 rd 1110011/csrrs/
/csr rs1 011 rd 1110011/csrrc/
/csr uimm 101 rd 1110011/csrrwi/
/csr uimm 110 rd 1110011/csrrsi/
/csr uimm 111 rd 1110011/csrrci/

// rv32m standard/extension
/0000001 rs2 rs1 000 rd 0110011/mul/
/0000001 rs2 rs1 001 rd 0110011/mulh/
/0000001 rs2 rs1 010 rd 0110011/mulhsu/
/0000001 rs2 rs1 011 rd 0110011/mulhu/
/0000001 rs2 rs1 100 rd 0110011/div/
/0000001 rs2 rs1 101 rd 0110011/divu/
/0000001 rs2 rs1 110 rd 0110011/rem/
/0000001 rs2 rs1 111 rd 0110011/remu/

// rv64m standard extension (in addition to/rv32m)
/0000001 rs2 rs1 000 rd 0111011/mulw/
/0000001 rs2 rs1 100 rd 0111011/divw/
/0000001 rs2 rs1 101 rd 0111011/divuw/
/0000001 rs2 rs1 110 rd 0111011/remw/
/0000001 rs2 rs1 111 rd 0111011/remuw/

//rv32a standard/extension
/00010 aq rl 00000 rs1 010 rd 0101111/lr.w/
/00011 aq rl rs2 rs1 010 rd 0101111/sc.w/
/00001 aq rl rs2 rs1 010 rd 0101111/amoswap.w/
/00000 aq rl rs2 rs1 010 rd 0101111/amoadd.w/
/00100 aq rl rs2 rs1 010 rd 0101111/amoxor.w/
/01100 aq rl rs2 rs1 010 rd 0101111/amoand.w/
/01000 aq rl rs2 rs1 010 rd 0101111/amoor.w/
/10000 aq rl rs2 rs1 010 rd 0101111/amomin.w/
/10100 aq rl rs2 rs1 010 rd 0101111/amomax.w/
/11000 aq rl rs2 rs1 010 rd 0101111/amominu.w/
/11100 aq rl rs2 rs1 010 rd 0101111/amomaxu.w/
/rv64a standard extension (in addition to/rv32a)/
/00010 aq rl 00000 rs1 011 rd 0101111/lr.d/
/00011 aq rl rs2 rs1 011 rd 0101111/sc.d/
/00001 aq rl rs2 rs1 011 rd 0101111/amoswap.d/
/00000 aq rl rs2 rs1 011 rd 0101111/amoadd.d/
/00100 aq rl rs2 rs1 011 rd 0101111/amoxor.d/
/01100 aq rl rs2 rs1 011 rd 0101111/amoand.d/
/01000 aq rl rs2 rs1 011 rd 0101111/amoor.d/
/10000 aq rl rs2 rs1 011 rd 0101111/amomin.d/
/10100 aq rl rs2 rs1 011 rd 0101111/amomax.d/
/11000 aq rl rs2 rs1 011 rd 0101111/amominu.d/
/11100 aq rl rs2 rs1 011 rd 0101111/amomaxu.d/

//rv32f standard/extension/
/imm[11:0] rs1 010 rd 0000111/flw/
/imm[11:5] rs2 rs1 010 imm[4:0] 0100111/fsw/
/rs3 00 rs2 rs1 rm rd 1000011/fmadd.s/
/rs3 00 rs2 rs1 rm rd 1000111/fmsub.s/
/rs3 00 rs2 rs1 rm rd 1001011/fnmsub.s/
/rs3 00 rs2 rs1 rm rd 1001111/fnmadd.s/
/0000000 rs2 rs1 rm rd 1010011/fadd.s/
/0000100 rs2 rs1 rm rd 1010011/fsub.s/
/0001000 rs2 rs1 rm rd 1010011/fmul.s/
/0001100 rs2 rs1 rm rd 1010011/fdiv.s/
/0101100 00000 rs1 rm rd 1010011/fsqrt.s/
/0010000 rs2 rs1 000 rd 1010011/fsgnj.s/
/0010000 rs2 rs1 001 rd 1010011/fsgnjn.s/
/0010000 rs2 rs1 010 rd 1010011/fsgnjx.s/
/0010100 rs2 rs1 000 rd 1010011/fmin.s/
/0010100 rs2 rs1 001 rd 1010011/fmax.s/
/1100000 00000 rs1 rm rd 1010011/fcvt.w.s/
/1100000 00001 rs1 rm rd 1010011/fcvt.wu.s/
/1110000 00000 rs1 000 rd 1010011/fmv.x.w/
/1010000 rs2 rs1 010 rd 1010011/feq.s/
/1010000 rs2 rs1 001 rd 1010011/flt.s/
/1010000 rs2 rs1 000 rd 1010011/fle.s/
/1110000 00000 rs1 001 rd 1010011/fclass.s/
/1101000 00000 rs1 rm rd 1010011/fcvt.s.w/
/1101000 00001 rs1 rm rd 1010011/fcvt.s.wu/
/1111000 00000 rs1 000 rd 1010011/fmv.w.x/
// rv64f standard extension (in addition to/rv32f)/
/1100000 00010 rs1 rm rd 1010011/fcvt.l.s/
/1100000 00011 rs1 rm rd 1010011/fcvt.lu.s/
/1101000 00010 rs1 rm rd 1010011/fcvt.s.l/
/1101000 00011 rs1 rm rd 1010011/fcvt.s.lu/

//rv32d standard/extension/
/imm[11:0] rs1 011 rd 0000111/fld/
/imm[11:5] rs2 rs1 011 imm[4:0] 0100111/fsd/
/rs3 01 rs2 rs1 rm rd 1000011/fmadd.d/
/rs3 01 rs2 rs1 rm rd 1000111/fmsub.d/
/rs3 01 rs2 rs1 rm rd 1001011/fnmsub.d/
/rs3 01 rs2 rs1 rm rd 1001111/fnmadd.d/
/0000001 rs2 rs1 rm rd 1010011/fadd.d/
/0000101 rs2 rs1 rm rd 1010011/fsub.d/
/0001001 rs2 rs1 rm rd 1010011/fmul.d/
/0001101 rs2 rs1 rm rd 1010011/fdiv.d/
/0101101 00000 rs1 rm rd 1010011/fsqrt.d/
/0010001 rs2 rs1 000 rd 1010011/fsgnj.d/
/0010001 rs2 rs1 001 rd 1010011/fsgnjn.d/
/0010001 rs2 rs1 010 rd 1010011/fsgnjx.d/
/0010101 rs2 rs1 000 rd 1010011/fmin.d/
/0010101 rs2 rs1 001 rd 1010011/fmax.d/
/0100000 00001 rs1 rm rd 1010011/fcvt.s.d/
/0100001 00000 rs1 rm rd 1010011/fcvt.d.s/
/1010001 rs2 rs1 010 rd 1010011/feq.d/
/1010001 rs2 rs1 001 rd 1010011/flt.d/
/1010001 rs2 rs1 000 rd 1010011/fle.d/
/1110001 00000 rs1 001 rd 1010011/fclass.d/
/1100001 00000 rs1 rm rd 1010011/fcvt.w.d/
/1100001 00001 rs1 rm rd 1010011/fcvt.wu.d/
/1101001 00000 rs1 rm rd 1010011/fcvt.d.w/
/1101001 00001 rs1 rm rd 1010011/fcvt.d.wu/

//rv64d standard extension (in addition to/rv32d)/
/1100001 00010 rs1 rm rd 1010011/fcvt.l.d/
/1100001 00011 rs1 rm rd 1010011/fcvt.lu.d/
/1110001 00000 rs1 000 rd 1010011/fmv.x.d/
/1101001 00010 rs1 rm rd 1010011/fcvt.d.l/
/1101001 00011 rs1 rm rd 1010011/fcvt.d.lu/
/1111001 00000 rs1 000 rd 1010011/fmv.d.x/

//rv32q standard/extension/
/imm[11:0] rs1 100 rd 0000111/flq/
/imm[11:5] rs2 rs1 100 imm[4:0] 0100111/fsq/
/rs3 11 rs2 rs1 rm rd 1000011/fmadd.q/
/rs3 11 rs2 rs1 rm rd 1000111/fmsub.q/
/rs3 11 rs2 rs1 rm rd 1001011/fnmsub.q/
/rs3 11 rs2 rs1 rm rd 1001111/fnmadd.q/
/0000011 rs2 rs1 rm rd 1010011/fadd.q/
/0000111 rs2 rs1 rm rd 1010011/fsub.q/
/0001011 rs2 rs1 rm rd 1010011/fmul.q/
/0001111 rs2 rs1 rm rd 1010011/fdiv.q/
/0101111 00000 rs1 rm rd 1010011/fsqrt.q/
/0010011 rs2 rs1 000 rd 1010011/fsgnj.q/
/0010011 rs2 rs1 001 rd 1010011/fsgnjn.q/
/0010011 rs2 rs1 010 rd 1010011/fsgnjx.q/
/0010111 rs2 rs1 000 rd 1010011/fmin.q/
/0010111 rs2 rs1 001 rd 1010011/fmax.q/
/0100000 00011 rs1 rm rd 1010011/fcvt.s.q/
/0100011 00000 rs1 rm rd 1010011/fcvt.q.s/
/0100001 00011 rs1 rm rd 1010011/fcvt.d.q/
/0100011 00001 rs1 rm rd 1010011/fcvt.q.d/
/1010011 rs2 rs1 010 rd 1010011/feq.q/
/1010011 rs2 rs1 001 rd 1010011/flt.q/
/1010011 rs2 rs1 000 rd 1010011/fle.q/
/1110011 00000 rs1 001 rd 1010011/fclass.q/
/1100011 00000 rs1 rm rd 1010011/fcvt.w.q/
/1100011 00001 rs1 rm rd 1010011/fcvt.wu.q/
/1101011 00000 rs1 rm rd 1010011/fcvt.q.w/
/1101011 00001 rs1 rm rd 1010011/fcvt.q.wu/

//rv64q standard extension (in addition to/rv32q)/
/1100011 00010 rs1 rm rd 1010011/fcvt.l.q/
/1100011 00011 rs1 rm rd 1010011/fcvt.lu.q/
/1101011 00010 rs1 rm rd 1010011/fcvt.q.l/
/1101011 00011 rs1 rm rd 1010011/fcvt.q.lu/

//rv32zfh standard/extension/
/imm[11:0] rs1 001 rd 0000111/flh/
/imm[11:5] rs2 rs1 001 imm[4:0] 0100111/fsh/
/rs3 10 rs2 rs1 rm rd 1000011/fmadd.h/
/rs3 10 rs2 rs1 rm rd 1000111/fmsub.h/
/rs3 10 rs2 rs1 rm rd 1001011/fnmsub.h/
/rs3 10 rs2 rs1 rm rd 1001111/fnmadd.h/
/0000010 rs2 rs1 rm rd 1010011/fadd.h/
/0000110 rs2 rs1 rm rd 1010011/fsub.h/
/0001010 rs2 rs1 rm rd 1010011/fmul.h/
/0001110 rs2 rs1 rm rd 1010011/fdiv.h/
/0101110 00000 rs1 rm rd 1010011/fsqrt.h/
/0010010 rs2 rs1 000 rd 1010011/fsgnj.h/
/0010010 rs2 rs1 001 rd 1010011/fsgnjn.h/
/0010010 rs2 rs1 010 rd 1010011/fsgnjx.h/
/0010110 rs2 rs1 000 rd 1010011/fmin.h/
/0010110 rs2 rs1 001 rd 1010011/fmax.h/
/0100000 00010 rs1 rm rd 1010011/fcvt.s.h/
/0100010 00000 rs1 rm rd 1010011/fcvt.h.s/
/0100001 00010 rs1 rm rd 1010011/fcvt.d.h/
/0100010 00001 rs1 rm rd 1010011/fcvt.h.d/
/0100011 00010 rs1 rm rd 1010011/fcvt.q.h/
/0100010 00011 rs1 rm rd 1010011/fcvt.h.q/
/1010010 rs2 rs1 010 rd 1010011/feq.h/
/1010010 rs2 rs1 001 rd 1010011/flt.h/
/1010010 rs2 rs1 000 rd 1010011/fle.h/
/1110010 00000 rs1 001 rd 1010011/fclass.h/
/1100010 00000 rs1 rm rd 1010011/fcvt.w.h/
/1100010 00001 rs1 rm rd 1010011/fcvt.wu.h/
/1110010 00000 rs1 000 rd 1010011/fmv.x.h/
/1101010 00000 rs1 rm rd 1010011/fcvt.h.w/
/1101010 00001 rs1 rm rd 1010011/fcvt.h.wu/
/1111010 00000 rs1 000 rd 1010011/fmv.h.x/

//rv64zfh standard extension (in addition to/rv32zfh)
/1100010 00010 rs1 rm rd 1010011/fcvt.l.h/

//rv64zfh standard extension (in addition to/rv32zfh)/
/1100010 00011 rs1 rm rd 1010011/fcvt.lu.h/
/1101010 00010 rs1 rm rd 1010011/fcvt.h.l/
/1101010 00011 rs1 rm rd 1010011/fcvt.h.lu/

//zawrs standard/extension/
/000000001101 00000 000 00000 1110011/wrs.nto/
/000000011101 00000 000 00000 1110011/wrs.sto/
*/
}