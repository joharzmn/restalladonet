using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RESTAll.Data.Common
{
    public class RestAllDbProviderFactory : DbProviderFactory
    {
        public static RestAllDbProviderFactory Instance = new RestAllDbProviderFactory();
        public override DbCommand? CreateCommand()
        {
            return new RestAllCommand();
        }

        public override DbParameter? CreateParameter()
        {
            return new RestAllParameter();
        }

        public override DbDataAdapter? CreateDataAdapter()
        {
            return new RestAllDataAdapter();
        }

        public override DbConnection? CreateConnection()
        {
            return new RestAllConnection();
        }

        public override DbConnectionStringBuilder? CreateConnectionStringBuilder()
        {
            return new RestAllConnectionStringBuilder();
        }
    }
}
