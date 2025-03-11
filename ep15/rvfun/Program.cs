using rvfun;

const uint BaseAddress = 0x80_0000;
const uint StackAddress = 0x40_0000;
const uint StackSize = 0x1000;

var logger = new Logger();
var m = new Assembler(logger);

m.addi(2, 2, -16);      // make space for 4 words
m.sw(10, 2, 0);         // store argc
m.sw(11, 2, 4);         // store argv
m.sw(0, 2, 8);          // store i
m.sw(1, 2, 12);         // store return address.

m.j("main_looptest");

m.label("main_loopbody");
m.lw(13, 2, 8);         // get i
m.beq(13, 0, "main_write_arg"); // only write space if i != 0

m.auipc(10, m.pcrel_hi("space"));
m.addi(10, 10, m.pcrel_lo(-4));
m.jal(1, "putstring");

m.label("main_write_arg");
m.lw(10, 2, 4);         // x10 = argv
m.slli(13, 13, 2);      // 13 = i*4
m.add(10, 10, 13);      // x10 = argv + i*4
m.lw(10, 10, 0);        // x10 = argv[i]
m.jal(1, "putstring");

m.addi(13, 13, 1);      // move to next argument
m.sw(13, 2, 8);         // save i

m.label("main_looptest");
m.lw(13, 2, 8);     // x13 = i
m.lw(10, 2, 0);     // x10 = argc
m.blt(13, 10, "main_loopbody");

m.auipc(10, m.pcrel_hi("nl"));
m.addi(10, 10, m.pcrel_lo(-4));
m.jal(1, "putstring");

m.li(10, 0);
m.li(17, OsEmulator.SYSCALL_EXIT);
m.ecall();
// m.lw(1, 2, 12);         // restore return address.
// m.addi(2, 2, 16);      // restore stack ptr
// m.jalr(0, 1, 0);

// putstring ////////////////
m.label("putstring");
m.addi(2, 2, -8);       // allocate space for two words
m.sw(1, 2, 0);          // store return address
m.sw(10, 2, 4);         // store initial value of the string ptr.

m.jal(1, "strlen");
m.addi(12, 10, 0);      // Copy x10 into x12
m.lw(11, 2, 4);         // reload the string ptr
m.li(10, 1);            // std output in x10
m.li(17, OsEmulator.SYSCALL_WRITE);

m.ecall();

m.lw(1, 2, 0);          // retore link register
m.add(2, 2, 8);         // restore stack ptr
m.jalr(0, 1, 0);

// strlen //////////////
m.label("strlen");
m.addi(11, 10, 0);
m.j("strlen_test");

m.label("strlen_body");
m.addi(10, 10, 1);

m.label("strlen_test");
m.lbu(12, 10, 0);       // read a byte pointed to by x0 into x12
m.bne(12, 0, "strlen_body");

m.sub(10, 10, 11);
m.jalr(0, 1, 0);

// data
m.label("space"); m.ds(" \0");
m.label("nl"); m.ds("\r\n\0");


var linker = new Linker(m.Section, BaseAddress, m.Symbols, m.Relocations, logger);
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
    ? ["hello", "world!"]
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
