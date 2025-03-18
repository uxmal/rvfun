using System.Text;
using rvfun;

namespace rvfun.UnitTests;

[TestFixture]
public class TextAssemblerTests
{
    [Test]
    public void Tasm_Number()
    {
        var testAsm = " 1 ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(1));

        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.EOF));
    }

    [Test]
    public void Tasm_Number_Long()
    {
        var testAsm = " 12 ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(12));
    }

    [Test]
    public void Task_Colon()
    {
        var testAsm = " : ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Colon));
    }

    [Test]
    public void Task_Parens()
    {
        var testAsm = ",() ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.LParen));
        var token2 = tasm.GetToken();
        Assert.That(token2, Is.EqualTo(Token.RParen));
    }

    [Test]
    public void Task_BinaryNumber()
    {
        var testAsm = " 0b1 ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(1));
    }

    [Test]
    public void Task_LongBinaryNumber()
    {
        var testAsm = " 0b1010 ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(10));
    }

    [Test]
    public void Task_HexNumber()
    {
        var testAsm = " 0xf ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(15));
    }

    
    [Test]
    public void Task_LongHexNumber()
    {
        var testAsm = " 0xff ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Number));
        Assert.That(tasm.CurrentValue, Is.EqualTo(255));
    }

    [Test]
    public void Task_Word()
    {
        var testAsm = " b ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("b"));
    }

        [Test]
    public void Task_Word_Underscore()
    {
        var testAsm = " _ ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("_"));
    }

            [Test]
    public void Task_Word_UnderscoreDigit()
    {
        var testAsm = " _2 ";
        var tasm = new TextAssembler(Encoding.UTF8.GetBytes(testAsm));
        var token = tasm.GetToken();
        Assert.That(token, Is.EqualTo(Token.Word));
        Assert.That(tasm.GetTokenString(), Is.EqualTo("_2"));
    }
}

