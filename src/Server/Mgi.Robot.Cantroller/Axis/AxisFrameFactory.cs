using Mgi.ALM.Util.Extension;
using System.Collections.Generic;
using System.Threading;

namespace Mgi.Robot.Cantroller.Axis
{

    /// <summary>
    /// 
    /// </summary>
    public static class AxisFrameFactory
    {
        private static int _id = 0;
        #region Properities
        private readonly static Dictionary<string, byte> _mapping = new Dictionary<string, byte>()
            {
                { "ROR",  1 }, // <motor number>, <velocity>  Rotate right with specified velocity
                { "ROL",  2 }, // <motor number>, <velocity>  Rotate left with specified velocity
                { "MST",  3 }, // <motor number>  Stop motor movement
                { "MVP",  4 }, //ABS|REL|COORD, <motor number>, <position|offset> Move to position (absolute or relative)
                { "SAP",  5 }, //<parameter>, <motor number>, <value>  Set axis parameter (motion control specific settings)
                { "GAP",  6 }, //<parameter>, <motor number>  Get axis parameter (read out motion control specific settings)
                { "STAP",  7 },//<parameter>, <motor number>  Store axis parameter permanently (non volatile)
                { "RSAP", 8 },// < parameter >, < motor number > Restore axis parameter
                { "SGP",  9 }, // < parameter >, < bank number >, value Set global parameter (module specific settings e.g.communication settings or TMCL user variables)
                { "GGP",  10 }, //< parameter >, < bank number > Get global parameter (read out module specific settings e.g.communication settings or TMCL user variables)
                { "STGP",  11 }, //< parameter >, < bank number > Store global parameter (TMCL user variables only)
                { "RSGP",  12 }, //< parameter >, < bank number > Restore global parameter (TMCL user variable only)
                { "RFS",  13 }, //START | STOP | STATUS, < motor number > Reference search
                { "SIO",  14 }, //< port number >, < bank number >, < value > Set digital output to specified value
                { "GIO",  15 }, //< port number >, < bank number > Get value of analogue / digital input
                { "CALC",  19 }, // < operation >, < value > Process accumulator & value
                { "COMP",  20 }, //< value > Compare accumulator <->value
                { "JC",  21 }, // < condition >, < jump address > Jump conditional
                { "JA",  22 },//< jump address > Jump absolute
                { "CSUB",  23 }, //< subroutine address > Call subroutine
                { "RSUB",  24 }, //Return from subroutine
                { "EI",  25 }, // < interrupt number > Enable interrupt
                { "DI",  26 }, //< interrupt number > Disable interrupt
                { "WAIT",  27 },//< condition >, < motor number >, < ticks > Wait with further program execution
                { "STOP",  28 },//   Stop program execution
                { "SAC",  29 }, // < bus  number >,  < number  of bites>, < send data > SPI bus access
                { "SCO",  30 }, //< coordinate number >, < motor number >, < position > Set coordinate
                { "GCO",  31 }, //< coordinate number >, < motor number > Get coordinate
                { "CCO",  32 }, //< coordinate number >, < motor number > Capture coordinate
                { "CALCX", 33 }, //< operation > Process accumulator & X - register
                { "AAP",  34 }, //< parameter >, < motor number > Accumulator to axis parameter
                { "AGP",  35 }, //< parameter >, < bank number > Accumulator to global parameter
                { "VECT",  37 }, //< interrupt number >, < label > Set interrupt vector
                { "RETI",  38 },   //Return from interrupt
                { "ACO",  39 }
            };
        #endregion

        public static IReadOnlyDictionary<string, byte> CommandMapping => _mapping;

        /// <summary>
        /// 创建轴回Home的帧
        /// </summary>
        /// <returns></returns>
        public static AxisRequestFrame CreateAskHomeFrame(byte moto) => GenerateRequestFrame("RFS", moto, 0);


        public static AxisRequestFrame GenerateRequestFrame(string cmd, byte moto, byte type)
        {
            return new AxisRequestFrame()
            {
                RequestId = NewRequestId(),
                InstructionNo = CommandMapping[cmd],
                MotorOrBand = moto,
                Type = type
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moto"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateReferenceSearchFrame(byte moto) => GenerateRequestFrame("RFS", moto, 0x02);

        /// <summary>
        /// 电流设置
        /// </summary>
        /// <param name="moto"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static AxisRequestFrame CurrentSetFrame(byte moto, int current)
                => GenerateSapFreame(6, moto, current);

        /// <summary>
        /// 电流获取
        /// </summary>
        /// <param name="moto"></param>
        /// <returns></returns>
        public static AxisRequestFrame CurrentQueryFrame(byte moto)
                => GenerateGapFrame(6, moto);

        /// <summary>
        /// 构建发出运送指令的帧
        /// </summary>
        /// <param name="moto"></param>
        /// <param name="type"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateMoveFrame(byte moto, MoveType type, int pos)
        {
            var frame = new AxisRequestFrame()
            {
                RequestId = NewRequestId(),
                InstructionNo = CommandMapping[@"MVP"],
                MotorOrBand = moto,
                Type = (byte)type,
                TargetAddress = 1,
                Data = pos.ToByteArray()
            };
            return frame;
        }

        /// <summary>
        /// 构建查询是否到达目标位置的帧
        /// </summary>
        /// <param name="moto"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateQueryReachedFrame(byte moto)
                => GenerateGapFrame(8, moto);

        /// <summary>
        /// 构建查询编码器实际位置的帧
        /// </summary>
        /// <param name="moto"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateQueryEncoderPosFrame(byte moto)
                => GenerateGapFrame(209, moto);



        public static AxisRequestFrame GenerateRorFrame(byte moto, uint value)
                => new AxisRequestFrame()
                {
                    RequestId = NewRequestId(),
                    InstructionNo = CommandMapping["ROR"],
                    MotorOrBand = moto,
                    Data = value.ToByteArray()
                };

        /// <summary>
        /// 构建查询实际位置的帧
        /// </summary>
        /// <param name="moto"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateQueryActualPosFrame(byte moto)
                => GenerateGapFrame(1, moto);


        /// <summary>
        /// 构建查询目标位置的帧
        /// </summary>
        /// <param name="moto"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateQueryTargetPosFrame(byte moto)
                => GenerateGapFrame(0, moto);


        public static AxisRequestFrame GenerateGapFrame(byte type, byte moto)
                => GenerateRequestFrame("GAP", moto, type);


        public static AxisRequestFrame GenerateQuerySpeedFrame(byte moto)
            => GenerateGapFrame(3, moto);

        /// <summary>
        /// 设置轴目标位置值
        /// </summary>
        /// <param name="moto"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateSetTargetPosFrame(byte moto, int value)
                => GenerateSapFreame(0, moto, value);


        /// <summary>
        /// 设置轴实际位置值
        /// </summary>
        /// <param name="moto"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateSetActualPosFrame(byte moto, int value)
                => GenerateSapFreame(1, moto, value);

        public static AxisRequestFrame GenerateStopFrame(byte moto)
                => new AxisRequestFrame()
                {
                    RequestId = NewRequestId(),
                    InstructionNo = CommandMapping["MST"],
                    MotorOrBand = moto
                };

        /// <summary>
        /// 设置编码器位置值
        /// </summary>
        /// <param name="moto"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AxisRequestFrame GenerateSetEncoderPosFrame(byte moto, int value)
                => GenerateSapFreame(209, moto, value);



        public static AxisRequestFrame GenerateSapFreame(byte type, byte moto, int value)
                => new AxisRequestFrame()
                {
                    RequestId = NewRequestId(),
                    InstructionNo = CommandMapping["SAP"],
                    MotorOrBand = moto,
                    Type = type,
                    Data = ((uint)value).ToByteArray()
                };


        public static AxisRequestFrame GenerateSioFrame(byte port, byte moto, uint value)
                => new AxisRequestFrame()
                {
                    RequestId = NewRequestId(),
                    InstructionNo = CommandMapping["SIO"],
                    MotorOrBand = moto,
                    Type = port,
                    Data = value.ToByteArray()
                };

        public static AxisRequestFrame GenerateGioFrame(byte port, byte moto, uint value = 0)
               => new AxisRequestFrame()
               {
                   RequestId = NewRequestId(),
                   InstructionNo = CommandMapping["GIO"],
                   MotorOrBand = moto,
                   Type = port,
                   Data = value.ToByteArray()
               };

        private static byte NewRequestId()
        {
            Interlocked.CompareExchange(ref _id, 0, byte.MaxValue);
            var id = Interlocked.Increment(ref _id);
            return (byte)id;
        }
    }
}
