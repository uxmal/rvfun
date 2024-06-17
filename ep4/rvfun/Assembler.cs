
namespace rvfun;

public class Assembler
{
    private Memory memory;
    private uint instrPtr;

    public Assembler(Memory memory)
    {
        this.memory = memory;
        this.instrPtr = 0;
    }

    public void asm(int operation, int dst, int src1, int src2)
    {
        switch (operation)
        {
            case Mnemonics.addi:
                asmI(0b0010011, 0b000, dst, src1, src2);
                break;
            default:
                throw new InvalidOperationException($"Unknown instruction {operation}.");
        }
    }

    private void asmI(uint opcode, uint func3, int dst, int src1, int src2)
    {
        uint uInstr = opcode;
        uInstr |= (func3 << 12);
        uInstr |= (uint) (dst & 0b11111) << 7;
        uInstr |= (uint) (src1 & 0b11111) << 15;
        uInstr |= (uint) src2 << 20;
        memory.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }
}

