namespace rvfun.UnitTests;

using NUnit.Framework;

#pragma warning disable NUnit2005

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
}