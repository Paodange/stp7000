using System.Diagnostics;
using System.Threading;
using Mgi.ALM.Util.Extension;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MGI.STP7000.WebHost.Filters
{
    public class ApiInvokeLogFilter : ActionFilterAttribute
    {
        private readonly ThreadLocal<Stopwatch> threadLocalSw = new ThreadLocal<Stopwatch>(true);
        readonly ILogger log;
        public ApiInvokeLogFilter(ILogger<ApiInvokeLogFilter> logger)
        {
            log = logger;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            threadLocalSw.Value = new Stopwatch();
            threadLocalSw.Value.Start();
            log.LogInformation("Request begin, TraceId:{traceId},Url:{url},Method:{method},Args:{@args}"
                , context.HttpContext.TraceIdentifier, context.HttpContext.Request.Path, context.HttpContext.Request.Method, context.ActionArguments);
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            threadLocalSw.Values[0].Stop();
            if (context.Exception == null)
            {
                log.LogInformation("Request end, TraceId:{traceId},Duration:{duration}ms ,Url:{url},Method:{method},Result:{@result}",
                    context.HttpContext.TraceIdentifier, threadLocalSw.Values[0].ElapsedMilliseconds, context.HttpContext.Request.Path, context.HttpContext.Request.Method, context.Result.ToJson());
            }
            else
            {
                log.LogInformation("Request end, TraceId:{traceId},Duration:{duration}ms ,Url:{url},Method:{method} Exception",
                     context.HttpContext.TraceIdentifier, threadLocalSw.Values[0].ElapsedMilliseconds, context.HttpContext.Request.Path, context.HttpContext.Request.Method);
            }
        }
    }
}
