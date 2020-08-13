namespace Mgi.Epson.Robot.T3
{
    public enum RobotAction
    {
        /// <summary>
        /// Z轴抬起在移动XY,Z不回去
        /// </summary>
        JumpZ0,
        /// <summary>
        /// Z轴抬起在移动XY,Z回去
        /// </summary>
        Jump,
        /// <summary>
        /// 最短路线
        /// </summary>
        Go,
        /// <summary>
        /// 直线运动
        /// </summary>
        Move,
        /// <summary>
        /// 弧度
        /// </summary>
        Arc,
        /// <summary>
        /// 三个点弧度
        /// </summary>
        Arc3
    }
}
