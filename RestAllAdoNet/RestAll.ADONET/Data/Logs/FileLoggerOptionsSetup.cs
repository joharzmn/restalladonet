using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace RESTAll.Data.Logs
{
    internal class FileLoggerOptionsSetup : ConfigureFromConfigurationOptions<FileLoggerOptions>
    {
        public FileLoggerOptionsSetup(IConfiguration config) : base(config)
        {
        }
    }
}
