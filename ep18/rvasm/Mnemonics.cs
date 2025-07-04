namespace rvfun.asm;

public enum Mnemonics
{
    Invalid, 
    addi,
    muli,
    add,
    mul,    

    lb,
    lbu,
    sb,

    ecall,
    lh,
    lhu,
    lw,
    sh,
    sw,

    jal,
    sgti,
    bge,
    bne,
    slti,
    jalr,


lui,
auipc,
beq,
blt,
bltu,
bgeu,
sltiu,
xori,
ori,
andi,
slli,
srli,
srai,
sub,
sll,
slt,
sltu,
xor,
srl,
sra,
or,
and,
fence_tso,
pause,
ebreak,
lwu,
ld,
sd,
addiw,
slliw,
srliw,
sraiw,
addw,
subw,
sllw,
srlw,
sraw,
fence_i,
csrrw,
csrrs,
csrrc,
csrrwi,
csrrsi,
csrrci,
mulh,
mulhsu,
mulhu,
div,
divu,
rem,
remu,
mulw,
divw,
divuw,
remw,
remuw,
lr_w,
sc_w,
amoswap_w,
amoadd_w,
amoxor_w,
amoand_w,
amoor_w,
amomin_w,
amomax_w,
amominu_w,
amomaxu_w,
lr_d,
sc_d,
amoswap_d,
amoadd_d,
amoxor_d,
amoand_d,
amoor_d,
amomin_d,
amomax_d,
amominu_d,
amomaxu_d,
flw,
fsw,
fmadd_s,
fmsub_s,
fnmsub_s,
fnmadd_s,
fadd_s,
fsub_s,
fmul_s,
fdiv_s,
fsqrt_s,
fsgnj_s,
fsgnjn_s,
fsgnjx_s,
fmin_s,
fmax_s,
fcvt_w_s,
fcvt_wu_s,
fmv_x_w,
feq_s,
flt_s,
fle_s,
fclass_s,
fcvt_s_w,
fcvt_s_wu,
fmv_w_x,
fcvt_l_s,
fcvt_lu_s,
fcvt_s_l,
fcvt_s_lu,
fld,
fsd,
fmadd_d,
fmsub_d,
fnmsub_d,
fnmadd_d,
fadd_d,
fsub_d,
fmul_d,
fdiv_d,
fsqrt_d,
fsgnj_d,
fsgnjn_d,
fsgnjx_d,
fmin_d,
fmax_d,
fcvt_s_d,
fcvt_d_s,
feq_d,
flt_d,
fle_d,
fclass_d,
fcvt_w_d,
fcvt_wu_d,
fcvt_d_w,
fcvt_d_wu,
fcvt_l_d,
fcvt_lu_d,
fmv_x_d,
fcvt_d_l,
fcvt_d_lu,
fmv_d_x,
flq,
fsq,
fmadd_q,
fmsub_q,
fnmsub_q,
fnmadd_q,
fadd_q,
fsub_q,
fmul_q,
fdiv_q,
fsqrt_q,
fsgnj_q,
fsgnjn_q,
fsgnjx_q,
fmin_q,
fmax_q,
fcvt_s_q,
fcvt_q_s,
fcvt_d_q,
fcvt_q_d,
feq_q,
flt_q,
fle_q,
fclass_q,
fcvt_w_q,
fcvt_wu_q,
fcvt_q_w,
fcvt_q_wu,
fcvt_l_q,
fcvt_lu_q,
fcvt_q_l,
fcvt_q_lu,
flh,
fsh,
fmadd_h,
fmsub_h,
fnmsub_h,
fnmadd_h,
fadd_h,
fsub_h,
fmul_h,
fdiv_h,
fsqrt_h,
fsgnj_h,
fsgnjn_h,
fsgnjx_h,
fmin_h,
fmax_h,
fcvt_s_h,
fcvt_h_s,
fcvt_d_h,
fcvt_h_d,
fcvt_q_h,
fcvt_h_q,
feq_h,
flt_h,
fle_h,
fclass_h,
fcvt_w_h,
fcvt_wu_h,
fmv_x_h,
fcvt_h_w,
fcvt_h_wu,
fmv_h_x,
fcvt_l_h,
fcvt_lu_h,
fcvt_h_l,
fcvt_h_lu,
wrs_nto,
wrs_sto,
}