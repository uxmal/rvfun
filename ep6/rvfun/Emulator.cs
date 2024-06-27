namespace rvfun;

using static rvfun.Mnemonics;

public class Emulator
{
    private readonly Memory mem;

    private uint iptr;       // instruction pointer
    private readonly int[] x;
    private readonly int[] regMasks;

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
    private static BitField bf20L1 = new BitField(20, 1);
    private static BitField bf21L10 = new BitField(21, 10);
    private static BitField[] bitFieldsJimm = new BitField[] { bf31L1, bf12L8, bf20L1, bf21L10 };

    private static BitField bf7L1 = new BitField(7, 1);
    private static BitField bf25L6 = new BitField(25, 6);
    private static BitField bf8L4 = new BitField(8, 4);
    private static BitField[] bitFieldsBimm = new BitField[] { bf31L1, bf7L1, bf25L6, bf8L4 };



    public Emulator(Memory mem)
    {
        this.mem = mem;
        this.x = new int[32];
        this.regMasks = new int[32];
        for (int i = 1; i < regMasks.Length; ++i)
        {
            this.regMasks[i] = ~0;
        }
        this.iptr = 0;
    }

    public int[] Registers => x;

    public void exec()
    {
        while (mem.IsValidAddress(iptr))
        {
            var instr = mem.ReadLeWord32(iptr);
            if (instr == 0)
                break;
            iptr = exec(instr);
        }
    }

    private uint exec(uint uInstr)
    {
        var iptrNext = iptr + 4;
        var opcode = bfOpcode.ExtractUnsigned(uInstr);
        uint funct3;
        uint funct7;
        uint src1;
        uint src2;
        uint dst;
        int imm;
        uint ea;
        switch (opcode)
        {
            case 0b0000011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                imm = bfImm20.ExtractSigned(uInstr);
                dst = bfDst.ExtractUnsigned(uInstr);
                ea = (uint)(this.Registers[src1] + imm);
                switch (funct3)
                {
                    case 0b000: // lb
                        WriteRegister(dst, (sbyte)mem.ReadByte(ea));
                        break;
                    case 0b001: // lh
                        WriteRegister(dst, (short)mem.ReadLeWord16(ea));
                        break;
                    case 0b010: // lw
                        WriteRegister(dst, (int)mem.ReadLeWord32(ea));
                        break;
                    case 0b100: // lbu
                        WriteRegister(dst, mem.ReadByte(ea));
                        break;
                    case 0b101: // lhu
                        WriteRegister(dst, (ushort)mem.ReadLeWord16(ea));
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                break;

            case 0b0110011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                src2 = bfSrc2.ExtractUnsigned(uInstr);
                dst = bfDst.ExtractUnsigned(uInstr);
                funct7 = bfFunct7.ExtractUnsigned(uInstr);

                switch (funct3)
                {
                    case 0:
                        switch (funct7)
                        {
                            case 0:
                                WriteRegister(dst, x[src1] + x[src2]);
                                break;
                            case 1:
                                WriteRegister(dst, x[src1] * x[src2]);
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown funct7 {Convert.ToString(funct7, 2)}");
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                break;

            case 0b0010011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                imm = bfImm20.ExtractSigned(uInstr);
                dst = bfDst.ExtractUnsigned(uInstr);
                switch (funct3)
                {
                    case 0b000: // addi
                        WriteRegister(dst, x[src1] + imm);
                        break;
                    case 0b010: // slti
                        WriteRegister(dst, x[src1] < imm ? 1 : 0);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                break;
            case 0b0100011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                src1 = bfSrc1.ExtractUnsigned(uInstr);
                imm = BitField.ExtractSigned(uInstr, bitFieldsSimm);
                dst = bfDst.ExtractUnsigned(uInstr);
                ea = (uint)(this.Registers[src1] + imm);
                switch (funct3)
                {
                    case 0b000: // sb
                        this.mem.WriteByte(ea, (byte)Registers[dst]);
                        break;
                    case 0b001: // sh
                        this.mem.WriteLeWord16(ea, (ushort)Registers[dst]);
                        break;
                    case 0b010: // sw
                        this.mem.WriteLeWord32(ea, (uint)Registers[dst]);
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
                switch (funct3)
                {
                    case 0b001:
                        if (Registers[src1] != Registers[src2])
                        {
                            iptrNext = iptr + (uint)imm;
                        }
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
            default:
                throw new InvalidOperationException($"Unknown major opcode {Convert.ToString(opcode, 2)}");
        }
        iptr = iptrNext;
        return iptr;
    }

    private void WriteRegister(uint dst, int value)
    {
        var mask = regMasks[dst];
        this.Registers[dst] = value & mask;
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
