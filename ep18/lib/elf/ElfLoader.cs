using System.Diagnostics;
using System.Runtime.InteropServices;

namespace rvfun.lib.elf;

public class ElfLoader
{
    public Elf32_Ehdr header;
    public List<Elf32_Phdr> SegmentHeaders;
    public List<byte[]> SegmentData;

    public ElfLoader()
    {
        SegmentHeaders = [];
        SegmentData = [];
    }

    public void Load(Stream stream)
    {
        var hdrSpan = MemoryMarshal.CreateSpan<Elf32_Ehdr>(ref this.header, 1);
        var byteSpan = MemoryMarshal.Cast<Elf32_Ehdr, byte>(hdrSpan);
        var bytesRead = stream.Read(byteSpan);      //$TODO: check for errors.
        if (!IsValidMagicNumber())
            throw new BadImageFormatException("Not an ELF file.");
        LoadSegmentHeaders(stream);
        LoadSegmentData(stream);
    }

    private void LoadSegmentHeaders(Stream stream)
    {
        stream.Position = this.header.e_phoff;
        for (int i = 0; i < this.header.e_phnum; ++i)
        {
            var hdr = new Elf32_Phdr();
            var hdrSpan = MemoryMarshal.CreateSpan<Elf32_Phdr>(ref hdr, 1);
            var byteSpan = MemoryMarshal.Cast<Elf32_Phdr, byte>(hdrSpan);
            stream.Read(byteSpan);
            SegmentHeaders.Add(hdr);
        }
    }

    private void LoadSegmentData(Stream stream)
    {
        for (int i = 0; i < this.header.e_phnum; ++i)
        {
            var hdr = this.SegmentHeaders[i];
            var data = new byte[hdr.p_memsz];
            stream.Position = hdr.p_offset;
            Debug.Assert(hdr.p_filesz <= hdr.p_memsz);
            stream.Read(data, 0, (int)hdr.p_filesz);
            SegmentData.Add(data);
        }
    }


    private bool IsValidMagicNumber()
    {
        unsafe
        {
            return
               (header.e_ident[0] == 0x7F &&
                header.e_ident[1] == 'E' &&
                header.e_ident[2] == 'L' &&
                header.e_ident[3] == 'F');
        }
    }
}
