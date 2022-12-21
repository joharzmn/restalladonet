using Microsoft.SqlServer.TransactSql.ScriptDom;
using RESTAll.Data.Models;
using SampleConsole.Models;

#nullable disable
namespace RESTAll.Data.Parser
{
    public class ColumnValueVisitor : TSqlFragmentVisitor
    {
        private List<ColumnDefinitionModel> _columns { get; }
        public List<ValuesDefinitionModel> ColumnValues { get; }
        private int currentIndex = 0;
        public ColumnValueVisitor(List<ColumnDefinitionModel> columns)
        {
            _columns = columns;
            ColumnValues = new List<ValuesDefinitionModel>();
        }

        public override void Visit(ScalarExpression rowValue)
        {
            if (rowValue is not ColumnReferenceExpression)
            {
                var column = _columns[currentIndex];
                var columnValue = new ValuesDefinitionModel();
                if (rowValue is StringLiteral sl)
                {
                    columnValue.Value = sl.Value;
                    columnValue.ValueTypes = ValueTypes.String;
                    columnValue.Name = column.Name;
                    columnValue.Table = column.Table;
                }

                if (rowValue is IntegerLiteral il)
                {
                    columnValue.Value = il.Value;
                    columnValue.ValueTypes = ValueTypes.Int;
                    columnValue.Name = column.Name;
                }

                if (rowValue is MoneyLiteral ml)
                {
                    columnValue.Value = ml.Value;
                    columnValue.ValueTypes = ValueTypes.Double;
                    columnValue.Name = column.Name;
                }

                if (rowValue is RealLiteral rl)
                {
                    columnValue.Value = rl.Value;
                    columnValue.ValueTypes = ValueTypes.Double;
                    columnValue.Name = column.Name;
                }

                if (rowValue is NumericLiteral nl)
                {
                    columnValue.Value = nl.Value;
                    columnValue.ValueTypes = ValueTypes.Double;
                    columnValue.Name = column.Name;
                }

                if (rowValue is NullLiteral nul)
                {
                    columnValue.Value = nul.Value;
                    columnValue.ValueTypes = ValueTypes.String;
                    columnValue.Name = column.Name;
                }

                if (rowValue is VariableReference vr)
                {
                    columnValue.Value = vr.Name;
                    columnValue.ValueTypes = ValueTypes.Variable;
                    columnValue.Name = column.Name;
                }
                ColumnValues.Add(columnValue);
                currentIndex++;
            }
           
        }
    }
}
