namespace rvfun;

public enum Mnemonics
{
    addi,
    muli,
    add,
    mul,    

    lb,
    lbu,
    sb,

    lh,
    lhu,
    lw,
    sh,
    sw,

    @goto,  //$TODO Doesn't really exist, remove soon
    jal,
    sgti,
    bne,
    bnz,    //$TODO Doesn't really exist, remove soon
    slti,
}