namespace rvfun.lib.elf;

using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Reko.ImageLoaders.Coff.eCoff;
using static rvfun.lib.elf.Elf;

public class ElfBuilder
{
    private Elf32_Ehdr fileHeader;
    private List<Elf32_Phdr> segmentHeaders;
    private List<byte[]> segmentData;
    private List<uint> segmentOffsets;

    public ElfBuilder(
        bool is64bit,
        bool isBigEndian,
        ushort fileType,
        ushort machineType)
    {
        unsafe
        {
            fileHeader.e_ident[0] = 0x7F;
            fileHeader.e_ident[1] = (byte)'E';
            fileHeader.e_ident[2] = (byte)'L';
            fileHeader.e_ident[3] = (byte)'F';
            fileHeader.e_ident[4] = is64bit ? ELFCLASS64 : ELFCLASS32;
            fileHeader.e_ident[5] = isBigEndian ? ELFDATA2MSB : ELFDATA2LSB;
            fileHeader.e_ident[6] = EV_CURRENT;
        }
        this.fileHeader.e_type = fileType;
        this.fileHeader.e_machine = machineType;
        this.fileHeader.e_version = EV_CURRENT;
        this.fileHeader.e_entry = 0;
        this.fileHeader.e_shoff = 0;
        this.fileHeader.e_flags = 0;
        unsafe
        {
            this.fileHeader.e_ehsize = (ushort)sizeof(Elf32_Ehdr);
            this.fileHeader.e_phentsize = (ushort)sizeof(Elf32_Phdr);
            this.fileHeader.e_shentsize = (ushort)sizeof(Elf32_Shdr);
        }
        this.fileHeader.e_phoff = this.fileHeader.e_ehsize;
        this.fileHeader.e_phnum = 0;
        this.fileHeader.e_shnum = 0;
        this.fileHeader.e_shstrndx = 0;

        this.segmentHeaders = [];
        this.segmentData = [];
        this.segmentOffsets = [];
    }

    public uint EntryPointAddress
    {
        get => this.fileHeader.e_entry;
        set => this.fileHeader.e_entry = value;
    }

    public void AddSegment(
        uint segmentType,
        uint address,
        byte[] data,
        uint memorySize,
        AccessMode mode,
        uint align)
    {
        uint flags = 0u;
        if (mode.HasFlag(AccessMode.Read)) flags |= PF_R;
        if (mode.HasFlag(AccessMode.Write)) flags |= PF_W;
        if (mode.HasFlag(AccessMode.Execute)) flags |= PF_X;
        var segHdr = new Elf32_Phdr
        {
            p_type = segmentType,
            p_vaddr = address,
            p_paddr = 0,
            p_filesz = (uint)data.Length,
            p_memsz = memorySize == 0 ? (uint)data.Length : memorySize,
            p_flags = flags,
            p_align = align
        };
        segmentHeaders.Add(segHdr);
        segmentData.Add(data);
    }

    public void WriteToStream(Stream stm)
    {
        Console.WriteLine("Entry: {0:X}", fileHeader.e_entry);
        this.fileHeader.e_phnum = (ushort)segmentHeaders.Count;
        var hdrSpan = MemoryMarshal.CreateSpan(ref this.fileHeader, 1);
        var byteSpan = MemoryMarshal.Cast<Elf32_Ehdr, byte>(hdrSpan);
        stm.Write(byteSpan);

        this.segmentOffsets = ComputeSegmentFileOffsets();
        WriteProgramHeaders(stm);
        WriteSegmentData(stm);
    }

    private List<uint> ComputeSegmentFileOffsets()
    {
        uint fileOffset = fileHeader.e_ehsize + (uint)segmentHeaders.Count * fileHeader.e_phentsize;
        var fileOffsets = new List<uint>();
        foreach (var segment in this.segmentHeaders)
        {
            fileOffset = AlignUp(fileOffset, segment.p_align);
            fileOffsets.Add(fileOffset);
            fileOffset += segment.p_filesz;
        }
        return fileOffsets;
    }

    private uint AlignUp(uint value, uint align)
    {
        Debug.Assert(align > 0);
        return align * ((value + align - 1) / align);
    }

    private void WriteProgramHeaders(Stream stm)
    {
        Debug.Assert(segmentHeaders.Count == segmentData.Count);
        Debug.Assert(segmentHeaders.Count == segmentOffsets.Count);
        for (int i = 0; i < segmentHeaders.Count; ++i)
        {
            var segHdr = segmentHeaders[i];
            segHdr.p_offset = segmentOffsets[i];
            var hdrSpan = MemoryMarshal.CreateSpan(ref segHdr, 1);
            var byteSpan = MemoryMarshal.Cast<Elf32_Phdr, byte>(hdrSpan);
            stm.Write(byteSpan);
        }
    }

    private void WriteSegmentData(Stream stm)
    {
        Debug.Assert(segmentHeaders.Count == segmentData.Count);
        Debug.Assert(segmentHeaders.Count == segmentOffsets.Count);
        for (int i = 0; i < segmentOffsets.Count; ++i)
        {
            var fileOffset = segmentOffsets[i];
            Debug.Assert(stm.Position <= fileOffset);
            int nPaddingBytes = (int)fileOffset - (int)stm.Position;
            while (nPaddingBytes > 0)
            {
                stm.WriteByte(0);
                --nPaddingBytes;
            }
            stm.Write(segmentData[i]);
        }
    }
}