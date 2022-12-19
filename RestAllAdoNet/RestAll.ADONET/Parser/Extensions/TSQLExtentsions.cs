using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSQL;
using TSQL.Tokens;
#nullable disable
namespace RESTAll.Parser.Extensions
{
    public static class TSQLExtentsions
    {
        public static TSQLToken NextToken(this List<TSQLToken> tokens, TSQLToken currneToken)
        {
            var tokenIndex = tokens.IndexOf(currneToken);
            if (tokenIndex < tokens.Count - 1)
            {
                return tokens[tokenIndex + 1];
            }

            return null;
        }

        public static TSQLToken StartToken(this List<TSQLToken> tokens, string text)
        {
            return tokens.FirstOrDefault(x => x.Text == text);
        }

        public static string ToStringStatement(this List<TSQLToken> tokens)
        {
            var sb = new StringBuilder();
            foreach (var tsqlToken in tokens)
            {

                sb.Append($"{tsqlToken.Text} ");
            }

            return sb.ToString();
        }

        public static string ToStringStatement(this List<TSQLToken> tokens, TSQLToken startToken, TSQLToken lastToken)
        {
            var sb = new StringBuilder();
            if (startToken != null && lastToken != null)
            {
                foreach (var tsqlToken in tokens.FindAll(x => x.BeginPosition >= startToken.BeginPosition && x.EndPosition <= lastToken.EndPosition - 1))
                {

                    sb.Append($"{tsqlToken.Text} ");
                }

                tokens.RemoveAll(x =>
                    x.BeginPosition >= startToken.BeginPosition && x.EndPosition <= lastToken.EndPosition);
                return sb.ToString();
            }

            return "";
        }

        public static List<TSQLToken> GetBetween(this List<TSQLToken> tokens,TSQLToken startToken, TSQLToken endToken)
        {
            return tokens.Where(x =>
                x.BeginPosition > startToken.BeginPosition && x.EndPosition < endToken.EndPosition).ToList();
        }

        public static List<TSQLToken> GetBetween(this List<TSQLToken> tokens, TSQLToken startToken, TSQLToken endToken, TSQLCharacters skip)
        {
            return tokens.Where(x =>
                x.BeginPosition > startToken.BeginPosition && x.EndPosition < endToken.EndPosition && !x.IsCharacter(skip)).ToList();
        }
    }
}
