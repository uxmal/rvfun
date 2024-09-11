
using rvfun;

using static rvfun.Mnemonics;

var mem = new Memory(new byte[1024]);
var x = new int[32];

var m = new Assembler(mem);

m.li(17, OsEmulator.SYSCALL_WRITE);   // select the 'write' system call
m.li(10, 1);                          // use handle 1 = stdout
m.li(11, 32);                         // x11 has pointer to buffer.
m.li(12, 14);                         // x12 has number of bytes to write.
m.ecall(0, 0, 0);                          // do the write

m.li(10, 0);                          // exit code = 0;
m.li(17,  OsEmulator.SYSCALL_EXIT);    // select the 'exit' system call
m.ecall(0, 0, 0);
m.ds("Hello, world!\n\0");

var osemu = new OsEmulator(mem);
var emu = new Emulator(mem, osemu);
emu.Registers[2] = 1020;
emu.Registers[10] = 10;
var result = emu.exec();


Console.WriteLine("Asm Result: {0}", result);
