using System;
using System.Runtime.Serialization;

namespace Mgi.Robot.Cantroller.Axis
{
    [Serializable]
    public class AxisOperateException : Exception
    {
        public AxisOperateException()
        {
        }

        public AxisOperateException(string message) : base(message)
        {
        }

        public AxisOperateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AxisOperateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
