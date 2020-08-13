using System;
using Mgi.Instrument.ALM.Attr;

namespace Mgi.Instrument.ALM.Device
{
    public interface IALMIOBoard : IALMDevice
    {
        /// <summary>
        /// 查询版本
        /// </summary>
        /// <returns></returns>
        string GetVersion();
        int GetVolume();
        DoorState GetFrontDoorState();
        void OpenBuzzer();
        void CloseBuzzer();
        void OpenFloodLight();
        void CloseFloodLight();
        void CloseSterilamp();
        void OpenSafetyLock();
        void Lock();
        void OpenLaminarHood();
        void CloseLaminarHood();
        void OpenSterilamp();
        void HoodSpeed(int speed);
        void SetLedColor(LedColor color);
        void Warn();
        void Error();
        void Run();
        void Idle();

        bool IfLinghtOn { get; }
        bool IsLaminarHoodOn { get; }
        bool IsDoorLocked { get; }
        bool IsSterilampOn { get; }
        bool IsBuzzerOn { get; }
        event EventHandler<DoorStateChangedEventArgs> DoorStateChanged;
    }

    public enum LedColor
    {
        Green = 1,
        Yellow,
        Red
    }
    public class DoorStateChangedEventArgs
    {
        public DoorState State { get; }
        public DoorStateChangedEventArgs(DoorState state)
        {
            State = state;
        }
    }

    public enum DoorState
    {
        Unknow = 1,
        Open,
        Closed
    }
}
