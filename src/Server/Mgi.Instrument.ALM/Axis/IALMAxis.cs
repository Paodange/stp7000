using Mgi.Instrument.ALM.Attr;
using Mgi.Robot.Cantroller;

namespace Mgi.Instrument.ALM.Axis
{
    public interface IALMAxis : ISp200Axis
    {
        /// <summary>
        /// 移动 并执行软限位,丢步检查  如果 轴配置了不检查（IfCheckDeviation=false）  则等同于Move
        /// </summary>
        /// <param name="pulse"></param>
        /// <param name="type"></param>
        void MoveWithCheck(int pulse, MoveType type);
        void MoveBeginWithCheck(int pulse, MoveType type);

        /// <summary>
        /// move 不执行软限位检查，根据IfCheckDeviation执行丢步检查
        /// </summary>
        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        void MoveFriendly(double mm, MoveType type);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        void MoveFriendlyWithCheck(double mm, MoveType type);
        /// <summary>
        /// move 不执行软限位检查
        /// </summary>

        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        void MoveFriendlyBegin(double mm, MoveType type);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mm">距离单位为毫米，体积单位为微升</param>
        /// <param name="type"></param>
        void MoveFriendlyBeginWithCheck(double mm, MoveType type);

        /// <summary>
        /// MoveEnd之后  执行丢步检查 如果 轴配置了不检查（IfCheckDeviation=false）  则等同于MoveEnd
        /// </summary>
        void MoveEndWithCheck();

        /// <summary>
        /// 
        /// </summary>
        [NonInterceptor]
        void MoveEndWithCheckNonInterceptor();

        /// <summary>
        /// 检查扎吸头最小最大丢步阈值检测是否在范围内
        /// </summary>
        /// <param name="minAccurary"></param>
        /// <param name="maxAccuracy"></param>
        void LoadTipMoveEndWithCheck(int minAccurary, int maxAccuracy);

        /// <summary>
        /// 毫米/微升 转脉冲
        /// </summary>
        /// <param name="unit">1单位 距离单位为毫米，体积单位为微升</param>
        /// <returns></returns>
        int MMToPulse(double unit);
        /// <summary>
        /// 脉冲转 毫米/微升
        /// </summary>
        /// <param name="pulse"></param>
        /// <returns></returns>
        double PulseToMM(double pulse);
    }
}
