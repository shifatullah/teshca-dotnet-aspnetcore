using System;
namespace Teshca.DotNet.AspNetCore
{
    public class MyTransientDependency
    {
        public string MyProperty { get; set; }

        public MyTransientDependency()
        {
            MyProperty = $"MyTransientDependency instance created at {DateTime.Now}";
        }
    }
}
