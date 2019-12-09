using Kinovea.ScreenManager.Data.Rtf;
using System.Collections.Generic;

namespace Kinovea.Rtf.Tests
{
    internal class TokenComparer : IEqualityComparer<Token>
    {
        public bool Equals(Token x, Token y)
        {
            return (x.Type == y.Type)
                && (x.Value == y.Value);
        }

        public int GetHashCode(Token token)
        {
            return token.GetHashCode();
        }
    }
}