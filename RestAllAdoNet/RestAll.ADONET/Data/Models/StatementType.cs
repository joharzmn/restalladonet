using System.ComponentModel;

namespace RESTAll.Data.Models;

public enum StatementType
{
    [Description("select")]
    Select,
    [Description("insert")]
    Insert,
    [Description("insert")]
    InsertSelect,
    [Description("update")]
    Update,
    [Description("update")]
    UpdateSelect,
    [Description("delete")]
    Delete
}