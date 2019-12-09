using Kinovea.ScreenManager.Data.Rtf;
using System.Collections.Generic;
using Xunit;

namespace Kinovea.Rtf.Tests
{
    public class TokenStreamTests
    {
        [Theory]
        [InlineData("\\rtf1", TokenType.Control, TokenType.Number)]
        [InlineData("\\b bold \\b0", TokenType.Control, TokenType.Whitespace, TokenType.Text, TokenType.Whitespace, TokenType.Control, TokenType.Number)]
        [InlineData("\\ulnone\\par", TokenType.Control, TokenType.Control)]
        [InlineData("{\\fonttbl{\\f0\\fnil\\fcharset0 Arial;}}", TokenType.GroupStart, TokenType.Control, TokenType.GroupStart, TokenType.Control, TokenType.Number, TokenType.Control, TokenType.Control, TokenType.Number, TokenType.Whitespace, TokenType.Text, TokenType.SemiColon, TokenType.GroupEnd, TokenType.GroupEnd)]
        [InlineData("{\\*\\generator Msftedit 5.41.21.2510;}", TokenType.GroupStart, TokenType.Control, TokenType.Control, TokenType.Whitespace, TokenType.Text, TokenType.Whitespace, TokenType.Illegal, TokenType.SemiColon, TokenType.GroupEnd)]
        public void ReadSequenceTheory(string input, params TokenType[] expected)
        {
            var stream = TokenStream.New(input);
            var actual = new List<TokenType>();
            var token = stream.Read();
            while (token.Type != TokenType.EOF)
            {
                actual.Add(token.Type);
                token = stream.Read();
            }

            Assert.Equal(expected, actual.ToArray());
        }

        [Theory]
        [InlineData("", TokenType.EOF, "")]
        [InlineData(" ", TokenType.Whitespace, " ")]
        [InlineData("\t", TokenType.Whitespace, "\t")]
        [InlineData("\r", TokenType.Whitespace, "\r")]
        [InlineData("  ", TokenType.Whitespace, "  ")]
        [InlineData("\n", TokenType.Newline, "\n")]
        [InlineData(";", TokenType.SemiColon, ";")]
        [InlineData("{", TokenType.GroupStart, "{")]
        [InlineData("}", TokenType.GroupEnd, "}")]
        [InlineData("\\rtf", TokenType.Control, "rtf")]
        [InlineData("test", TokenType.Text, "test")]
        [InlineData("test1", TokenType.Text, "test1")]
        [InlineData("1", TokenType.Number, "1")]
        [InlineData("-1", TokenType.Number, "-1")]
        [InlineData("1.0", TokenType.Number, "1.0")]
        [InlineData("-1.0", TokenType.Number, "-1.0")]
        public void ReadTheory(string input, TokenType expectedType, string expectedValue)
        {
            var stream = TokenStream.New(input);
            var token = stream.Read();
            var expected = new Token(expectedType, expectedValue);
            Assert.Equal(expected, token, new TokenComparer());
        }
    }
}