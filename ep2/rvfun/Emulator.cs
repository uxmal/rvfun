namespace rvfun;

using static rvfun.Mnemonics;

public class Emulator
{
    private readonly List<Instruction> program;
    private readonly Memory mem;
    private readonly int[] x;

    public Emulator(List<Instruction> program, Memory mem)
    {
        this.program = program;
        this.mem = mem;
        this.x = new int[32];
    }
    public int[] Registers => x;

    public void exec()
    {
        foreach (var instr in program)
        {
            exec(instr);
        }
    }

    private void exec(Instruction instr)
    {
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

            default:
                throw new NotImplementedException($"Operation {instr.opcode} not implemented yet.");
        }
    }
}