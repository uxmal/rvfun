namespace rvfun;

public class Instruction
{
    public readonly int opcode;
    public readonly int dst;
    public readonly int src1;
    public readonly int src2;

    public Instruction(int opcode, int dst, int src1, int src2)
    {
        this.opcode = opcode;
        this.dst = dst;
        this.src1 = src1;
        this.src2 = src2;
    }
}