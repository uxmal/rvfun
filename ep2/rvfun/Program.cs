
using System.Runtime.CompilerServices;
using rvfun;

using static rvfun.Mnemonics;

var mem = new Memory(new byte[1024]);


var assembler = new Assembler();

assembler.asm(addi, 4, 0, 0x10);
assembler.asm(addi, 5, 0, 1);
assembler.asm(sb, 5, 4, 0);
assembler.asm(addi, 5, 5, 1);
assembler.asm(sb, 5, 4, 1);
assembler.asm(addi, 5, 5, 1);
assembler.asm(sb, 5, 4, 2);
assembler.asm(addi, 5, 5, 1);
assembler.asm(sb, 5, 4, 3);
assembler.asm(lw, 6, 4, 0);

var emulator = new Emulator(assembler.ToProgram(), mem);
emulator.exec();

Console.WriteLine("{0:X}", emulator.Registers[6]);





