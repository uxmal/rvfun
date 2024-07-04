using rvfun;

public class OsEmulator
{
    public const int SYSCALL_EXIT = 0;
    public const int SYSCALL_WRITE = 1;

private readonly Memory mem;
    private List<Stream> handles;

    public OsEmulator(Memory mem)
    {
        this.mem = mem;
        this.handles = new List<Stream>();

        Console.In.Close();
        Console.Out.Close();
        Console.Error.Close();

        var inStm = Console.OpenStandardInput();
        var outStm = Console.OpenStandardOutput();
        var errStm = Console.OpenStandardError();

        this.handles.Add(inStm);
        this.handles.Add(outStm);
        this.handles.Add(errStm);

    }

    public void Trap(Emulator emu)
    {
        var syscall = emu.Registers[17];
        switch (syscall)
        {
            case SYSCALL_EXIT:
                var exitCode = emu.Registers[10];
                emu.Stop(exitCode);
                break;
            case SYSCALL_WRITE:
                var handle = emu.Registers[10];
                var address = emu.Registers[11];
                var length = emu.Registers[12];
                var result = WriteToHandle(handle, address, length);
                emu.Registers[10] = result;
                break;
            default:
                throw new NotImplementedException($"Unimplemented system call {syscall}");
        }
    }

    private int WriteToHandle(int handle, int address, int length)
    {
        if (!IsHandleValid(handle))
            return -1;
        try {
            var span = mem.GetSpan(address, length);
            handles[handle].Write(span);
        }
        catch 
        {
            return -1;
        }
        return length;
    }

    private bool IsHandleValid(int handle)
    {
        if (handle < 0 || handle >= handles.Count)
            return false;
        var stm = handles[handle];
        return stm != null;
    }
}