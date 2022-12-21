using Microsoft.SqlServer.TransactSql.ScriptDom;
#nullable disable
namespace RESTAll.Data.Parser
{
    public class TableVisitor : TSqlFragmentVisitor
    {
        public string Name { set; get; }
        public string Schema { set; get; }
        public string Alias { set; get; }
        private bool visited = false;
        public override void Visit(NamedTableReference fragment)
        {
            if (!visited)
            {
                Name = fragment.SchemaObject.BaseIdentifier.Value;
                Schema = fragment.SchemaObject.SchemaIdentifier?.Value;
                Alias = fragment.Alias?.Value;
                visited = true;
            }
        }
    }
}
