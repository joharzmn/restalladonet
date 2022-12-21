using Microsoft.SqlServer.TransactSql.ScriptDom;
using RESTAll.Data.Models;

#nullable disable
namespace RESTAll.Data.Parser
{
    internal class WhereComparisonVisitor : TSqlFragmentVisitor
    {
        private bool _hasAlias = false;
        public string Name { set; get; }
        public string Alias { set; get; }
        public object Value { set; get; }
        public ValueTypes Type { set; get; }
        private int CurrentIdentifier { set; get; }
        public override void Visit(ColumnReferenceExpression fragment)
        {
            if (fragment.MultiPartIdentifier.Identifiers.Count > 1)
            {
                _hasAlias = true;
            }
        }

        public void Reset()
        {
            _hasAlias = false;
            CurrentIdentifier = 0;
        }

        public override void Visit(Identifier fragment)
        {
            if (_hasAlias)
            {
                if (CurrentIdentifier == 0)
                {
                    Alias = fragment.Value;

                }

                if (CurrentIdentifier == 1)
                {
                    Name = fragment.Value;
                    CurrentIdentifier = 0;
                }
                CurrentIdentifier++;
            }
            else
            {
                Name = fragment.Value;
            }

        }

        public override void Visit(IntegerLiteral integer)
        {

            Value = integer.Value;
            Type = ValueTypes.Int;
        }

        public override void Visit(StringLiteral stringLiteral)
        {
            Value = stringLiteral.Value;
            Type = ValueTypes.String;
        }

        public override void Visit(NumericLiteral stringLiteral)
        {
            Value = stringLiteral.Value;
            Type = ValueTypes.Int;
        }

        public override void Visit(MoneyLiteral stringLiteral)
        {
            Value = stringLiteral.Value;
            Type = ValueTypes.Double;
        }

        public override void Visit(RealLiteral stringLiteral)
        {
            Value = stringLiteral.Value;
            Type = ValueTypes.Double;
        }

        public override void Visit(NullLiteral stringLiteral)
        {
            Value = stringLiteral.Value;
            Type = ValueTypes.String;
        }

        public override void Visit(VariableReference variableReference)
        {
            Value = variableReference.Name;
            Type = ValueTypes.Variable;
        }
    }
}
