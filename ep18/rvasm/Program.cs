using rvfun;
using rvfun.asm;
using rvfun.lib;
using rvfun.lib.elf;

const uint BaseAddress = 0x80_0000;

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
var elfBuilder = new ElfBuilder(false, false, Elf.ET_EXEC, Elf.EM_RISCV);
var sizeInBytes = section.GetAssembledBytes().Length; //$PERF: this is expensive; creates an array copy.
var bytesToWrite = mem.GetSpan((int)BaseAddress, sizeInBytes, AccessMode.Read).ToArray(); //$PERF: another copy.
elfBuilder.AddSegment(Elf.PT_LOAD, BaseAddress, bytesToWrite, 0, AccessMode.RX, Memory.BytesPerPage);
elfBuilder.EntryPointAddress = BaseAddress;
using var file = File.OpenWrite("a.out");
elfBuilder.WriteToStream(file);
