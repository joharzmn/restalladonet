using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace RESTAll.Data.Common
{
    public class RestAllParameter : DbParameter
    {
        public RestAllParameter()
        {
            
        }
        public RestAllParameter(string parameterName, DbType parameterType)
        {
            this.ParameterName = parameterName;
            this.DbType = parameterType;
        }

        public RestAllParameter(string parameterName,DbType dbType,string sourceColumn)
        {
            
        }

        public RestAllParameter(string parameterName,object value)
        {
            this.ParameterName = parameterName;
            this.Value = value;
        }

        public override void ResetDbType()
        {
            DbType = DbType.String;
        }

        public override DbType DbType { get; set; }

        public override ParameterDirection Direction { get; set; }
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; }
        public override string SourceColumn { get; set; }
        public override object Value { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public override int Size { get; set; }


        public bool CanAddToCommand()
        {
            if (string.IsNullOrWhiteSpace(this.ParameterName))
            {
                return false;
            }
            return true;
        }
    }
}
