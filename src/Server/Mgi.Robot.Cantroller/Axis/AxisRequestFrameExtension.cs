
using Mgi.ALM.Util.Extension;

namespace Mgi.Robot.Cantroller.Axis
{
    static class AxisRequestFrameExtension
    {
        public static byte[] ToArray(this AxisRequestFrame frame)
        {
            return frame.Data != null ?
                new byte[]
                {
                    frame.InstructionNo,
                    frame.Type,
                    frame.MotorOrBand,
                    frame.Data[0],
                    frame.Data[1],
                    frame.Data[2],
                    frame.Data[3],
                    frame.RequestId
                }
                :
                new byte[]
                {
                    frame.InstructionNo,
                    frame.Type,
                    frame.MotorOrBand,
                    0x00,
                    0x00,
                    0x00,
                    0x00,
                    frame.RequestId
                };
        }

        public static byte Size(this AxisRequestFrame frame)
        {
            return (byte)frame.ToArray().Length;
        }

        /// <summary>
        /// ToString的重写
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static string ToReadableString(this AxisRequestFrame frame)
        {
            return string.Format("RequestId:{0},TargetAddress：{1}, InstructionNo：{2}, Type：{3}, MotorOrBandk：{4}, Data：{5}",
                                    frame.RequestId.ToString("X2"), frame.TargetAddress, frame.InstructionNo, frame.Type,
                                    frame.MotorOrBand, frame.Data?.ToHexString());
        }
    }
}
