using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Card = Mgi.Robot.Cantroller.Can.CanController;

namespace Mgi.Robot.Cantroller.Can
{
    /// <summary>
    /// 将CAN DELL的简单逻辑封装
    /// </summary>
    public static class CanInterfaceExtension
    {
        /// <summary>
        /// 清除缓冲区
        /// </summary>
        /// <param name="can"></param>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        public static void ClearBuffer(this ICanController can)
        {
            can.CheckOpearateSuccess(
                Card.ClearBuffer(can.Parameter.DeviceType,
                                 can.Parameter.DeviceIndex,
                                 can.Parameter.CanIndex
                                 )
                    );
        }

        public static void CloseDevice(this ICanController can)
        {
            can.CheckOpearateSuccess(
                Card.CloseDevice(can.Parameter.DeviceType,
                                 can.Parameter.DeviceIndex
                                )
                             );
        }

        public static uint GetReadBufferLength(this ICanController can)
        {
            return Card.GetReceiveNum(can.Parameter.DeviceType,
                             can.Parameter.DeviceIndex,
                             can.Parameter.CanIndex
                             );
        }

        public static void GetReference(this ICanController can, uint deviceType, uint deviceIndex, uint canIndex, uint refType, out IntPtr pData)
        {
            can.CheckOpearateSuccess(
                    Card.GetReference(can.Parameter.DeviceType,
                                 can.Parameter.DeviceIndex,
                                 can.Parameter.CanIndex,
                                 refType,
                                 out pData
                                 )
                );
        }


        public static void InitCan(this ICanController can, ref VciInitConfig pInitConfig)
        {
            can.CheckOpearateSuccess(
                Card.InitCAN(can.Parameter.DeviceType,
                                 can.Parameter.DeviceIndex,
                                 can.Parameter.CanIndex,
                                 ref pInitConfig
                                 )
                             );
        }

        /// <summary>
        /// 打开CAN口
        /// </summary>
        /// <param name="can"></param>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="reserved"></param>
        /// <exception cref="InvalidCanOperationException">如果打开失败</exception>
        public static void OpenCan(this ICanController can)
        {
            can.CheckOpearateSuccess(
                Card.OpenDevice(can.Parameter.DeviceType,
                                 can.Parameter.DeviceIndex,
                                 can.Parameter.Reserved
                                )
                             );
        }

        public static VciBoardInfo ReadBoardInfo(this ICanController can)
        {
            var info = new VciBoardInfo();
            can.CheckOpearateSuccess(
                Card.ReadBoardInfo(can.Parameter.DeviceType,
                                 can.Parameter.DeviceIndex,
                                 ref info
                                 )
                            );
            return info;
        }

        public static VciCanStatus ReadCANStatus(this ICanController can)
        {
            var status = new VciCanStatus();
            can.CheckOpearateSuccess(
                Card.ReadCANStatus(can.Parameter.DeviceType,
                                 can.Parameter.DeviceIndex,
                                 can.Parameter.CanIndex, ref status
                                 )
                            );
            return status;
        }

        /// <summary>
        /// 返回CAN当前的错误描述
        /// </summary>
        /// <param name="can"></param>
        /// <returns></returns>
        public static string ReadErrInfo(this ICanController can)
        {
            var err = new VciError();
            Card.ReadErrInfo(can.Parameter.DeviceType,
                             can.Parameter.DeviceIndex,
                             can.Parameter.CanIndex, ref err
                             );
            return err.ToUserFriendly();
        }

        /// <summary>
        /// 在CAN的
        /// </summary>
        /// <param name="can"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IEnumerable<VciCanFrame> Read(this ICanController can, uint length)
        {
            if (length <= 0)
                throw new InvalidCanOperationException("-0xFF[1326662]0xEE-Length can't less or equal zero ");

            var frames = new List<VciCanFrame>((int)length);
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf<VciCanFrame>() * (int)length);

            can.CheckIoSuccess(
                Card.Receive(can.Parameter.DeviceType,
                                can.Parameter.DeviceIndex,
                                can.Parameter.CanIndex,
                                pt,
                                length, //(length * Marshal.SizeOf<VciCanFrame>()),
                                can.Parameter.IoTimeout
                                )
                        );
            for (var i = 0; i < length; i++)
            {
                var obj = (VciCanFrame)Marshal.PtrToStructure(pt + i * Marshal.SizeOf<VciCanFrame>(), typeof(VciCanFrame));
                frames.Add(obj);
            }
            Marshal.FreeHGlobal(pt);
            return frames;
        }

        /// <summary>
        /// 读取当前缓冲区内部所有的帧。
        /// </summary>
        /// <param name="can"></param>
        /// <returns>NULL OR </returns>
        /// <exception cref="InvalidCanOperationException">如果CAN卡操作失败</exception>
        public static IEnumerable<VciCanFrame> ReadAll(this ICanController can)
        {
            var counts = can.GetReadBufferLength();
            if (counts == 0)
                return null;
            //else



            return can.Read(counts);
        }

        public static void ResetCan(this ICanController can)
        {
            can.CheckOpearateSuccess(
                Card.ResetCAN(can.Parameter.DeviceType,
                                can.Parameter.DeviceIndex,
                                can.Parameter.CanIndex
                            )
                    );
        }

        public static void SetReference(this ICanController can, uint refType, out IntPtr pData)
        {
            throw new NotImplementedException("-0xFF[1326663]0xEE-");
        }

        /// <summary>
        /// 此函数用以启动CAN卡的某一个 CAN 通道。有多个CAN通道时，需要多次调用。
        /// </summary>
        /// <param name="can"></param>
        /// <param name="deviceType"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="canIndex"></param>
        /// <exception cref="InvalidCanOperationException">如果开始失败</exception>
        public static void Start(this ICanController can)
        {
            can.CheckOpearateSuccess(
                Card.StartCAN(can.Parameter.DeviceType,
                                can.Parameter.DeviceIndex,
                                can.Parameter.CanIndex
                    )
                );
        }

        public static void Write(this ICanController can, ref VciCanFrame frame)
        {
            if (Card.Transmit(can.Parameter.DeviceType,
                                can.Parameter.DeviceIndex,
                                can.Parameter.CanIndex, ref frame, 1) != 1)
                throw new InvalidCanOperationException($"-0xFF[1326664]0xEE-Send data error (via CAN):{can.ReadErrInfo()}");
        }

        public static void Write(this ICanController can, IEnumerable<VciCanFrame> frames)
        {
            Action<VciCanFrame> sender = frame =>
            {
                if (Card.Transmit(can.Parameter.DeviceType,
                                can.Parameter.DeviceIndex,
                                can.Parameter.CanIndex,
                                ref frame,
                                1
                            ) != 1)
                    throw new InvalidCanOperationException("-0xFF[1326665]0xEE-Send data error (via CAN). May not send completely");
            };
            frames.ToList().ForEach(f => sender(f));
        }

        #region Error Check
        /// <summary>
        /// 确认CAN的读取是否成功。（如果返回值为 0xFFFFFFFF，则表示读取数据失败，有错误发
        /// 生，请调用 VCI_ReadErrInfo 函数来获取错误码
        /// </summary>
        /// <param name="returnValue">函数返回值</param>
        public static void CheckIoSuccess(this ICanController can, ulong reValue)
        {
            if (0xFFFFFFFF == reValue)
                throw new InvalidCanOperationException($"-0xFF[1326666]0xEE-{can.ReadErrInfo()}");
        }

        /// <summary>
        /// 确认Can的操作是否成功(基于返回值,为 1 表示操作成功，0 表示操作失败。)
        /// </summary>
        /// <param name="returnValue"></param>
        /// <exception cref="InvalidCanOperationException">当Can操作失败（为0或其他值时）发生</exception>
        public static void CheckOpearateSuccess(this ICanController can, ulong reValue)
        {
            if (can is SimulatedCanContorller) return;
            switch (reValue)
            {
                case 1:
                    break;
                case 0:
                    throw new InvalidCanOperationException($"-0xFF[1326667]0xEE-{can.ReadErrInfo()}");
                default:
                    throw new InvalidCanOperationException("-0xFF[1326668]0xEE-Unknow return value");
            }
        }
        #endregion
    }
}
