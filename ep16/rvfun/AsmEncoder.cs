
namespace rvfun;

public class AsmEncoder
{

    private static readonly BitField bf1L4 = new BitField(1, 4);
    private static readonly BitField bf5L6 = new BitField(5, 6);
    private static readonly BitField bf11L1 = new BitField(11, 1);
    private static readonly BitField bf12L1 = new BitField(12, 1);


    private static readonly BitField bf1L10 = new BitField(1, 10);
    private static readonly BitField bf12L8 = new BitField(12, 8);
    private static readonly BitField bf20L1 = new BitField(20, 1);



    public static uint EncodeBdisplacement(int offset)
    {
        uint uOffset = (uint)offset;
        uint encodedDisplacement = 0;
        var bits1_4 = bf1L4.ExtractUnsigned(uOffset);
        var bits5_10 = bf5L6.ExtractUnsigned(uOffset);
        var bit11 = bf11L1.ExtractUnsigned(uOffset);
        var bit12 = bf12L1.ExtractUnsigned(uOffset);
        encodedDisplacement |= bits1_4 << 8;
        encodedDisplacement |= bits5_10 << 25;
        encodedDisplacement |= bit11 << 7;
        encodedDisplacement |= bit12 << 31;
        return encodedDisplacement;
    }

    public static uint EncodeJdisplacement(int offset)
    {
        uint uOffset = (uint)offset;
        uint encodedDisplacement = 0;
        var bits1_10 = bf1L10.ExtractUnsigned(uOffset);
        var bit11 = bf11L1.ExtractUnsigned(uOffset);
        var bits12_19 = bf12L8.ExtractUnsigned(uOffset);
        var bit20 = bf20L1.ExtractUnsigned(uOffset);
        encodedDisplacement |= bits1_10 << 21;
        encodedDisplacement |= bit11 << 20;
        encodedDisplacement |= bits12_19 << 12;
        encodedDisplacement |= bit20 << 31;
        return encodedDisplacement;
    }

    public static uint EncodeUdisplacement(int src1)
    {
        return (uint)src1 << 12;
    }

    public static uint EncodeIdisplacement(int src2)
    {
        return (uint)src2 << 20;
    }

    public static uint AsmB(uint opcode, int src1, int src2, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uInstr |= EncodeBdisplacement(offset);
        return uInstr;
    }

    public static uint AsmI(uint opcode, int dst, int src1, int src2)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= EncodeIdisplacement(src2);
        return uInstr;
    }

    public static uint AsmJ(uint opcode, int rd, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(rd & 0b11111) << 7;
        uInstr |= EncodeJdisplacement(offset);
        return uInstr;
    }

    public static uint AsmF(uint opcode, int dst, int src1, int roundingMode)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(roundingMode & 0b111) << 12;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        return uInstr;
    }

    public static uint AsmF(uint opcode, int dst, int src1, int src2, int roundingMode)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(roundingMode & 0b111) << 12;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        return uInstr;
    }

    public static uint AsmR(uint opcode, int dst, int src1, int src2)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        return uInstr;
    }

    public static uint AsmR4(uint opcode, int dst, int src1, int src2, int src3, int roundingMode)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(roundingMode & 0b111) << 12;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uInstr |= (uint)(src3 & 0b11111) << 27;
        return uInstr;
    }

    public static uint AsmS(uint opcode, int src2, int baseReg, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(offset & 0b11111) << 7;
        uInstr |= (uint)(baseReg & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uInstr |= (uint)(offset >> 5) << 25;
        return uInstr;
    }

    public static uint AsmU(uint opcode, int dst, int src1)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= AsmEncoder.EncodeUdisplacement(src1);
        return uInstr;
    }

}