using LearnEnglish.DataModels.Models;
using LearnEnglish.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace LearnEnglish.API.CustomFilters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomActionFilter : ActionFilterAttribute
    {
        private static LearnEnglishService _learnEnglishService;
        public CustomActionFilter()
        {
            _learnEnglishService = LearnEnglishService.GetSingletonInstance();
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            Log(actionContext);
        }

        static void Log(HttpActionContext context)
        {
            DateTime requestTime = DateTime.Now;
            string ControllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName;
            string ActionName = context.ActionDescriptor.ActionName;
            var queryString = context.Request.RequestUri.Query;
            string requestBody;
            using (var stream = new StreamReader(context.Request.Content.ReadAsStreamAsync().Result))
            {
                stream.BaseStream.Position = 0;
                requestBody = stream.ReadToEnd();
            }

            if (!(ActionName.ToLower() == "getlogs"))
            {
                LoggingData loggingData = new LoggingData(requestTime, requestBody, queryString, ControllerName, ActionName);
                var loggedData = _learnEnglishService.InsertLog(loggingData);
            }
        }
    }
}