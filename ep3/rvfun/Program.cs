
using rvfun;

using static rvfun.Mnemonics;

var mem = new Memory(new byte[1024]);

var m = new Assembler();
m.asm(addi, 1, 0, 10);
m.asm(addi, 2, 0, 1);
m.asm(@goto, 5, 0, 0);

m.asm(mul, 2, 2, 1);    // 3: loop header
m.asm(addi, 1, 1, -1);

m.asm(sgti, 3, 1, 1);       // 5: check
m.asm(bnz, 3, 3, 0);

// int x1 = 10;
// int x2 = 1;
// goto check;

// loop:
// x2 = x2 * x1;
// x1 = x1 - 1;

// check:
// if (x1 > 1) goto loop;

var emu = new Emulator(m.ToProgram(), mem);
emu.exec();

Console.WriteLine("{0}", emu.Registers[2]);
