using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Teshca.DotNet.AspNetCore.Models.Api;

namespace Teshca.DotNet.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyApiController : ControllerBase
    {
        [MyActionFilter]
        public MyApiModel MyData()
        {
            object controller = HttpContext.Items.TryGetValue("ControllerNameFromActionFilter", out object x) ? x : null;
            object action = HttpContext.Items.TryGetValue("ActionNameFromActionFilter", out object y) ? y : null;

            MyApiModel myApiModel = new MyApiModel()
            {
                MyStringProperty1 = $"Api controller name is: {controller}",
                MyStringProperty2 = $"Api action name is: {action}",
                MyIntProperty1 = 1,
                MyIntProperty2 = 2,
                MyDateTimeProperty1 = DateTime.Now
            };

            return myApiModel;
        }        
    }
}