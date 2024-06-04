int[] x = new int[32];

const int addi = 1;
const int muli = 2;
const int add = 3;
const int mul = 4;

var program = new List<Instruction>();
asm(addi, 1, 0, 3);
asm(addi, 2, 0, -4);
asm(addi, 3, 0, 2);

asm(muli, 4, 1, 3);
asm(mul, 5, 2, 3);
asm(add, 4, 4, 5);
asm(addi, 6, 4, -1);


foreach (var instr in program)
{
    exec(instr);
}


Console.WriteLine(x[6]);

void asm(int operation, int dst, int src1, int src2)
{
    var instr = new Instruction(operation, dst, src1, src2);
    program.Add(instr);
}


void exec(Instruction instr)
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
        default:
            throw new NotImplementedException($"Operation {instr.opcode} not implemented yet.");
    }
}

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