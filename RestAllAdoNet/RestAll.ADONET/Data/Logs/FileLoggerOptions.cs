using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable
namespace RESTAll.Data.Logs
{
    public class FileLoggerOptions
    {
        public FileLoggerOptions()
        { }


        public string LogFilePath { get; set; }

        public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } =
            Microsoft.Extensions.Logging.LogLevel.Information;

    }
}
