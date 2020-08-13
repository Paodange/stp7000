using System;
using System.Runtime.Serialization;

namespace Mgi.ALM.IOBoard
{
    public class SensorException : Exception
    {
        public SensorException()
        {
        }

        public SensorException(string message) : base(message)
        {
        }

        public SensorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SensorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}