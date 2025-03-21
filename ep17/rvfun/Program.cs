using rvfun;

const uint BaseAddress = 0x80_0000;
const uint StackAddress = 0x40_0000;
const uint StackSize = 0x1000;

var logger = new Logger();
var bytes = File.ReadAllBytes("../../../echo.asm");
var m = new TextAssembler(bytes, logger);
var section = m.AssembleFile();
if (section is null)
{
    Console.WriteLine("Errors were found. Stopping assembly.");
    return;
}



var linker = new Linker(section, BaseAddress, m.Symbols, m.Relocations, logger);
var mem = linker.Relocate();

/*
m.li(11, 10);
m.li(10, 1);
m.j("loop_head");    // head of loop

m.label("loop_body");
m.mul(10, 10, 11);
m.addi(11, 11, -1);

m.label("loop_head");
m.blt(0, 11, "loop_body");  // loop body
*/


/*
m.li(17, OsEmulator.SYSCALL_WRITE);   // select the 'write' system call
m.li(10, 1);                          // use handle 1 = stdout
m.li(11, 32);                         // x11 has pointer to buffer.
m.li(12, 14);                         // x12 has number of bytes to write.
m.ecall(0, 0, 0);                          // do the write

m.li(10, 0);                          // exit code = 0;
m.li(17,  OsEmulator.SYSCALL_EXIT);    // select the 'exit' system call
m.ecall(0, 0, 0);
m.ds("Hello, world!\n\0");
*/
args = args.Length == 0
    ? ["hello", "world!", "another", "argument"]
    : args;

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
