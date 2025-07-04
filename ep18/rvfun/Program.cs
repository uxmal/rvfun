using rvfun;

const uint BaseAddress = 0x80_0000;
const uint StackAddress = 0x40_0000;
const uint StackSize = 0x1000;

// Assembler code.
{
    var logger = new Logger();
    var bytes = File.ReadAllBytes(args[0]);
    var m = new TextAssembler(bytes, logger);
    var section = m.AssembleFile();
    if (section is null)
    {
        Console.WriteLine("Errors were found. Stopping assembly.");
        return;
    }

    var linker = new Linker(section, BaseAddress, m.Symbols, m.Relocations, logger);
    var mem = linker.Relocate();
    var sizeInBytes = section.GetAssembledBytes().Length; //$PERF: this is expensive; creates an array copy.
    var bytesToWrite = mem.GetSpan((int)BaseAddress, sizeInBytes, AccessMode.Read).ToArray(); //$PERF: another copy.
    File.WriteAllBytes("a.out", bytesToWrite);
}

{   // Emulator code 
    var rawBytes = File.ReadAllBytes("a.out");      //$TODO: use args[0] in the future.
    var mem = new Memory();
    mem.Allocate(BaseAddress, rawBytes, AccessMode.RX);

    args = args[1..];
    var osemu = new OsEmulator(mem);
    var (stackPtr, argPtr, argCount) = osemu.CreateStackSegment(StackAddress, StackSize, args);
    var emu = new Emulator(mem, osemu, BaseAddress);
    var dumper = new HexDumper(mem);
    dumper.Dump(stackPtr, (int)((StackAddress + StackSize) - stackPtr), Console.Out);
    emu.Registers[2] = (int)stackPtr;
    emu.Registers[10] = argCount;
    emu.Registers[11] = (int)argPtr;
    emu.exec();
    var result = emu.Registers[2];
    Console.WriteLine("Asm Result: {0}", result);
}