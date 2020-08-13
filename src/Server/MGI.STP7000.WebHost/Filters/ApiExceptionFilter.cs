using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mgi.STP7000.Infrastructure.ApiProtocol;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MGI.STP7000.WebHost.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        readonly ILogger logger;
        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            this.logger = logger;
        }
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
            var ex = context.Exception.InnerException ?? context.Exception;
            logger.LogError("{type}:{message}——StackTrace:\r\n" + ex.StackTrace, ex.GetType().ToString(), ex.Message);
            if (ex is BusinessException e)
            {
                context.Result = new OkObjectResult(new ApiResponse(e.ResponseCode));
            }
            else
            {
                context.Result = new OkObjectResult(new ApiResponse(ResponseCode.InternalServerError));
            }
        }
    }
}
