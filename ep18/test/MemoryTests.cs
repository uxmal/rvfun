namespace rvfun.UnitTests;

using rvfun.lib;
using NUnit.Framework;

#pragma warning disable NUnit2005
#pragma warning disable NUnit2006

[TestFixture]
public class MemoryTests
{
    [Test]
    public void Mem_Allocate()
    {
        var mem = new Memory();

        var uAddr = mem.Allocate(0, 42, AccessMode.Read);
        Assert.AreEqual(0, uAddr);
        Assert.IsTrue(mem.IsValidAddress(uAddr + Memory.BytesPerPage - 1));
        Assert.IsFalse(mem.IsValidAddress(uAddr + Memory.BytesPerPage));
    }

    [Test]
    public void Mem_Disallow_OverlappedAllocations()
    {
        var mem = new Memory();

        var uChunk1 = mem.Allocate(0x1000, Memory.BytesPerPage + 1, AccessMode.Read);
        try 
        {
            mem.Allocate(0x2000, 42, AccessMode.Read);
            Assert.Fail();
        }
        catch (InvalidOperationException)
        {
        }
    }

    [Test]
    public void Mem_Allocate_AnyAddress()
    {
        var mem = new Memory();
        var addr = mem.Allocate(Memory.Anywhere, 0x42, AccessMode.RX);
        Assert.AreNotEqual(0, addr);
    }

    [Test]
    public void Mem_Allocate_AnyAddress_2()
    {
        var mem = new Memory();
        var addr = mem.Allocate(Memory.Anywhere, 0x42, AccessMode.RX);
        var addr2 = mem.Allocate(Memory.Anywhere, 0x42, AccessMode.RX);
        Assert.IsTrue(addr2 > addr);
    }
}