using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Teshca.DotNet.AspNetCore
{
    public class MyStartupFilter : IStartupFilter
    {
        public MyStartupFilter()
        {
                          
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<MyMiddleware>();
                builder.UseMiddleware<MyMiddlewareWithDependency>();
                next(builder);
            };
        }
    }
}
