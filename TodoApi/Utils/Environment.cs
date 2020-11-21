using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Utils
{
    public class Environment
    {
        public static IConfiguration GetConfiguration(IHostEnvironment env)
        {
#if DEBUG
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
#else
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsetting/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsetting/appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
#endif
            return builder.Build();
        }
    }
}
