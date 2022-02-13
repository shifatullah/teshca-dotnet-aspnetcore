using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.ComponentModel;

namespace Teshca.DotNet.AspNetCore
{
    public class MyActionFilterAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {            
            string controllerName = string.Empty;
            string actionName = string.Empty;


            if (context.ActionDescriptor is ControllerActionDescriptor)
            {
                controllerName = ((ControllerActionDescriptor)context.ActionDescriptor).ControllerName;
                actionName = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;
            }            
            context.HttpContext.Items.Add("ControllerNameFromActionFilter", controllerName);
            context.HttpContext.Items.Add("ActionNameFromActionFilter", actionName);
        }
    }
}
