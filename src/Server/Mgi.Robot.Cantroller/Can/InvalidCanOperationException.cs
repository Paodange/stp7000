using System;
using System.Runtime.Serialization;

namespace Mgi.Robot.Cantroller.Can
{
    /// <summary>
    /// Can 操作无效操作
    /// </summary>
    [Serializable]
    public class InvalidCanOperationException : Exception
    {
        public InvalidCanOperationException() : base() { }

        public InvalidCanOperationException(string message) : base(message) { }

        public InvalidCanOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidCanOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
