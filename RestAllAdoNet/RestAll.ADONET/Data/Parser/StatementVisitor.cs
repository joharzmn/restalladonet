using Microsoft.SqlServer.TransactSql.ScriptDom;
using RESTAll.Data.Models;
using SampleConsole.Models;

#nullable disable
namespace RESTAll.Data.Parser
{
    public class StatementVisitor : TSqlFragmentVisitor
    {

       
        public List<TableDefinitionModel> Tables { get; }
        public StatementVisitor()
        {
            Tables = new List<TableDefinitionModel>();

        }

        public override void Visit(InsertStatement node)
        {
            var table = new TableDefinitionModel();
            var target = node.InsertSpecification.Target as NamedTableReference;
            table.Name = target.SchemaObject.BaseIdentifier.Value;
            table.Schema = target.SchemaObject.SchemaIdentifier?.Value;
            var columnVisitor = new ColumnVisitor();
            foreach (var column in node.InsertSpecification.Columns)
            {

                column.Accept(columnVisitor);
                columnVisitor.Reset();
                table.Columns.Add(new ColumnDefinitionModel()
                {
                    Name = columnVisitor.Name,
                    Table = table.Name
                });
            }

            var columnValuesVisitor = new ColumnValueVisitor(table.Columns);
            if (node.InsertSpecification.InsertSource is SelectInsertSource sis)
            {
                table.Operation = StatementType.InsertSelect;
                var tableVisitor = new TableVisitor();
                sis.Select.Accept(tableVisitor);
                table.ActionTable = tableVisitor.Name;
                var whereVisitor = new WhereVisitor();
                sis.Accept(whereVisitor);
                table.Filters.AddRange(whereVisitor.Filters);
            }
            else
            {
                table.Operation = StatementType.Insert;
            }
            node.InsertSpecification.InsertSource.Accept(columnValuesVisitor);
            table.Values.AddRange(columnValuesVisitor.ColumnValues);
            Tables.Add(table);
        }

        public override void Visit(SelectStatement node)
        {
            var selectStatementVisitor = new SelectStatementVisitor();
            node.Accept(selectStatementVisitor);
            var whereVisitor = new WhereVisitor();
            var tableVisitor = new TableVisitor();
            node.Accept(tableVisitor);
            node.QueryExpression.Accept(whereVisitor);
            Tables.Add(new TableDefinitionModel()
            {
                Schema = tableVisitor.Schema,
                Alias = tableVisitor.Alias,
                Operation = StatementType.Select,
                ActionTable = tableVisitor.Name,
                Filters = whereVisitor.Filters,
                Name = tableVisitor.Name
            });
        }


        public override void Visit(UpdateStatement node)
        {
            var tableReference = node.UpdateSpecification.Target as NamedTableReference;
            var table = new TableDefinitionModel
            {
                Schema = tableReference.SchemaObject.SchemaIdentifier?.Value,
                Alias = tableReference.Alias?.Value,
                Name = tableReference.SchemaObject.BaseIdentifier.Value,
                Operation = StatementType.Update
            };
            foreach (var setClause in node.UpdateSpecification.SetClauses)
            {
                var columnVisitor = new ColumnVisitor();
                setClause.Accept(columnVisitor);
                table.Columns.Add(new ColumnDefinitionModel()
                {
                    Alias = columnVisitor.Alias,
                    Name = columnVisitor.Name,
                    Table = table.Name
                });
            }

            foreach (var setClause in node.UpdateSpecification.SetClauses)
            {
                var columnVisitor = new ColumnValueVisitor(table.Columns);
                setClause.Accept(columnVisitor);
                table.Values.AddRange(columnVisitor.ColumnValues);

            }

            var whereVisitor = new WhereVisitor();
            node.UpdateSpecification.Accept(whereVisitor);
            table.Filters.AddRange(whereVisitor.Filters);
            Tables.Add(table);
        }

        public override void Visit(DeleteStatement node)
        {
            var table = new TableDefinitionModel();
            var tableReference = node.DeleteSpecification.Target as NamedTableReference;
            table.Schema = tableReference.SchemaObject.SchemaIdentifier?.Value;
            table.Name = tableReference.SchemaObject.BaseIdentifier.Value;
            table.Operation = StatementType.Delete;
            var whereVisitor = new WhereVisitor();
            node.DeleteSpecification.Accept(whereVisitor);
            table.Filters.AddRange(whereVisitor.Filters);

        }



    }
}
