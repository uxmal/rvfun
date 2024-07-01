namespace rvfun.UnitTests;

#pragma warning disable NUnit2005

[TestFixture]
public class AssemblerTests
{

    private void RunTest(uint uInstrExpected, Action<Assembler> testBuilder)
    {
        var bytes = new byte[1024];
        var memory = new Memory(bytes);
        var m = new Assembler(memory);

        testBuilder(m);

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
            m => m.asm(Mnemonics.mul, 17, 21, 31));
    }

    [Test]
    public void RiscvAsm_slti()
    {
        RunTest(
            0b111111111110_10101_010_10001_0010011u,
            m => m.asm(Mnemonics.slti, 17, 21, -2));
    }

    [Test]
    public void RiscvAsm_lb()
    {
        RunTest(
            0b111111111110_10101_000_10001_0000011u,
            m => m.asm(Mnemonics.lb, 17, 21, -2));
    }

       [Test]
    public void RiscvAsm_lh()
    {
        RunTest(
            0b111111111110_10101_001_10001_0000011u,
            m => m.asm(Mnemonics.lh, 17, 21, -2));
    }

    [Test]
    public void RiscvAsm_lw()
    {
        RunTest(
            0b111111111110_10101_010_10001_0000011u,
            m => m.asm(Mnemonics.lw, 17, 21, -2));
    }
    
[Test]
    public void RiscvAsm_lbu()
    {
        RunTest(
            0b111111111110_10101_100_10001_0000011u,
            m => m.asm(Mnemonics.lbu, 17, 21, -2));
    }

       [Test]
    public void RiscvAsm_lhu()
    {
        RunTest(
            0b111111111110_10101_101_10001_0000011u,
            m => m.asm(Mnemonics.lhu, 17, 21, -2));
    }

    [Test]
    public void RiscvAsm_sb()
    {
        RunTest(
            0b1010101_10001_10101_000_01010_0100011u,
            m => m.asm(Mnemonics.sb, 17, 21, -1366));
    }

      [Test]
    public void RiscvAsm_sh()
    {
        RunTest(
            0b1010101_10001_10101_001_01010_0100011u,
            m => m.asm(Mnemonics.sh, 17, 21, -1366));
    }

      [Test]
    public void RiscvAsm_sw()
    {
        RunTest(
            0b1010101_10001_10101_010_01010_0100011u,
            m => m.asm(Mnemonics.sw, 17, 21, -1366));
    }

    [Test]
    public void RiscvAsm_bne()
    {
        //     imm[12|10:5] rs2 rs1 001 imm[4:1|11] 1100011 
        RunTest(
            0b1010101_10101_10001_001_01011_1100011u,
            m => m.asm(Mnemonics.bne, 17, 21, -1366));
    }

    [Test]
    public void RiscvAsm_jal()
    {
        RunTest(
            // imm[20|10:1|11|19:12] rd 1101111 
            0b10101010101111111111_00000_1101111u,
            m => m.asm(Mnemonics.jal, 0, -1366, 0));
    }
}