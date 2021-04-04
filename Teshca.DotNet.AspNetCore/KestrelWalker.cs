using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Teshca.DotNet.AspNetCore
{
    public class KestrelWalker
    {
        StringBuilder sb;

        public KestrelWalker()
        {
            sb = new StringBuilder();
        }

        public string Walk(IServer server)
        {
            var nl = System.Environment.NewLine;
            var rule = string.Concat(nl, new string('-', 40), nl);

            sb.Append($"KestrelServerImpl.Features{rule}");
            PropertyInfo _featuresPropertyInfo = server.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Single(pi => pi.Name == "Features");

            FeatureCollection featuresCollection =
                _featuresPropertyInfo.GetValue(server) as FeatureCollection;

            featuresCollection.ToList().ForEach(x =>
                {
                    sb.Append($"Key: {x.Key}, Value: {x.Value}\n");
                    if (x.Value.GetType().GetInterfaces().Contains(typeof(IServerAddressesFeature)))
                    {
                        IServerAddressesFeature serverAddressesFeature =
                            x.Value as IServerAddressesFeature;
                        serverAddressesFeature.Addresses.ToList().ForEach(y =>
                       {
                           sb.Append($"Address: {y}\n");
                       });
                    }
                }
            );

            sb.Append($"KestrelServerImpl.Options{rule}");
            PropertyInfo _optionsPropertyInfo = server.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Single(pi => pi.Name == "Options");

            KestrelServerOptions options =
                _optionsPropertyInfo.GetValue(server) as KestrelServerOptions;

            sb.Append($"Options.AllowResponseHeaderCompression: {options.AllowResponseHeaderCompression}\n");
            sb.Append($"Options.Limits.MaxRequestBodySize: {options.Limits.MaxRequestBodySize}\n");
            sb.Append($"Options.Limits.Http2.HeaderTableSize: {options.Limits.Http2.HeaderTableSize}\n");
            sb.Append($"Options.Limits.Http3.HeaderTableSize: {options.Limits.Http3.HeaderTableSize}\n");

            return sb.ToString();
        }
    }
}
