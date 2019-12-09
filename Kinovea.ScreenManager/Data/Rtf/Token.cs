namespace Kinovea.ScreenManager.Data.Rtf
{
    public class Token
    {
        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public Token(TokenType type, string value, int lineNumber, int linePosition)
            : this(type, value)
        {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }

        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public TokenType Type { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return "{" + this.Value + ":" + this.Type.ToString() + "}";
        }
    }
}