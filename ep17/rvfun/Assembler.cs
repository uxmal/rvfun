using System.Text;

namespace rvfun;

public class Assembler
{
    private uint instrPtr;

    public Assembler(Logger logger)
    {
        this.Section = new AssemblerSection();
        this.instrPtr = 0;
        this.Symbols = new();
        this.Relocations = new();
        this.Logger = logger;
    }

    public AssemblerSection Section {get;}
    public Dictionary<string, Symbol> Symbols {get; }
    public List<Relocation> Relocations {get;}
    public uint Position => instrPtr;
    public Logger Logger {get;}

    public Symbol? AddSymbol(string name, uint value)
    {
        if (Symbols.ContainsKey(name))
        {
            Logger.ReportError($"error: label {name} was redefined.");
            return null;
        }
        var sym = new Symbol(name, value);
        Symbols.Add(name, sym);
        return sym;
    }

            public void add(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_000_00000_0110011, dst, src1, src2);
        }

            public void addi(int dst, int src1, int src2) {
                asmI(0b000_00000_0010011, dst, src1, src2);
            }

    public void addi(int dst, int src1, Relocation rel) {
        Relocations.Add(rel);
        asmI(0b000_00000_0010011, dst, src1, 0);
    }

            public void addiw(int dst, int src1, int src2) {
                asmI(0b000_00000_0011011, dst, src1, src2);
    }

            public void addw(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_000_00000_0111011, dst, src1, src2);
    }

            public void and(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_111_00000_0110011, dst, src1, src2);
    }

            public void andi(int dst, int src1, int src2) {
                asmI(0b111_00000_0010011, dst, src1, src2);
    }

            public void auipc(int dst, int src1) {
                asmU(0b0010111, dst, src1);
            }

            public void auipc(int dst, Relocation rel) {
                Relocations.Add(rel);
                asmU(0b0010111, dst, 0);
            }

            public void beq(int dst, int src1, int src2) {
                asmB(0b000_00000_1100011, dst, src1, src2);
    }

            public void bge(int dst, int src1, int src2) {
                asmB(0b101_00000_1100011, dst, src1, src2);
    }

            public void bgeu(int dst, int src1, int src2) {
                asmB(0b111_00000_1100011, dst, src1, src2);
    }

            public void blt(int dst, int src1, int src2) {
                asmB(0b100_00000_1100011, dst, src1, src2);
    }

            public void bltu(int dst, int src1, int src2) {
                asmB(0b110_00000_1100011, dst, src1, src2);
    }

            public void bne(int dst, int src1, int src2) {
                asmB(0b001_00000_1100011, dst, src1, src2);
    }

        public void beq(int dst, int src1, string target) {
                EmitRelocation(instrPtr, RelocationType.B_PcRelative, target);
                asmB(0b000_00000_1100011, dst, src1, 0);
    }

            public void bge(int dst, int src1, string target) {
                EmitRelocation(instrPtr, RelocationType.B_PcRelative, target);
                asmB(0b101_00000_1100011, dst, src1, 0);
    }

            public void bgeu(int dst, int src1, string target) {
                EmitRelocation(instrPtr, RelocationType.B_PcRelative, target);
                asmB(0b111_00000_1100011, dst, src1, 0);
    }

              public void blt(int dst, int src1, string target) {
                EmitRelocation(instrPtr, RelocationType.B_PcRelative, target);
                asmB(0b100_00000_1100011, dst, src1, 0);
    }


    
            public void bltu(int dst, int src1, string target) {
                EmitRelocation(instrPtr, RelocationType.B_PcRelative, target);
                asmB(0b110_00000_1100011, dst, src1, 0);
    }

            public void bne(int dst, int src1, string target) {
                EmitRelocation(instrPtr, RelocationType.B_PcRelative, target);
                asmB(0b001_00000_1100011, dst, src1, 0);
    }


            public void csrrc(int dst, int src1, int src2) {
                asmI(0b011_00000_1110011, dst, src1, src2);
    }

            public void csrrci(int dst, int src1, int src2) {
                asmI(0b111_00000_1110011, dst, src1, src2);
    }

            public void csrrs(int dst, int src1, int src2) {
                asmI(0b010_00000_1110011, dst, src1, src2);
    }

            public void csrrsi(int dst, int src1, int src2) {
                asmI(0b110_00000_1110011, dst, src1, src2);
    }

            public void csrrw(int dst, int src1, int src2) {
                asmI(0b001_00000_1110011, dst, src1, src2);
    }

            public void csrrwi(int dst, int src1, int src2) {
                asmI(0b101_00000_1110011, dst, src1, src2);
    }

            public void div(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_100_00000_0110011, dst, src1, src2);
    }

            public void divu(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_101_00000_0110011, dst, src1, src2);
    }

            public void divuw(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_101_00000_0111011, dst, src1, src2);
    }

            public void divw(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_100_00000_0111011, dst, src1, src2);
    }

            public void ebreak(int dst, int src1, int src2) {
                asmI(0b000_00000_1110011, 0, 0, 1);
    }

            public void ecall() {
                asmI(0b000_00000_1110011, 0, 0, 0);
    }

            public void jal(int dst, int src1) {
                asmJ(0b1101111, dst, src1);
    }

            public void j(string target) {
                EmitRelocation(instrPtr, RelocationType.J_PcRelative, target);
                asmJ(0b1101111, 0, 0);
            }


            public void jal(int dst, string target) {
                EmitRelocation(instrPtr, RelocationType.J_PcRelative, target);
                asmJ(0b1101111, dst, 0);
            }

    public void jalr(int dst, int src1, int src2) {
                asmI(0b000_00000_1100111, dst, src1, src2);
    }

            public void lb(int dst, int src1, int src2) {
                asmI(0b000_00000_0000011, dst, src1, src2);
    }

            public void ld(int dst, int src1, int src2) {
                asmI(0b011_00000_0000011, dst, src1, src2);
    }

            public void lbu(int dst, int src1, int src2) {
                asmI(0b100_00000_0000011, dst, src1, src2);
    }

            public void lh(int dst, int src1, int src2) {
                asmI(0b001_00000_0000011, dst, src1, src2);
    }

    public void li(int dst, int imm)
    {
        asmI(0b000_00000_0010011, dst, 0, imm);
    }
            public void lhu(int dst, int src1, int src2) {
                asmI(0b101_00000_0000011, dst, src1, src2);
    }

            public void lui(int dst, int src1, int src2) {
                asmU(0b0110111, dst, src1);
    }

            public void lw(int dst, int src1, int src2) {
                asmI(0b010_00000_0000011, dst, src1, src2);
    }

            public void lwu(int dst, int src1, int src2) {
                asmI(0b110_00000_0000011, dst, src1, src2);
    }

            public void mul(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_000_00000_0110011, dst, src1, src2);
    }

            public void mulh(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_001_00000_0110011, dst, src1, src2);
    }

            public void mulhsu(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_010_00000_0110011, dst, src1, src2);
    }

            public void mulhu(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_011_00000_0110011, dst, src1, src2);
    }

            public void mulw(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_000_00000_0111011, dst, src1, src2);
    }

            public void rem(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_110_00000_0110011, dst, src1, src2);
    }

            public void remu(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_111_00000_0110011, dst, src1, src2);
    }

            public void remuw(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_111_00000_0111011, dst, src1, src2);
    }

            public void remw(int dst, int src1, int src2) {
                asmR(0b0000001_0000000000_110_00000_0111011, dst, src1, src2);
    }

            public void or(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_110_00000_0110011, dst, src1, src2);
    }

            public void ori(int dst, int src1, int src2) {
                asmI(0b110_00000_0010011, dst, src1, src2);
    }

            public void sb(int dst, int src1, int src2) {
                asmS(0b000_00000_0100011, dst, src1, src2);
    }

            public void sd(int dst, int src1, int src2) {
                asmS(0b011_00000_0100011, dst, src1, src2);
    }

            public void sh(int dst, int src1, int src2) {
                asmS(0b001_00000_0100011, dst, src1, src2);
    }

            public void slt(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_010_00000_0110011, dst, src1, src2);
    }

            public void slti(int dst, int src1, int src2) {
                asmI(0b010_00000_0010011, dst, src1, src2);
    }

            public void sltiu(int dst, int src1, int src2) {
                asmI(0b011_00000_0010011, dst, src1, src2);
    }

            public void sltu(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_011_00000_0110011, dst, src1, src2);
    }

            public void sll(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_001_00000_0110011, dst, src1, src2);
    }

            public void slli(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_001_00000_0010011, dst, src1, src2);
    }

            public void slliw(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_001_00000_0011011, dst, src1, src2);
    }

            public void sllw(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_001_00000_0111011, dst, src1, src2);
    }

            public void sra(int dst, int src1, int src2) {
                asmR(0b0100000_0000000000_101_00000_0110011, dst, src1, src2);
    }

            public void srai(int dst, int src1, int src2) {
                asmR(0b0100000_0000000000_101_00000_0010011, dst, src1, src2);
    }

            public void sraiw(int dst, int src1, int src2) {
                asmR(0b0100000_0000000000_101_00000_0011011, dst, src1, src2);
    }

            public void sraw(int dst, int src1, int src2) {
                asmR(0b0100000_0000000000_101_00000_0111011, dst, src1, src2);
    }

            public void srli(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_101_00000_0010011, dst, src1, src2);
    }

            public void srliw(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_101_00000_0011011, dst, src1, src2);
    }

            public void srl(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_101_00000_0110011, dst, src1, src2);
    }

            public void srlw(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_101_00000_0111011, dst, src1, src2);
    }

            public void sub(int dst, int src1, int src2) {
                asmR(0b0100000_0000000000_000_00000_0110011, dst, src1, src2);
    }

            public void subw(int dst, int src1, int src2) {
                asmR(0b0100000_0000000000_000_00000_0111011, dst, src1, src2);
    }

            public void sw(int dst, int src1, int src2) {
                asmS(0b010_00000_0100011, dst, src1, src2);
    }

                //zawrs standard/extension/
            public void wrs_nto() {
                asmR(0b0000000_00000_00000_000_00000_1110011, 0, 0, 0b01101);
             }

            public void wrs_sto() {
                asmR(0b0000000_00000_00000_000_00000_1110011, 0, 0, 0b11101);
            }


            public void xor(int dst, int src1, int src2) {
                asmR(0b0000000_0000000000_100_00000_0110011, dst, src1, src2);
    }

            public void xori(int dst, int src1, int src2) {
                asmI(0b100_00000_0010011, dst, src1, src2);
    }


            public void flw(int dst, int src1, int src2) {
                        asmI(0b010_00000_0000111, dst, src1, src2);
}
            public void fsw(int dst, int src1, int src2) {
                        asmS(0b010_00000_0100111, dst, src1, src2);
}
            public void fmadd_s(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b00_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm);
}
            public void fmsub_s(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b00_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm);
}
            public void fnmsub_s(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b00_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm);
}
            public void fnmadd_s(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b00_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm);
}
            public void fadd_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0000000_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsub_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0000100_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fmul_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0001000_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fdiv_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0001100_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsqrt_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0101100_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fsgnj_s(int dst, int src1, int src2) {
                        asmR(0b0010000_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fsgnjn_s(int dst, int src1, int src2) {
                        asmR(0b0010000_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fsgnjx_s(int dst, int src1, int src2) {
                        asmR(0b0010000_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void fmin_s(int dst, int src1, int src2) {
                        asmR(0b0010100_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fmax_s(int dst, int src1, int src2) {
                        asmR(0b0010100_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fcvt_w_s(int dst, int src1, int src2, int rm) {
                        asmF(0b1100000_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_wu_s(int dst, int src1, int src2, int rm) {
                        asmF(0b1100000_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fmv_x_w(int dst, int src1, int src2) {
                        asmR(0b1110000_00000_00000_000_00000_1010011, dst, src1, 0);
}
            public void feq_s(int dst, int src1, int src2) {
                        asmR(0b1010000_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void flt_s(int dst, int src1, int src2) {
                        asmR(0b1010000_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fle_s(int dst, int src1, int src2) {
                        asmR(0b1010000_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fclass_s(int dst, int src1, int src2) {
                        asmR(0b1110000_00000_00000_001_00000_1010011, dst, src1, 00000);
}
            public void fcvt_s_w(int dst, int src1, int src2, int rm) {
                        asmF(0b1101000_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_s_wu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101000_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fmv_w_x(int dst, int src1, int src2) {
                        asmR(0b1111000_00000_00000_000_00000_1010011, dst, src1, 00000);
}
            public void fcvt_l_s(int dst, int src1, int src2, int rm) {
                        asmF(0b1100000_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_lu_s(int dst, int src1, int src2, int rm) {
                        asmF(0b1100000_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_s_l(int dst, int src1, int src2, int rm) {
                        asmF(0b1101000_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_s_lu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101000_00011_00000_000_00000_1010011, dst, src1, rm);
}

            public void fld(int dst, int src1, int src2) {
                        asmI(0b011_00000_0000111, dst, src1, src2);
}
            public void fsd(int dst, int src1, int src2) {
                        asmS(0b011_00000_0100111, dst, src1, src2);
}
            public void fmadd_d(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b01_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm);
}
            public void fmsub_d(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b01_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm);
}
            public void fnmsub_d(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b01_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm);
}
            public void fnmadd_d(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b01_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm);
}
            public void fadd_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0000001_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsub_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0000101_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fmul_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0001001_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fdiv_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0001101_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsqrt_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0101101_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fsgnj_d(int dst, int src1, int src2) {
                        asmR(0b0010001_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fsgnjn_d(int dst, int src1, int src2) {
                        asmR(0b0010001_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fsgnjx_d(int dst, int src1, int src2) {
                        asmR(0b0010001_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void fmin_d(int dst, int src1, int src2) {
                        asmR(0b0010101_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fmax_d(int dst, int src1, int src2) {
                        asmR(0b0010101_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fcvt_s_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0100000_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_d_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0100001_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void feq_d(int dst, int src1, int src2) {
                        asmR(0b1010001_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void flt_d(int dst, int src1, int src2) {
                        asmR(0b1010001_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fle_d(int dst, int src1, int src2) {
                        asmR(0b1010001_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fclass_d(int dst, int src1, int src2) {
                        asmR(0b1110001_00000_00000_001_00000_1010011, dst, src1, 00000);
}
            public void fcvt_w_d(int dst, int src1, int src2, int rm) {
                        asmF(0b1100001_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_wu_d(int dst, int src1, int src2, int rm) {
                        asmF(0b1100001_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_d_w(int dst, int src1, int src2, int rm) {
                        asmF(0b1101001_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_d_wu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101001_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_l_d(int dst, int src1, int src2, int rm) {
                        asmF(0b1100001_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_lu_d(int dst, int src1, int src2, int rm) {
                        asmF(0b1100001_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void fmv_x_d(int dst, int src1, int src2) {
                        asmR(0b1110001_00000_00000_000_00000_1010011, dst, src1, 00000);
}
            public void fcvt_d_l(int dst, int src1, int src2, int rm) {
                        asmF(0b1101001_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_d_lu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101001_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void fmv_d_x(int dst, int src1, int src2) {
                        asmR(0b1111001_00000_00000_000_00000_1010011, dst, src1, 00000);
}

            public void flq(int dst, int src1, int src2) {
                        asmI(0b100_00000_0000111, dst, src1, src2);
}
            public void fsq(int dst, int src1, int src2) {
                        asmS(0b100_00000_0100111, dst, src1, src2);
}
            public void fmadd_q(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b11_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm);
}
            public void fmsub_q(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b11_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm);
}
            public void fnmsub_q(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b11_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm);
}
            public void fnmadd_q(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b11_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm);
}
            public void fadd_q(int dst, int src1, int src2, int rm) {
                        asmF(0b0000011_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsub_q(int dst, int src1, int src2, int rm) {
                        asmF(0b0000111_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fmul_q(int dst, int src1, int src2, int rm) {
                        asmF(0b0001011_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fdiv_q(int dst, int src1, int src2, int rm) {
                        asmF(0b0001111_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsqrt_q(int dst, int src1, int src2, int rm) {
                        asmF(0b0101111_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fsgnj_q(int dst, int src1, int src2) {
                        asmR(0b0010011_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fsgnjn_q(int dst, int src1, int src2) {
                        asmR(0b0010011_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fsgnjx_q(int dst, int src1, int src2) {
                        asmR(0b0010011_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void fmin_q(int dst, int src1, int src2) {
                        asmR(0b0010111_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fmax_q(int dst, int src1, int src2) {
                        asmR(0b0010111_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fcvt_s_q(int dst, int src1, int src2, int rm) {
                        asmF(0b0100000_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_q_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0100011_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_d_q(int dst, int src1, int src2, int rm) {
                        asmF(0b0100001_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_q_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0100011_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void feq_q(int dst, int src1, int src2) {
                        asmR(0b1010011_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void flt_q(int dst, int src1, int src2) {
                        asmR(0b1010011_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fle_q(int dst, int src1, int src2) {
                        asmR(0b1010011_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fclass_q(int dst, int src1, int src2) {
                        asmR(0b1110011_00000_00000_001_00000_1010011, dst, src1, 00000);
}
            public void fcvt_w_q(int dst, int src1, int src2, int rm) {
                        asmF(0b1100011_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_wu_q(int dst, int src1, int src2, int rm) {
                        asmF(0b1100011_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_q_w(int dst, int src1, int src2, int rm) {
                        asmF(0b1101011_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_q_wu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101011_00001_00000_000_00000_1010011, dst, src1, rm);
}

            public void fcvt_l_q(int dst, int src1, int src2, int rm) {
                        asmF(0b1100011_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_lu_q(int dst, int src1, int src2, int rm) {
                        asmF(0b1100011_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_q_l(int dst, int src1, int src2, int rm) {
                        asmF(0b1101011_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_q_lu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101011_00011_00000_000_00000_1010011, dst, src1, rm);
}

            public void flh(int dst, int src1, int src2) {
                        asmI(0b001_00000_0000111, dst, src1, src2);
}
            public void fsh(int dst, int src1, int src2) {
                        asmS(0b001_00000_0100111, dst, src1, src2);
}
            public void fmadd_h(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b10_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm);
}
            public void fmsub_h(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b10_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm);
}
            public void fnmsub_h(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b10_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm);
}
            public void fnmadd_h(int dst, int src1, int src2, int src3, int rm) {
                        asmR4(0b10_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm);
}
            public void fadd_h(int dst, int src1, int src2, int rm) {
                        asmF(0b0000010_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsub_h(int dst, int src1, int src2, int rm) {
                        asmF(0b0000110_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fmul_h(int dst, int src1, int src2, int rm) {
                        asmF(0b0001010_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fdiv_h(int dst, int src1, int src2, int rm) {
                        asmF(0b0001110_00000_00000_000_00000_1010011, dst, src1, src2, rm);
}
            public void fsqrt_h(int dst, int src1, int rm) {
                        asmF(0b0101110_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fsgnj_h(int dst, int src1, int src2) {
                        asmR(0b0010010_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fsgnjn_h(int dst, int src1, int src2) {
                        asmR(0b0010010_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fsgnjx_h(int dst, int src1, int src2) {
                        asmR(0b0010010_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void fmin_h(int dst, int src1, int src2) {
                        asmR(0b0010110_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fmax_h(int dst, int src1, int src2) {
                        asmR(0b0010110_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fcvt_s_h(int dst, int src1, int src2, int rm) {
                        asmF(0b0100000_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_h_s(int dst, int src1, int src2, int rm) {
                        asmF(0b0100010_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_d_h(int dst, int src1, int src2, int rm) {
                        asmF(0b0100001_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_h_d(int dst, int src1, int src2, int rm) {
                        asmF(0b0100010_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_q_h(int dst, int src1, int rm) {
                        asmF(0b0100011_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_h_q(int dst, int src1, int rm) {
                        asmF(0b0100010_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void feq_h(int dst, int src1, int src2) {
                        asmR(0b1010010_00000_00000_010_00000_1010011, dst, src1, src2);
}
            public void flt_h(int dst, int src1, int src2) {
                        asmR(0b1010010_00000_00000_001_00000_1010011, dst, src1, src2);
}
            public void fle_h(int dst, int src1, int src2) {
                        asmR(0b1010010_00000_00000_000_00000_1010011, dst, src1, src2);
}
            public void fclass_h(int dst, int src1, int src2) {
                        asmR(0b1110010_00000_00000_001_00000_1010011, dst, src1, 00000);
}
            public void fcvt_w_h(int dst, int src1, int src2, int rm) {
                        asmF(0b1100010_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_wu_h(int dst, int src1, int src2, int rm) {
                        asmF(0b1100010_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fmv_x_h(int dst, int src1, int src2) {
                        asmR(0b1110010_00000_00000_000_00000_1010011, dst, src1, 00000);
}
            public void fcvt_h_w(int dst, int src1, int src2, int rm) {
                        asmF(0b1101010_00000_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_h_wu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101010_00001_00000_000_00000_1010011, dst, src1, rm);
}
            public void fmv_h_x(int dst, int src1, int src2) {
                        asmR(0b1111010_00000_00000_000_00000_1010011, dst, src1, 00000);
}
            public void fcvt_l_h(int dst, int src1, int src2, int rm) {
                        asmF(0b1100010_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_lu_h(int dst, int src1, int src2, int rm) {
                        asmF(0b1100010_00011_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_h_l(int dst, int src1, int src2, int rm) {
                        asmF(0b1101010_00010_00000_000_00000_1010011, dst, src1, rm);
}
            public void fcvt_h_lu(int dst, int src1, int src2, int rm) {
                        asmF(0b1101010_00011_00000_000_00000_1010011, dst, src1, rm);
}




    //$TODO: this method doesn't seem to be used anywhere anymore. Delete it?
    public void asm(Mnemonics operation, int dst, int src1, int src2, int src3 = 0, int rm = 0)
    {
        switch (operation)
        {
            case Mnemonics.add:
                asmR(0b0000000_0000000000_000_00000_0110011, dst, src1, src2); break;
            case Mnemonics.addi:
                asmI(0b000_00000_0010011, dst, src1, src2); break;
            case Mnemonics.addiw:
                asmI(0b000_00000_0011011, dst, src1, src2); break;
            case Mnemonics.addw:
                asmR(0b0000000_0000000000_000_00000_0111011, dst, src1, src2); break;
            case Mnemonics.and:
                asmR(0b0000000_0000000000_111_00000_0110011, dst, src1, src2); break;
            case Mnemonics.andi:
                asmI(0b111_00000_0010011, dst, src1, src2); break;
            case Mnemonics.auipc:
                asmU(0b0010111, dst, src1); break;
            case Mnemonics.beq:
                asmB(0b000_00000_1100011, dst, src1, src2); break;
            case Mnemonics.bge:
                asmB(0b101_00000_1100011, dst, src1, src2); break;
            case Mnemonics.bgeu:
                asmB(0b111_00000_1100011, dst, src1, src2); break;
            case Mnemonics.blt:
                asmB(0b100_00000_1100011, dst, src1, src2); break;
            case Mnemonics.bltu:
                asmB(0b110_00000_1100011, dst, src1, src2); break;
            case Mnemonics.bne:
                asmB(0b001_00000_1100011, dst, src1, src2); break;
            case Mnemonics.csrrc:
                asmI(0b011_00000_1110011, dst, src1, src2); break;
            case Mnemonics.csrrci:
                asmI(0b111_00000_1110011, dst, src1, src2); break;
            case Mnemonics.csrrs:
                asmI(0b010_00000_1110011, dst, src1, src2); break;
            case Mnemonics.csrrsi:
                asmI(0b110_00000_1110011, dst, src1, src2); break;
            case Mnemonics.csrrw:
                asmI(0b001_00000_1110011, dst, src1, src2); break;
            case Mnemonics.csrrwi:
                asmI(0b101_00000_1110011, dst, src1, src2); break;
            case Mnemonics.div:
                asmR(0b0000001_0000000000_100_00000_0110011, dst, src1, src2); break;
            case Mnemonics.divu:
                asmR(0b0000001_0000000000_101_00000_0110011, dst, src1, src2); break;
            case Mnemonics.divuw:
                asmR(0b0000001_0000000000_101_00000_0111011, dst, src1, src2); break;
            case Mnemonics.divw:
                asmR(0b0000001_0000000000_100_00000_0111011, dst, src1, src2); break;
            case Mnemonics.ebreak:
                asmI(0b000_00000_1110011, 0, 0, 1); break;
            case Mnemonics.ecall:
                asmI(0b000_00000_1110011, 0, 0, 0); break;
            case Mnemonics.jal:
                asmJ(0b1101111, dst, src1); break;
            case Mnemonics.jalr:
                asmI(0b000_00000_1100111, dst, src1, src2); break;
            case Mnemonics.lb:
                asmI(0b000_00000_0000011, dst, src1, src2); break;
            case Mnemonics.ld:
                asmI(0b011_00000_0000011, dst, src1, src2); break;
            case Mnemonics.lbu:
                asmI(0b100_00000_0000011, dst, src1, src2); break;
            case Mnemonics.lh:
                asmI(0b001_00000_0000011, dst, src1, src2); break;
            case Mnemonics.lhu:
                asmI(0b101_00000_0000011, dst, src1, src2); break;
            case Mnemonics.lui:
                asmU(0b0110111, dst, src1); break;
            case Mnemonics.lw:
                asmI(0b010_00000_0000011, dst, src1, src2); break;
            case Mnemonics.lwu:
                asmI(0b110_00000_0000011, dst, src1, src2); break;
            case Mnemonics.mul:
                asmR(0b0000001_0000000000_000_00000_0110011, dst, src1, src2); break;
            case Mnemonics.mulh:
                asmR(0b0000001_0000000000_001_00000_0110011, dst, src1, src2); break;
            case Mnemonics.mulhsu:
                asmR(0b0000001_0000000000_010_00000_0110011, dst, src1, src2); break;
            case Mnemonics.mulhu:
                asmR(0b0000001_0000000000_011_00000_0110011, dst, src1, src2); break;
            case Mnemonics.mulw:
                asmR(0b0000001_0000000000_000_00000_0111011, dst, src1, src2); break;
            case Mnemonics.rem:
                asmR(0b0000001_0000000000_110_00000_0110011, dst, src1, src2); break;
            case Mnemonics.remu:
                asmR(0b0000001_0000000000_111_00000_0110011, dst, src1, src2); break;
            case Mnemonics.remuw:
                asmR(0b0000001_0000000000_111_00000_0111011, dst, src1, src2); break;
            case Mnemonics.remw:
                asmR(0b0000001_0000000000_110_00000_0111011, dst, src1, src2); break;
            case Mnemonics.or:
                asmR(0b0000000_0000000000_110_00000_0110011, dst, src1, src2); break;
            case Mnemonics.ori:
                asmI(0b110_00000_0010011, dst, src1, src2); break;
            case Mnemonics.sb:
                asmS(0b000_00000_0100011, dst, src1, src2); break;
            case Mnemonics.sd:
                asmS(0b011_00000_0100011, dst, src1, src2); break;
            case Mnemonics.sh:
                asmS(0b001_00000_0100011, dst, src1, src2); break;
            case Mnemonics.slt:
                asmR(0b0000000_0000000000_010_00000_0110011, dst, src1, src2); break;
            case Mnemonics.slti:
                asmI(0b010_00000_0010011, dst, src1, src2); break;
            case Mnemonics.sltiu:
                asmI(0b011_00000_0010011, dst, src1, src2); break;
            case Mnemonics.sltu:
                asmR(0b0000000_0000000000_011_00000_0110011, dst, src1, src2); break;
            case Mnemonics.sll:
                asmR(0b0000000_0000000000_001_00000_0110011, dst, src1, src2); break;
            case Mnemonics.slli:
                asmR(0b0000000_0000000000_001_00000_0010011, dst, src1, src2); break;
            case Mnemonics.slliw:
                asmR(0b0000000_0000000000_001_00000_0011011, dst, src1, src2); break;
            case Mnemonics.sllw:
                asmR(0b0000000_0000000000_001_00000_0111011, dst, src1, src2); break;
            case Mnemonics.sra:
                asmR(0b0100000_0000000000_101_00000_0110011, dst, src1, src2); break;
            case Mnemonics.srai:
                asmR(0b0100000_0000000000_101_00000_0010011, dst, src1, src2); break;
            case Mnemonics.sraiw:
                asmR(0b0100000_0000000000_101_00000_0011011, dst, src1, src2); break;
            case Mnemonics.sraw:
                asmR(0b0100000_0000000000_101_00000_0111011, dst, src1, src2); break;
            case Mnemonics.srli:
                asmR(0b0000000_0000000000_101_00000_0010011, dst, src1, src2); break;
            case Mnemonics.srliw:
                asmR(0b0000000_0000000000_101_00000_0011011, dst, src1, src2); break;
            case Mnemonics.srl:
                asmR(0b0000000_0000000000_101_00000_0110011, dst, src1, src2); break;
            case Mnemonics.srlw:
                asmR(0b0000000_0000000000_101_00000_0111011, dst, src1, src2); break;
            case Mnemonics.sub:
                asmR(0b0100000_0000000000_000_00000_0110011, dst, src1, src2); break;
            case Mnemonics.subw:
                asmR(0b0100000_0000000000_000_00000_0111011, dst, src1, src2); break;
            case Mnemonics.sw:
                asmS(0b010_00000_0100011, dst, src1, src2); break;
            case Mnemonics.xor:
                asmR(0b0000000_0000000000_100_00000_0110011, dst, src1, src2); break;
            case Mnemonics.xori:
                asmI(0b100_00000_0010011, dst, src1, src2); break;

            // 00010 aq rl 00000 rs1 010 rd 0101111 lr_w
            // 00011 aq rl rs2 rs1 010 rd 0101111 sc_w
            // 00001 aq rl rs2 rs1 010 rd 0101111 amoswap_w
            // 00000 aq rl rs2 rs1 010 rd 0101111 amoadd_w
            // 00100 aq rl rs2 rs1 010 rd 0101111 amoxor_w
            // 01100 aq rl rs2 rs1 010 rd 0101111 amoand_w
            // 01000 aq rl rs2 rs1 010 rd 0101111 amoor_w
            // 10000 aq rl rs2 rs1 010 rd 0101111 amomin_w
            // 10100 aq rl rs2 rs1 010 rd 0101111 amomax_w
            // 11000 aq rl rs2 rs1 010 rd 0101111 amominu_w
            // 11100 aq rl rs2 rs1 010 rd 0101111 amomaxu_w
            // 00010 aq rl 00000 rs1 011 rd 0101111 lr_d
            // 00011 aq rl rs2 rs1 011 rd 0101111 sc_d
            // 00001 aq rl rs2 rs1 011 rd 0101111 amoswap_d
            // 00000 aq rl rs2 rs1 011 rd 0101111 amoadd_d
            // 00100 aq rl rs2 rs1 011 rd 0101111 amoxor_d
            // 01100 aq rl rs2 rs1 011 rd 0101111 amoand_d
            // 01000 aq rl rs2 rs1 011 rd 0101111 amoor_d
            // 10000 aq rl rs2 rs1 011 rd 0101111 amomin_d
            // 10100 aq rl rs2 rs1 011 rd 0101111 amomax_d
            // 11000 aq rl rs2 rs1 011 rd 0101111 amominu_d
            // 11100 aq rl rs2 rs1 011 rd 0101111 amomaxu_d

            case Mnemonics.flw:
                asmI(0b010_00000_0000111, dst, src1, src2); break;
            case Mnemonics.fsw:
                asmS(0b010_00000_0100111, dst, src1, src2); break;
            case Mnemonics.fmadd_s:
                asmR4(0b00_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fmsub_s:
                asmR4(0b00_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmsub_s:
                asmR4(0b00_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmadd_s:
                asmR4(0b00_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fadd_s:
                asmF(0b0000000_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsub_s:
                asmF(0b0000100_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fmul_s:
                asmF(0b0001000_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fdiv_s:
                asmF(0b0001100_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsqrt_s:
                asmF(0b0101100_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fsgnj_s:
                asmR(0b0010000_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjn_s:
                asmR(0b0010000_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjx_s:
                asmR(0b0010000_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmin_s:
                asmR(0b0010100_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmax_s:
                asmR(0b0010100_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fcvt_w_s:
                asmF(0b1100000_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_wu_s:
                asmF(0b1100000_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fmv_x_w:
                asmR(0b1110000_00000_00000_000_00000_1010011, dst, src1, 0); break;
            case Mnemonics.feq_s:
                asmR(0b1010000_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.flt_s:
                asmR(0b1010000_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fle_s:
                asmR(0b1010000_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fclass_s:
                asmR(0b1110000_00000_00000_001_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_s_w:
                asmF(0b1101000_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_s_wu:
                asmF(0b1101000_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fmv_w_x:
                asmR(0b1111000_00000_00000_000_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_l_s:
                asmF(0b1100000_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_lu_s:
                asmF(0b1100000_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_s_l:
                asmF(0b1101000_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_s_lu:
                asmF(0b1101000_00011_00000_000_00000_1010011, dst, src1, rm); break;

            case Mnemonics.fld:
                asmI(0b011_00000_0000111, dst, src1, src2); break;
            case Mnemonics.fsd:
                asmS(0b011_00000_0100111, dst, src1, src2); break;
            case Mnemonics.fmadd_d:
                asmR4(0b01_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fmsub_d:
                asmR4(0b01_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmsub_d:
                asmR4(0b01_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmadd_d:
                asmR4(0b01_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fadd_d:
                asmF(0b0000001_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsub_d:
                asmF(0b0000101_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fmul_d:
                asmF(0b0001001_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fdiv_d:
                asmF(0b0001101_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsqrt_d:
                asmF(0b0101101_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fsgnj_d:
                asmR(0b0010001_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjn_d:
                asmR(0b0010001_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjx_d:
                asmR(0b0010001_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmin_d:
                asmR(0b0010101_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmax_d:
                asmR(0b0010101_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fcvt_s_d:
                asmF(0b0100000_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_d_s:
                asmF(0b0100001_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.feq_d:
                asmR(0b1010001_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.flt_d:
                asmR(0b1010001_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fle_d:
                asmR(0b1010001_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fclass_d:
                asmR(0b1110001_00000_00000_001_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_w_d:
                asmF(0b1100001_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_wu_d:
                asmF(0b1100001_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_d_w:
                asmF(0b1101001_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_d_wu:
                asmF(0b1101001_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_l_d:
                asmF(0b1100001_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_lu_d:
                asmF(0b1100001_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fmv_x_d:
                asmR(0b1110001_00000_00000_000_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_d_l:
                asmF(0b1101001_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_d_lu:
                asmF(0b1101001_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fmv_d_x:
                asmR(0b1111001_00000_00000_000_00000_1010011, dst, src1, 00000); break;

            case Mnemonics.flq:
                asmI(0b100_00000_0000111, dst, src1, src2); break;
            case Mnemonics.fsq:
                asmS(0b100_00000_0100111, dst, src1, src2); break;
            case Mnemonics.fmadd_q:
                asmR4(0b11_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fmsub_q:
                asmR4(0b11_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmsub_q:
                asmR4(0b11_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmadd_q:
                asmR4(0b11_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fadd_q:
                asmF(0b0000011_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsub_q:
                asmF(0b0000111_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fmul_q:
                asmF(0b0001011_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fdiv_q:
                asmF(0b0001111_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsqrt_q:
                asmF(0b0101111_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fsgnj_q:
                asmR(0b0010011_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjn_q:
                asmR(0b0010011_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjx_q:
                asmR(0b0010011_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmin_q:
                asmR(0b0010111_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmax_q:
                asmR(0b0010111_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fcvt_s_q:
                asmF(0b0100000_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_q_s:
                asmF(0b0100011_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_d_q:
                asmF(0b0100001_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_q_d:
                asmF(0b0100011_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.feq_q:
                asmR(0b1010011_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.flt_q:
                asmR(0b1010011_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fle_q:
                asmR(0b1010011_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fclass_q:
                asmR(0b1110011_00000_00000_001_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_w_q:
                asmF(0b1100011_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_wu_q:
                asmF(0b1100011_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_q_w:
                asmF(0b1101011_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_q_wu:
                asmF(0b1101011_00001_00000_000_00000_1010011, dst, src1, rm); break;

            case Mnemonics.fcvt_l_q:
                asmF(0b1100011_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_lu_q:
                asmF(0b1100011_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_q_l:
                asmF(0b1101011_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_q_lu:
                asmF(0b1101011_00011_00000_000_00000_1010011, dst, src1, rm); break;

            case Mnemonics.flh:
                asmI(0b001_00000_0000111, dst, src1, src2); break;
            case Mnemonics.fsh:
                asmS(0b001_00000_0100111, dst, src1, src2); break;
            case Mnemonics.fmadd_h:
                asmR4(0b10_00000_00000_000_00000_1000011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fmsub_h:
                asmR4(0b10_00000_00000_000_00000_1000111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmsub_h:
                asmR4(0b10_00000_00000_000_00000_1001011, dst, src1, src2, src3, rm); break;
            case Mnemonics.fnmadd_h:
                asmR4(0b10_00000_00000_000_00000_1001111, dst, src1, src2, src3, rm); break;
            case Mnemonics.fadd_h:
                asmF(0b0000010_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsub_h:
                asmF(0b0000110_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fmul_h:
                asmF(0b0001010_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fdiv_h:
                asmF(0b0001110_00000_00000_000_00000_1010011, dst, src1, src2, rm); break;
            case Mnemonics.fsqrt_h:
                asmF(0b0101110_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fsgnj_h:
                asmR(0b0010010_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjn_h:
                asmR(0b0010010_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fsgnjx_h:
                asmR(0b0010010_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmin_h:
                asmR(0b0010110_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fmax_h:
                asmR(0b0010110_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fcvt_s_h:
                asmF(0b0100000_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_h_s:
                asmF(0b0100010_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_d_h:
                asmF(0b0100001_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_h_d:
                asmF(0b0100010_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_q_h:
                asmF(0b0100011_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_h_q:
                asmF(0b0100010_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.feq_h:
                asmR(0b1010010_00000_00000_010_00000_1010011, dst, src1, src2); break;
            case Mnemonics.flt_h:
                asmR(0b1010010_00000_00000_001_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fle_h:
                asmR(0b1010010_00000_00000_000_00000_1010011, dst, src1, src2); break;
            case Mnemonics.fclass_h:
                asmR(0b1110010_00000_00000_001_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_w_h:
                asmF(0b1100010_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_wu_h:
                asmF(0b1100010_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fmv_x_h:
                asmR(0b1110010_00000_00000_000_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_h_w:
                asmF(0b1101010_00000_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_h_wu:
                asmF(0b1101010_00001_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fmv_h_x:
                asmR(0b1111010_00000_00000_000_00000_1010011, dst, src1, 00000); break;
            case Mnemonics.fcvt_l_h:
                asmF(0b1100010_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_lu_h:
                asmF(0b1100010_00011_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_h_l:
                asmF(0b1101010_00010_00000_000_00000_1010011, dst, src1, rm); break;
            case Mnemonics.fcvt_h_lu:
                asmF(0b1101010_00011_00000_000_00000_1010011, dst, src1, rm); break;

            //zawrs standard/extension/
            case Mnemonics.wrs_nto:
                asmR(0b0000000_00000_00000_000_00000_1110011, 0, 0, 0b01101); break;
            case Mnemonics.wrs_sto:
                asmR(0b0000000_00000_00000_000_00000_1110011, 0, 0, 0b11101); break;

            case Mnemonics.Invalid:
                Section.WriteLeWord32(instrPtr, 0);
                instrPtr += 4;
                break;
            default:
                throw new InvalidOperationException($"Unknown instruction {operation}.");
        }
    }

    internal void asmJ(uint opcode, int rd, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(rd & 0b11111) << 7;
        uInstr |= AsmEncoder.EncodeJdisplacement(offset);
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }


    internal void asmB(uint opcode, int src1, int src2, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uInstr |= AsmEncoder.EncodeBdisplacement(offset);
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    internal void asmS(uint opcode, int src2, int baseReg, int offset)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(offset & 0b11111) << 7;
        uInstr |= (uint)(baseReg & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uInstr |= (uint)(offset >> 5) << 25;
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    // Assembles a Risc-V I-type instruction.

    internal void asmI(uint opcode, int dst, int src1, int src2)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= AsmEncoder.EncodeIdisplacement(src2);
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    // Assembles a Risc-V R-type instruction.
    internal void asmR(uint opcode, int dst, int src1, int src2)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    // Assembles a Risc-V R4-type instruction.
    private void asmR4(uint opcode, int dst, int src1, int src2, int src3, int roundingMode)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(roundingMode & 0b111) << 12;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        uInstr |= (uint)(src3 & 0b11111) << 27;
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    // Assembles a Risc-V R-type instruction with the func3 replaced 
    // with a 3-bit floating point rounding mode.

    private void asmF(uint opcode, int dst, int src1, int src2, int roundingMode)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(roundingMode & 0b111) << 12;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        uInstr |= (uint)(src2 & 0b11111) << 20;
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    // Assembles a Risc-V R-type instruction with the func3 replaced 
    // with a 3-bit floating point rounding mode, using only a single
    // source operand

    private void asmF(uint opcode, int dst, int src1, int roundingMode)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= (uint)(roundingMode & 0b111) << 12;
        uInstr |= (uint)(src1 & 0b11111) << 15;
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }


    // Assembles a Risc-V U-type instruction.

    internal void asmU(uint opcode, int dst, int src1)
    {
        uint uInstr = opcode;
        uInstr |= (uint)(dst & 0b11111) << 7;
        uInstr |= AsmEncoder.EncodeUdisplacement(src1);
        Section.WriteLeWord32(instrPtr, uInstr);
        instrPtr += 4;
    }

    public void ds(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        Section.WriteBytes(instrPtr, bytes);
        instrPtr += (uint)bytes.Length;
    }

    public void dw(int n)
    {
        Section.WriteLeWord32(instrPtr, n);
        instrPtr += 4;
    }

    public void dw(int n, int count)
    {
        for (int i = 0; i < count; ++i) {
            Section.WriteLeWord32(instrPtr, n);
            instrPtr += 4;
        }
    }

    public void dw(string label)
    {
        EmitRelocation(instrPtr, RelocationType.W32_Absolute, label);
        Section.WriteLeWord32(instrPtr, 0);
        instrPtr += 4;
    }

    public void label(string sLabel)
    {
        AddSymbol(sLabel, instrPtr);
    }

    public Relocation pcrel_hi(string symbol)
    {
        var rel = new Relocation(instrPtr, RelocationType.U_PcRelative_Hi20, symbol);
        return rel;
    }

    public Relocation pcrel_lo(int displacement)
    {
        var symPcrelHi = GenerateLocalSymbol(instrPtr + (uint)displacement);
        var rel = new Relocation(instrPtr, RelocationType.I_PcRelative_Lo12, symPcrelHi.Name);
        return rel;
    }

    private Symbol GenerateLocalSymbol(uint value)
    {
        string name;
        int n = Symbols.Count+1;
        do {
            name = $".L_{n,0000}";
        } while (Symbols.ContainsKey(name));
        var sym = AddSymbol(name, value);
        return sym!;
    }

    internal Relocation EmitRelocation(uint instrPtr, RelocationType rtype, string symbolName)
    {
        var rel = new Relocation(instrPtr, rtype, symbolName);
        this.Relocations.Add(rel);
        return rel;
    }
}

