using Newtonsoft.Json.Linq;
using RESTAll.Data.Common;
using RESTAll.Data.Extensions;
using RESTAll.Data.Models;
using RESTAll.Data.Providers;

namespace RestAll.UnitTests;
#nullable disable
public class Utility
{
    /// <summary>
    /// Generate XML Schema File From Json Element
    /// </summary>
    /// <param name="jsonPath">Path to Json File</param>
    /// <param name="body">Body of the Request if any</param>
    /// <param name="rootElement">Root element in Json Response</param>
    /// <param name="tableName">Schema Table Name</param>
    /// <param name="schema">Schema Name for Table</param>
    /// <param name="url">Url to fetch data</param>
    /// <param name="description">Optional Description of Table</param>
    /// <param name="_builder"><see cref="RestAllConnectionStringBuilder"/> For parameters required in XML Templates</param>
    public static void GenerateEntity(string jsonPath,string body, string rootElement, string tableName, string url, string description, RestAllConnectionStringBuilder _builder)
    {
        var fileText = File.ReadAllText(jsonPath);
        var obj = JObject.Parse(fileText);
        var flatten = obj.Flatten();
        var metaProvider = new MetaDataProvider(_builder, null, null);
        var entityDescriptor = new EntityDescriptor();
        entityDescriptor.Actions.Add(new DataAction() { Url = url, Operation = "Select",Body = body,Method = "POST",ContentType = "application/text"});

        entityDescriptor.Table.Input.Add(new DataInput() { Column = "Id" });
        entityDescriptor.RepeatElement = rootElement;
        entityDescriptor.Table.TableName = tableName;
        entityDescriptor.Table.Description = description;
        foreach (var o in flatten)
        {
            if (o.Key.StartsWith("Line[0]"))
            {
                entityDescriptor.Table.Fields.Add(new DataField()
                {
                    DataType = Type.GetTypeCode(o.Value.GetType()),
                    Field = "Lines",
                    Path = "Line",
                    Key = false
                });
            }
            else if (o.Key.StartsWith("Line")) { }
            else
            {
                entityDescriptor.Table.Fields.Add(new DataField()
                {
                    DataType = Type.GetTypeCode(o.Value.GetType()),
                    Field = o.Key.Replace(".","_"),
                    Path = o.Key,
                    Key = o.Key.ToLower() == "id"
                });
            }
        }
        metaProvider.GenerateEntityDescriptor(entityDescriptor);
    }
}