using Microsoft.SqlServer.TransactSql.ScriptDom;
using RESTAll.Data.Models;
using SampleConsole.Models;

#nullable disable
namespace RESTAll.Data.Parser
{
    internal class WhereVisitor : TSqlFragmentVisitor
    {
        public List<WhereFilterModel> Filters { set; get; }

        public WhereVisitor()
        {
            Filters = new();
        }
        public override void Visit(BooleanBinaryExpression fragment)
        {
        }

        public override void Visit(InPredicate predicate)
        {
            var visitor = new WhereComparisonVisitor();
            predicate.Expression.Accept(visitor);
            var valuesList = new List<object>();
            ValueTypes types = ValueTypes.String;
            if (predicate.Values.FirstOrDefault() is StringLiteral)
            {
                types = ValueTypes.String;
                foreach (var scalarExpression1 in predicate.Values)
                {
                    var scalarExpression = (StringLiteral)scalarExpression1;
                    valuesList.Add(scalarExpression.Value);
                }
            }

            if (predicate.Values.FirstOrDefault() is IntegerLiteral)
            {
                foreach (var scalarExpression1 in predicate.Values)
                {
                    var scalarExpression = (IntegerLiteral)scalarExpression1;
                    valuesList.Add(Convert.ToInt64(scalarExpression.Value));
                }
                types = ValueTypes.Int;
            }

            if (predicate.Values.FirstOrDefault() is MoneyLiteral)
            {
                foreach (var scalarExpression1 in predicate.Values)
                {
                    var scalarExpression = (MoneyLiteral)scalarExpression1;
                    valuesList.Add(Convert.ToDouble(scalarExpression.Value));
                }
                types = ValueTypes.Double;
            }

            if (predicate.Values.FirstOrDefault() is NumericLiteral)
            {
                foreach (var scalarExpression1 in predicate.Values)
                {
                    var scalarExpression = (NumericLiteral)scalarExpression1;
                    valuesList.Add(Convert.ToDouble(scalarExpression.Value));
                }

                types = ValueTypes.Int;
            }

            if (predicate.Values.FirstOrDefault() is RealLiteral)
            {
                foreach (var scalarExpression1 in predicate.Values)
                {
                    var scalarExpression = (RealLiteral)scalarExpression1;
                    valuesList.Add(Convert.ToDouble(scalarExpression.Value));
                }

                types = ValueTypes.Double;
            }

            Filters.Add(new WhereFilterModel()
            {
                Value = string.Join(",", valuesList),
                ValueTypes = types,
                Alias = visitor.Alias,
                ColumnName = visitor.Name,
                IsList = true
            });
        }

        public override void Visit(ColumnReferenceExpression fragment)
        {

        }

        public override void Visit(Identifier identifier)
        {

        }

        public override void Visit(BooleanComparisonExpression fragment)
        {
            var firstColumn = new WhereComparisonVisitor();
            fragment.FirstExpression.Accept(firstColumn);
            firstColumn.Reset();
            fragment.SecondExpression.Accept(firstColumn);

            Filters.Add(new WhereFilterModel()
            {
                Value = firstColumn.Value,
                ValueTypes = firstColumn.Type,
                Alias = firstColumn.Alias,
                ColumnName = firstColumn.Name
            });
        }
    }


}
