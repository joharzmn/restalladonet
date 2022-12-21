using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RESTAll.Data.Utilities;
#nullable disable
namespace RESTAll.Data.Common
{
    public class RestAllCommand : DbCommand, IDbCommand, IDisposable, ICloneable
    {
        private DataUtility _DataUtility;
        public RestAllCommand():base()
        {
            _DataUtility = ServiceContainer.ServiceProvider.GetRequiredService<DataUtility>();
            DbParameterCollection = new RestAllParameterCollection();
        }


        public override void Cancel()
        {

        }

        
        public override int ExecuteNonQuery()
        {
            _DataUtility.ExecuteQuery(this.CommandText,this.DbParameterCollection);
            return 0;
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; }
        protected override DbTransaction DbTransaction { get; set; }
        public override bool DesignTimeVisible { get; set; }


        protected override DbParameter CreateDbParameter()
        {
            return new RestAllParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {

            var dt = _DataUtility.ExecuteQuery(this.CommandText,this.DbParameterCollection);
            return new RestAllDataReader(dt);

        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
