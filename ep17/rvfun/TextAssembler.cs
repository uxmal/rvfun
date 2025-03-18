


using System.Text;

namespace rvfun;

public class TextAssembler
{
    private byte[] bytes;
    private int iPos;
    private int linenumber;
    private bool valid;
    private int iBeginPos;

    public TextAssembler(byte[] bytes)
    {
        this.bytes = bytes;
        this.iPos = 0;
        this.linenumber = 1;
        this.valid = true;
    }

    public int CurrentValue { get; private set; }

    public Token GetToken()
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
                        case ':':
                            return Token.Colon;
                        case '(':
                            return Token.LParen;
                        case ')':
                            return Token.RParen;
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
                            if (char.IsAsciiLetter((char)c) || c == '_')
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
}