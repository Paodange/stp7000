using Castle.DynamicProxy;
using Castle.DynamicProxy.Internal;
using log4net;
using Mgi.ALM.IOBoard;
using Mgi.ALM.Util.Extension;
using Mgi.Instrument.ALM.Attr;
using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Workflow;
using Mgi.Robot.Cantroller;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Mgi.Instrument.ALM.Util
{
    public class DeviceCommandInterceptor : GeneralInterceptor
    {
        IWorkflowManager _workflowManager;
        public string DeviceName { get; } = "";
        public event EventHandler<MethodExceptionEventArgs> Error;
        internal event EventHandler OnPauseComplete;
        internal event EventHandler OnResumeComplete;
        internal event EventHandler OnRetry;
        internal event EventHandler OnAbort;
        internal event EventHandler OnIgnore;
        public DeviceCommandInterceptor(string deviceName, IWorkflowManager workflowManager, ILog log) : base(log)
        {
            DeviceName = deviceName;
            _workflowManager = workflowManager;
        }

        public override void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("get_")
                            || invocation.Method.Name.StartsWith("set_")
                            || invocation.Method.Name.StartsWith("add_")
                            || invocation.Method.Name.StartsWith("remove_")
                            || invocation.Method.GetCustomAttribute<NonInterceptorAttribute>(true) != null)
            {
                //不代理属性 和事件
                invocation.Proceed();
                return;
            }
            if (invocation.TargetType.GetCustomAttribute<IgnoreWorkflowStatusAttribute>(true) == null
                && !(invocation.InvocationTarget is IOnboardHardware))
            {
                CheckWorkflowStatus();
            }
            var traceId = Guid.NewGuid().ToString("N");
            log.Debug($"{invocation.TargetType.FullName}.{invocation.Method.Name} begin, Parameters:{invocation.Arguments.ToJsonString()}, TraceId:{traceId}");
            var stopwatch = new Stopwatch();
            int times = 0;
            stopwatch.Start();
            //if (invocation.InvocationTarget is IALMDevice)
            //{
            while (true)
            {
                try
                {
                    invocation.Proceed();
                    stopwatch.Stop();
                    log.Debug($"{invocation.TargetType.FullName}.{invocation.Method.Name} successfully complete, Parameters:{invocation.Arguments.ToJsonString()}, Result:{invocation.ReturnValue?.ToJsonString()}, Duration:{stopwatch.ElapsedMilliseconds}ms, TraceId:{traceId}");
                    break;
                }
                catch (Exception ex)
                {
                    log.Error($"Error {invocation.TargetType.FullName}.{invocation.Method.Name}, Parameters:{invocation.Arguments.ToJsonString()} TraceId:{traceId}", ex);
                    if (ex is UserAbortException || ex is AspirateAccuracyCheckFailException || !IsCallingFromWorkflow())
                    {
                        stopwatch.Stop();
                        throw;
                    }
                    var supportOperations = ErrorOperation.All;
                    var errorOperationAttr = invocation.Method.GetCustomAttribute<ErrorOperationAttribute>(true);
                    if (errorOperationAttr != null)
                    {
                        supportOperations = errorOperationAttr.ErrorOperation;
                    }
                    var err = new MethodExceptionEventArgs(invocation.InvocationTarget.GetType(),
                         invocation.Method, invocation.Arguments, ex, supportOperations)
                    {
                        DeviceName = DeviceName
                    };
                    OnMethodException(invocation.InvocationTarget, err);
                    if (err.HandleResult == ExceptionHandleResult.Abort)
                    {
                        stopwatch.Stop();
                        log.Error($"Abort,{invocation.TargetType.FullName}.{invocation.Method.Name}, Parameters:{invocation.Arguments.ToJsonString()},Exception:{ex}, TraceId:{traceId}");
                        OnAbort?.Invoke(this, new EventArgs());
                        _workflowManager.SetStatus(WorkflowStatus.Stopped);
                        throw new UserAbortException(ex.Message, ex);
                    }
                    else if (err.HandleResult == ExceptionHandleResult.Ignore)
                    {
                        stopwatch.Stop();
                        log.Warn($"Ignore,{invocation.TargetType.FullName}.{invocation.Method.Name}, Parameters:{invocation.Arguments.ToJsonString()},Exception:{ex}, TraceId:{traceId}");
                        OnIgnore?.Invoke(this, new EventArgs());
                        SetValueTypeReturnValue(invocation);   // 如果函数有返回值是值类型 并且不能为空，则需要设置其返回值为默认值
                        break;
                    }
                    else if (err.HandleResult == ExceptionHandleResult.Retry)
                    {
                        times++;
                        log.Warn($"Retry times:{times},{invocation.TargetType.FullName}.{invocation.Method.Name}, Parameters:{invocation.Arguments.ToJsonString()},Exception:{ex}, TraceId:{traceId}");
                        OnRetry?.Invoke(this, new EventArgs());
                        if (ex is AccuracyCheckFailException ace)
                        {
                            // 丢步异常  需要在重试前修复编码器
                            OnRetryFixEncoder(ace.Axis);
                        }
                        continue;
                    }
                }
            }
            //}
            //else
            //{
            //    try
            //    {
            //        invocation.Proceed();
            //        stopwatch.Stop();
            //        log.Debug($"{invocation.TargetType.FullName}.{invocation.Method.Name} successfully complete, Parameters:{invocation.Arguments.ToJsonString()}, Result:{invocation.ReturnValue?.ToJsonString()}, Duration:{stopwatch.ElapsedMilliseconds}ms, TraceId:{traceId}");
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error($"Error {invocation.TargetType.FullName}.{invocation.Method.Name}, Parameters:{invocation.Arguments.ToJsonString()} TraceId:{traceId}", ex);
            //        throw;
            //    }
            //}
        }


        protected virtual void OnMethodException(object sender, MethodExceptionEventArgs e)
        {
            Error?.Invoke(sender, e);
        }


        private void CheckWorkflowStatus()
        {
            if (IsCallingFromWorkflow())
            {
                // 暂停逻辑
                if (_workflowManager.Status == WorkflowStatus.Paused)
                {
                    OnPauseComplete?.Invoke(this, new EventArgs());
                    while (_workflowManager.Status == WorkflowStatus.Paused)
                    {
                        Thread.Sleep(100);
                    }
                    if (_workflowManager.Status == WorkflowStatus.Running)
                    {
                        OnResumeComplete?.Invoke(this, new EventArgs());
                    }
                }
                if (_workflowManager.Status == WorkflowStatus.Stopped)
                {
                    throw new UserAbortException("User abort the workflow", null);
                }
            }
        }

        /// <summary>
        /// 判断调用是否来自流程运行
        /// </summary>
        private bool IsCallingFromWorkflow()
        {
            StackFrame f;
            int skipFrames = 1;
            while ((f = new StackFrame(skipFrames)).GetNativeOffset() != StackFrame.OFFSET_UNKNOWN)
            {
                int i = 0;
                var method = f.GetMethod();
                var type = method.DeclaringType;
                while (type != null && (i++) < 10)
                {
                    if (typeof(IALMWorkflow).IsAssignableFrom(type))
                    {
                        return true;
                    }
                    type = type.DeclaringType;
                }
                skipFrames++;
            }
            return false;
        }

        private void SetValueTypeReturnValue(IInvocation invocation)
        {
            if (invocation.Method.ReturnType != typeof(void)
                && invocation.Method.ReturnType.IsValueType
                && !invocation.Method.ReturnType.IsNullableType())
            {
                var returnValue = DefaultGenerator.GetDefaultValue(invocation.Method.ReturnType);
                invocation.ReturnValue = returnValue;
            }
        }

        private void OnRetryFixEncoder(ISp200Axis axis)
        {
            //axis.GoHome();
            axis.FixedPosRegister();
        }
    }

    public class UserAbortException : Exception
    {
        public UserAbortException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }

    public class DefaultGenerator
    {
        public static object GetDefaultValue(Type parameter)
        {
            var defaultGeneratorType =
              typeof(DefaultGenerator<>).MakeGenericType(parameter);

            return defaultGeneratorType.InvokeMember(
              "GetDefault",
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.InvokeMethod,
              null, null, new object[0]);
        }
    }

    public class DefaultGenerator<T>
    {
        public static T GetDefault()
        {
            return default(T);
        }
    }
}
