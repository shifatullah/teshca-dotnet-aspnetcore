using System;
namespace Teshca.DotNet.AspNetCore
{
    public class MyScopedDependency
    {
        public string MyProperty { get; set; }

        public MyScopedDependency()
        {
            MyProperty = $"MyScopedDependency instance created at {DateTime.Now}";
        }
    }
}
