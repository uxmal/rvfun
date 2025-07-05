namespace rvfun;

using rvfun.lib;
using Reko.Core;
using Reko.Core.Memory;
// using static rvfun.Mnemonics;

public class Emulator
{
    private readonly Memory mem;
    private readonly OsEmulator osEmulator;

    private uint iptr;       // instruction pointer
    private int instrsExecuted = 0;
    private readonly int[] x;
    private readonly int[] regMasks;

    private int? exitCode;

    private static BitField bfOpcode = new BitField(0, 7);
    private static BitField bfFunct3 = new BitField(12, 3);
    private static BitField bfFunct7 = new BitField(25, 7);
    private static BitField bfDst = new BitField(7, 5);
    private static BitField bfSrc1 = new BitField(15, 5);
    private static BitField bfSrc2 = new BitField(20, 5);

    private static BitField bfImm20 = new BitField(20, 12);
    private static BitField bf25L7 = new BitField(25, 7);
    private static BitField bf7L5 = new BitField(7, 5);
    private static BitField[] bitFieldsSimm = new BitField[] { bf25L7, bf7L5 };

    private static BitField bf31L1 = new BitField(31, 1);
    private static BitField bf12L8 = new BitField(12, 8);
    private static BitField bf12L20 = new BitField(12, 20);
    private static BitField bf20L1 = new BitField(20, 1);
    private static BitField bf21L10 = new BitField(21, 10);
    private static BitField[] bitFieldsJimm = new BitField[] { bf31L1, bf12L8, bf20L1, bf21L10 };

    private static BitField bf7L1 = new BitField(7, 1);
    private static BitField bf25L6 = new BitField(25, 6);
    private static BitField bf8L4 = new BitField(8, 4);
    private static BitField[] bitFieldsBimm = new BitField[] { bf31L1, bf7L1, bf25L6, bf8L4 };


    public Emulator(Memory mem, OsEmulator osEmulator, uint startAddress)
        : this(mem, osEmulator, new int[32], startAddress) 
    {}


    public Emulator(Memory mem, OsEmulator osEmulator, int[] registers, uint startAddress)
    {
        if (registers.Length != 32)
            throw new ArgumentException("Risc-V must have 32 registers.");
        this.mem = mem;
        this.osEmulator = osEmulator;

        this.x = registers;
        this.regMasks = new int[32];
        for (int i = 1; i < regMasks.Length; ++i)
        {
            this.regMasks[i] = ~0;
        }
        this.iptr = startAddress;
    }

    public int[] Registers => x;

    // Executes RiscV instructions until told to stop.
    public int exec()
    {
        for (;;)
        {
            var instr = mem.ReadLeWord32Executable(iptr);
            if (instr == 0)
                break;
            ++this.instrsExecuted;
            if (this.instrsExecuted > 2000)
            {
                Console.Error.WriteLine("Watchdog timer: expired");
                return -1;
            }
            iptr = exec(instr);
            if (exitCode.HasValue)
            {
                return exitCode.Value;
            }
        }
        return -1;
    }

    private uint exec(uint uInstr)
    {
        // Console.WriteLine("{0:X8}: {1}", iptr, Convert.ToString(uInstr, 2).PadLeft(32, '0'));
        // DumpCurrentInstruction(iptr);
        var iptrNext = iptr + 4;
        var opcode = bfOpcode.ExtractUnsigned(uInstr);
        uint funct3;
        uint funct7;
        uint src1;
        uint src2;
        uint dst;
        int imm;
        uint uimm;
        uint ea;
        switch (opcode)
        {
            case 0b0000011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                imm = bfImm20.ExtractSigned(uInstr);
                dst = bfDst.ExtractUnsigned(uInstr);
                ea = (uint)(this.Registers[src1] + imm);
                int value;
                switch (funct3)
                {
                    case 0b000: // lb
                        value = (sbyte)mem.ReadByte(ea);
                        break;
                    case 0b001: // lh
                        value = (short)mem.ReadLeWord16(ea);
                        break;
                    case 0b010: // lw
                        value = (int)mem.ReadLeWord32(ea);
                        break;
                    case 0b100: // lbu
                        value = mem.ReadByte(ea);
                        break;
                    case 0b101: // lhu
                        value = (ushort)mem.ReadLeWord16(ea);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                WriteRegister(dst, value);
                break;

            case 0b0110011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                src2 = bfSrc2.ExtractUnsigned(uInstr);
                dst = bfDst.ExtractUnsigned(uInstr);
                funct7 = bfFunct7.ExtractUnsigned(uInstr);
                var src1Value = x[src1];
                var src2Value = x[src2];
                switch (funct3)
                {
                    case 0:
                        switch (funct7)
                        {
                            case 0:
                                value = src1Value + src2Value;
                                break;
                            case 1:
                                value = src1Value * src2Value;
                                break;
                            case 0b0100000:
                                value = src1Value - src2Value;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    case 1:
                        switch (funct7)
                        {
                            case 0b0000000 : // sll
                                value = src1Value << src2Value;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    case 0b010:
                        switch (funct7)
                        {
                            case 0b0000000 : // slt
                                value = src1Value < src2Value ? 1 : 0;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    case 0b011:
                        switch (funct7)
                        {
                            case 0b0000000 : // sltu
                                value = (uint)src1Value < (uint)src2Value ? 1 : 0;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    case 0b100:
                        switch (funct7)
                        {
                            case 0b0000000 : // xor
                                value = src1Value ^ src2Value;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    case 0b101:
                        switch (funct7)
                        {
                            case 0b0000000 : // srl
                                value = src1Value >>> src2Value;
                                break;
                            case 0b0100000: // sra
                                value = src1Value >> src2Value;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;

                    case 0b110:
                        switch (funct7)
                        {
                            case 0b0000000 : // or
                                value = src1Value | src2Value;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;                        
                    case 0b111:
                        switch (funct7)
                        {
                            case 0b0000000 : // and
                                value = src1Value & src2Value;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                WriteRegister(dst, value);
                break;

            case 0b0010011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                imm = bfImm20.ExtractSigned(uInstr);
                dst = bfDst.ExtractUnsigned(uInstr);
                src1Value = x[src1];
                switch (funct3)
                {
                    case 0b000: // addi
                        value = src1Value + imm;
                        break;
                    case 0b001:
                        funct7 = bfFunct7.ExtractUnsigned(uInstr);
                        src2 = bfSrc2.ExtractUnsigned(uInstr);
                        var shamt = (int)src2;
                        switch (funct7)
                        {
                            case 0b0000000: // slli
                                value = src1Value << shamt;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    case 0b010: // slti
                        value = src1Value < imm ? 1 : 0;
                        break;
                    case 0b011: // sltiu
                        value = (uint)src1Value < (uint) imm ? 1 : 0;
                        break;
                    case 0b100: // xori
                        value = src1Value ^ imm;
                        break;
                    case 0b101:
                        funct7 = bfFunct7.ExtractUnsigned(uInstr);
                        src2 = bfSrc2.ExtractUnsigned(uInstr);
                        shamt = (int)src2;
                        switch (funct7)
                        {
                            case 0b0000000: // srli
                                value = src1Value >>> shamt;
                                break;
                            case 0b0100000: // srai
                                value = src1Value >> shamt;
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;

                    case 0b110: // ori
                        value = src1Value | imm;
                        break;
                    case 0b111: // andi
                        value = src1Value & imm;
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                WriteRegister(dst, value);
                break;
            case 0b0010111: // auipc
                dst = bfDst.ExtractUnsigned(uInstr);
                uimm = bf12L20.ExtractUnsigned(uInstr);
                WriteRegister(dst, (int)(iptr + (uimm << 12)));
                break;

            case 0b0100011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                imm = BitField.ExtractSigned(uInstr, bitFieldsSimm);
                src2 = bfSrc2.ExtractUnsigned(uInstr);
                src2Value = Registers[src2];
                ea = (uint)(this.Registers[src1] + imm);
                switch (funct3)
                {
                    case 0b000: // sb
                        this.mem.WriteByte(ea, (byte)src2Value);
                        break;
                    case 0b001: // sh
                        this.mem.WriteLeWord16(ea, (ushort)src2Value);
                        break;
                    case 0b010: // sw
                        this.mem.WriteLeWord32(ea, (uint)src2Value);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                break;
            case 0b1100011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                src2 = bfSrc2.ExtractUnsigned(uInstr);
                imm = BitField.ExtractSigned(uInstr, bitFieldsBimm) << 1;
                bool predicate;
                src1Value = Registers[src1];
                src2Value = Registers[src2];
                switch (funct3)
                {
                    case 0b000:     // beq
                        predicate = src1Value == src2Value;
                        break;
                    case 0b001:     // bne
                        predicate = src1Value != src2Value;
                        break;
                    case 0b100:     // blt
                        predicate = src1Value < src2Value;
                        break;
                    case 0b101:     // bge
                        predicate = src1Value >= src2Value;
                        break;
                    case 0b110:     // bltu
                        predicate = (uint)src1Value < (uint)src2Value;
                        break;
                    case 0b111:     // bgeu
                        predicate = (uint)src1Value >= (uint) src2Value;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                if (predicate)
                {
                    iptrNext = iptr + (uint)imm;
                }
                break;
            case 0b0110111: // lui
                dst = bfDst.ExtractUnsigned(uInstr);
                uimm = bf12L20.ExtractUnsigned(uInstr);
                WriteRegister(dst, (int)uimm << 12);
                break;
            case 0b1100111: // jalr
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                imm = bfImm20.ExtractSigned(uInstr);
                dst = bfDst.ExtractUnsigned(uInstr);
                switch (funct3)
                {
                    case 0:
                        var retAddr = iptrNext;
                        iptrNext = (uint)(Registers[src1] + imm);
                        WriteRegister(dst, (int)retAddr);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                break;
            case 0b1101111: // jal
                dst = bfDst.ExtractUnsigned(uInstr);
                imm = BitField.ExtractSigned(uInstr, bitFieldsJimm) << 1;
                WriteRegister(dst, (int)iptrNext);
                iptrNext = iptr + (uint)imm;
                break;
            case 0b1110011:
                uint sysfunc = bfImm20.ExtractUnsigned(uInstr);
                switch (sysfunc)
                {
                    case 0: // ecall
                        osEmulator.Trap(this);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown system instruction {Convert.ToString(opcode, 2)}");
                }
                break;
            default:
                throw new InvalidOperationException($"Unknown major opcode {Convert.ToString(opcode, 2)}");
        }
        iptr = iptrNext;
        return iptr;
    }   

    private void DumpCurrentInstruction(uint iptr)
    {
        var arch = new Reko.Arch.RiscV.RiscVArchitecture(null!, "riscv", []);
        var span = mem.GetSpan((int)iptr, 4, AccessMode.Read);
        var bytes = new byte[4];
        span.CopyTo(bytes.AsSpan());
        var m = new ByteMemoryArea(Address.Ptr32(iptr), bytes);
        var rdr = m.CreateLeReader(0);
        var dasm = arch.CreateDisassembler(rdr);
        var instr = dasm.First();
        Console.WriteLine("{0:X8}: {1}", iptr, instr);
    }

    private void WriteRegister(uint dst, int value)
    {
        var mask = regMasks[dst];
        this.Registers[dst] = value & mask;
        if (mask != 0)
        {
            // Console.WriteLine($"    x{dst} = {value:X8}");
        }
    }

    public void Stop(int exitCode)
    {
        this.exitCode = exitCode;
    }

    /*
       switch (instr.opcode)
       {
           case mul:
               x[instr.dst] = x[instr.src1] * x[instr.src2];
               break;
           case muli:
               x[instr.dst] = x[instr.src1] * instr.src2;
               break;
           case lb:
               var ea = x[instr.src1] + instr.src2;
               x[instr.dst] = (sbyte)mem.ReadByte(ea);
               break;
           case lw:
               ea = x[instr.src1] + instr.src2;
               x[instr.dst] = mem.ReadLeWord32(ea);
               break;
           case sb:
               ea = x[instr.src1] + instr.src2;
               mem.WriteByte(ea, (byte)x[instr.dst]);
               break;
           case sw:
               ea = x[instr.src1] + instr.src2;
               mem.WriteLeWord32(ea, x[instr.dst]);
               break;
           case sgti:
               int flag = x[instr.src1] > instr.src2 ? 1 : 0;
               x[instr.dst] = flag;
               break;
           case @goto:
               iptr = instr.dst;
               break;
           case bnz:
               if (x[instr.src1] != 0)
                   iptr = instr.dst;
               break;
           default:
               throw new NotImplementedException($"Operation {instr.opcode} not implemented yet.");
       }
       return iptr;
       */
}
