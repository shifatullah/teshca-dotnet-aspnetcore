using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Teshca.DotNet.AspNetCore
{
    public class Program
    {
        public static StringBuilder ServiceFromMain;

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    ServiceFromMain = new StringBuilder();

                    var myScopedDependency1 = services.GetRequiredService<MyScopedDependency>();
                    ServiceFromMain.Append($"Instance 1 of MyScopedDependency.MyProperty is {myScopedDependency1.MyProperty} from Main()\n");

                    Thread.Sleep(1100);

                    var myScopedDependency2 = services.GetRequiredService<MyScopedDependency>();
                    ServiceFromMain.Append($"Instance 2 of MyScopedDependency.MyProperty is {myScopedDependency2.MyProperty} from Main()\n");

                    Thread.Sleep(1100);

                    var myTransientDependency1 = services.GetRequiredService<MyTransientDependency>();
                    ServiceFromMain.Append($"Instance 1 of MyTransientDependency.MyProperty is {myTransientDependency1.MyProperty} from Main()\n");

                    Thread.Sleep(1100);

                    var myTransientDependency2 = services.GetRequiredService<MyTransientDependency>();
                    ServiceFromMain.Append($"Instance 2 of MyTransientDependency.MyProperty is {myTransientDependency2.MyProperty} from Main()\n");

                    Thread.Sleep(1100);

                    var mySingletonDependency1 = services.GetRequiredService<MySingletonDependency>();
                    ServiceFromMain.Append($"Instance 1 of MySingletonDependency.MyProperty is {mySingletonDependency1.MyProperty} from Main()\n");

                    Thread.Sleep(1100);

                    var mySingletonDependency2 = services.GetRequiredService<MySingletonDependency>();
                    ServiceFromMain.Append($"Instance 2 of MySingletonDependency.MyProperty is {mySingletonDependency2.MyProperty} from Main()\n");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred.");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseWebRoot("mywwwroot");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
