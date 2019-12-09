namespace Kinovea.ScreenManager.Data.Rtf
{
    public enum TokenType
    {
        Illegal,
        EOF,
        Whitespace,
        Text,
        Number,
        Newline,
        Control,
        GroupStart,
        GroupEnd,
        SemiColon,
    }
}