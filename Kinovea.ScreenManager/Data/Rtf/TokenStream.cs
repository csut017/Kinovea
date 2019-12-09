using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kinovea.ScreenManager.Data.Rtf
{
    /// <summary>
    /// Splits an incoming stream of characters into RTF tokens that can be used by a higher level processor.
    /// </summary>
    public class TokenStream
    {
        private readonly IDictionary<char, TokenType> _chars = new Dictionary<char, TokenType>
        {
            {';', TokenType.SemiColon },
            {'{', TokenType.GroupStart },
            {'}', TokenType.GroupEnd }
        };

        private readonly TextReader _reader;
        private bool _hasChar;
        private int _lastChar;
        private int _lastTokenStart;
        private int _lineNumber;
        private int _linePosition;

        public TokenStream(TextReader reader)
        {
            this._reader = reader;
        }

        public static TokenStream New(string input)
        {
            var reader = new StringReader(input);
            return new TokenStream(reader);
        }

        public Token Read()
        {
            this._lastTokenStart = this._linePosition - (this._hasChar ? 1 : 0);
            var inputChar = this.ReadNextCharacter();

            if (inputChar < 0)
            {
                return this.MakeToken(TokenType.EOF, "");
            }

            var charToCheck = (char)inputChar;
            if (this._chars.TryGetValue(charToCheck, out TokenType tokenType)) return this.MakeToken(tokenType, charToCheck.ToString());
            if (charToCheck == '\n')
            {
                var token = this.MakeToken(TokenType.Newline, "\n");
                this._linePosition = 0;
                this._lineNumber++;
                return token;
            };
            if (char.IsWhiteSpace(charToCheck)) return this.GenerateWhitespace();
            if (char.IsDigit(charToCheck) || (charToCheck == '-')) return this.GenerateNumber(charToCheck);
            if (charToCheck == '\\') return this.GenerateControl();
            return this.GenerateText(charToCheck);
        }

        private Token GenerateControl()
        {
            var builder = new StringBuilder();
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (!char.IsLetter(charToCheck) && (charToCheck != '*')) break;
                builder.Append(charToCheck);
            }

            this.Unread();
            return this.MakeToken(TokenType.Control, builder.ToString());
        }

        private Token GenerateNumber(char firstChar)
        {
            var builder = new StringBuilder();
            builder.Append(firstChar);
            var hasDecimal = false;
            var isLegal = true;
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsDigit(charToCheck))
                {
                    builder.Append(charToCheck);
                }
                else if (charToCheck == '.')
                {
                    builder.Append(charToCheck);
                    if (hasDecimal)
                    {
                        isLegal = false;
                    }
                    else
                    {
                        hasDecimal = true;
                    }
                }
                else
                {
                    this.Unread();
                    break;
                }
            }

            var finalNumber = builder.ToString();
            isLegal &= finalNumber != "-";
            return this.MakeToken(isLegal ? TokenType.Number : TokenType.Illegal, finalNumber);
        }

        private Token GenerateText(char firstChar)
        {
            var builder = new StringBuilder();
            builder.Append(firstChar);
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsWhiteSpace(charToCheck) || (charToCheck == '\\') || _chars.ContainsKey(charToCheck)) break;
                builder.Append(charToCheck);
            }

            this.Unread();
            return this.MakeToken(TokenType.Text, builder.ToString());
        }

        private Token GenerateWhitespace()
        {
            var builder = new StringBuilder();
            builder.Append((char)this._lastChar);
            while (true)
            {
                var nextChar = this.ReadNextCharacter();
                if (nextChar < 0) break;

                var charToCheck = (char)nextChar;
                if (char.IsWhiteSpace(charToCheck) && (charToCheck != '\n'))
                {
                    builder.Append(charToCheck);
                }
                else
                {
                    this.Unread();
                    break;
                }
            }

            return this.MakeToken(TokenType.Whitespace, builder.ToString());
        }

        private Token MakeToken(TokenType type, string value)
        {
            return new Token(type, value, this._lineNumber, this._lastTokenStart);
        }

        private int ReadNextCharacter()
        {
            if (this._hasChar)
            {
                this._hasChar = false;
                return this._lastChar;
            }

            this._lastChar = this._reader.Read();
            this._linePosition++;
            return this._lastChar;
        }

        private void Unread()
        {
            this._hasChar = true;
        }
    }
}