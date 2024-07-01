
using rvfun;

using static rvfun.Mnemonics;

var mem = new Memory(new byte[1024]);
var x = new int[32];

int fact(int n)
{
    if (n < 2)
        return 1;
    return n * fact(n - 1);
}

var result = fact(10);

// 100: 03 00 00 00 00 00 00 00 03 00 00 00
// x2 = 100

var m = new Assembler(mem);

m.asm(jal, 1, 8, 0);
m.asm(Invalid, 0, 0, 0);

// save n on the stack
m.asm(addi, 2, 2, -8);
m.asm(sw, 1, 2, 4);         // Save the return address
m.asm(sw, 10, 2, 0);        // Save the n which is in register x10 
m.asm(addi, 11, 10, -2);
m.asm(bge, 11, 0, 16);      // Skip the base case if n >= 2

m.asm(addi, 10, 0, 1);      // Base case: return 1
m.asm(addi, 2, 2, 8);       // Restore stack pointer
m.asm(jalr, 0, 1, 0);       // Return

m.asm(addi, 10, 10, -1);    // Compute n - 1 into x10
m.asm(jal, 1, -36, 0);      // x10 = fact(n - 1)
m.asm(lw, 11, 2, 0);        // get the old value of n into x11
m.asm(mul, 10, 10, 11);     // compute n * fact(n-1) into x10

m.asm(lw, 1, 2, 4);         // restore the return register
m.asm(addi, 2, 2, 8);       // Restore stack pointer
m.asm(jalr, 0, 1, 0);       // Return

/*

m.asm(addi, 2, 2, -12);
m.asm(sw, 1, 2, 8);
m.asm(sw, 10, 2, 0);
m.asm(jal, 1, 40, 0);           // sum

m.asm(sw, 10, 2, 4);
m.asm(lw, 10, 2, 0);
m.asm(addi, 11, 0, 2);
m.asm(jal, 1, 24, 0);          // sum

m.asm(lw, 11, 2, 4);
m.asm(mul, 10, 10, 11);
m.asm(lw, 1, 2, 8);
m.asm(addi, 2, 2, 12);

m.asm(Invalid, 0, 1, 0);

m.asm(add, 10, 10, 11);         // sum:
m.asm(jalr, 0, 1, 0);





/*
m.asm(addi, 10, 0, 3);          
m.asm(addi, 11, 0, 4);
m.asm(jal, 1, 8, 0);            // call sum subroutine
m.asm(jal, 0, 12, 0);           // go to the exit instruction

m.asm(add, 10, 10, 11);         // sum:
m.asm(jalr, 0, 1, 0);

m.asm(addi, 0, 0, 0);           // exit:
*/
// var emu = new Emulator(mem);
// emu.Registers[2] = 1020;
// emu.Registers[10] = 3;
// emu.Registers[11] = 4;
// emu.exec();
// var result = emu.Registers[10];

 var emu = new Emulator(mem);
emu.Registers[2] = 1020;
emu.Registers[10] = 10;
emu.exec();
result = emu.Registers[10];


Console.WriteLine("C# Result:  {0}", fact(10));
Console.WriteLine("Asm Result: {0}", result);
