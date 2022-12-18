using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSQL.Expressions;
using TSQL;
using TSQL.Tokens;

namespace RESTAll.Parser
{
    internal class TSQLCaseExpressionParser
    {
        public TSQLCaseExpression Parse(TSQLTokenizer tokenizer)
        {
            TSQLCaseExpression caseExpression = new TSQLCaseExpression();

            if (!tokenizer.Current.IsKeyword(TSQLKeywords.CASE))
            {
                throw new InvalidOperationException("CASE expected.");
            }

            caseExpression.Tokens.Add(tokenizer.Current);

            TSQLTokenParserHelper.ReadUntilStop(
                tokenizer,
                caseExpression,
                new List<TSQLFutureKeywords>() { },
                new List<TSQLKeywords>() {
                    TSQLKeywords.END
                },
                lookForStatementStarts: false);

            // this is different than the other clauses because the
            // stop word is still part of the expression instead of
            // being part of the next expression or clause like in
            // the other parsers
            caseExpression.Tokens.Add(tokenizer.Current);

            tokenizer.MoveNext();

            return caseExpression;
        }
    }
}
