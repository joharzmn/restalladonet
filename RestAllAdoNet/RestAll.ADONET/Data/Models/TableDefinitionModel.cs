using SampleConsole.Models;

namespace RESTAll.Data.Models;
#nullable disable
public class TableDefinitionModel
{
    public TableDefinitionModel()
    {
        Columns = new();
        Values = new();
        Comparisons = new();
        Filters = new();
    }
    public string Name { set; get; }
    public string Schema { set; get; }
    public string Alias { set; get; }
    public List<ColumnDefinitionModel> Columns { set; get; }
    public StatementType Operation { set; get; }
    public string ActionTable { set; get; }
    public List<ValuesDefinitionModel> Values { set; get; }
    public List<ComparisonModel> Comparisons { set; get; }
    public List<WhereFilterModel> Filters { set; get; }
}