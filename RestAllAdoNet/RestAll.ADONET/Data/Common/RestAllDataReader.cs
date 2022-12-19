using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RESTAll.Data.Extensions;
using Convert = System.Convert;
#nullable disable
namespace RESTAll.Data.Common
{
    public class RestAllDataReader : DbDataReader, IDataReader
    {
        private DataTable _Data;
        private DbConnection _DbConnection;
        public RestAllDataReader(DataTable dt)
        {
            this.HasRows = dt.Rows.Count > 0;
            _Data = dt;
            this.FieldCount = dt.Columns.Count;
            _DbConnection = ServiceContainer.ServiceProvider.GetRequiredService<RestAllConnection>();

        }

        private const int StartPosition = -1;
        private int _Position = StartPosition;
        public override bool GetBoolean(int ordinal)
        {
            return Convert.ToBoolean(_Data.Rows[_Position][ordinal]);
        }

        public override byte GetByte(int ordinal)
        {
            return Convert.ToByte(_Data.Rows[_Position][ordinal]);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            return Convert.ToChar(_Data.Rows[_Position][ordinal]);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            return _Data.Columns[ordinal].DataType.Name;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return Convert.ToDateTime(_Data.Rows[_Position][ordinal]);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return Convert.ToDecimal(_Data.Rows[_Position][ordinal]);
        }

        public override double GetDouble(int ordinal)
        {
            return Convert.ToDouble(_Data.Rows[_Position][ordinal]);
        }

        public override Type GetFieldType(int ordinal)
        {
            return _Data.Columns[ordinal].DataType;
        }

        public override float GetFloat(int ordinal)
        {
            return float.Parse(_Data.Rows[_Position][ordinal].ToString());
        }

        public override Guid GetGuid(int ordinal)
        {
            return Guid.Parse(_Data.Rows[_Position][ordinal].ToString());
        }

        public override short GetInt16(int ordinal)
        {
            return Convert.ToInt16(_Data.Rows[_Position][ordinal]);
        }

        public override int GetInt32(int ordinal)
        {
            return Convert.ToInt32(_Data.Rows[_Position][ordinal]);
        }

        public override long GetInt64(int ordinal)
        {
            return Convert.ToInt64(_Data.Rows[_Position][ordinal]);
        }

        public override string GetName(int ordinal)
        {
            return _Data.Columns[ordinal].ColumnName;
        }

        public override int GetOrdinal(string name)
        {
            return _Data.Columns[name].Ordinal;
        }

        public override string GetString(int ordinal)
        {
            return _Data.Rows[_Position][ordinal].ToString();
        }

        public override object GetValue(int ordinal)
        {
            var value = _Data.Rows[_Position][ordinal];
            return value;
        }

        public override int GetValues(object[] values)
        {
            int i;
            for (i = 0; i < values.Length; i++)
            {
                values[i] = GetValue(i);
            }
            return i;
        }



        public override bool IsDBNull(int ordinal)
        {
            return _Data.Rows[_Position][ordinal] == DBNull.Value;
        }

        public override int FieldCount { get; }

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => this[GetOrdinal(name)];

        public override int RecordsAffected { get; }
        public override bool HasRows { get; }
        public override bool IsClosed { get; }

        public override bool NextResult()
        {
            //throw new NotImplementedException();
            if (_Data.Rows.Count >= _Position)
            {
                if (_Data.Rows.Count == 0)
                {
                    return false;
                }
                _Position = 0;
                return true;
            }
            return false;
        }

        public override DataTable GetSchemaTable()
        {
            var schema = _Data.GetSchemaTable();

            return schema;
        }

        public override bool Read()
        {

            if (++_Position >= _Data.Rows.Count)
            {
                // if the result set supports paging and has more records, load the next page.              
                //if (!CurrentResultSet.HasMoreRecords())
                //{
                //    return false;
                //}
                //else
                //{
                //    CurrentResultSet.LoadNextPage();
                //    _Position = StartPosition;
                //    return true;
                //    //TODO: Look into loading this asynchornously and ahead of time - perhaps 100 rows in advance?
                //    // Could grab the next page of results..                      
                //}
                return false;
            }
            else
            {

                return true;
            }
        }

        public override int Depth { get; }

        public override void Close()
        {
            this._DbConnection.Close();
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, true);
        }
    }
}
