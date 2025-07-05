using K4os.Compression.LZ4.Internal;
using Reko.Arch.Arm;
using Reko.Core.Output;
using rvfun;
using rvfun.lib;
using rvfun.lib.elf;

const uint StackAddress = 0x40_0000;
const uint StackSize = 0x1000;

var elfLoader = new ElfLoader();
using var file = File.OpenRead("a.out");
elfLoader.Load(file);

var mem = LoadElfFile(elfLoader);

args = args[1..];
var osemu = new OsEmulator(mem);
var (stackPtr, argPtr, argCount) = osemu.CreateStackSegment(StackAddress, StackSize, args);
var emu = new Emulator(mem, osemu, elfLoader.header.e_entry);
var dumper = new HexDumper(mem);
dumper.Dump(stackPtr, (int)((StackAddress + StackSize) - stackPtr), Console.Out);
emu.Registers[2] = (int)stackPtr;
emu.Registers[10] = argCount;
emu.Registers[11] = (int)argPtr;
emu.exec();
var result = emu.Registers[2];
Console.WriteLine("Asm Result: {0}", result);


Memory LoadElfFile(ElfLoader elfLoader)
{

    var mem = new Memory();
    for (int i = 0; i < elfLoader.SegmentHeaders.Count; ++i)
    {
        var hdr = elfLoader.SegmentHeaders[i];
        var data = elfLoader.SegmentData[i];
        AccessMode mode = 0;
        if ((hdr.p_flags & Elf.PF_R) != 0) mode |= AccessMode.Read;
        if ((hdr.p_flags & Elf.PF_W) != 0) mode |= AccessMode.Write;
        if ((hdr.p_flags & Elf.PF_X) != 0) mode |= AccessMode.Execute;
        mem.Allocate(hdr.p_vaddr, data, mode);
    }
    return mem;
}