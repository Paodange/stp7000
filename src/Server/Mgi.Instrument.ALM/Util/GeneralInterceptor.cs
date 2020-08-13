using Castle.DynamicProxy;
using log4net;
using Mgi.ALM.Util.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mgi.Instrument.ALM.Util
{
    public class GeneralInterceptor : IInterceptor
    {
        public static List<GeneralInterceptor> AllFilters { get; } = new List<GeneralInterceptor>();
        protected readonly ILog log;
        public GeneralInterceptor(ILog _log)
        {
            log = _log;
            AllFilters.Add(this);
        }
        public virtual void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("get_")
                || invocation.Method.Name.StartsWith("set_")
                || invocation.Method.Name.StartsWith("add_")
                || invocation.Method.Name.StartsWith("remove_"))
            {
                //不代理属性 和事件
                invocation.Proceed();
                return;
            }
            var traceId = Guid.NewGuid().ToString("N");
            log.Debug($"{invocation.TargetType.FullName}.{invocation.Method.Name} begin, Parameters:{invocation.Arguments.ToJsonString()}, TraceId:{traceId}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                invocation.Proceed();
                stopwatch.Stop();
                log.Debug($"{invocation.TargetType.FullName}.{invocation.Method.Name} successfully complete, Parameters:{invocation.Arguments.ToJsonString()}, Result:{invocation.ReturnValue.ToJsonString()}, Duration:{stopwatch.ElapsedMilliseconds}ms, TraceId:{traceId}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                log.Error($"{invocation.TargetType.FullName}.{invocation.Method.Name} failed, Parameters:{invocation.Arguments.ToJsonString()},Exception:{ex}, TraceId:{traceId}");
                throw;
            }
        }
    }
}
