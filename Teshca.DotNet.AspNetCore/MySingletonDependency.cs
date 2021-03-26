using System;
namespace Teshca.DotNet.AspNetCore
{
    public class MySingletonDependency
    {
        public string MyProperty { get; set; }

        public MySingletonDependency()
        {
            MyProperty = $"MySingletonDependency instance created at {DateTime.Now}";
        }
    }
}
