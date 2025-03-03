
using rvfun;
using Reko.Arch.RiscV;

using static rvfun.Mnemonics;
using Reko.Core.Memory;
using Reko.Core;
using System.ComponentModel.DataAnnotations;

var bytes = new byte[2048];
var mem = new Memory(bytes);
var x = new int[32];

var logger = new Logger();
var m = new Assembler(mem, logger);

// int h = open(path, O_RDONLY);

m.li(17, OsEmulator.SYSCALL_OPEN);
m.auipc(10, m.pcrel_hi("path"));
m.addi(10, 10, m.pcrel_lo(-4));
m.li(11, OsEmulator.O_RDONLY);
m.ecall(0, 0, 0);           // handle in x10 register
m.addi(8, 10, 0);

//for (;;) {
m.label("loop_top");

//int cb = read(h, buffer, sizeof(Buffer));
m.li(17, OsEmulator.SYSCALL_READ);
m.add(10, 8, 0);
m.auipc(11, m.pcrel_hi("buffer"));
m.addi(11, 11, m.pcrel_lo(-4));
m.li(12, 1024);
m.ecall(0, 0, 0);           // number of bytes read in x10 register.

//    if (0 >= cb)
//        break;
m.bge(0, 10, "loop_exit");

//  write(1, buffer, cb);
m.li(17, OsEmulator.SYSCALL_WRITE);
m.addi(12, 10, 0);
m.li(10, 1);
m.auipc(11, m.pcrel_hi("buffer"));
m.addi(11, 11, m.pcrel_lo(-4));
m.ecall(0, 0, 0);           // number of bytes written in x10 register.

m.j("loop_top");

m.label("loop_exit");
m.li(17, OsEmulator.SYSCALL_CLOSE);
m.add(10, 8, 0);
m.ecall(0, 0, 0);

m.li(17, OsEmulator.SYSCALL_EXIT);
m.li(10, 0);
m.ecall(0, 0, 0);

m.label("buffer");
m.dw(0, 1024/4);

m.label("path");
m.ds(@"c:\tmp\test.txt");


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
