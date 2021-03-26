using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Teshca.DotNet.AspNetCore
{
    public class MyMiddlewareWithDependency
    {
        private readonly MyTransientDependency _myTransientDependency;
        
        private readonly RequestDelegate _next;

        public MyMiddlewareWithDependency(RequestDelegate next, MyTransientDependency myDependency)
        {
            _next = next;
            _myTransientDependency = myDependency;
        }

        public async Task Invoke(HttpContext httpContext, MyScopedDependency myScopedDependency)
        {
            httpContext.Items["transient_dependency_message"] = _myTransientDependency.MyProperty;
            httpContext.Items["scoped_dependency_message"] = myScopedDependency.MyProperty;

            await _next(httpContext);
        }
    }
}
