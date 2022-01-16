using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Teshca.DotNet.AspNetCore
{
    public class Startup
    {

        private StringBuilder _response;
        private StringBuilder _startupConstructor;
        private StringBuilder _listOfMiddleware;
        IServiceCollection _services;

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
            _response.Append("<div><a href='/registeredservices'>Services registration in Startup.ConfigureServices()</a></div>");
            _response.Append("<div><a href='/servicefrommain'>Call service from main</a></div>");

            _response.Append("<div><h3>Configuration</h3></div>");
            _response.Append("<div><a href='/configurationinfo'>Configuration Information</a></div>");

            _response.Append("<div><h3>Context</h3></div>");
            _response.Append("<div><a href='/requestinfo'>Request Information</a></div>");
            _response.Append("<div><a href='/serverinfo'>Server Information</a></div>");
            _response.Append("<div><a href='/connectioninfo'>Connection Information</a></div>");
            _response.Append("<div><a href='/authenticationinfo'>Authentication Information</a></div>");
            _response.Append("<div><a href='/featuresinfo'>Features Information</a></div>");

            _response.Append("<div><h3>Static Files</h3></div>");
            _response.Append("<div><a href='/staticfilesinfo'>Static Files Info</a></div>");
            _response.Append("<div><a href='/StaticFiles/NET_Core_Logo.png'>NET_Core_Logo.png</a></div>");
            _response.Append("<div><a href='/MyImages'>Browse MyImges directory inside web root</a></div>");

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
            services.AddDirectoryBrowser();

            _services = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration config, IServer server)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("mydefault.html");
            app.UseDefaultFiles(options);

            app.UseFileServer(enableDirectoryBrowsing: true);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                             Path.Combine(env.ContentRootPath, "MyStaticFiles")),
                RequestPath = "/StaticFiles"
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                               Path.Combine(env.WebRootPath, "images")),
                RequestPath = "/MyImages"
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/home", async context =>
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

                endpoints.MapGet("/requestinfo", async context =>
                {
                    var sb = new StringBuilder();
                    var nl = System.Environment.NewLine;
                    var rule = string.Concat(nl, new string('-', 40), nl);

                    sb.Append($"Request{rule}");
                    sb.Append($"{DateTimeOffset.Now}{nl}");
                    sb.Append($"{context.Request.Method} {context.Request.Path}{nl}");
                    sb.Append($"Scheme: {context.Request.Scheme}{nl}");
                    sb.Append($"Host: {context.Request.Headers["Host"]}{nl}");
                    sb.Append($"PathBase: {context.Request.PathBase.Value}{nl}");
                    sb.Append($"Path: {context.Request.Path.Value}{nl}");
                    sb.Append($"Query: {context.Request.QueryString.Value}{nl}{nl}");

                    sb.Append($"Headers{rule}");
                    foreach (var header in context.Request.Headers)
                    {
                        sb.Append($"{header.Key}: {header.Value}{nl}");
                    }
                    sb.Append(nl);

                    await context.Response.WriteAsync(sb.ToString());
                });

                endpoints.MapGet("/serverinfo", async context =>
                {
                    var sb = new StringBuilder();
                    var nl = System.Environment.NewLine;
                    var rule = string.Concat(nl, new string('-', 40), nl);

                    sb.Append($"Server{rule}");
                    sb.Append($"{DateTimeOffset.Now}{nl}");
                    sb.Append($"{server.GetType()}{nl}");

                    if (server.GetType().FullName == "Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerImpl")
                    {
                        KestrelWalker serverWalker = new KestrelWalker();
                        sb.Append(serverWalker.Walk(server));
                    }

                    await context.Response.WriteAsync(sb.ToString());
                });

                endpoints.MapGet("/connectioninfo", async context =>
                {
                    var sb = new StringBuilder();
                    var nl = System.Environment.NewLine;
                    var rule = string.Concat(nl, new string('-', 40), nl);

                    sb.Append($"Connection{rule}");
                    sb.Append($"RemoteIp: {context.Connection.RemoteIpAddress}{nl}");
                    sb.Append($"RemotePort: {context.Connection.RemotePort}{nl}");
                    sb.Append($"LocalIp: {context.Connection.LocalIpAddress}{nl}");
                    sb.Append($"LocalPort: {context.Connection.LocalPort}{nl}");
                    sb.Append($"ClientCert: {context.Connection.ClientCertificate}{nl}{nl}");

                    await context.Response.WriteAsync(sb.ToString());
                });

                endpoints.MapGet("/configurationinfo", async context =>
                {
                    var sb = new StringBuilder();
                    var nl = System.Environment.NewLine;
                    var rule = string.Concat(nl, new string('-', 40), nl);

                    sb.Append($"Configuration{rule}");
                    foreach (var pair in config.AsEnumerable())
                    {
                        sb.Append($"{pair.Key}: {pair.Value}{nl}");
                    }
                    sb.Append(nl);

                    sb.Append($"Environment Variables{rule}");
                    var vars = System.Environment.GetEnvironmentVariables();
                    foreach (var key in vars.Keys.Cast<string>().OrderBy(key => key,
                        StringComparer.OrdinalIgnoreCase))
                    {
                        var value = vars[key];
                        sb.Append($"{key}: {value}{nl}");
                    }

                    await context.Response.WriteAsync(sb.ToString());
                });

                endpoints.MapGet("/authenticationinfo", async context =>
                {
                    var sb = new StringBuilder();
                    var nl = System.Environment.NewLine;
                    var rule = string.Concat(nl, new string('-', 40), nl);

                    sb.Append($"Identity{rule}");
                    sb.Append($"User: {context.User.Identity.Name}{nl}");

                    // TODO: fix authentication issues
                    //var authSchemeProvider = app.ApplicationServices
                    //    .GetRequiredService<IAuthenticationSchemeProvider>();

                    //var scheme = await authSchemeProvider
                    //    .GetSchemeAsync(IISDefaults.AuthenticationScheme);

                    //sb.Append($"DisplayName: {scheme?.DisplayName}{nl}{nl}");

                    await context.Response.WriteAsync(sb.ToString());
                });

                endpoints.MapGet("/featuresinfo", async context =>
                {
                    var sb = new StringBuilder();
                    var nl = System.Environment.NewLine;
                    var rule = string.Concat(nl, new string('-', 40), nl);

                    sb.Append($"Websockets{rule}");
                    if (context.Features.Get<IHttpUpgradeFeature>() != null)
                    {
                        sb.Append($"Status: Enabled{nl}{nl}");
                    }
                    else
                    {
                        sb.Append($"Status: Disabled{nl}{nl}");
                    }

                    await context.Response.WriteAsync(sb.ToString());
                });

                endpoints.MapGet("/registeredservices", async context =>
                {
                    var sb = new StringBuilder();
                    sb.Append("<html><body>");
                    sb.Append("<h1>All Services</h1>");
                    foreach (var svc in _services)
                    {
                        sb.Append($"<h3>{svc.ServiceType.Name}</h3>");
                        sb.Append($"<div>Lifetime: {svc.Lifetime}, FullName: {svc.ServiceType.FullName}, ImplementationType?.FullName{svc.ImplementationType?.FullName}</div>");
                    }
                    sb.Append("</body></html>");
                    await context.Response.WriteAsync(sb.ToString());
                });

                endpoints.MapGet("/staticfilesinfo", async context =>
                {
                    var sb = new StringBuilder();
                    sb.Append("<html><body>");
                    sb.Append("<h1>Static Files Information</h1>");
                    sb.Append($"<div>ContentRootPath: {env.ContentRootPath}</div>");
                    sb.Append($"<div>WebRootPath: {env.WebRootPath}</div>");
                    sb.Append("</body></html>");
                    await context.Response.WriteAsync(sb.ToString());
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
                    FieldInfo middlewareField = x.Target.GetType().GetRuntimeFields().FirstOrDefault(pi => pi.Name == "middleware");
                    if (middlewareField != null)
                    {
                        object middlewareValue = middlewareField.GetValue(x.Target);
                        _listOfMiddleware.Append($"{middlewareValue.ToString()}\n");
                    }
                }
            );
        }
    }
}
