using System.ComponentModel;

namespace RESTAll.Data.Models;

public enum StatementType
{
    [Description("select")]
    Select,
    [Description("insert")]
    Insert,
    [Description("insert")]
    InsertWithSelect,
    [Description("update")]
    Update,
    [Description("update")]
    UpdateWithSelect,
    [Description("delete")]
    Delete
}