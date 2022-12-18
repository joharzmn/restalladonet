using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RESTAll.Data.Common;
using RESTAll.Data.Extensions;
#nullable disable
namespace RESTAll.Data.Models
{
    [Serializable]
    public class DataField
    {

        [XmlAttribute("Name")]
        public string Field { set; get; }
        [XmlAttribute("DataType")]
        public TypeCode DataType { set; get; }
        [XmlAttribute("Path")]
        public string Path { set; get; }
        [XmlAttribute("MaxLength")]
        public int MaxLength { set; get; }
        [XmlAttribute("Key")]
        public bool Key { set; get; }
        [XmlAttribute("Alias")]
        public string Alias { set; get; }
        [XmlAttribute("Reference")]
        public string Reference { set; get; }
        [XmlAttribute("ParameterName")]
        public string ParameterName { set; get; }

        [XmlAttribute("IsRaw")]
        public bool IsRaw { set; get; }

        [XmlAttribute("Description")]
        public string Description { set; get; }

        [XmlIgnore]
        public bool HasAlias => !string.IsNullOrEmpty(Alias);
    }


    [Serializable]
    public class EntityDescriptor
    {
        public EntityDescriptor()
        {
            Actions = new List<DataAction>();
            Table = new EntityTable();
        }

        [XmlElement("TableDefination")]
        public EntityTable Table { set; get; }
        [XmlArray("Operations")]
        public List<DataAction> Actions { set; get; }
        [XmlElement("PageSize")]
        public int PageSize { set; get; }
        [XmlElement("PageParameter")]
        public string PageParameter { set; get; }
        [XmlElement("PageMapPath")]
        public string PageMapPath { set; get; }
        [XmlAttribute("EnablePaging")]
        public bool EnablePaging { set; get; }
        [XmlElement]
        public string RepeatElement { set; get; }
        [XmlElement]
        public string EntityElement { set; get; }
        [XmlAttribute("AutoBuild")]
        public bool AutoBuild { set; get; }

        [XmlElement("ViewSql")]
        public string ViewSql { set; get; }

        public DataTable GetBaseDataTable()
        {
            var tableName = "";
            if (!string.IsNullOrEmpty(Table.Schema))
            {
                tableName = $"{Table.Schema}.{Table.TableName}";
            }
            else
            {
                tableName = Table.TableName;
            }
            var dt = new DataTable(tableName);

            var dataColumns = new List<string>();
            foreach (var dataField in Table.Fields)
            {
                if (dataField.Key)
                {
                    dataColumns.Add(dataField.Field);
                }
                dt.Columns.Add(dataField.Field, dataField.DataType.GetDataType());
            }

            dt.PrimaryKey = dt.Columns.Cast<DataColumn>().Where(x=>dataColumns.Contains(x.ColumnName)).ToArray();
            

            return dt;
        }
    }

    [Serializable]
    public class EntityTable
    {
        public EntityTable()
        {
            Fields = new List<DataField>();
            Input = new List<DataInput>();
        }
        [XmlAttribute("TableName")]
        public string TableName { set; get; }
        [XmlAttribute("Description")]
        public string Description { set; get; }
        [XmlAttribute("Schema")]
        public string Schema { set; get; }
        [XmlAttribute("Filters")]
        public string FilterColumns { set; get; }
        [XmlElement("Column")]
        public List<DataField> Fields { set; get; }
        [XmlElement("Input")]
        public List<DataInput> Input { set; get; }
    }

    [Serializable]
    public class DataInput
    {
        [XmlAttribute("Column")]
        public string Column { set; get; }
        [XmlAttribute("Reference")]
        public string Reference { set; get; }
    }

    [Serializable]
    public class DataAction
    {

        [XmlAttribute("Operation")]
        public string Operation { set; get; }

        [XmlAttribute("Input")]
        public string? Input { set; get; }
        [XmlElement("Url")]
        public string? Url { set; get; }
        [XmlAttribute("Method")]
        public string Method { set; get; }
        [XmlElement("Body")]
        public string Body { set; get; }
        [XmlAttribute("ContentType")]
        public string ContentType { set; get; }
    }
}
