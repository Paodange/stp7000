using System;
using System.Runtime.Serialization;

namespace Mgi.Robot.Cantroller
{
    [Serializable]
    public class RobotException : Exception
    {

        public RobotException()
        {
        }

        public RobotException(string message) : base(message)
        {
        }

        public RobotException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RobotException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
