using System;

namespace Mgi.ALM.Util.Extension
{
    /// <summary>
    /// 异常扩展；用来处理嵌套异常的记录
    /// </summary>
    public static class ExceptionExtension
    {
        /// <summary>
        /// 获取最原始的异常信息
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception GetSourceException(this Exception ex)
        {
            if (ex == null) return null;
            var exception = ex;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            return exception;
        }
    }
}
