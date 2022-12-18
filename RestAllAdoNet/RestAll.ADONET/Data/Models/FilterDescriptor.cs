namespace RESTAll.Data.Models;
#nullable disable
public class FilterDescriptor
{
    public string ColumnName { set; get; }
    public string Operator { set; get; }
    public object Value { set; get; }
    public QueryFilterType FilterType { set; get; }
}

public enum QueryFilterType
{
    Value,
    Parameter,
    SubQuery
}