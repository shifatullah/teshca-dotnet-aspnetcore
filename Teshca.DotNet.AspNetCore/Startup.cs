using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Teshca.DotNet.AspNetCore
{
    public class Startup
    {

        private StringBuilder _response;
        private StringBuilder _startupConstructor;
        private StringBuilder _listOfMiddleware;

        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _response = new StringBuilder();

            _response.Append("<html><body>");

            _response.Append("<h1>Tescha Asp.Net Core Fundamentals</h1>");

            _response.Append("<div><h3>Startup</h3></div>");
            _response.Append("<div><a href='/startupconstructor'>Information available in Startup class constructor</a></div>");
            _response.Append("<div><a href='/mycustomerror'>Let's throw custom exception to see DeveloperExceptionPage?</a></div>");
            _response.Append("<div><a href='/mystartupfilter?option=Dummy'>IStartupFilter (middleware with/without dependencies)</a></div>");
            _response.Append("<div><a href='/listofmiddleware?option=Dummy'>List of Middleware registered in request pipeline</a></div>");

            _response.Append("<div><h3>Dependency Injection</h3></div>");
            _response.Append("<div><a href='/configureservices'>Services registration in Startup.ConfigureServices()</a></div>");
            _response.Append("<div><a href='/servicefrommain'>Call service from main</a></div>");

            _response.Append("</body></html>");

            CollectInfoInsideStartupConstructor(webHostEnvironment);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IStartupFilter, MyStartupFilter>();
            
            services.AddScoped<MyScopedDependency>();
            services.AddTransient<MyTransientDependency>();
            services.AddSingleton<MySingletonDependency>();
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

                endpoints.MapGet("/startupconstructor", async context =>
                {
                    await context.Response.WriteAsync(_startupConstructor.ToString());
                });

                endpoints.MapGet("/mycustomerror", async context =>
                {
                    throw new ApplicationException("my custom error");
                });

                endpoints.MapGet("/mystartupfilter", async context =>
                {
                    object value;
                    if (context.Items.TryGetValue("option", out value))
                        await context.Response.WriteAsync($"{value} value is passed as option parameter in query string\n");
                    else
                        await context.Response.WriteAsync($"No value is passed as option parameter in query string\n");

                    if (context.Items.TryGetValue("transient_dependency_message", out value))
                        await context.Response.WriteAsync($"{value} is message from MyTransientDependency instance injected into MyMiddlewareWithDependency\n");
                    else
                        await context.Response.WriteAsync($"No value retrieved from MyTransientDependency instance\n");

                    if (context.Items.TryGetValue("scoped_dependency_message", out value))
                        await context.Response.WriteAsync($"{value} is message from MyScopedDependency instance injected into MyMiddlewareWithDependency\n");
                    else
                        await context.Response.WriteAsync($"No value retrieved from MyScopedDependency instance\n");
                });

                endpoints.MapGet("/listofmiddleware", async context =>
                {
                    await context.Response.WriteAsync(_listOfMiddleware.ToString());
                });

                endpoints.MapGet("/servicefrommain", async context =>
                {
                    await context.Response.WriteAsync(Program.ServiceFromMain.ToString());
                });
            });

            CollectListOfMiddleware(app);
        }

        private void CollectInfoInsideStartupConstructor(IWebHostEnvironment webHostEnvironment)
        {
            _startupConstructor = new StringBuilder();
            _startupConstructor.Append("<html><body>");
            _startupConstructor.Append("<div><h1>Startup Constructor</h1></div>");
            _startupConstructor.Append("<div><h3>IWebHostEnvironment Features:</h3></div>");
            _startupConstructor.Append($"<div>ApplicationName: {webHostEnvironment.ApplicationName}</div>");
            _startupConstructor.Append($"<div>EnvironmentName: {webHostEnvironment.EnvironmentName}</div>");
            _startupConstructor.Append($"<div>ContentRootPath: {webHostEnvironment.ContentRootPath}</div>");
            _startupConstructor.Append($"<div>IsDevelopment(): {webHostEnvironment.IsDevelopment()}</div>");
            _startupConstructor.Append("</body></html>");
        }

        private void CollectListOfMiddleware(IApplicationBuilder app)
        {
            _listOfMiddleware = new StringBuilder();
            FieldInfo _componentsField = app.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Single(pi => pi.Name == "_components");
            List<Func<RequestDelegate, RequestDelegate>> _componentsValue =
                _componentsField.GetValue(app) as List<Func<RequestDelegate, RequestDelegate>>;
            _componentsValue.ForEach(x =>
            {
                FieldInfo middlewareField = x.Target.GetType().GetRuntimeFields().Single(pi => pi.Name == "middleware");
                object middlewareValue = middlewareField.GetValue(x.Target);
                _listOfMiddleware.Append($"{middlewareValue.ToString()}\n");
            }
            );
        }
    }
}
