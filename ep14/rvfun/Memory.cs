using System.Text;
using Microsoft.Win32.SafeHandles;
using Reko.Core.Collections;

namespace rvfun;

public class Memory
{
    public const uint BytesPerPage = 4096;
    public const uint Anywhere = ~0u;

    private readonly SortedList<uint, MemoryDescriptor> descriptors;

    public Memory(byte[] bytes) : this()
    {
        Allocate(0, bytes, AccessMode.RWX);
    }

    public Memory()
    {
        this.descriptors = [];
    }

    public byte ReadByte(uint address)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.Read);
        return bytes[offset];
    }

    public void WriteByte(uint address, byte value)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.Read);
        bytes[offset] = value;
    }

    public uint ReadLeWord16(uint address)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.Read);
        var b0 = bytes[address];
        var b1 = bytes[address + 1];
        return b0 +
            (uint)(b1 * 256);
    }

    public uint ReadLeWord32(uint address)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.Read);
        var b0 = bytes[offset];
        var b1 = bytes[offset + 1];
        var b2 = bytes[offset + 2];
        var b3 = bytes[offset + 3];
        return b0 +
            (uint)(b1 * 256) +
            (uint)(b2 * 65536) +
            (uint)(b3 * 256 * 65536);
    }

    // Use this when reading instructions for execution.
    public uint ReadLeWord32Executable(uint address)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.RX);
        var b0 = bytes[offset];
        var b1 = bytes[offset + 1];
        var b2 = bytes[offset + 2];
        var b3 = bytes[offset + 3];
        return b0 +
            (uint)(b1 * 256) +
            (uint)(b2 * 65536) +
            (uint)(b3 * 256 * 65536);
    }

    public void WriteLeWord16(uint address, ushort value) => WriteLeWord16(address, (short)value);

    public void WriteLeWord16(uint address, short value)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.Write);
        var b1 = (byte)(value >> 8);
        var b0 = (byte)value;
        bytes[offset] = b0;
        bytes[offset + 1] = b1;
    }


    public void WriteLeWord32(uint address, uint value) => WriteLeWord32(address, (int)value);

    public void WriteLeWord32(uint address, int value)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.Write);
        var b3 = (byte)(value >> 24);
        var b2 = (byte)(value >> 16);
        var b1 = (byte)(value >> 8);
        var b0 = (byte)value;
        bytes[offset] = b0;
        bytes[offset + 1] = b1;
        bytes[offset + 2] = b2;
        bytes[offset + 3] = b3;
    }

    public bool IsValidAddress(uint iptr)
    {
        if (!descriptors.TryGetLowerBound(iptr, out var descriptor))
            return false;
        return descriptor.IsInside(iptr);
    }

    public bool IsAccessible(uint iptr, AccessMode requestedMode)
    {
        if (!descriptors.TryGetLowerBound(iptr, out var descriptor))
            return false;
        return descriptor.IsInside(iptr) && (requestedMode & descriptor.accessMode) == requestedMode;
    }


    public void WriteBytes(uint address, byte[] bytesToWrite)
    {
        var (offset, bytes) = AccessPage(address, AccessMode.Write);
        Array.Copy(bytesToWrite, 0, bytes, offset, bytesToWrite.Length);
    }

    public Span<byte> GetSpan(int address, int length, AccessMode mode)
    {
        var (offset, bytes) = AccessPage((uint)address, mode);
        return bytes.AsSpan((int)offset, length);
    }

    public uint Allocate(uint address, uint bytesize, AccessMode mode)
    {
        if (bytesize == 0)
            throw new ArgumentException();
        bytesize = AlignUp(bytesize, BytesPerPage);
        var bytes = new byte[bytesize];
        return Allocate(address, bytes, mode);
    }

    public uint Allocate(uint address, byte[] bytes, AccessMode mode)
    {
        if (address == Anywhere)
        {
            address = FindAvailableAddressGap((uint)bytes.Length);
            var bytesize = AlignUp((uint)bytes.Length, BytesPerPage);
            var descriptor = new MemoryDescriptor(address, bytesize, mode, bytes);
            descriptors.Add(address, descriptor);
            return address;
        }
        else
        {
            if (this.descriptors.TryGetLowerBound(address, out var descriptorExisting))
            {
                if ((descriptorExisting.address + (uint)descriptorExisting.bytes.Length) > address)
                    throw new InvalidOperationException("Memory areas would overlap");
            }
            var bytesize = AlignUp((uint)bytes.Length, BytesPerPage);
            var descriptor = new MemoryDescriptor(address, bytesize, mode, bytes);
            descriptors.Add(address, descriptor);
            return address;
        }
    }

    private uint FindAvailableAddressGap(uint length)
    {
        uint address = BytesPerPage;
        foreach (var descriptor in this.descriptors.Values)
        {
            if (address + length <= descriptor.address)
                return address;
            address = descriptor.address + (uint)descriptor.bytes.Length;
        }
        return address;
    }

    private (uint offset, byte[]) AccessPage(uint address, AccessMode mode)
    {
        if (!descriptors.TryGetLowerBound(address, out var descriptor))
            throw new InvalidOperationException();
        if ((mode & descriptor.accessMode) != mode)
            throw new InvalidOperationException();
        uint offset = address - descriptor.address;
        if (offset >= (uint)descriptor.bytes.Length)
            throw new InvalidOperationException();
        return (offset, descriptor.bytes);
    }

    private static uint AlignUp(uint n, uint unitsize)
    {
        uint result = unitsize * ((n + (unitsize - 1)) / unitsize);
        return result;
    }

    public string GetZeroTerminatedString(uint addrUtf8String)
    {
        var address = addrUtf8String;
        for (;;) {
            byte b = this.ReadByte(address);
            if (b == 0)
                break;
            ++address;
        }
        var length = (int) (address - addrUtf8String);
        var span = GetSpan((int)addrUtf8String, length, AccessMode.Read);
        return Encoding.UTF8.GetString(span);
    }

    private class MemoryDescriptor
    {
        public uint address;
        public AccessMode accessMode;
        public byte[] bytes;

        public MemoryDescriptor(uint address, uint size, AccessMode mode, byte[] bytes)
        {
            this.address = address;
            this.accessMode = mode;
            this.bytes = bytes;
        }

        public bool IsInside(uint address)
        {
            var offset = address - this.address;
            return offset < (uint)this.bytes.Length;
        }
    }
}

