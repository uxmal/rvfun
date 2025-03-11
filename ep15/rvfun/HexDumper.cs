
using Reko.Core.Machine;

namespace rvfun;

public class HexDumper
{
    private const uint LineLength = 0x10;

    private readonly Memory memory;

    public HexDumper(Memory memory)
    {
        this.memory = memory;
    }

    public void Dump(uint address, int length, TextWriter w)
    {
        var addrEnd = address+ length;
        for (uint addrLine = (address & ~0xFu); addrLine < addrEnd; addrLine += LineLength)
        {
            w.Write($"{address:X8}");
            for (uint i = 0; i < LineLength; ++i)
            {
                var a = addrLine + i;
                if (a < address || a >= addrEnd)
                {
                    w.Write("   ");
                }
                else 
                {
                    var b = memory.ReadByte(a);
                    w.Write($" {b:X2}");
                }
            }
            w.Write(' ');
            for (uint i = 0; i < LineLength; ++i)
            {
                var a = addrLine + i;
                if (a < address || a >= addrEnd)
                {   
                    w.Write(' ');
                }
                else 
                {
                    var b = memory.ReadByte(addrLine + i);
                    if (0x20 <= b && b < 0x7F)
                    {
                        w.Write((char)b);
                    }
                    else 
                    {
                        w.Write('.');
                    }
                }
            }
            w.WriteLine();
        }
    }
}