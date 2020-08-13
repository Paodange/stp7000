using System;

namespace Mgi.Instrument.ALM.Attr
{
    /// <summary>
    /// 指示一个指令出错时 允许进行的操作  重试  中止  忽略等
    /// </summary>
    public class ErrorOperationAttribute : Attribute
    {
        public ErrorOperation ErrorOperation { get; }
        public ErrorOperationAttribute(ErrorOperation operation)
        {
            ErrorOperation = operation;
        }
    }

    [Flags]
    public enum ErrorOperation
    {
        Abort = 1,
        Retry = 2,
        Ignore = 4,
        All = ~0
    }
}
