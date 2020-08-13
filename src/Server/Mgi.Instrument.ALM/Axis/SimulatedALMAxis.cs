using System;
using log4net;
using Mgi.Robot.Cantroller;
using Mgi.Robot.Cantroller.Axis;
using Mgi.Robot.Cantroller.Can;

namespace Mgi.Instrument.ALM.Axis
{
    public class SimulatedALMAxis : SimulatedAxis, IALMAxis
    {
        public SimulatedALMAxis(AxisConfig config, ICanController can, ILog logger)
           : base(config, can, logger)
        {

        }
        /// <summary>
        /// 移动 并执行软限位,丢步检查  如果 轴配置了不检查（IfCheckDeviation=false）  则等同于Move
        /// </summary>
        /// <param name="pulse"></param>
        /// <param name="type"></param>
        public void MoveWithCheck(int pulse, MoveType type)
        {
            var target = SoftLimitCheck(pulse, type);
            Move(target, MoveType.ABS);
            if (IfCheckDeviation)
            {
                AccuracyCheck();
            }
        }
        public void MoveBeginWithCheck(int pulse, MoveType type)
        {
            var target = SoftLimitCheck(pulse, type);
            MoveBegin(target, MoveType.ABS);
        }

        /// <summary>
        /// move 不执行软限位检查，根据IfCheckDeviation执行丢步检查
        /// </summary>
        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        public void MoveFriendly(double mm, MoveType type)
        {
            var pulse = MMToPulse(mm);
            Move(pulse, type);
            if (IfCheckDeviation)
            {
                AccuracyCheck();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        public void MoveFriendlyWithCheck(double mm, MoveType type)
        {
            var pulse = MMToPulse(mm);
            var target = SoftLimitCheck(pulse, type);
            Move(target, MoveType.ABS);
            if (IfCheckDeviation)
            {
                AccuracyCheck();
            }
        }
        /// <summary>
        /// move 不执行软限位检查
        /// </summary>
        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        public void MoveFriendlyBegin(double mm, MoveType type)
        {
            var pulse = MMToPulse(mm);
            MoveBegin(pulse, type);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        public void MoveFriendlyBeginWithCheck(double mm, MoveType type)
        {
            var pulse = MMToPulse(mm);
            var target = SoftLimitCheck(pulse, type);
            MoveBegin(target, MoveType.ABS);
        }

        /// <summary>
        /// MoveEnd之后  执行丢步检查 如果 轴配置了不检查（IfCheckDeviation=false）  则等同于MoveEnd
        /// </summary>
        public void MoveEndWithCheck()
        {
            MoveEnd();
            if (IfCheckDeviation)
            {
                AccuracyCheck();
            }
        }

        public void MoveEndWithCheckNonInterceptor()
        {
            MoveEnd();
            if (IfCheckDeviation)
            {
                AccuracyCheck();
            }
        }
        public void LoadTipMoveEndWithCheck(int minAccurary, int maxAccuracy)
        {
            MoveEnd();
            var encoder = ReadEncoderPosition();
            var actual = ReadActualPosition();
            var diff = actual - encoder;// 丢步了，编码器的值会比实际值小  
            if (diff < minAccurary || diff > maxAccuracy)
            {
                // 丢步不在扎吸头预计的范围内  有可能是没扎到吸头或是碰撞了  抛出此异常
                throw new Exception($"{Name} load tip error,accuracy range[{minAccurary},{maxAccuracy}],actual:{diff}");
            }
            if (diff != 0)
            {
                this.FixedPosRegister();
            }
        }

        /// <summary>
        /// 毫米/微升 转脉冲
        /// </summary>
        /// <param name="unit">1单位 距离单位为毫米，体积单位为微升</param>
        /// <returns></returns>
        public int MMToPulse(double unit)
        {
            var name = Name;
            if (name.StartsWith("Simulated_"))
            {
                name = name.Replace("Simulated_", "");
            }
            double value;
            switch (name)
            {
                //移液器X轴
                case "X":
                    value = unit * 200 * 64 / Math.PI / 28.65; //mm*200*64/PI()/28.65
                    break;
                //移液器Y轴
                case "Y1":
                case "Y2":
                    value = unit * 200 * 64 / Math.PI / 12.73; //mm*200*64/PI()/12.73
                    break;
                //移液器Z轴
                case "PZ1":
                case "PZ2":
                    value = unit * 200 * 64 / 10.16; //mm*200*64/10.16
                    break;
                //移液器P轴 吸液排液用
                case "P1":
                case "P2":
                    value = unit * 200 * 64 / Math.PI / 5.08 / 3.57 / 3.57; //v*200*64/PI()/5.08/3.57/3.57
                    break;
                //拔盖旋转电机
                case "R1":
                case "R2":
                case "R3":
                case "R4":
                    value = unit * 200 * 64 * 4 / 360; //unit*200*64*4/360 degree
                    break;
                //拔盖器Z轴
                case "DZ1":
                case "DZ2":
                    value = unit * 200 * 64 / 10; //mm*200*64/10
                    break;
                //导轨T轴
                case "T1":
                case "T2":
                    value = unit * 200 * 64 / Math.PI / 19.1; //mm*200*64/PI()/19.1
                    break;
                //导轨夹紧试管的轴
                case "C1":
                case "C2":
                case "E1":
                case "E2":
                    value = unit * 200 * 64 / 3.175; //mm*200*64/3.175
                    break;
                default:
                    throw new Exception($"axis:{Name} not supported");
            }
            return (int)Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 脉冲转 毫米/微升
        /// </summary>
        /// <param name="pulse"></param>
        /// <returns></returns>
        public double PulseToMM(double pulse)
        {
            double value;
            switch (Name)
            {
                //移液器X轴
                case "X":
                    value = pulse * 28.65 * Math.PI / 200 / 64; //unit * 200 * 64 / Math.PI / 28.65; //mm*200*64/PI()/28.65
                    break;
                //移液器Y轴
                case "Y1":
                case "Y2":
                    value = pulse * Math.PI * 12.73 / 200 / 64; //mm*200*64/PI()/12.73
                    break;
                //移液器Z轴
                case "PZ1":
                case "PZ2":
                    value = pulse * 10.16 / 200 / 64; //mm*200*64/10.16
                    break;
                //移液器P轴 吸液排液用
                case "P1":
                case "P2":
                    value = pulse * 3.57 * 3.57 * 5.08 * Math.PI / 200 / 64; //v*200*64/PI()/5.08/3.57/3.57
                    break;
                //拔盖旋转电机
                case "R1":
                case "R2":
                case "R3":
                case "R4":
                    value = pulse * 4 / 200 / 64; //mm*200*64/4
                    break;
                //拔盖器Z轴
                case "DZ1":
                case "DZ2":
                    value = pulse * 10 / 200 / 64; //mm*200*64/10
                    break;
                //导轨T轴
                case "T1":
                case "T2":
                    value = pulse * 19.1 * Math.PI / 200 / 64; //mm*200*64/PI()/19.1
                    break;
                //导轨夹紧试管的轴
                case "C1":
                case "C2":
                case "E1":
                case "E2":
                    value = pulse * 3.175 / 200 / 64; //mm*200*64/3.175
                    break;
                default:
                    throw new Exception($"axis:{Name} not supported");
            }
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private void AccuracyCheck(double factor = 1.0)
        {
            var encoder = (int)(ReadEncoderPosition() * factor);
            var actual = ReadActualPosition();
            var target = ReadTargetPosision();
            var diff = Math.Abs(actual - encoder);
            if (diff > AllowedDeviation)
            {
                throw new AccuracyCheckFailException($"Axis:{Name} accuracy check fail:{diff},allowed:{AllowedDeviation},encoder:{encoder},actual:{actual},target:{target}", this, actual, encoder);
            }
        }

        private int SoftLimitCheck(int pulse, MoveType type)
        {
            (var target, _) = this.GetValidatedDestination(type, pulse);
            return target;
        }

    }
}
