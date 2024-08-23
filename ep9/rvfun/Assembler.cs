using System.Text;

namespace rvfun;

public class Assembler
{
    private Memory memory;
    private uint instrPtr;

    private static readonly BitField bf1L4 = new BitField(1, 4);
    private static readonly BitField bf5L6 = new BitField(5, 6);
    private static readonly BitField bf11L1 = new BitField(11, 1);
    private static readonly BitField bf12L1 = new BitField(12, 1);

    private static readonly BitField bf1L10 = new BitField(1, 10);
    private static readonly BitField bf12L8 = new BitField(12, 8);
    private static readonly BitField bf20L1 = new BitField(20, 1);

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
            case Mnemonics.auipc:
                asmU(0b0010111, dst, src1); break;
            case Mnemonics.bge:
                asmB(0b1100011, 0b101, dst, src1, src2); break;
            case Mnemonics.bne:
                asmB(0b1100011, 0b001, dst, src1, src2); break;
            case Mnemonics.ecall:
                asmI(0b1110011, 0b000, 0, 0, 0); break;
            case Mnemonics.jal:
                asmJ(0b1101111, dst, src1); break;
            case Mnemonics.jalr:
                asmI(0b1100111, 0b000, dst, src1, src2); break;
            case Mnemonics.lb:
                asmI(0b0000011, 0b000, dst, src1, src2); break;
            case Mnemonics.lbu:
                asmI(0b0000011, 0b100, dst, src1, src2); break;
            case Mnemonics.lh:
                asmI(0b0000011, 0b001, dst, src1, src2); break;
            case Mnemonics.lhu:
                asmI(0b0000011, 0b101, dst, src1, src2); break;
            case Mnemonics.lui:
                asmU(0b0110111, dst, src1); break;
            case Mnemonics.lw:
                asmI(0b0000011, 0b010, dst, src1, src2); break;
            case Mnemonics.mul:
                asmR(0b0110011, 0b000, 0b0000001, dst, src1, src2); break;
            case Mnemonics.sb:
                asmS(0b0100011, 0b000, dst, src1, src2); break;
            case Mnemonics.sh:
                asmS(0b0100011, 0b001, dst, src1, src2); break;
            case Mnemonics.slti:
                asmI(0b0010011, 0b010, dst, src1, src2); break;
            case Mnemonics.sw:
                asmS(0b0100011, 0b010, dst, src1, src2); break;

            case Mnemonics.Invalid:
                memory.WriteLeWord32(instrPtr, 0);
                instrPtr += 4;
                break;
            default:
                throw new InvalidOperationException($"Unknown instruction {operation}.");
        }
    }

    private void asmJ(uint opcode, int rd, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(rd & 0b11111) << 7;
        uint uOffset = (uint) offset;
        var bits1_10 = bf1L10.ExtractUnsigned(uOffset);
        var bit11 = bf11L1.ExtractUnsigned(uOffset);
        var bits12_19 = bf12L8.ExtractUnsigned(uOffset);
        var bit20 = bf20L1.ExtractUnsigned(uOffset);
        uInstr |= bits1_10 << 21;
        uInstr |= bit11 << 20;
        uInstr |= bits12_19 << 12;
        uInstr |= bit20 << 31;
        memory.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    private void asmB(uint opcode, uint func3, int src1, int src2, int offset)
    {
        uint uInstr = opcode;
        uInstr |= func3 << 12;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uint uOffset = (uint) offset;
        var bits1_4 = bf1L4.ExtractUnsigned(uOffset);
        var bits5_10 = bf5L6.ExtractUnsigned(uOffset);
        var bit11 = bf11L1.ExtractUnsigned(uOffset);
        var bit12 = bf12L1.ExtractUnsigned(uOffset);
        uInstr |= bits1_4 << 8;
        uInstr |= bits5_10 << 25;
        uInstr |= bit11 << 7;
        uInstr |= bit12 << 31;
        memory.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    private void asmS(uint opcode, uint func3, int src2, int baseReg, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(offset & 0b11111) << 7;
        uInstr |= func3 << 12;
        uInstr |= (uint)(baseReg & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uInstr |= (uint)(offset >> 5) << 25;
        memory.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
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

        // Assembles a Risc-V I-type instruction.

    private void asmU(uint opcode, int dst, int src1)
    {
        uint uInstr = opcode;
        uInstr |= (uint) (dst & 0b11111) << 7;
        uInstr |= (uint) src1 << 12;
        memory.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    public void ds(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        memory.WriteBytes(instrPtr, bytes);
        instrPtr += (uint)bytes.Length;
    }
}

