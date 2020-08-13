//using Mgi.Robot.Cantroller;
//using System;

//namespace Mgi.Instrument.ALM.Util
//{
//    public static class ISp200AxisExtention
//    {
//        /// <summary>
//        /// 移动 并执行软限位,丢步检查  如果 轴配置了不检查（IfCheckDeviation=false）  则等同于Move
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="pulse"></param>
//        /// <param name="type"></param>
//        public static void MoveWithCheck(this ISp200Axis axis, int pulse, MoveType type)
//        {
//            var target = SoftLimitCheck(axis, pulse, type);
//            axis.Move(target, MoveType.ABS);
//            if (axis.IfCheckDeviation)
//            {
//                AccuracyCheck(axis);
//            }
//        }
//        public static void MoveBeginWithCheck(this ISp200Axis axis, int pulse, MoveType type)
//        {
//            var target = SoftLimitCheck(axis, pulse, type);
//            axis.MoveBegin(target, MoveType.ABS);
//        }

//        /// <summary>
//        /// move 不执行软限位检查，根据IfCheckDeviation执行丢步检查
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
//        /// <param name="type"></param>
//        public static void MoveFriendly(this ISp200Axis axis, double mm, MoveType type)
//        {
//            var pulse = MMToPulse(axis, mm);
//            axis.Move(pulse, type);
//            if (axis.IfCheckDeviation)
//            {
//                AccuracyCheck(axis);
//            }
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
//        /// <param name="type"></param>
//        public static void MoveFriendlyWithCheck(this ISp200Axis axis, double mm, MoveType type)
//        {
//            var pulse = MMToPulse(axis, mm);
//            var target = SoftLimitCheck(axis, pulse, type);
//            axis.Move(target, MoveType.ABS);
//            if (axis.IfCheckDeviation)
//            {
//                AccuracyCheck(axis);
//            }
//        }
//        /// <summary>
//        /// move 不执行软限位检查
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
//        /// <param name="type"></param>
//        public static void MoveFriendlyBegin(this ISp200Axis axis, double mm, MoveType type)
//        {
//            var pulse = MMToPulse(axis, mm);
//            axis.MoveBegin(pulse, type);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
//        /// <param name="type"></param>
//        public static void MoveFriendlyBeginWithCheck(this ISp200Axis axis, double mm, MoveType type)
//        {
//            var pulse = MMToPulse(axis, mm);
//            var target = SoftLimitCheck(axis, pulse, type);
//            axis.MoveBegin(target, MoveType.ABS);
//        }

//        /// <summary>
//        /// MoveEnd之后  执行丢步检查 如果 轴配置了不检查（IfCheckDeviation=false）  则等同于MoveEnd
//        /// </summary>
//        /// <param name="axis"></param>
//        public static void MoveEndWithCheck(this ISp200Axis axis)
//        {
//            axis.MoveEnd();
//            if (axis.IfCheckDeviation)
//            {
//                AccuracyCheck(axis);
//            }
//        }


//        /// <summary>
//        /// 毫米/微升 转脉冲
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="unit">1单位 距离单位为毫米，体积单位为微升</param>
//        /// <returns></returns>
//        public static int MMToPulse(this ISp200Axis axis, double unit)
//        {
//            var name = axis.Name;
//            if (name.StartsWith("Simulated_"))
//            {
//                name = name.Replace("Simulated_", "");
//            }
//            double value;
//            switch (name)
//            {
//                //移液器X轴
//                case "X":
//                    value = unit * 200 * 64 / Math.PI / 28.65; //mm*200*64/PI()/28.65
//                    break;
//                //移液器Y轴
//                case "Y1":
//                case "Y2":
//                    value = unit * 200 * 64 / Math.PI / 12.73; //mm*200*64/PI()/12.73
//                    break;
//                //移液器Z轴
//                case "PZ1":
//                case "PZ2":
//                    value = unit * 200 * 64 / 10.16; //mm*200*64/10.16
//                    break;
//                //移液器P轴 吸液排液用
//                case "P1":
//                case "P2":
//                    value = unit * 200 * 64 / Math.PI / 5.08 / 3.57 / 3.57; //v*200*64/PI()/5.08/3.57/3.57
//                    break;
//                //拔盖旋转电机
//                case "R1":
//                case "R2":
//                case "R3":
//                case "R4":
//                    value = unit * 200 * 64 * 4 / 360; //unit*200*64*4/360 degree
//                    break;
//                //拔盖器Z轴
//                case "DZ1":
//                case "DZ2":
//                    value = unit * 200 * 64 / 10; //mm*200*64/10
//                    break;
//                //导轨T轴
//                case "T1":
//                case "T2":
//                    value = unit * 200 * 64 / Math.PI / 19.1; //mm*200*64/PI()/19.1
//                    break;
//                //导轨夹紧试管的轴
//                case "C1":
//                case "C2":
//                case "E1":
//                case "E2":
//                    value = unit * 200 * 64 / 3.175; //mm*200*64/3.175
//                    break;
//                default:
//                    throw new Exception($"axis:{axis.Name} not supported");
//            }
//            return (int)Math.Round(value, 0, MidpointRounding.AwayFromZero);
//        }

//        /// <summary>
//        /// 脉冲转 毫米/微升
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="pulse"></param>
//        /// <returns></returns>
//        public static double PulseToMM(this ISp200Axis axis, double pulse)
//        {
//            double value;
//            switch (axis.Name)
//            {
//                //移液器X轴
//                case "X":
//                    value = pulse * 28.65 * Math.PI / 200 / 64; //unit * 200 * 64 / Math.PI / 28.65; //mm*200*64/PI()/28.65
//                    break;
//                //移液器Y轴
//                case "Y1":
//                case "Y2":
//                    value = pulse * Math.PI * 12.73 / 200 / 64; //mm*200*64/PI()/12.73
//                    break;
//                //移液器Z轴
//                case "PZ1":
//                case "PZ2":
//                    value = pulse * 10.16 / 200 / 64; //mm*200*64/10.16
//                    break;
//                //移液器P轴 吸液排液用
//                case "P1":
//                case "P2":
//                    value = pulse * 3.57 * 3.57 * 5.08 * Math.PI / 200 / 64; //v*200*64/PI()/5.08/3.57/3.57
//                    break;
//                //拔盖旋转电机
//                case "R1":
//                case "R2":
//                case "R3":
//                case "R4":
//                    value = pulse * 4 / 200 / 64; //mm*200*64/4
//                    break;
//                //拔盖器Z轴
//                case "DZ1":
//                case "DZ2":
//                    value = pulse * 10 / 200 / 64; //mm*200*64/10
//                    break;
//                //导轨T轴
//                case "T1":
//                case "T2":
//                    value = pulse * 19.1 * Math.PI / 200 / 64; //mm*200*64/PI()/19.1
//                    break;
//                //导轨夹紧试管的轴
//                case "C1":
//                case "C2":
//                case "E1":
//                case "E2":
//                    value = pulse * 3.175 / 200 / 64; //mm*200*64/3.175
//                    break;
//                default:
//                    throw new Exception($"axis:{axis.Name} not supported");
//            }
//            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
//        }

//        private static void AccuracyCheck(ISp200Axis axis, double factor = 1.0)
//        {
//            var encoder = (int)(axis.ReadEncoderPosition() * factor);
//            var actual = axis.ReadActualPosition();
//            var target = axis.ReadTargetPosision();
//            var diff = Math.Abs(actual - encoder);
//            if (diff > axis.AllowedDeviation)
//            {
//                throw new AccuracyCheckFailException($"Axis:{axis.Name} accuracy check fail:{diff},allowed:{axis.AllowedDeviation},encoder:{encoder},actual:{actual},target:{target}", axis, actual, encoder);
//            }
//        }

//        private static int SoftLimitCheck(ISp200Axis axis, int pulse, MoveType type)
//        {
//            (var target, _) = axis.GetValidatedDestination(type, pulse);
//            return target;
//        }
//    }
//}
