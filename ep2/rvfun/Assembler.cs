namespace rvfun;

public class Assembler
{
    private readonly List<Instruction> instructions;

    public Assembler()
    {
        this.instructions = new List<Instruction>();
    }

    public void asm(int operation, int dst, int src1, int src2)
    {
        var instr = new Instruction(operation, dst, src1, src2);
        instructions.Add(instr);
    }

    public List<Instruction> ToProgram() => instructions;

}

