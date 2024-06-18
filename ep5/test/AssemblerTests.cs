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

    
    /*
m.asm(@goto, 5, 0, 0);

m.asm(bnz, 3, 3, 0);    
    */
}