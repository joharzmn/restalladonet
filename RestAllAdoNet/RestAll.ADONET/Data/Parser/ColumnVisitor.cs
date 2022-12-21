using Microsoft.SqlServer.TransactSql.ScriptDom;
#nullable disable
namespace RESTAll.Data.Parser
{
    public class ColumnVisitor : TSqlFragmentVisitor
    {
        public string Name { set; get; }
        public string Schema { set; get; }
        public string Alias { set; get; }
        private bool _hasSchema = false;
        private int currentIdentifier = 0;
        public override void Visit(ColumnReferenceExpression column)
        {
            if (column.MultiPartIdentifier.Identifiers.Count > 1)
            {
                _hasSchema = true;
            }
        }

        public override void Visit(IdentifierOrValueExpression fragment)
        {
            Alias = fragment.Value;
        }

        public void Reset()
        {
            _hasSchema = false;
            currentIdentifier = 0;
        }

        public override void Visit(Identifier identifier)
        {
            if (_hasSchema)
            {
                if (currentIdentifier == 0)
                {
                    Schema = identifier.Value;
                }

                if (currentIdentifier == 1)
                {
                    Name = identifier.Value;
                }

                currentIdentifier++;
            }
            else
            {
                if (Alias != identifier.Value)
                {
                    Name = identifier.Value;
                }
                
            }
        }
    }
}
