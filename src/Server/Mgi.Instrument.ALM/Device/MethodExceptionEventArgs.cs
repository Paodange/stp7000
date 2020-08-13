using System;
using System.Reflection;
using Mgi.Instrument.ALM.Attr;

namespace Mgi.Instrument.ALM.Device
{
    /// <summary>
    /// 
    /// </summary>
    public class MethodExceptionEventArgs
    {
        public Exception Exception { get; }
        public object[] Parameters { get; }
        public Type Type { get; }
        public MethodInfo Method { get; }
        public string DeviceName { get; set; }
        public string StepName { get; set; }
        /// <summary>
        /// 指示此异常支持的操作方式
        /// </summary>
        public ErrorOperation SupportOperations { get; }
        public ExceptionHandleResult HandleResult { get; set; }
        public MethodExceptionEventArgs(Type type, MethodInfo method, object[] args, Exception exception, ErrorOperation supportOperations)
        {
            HandleResult = ExceptionHandleResult.Abort;
            Type = type;
            Method = method;
            Parameters = args;
            Exception = exception;
            SupportOperations = supportOperations;
        }
    }

    public enum ExceptionHandleResult
    {
        Abort = 1,
        Retry,
        Ignore,
    }
}
