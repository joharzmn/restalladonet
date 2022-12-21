using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Antlr.Runtime.Misc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTAll.Data.Common;
using RESTAll.Data.Exceptions;
using RESTAll.Data.Models;
using TSQL.Tokens;

#nullable disable
namespace RESTAll.Data.Extensions
{
    internal static class RestAllExtensions
    {
        public static string GetSqlDataTypeName(this Type typeName)
        {
            switch (Type.GetTypeCode(typeName))
            {
                case TypeCode.Boolean: return "bit";
                case TypeCode.String: return "nvarchar";
                case TypeCode.Object: return "uniqueidentifier";
                case TypeCode.Double: return "double";
                case TypeCode.DateTime: return "datetime";
                case TypeCode.Int16: return "short";
                case TypeCode.Int32: return "int";
                case TypeCode.Int64: return "long";
                case TypeCode.Decimal: return "decimal";
                default: return "nvarchar";
            }
        }

        public static string GetSqlDataTypeName(this TypeCode typeName)
        {
            switch (typeName)
            {
                case TypeCode.Boolean: return "bit";
                case TypeCode.String: return "nvarchar";
                case TypeCode.Object: return "uniqueidentifier";
                case TypeCode.Double: return "double";
                case TypeCode.DateTime: return "datetime";
                case TypeCode.Int16: return "short";
                case TypeCode.Int32: return "int";
                case TypeCode.Int64: return "long";
                case TypeCode.Decimal: return "decimal";
                default: return "nvarchar";
            }
        }

        public static Type GetDataType(this TypeCode datatype)
        {
            switch (datatype)
            {
                case TypeCode.Boolean: return typeof(bool);
                case TypeCode.Double: return typeof(double);
                case TypeCode.String: return typeof(string);
                case TypeCode.Int16: return typeof(short);
                case TypeCode.Int32: return typeof(int);
                case TypeCode.Int64: return typeof(long);
                case TypeCode.Byte: return typeof(byte);
                case TypeCode.Char: return typeof(char);
                case TypeCode.Decimal: return typeof(decimal);
                case TypeCode.SByte: return typeof(SByte);
                case TypeCode.UInt16: return typeof(ushort);
                case TypeCode.UInt32: return typeof(uint);
                case TypeCode.UInt64: return typeof(ulong);
                case TypeCode.DateTime: return typeof(DateTime);
                default: return typeof(string);
            }
        }


        public static string InsertStatement(this DataTable dt)
        {
            var sb = new StringBuilder();
            sb.Append($"Insert or Replace into [{dt.TableName}](");
            var columns = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName);
            sb.Append(string.Join(",", columns.Select(x => $"[{x}]")));
            sb.Append(") Values (" + string.Join(",", columns.Select(x => $"@{x}")) + ")");
            return sb.ToString();
        }

        public static string CreateTableText(this DataTable dt)
        {
            var sb = new StringBuilder();
            var tableName = "";
            if (dt.TableName.Contains("."))
            {
                var splited = dt.TableName.Split(".");
                tableName = $"[{splited[0]}].[{splited[1]}]";
            }
            else
            {
                tableName = dt.TableName;
            }
            sb.Append($"CREATE TABLE IF NOT EXISTS [{tableName}](");
            var stringColumns = new List<string>();
            foreach (DataColumn dataColumn in dt.Columns)
            {
                if (dt.PrimaryKey.Any(x => x.ColumnName == dataColumn.ColumnName))
                {
                    stringColumns.Add($"[{dataColumn.ColumnName}] {dataColumn.DataType.SqliteDataType()} NOT NULL");
                }
                else
                {
                    stringColumns.Add($"[{dataColumn.ColumnName}] {dataColumn.DataType.SqliteDataType()} NULL");
                }

            }

            sb.Append(string.Join(",", stringColumns));
            if (dt.PrimaryKey.Length > 0)
            {
                sb.Append($", Primary Key ({string.Join(",", dt.PrimaryKey.Select(x => $"[{x.ColumnName}]"))})");
            }
            sb.Append(")");
            return sb.ToString();
        }

        public static string SqliteDataType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return "Integer";
                case TypeCode.Byte: return "Blob";
                case TypeCode.Char: return "TEXT";
                case TypeCode.Decimal: return "REAL";
                case TypeCode.DateTime: return "TEXT";
                case TypeCode.Double: return "REAL";
                case TypeCode.Int16: return "Integer";
                case TypeCode.Int32: return "Integer";
                case TypeCode.Single: return "Integer";
                case TypeCode.UInt16: return "Integer";
                case TypeCode.UInt64: return "Integer";
                case TypeCode.Int64: return "Integer";
                case TypeCode.UInt32: return "Integer";

                default: return "Text";
            }
        }

        public static DataTable GetSchemaTable(this DataTable dt)
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
            ;
            int ordinal = 0;
            foreach (DataColumn columnMetadata in dt.Columns)
            {
                var row = schemaTable.Rows.Add();
                var dataColumn = dt.PrimaryKey.FirstOrDefault(x => x.ColumnName == columnMetadata.ColumnName);
                if (dataColumn != null)
                {
                    row[SchemaTableColumn.AllowDBNull] = false;
                }
                else
                {
                    row[SchemaTableColumn.AllowDBNull] = true;
                }
                //bool isPrimaryId = attMeta.IsPrimaryId.HasValue && attMeta.IsPrimaryId.Value;



                row[SchemaTableOptionalColumn.BaseCatalogName] = null;


                row[SchemaTableColumn.BaseColumnName] = columnMetadata.ColumnName;
                row[SchemaTableColumn.BaseSchemaName] = "dbo";
                //   row[SchemaTableOptionalColumn.BaseServerName] = "dbo";
                row[SchemaTableColumn.BaseTableName] = dt.TableName;
                row[SchemaTableColumn.ColumnName] = columnMetadata.ColumnName;
                row[SchemaTableColumn.ColumnOrdinal] = columnMetadata.Ordinal; // columnMetadata.AttributeMetadata.ColumnNumber;

                // set column size
                row[SchemaTableColumn.ColumnSize] = 5000;
                row["DataTypeName"] = columnMetadata.DataType.GetSqlDataTypeName();
                row[SchemaTableColumn.IsAliased] = false;
                row["IsColumnSet"] = false;
                row[SchemaTableColumn.IsExpression] = false;

                row[SchemaTableColumn.DataType] = columnMetadata.DataType;
                row[SchemaTableOptionalColumn.IsAutoIncrement] = false;
                row["IsHidden"] = false; // !attMeta.IsValidForRead;


                if (dataColumn != null)
                {
                    row["IsIdentity"] = true;
                    row[SchemaTableColumn.IsKey] = true;
                }
                else
                {
                    row["IsIdentity"] = false;
                    row[SchemaTableColumn.IsKey] = false;
                }

                // false; //for multi part keys // columnMetadata.AttributeMetadata.IsPrimaryId;
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

        public static string EncodeBase64(this string input)
        {
            var plainTextBytes = System.Text.Encoding.ASCII.GetBytes(input);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static LogLevel ToLogLevel(this string logLevel)
        {
            return (LogLevel)Enum.Parse(typeof(LogLevel), logLevel, true);
        }

        public static GrantType ToGrantType(this string grantType)
        {
            return (GrantType)Enum.Parse(typeof(GrantType), grantType, true);
        }

        public static AuthenticationStyle ToAuthenticationType(this string authType)
        {
            return (AuthenticationStyle)Enum.Parse(typeof(AuthenticationStyle), authType, true);
        }

        public static RefreshTokenType ToRefreshTokenType(this string value)
        {
            return (RefreshTokenType)Enum.Parse(typeof(RefreshTokenType), value, true);
        }

        public static Dictionary<string, object> ToDictionary(this string value)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(value);
        }

        public static ProviderType ToProviderType(this string value)
        {
            return (ProviderType)Enum.Parse(typeof(ProviderType), value, true);
        }


        /// <summary>
        /// Splits an array into several smaller arrays.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="size">The size of the smaller arrays.</param>
        /// <returns>An array containing smaller arrays.</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }

        /// <summary>
        /// Splits List into several smaller List.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array to split.</param>
        /// <param name="size">The size of the smaller arrays.</param>
        /// <returns>An array containing smaller arrays.</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IList<T> array, int size)
        {
            for (var i = 0; i < (float)array.Count / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
        /// <summary>
        ///     A string extension method that escape XML.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>A string.</returns>
        public static string EscapeXml(this string @this)
        {
            return @this.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static void AddDictionary<T, T2>(this Dictionary<T, T2> dic, Dictionary<T, T2> elements)
        {
            foreach (var element in elements)
            {
                dic.Add(element.Key, element.Value);
            }
        }

        public static string Description(this Enum source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static string CleanStringLiteral(this string value)
        {
            return value.Remove(0, 1)
                .Remove(value.Length - 2, 1);
        }

        public static T ToXmlEntity<T>(this string input)
        {

            using TextReader tr = new StringReader(input);
            var xmlSerializer = new XmlSerializer(typeof(T));
            var entity = (T)xmlSerializer.Deserialize(tr);
            tr.Close();
            return entity;
        }

        public static void MapParameters(this Dictionary<string, object> body, DbParameterCollection parameters,List<ValuesDefinitionModel> queryParameters)
        {
            if (parameters != null)
            {
                for (int i = 0; i <= parameters.Count - 1; i++)
                {
                    var parameter = queryParameters.FirstOrDefault(x =>
                        x.ValueTypes == ValueTypes.Variable &&
                        x.Value.ToString().ToLower() == parameters[i].ParameterName.ToLower());
                    body.Add(parameter.Name.Replace("_", "."), parameters[i].Value);
                }
            }
        }

        public static void MapFilterAsElement(this Dictionary<string, object> body,List<WhereFilterModel> filters,DbParameterCollection parameters)
        {
            foreach (var filterDescriptor in filters)
            {
                if (filterDescriptor.ValueTypes != ValueTypes.Variable)
                {
                    body.Add(filterDescriptor.ColumnName.Replace("_", "."), filterDescriptor.Value);
                }

                if (filterDescriptor.ValueTypes == ValueTypes.Variable)
                {
                    if (!parameters.Cast<RestAllParameter>().Any(x=>x.ParameterName.ToLower()==filterDescriptor.Value.ToString().ToLower()))
                    {
                        throw new RESTException($"Parameter {filterDescriptor.Value} Value not Provided", HttpStatusCode.FailedDependency);
                    }
                    body.Add(filterDescriptor.ColumnName.Replace("_", "."), parameters.Cast<RestAllParameter>().FirstOrDefault(x=>x.ParameterName.ToLower()==filterDescriptor.Value.ToString().ToLower()).Value);
                }
            }
        }

        public static void MapValues(this Dictionary<string, object> body, List<ValuesDefinitionModel> queryParameters)
        {
            foreach (var parameterModel in queryParameters.Where(x => x.ValueTypes != ValueTypes.Variable))
            {
                if (parameterModel.ValueTypes == ValueTypes.String)
                {
                    var value = parameterModel.Value.ToString().CleanStringLiteral();
                    body.Add(parameterModel.Name.Replace("_", "."), value);
                }
                else
                {
                    body.Add(parameterModel.Name.Replace("_", "."), parameterModel.Value);
                }

            }
        }

        public static void ValidateRequiredColumns(this Dictionary<string, object> body, List<string> requiredColumns)
        {
            foreach (var actionRequiredColumn in requiredColumns)
            {
                if (!body.ContainsKey(actionRequiredColumn.Replace("_", ".")))
                {
                    throw new RESTException($"Required Parameter [{actionRequiredColumn}] Missing", HttpStatusCode.ExpectationFailed);
                }
            }
        }
    }
}
