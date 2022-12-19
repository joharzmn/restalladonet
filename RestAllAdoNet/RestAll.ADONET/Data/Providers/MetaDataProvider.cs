using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Antlr4.StringTemplate.Compiler;
using RESTAll.Data.Common;
using RESTAll.Data.Contracts;
using RESTAll.Data.Extensions;
using RESTAll.Data.Models;
using static System.Net.Mime.MediaTypeNames;
using StatementType = RESTAll.Data.Models.StatementType;
using TypeCode = System.TypeCode;
#nullable disable
namespace RESTAll.Data.Providers
{
    public class MetaDataProvider
    {

        private string _profileUrl;
        private ITemplateEngine _templateEngine;
        private RestAllConnectionStringBuilder _Builder;
        private Dictionary<string, string> _schemaXmls = new Dictionary<string, string>();
        public List<EntityDescriptor> Entities { set; get; }
        private IAuthenticationClient _AuthenticationClient;
        private readonly string _batchXml;
        public MetaDataProvider(RestAllConnectionStringBuilder builder, ITemplateEngine templateEngine, IAuthenticationClient authClient)
        {
            _Builder = builder;
            _templateEngine = templateEngine;
            Entities = new List<EntityDescriptor>();
            _AuthenticationClient = authClient;
            if (!string.IsNullOrEmpty(builder.Profile))
            {
                if (!Directory.Exists(builder.Profile))
                {
                    Directory.CreateDirectory(builder.Profile);
                }
                else
                {
                    LoadSchemas();
                }

                if (!Directory.Exists($@"{builder.Profile}\{_Builder.Schema}\Config"))
                {
                    Directory.CreateDirectory($@"{builder.Profile}\{_Builder.Schema}\Config");
                }

                _profileUrl = builder.Profile;
                if (File.Exists($@"{builder.Profile}\{_Builder.Schema}\Config\BatchRequest.xml"))
                {
                    _batchXml = File.ReadAllText($@"{builder.Profile}\{_Builder.Schema}\Config\BatchRequest.xml");
                }
            }
        }

        public DataTable GetSchemaTable(string tableName)
        {

            var schemaTable = new DataTable("SchemaTable");
            schemaTable.Locale = CultureInfo.InvariantCulture;


            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableOptionalColumn.BaseServerName, typeof(string)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.BaseTableName, typeof(string)));


            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.ColumnName, typeof(string)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.ColumnSize, typeof(int)));
            schemaTable.Columns.Add(new DataColumn("DataTypeName", typeof(string)));

            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.DataType, typeof(Type))); // THIS was not present in documentation.
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.IsAliased, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn("IsColumnSet", typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.IsExpression, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableOptionalColumn.IsHidden, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn("IsIdentity", typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.IsKey, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.IsLong, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.IsUnique, typeof(bool)));




            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.NonVersionedProviderType, typeof(int)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.NumericScale, typeof(short)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type)));
            schemaTable.Columns.Add(new DataColumn(SchemaTableColumn.ProviderType, typeof(string)));

            schemaTable.Columns.Add(new DataColumn("UdtAssemblyQualifiedName", typeof(string)));
            schemaTable.Columns.Add(new DataColumn("XmlSchemaCollectionDatabase", typeof(string)));
            schemaTable.Columns.Add(new DataColumn("XmlSchemaCollectionName", typeof(string)));
            schemaTable.Columns.Add(new DataColumn("XmlSchemaCollectionOwningSchema", typeof(string)));
            var entity = GetEntityDescriptor(tableName, new { }, new { });
            int ordinal = 0;
            foreach (var columnMetadata in entity.Table.Fields)
            {
                var row = schemaTable.Rows.Add();

                //bool isPrimaryId = attMeta.IsPrimaryId.HasValue && attMeta.IsPrimaryId.Value;
                row[SchemaTableColumn.AllowDBNull] = !columnMetadata.Key;


                row[SchemaTableOptionalColumn.BaseCatalogName] = null;


                row[SchemaTableColumn.BaseColumnName] = columnMetadata.Field;
                row[SchemaTableColumn.BaseSchemaName] = "dbo";
                //   row[SchemaTableOptionalColumn.BaseServerName] = "dbo";
                row[SchemaTableColumn.BaseTableName] = entity.Table.TableName;
                row[SchemaTableColumn.ColumnName] = columnMetadata.Field;
                row[SchemaTableColumn.ColumnOrdinal] = ordinal; // columnMetadata.AttributeMetadata.ColumnNumber;

                // set column size
                row[SchemaTableColumn.ColumnSize] = 500;
                row["DataTypeName"] = columnMetadata.DataType.GetSqlDataTypeName();
                row[SchemaTableColumn.IsAliased] = columnMetadata.HasAlias;
                row["IsColumnSet"] = false;
                row[SchemaTableColumn.IsExpression] = false;

                row[SchemaTableColumn.DataType] = columnMetadata.DataType.GetDataType();
                row[SchemaTableOptionalColumn.IsAutoIncrement] = false;
                row["IsHidden"] = false; // !attMeta.IsValidForRead;


                row["IsIdentity"] = columnMetadata.Key;

                row[SchemaTableColumn.IsKey] = columnMetadata.Key; // false; //for multi part keys // columnMetadata.AttributeMetadata.IsPrimaryId;
                row[SchemaTableColumn.IsLong] = false;
                //row[SchemaTableOptionalColumn.IsReadOnly] = !columnMetadata.AttributeMetadata.IsValidForUpdate.GetValueOrDefault() && !columnMetadata.AttributeMetadata.IsValidForCreate.GetValueOrDefault();
                row[SchemaTableOptionalColumn.IsReadOnly] = false;
                row[SchemaTableOptionalColumn.IsRowVersion] = false; //columnMetadata.AttributeMetadata.LogicalName == "versionnumber";

                row[SchemaTableColumn.IsUnique] = false;  //for timestamp columns only. //columnMetadata.AttributeMetadata.IsPrimaryId;
                //row[SchemaTableColumn.NonVersionedProviderType] = (int)attMeta.AttributeType;

                //var haveMinAndMax = attMeta as IColumnMetadata;
                //if (haveMinAndMax != null)
                //{
                //if (columnMetadata.DataType== DataTypes.Double)
                //{
                //    row[SchemaTableColumn.NumericPrecision] = 0;// DBNull.Value;
                //}
                //else
                //{
                //    row[SchemaTableColumn.NumericPrecision] = attMeta.NumericPrecision;
                //}

                //if (attMeta.NumericScale == null)
                //{
                //    row[SchemaTableColumn.NumericScale] = 0; // DBNull.Value;
                //}
                //else
                //{
                //    row[SchemaTableColumn.NumericScale] = attMeta.NumericScale;
                //}

                //row[SchemaTableOptionalColumn.ProviderSpecificDataType] = null; // attMeta.AttributeType;
                //row[SchemaTableColumn.ProviderType] = attMeta.GetSqlDataTypeName();

                row["UdtAssemblyQualifiedName"] = null;
                row["XmlSchemaCollectionDatabase"] = null;
                row["XmlSchemaCollectionName"] = null;
                row["XmlSchemaCollectionOwningSchema"] = null;

                ordinal++;
            }

            return schemaTable;
        }

        public DataTable GetTableInfo(string collectionName)
        {
            if (collectionName.Contains("."))
            {
                var splits = collectionName.Split('.');
                var schema = splits[0].Replace("[", "").Replace("]", "");
                var table = splits[1].Replace("[", "").Replace("]", "");
                return LoadInfoTable(table, schema);
            }

            return LoadInfoTable(collectionName);
        }

        public EntityDescriptor GetEntityDescriptor(string tableName, object input, object token, string schema = "")
        {
            var xmlSchemaText = _schemaXmls[$"{tableName.Replace("[", "").Replace("]", "")}".ToLower()];

            var text = _templateEngine.Parse(xmlSchemaText, _Builder, input, token);
            using TextReader tr = new StringReader(text);
            var xmlSerializer = new XmlSerializer(typeof(EntityDescriptor));
            var entity = (EntityDescriptor)xmlSerializer.Deserialize(tr);
            tr.Close();
            return entity;

        }

        public EntityDescriptor GetEntityDescriptor(QueryDescriptor queryDescriptor)
        {
            if (_schemaXmls.Count == 0)
            {
                return null;
            }
            var xmlSchemaText = "";
            dynamic input = new ExpandoObject();
            var tableName = "";
            if (!string.IsNullOrEmpty(_Builder.Schema))
            {
                if (queryDescriptor.StatementType == StatementType.Insert || queryDescriptor.StatementType==StatementType.Update)
                {
                    tableName = queryDescriptor.TargetTable.Replace("[", "").Replace("]", "");
                    xmlSchemaText = _schemaXmls[$"{tableName}".ToLower()];
                }

                if (queryDescriptor.StatementType == StatementType.Select)
                {
                    tableName = queryDescriptor.TableName.Replace("[", "").Replace("]", "");
                    xmlSchemaText = _schemaXmls[$"{tableName}".ToLower()];
                }

            }
            else
            {
                xmlSchemaText = _schemaXmls[queryDescriptor.TableName.Replace("[", "").Replace("]", "").ToLower()];
            }

            var singleEntity = Entities.FirstOrDefault(x =>
                x.Table.TableName.ToLower() == tableName.ToLower());
            if (singleEntity != null)
            {
                input.Fields = string.Join(",", singleEntity.Table.Fields.Select(x => x.Field));
            }

            var text = _templateEngine.Parse(xmlSchemaText, _Builder, input, _AuthenticationClient.Token);
            using TextReader tr = new StringReader(text);
            var xmlSerializer = new XmlSerializer(typeof(EntityDescriptor));
            var entity = (EntityDescriptor)xmlSerializer.Deserialize(tr);
            tr.Close();
            return entity;

        }



        public void GenerateEntityDescriptor(EntityDescriptor descriptor)
        {
            var xmlSerializer = new XmlSerializer(typeof(EntityDescriptor));

            var filePath = $"{_Builder.Profile}/{_Builder.Schema}/{descriptor.Table.TableName}.xml";

            var reader = new StreamWriter(filePath);
            xmlSerializer.Serialize(reader, descriptor);
            reader.Close();

        }

        public BatchRequest GetBatch(string batchId, string body, string entity, object token)
        {
            var templateText = _templateEngine.Parse(_batchXml, _Builder, new
            {
                BatchId = batchId,
                Entity = entity,
                Data = body.EscapeXml()

            }, token);
            return templateText.ToXmlEntity<BatchRequest>();
        }

        public IEnumerable<BatchRequest> GetBatchRequest()
        {
            return _batchXml.ToXmlEntity<List<BatchRequest>>();
        }

        public void GenerateBatchRequest(List<BatchRequest> request)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<BatchRequest>));
            if (!Directory.Exists($"{_Builder.Profile}/{_Builder.Schema}/Config"))
            {
                Directory.CreateDirectory($"{_Builder.Profile}/{_Builder.Schema}/Config");
            }
            var filePath = $@"{_Builder.Profile}/{_Builder.Schema}/Config/BatchRequest.xml";

            var reader = new StreamWriter(filePath);
            xmlSerializer.Serialize(reader, request);
            reader.Close();
        }

        public TypeCode GetType(string type)
        {
            switch (type)
            {
                case "boolean": return TypeCode.Boolean;
                case "string": return TypeCode.String;
                case "datetime": return TypeCode.DateTime;
                case "double": return TypeCode.Double;
                case "integer": return TypeCode.Int64;
                default: return TypeCode.String;
            }
        }

        private EntityDescriptor GetEntityByFilePath(string filePath)
        {
            using TextReader tr = new StreamReader(filePath);
            var xmlSerializer = new XmlSerializer(typeof(EntityDescriptor));
            var entity = (EntityDescriptor)xmlSerializer.Deserialize(tr);
            tr.Close();
            return entity;
        }

        private EntityDescriptor GetEntityFromXml(string xml)
        {
            using TextReader tr = new StringReader(xml);
            var xmlSerializer = new XmlSerializer(typeof(EntityDescriptor));
            var entity = (EntityDescriptor)xmlSerializer.Deserialize(tr);
            tr.Close();
            return entity;
        }



        private void LoadSchemas()
        {
            var files = Directory.GetFiles($@"{_Builder.Profile}\{_Builder.Schema}");
            foreach (var file in files.Where(x => Path.GetExtension(x) == ".xml"))
            {
                var xmlText = File.ReadAllText(file);
                Entities.Add(GetEntityFromXml(xmlText));
                _schemaXmls.Add(Path.GetFileNameWithoutExtension(file).ToLower(), xmlText);
            }
        }

        private DataTable LoadInfoTable(string tableName, string schema = "")
        {
            var dt = new DataTable("TableInfo");
            dt.Columns.Add("SchemaName", typeof(string));
            dt.Columns.Add("ColumnName", typeof(string));
            dt.Columns.Add("DataType", typeof(string));
            dt.Columns.Add("SqlDataType", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("PathMap", typeof(string));
            dt.Columns.Add("PrimaryKey", typeof(bool));
            dt.Columns.Add("MaxLength", typeof(int));
            var entityDescriptor = GetEntityDescriptor(tableName, new { }, new { }, schema);

            foreach (var tableField in entityDescriptor.Table.Fields)
            {
                var row = dt.NewRow();
                row["SchemaName"] = _Builder.Schema;
                row["ColumnName"] = tableField.Field;
                row["DataType"] = tableField.DataType.ToString();
                row["SqlDataType"] = tableField.DataType.GetSqlDataTypeName();
                row["Description"] = tableField.Description;
                row["PathMap"] = tableField.Path;
                row["PrimaryKey"] = tableField.Key;
                row["MaxLength"] = tableField.MaxLength;
                dt.Rows.Add(row);
            }
            dt.AcceptChanges();
            return dt;
        }

        public DataTable GetSchemaInfo()
        {
            var dt = new DataTable("Tables");
            dt.Columns.Add("Schema");
            dt.Columns.Add("TableName");
            dt.Columns.Add("TableType");
            dt.Columns.Add("Description");
            dt.Columns.Add("RootElement");
            dt.Columns.Add("PageSize", typeof(int));
            dt.Columns.Add("EnabledPaging", typeof(bool));
            dt.Columns.Add("PageElement");
            foreach (var entityDescriptor in Entities)
            {
                var row = dt.NewRow();
                row["Schema"] = _Builder.Schema;
                row["TableName"] = entityDescriptor.Table.TableName;
                row["TableType"] = "Table";
                row["Description"] = entityDescriptor.Table.Description;
                row["RootElement"] = entityDescriptor.RepeatElement;
                row["PageSize"] = entityDescriptor.PageSize;
                row["EnabledPaging"] = entityDescriptor.EnablePaging;
                row["PageElement"] = entityDescriptor.PageMapPath;
                dt.Rows.Add(row);
            }
            dt.AcceptChanges();
            return dt;
        }
    }
}
