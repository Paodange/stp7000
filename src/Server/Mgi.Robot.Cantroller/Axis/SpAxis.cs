

using System;
using System.Collections.Concurrent;
using System.Linq;
using Mgi.ALM.Util.Extension;
using Mgi.Robot.Cantroller.Can;
using static System.Threading.Thread;
using static Mgi.Robot.Cantroller.Axis.AxisFrameFactory;

namespace Mgi.Robot.Cantroller.Axis
{
    /// <summary>
    /// 此类为轴控制驱动 *本类不是线程安全的 （受限于自动化组的协议）
    /// </summary>
    public class SpAxis : ISp200Axis
    {
        private readonly ICanController _can;
        private readonly AxisConfig _config;
        #region Properities

        public byte No => _config.No;

        public string Name => _config.Name;

        public byte FrameId => _config.FrameId;

        public bool IfCheckDeviation => _config.IfCheckDeviation;

        public int AllowedDeviation => _config.AllowedDeviation;

        public TimeSpan Max => _config.WaitTimeout;

        public TimeSpan Interval { get; }

        public int SoftMinLimit => _config.SoftLimitMin;

        public int SoftMaxLimit => _config.SoftLimitMax;

        #endregion

        public SpAxis(AxisConfig config, ICanController can)
        {
            _config = config;
            _can = can;
            Interval = TimeSpan.FromMilliseconds(200);
        }

        /// <summary>
        /// 使用配置参数
        /// </summary>
        public void UseConfigSetting()
        {
            _config.Setting
                    .ForEach(s => Setting(s.Type, s.Value));

            Sleep(50);
        }

        /// <summary>
        /// 运动一个脉冲
        /// </summary>
        /// <param name="pulse"></param>
        /// <exception cref="ExceedDeviationException"></exception>
        /// <exception cref="TimeoutException">超时错误</exception>
        public void Move(int pulse, MoveType type = MoveType.ABS)
        {
            MoveBegin(pulse, type);
            MoveEnd();
        }

        /// <summary>
        /// 发出运动命令。如果通信线路问题，可能会阻塞
        /// </summary>
        /// <param name="pluses"></param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidCanOperationException"></exception>
        public void MoveBegin(int pluses, MoveType type = MoveType.ABS)
        {
            var frame = GenerateMoveFrame(No, type, pluses);
            this.Request(frame)
                .IsMyAckFrame(FrameId, frame.InstructionNo)
                .ThrowIfNotChecked(this.FormatInformation(frame));
        }

        /// <summary>
        /// 等待/确认运动结束。该函数阻塞，直到运动结束
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidCanOperationException"></exception>
        /// <exception cref="AxisOperateException">轴操作异常</exception>
        public void MoveEnd()
        {
            var frame = GenerateQueryReachedFrame(No);
            (this as object).WaitForEnd(Max, Interval, () => LoopQueryFunc(r => r.Data.ToUint32() == 1).Invoke(frame))
                            .ThrowIfNotChecked($"Move end check error. May be timeout error.{this.FormatInformation(frame)}");
        }

        /// <summary>
        /// 可以等待轮询的查询函数。condition为轮询时的条件判断函数，如果condition为true,则queryloop结束，返回true。
        /// </summary>
        /// <param name="condition">Response是否满足指定条件</param>
        /// <returns>LoopQueryFunc(request)当response不是request的响应，或者condition不满足时返回false.</returns>
        private Func<AxisRequestFrame, bool> LoopQueryFunc(Func<AxisResponseFrame, bool> condition)
        {
            return (req) =>
            {
                var response = Request(req);
                return response.IsMyAckFrame(FrameId, req.InstructionNo)
                        ? condition.Invoke(response)
                        : false
                        ;
            };
        }

        /// <summary>
        /// 回到电机的原点。（绝对原点）
        /// </summary>
        public void GoHome()
        {
            HomeBegin();
            HomeEnd();
        }

        /// <summary>
        /// 开始回到电机远点。（绝对远点）
        /// </summary>
        public void HomeBegin()
        {
            var frame = CreateAskHomeFrame(No);
            Request(frame)
                .IsMyAckFrame(FrameId, frame.InstructionNo)
                .ThrowIfNotChecked($"({Name})Home begin error, is not my ack frame");
        }

        /// <summary>
        /// 等待电机回到原点。该函数是阻塞的，至少阻塞Interval时间。
        /// </summary>
        public void HomeEnd()
        {
            var frame = GenerateReferenceSearchFrame(No);
            (this as object).WaitForEnd(Max, Interval,
                                () => LoopQueryFunc(f => f.IsReferenceSearchCompleted(FrameId, frame.InstructionNo))
                                        .Invoke(frame)
                             )
                             .ThrowIfNotChecked($"({Name})Home End (RFS status) Check failed or Timeout {this.FormatInformation(frame)}");
        }


        ///// <summary>
        ///// 获取当前的编码器试剂位置。(脉冲数)
        ///// </summary>
        ///// <returns></returns>
        //public int ReadEncoderPosition() => Query(GenerateQueryEncoderPosFrame(No));
        /// <summary>
        ///  获取当前的编码器试剂位置。(脉冲数)  有的板卡type不一样 增加一个type参数
        /// </summary>
        /// <returns></returns>
        public int ReadEncoderPosition()
        {
            var frame = GenerateQueryEncoderPosFrame(No);
            frame.Type = _config.EncoderPosFrameType;
            return Query(frame);
        }

        /// <summary>
        /// 获取当前电机的当前位置 (脉冲数)
        /// </summary>
        /// <returns></returns>
        public int ReadActualPosition() => Query(GenerateQueryActualPosFrame(No));

        /// <summary>
        /// 获取当前电机的实际位置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AxisOperateException">电机操作异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        public int ReadTargetPosision() => Query(GenerateQueryTargetPosFrame(No));

        /// <summary>
        /// 停止运动
        /// </summary>
        public void StopAsync()
        {
            Request(GenerateStopFrame(No));
            _can.ClearBuffer();
        }

        public bool IfStopped() => Query(GenerateQuerySpeedFrame(No)) == 0;


        /// <summary>
        /// 阻塞操作，直到IfStopped == true
        /// </summary>
        public void WaitforStopping()
        {
            (this as object).WaitForEnd(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(0.1), IfStopped)
                                .ThrowIfNotChecked($"Waitfor {Name} stopping timeout (Actual speed is not Zero after 30s");
        }

        /// <summary>
        /// 设置工作速度(脉冲)
        /// </summary>
        /// <param name="pulse"></param>
        public void SetRate(int pulse)
        {
            Setting(4, pulse);
        }
        public void SetToDefaultRate()
        {
            Setting(4, _config.Setting.FirstOrDefault(x => x.Type == 4).Value);
        }

        public void SettingToDefault(byte type)
        {
            Setting(type, _config.Setting.FirstOrDefault(x => x.Type == type).Value);
        }
        /// <summary>
        /// 设置轴。设置之后，可能需要手动等待一段时间。比如10ms?
        /// </summary>
        public void Setting(byte type, int value)
        {
            var frame = AxisFrameFactory.GenerateSapFreame(type, No, (int)value);
            Request(frame)
                .IsMyAckFrame(FrameId, frame.InstructionNo)
                .ThrowIfNotChecked($"({Name})Setting axis parameters error, is not my ack frame");
        }

        /// <summary>
        /// 写目标位置寄存器值.
        /// </summary>
        /// <param name="value"></param>
        public void WriteTargetPosition(int value)
        {
            RequestWithNoResponse(
                frame: GenerateSetTargetPosFrame(No, value),
                error: @"Write target position fail, is not my ack frame"
            );
        }

        public void WriteActualPosition(int value)
        {
            RequestWithNoResponse(
                frame: GenerateSetActualPosFrame(No, value),
                error: @"Write actual position fail, is not my ack frame"
            );
        }

        public void WriteEncoderPosition(int value)
        {
            var frame = GenerateSetEncoderPosFrame(No, value);
            frame.Type = _config.EncoderPosFrameType;
            RequestWithNoResponse(
                frame: frame,
                error: @"Write target position fail, is not my ack frame"
            );
        }

        /// <summary>
        /// 获取轴的工作属性信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int Getting(byte type) => Query(GenerateGapFrame(type, No));

        /// <summary>
        /// 设置电压
        /// The  motor  will  be  instructed  to  rotate  with  a  specified  velocity  in  right  direction  (increasing  the  position 
        /// counter). 
        /// </summary>
        /// <param name="velocity"></param>
        public void RotateRight(uint velocity)
        {
            RequestWithNoResponse(GenerateRorFrame(No, velocity),
                                    $"({Name})Error: Rotate with a specified velocity in right direction {velocity} fail");
            Sleep(TimeSpan.FromMilliseconds(20));
        }

        /// <summary>
        /// 返回当前电流值
        /// </summary>
        /// <returns></returns>
        public uint Current()
        {
            return (uint)Query(CurrentQueryFrame(No));
        }

        /// <summary>
        /// 设置当前的电流值
        /// </summary>
        /// <param name="current"></param>
        public void Current(uint current)
        {
            if (current > 255)
                throw new RobotException("-0xFF[1318658]0xEE-Axis current should be located in [0,255]");
            var frame = CurrentSetFrame(No, (int)current);
            RequestWithNoResponse(frame, $"Error: Set current {current} fail");
        }

        public void Sio(byte port, uint value)
        {
            RequestWithNoResponse(GenerateSioFrame(port, 2, value), $"({Name})SIO fail (timeout or inconformity): port = {port}, value = {value}");
            Sleep(TimeSpan.FromMilliseconds(20));
        }


        public uint Gio(byte port)
        {
            var state = Query(GenerateGioFrame(port, 0));
            Sleep(TimeSpan.FromMilliseconds(20));
            return (uint)state;
        }

        public uint Gap(byte type, byte moto)
        {
            var state = Query(GenerateGapFrame(type, moto));
            return (uint)state;
        }

        public int Gapio(int ioType)
        {
            var state = Query(GenerateGapFrame((byte)ioType, _config.No));
            Sleep(TimeSpan.FromMilliseconds(20));
            return (int)state;
        }

        #region private
        /// <summary>
        /// 执行查询指令，将响应指令的value解析为整数返回。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AxisOperateException">发生错误</exception>
        private int Query(AxisRequestFrame frame)
        {
            //_logger.DebugFormat("Query:{0}", frame.ToReadableString());
            var response = Request(frame);
            response.IsMyAckFrame(FrameId, frame.InstructionNo)
                    .ThrowIfNotChecked(this.FormatInformation(frame, response));
            return response.Data.ToInt32();
        }

        /// <summary>
        /// 不处理响应，只进行内部确认。
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="error"></param>
        private void RequestWithNoResponse(AxisRequestFrame frame, string error)
        {
            Request(frame)
               .IsMyAckFrame(FrameId, frame.InstructionNo)
               .ThrowIfNotChecked(error);
        }

        #endregion

        #region Protected
        private static readonly object syncLock = new object();
        /// <summary>
        /// 发送一个电机请求，并读取一个响应。执行不同的命令，该函数是阻塞的。
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="System.TimeoutException"></exception>
        /// <exception cref="InvalidCanOperationException">如果读写失败</exception>
        protected AxisResponseFrame Request(AxisRequestFrame frame)
        {
            //_logger?.Info(frame.ToReadableString());
            //VciCanFrame responseFrame;
            //// 解决多线程 操作  窜帧的问题
            //lock (syncLock)
            //{
            //    responseFrame = _can.Write(frame.ToArray(), frame.Size())
            //             .Read();
            //}
            //return responseFrame.ExtractBody()
            //               .ToAxisResponseFrame();

            _can.Write(frame.ToArray(), frame.Size());
            return GetResponse(frame);
        }



        protected void RequestAnyway(AxisRequestFrame frame)
        {
            //_logger?.Info($"Request anyway(not care response){frame.ToReadableString()}");
            _can.Write(frame.ToArray(), frame.Size());
        }

        public int CurrentPostion() => ReadActualPosition();

        private AxisResponseFrame GetResponse(AxisRequestFrame frame)
        {
            //_logger.Debug($"Request:{frame.ToJsonString()}");
            var requestId = new FrameIdentifier(FrameId, frame.InstructionNo, frame.RequestId);
            var d = DateTime.Now;
            AxisResponseFrame f;
            while (!responseFrames.TryRemove(requestId, out f))
            {
                var resp = _can.Read().ExtractBody().ToAxisResponseFrame();
                if (resp.FrameId != 0)
                {
                    responseFrames.TryAdd(new FrameIdentifier(resp.FrameId, resp.InstructionNo, resp.RequestId), resp);
                }
                if ((DateTime.Now - d).TotalSeconds > _config.WaitTimeout.TotalSeconds)
                {
                    //_logger.Error(responseFrames.ToJsonString());
                    throw new TimeoutException($"{frame.ToReadableString()} wait response timeout, timeout={_config.WaitTimeout.TotalSeconds}s");
                }
            }
            //_logger.Debug($"Response:{f.ToJsonString()}");
            return f;
        }

        private static readonly ConcurrentDictionary<FrameIdentifier, AxisResponseFrame> responseFrames
            = new ConcurrentDictionary<FrameIdentifier, AxisResponseFrame>();
        #endregion
    }

    struct FrameIdentifier : IEquatable<FrameIdentifier>
    {
        public byte FrameId;
        public byte InstructionNo;
        public byte RequestId;
        public FrameIdentifier(byte frameId, byte instructionNo, byte requestId)
        {
            FrameId = frameId;
            InstructionNo = instructionNo;
            RequestId = requestId;
        }

        public override string ToString()
        {
            return $"FrameId:{FrameId},InstructionNo:{InstructionNo},RequestId:{RequestId}";
        }

        public bool Equals(FrameIdentifier other)
        {
            return FrameId == other.FrameId && InstructionNo == other.InstructionNo && RequestId == other.RequestId;
        }

        public static bool operator ==(FrameIdentifier left, FrameIdentifier right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(FrameIdentifier left, FrameIdentifier right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return (FrameId, InstructionNo, RequestId).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FrameIdentifier))
            {
                return false;
            }
            return Equals((FrameIdentifier)obj);
        }
    }
}
