using Microsoft.SqlServer.TransactSql.ScriptDom;
using RESTAll.Data.Models;
using SampleConsole.Models;

#nullable disable
namespace RESTAll.Data.Parser
{
    internal class SelectStatementVisitor : TSqlFragmentVisitor
    {
        public List<TableDefinitionModel> Tables { get; }

        public SelectStatementVisitor()
        {
            Tables = new();
        }
        public override void Visit(QuerySpecification fragment)
        {
            foreach (var item in fragment.SelectElements)
            {
                var columnVisitor = new ColumnVisitor();
                item.Accept(columnVisitor);

            }
        }

        public override void Visit(QualifiedJoin fragment)
        {
            var firstTable = fragment.FirstTableReference as NamedTableReference;
            if (!Tables.Exists(x =>
                    x.Schema == firstTable.SchemaObject.SchemaIdentifier?.Value &&
                    x.Name == firstTable.SchemaObject.BaseIdentifier.Value))
            {
                Tables.Add(new TableDefinitionModel()
                {
                    Schema = firstTable.SchemaObject.SchemaIdentifier?.Value,
                    Alias = firstTable.Alias?.Value,
                    Name = firstTable.SchemaObject.BaseIdentifier.Value
                });
            }
            var secondTable = fragment.SecondTableReference as NamedTableReference;

            if (!Tables.Exists(x =>
                    x.Schema == secondTable.SchemaObject.SchemaIdentifier?.Value &&
                    x.Name == secondTable.SchemaObject.BaseIdentifier.Value))
            {
                Tables.Add(new TableDefinitionModel()
                {
                    Schema = secondTable.SchemaObject.SchemaIdentifier?.Value,
                    Alias = secondTable.Alias?.Value,
                    Name = secondTable.SchemaObject.BaseIdentifier.Value
                });
            }

            var expression = fragment.SearchCondition as BooleanComparisonExpression;
            var firstColumn = expression.FirstExpression as ColumnReferenceExpression;
            var secondColumn = expression.SecondExpression as ColumnReferenceExpression;
            var visitor = new WhereComparisonVisitor();
            firstColumn.Accept(visitor);
            var table = Tables.FirstOrDefault(x => x.Schema == firstTable.SchemaObject?.SchemaIdentifier.Value &&
                                                   x.Name == firstTable.SchemaObject.BaseIdentifier.Value);
            table.Comparisons.Add(new ComparisonModel()
            {
                Alias = visitor.Alias,
                ColumnName = visitor.Name
            });
            visitor.Reset();

            secondColumn.Accept(visitor);
            table.Comparisons.Add(new ComparisonModel()
            {
                Alias = visitor.Alias,
                ColumnName = visitor.Name
            });
        }

        public override void Visit(NamedTableReference fragment)
        {
            if (!Tables.Exists(x =>
                    x.Schema == fragment.SchemaObject.SchemaIdentifier?.Value &&
                    x.Name == fragment.SchemaObject.BaseIdentifier.Value))
            {
                Tables.Add(new TableDefinitionModel()
                {
                    Schema = fragment.SchemaObject.SchemaIdentifier?.Value,
                    Name = fragment.SchemaObject.BaseIdentifier.Value,
                    Alias = fragment.Alias?.Value
                });
            }

        }
    }
}
