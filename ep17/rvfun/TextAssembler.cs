using System.Text;

namespace rvfun;

public class TextAssembler
{
    private byte[] bytes;
    private int iPos;
    private int linenumber;
    private bool valid;
    private int iBeginPos;
    private Token previousToken;

    private Assembler asm;

    private Dictionary<string, int> equates;
    private StringBuilder sb;

    private string currentString;

    public TextAssembler(byte[] bytes, Logger logger)
    {
        this.bytes = bytes;
        this.iPos = 0;
        this.linenumber = 1;
        this.valid = true;
        this.previousToken = Token.EOF;
        this.asm = new Assembler(logger);
        this.equates = [];
        this.sb = new();
        this.currentString = "";
    }

    public int CurrentValue { get; private set; }
    public Dictionary<string, Symbol> Symbols => asm.Symbols;
    public AssemblerSection Section => asm.Section;

    public List<Relocation> Relocations => asm.Relocations;

    public AssemblerSection? AssembleFile()
    {
        while (AssembleLine())
        {
        }
        return valid ? Section : null;
    }

    // Returns true if a line was processed by the assembler and more 
    // input is expected, returns false if we have reached end of file.
    public bool AssembleLine()
    {
        string w;
        for (; ; )
        {
            var token = GetToken();
            if (token == Token.EOF)
                return false;
            if (token == Token.EndLine)
                return true;
            if (token != Token.Word)
            {
                UnexpectedToken(token);
                return SkipUntil(Token.EndLine);
            }
            w = this.GetTokenString();
            if (!PeekAndDiscard(Token.Colon))
                break;
            if (asm.AddSymbol(w, asm.Position) is null)
            {
                Error($"Redefinition of symbol '{w}'.");
            }
        }
        // We have seen a word, is it followed by a directive?
        if (PeekToken() == Token.Word)
        {
            if (this.GetTokenString() == "equ")
            {
                GetToken();
                var n = ExpectNumber();
                ExpectEndLine();

                if (!equates.TryAdd(w, n))
                {
                    Error("Duplicate equ definition.");
                }
                return true;
            }
        }
        // We have seen a word, classify it.
        foreach (var enc in encoders)
        {
            if (enc.Mnemonic == w)
            {
                enc.Handler(this, enc.Opcode);
                return true;
            }
        }
        Error($"Unknown mnemonic or directive '{w}'.");
        return SkipUntil(Token.EndLine);
    }

    private void Error(string message)
    {
        Console.WriteLine($"error({linenumber}): {message}");
        this.valid = false;
    }

    private static AsmEncoder[] encoders = [
        new AsmEncoder("addi", 0b000_00000_0010011, AsmI),
        new AsmEncoder("add", 0b0000000_0000000000_000_00000_0110011, AsmR),
        new AsmEncoder("auipc", 0b0010111, AsmAuipc),
        new AsmEncoder("beq", 0b000_00000_1100011, AsmB),
        new AsmEncoder("blt", 0b100_00000_1100011, AsmB),
        new AsmEncoder("bne", 0b001_00000_1100011, AsmB),
        new AsmEncoder("ds", 0, AsmDs),
        new AsmEncoder("ecall", 0, AsmEcall),
        new AsmEncoder("j", 0b1101111, AsmJ),
        new AsmEncoder("jal", 0b1101111, AsmJal),
        new AsmEncoder("jalr", 0b000_00000_1100111, AsmI),
        new AsmEncoder("lbu", 0b100_00000_0000011, AsmI),
        new AsmEncoder("lw", 0b010_00000_0000011, AsmI),
        new AsmEncoder("li", 0, AsmLi),
        new AsmEncoder("slli", 0b0000000_0000000000_001_00000_0010011, AsmShifti),
        new AsmEncoder("sub", 0b0100000_0000000000_000_00000_0110011, AsmR),
        new AsmEncoder("sw", 0b010_00000_0100011, AsmS)
    ];

    private class AsmEncoder
    {
        public AsmEncoder(string mnemonic, uint opcode, Action<TextAssembler, uint> handler)
        {
            this.Mnemonic = mnemonic;
            this.Opcode = opcode;
            this.Handler = handler;
        }

        public string Mnemonic { get; }
        public uint Opcode { get; }
        public Action<TextAssembler, uint> Handler { get; }
    }


    private static void AsmAuipc(TextAssembler asm, uint opcode)
    {
        var reg = asm.ExpectNumber();
        var imm = asm.ExpectImmediate();
        asm.ExpectEndLine();
        asm.asm.asmU(opcode, reg, imm);

    }

    private static void AsmB(TextAssembler asm, uint opcode)
    {
        var reg1 = asm.ExpectNumber();
        var reg2 = asm.ExpectNumber();
        var target = asm.ExpectSymbol();
        asm.ExpectEndLine();

        asm.asm.EmitRelocation(asm.asm.Position, RelocationType.B_PcRelative, target);
        asm.asm.asmB(opcode, reg1, reg2, 0);
    }

    private static void AsmDs(TextAssembler asm, uint opcode)
    {
        var s = asm.ExpectStringLiteral();
        asm.ExpectEndLine();

        asm.asm.ds(s);
    }

    private static void AsmEcall(TextAssembler asm, uint opcode)
    {
        asm.ExpectEndLine();

        asm.asm.ecall();
    }


    private static void AsmI(TextAssembler asm, uint opcode)
    {
        var dst = asm.ExpectNumber();
        var src1 = asm.ExpectNumber();
        var src2 = asm.ExpectImmediate();
        asm.ExpectEndLine();
        asm.asm.asmI(opcode, dst, src1, src2);
    }

    private static void AsmJ(TextAssembler asm, uint opcode)
    {
        var target = asm.ExpectSymbol();
        asm.ExpectEndLine();
        asm.asm.EmitRelocation(asm.asm.Position, RelocationType.J_PcRelative, target);
        asm.asm.asmJ(opcode, 0, 0);
    }

    private static void AsmJal(TextAssembler asm, uint opcode)
    {
        var linkRegister = asm.ExpectNumber();
        var target = asm.ExpectSymbol();
        asm.ExpectEndLine();
        asm.asm.EmitRelocation(asm.asm.Position, RelocationType.J_PcRelative, target);
        asm.asm.asmJ(opcode, linkRegister, 0);
    }

    private static void AsmLi(TextAssembler asm, uint opcode)
    {
        var reg = asm.ExpectNumber();
        var imm = asm.ExpectImmediate();
        asm.ExpectEndLine();
        asm.asm.li(reg, imm);
    }

    private static void AsmR(TextAssembler asm, uint opcode)
    {
        var dst = asm.ExpectNumber();
        var src1 = asm.ExpectNumber();
        var src2 = asm.ExpectNumber();
        asm.ExpectEndLine();
        asm.asm.asmR(opcode, dst, src1, src2);
    }

    // Special casse for Shift instructions because their immediate
    // values are smaller than the I-type instructions.
    private static void AsmShifti(TextAssembler asm, uint opcode)
    {
        var dst = asm.ExpectNumber();
        var src1 = asm.ExpectNumber();
        var src2 = asm.ExpectImmediate();
        asm.ExpectEndLine();
        asm.asm.asmR(opcode, dst, src1, src2);
    }

    private static void AsmS(TextAssembler asm, uint opcode)
    {
        var src = asm.ExpectNumber();
        var baseReg = asm.ExpectNumber();
        var offset = asm.ExpectNumber();
        asm.ExpectEndLine();
        asm.asm.asmS(opcode, src, baseReg, offset);
    }


    private void ExpectEndLine()
    {
        var token = GetToken();
        if (token == Token.EOF || token == Token.EndLine)
            return;
        Expected("an end of line");
        SkipUntil(Token.EndLine);
    }

    private int ExpectImmediate()
    {
        var token = GetToken();
        if (token == Token.Number)
        {
            return this.CurrentValue;
        }
        if (token == Token.Word)
        {
            var w = this.GetTokenString();
            if (w == "%pcrel_hi")
            {
                ExpectToken(Token.LParen);
                var symbol = ExpectSymbol();
                ExpectToken(Token.RParen);
                var rel = asm.pcrel_hi(symbol);
                this.Relocations.Add(rel);
                return 0;
            }
            else if (w == "%pcrel_lo")
            {
                ExpectToken(Token.LParen);
                var offfset = ExpectNumber();
                ExpectToken(Token.RParen);
                var rel = asm.pcrel_lo(offfset);
                this.Relocations.Add(rel);
                return 0;
            }
        }
        UnexpectedToken(token);
        return 0;
    }

    private int ExpectNumber()
    {
        if (GetToken() != Token.Number)
        {
            Expected("a number");
            return 0;
        }
        return this.CurrentValue;
    }

    private string ExpectStringLiteral()
    {
        if (GetToken() != Token.StringLiteral)
        {
            Expected("a string literal");
            return "";
        }
        return this.GetTokenString();
    }

    private string ExpectSymbol()
    {
        if (GetToken() != Token.Word)
        {
            Expected("a symbol");
            return "";
        }
        return this.GetTokenString();
    }

    private void ExpectToken(Token expected)
    {
        if (GetToken() != expected)
        {
            Error($"expected a {expected}.");
        }
    }

    private void Expected(string expected)
    {
        Error($"Expected {expected}.");
    }

    private void UnexpectedToken(Token token)
    {
        Error($"Unexpected token '{token}'.");
    }

    private bool SkipUntil(Token stopToken)
    {
        for (; ; )
        {
            var token = GetToken();
            if (token == Token.EOF)
                return false;
            else if (token == stopToken)
                return true;
        }
    }

    private bool PeekAndDiscard(Token expectedToken)
    {
        if (PeekToken() != expectedToken)
            return false;
        GetToken();
        return true;
    }

    public Token PeekToken()
    {
        if (previousToken == Token.EOF)
        {
            this.previousToken = ReadToken();
        }
        return previousToken;
    }

    public Token GetToken()
    {
        if (previousToken == Token.EOF)
            return ReadToken();
        var result = previousToken;
        previousToken = Token.EOF;
        return result;
    }

    private Token ReadToken()
    {
        var state = State.Start;
        int sign = 1;
        this.sb.Clear();
        for (; ; )
        {
            int c;
            if (iPos >= this.bytes.Length)
            {
                c = -1;
            }
            else
            {
                c = (int)bytes[iPos];
                ++iPos;
            }
            switch (state)
            {
                case State.Start:
                    switch (c)
                    {
                        case -1:
                            return Token.EOF;
                        case ' ': // Ignore white space
                        case ',':   // Ignore commas
                        case '\t': // Ignore tabs.
                            continue;
                        case '\n':
                            ++linenumber;
                            return Token.EndLine;
                        case '\r':
                            state = State.Cr;
                            continue;
                        case ':':
                            return Token.Colon;
                        case '(':
                            return Token.LParen;
                        case ')':
                            return Token.RParen;
                        case ';':
                            state = State.Comment;
                            continue;
                        case '-':
                            state = State.Minus;
                            continue;
                        case '"':
                            state = State.StringLiteral;
                            continue;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            CurrentValue = c - '0';
                            state = State.Number;
                            continue;
                        default:
                            if (char.IsAsciiLetter((char)c) || c == '_' || c == '.' || c == '%')
                            {
                                sb.Append((char)c);
                                iBeginPos = iPos - 1;
                                state = State.Word;
                                continue;
                            }
                            Unexpected(c);
                            continue;
                    }
                case State.Number:
                    switch (c)
                    {
                        case -1:
                            CurrentValue *= sign;
                            return Token.Number;
                        case 'b':
                        case 'B':
                            state = State.BinaryNumberStart;
                            continue;
                        case 'x':
                        case 'X':
                            state = State.HexNumberStart;
                            continue;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            CurrentValue = CurrentValue * 10 + (c - '0');
                            continue;
                        default:
                            --iPos;
                            CurrentValue *= sign;
                            return Token.Number;
                    }
                case State.BinaryNumberStart:
                    switch (c)
                    {
                        case '0':
                        case '1':
                            CurrentValue = c - '0';
                            state = State.BinaryNumber;
                            continue;
                        default:
                            Unexpected(c);
                            continue;
                    }
                case State.BinaryNumber:
                    switch (c)
                    {
                        case -1:
                            CurrentValue *= sign;
                            return Token.Number;
                        case '0':
                        case '1':
                            CurrentValue = CurrentValue * 2 + (c - '0');
                            continue;
                        default:
                            --iPos;
                            CurrentValue *= sign;
                            return Token.Number;
                    }
                case State.HexNumberStart:
                    switch (c)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            CurrentValue = c - '0';
                            state = State.HexNumber;
                            continue;
                        case 'A':
                        case 'B':
                        case 'C':
                        case 'D':
                        case 'E':
                        case 'F':
                            CurrentValue = c - 'A' + 10;
                            state = State.HexNumber;
                            continue;
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                            CurrentValue = c - 'a' + 10;
                            state = State.HexNumber;
                            continue;
                        default:
                            Unexpected(c);
                            continue;
                    }
                case State.HexNumber:
                    switch (c)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            CurrentValue = CurrentValue * 16 + (c - '0');
                            state = State.HexNumber;
                            continue;
                        case 'A':
                        case 'B':
                        case 'C':
                        case 'D':
                        case 'E':
                        case 'F':
                            CurrentValue = CurrentValue * 16 + (c - 'A' + 10);
                            state = State.HexNumber;
                            continue;
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                            CurrentValue = CurrentValue * 16 + (c - 'a' + 10);
                            state = State.HexNumber;
                            continue;
                        default:
                            --iPos;
                            CurrentValue *= sign;
                            return Token.Number;
                    }
                case State.Word:
                    switch (c)
                    {
                        case -1:
                            return MaybeEquate();
                        default:
                            if (char.IsAsciiLetterOrDigit((char)c) || c == '_')
                            {
                                sb.Append((char)c);
                                continue;
                            }
                            --iPos;
                            return MaybeEquate();
                    }
                case State.Cr:
                    switch (c)
                    {
                        case -1:
                            return Token.EndLine;
                        case '\n':
                            ++linenumber;
                            return Token.EndLine;
                        default:
                            ++linenumber;
                            --iPos;
                            return Token.EndLine;
                    }
                case State.Comment:
                    switch (c)
                    {
                        case -1:
                            return Token.EndLine;
                        case '\n':
                            ++linenumber;
                            return Token.EndLine;
                        case '\r':
                            state = State.Cr;
                            continue;
                        default:
                            continue;
                    }
                case State.Minus:
                    switch (c)
                    {
                        case -1:
                        default:
                            this.Unexpected('-');
                            continue;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            sign = -1;
                            CurrentValue = c - '0';
                            state = State.Number;
                            continue;

                    }
                case State.StringLiteral:
                    switch (c)
                    {
                        case -1:
                            Error("Unterminated string.");
                            return Token.StringLiteral;
                        case '\r':
                        case '\n':
                            Error("Unterminated string.");
                            --iPos;
                            return Token.StringLiteral;
                        case '"':
                            this.currentString = sb.ToString();
                            return Token.StringLiteral;
                        case '\\':
                            state = State.EscapedCharacter;
                            continue;
                        default:
                            sb.Append((char)c);
                            continue;
                    }
                case State.EscapedCharacter:
                    switch (c)
                    {
                        case -1:
                            Error("Unterminated string.");
                            return Token.StringLiteral;
                        case '\r':
                        case '\n':
                            Error("Unterminated string.");
                            --iPos;
                            return Token.StringLiteral;
                        case 't':
                            sb.Append('\t');
                            state = State.StringLiteral;
                            continue;
                        case 'n':
                            sb.Append('\n');
                            state = State.StringLiteral;
                            continue;
                        case 'r':
                            sb.Append('\r');
                            state = State.StringLiteral;
                            continue;
                        case '0':
                            sb.Append('\0');
                            state = State.StringLiteral;
                            continue;
                        case '\\':
                            sb.Append('\\');
                            state = State.StringLiteral;
                            continue;
                        default:
                            Error("Invalid escape sequence in string.");
                            state = State.StringLiteral;
                            continue;
                    }
            }
        }
    }

    private Token MaybeEquate()
    {
        this.currentString = sb.ToString();
        var s = GetTokenString();
        if (equates.TryGetValue(s, out var value))
        {
            this.CurrentValue = value;
            return Token.Number;
        }
        return Token.Word;
    }

    public string GetTokenString()
    {
        return this.currentString;
    }

    private void Unexpected(int c)
    {
        Error($"Unexpected character '{(char)c}' (ASCII: {(int)c:X2})");
    }

    private enum State
    {
        Start,
        Number,
        BinaryNumberStart,
        BinaryNumber,
        HexNumberStart,
        HexNumber,
        Word,
        Cr,
        Comment,
        Minus,
        StringLiteral,
        EscapedCharacter,
    }
}

public enum Token
{
    EOF,
    Number,
    Colon,
    LParen,
    RParen,
    Word,
    EndLine,
    StringLiteral,
}