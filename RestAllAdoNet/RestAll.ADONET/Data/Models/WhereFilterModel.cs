#nullable disable
namespace RESTAll.Data.Models
{
    public class WhereFilterModel
    {
        public string Alias { set; get; }
        public string ColumnName { set; get; }
        public ValueTypes ValueTypes { set; get; }
        public object Value { set; get; }
        public bool IsList { set; get; }
    }
}
