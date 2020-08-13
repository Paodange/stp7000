using System;

namespace Mgi.Gripper.Zimma
{
    /// <summary>
    /// 空抓异常
    /// </summary>
    public class EmptyGraspException : GripperException
    {
        public EmptyGraspException(string message) : base(0x10010, message)
        {

        }
    }
}
