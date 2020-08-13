using Mgi.Epson.Robot.T3;

namespace Mgi.Instrument.ALM.Device
{
    /// <summary>
    /// 主机械臂接口
    /// </summary>
    public interface IALMRobot : IALMDevice
    {
        /// <summary>
        /// 
        /// </summary>
        IEpsonRobot EpsonRobot { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tubeId">试管id</param>
        /// <param name="throwIfOverTolerance">超过误差阈值是否要抛出异常</param>
        int TightenGripper(string tubeId, bool throwIfOverTolerance = true);
        /// <summary>
        /// 
        /// </summary>
        int ReleaseGripper();
        /// <summary>
        /// 复位
        /// </summary>
        void Home();


        /// <summary>
        /// 抓取并返回
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="location"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void Grasp(string tubeId, RobotLocation location, int row = 0, int column = 0);

        /// <summary>
        /// 释放并返回
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="location"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void Loosen(string tubeId, RobotLocation location, int row = 0, int column = 0);

        /// <summary>
        /// 仅抓取  抓取后保持抓取位置
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="location"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void GraspOnly(string tubeId, RobotLocation location, int row = 0, int column = 0);

        /// <summary>
        /// 仅释放 释放后保持在释放的位置
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="location"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void LoosenOnly(string tubeId, RobotLocation location, int row = 0, int column = 0);
        /// <summary>
        /// 抓取后 回安全位置
        /// </summary>
        /// <param name="location"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void GraspGoBack(RobotLocation location, int row = 0, int column = 0);
        /// <summary>
        /// 放下后 回安全位置
        /// </summary>
        /// <param name="location"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void LoosenGoBack(RobotLocation location, int row = 0, int column = 0);


    }
}
