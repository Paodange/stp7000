using System.Collections.Generic;

namespace Mgi.Epson.Robot.T3
{
    public interface IEpsonRobot
    {
        void Open();
        void Close();
        void Initialize();

        /// <summary>
        /// 移动到目标位置-GO方式
        /// </summary>
        /// <param name="pointNum"></param>
        void Go(int pointNum, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0);

        /// <summary>
        /// 移动到目标位置-Jump方式
        /// </summary>
        /// <param name="pointNum"></param>
        void Jump(int pointNum, double xOffset = 0, double yOffset = 0, double zOffset = 0);

        /// <summary>
        /// 设置最大速度百分比
        /// </summary>
        /// <param name="maxSpeedPercent"></param>
        void SetSpeed(int maxSpeedPercent);

        /// <summary>
        /// 设置最大加减速度百分比
        /// </summary>
        /// <param name="maxAccelPercent"></param>
        /// <param name="maxDecelPercent"></param>
        void SetAccel(int maxAccelPercent, int maxDecelPercent);

        /// <summary>
        /// 开启电机使能
        /// </summary>
        /// <exception cref="TongsExcetion"></exception>
        void MotorOn();

        /// <summary>
        /// 关闭电机使能
        /// </summary>
        /// <exception cref="TongsExcetion"></exception>
        void MotorOff();

        /// <summary>
        /// 抛异常后，重置仪器状态
        /// </summary>
        /// <exception cref="TongsExcetion"></exception>
        void Reset();

        /// <summary>
        /// 高功率模式
        /// </summary>
        /// <exception cref="TongsExcetion"></exception>
        void PowerHigh();

        /// <summary>
        /// 低功率模式
        /// </summary>
        /// <exception cref="TongsExcetion"></exception>
        void PowerLow();


        void Pause();

        void Resume();

        void ResetCmd();

        /// <summary>
        /// 各轴相对移动
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="u"></param>
        void BGo(double x, double y, double z, double u);

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <returns></returns>
        Dictionary<string, double> Current();

        /// <summary>
        /// 获取具体位点的坐标值
        /// </summary>
        /// <param name="pointNum"></param>
        /// <returns></returns>
        Dictionary<string, double> GetPos(int pointNum);

        /// <summary>
        /// 获取阵列的坐标值
        /// </summary>
        /// <param name="palletNo"></param>
        /// <param name="colrowNo"></param>
        Dictionary<string, double> GetPalletPos(int palletNo, int colrowNo);

        /// <summary>
        /// 转移到托盘具体的行列号
        /// </summary>
        /// <param name="palletNo">托盘号</param>
        /// <param name="row">行列号</param>
        /// <param name="column">行列号</param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="zOffset"></param>
        void GoPalletNo(int palletNo, int row, int column, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0);
        void JumpPalletNo(int palletNo, int row, int column, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0);
        /// <summary>
        /// 获取IO点状态
        /// </summary>
        /// <param name="io"></param>
        /// <returns></returns>
        int GetIO(int io);

        /// <summary>
        /// 直线插补
        /// </summary>
        /// <param name="pointNum"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="zOffset"></param>
        void Move(int pointNum, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0);

        /// <summary>
        /// 最大扭矩百分比
        /// </summary>
        /// <param name="percent"></param>
        void LimitTorque(int percent);

        /// <summary>
        /// 直线插补速度设置
        /// </summary>
        /// <param name="speed"></param>
        void MoveSpeed(int speed);

        /// <summary>
        /// 直线插补加速度
        /// </summary>
        /// <param name="accel"></param>
        void MoveAccel(int accel);
    }
}
