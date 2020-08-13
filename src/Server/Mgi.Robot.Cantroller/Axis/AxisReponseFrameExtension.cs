using Mgi.ALM.Util.Extension;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mgi.Robot.Cantroller.Axis
{
    public static class AxisReponseFrameExtension
    {
        public enum EnumCommandStatus
        {
            Received = 100,
            Inplace = 128
        }


        public static AxisResponseFrame ToAxisResponseFrame(this byte[] content)
        {
            if (content.Length != AxisResponseFrame.FrameSize)
                throw new ArgumentException($"-0xFF[1308653]0xEE-content.Length != {AxisResponseFrame.FrameSize}");

            return new AxisResponseFrame()
            {
                TargetAddress = 0x00,
                FrameId = content[0],
                Status = content[1],
                InstructionNo = content[2],
                Data = new byte[] { content[3], content[4], content[5], content[6] },
                RequestId = content[7]
            };
        }


        /// <summary>
        /// 是否是本轴帧应答。部分命令有应答，不会命令没有。 这个过程中，不验证Moto 号。
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="instrumentNo"></param>
        /// <param name="motoBitMask"></param>
        /// <returns></returns>
        public static bool IsMyAckFrame(this AxisResponseFrame frame, byte frameId, byte instrumentNo)
        {
            if (frame.Status == (byte)EnumCommandStatus.Received
                    && frame.InstructionNo == instrumentNo
                    && frame.FrameId == frameId)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否是本轴帧应答。部分命令有应答，不会命令没有。
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="instrumentNo"></param>
        /// <param name="motoBitMask"></param>
        /// <returns></returns>
        public static bool IsMyAckFrame(this AxisResponseFrame frame, byte frameId, byte instrumentNo, byte motoBitMask)
        {
            if (frame.Status == (byte)EnumCommandStatus.Received
                    && frame.InstructionNo == instrumentNo && motoBitMask == frame.Data[3]
                    && frame.FrameId == frameId)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否是本轴运动到位帧。
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="instrumentNo"></param>
        /// <param name="motoBitMask"></param>
        /// <returns></returns>
        public static bool IsMyInplaceFrame(this AxisResponseFrame frame, byte frameId, byte instrumentNo, byte motoBitMask)
        {
            if (frame.Status == (byte)EnumCommandStatus.Inplace
                    && frame.InstructionNo == instrumentNo && (1 >> motoBitMask) == frame.Data[3]
                    && frame.FrameId == frameId)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否找到极限位置
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="frameId"></param>
        /// <param name="instrumentNo"></param>
        /// <returns></returns>
        public static bool IsReferenceSearchCompleted(this AxisResponseFrame frame, byte frameId, byte instrumentNo)
        {
            return frame.Status == (byte)EnumCommandStatus.Received
                    && frame.InstructionNo == instrumentNo

                    && (frame.Data.Count(b => b == 0) == 4)
                    && frame.FrameId == frameId;
        }

        /// <summary>
        /// 是否到达左极限
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="frameId"></param>
        /// <param name="instrumentNo"></param>
        /// <returns></returns>
        public static bool IsLeftNegLimit(this AxisResponseFrame frame, byte frameId, byte instrumentNo)
        {
            return frame.Status == (byte)EnumCommandStatus.Received
                    && frame.InstructionNo == instrumentNo
                    && (frame.Data[3] == 1)
                    && frame.FrameId == frameId;
        }

        /// <summary>
        /// 是否速度为零
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="frameId"></param>
        /// <param name="instrumentNo"></param>
        /// <returns></returns>
        public static bool IsZeroSpeed(this AxisResponseFrame frame, byte frameId, byte instrumentNo)
        {
            return frame.Status == (byte)EnumCommandStatus.Received
                    && frame.InstructionNo == instrumentNo
                    && (frame.Data.Count(b => b == 0) == 4)
                    && frame.FrameId == frameId;
        }

        ///// <summary>
        ///// 是否是运动到位帧。如果是，读取其电机号
        ///// </summary>
        ///// <param name="frame"></param>
        ///// <returns></returns>
        //public static Tuple<bool, byte> IsInplaceFrame(this AxisResponseFrame frame)
        //{
        //    return null;
        //}

        public static string ToReadableString(this AxisResponseFrame frame)
        {
            return string.Format("RequestId:{0},TargetAddress：{1}, InstructionNo：{2}, FrameId：{3}, Status：{4}, Data：{5}",
                                    frame.RequestId.ToString("X2"), frame.TargetAddress, frame.InstructionNo, frame.FrameId,
                                    frame.Status, frame.Data?.ToHexString());
        }
    }
}
