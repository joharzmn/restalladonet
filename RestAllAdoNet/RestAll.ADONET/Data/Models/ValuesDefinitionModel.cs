namespace RESTAll.Data.Models;
#nullable disable
public class ValuesDefinitionModel
{
    public object Value { set; get; }
    public ValueTypes ValueTypes { set; get; }
    public string Name { set; get; }
    public string Table { set; get; }
    public bool IsList { set; get; }
}

public enum ValueTypes
{
    String,
    Int,
    Double,
    Date,
    Variable
}