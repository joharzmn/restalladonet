using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RESTAll.Data.Logs;
using RESTAll.Data.Providers;

namespace RESTAll.Data.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddTraceLogHandler(this IHttpClientBuilder builder)
        {
            return builder.AddHttpMessageHandler((services) => new TraceLogHandler(services.GetRequiredService<ILogger>()));
        }
    }
}
