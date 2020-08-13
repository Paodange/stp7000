using System;

namespace Mgi.Gripper.Zimma
{

    public class GripperException : Exception
    {
        public int ErrorCode { get; }
        public GripperException(int errCode, string message) : base(message)
        {
            ErrorCode = errCode;
        }
    }
}
