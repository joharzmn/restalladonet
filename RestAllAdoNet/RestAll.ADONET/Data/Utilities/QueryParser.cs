using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RESTAll.Data.Contracts;
using RESTAll.Data.Models;
using TSQL;
using TSQL.Statements;
using TSQL.Tokens;

namespace RESTAll.Data.Utilities
{
    public class QueryParser : IQueryParser
    {
        public List<QueryDescriptor> Parse(string query)
        {

            var statements = TSQLStatementReader.ParseStatements(query);

            var visitor = new Visitor();
            foreach (var item in statements)
            {

                var statementDescriptor = visitor.VisitStatement(item);
                visitor.queryDescriptor.Add(statementDescriptor);
            }

            return visitor.queryDescriptor;
        }

    }
}
