using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RESTAll
{
    public class ServiceContainer
    {
        public static IServiceCollection Services { set; get; }
        public static IServiceProvider ServiceProvider { set; get; }
        static ServiceContainer()
        {
            Services = new ServiceCollection();
        }

        public static void Build()
        {
            ServiceProvider = Services.BuildServiceProvider();
        }

    }
}
