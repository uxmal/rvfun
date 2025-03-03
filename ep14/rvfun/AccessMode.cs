namespace rvfun;

[Flags]
public enum AccessMode
{
    Read = 0x4,
    Write = 0x2,
    Execute = 0x1,

    RW = Read | Write,
    RWX = Read | Write | Execute,
    RX = Read | Execute
}