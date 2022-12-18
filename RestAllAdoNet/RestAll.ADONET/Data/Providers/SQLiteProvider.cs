using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RESTAll.Data.Common;
using RESTAll.Data.Extensions;

namespace RESTAll.Data.Providers
{

    public class SQLiteProvider
    {
        private MetaDataProvider _MetaData;
        private RestAllConnectionStringBuilder _Builder;
        private bool _isSetup = false;
        public SQLiteProvider(MetaDataProvider metaDataProvider, RestAllConnectionStringBuilder connectionStringBuilder)
        {
            _MetaData = metaDataProvider;
            _Builder = connectionStringBuilder;

            if (!_isSetup)
            {
                SetupDatabase();
            }
        }

        public void SetupDatabase()
        {
            var schemas = _MetaData.Entities.Select(x => x.Table.Schema).Distinct().ToList();
            if (!Directory.Exists(_Builder.CacheLocation))
            {
                Directory.CreateDirectory(_Builder.CacheLocation);
            }
            foreach (var schema in schemas)
            {
                using var connection = new SQLiteConnection($@"DataSource={_Builder.CacheLocation}\{schema}.db;pragma journal_mode = memory");
                connection.Open();
                using var cmd = connection.CreateCommand();
                foreach (var entity in _MetaData.Entities.Where(x => x.Table.Schema == schema))
                {
                    var entityTable = entity.GetBaseDataTable();
                    entityTable.TableName = entity.Table.TableName;
                    cmd.CommandText = entityTable.CreateTableText();
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }

            _isSetup = true;
        }

        private SQLiteConnection AttachDatabases()
        {
            var connection = new SQLiteConnection($@"Data Source={_Builder.CacheLocation}\Main.db;pragma journal_mode = memory");
            connection.Open();
            var schemas = _MetaData.Entities.Select(x => x.Table.Schema).Distinct().ToList();
            using var cmd = connection.CreateCommand();
            foreach (var schema in schemas)
            {
                cmd.CommandText = $@"Attach DATABASE '{_Builder.CacheLocation}\{schema}.db' as {schema}";
                cmd.ExecuteNonQuery();
            }

            return connection;

        }
        public void ParkData(DataTable dt)
        {


            if (dt.Columns.Count > 0)
            {
                var dbName = "";
                if (dt.TableName.Contains("."))
                {
                    var entity =
                        _MetaData.Entities.FirstOrDefault(x => $"{x.Table.Schema}.{x.Table.TableName}" == dt.TableName);
                    if (entity != null)
                    {
                        dt.TableName = entity.Table.TableName;
                        dbName = entity.Table.Schema;
                    }
                }
                else
                {
                    dbName = "Main";
                }

                using var connection = new SQLiteConnection($@"DataSource={_Builder.CacheLocation}\{dbName}.db");
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {

                    using var command = connection.CreateCommand();
                    if (dt.PrimaryKey.Length == 0)
                    {
                        command.CommandText = $"DROP TABLE IF EXISTS {dt.TableName};";
                        command.ExecuteNonQuery();
                    }

                    command.CommandText = dt.CreateTableText();
                    command.ExecuteNonQuery();
                    command.CommandText = dt.InsertStatement();

                    // Insert a lot of data
                    foreach (DataColumn dataColumn in dt.Columns)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = $"@{dataColumn.ColumnName}";
                        command.Parameters.Add(parameter);
                    }

                    foreach (DataRow dataRow in dt.Rows)
                    {
                        foreach (DataColumn dataColumn in dt.Columns)
                        {
                            command.Parameters[$"@{dataColumn.ColumnName}"].Value = dataRow[dataColumn];
                        }
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "vacuum;";
                cmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public DataTable GetData(string commandText)
        {
            var dt = new DataTable();
            using var connection = AttachDatabases();
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            try
            {
                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception e)
            {
                if (e.Message.ToLower().Contains("no such table"))
                {
                    return dt;
                }
                throw;
            }
            connection.Close();
            return dt;
        }
    }
}
