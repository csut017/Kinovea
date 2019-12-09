using System;
using System.Text;

namespace Kinovea.ScreenManager.Data.Rtf
{
    /// <summary>
    /// A bare-bones RTF processor that will extract the main text from an RTF document.
    /// </summary>
    /// <remarks>
    /// This processor is mainly designed to work with RTF from the RTF WinForms control, but it is based on the RTF specification
    /// so it should be able to handle a wide variety of RTF inputs.
    /// </remarks>
    public class Processor
    {
        public static void Process(string text, Action<string, Format> writeOutput)
        {
            var rtf = TokenStream.New(text);
            var token = rtf.Read();
            var group = 0;
            var format = new Format();

            var output = new StringBuilder();
            (bool, bool) checkForSetting()
            {
                token = rtf.Read();
                if (token.Type == TokenType.Number)
                {
                    token = rtf.Read();
                    return (token.Value == "1", token.Type == TokenType.Whitespace);
                }
                else
                {
                    return (true, token.Type == TokenType.Whitespace);
                }
            }

            while (token.Type != TokenType.EOF)
            {
                var readNext = true;
                switch (token.Type)
                {
                    case TokenType.Text:
                    case TokenType.Whitespace:
                    case TokenType.SemiColon:
                        if (group < 2)
                        {   // Always has one top group - all other groups can be ignored
                            output.Append(token.Value);
                        }
                        break;

                    case TokenType.GroupStart:
                        group++;
                        break;

                    case TokenType.GroupEnd:
                        group--;
                        break;

                    case TokenType.Control:
                        switch (token.Value)
                        {
                            case "par":
                                output.Append(Environment.NewLine);
                                break;

                            case "plain":
                                if (output.Length > 0) writeOutput(output.ToString(), format);
                                output.Clear();
                                format = new Format();
                                token = rtf.Read();
                                readNext = token.Type == TokenType.Whitespace;
                                break;

                            case "ulnone":
                                if (output.Length > 0) writeOutput(output.ToString(), format);
                                output.Clear();
                                format.IsUnderline = false;
                                token = rtf.Read();
                                readNext = token.Type == TokenType.Whitespace;
                                break;

                            case "ul":
                                if (output.Length > 0) writeOutput(output.ToString(), format);
                                output.Clear();
                                (format.IsUnderline, readNext) = checkForSetting();
                                break;

                            case "b":
                                if (output.Length > 0) writeOutput(output.ToString(), format);
                                output.Clear();
                                (format.IsBold, readNext) = checkForSetting();
                                break;

                            case "i":
                                if (output.Length > 0) writeOutput(output.ToString(), format);
                                output.Clear();
                                (format.IsItalic, readNext) = checkForSetting();
                                break;

                            case "strike":
                                if (output.Length > 0) writeOutput(output.ToString(), format);
                                output.Clear();
                                (format.IsStrikethrough, readNext) = checkForSetting();
                                break;

                            default:
                                // Control codes are either followed by a number, whitespace or another control code
                                // If we have whitespace or a number we need to strip them
                                token = rtf.Read();
                                if (token.Type == TokenType.Number) token = rtf.Read();
                                readNext = token.Type == TokenType.Whitespace;
                                break;
                        }
                        break;
                }

                if (readNext) token = rtf.Read();
            }

            if (output.Length > 0) writeOutput(output.ToString(), format);
        }

        public struct Format
        {
            public bool IsBold { get; set; }

            public bool IsItalic { get; set; }

            public bool IsStrikethrough { get; set; }

            public bool IsUnderline { get; set; }
        }
    }
}