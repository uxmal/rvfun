using rvfun;

public class OsEmulator
{
    public const int SYSCALL_EXIT = 0;
    public const int SYSCALL_WRITE = 1;
    public const int SYSCALL_READ = 2;
    public const int SYSCALL_OPEN = 3;
    public const int SYSCALL_CLOSE = 4;


    public const int O_RDONLY = 1;
    public const int O_WRONLY = 2;
    public const int O_RDWR = 3;

    public const int O_CREAT = 0x10;

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
            case SYSCALL_READ:
                handle = emu.Registers[10];
                address = emu.Registers[11];
                length = emu.Registers[12];
                result = ReadFromHandle(handle, address, length);
                emu.Registers[10] = result;
                break;
            case SYSCALL_OPEN:
                int path = emu.Registers[10];
                int flags = emu.Registers[11];
                result = OpenFileHandle(path, flags);
                emu.Registers[10] = result;
                break;
            case SYSCALL_CLOSE:
                handle = emu.Registers[10];
                result = CloseHandle(handle);
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
        try
        {
            var span = mem.GetSpan(address, length, AccessMode.Write);
            handles[handle].Write(span);
        }
        catch
        {
            return -1;
        }
        return length;
    }

    private int ReadFromHandle(int handle, int address, int length)
    {
        if (!IsHandleValid(handle))
            return -1;
        try
        {
            var span = mem.GetSpan(address, length, AccessMode.Write);
            int cbRead = handles[handle].Read(span);
            return cbRead;
        }
        catch
        {
            return -1;
        }
    }

    private int OpenFileHandle(int path, int flags)
    {
        try
        {
            var str = mem.GetZeroTerminatedString((uint)path);
            FileMode mode = FileMode.Open;
            if ((flags & O_CREAT) == O_CREAT)
                mode = FileMode.Create;

            FileAccess access = FileAccess.Read;
            if ((flags & O_RDWR) == O_RDONLY)
                access = FileAccess.Read;
            if ((flags & O_RDWR) == O_WRONLY)
                access = FileAccess.Write;
            if ((flags & O_RDWR) == O_RDWR)
                access = FileAccess.ReadWrite;

            FileStream stream = new FileStream(str, mode, access);

            int i;
            for (i = 0; i < handles.Count; ++i)
            {
                if (handles[i] is null)
                {
                    handles[i] = stream;
                    return i;
                }
            }
            handles.Add(stream);
            return handles.Count - 1;
        }
        catch
        {
            return -1;
        }
    }

    private int CloseHandle(int handle)
    {
        if (!IsHandleValid(handle))
            return -1;
        try
        {
            handles[handle].Close();
            handles[handle] = null!;
            return 0;
        }
        catch
        {
            return -1;
        }
    }
    private bool IsHandleValid(int handle)
    {
        if (handle < 0 || handle >= handles.Count)
            return false;
        var stm = handles[handle];
        return stm != null;
    }
}