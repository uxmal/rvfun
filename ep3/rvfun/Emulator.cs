namespace rvfun;

using static rvfun.Mnemonics;

public class Emulator
{
    private readonly List<Instruction> program;
    private readonly Memory mem;

    private int iptr;       // instruction pointer
    private readonly int[] x;

    public Emulator(List<Instruction> program, Memory mem)
    {
        this.program = program;
        this.mem = mem;
        this.x = new int[32];
        this.iptr = 0;
    }
    
    public int[] Registers => x;

    public void exec()
    {
        while (iptr < program.Count)
        {
            var instr = program[iptr];
            iptr = exec(instr);
        }
    }

    private int exec(Instruction instr)
    {
        iptr = iptr+1;
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
            case addi:
                x[instr.dst] = x[instr.src1] + instr.src2;
                break;
            case lb:
                var ea = x[instr.src1] + instr.src2;
                x[instr.dst] = (sbyte) mem.ReadByte(ea);
                break;
            case lw:
                ea = x[instr.src1] + instr.src2;
                x[instr.dst] = mem.ReadLeWord32(ea);
                break;
            case sb:
                ea = x[instr.src1] + instr.src2;
                mem.WriteByte(ea, (byte) x[instr.dst]);
                break;
            case sw:
                ea = x[instr.src1] + instr.src2;
                mem.WriteLeWord32(ea, x[instr.dst]);
                break;
            case sgti:
                int flag = x[instr.src1] > instr.src2 ? 1: 0;
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
    }
}