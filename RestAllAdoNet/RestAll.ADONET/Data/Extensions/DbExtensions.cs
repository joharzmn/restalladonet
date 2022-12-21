using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RESTAll.Data.Common;

namespace RESTAll.Data.Extensions
{
    public static class DbExtensions
    {
        public static void AddParameter(this DbCommand cmd, string name, object value)
        {
            DbParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            cmd.Parameters.Add(parameter);
        }

        public static void AddBatchParameter(this DbCommand cmd, string name, string sourceColumn)
        {
            DbParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.SourceColumn = sourceColumn;
            cmd.Parameters.Add(parameter);
        }
    }
}
