using System;

namespace Mgi.Robot.Cantroller
{
    /// <summary>
    /// 此扩展类用来转换、补偿、Check Axis (hand)的位置
    /// </summary>
    public static class ISp200AxisExtension
    {
        /// <summary>
        /// 根据运动方式、axis特性，计算绝对目标位置，以及补偿值。
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="type"></param>
        /// <param name="pluse"></param>
        /// <returns>(补偿后的值，补偿的大小)。如果补偿大小为0，则代表无补偿</returns>
        /// <exception cref="RobotException">当确认失败时</exception>
        public static Tuple<int,int> GetValidatedDestination(this ISp200Axis hand, MoveType type, int pluse)
        {
            var target = hand.CaclTarget(type, pluse);
            var compensated = hand.CompensateIfNeeded(target);

            if (hand.IsDestinationOverflow(compensated))
                throw new RobotException(
                        string.Format("({0})Destination still over the boundary even it had been compensated. Abs target {1}, compensated {2}. Range[{3},{4}]",
                                       hand.Name ,target, compensated, hand.SoftMinLimit, hand.SoftMaxLimit
                                     )
                                );
            else
                return new Tuple<int, int>(compensated, compensated - target);
        }

        private static int CaclTarget(this ISp200Axis hand, MoveType type, int destination)
        {
            switch (type)
            {
                case MoveType.ABS:
                    return destination;
                case MoveType.REL:
                    return hand.CurrentPostion() + destination;
                default:
                    throw new RobotException($"-0xFF[1374687]0xEE-Not support {type}. Just can be {MoveType.ABS} & {MoveType.REL}");
            }
        }
        public static void FixedPosRegister(this ISp200Axis axis)
        {
            axis.RotateRight(0);
            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(50));
            axis.WriteActualPosition(axis.ReadEncoderPosition());
        }

        //Used abs destination
        private static int CompensateIfNeeded(this ISp200Axis hand, int destination)
        {
            const int Offset = 5;
            if (destination < hand.SoftMinLimit && hand.SoftMinLimit - destination < Offset )
            {
                return hand.SoftMinLimit;
            }
            else if (destination > hand.SoftMaxLimit && destination - hand.SoftMaxLimit < Offset)
                return hand.SoftMaxLimit;
            else
            {
                return destination;
            }
        }

        //Used abs destination
        private static bool IsDestinationOverflow(this ISp200Axis hand, int destination)
        {
            return !(hand.SoftMinLimit <= destination
                        && destination <= hand.SoftMaxLimit);
        }

        internal static (int, int, int) Registers(this ISp200Axis axis)
        {
            return (axis.ReadEncoderPosition(),
                        axis.ReadActualPosition(),
                        axis.ReadTargetPosision()
                    );
        }
    }
}
