using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Teshca.DotNet.AspNetCore.Controllers
{
    public class MyMvcController : Controller
    {
        [MyActionFilter]
        public IActionResult Index()
        {
            ViewBag.ControllerName = HttpContext.Items.TryGetValue("ControllerNameFromActionFilter", out object x) ? x : null;
            ViewBag.ActionName = HttpContext.Items.TryGetValue("ActionNameFromActionFilter", out object y) ? y : null;

            return View();
        }
    }
}