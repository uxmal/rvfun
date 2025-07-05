using System.Runtime.InteropServices;
using static rvfun.lib.elf.Elf;

namespace rvfun.lib.elf;


public static class Elf
{
    public const int EI_NIDENT = 16;

    public const byte ELFCLASSNONE = 0;  // Invalid class
    public const byte ELFCLASS32 = 1;  // 32-bit objects
    public const byte ELFCLASS64 = 2;  // 64-bit objects


    public const byte ELFDATANONE = 0;  // Invalid data encoding
    public const byte ELFDATA2LSB = 1;  // See below
    public const byte ELFDATA2MSB = 2;  // See below


    public const byte EV_CURRENT = 1;


    public const ushort ET_NONE = 0; // No file type
    public const ushort ET_REL = 1; // Relocatable file
    public const ushort ET_EXEC = 2; // Executable file
    public const ushort ET_DYN = 3; // Shared object file
    public const ushort ET_CORE = 4; // Core file
    public const ushort ET_LOPROC = 0xff00; // Processor-specific
    public const ushort ET_HIPROC = 0xffff; // Processor-specific


    public const ushort EM_NONE = 0; // No machine
    public const ushort EM_M32 = 1; // AT&T WE 32100
    public const ushort EM_SPARC = 2; // SPARC
    public const ushort EM_386 = 3; // Intel Architecture
    public const ushort EM_68K = 4; // Motorola 68000
    public const ushort EM_88K = 5; // Motorola 88000
    public const ushort EM_860 = 7; // Intel 80860
    public const ushort EM_MIPS = 8; // MIPS RS3000 Big-Endian
    public const ushort EM_MIPS_RS4_BE = 10; // MIPS RS4000 Big-Endian    

    public const ushort EM_RISCV = 243;     // Risc-V

    public const uint PT_NULL = 0; // 
    public const uint PT_LOAD = 1; // 
    public const uint PT_DYNAMIC = 2; // 
    public const uint PT_INTERP = 3; // 
    public const uint PT_NOTE = 4; // 
    public const uint PT_SHLIB = 5; // 
    public const uint PT_PHDR = 6; // 
    public const uint PT_LOPROC = 0x70000000; // 
    public const uint PT_HIPROC = 0x7fffffff; // 

    public const uint PF_X = 0x1; // Execute
    public const uint PF_W = 0x2; // Write
    public const uint PF_R = 0x4; // Read
    public const uint PF_MASKPROC = unchecked((uint)0xF0000000u); // Unspecified
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Elf32_Ehdr
{
    public fixed byte e_ident[EI_NIDENT];
    public Elf32_Half e_type;
    public Elf32_Half e_machine;
    public Elf32_Word e_version;
    public Elf32_Addr e_entry;
    public Elf32_Off e_phoff;
    public Elf32_Off e_shoff;
    public Elf32_Word e_flags;
    public Elf32_Half e_ehsize;
    public Elf32_Half e_phentsize;
    public Elf32_Half e_phnum;
    public Elf32_Half e_shentsize;
    public Elf32_Half e_shnum;
    public Elf32_Half e_shstrndx;
}


[StructLayout(LayoutKind.Sequential)]
public struct Elf32_Phdr
{
    public Elf32_Word p_type;
    public Elf32_Off p_offset;
    public Elf32_Addr p_vaddr;
    public Elf32_Addr p_paddr;
    public Elf32_Word p_filesz;
    public Elf32_Word p_memsz;
    public Elf32_Word p_flags;
    public Elf32_Word p_align;
} 


public struct Elf32_Shdr {
public Elf32_Word sh_name;
public Elf32_Word sh_type;
public Elf32_Word sh_flags;
public Elf32_Addr sh_addr;
public Elf32_Off sh_offset;
public Elf32_Word sh_size;
public Elf32_Word sh_link;
public Elf32_Word sh_info;
public Elf32_Word sh_addralign;
public Elf32_Word sh_entsize;
} 