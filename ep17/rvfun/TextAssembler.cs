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

    public TextAssembler(byte[] bytes, Logger logger)
    {
        this.bytes = bytes;
        this.iPos = 0;
        this.linenumber = 1;
        this.valid = true;
        this.Symbols = [];
        this.previousToken = Token.EOF;
        this.asm = new Assembler(logger);
    }

    public int CurrentValue { get; private set; }
    public Dictionary<string, Symbol> Symbols { get; }
    public AssemblerSection Section => asm.Section;

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
            var symbol = new Symbol(w, Section.Position);
            AddSymbol(w, symbol);
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
        switch (w)
        {
            default:
                throw new NotImplementedException($"Unknown mnemonic or directive '{w}'.");
        }
        return false;
    }

    private static AsmEncoder [] encoders = [
        new AsmEncoder("addi", 0b000_00000_0010011, AsmI),
        new AsmEncoder("add", 0b0000000_0000000000_000_00000_0110011, AsmR),
        new AsmEncoder("sub", 0b0100000_0000000000_000_00000_0110011, AsmR)
    ] ;

    private class AsmEncoder{
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

    
    private static void AsmI(TextAssembler asm, uint opcode)
    {
        var dst = asm.ExpectNumber();
        var src1 = asm.ExpectNumber();
        var src2 = asm.ExpectNumber();
        asm.ExpectEndLine();
        asm.asm.asmI(opcode, dst, src1, src2);
    }

    private static void AsmR(TextAssembler asm, uint opcode)
    {
        var dst = asm.ExpectNumber();
        var src1 = asm.ExpectNumber();
        var src2 = asm.ExpectNumber();
        asm.ExpectEndLine();
        asm.asm.asmR(opcode, dst, src1, src2);
    }

    private void ExpectEndLine()
    {
        var token = GetToken();
        if (token == Token.EOF || token == Token.EndLine)
            return;
        Expected("an end of line");
        SkipUntil(Token.EndLine);
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

    private void Expected(string expected)
    {
        Console.WriteLine($"error({linenumber}): Expected {expected}.");
        this.valid = false;
    }

    private void UnexpectedToken(Token token)
    {
        Console.WriteLine($"error({linenumber}): Unexpected token '{token}'.");
        valid = false;
    }

    private bool SkipUntil(Token endLine)
    {
        for (; ; )
        {
            var token = GetToken();
            if (token == Token.EOF)
                return false;
            else if (token == Token.EndLine)
                return true;
        }
    }

    private void AddSymbol(string name, Symbol symbol)
    {
        if (Symbols.TryAdd(name, symbol))
            return;
        Console.WriteLine($"error({linenumber}): Duplicate symbol '{name}'.");
        this.valid = false;
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
                            return Token.Number;
                        case '0':
                        case '1':
                            CurrentValue = CurrentValue * 2 + (c - '0');
                            continue;
                        default:
                            --iPos;
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
                            return Token.Number;
                    }
                case State.Word:
                    switch (c)
                    {
                        case -1:
                            return Token.Word;
                        default:
                            if (char.IsAsciiLetterOrDigit((char)c) || c == '_')
                                continue;
                            --iPos;
                            return Token.Word;
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
            }
        }
    }

    public string GetTokenString()
    {
        return Encoding.UTF8.GetString(this.bytes, iBeginPos, iPos - iBeginPos);
    }

    private void Unexpected(int c)
    {
        Console.WriteLine($"error({linenumber}): Unexpected character '{c}' (ASCII: {(int)c:X2})");
        this.valid = false;
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
}