using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RESTAll.Data.Contracts;
using RESTAll.Data.Models;
using SampleConsole.Models;

#nullable disable
namespace RESTAll.Data.Parser
{
    internal class QueryParser:IQueryParser
    {
        public  List<TableDefinitionModel> Parse(string queryText)
        {
            var parser = new TSql150Parser(true, SqlEngineType.All);
            using var textReader = new StringReader(queryText);
            var sqlTree = parser.Parse(textReader, out var errors);
            if (errors.Count > 0)
            {
                throw new Exception($@"SQL Parse Error: {string.Join("\n", errors.Select(x => x.Message))}");
            }

            var statementVisitor = new StatementVisitor();
            sqlTree.Accept(statementVisitor);
            return statementVisitor.Tables;
        }
    }
}
