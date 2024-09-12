
using rvfun;
using Reko.Arch.RiscV;

using static rvfun.Mnemonics;
using Reko.Core.Memory;
using Reko.Core;

var bytes = new byte[1024];
var mem = new Memory(bytes);
var x = new int[32];

var logger = new Logger();
var m = new Assembler(mem, logger);

m.j("skip");

m.dw("set_r2");

m.label("set_r2");
m.li(2, 3);
m.dw(0);        // Invalid instruction stops execution.

m.label("skip");
m.lw(10, 0, 4);
m.jalr(0, 10, 0);

var linker = new Linker(mem, m.Symbols, m.Relocations, logger);
linker.Relocate();

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

var osemu = new OsEmulator(mem);
var emu = new Emulator(mem, osemu);
emu.Registers[2] = 1020;
emu.Registers[10] = 10;
emu.exec();
var result = emu.Registers[2];


Console.WriteLine("Asm Result: {0}", result);
