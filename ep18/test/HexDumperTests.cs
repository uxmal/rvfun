namespace rvfun.UnitTests;

using rvfun;
using rvfun.lib;

[TestFixture]
public class HexDumperTests
{
    private const uint MemoryAddress = 0x10_0000;
    private readonly Memory memory;

    public HexDumperTests()
    {
        byte[] bytes = [.. Enumerable.Range(0x40, 0x10).Select(a => (byte) a)];
        memory = new Memory();
        memory.Allocate(MemoryAddress, bytes, AccessMode.Read);
    }

    [Test]
    public void HexDumper_Line()
    {
        var sw = new StringWriter();
        var dumper = new HexDumper(memory);
        dumper.Dump(MemoryAddress, 0x10, sw);
        var sActual = sw.ToString();
        var sExpected = "00100000 40 41 42 43 44 45 46 47 48 49 4A 4B 4C 4D 4E 4F @ABCDEFGHIJKLMNO" + Environment.NewLine;
        Assert.That(sActual, Is.EqualTo(sExpected));
    }

    [Test]
    public void HexDumper_PartialLine()
    {
        var sw = new StringWriter();
        var dumper = new HexDumper(memory);
        dumper.Dump(MemoryAddress + 2, 0xC, sw);
        var sActual = sw.ToString();
        var sExpected = "00100000       42 43 44 45 46 47 48 49 4A 4B 4C 4D         BCDEFGHIJKLM  " + Environment.NewLine;
        Assert.That(sActual, Is.EqualTo(sExpected));
    }
}