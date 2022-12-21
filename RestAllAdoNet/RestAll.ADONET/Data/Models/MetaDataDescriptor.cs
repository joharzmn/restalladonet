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
        /// <summary>
        /// Table Filed Name
        /// </summary>
        [XmlAttribute("Name")]
        public string Field { set; get; }
        /// <summary>
        /// Data Type of type <see cref="TypeCode"/>
        /// </summary>
        [XmlAttribute("DataType")]
        public TypeCode DataType { set; get; }
        /// <summary>
        /// Json Element Name to Map into DataTable
        /// </summary>
        [XmlAttribute("Path")]
        public string Path { set; get; }
        /// <summary>
        /// Currently Not Required
        /// </summary>
        [XmlAttribute("MaxLength")]
        public int MaxLength { set; get; }
        /// <summary>
        /// If Column is a primary key then true else False
        /// </summary>
        [XmlAttribute("Key")]
        public bool Key { set; get; }
        /// <summary>
        /// TODO:Currently not implemented
        /// </summary>
        [XmlAttribute("Alias")]
        public string Alias { set; get; }
        /// <summary>
        /// Reference to other table like foreign key
        /// </summary>
        [XmlAttribute("Reference")]
        public string Reference { set; get; }
        [XmlAttribute("ParameterName")]
        public string ParameterName { set; get; }
        /// <summary>
        /// Set To True if this is json Element
        /// </summary>
        [XmlAttribute("IsRaw")]
        public bool IsRaw { set; get; }
        /// <summary>
        /// Description of Column
        /// </summary>
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
        /// <summary>
        /// Table Structure Definition
        /// </summary>
        [XmlElement("TableDefination")]
        public EntityTable Table { set; get; }
        /// <summary>
        /// Operations to perform on each query type like select insert update merge etc
        /// </summary>
        [XmlArray("Operations")]
        public List<DataAction> Actions { set; get; }
        /// <summary>
        /// Page Size to fetch rows from REST Endpoint
        /// TODO: Implementation Pending
        /// </summary>
        [XmlElement("PageSize")]
        public int PageSize { set; get; }
        /// <summary>
        /// Page parameter name in URL
        /// TODO: Implementation Pending
        /// </summary>
        [XmlElement("PageParameter")]
        public string PageParameter { set; get; }
        /// <summary>
        /// Get PageNumber or PageUrl from Json Response
        /// TODO: Implementation Pending
        /// </summary>
        [XmlElement("PageMapPath")]
        public string PageMapPath { set; get; }
        /// <summary>
        /// If you want to fetch data by pages from Rest Endpoint
        /// TODO:currently not implemented it is in milestones
        /// </summary>
        [XmlAttribute("EnablePaging")]
        public bool EnablePaging { set; get; }

        [XmlAttribute("IsView")]
        public bool IsView { set; get; }
        /// <summary>
        /// Root Element of Json Response
        /// </summary>
        [XmlElement]
        public string RepeatElement { set; get; }
        /// <summary>
        /// Required for Batch Operation otherwise Optional
        /// </summary>
        [XmlElement]
        public string EntityElement { set; get; }
        /// <summary>
        /// A column to setup incremental cache
        /// </summary>
        [XmlElement]
        public string IncrementalCacheColumn { set; get; }
        [XmlElement]
        public bool IsIncrementalCache { set; get; }
        [XmlElement]
        internal DataMethod DataMethod { set; get; }
        /// <summary>
        /// If you dont want to define fields and want to fetch first level elements then you can set it to true so it will
        /// build schema automatically
        /// </summary>
        [XmlAttribute("AutoBuild")]
        public bool AutoBuild { set; get; }
        /// <summary>
        /// View Sql To Parse Json Column in SQLite
        /// </summary>
        [XmlElement("ViewSql")]
        public string ViewSql { set; get; }
        /// <summary>
        /// Get The Table Schema Definition of Table
        /// </summary>
        /// <returns></returns>
        public DataTable GetBaseDataTable()
        {
            var dt = new DataTable(Table.TableName);
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

    /// <summary>
    /// XML Definition Model
    /// </summary>
    [Serializable]
    public class EntityTable
    {
        
        public EntityTable()
        {
            Fields = new List<DataField>();
            Input = new List<DataInput>();
        }
        /// <summary>
        /// Table Name
        /// </summary>
        [XmlAttribute("TableName")]
        public string TableName { set; get; }
        /// <summary>
        /// Description of Table or endpoint if any
        /// </summary>
        [XmlAttribute("Description")]
        public string Description { set; get; }
        ///// <summary>
        ///// Schema of Table Usually it should be setup in connection in future it can be removed but
        ///// currently it is part of logic
        ///// </summary>
        //[XmlAttribute("Schema")]
        //public string Schema { set; get; }
        /// <summary>
        /// Filter columns separated by comma (,)
        /// It is definition of which fields can be direct filter to endpoint
        /// </summary>
        [XmlAttribute("Filters")]
        public string FilterColumns { set; get; }
        /// <summary>
        /// Columns List of type <see cref="List{DataField}"/>
        /// </summary>
        [XmlElement("Column")]
        public List<DataField> Fields { set; get; }
        /// <summary>
        /// Input columns are columns that can use to fetch data from endpoint optional
        /// Its purpose to work for join queries TODO: Pending Implementation
        /// </summary>
        [XmlElement("Input")]
        public List<DataInput> Input { set; get; }
    }

    [Serializable]
    public class DataInput
    {
        /// <summary>
        /// Column Name
        /// </summary>
        [XmlAttribute("Column")]
        public string Column { set; get; }
        /// <summary>
        /// Reference Table where this column is defined
        /// Will be used in Join Queries in Future
        /// </summary>
        [XmlAttribute("Reference")]
        public string Reference { set; get; }
    }

    [Serializable]
    public class DataAction
    {
        public DataAction()
        {
            RequiredColumns = new();
        }
        /// <summary>
        /// Operation can be Select,Delete,Update,Insert or Merge
        /// </summary>
        [XmlAttribute("Operation")]
        public StatementType Operation { set; get; }

        [XmlAttribute("Input")]
        public string Input { set; get; }
        /// <summary>
        /// Endpoint address to fetch or Post Data
        /// </summary>
        [XmlElement("Url")]
        public string Url { set; get; }
        
        /// <summary>
        /// Http Method like GET,PUT,POST,PATCH,DELETE
        /// </summary>
        [XmlAttribute("Method")]
        public string Method { set; get; }
        [XmlAttribute("FilterAsElement")]
        public bool FilterAsElement { set; get; }
        /// <summary>
        /// Request Body if Required
        /// </summary>
        [XmlElement("Body")]
        public string Body { set; get; }
        /// <summary>
        /// Request Content Type eg application/json , application/text, text/xml
        /// </summary>
        [XmlAttribute("ContentType")]
        public string ContentType { set; get; }
        [XmlElement("RequiredColumn")]
        public List<string> RequiredColumns { set; get; }
        [XmlElement("ResponseElement")]
        public string ResponseElement { set; get; }
    }
}
