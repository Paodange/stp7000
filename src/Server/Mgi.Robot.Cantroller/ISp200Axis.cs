using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Robot.Cantroller
{
    public interface ISp200Axis
    {
        byte No { get; }
        void UseConfigSetting();
        string Name { get; }
        int SoftMinLimit { get; }
        int SoftMaxLimit { get; }
        bool IfCheckDeviation { get; }
        int AllowedDeviation { get; }        
        void HomeBegin();
        void HomeEnd();
        void GoHome();
        void Move(int pulse, MoveType type);
        void MoveBegin(int pluses, MoveType type);
        void MoveEnd();

        /// <summary>
        /// 异步发送停止指令。硬件停止需要一定时间.可使用IfStopped 进行确认
        /// </summary>
        void StopAsync();

        bool IfStopped();

        /// <summary>
        /// 直到电机停止，然后返回
        /// </summary>
        void WaitforStopping();

        void SetRate(int pulse);
        /// <summary>
        /// 设置为默认的速度
        /// </summary>
        void SetToDefaultRate();
        void Setting(byte type, int value);
        /// <summary>
        /// 设置回默认值
        /// </summary>
        /// <param name="type"></param>
        void SettingToDefault(byte type);
        int Getting(byte type);
        void WriteTargetPosition(int value);
        void WriteActualPosition(int value);
        void WriteEncoderPosition(int value);
        int ReadActualPosition();
        int ReadTargetPosision();
        int ReadEncoderPosition();
        int CurrentPostion();
        void RotateRight(uint velocity);

        /// <summary>
        /// 设置轴电流
        /// </summary>
        /// <param name="current"></param>
        void Current(uint current);

        /// <summary>
        /// 当前电流
        /// </summary>
        /// <returns></returns>
        uint Current();

        void Sio(byte port, uint value);

        uint Gio(byte port);
        uint Gap(byte type, byte moto);
        int Gapio(int ioType);
    }
}
