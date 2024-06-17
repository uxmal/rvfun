namespace rvfun;

using static rvfun.Mnemonics;

public class Emulator
{
    private readonly Memory mem;

    private uint iptr;       // instruction pointer
    private readonly int[] x;

    private static BitField bfOpcode = new BitField(0, 7);
    private static BitField bfFunct3 = new BitField(12, 3);
    private static BitField bfDst = new BitField(7, 5);
    private static BitField bfSrc1 = new BitField(15, 5);

    private static BitField bfImm20 = new BitField(20, 12);

    public Emulator(Memory mem)
    {
        this.mem = mem;
        this.x = new int[32];
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
        iptr = iptr + 4;
        var opcode = bfOpcode.ExtractUnsigned(uInstr);
        uint funct3;
        switch (opcode)
        {
            case 0b0010011:
                funct3 = bfFunct3.ExtractUnsigned(uInstr);
                uint src1;
                uint dst;
                int imm;
                switch (funct3)
                {
                    case 0b000: // addi
                        src1 = bfSrc1.ExtractUnsigned(uInstr);
                        imm = bfImm20.ExtractSigned(uInstr);
                        dst = bfDst.ExtractUnsigned(uInstr);
                        x[dst] = x[src1] + imm;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown funct3 {Convert.ToString(funct3, 2)}");
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown major opcde {Convert.ToString(opcode, 2)}");
        }
            return iptr;
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
            case add:
                x[instr.dst] = x[instr.src1] + x[instr.src2];
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
