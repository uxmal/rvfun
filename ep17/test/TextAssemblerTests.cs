using System.Text;
using rvfun;

namespace rvfun.UnitTests;

[TestFixture]
public class TextAssemblerTests
{
    private TextAssembler Given_TextAssembler(string testAsm)
    {
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm), new Logger());
        return tasm;
    }

    [Test]
    public void Tasm_Number()
    {
        var tasm = Given_TextAssembler(" 1 ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(1));

        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.EOF));
    }

    [Test]
    public void Tasm_Number_Long()
    {
        var tasm = Given_TextAssembler(" 12 ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(12));
    }

    [Test]
    public void Tasm_Colon()
    {
        var tasm = Given_TextAssembler(" : ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Colon));
    }

    [Test]
    public void Tasm_Parens()
    {
        var tasm = Given_TextAssembler(",() ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.LParen));
        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.RParen));
    }

    [Test]
    public void Tasm_BinaryNumber()
    {
        var tasm = Given_TextAssembler(" 0b1 ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(1));
    }

    [Test]
    public void Tasm_LongBinaryNumber()
    {
        var tasm = Given_TextAssembler(" 0b1010 ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(10));
    }

    [Test]
    public void Tasm_HexNumber()
    {
        var tasm = Given_TextAssembler(" 0xf ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(15));
    }


    [Test]
    public void Tasm_LongHexNumber()
    {
        var tasm = Given_TextAssembler(" 0xff ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(255));
    }

    [Test]
    public void Tasm_Word()
    {
        var tasm = Given_TextAssembler(" b ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("b"));
    }

    [Test]
    public void Tasm_Word_Underscore()
    {
        var tasm = Given_TextAssembler(" _ ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("_"));
    }

    [Test]
    public void Tasm_Word_UnderscoreDigit()
    {
        var tasm = Given_TextAssembler(" _2 ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("_2"));
    }

    [Test]
    public void Tasm_Unix()
    {
        var tasm = Given_TextAssembler("  \n ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.EndLine));
        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.EOF));
    }

    [Test]
    public void Tasm_Windows()
    {
        var tasm = Given_TextAssembler("  \r\n ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.EndLine));
        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.EOF));
    }

    [Test]
    public void Tasm_MacOsClassic()
    {
        var tasm = Given_TextAssembler("  \r ");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.EndLine));
        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.EOF));
    }

    [Test]
    public void Tasm_Comment()
    {
        var tasm = Given_TextAssembler(" hello ; comment\n world");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("hello"));
        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.EndLine));
        var token3 = tasm.GetToken();
        Assert.That(token3, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("world"));
    }

    [Test]
    public void Tasm_Directive()
    {
        var tasm = Given_TextAssembler(" .test");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo(".test"));
    }

    [Test]
    public void Tasm_OperandDirective()
    {
        var tasm = Given_TextAssembler(" %test");
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("%test"));
    }

    [Test]
    public void Tasm_Label()
    {
        var tasm = Given_TextAssembler("symbol:");
        tasm.AssembleLine();

        Assert.That(tasm.Symbols.ContainsKey("symbol"), Is.True);

    }

    [Test]
    public void Tasm_Peek()
    {
        var tasm = Given_TextAssembler("hello 1");
        Assert.That(tasm.PeekToken(), Is.EqualTo(Token.Word));
        Assert.That(tasm.PeekToken(), Is.EqualTo(Token.Word));
        Assert.That(tasm.GetToken(), Is.EqualTo(Token.Word));
        Assert.That(tasm.PeekToken(), Is.EqualTo(Token.Number));
    }

    [Test]
    public void Tasm_EmptyFile()
    {
        var tasm = Given_TextAssembler("");
        Assert.That(tasm.AssembleLine(), Is.False);
    }

    [Test]
    public void Tasm_TwoLines()
    {
        var tasm = Given_TextAssembler("\r\n");
        Assert.That(tasm.AssembleLine(), Is.True);
        Assert.That(tasm.AssembleLine(), Is.False);
    }

    [Test]
    public void Tasm_Line_not_beginning_with_word()
    {
        var asm = "3 a d\r\n";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(asm), new Logger());
        Assert.That(tasm.AssembleLine(), Is.True);
        Assert.That(tasm.AssembleFile(), Is.Null);
    }

    [Test]
    public void Tasm_Two_Labels_same_line()
    {
        var tasm = Given_TextAssembler("label: label2:\r\n");
        Assert.That(tasm.AssembleFile(), !Is.Null);
        Assert.That(tasm.Symbols.Count, Is.EqualTo(2));
        Assert.That(tasm.Symbols.ContainsKey("label"), Is.True);
        Assert.That(tasm.Symbols.ContainsKey("label2"), Is.True);
    }

    [Test]
    public void Tasm_addi()
    {
        RunTest(
            "   addi 2,0,1\r\n",
            m => m.addi(2, 0, 1));
    }

    [Test]
    public void Tasm_sub()
    {
        RunTest(
            "   sub 2,0,1\r\n",
            m => m.sub(2, 0, 1));
    }

    private void RunTest(string asmSource, Action<Assembler> builder)
    {
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(asmSource), new Logger());
        var section = tasm.AssembleFile();
        Assert.That(section, !Is.Null);

        var casm = new Assembler(new Logger());
        builder(casm);

        Assert.That(section.GetAssembledBytes(), Is.EqualTo(casm.Section.GetAssembledBytes()));
    }

    [Test]
    public void Tasm_add()
    {
        var asm = "   add 2,0,1\r\n";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(asm), new Logger());
        var section = tasm.AssembleFile();
        Assert.That(section, !Is.Null);

        var casm = new Assembler(new Logger());
        casm.add(2, 0, 1);

        Assert.That(section.GetAssembledBytes(), Is.EqualTo(casm.Section.GetAssembledBytes()));
    }
}

