using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Teshca.DotNet.AspNetCore
{
    public class Startup
    {

        private StringBuilder _response;

        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)            
        {
            _response = new StringBuilder();

            _response.Append("Tescha Hello World\n");

            _response.Append("Startup Constructor\n\n");

            _response.Append("IWebHostEnvironment Features:\n");

            _response.Append($"ApplicationName: {webHostEnvironment.ApplicationName}\n");
            _response.Append($"EnvironmentName: {webHostEnvironment.EnvironmentName}\n");
            _response.Append($"ContentRootPath: {webHostEnvironment.ContentRootPath}\n");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(_response.ToString());
                });
            });
        }
    }
}
