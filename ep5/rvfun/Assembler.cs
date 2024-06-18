
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

    public void asm(Mnemonics operation, int dst, int src1, int src2)
    {
        switch (operation)
        {
            case Mnemonics.add:
                asmR(0b0110011, 0, 0, dst, src1, src2); break;
            case Mnemonics.addi:
                asmI(0b0010011, 0b000, dst, src1, src2); break;
            case Mnemonics.mul:
                asmR(0b0110011, 0b000, 0b0000001, dst, src1, src2); break;
            case Mnemonics.slti:
                asmI(0b0010011, 0b010, dst, src1, src2); break;
            default:
                throw new InvalidOperationException($"Unknown instruction {operation}.");
        }
    }

    // Assembles a Risc-V I-type instruction.

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

    // Assembles a Risc-V R-type instruction.
    private void asmR(uint opcode, uint func3, uint func7, int dst, int src1, int src2)
    {
        uint uInstr = opcode;
        uInstr |= (func3 << 12);
        uInstr |= (func7 << 25);
        uInstr |= (uint) (dst & 0b11111) << 7;
        uInstr |= (uint) (src1 & 0b11111) << 15;
        uInstr |= (uint) (src2 & 0b11111) << 20;
        memory.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

}

