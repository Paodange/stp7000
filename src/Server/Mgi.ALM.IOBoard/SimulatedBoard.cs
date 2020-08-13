using System;
using static System.Threading.Thread;

namespace Mgi.ALM.IOBoard
{
    public class SimulatedBoard : IOnboardHardware
    {
        #region 继承变量
        public string Name
        {
            get
            {
                return @"SimulatedSensor";
            }
        }

        public bool Simulated
        {
            get
            {
                return true;
            }
        }
        public string Id
        {
            get;
            private set;
        }
        #endregion

        public SimulatedBoard()
        {
            Id = Guid.NewGuid().ToString();
        }

        public void Close()
        {
            Sleep(50);
            return;
        }

        public uint GpioIn(uint socket)
        {
            Sleep(50);
            return 1;
        }

        public void GpioOut(uint socket, uint value)
        {
            Sleep(50);
            return;
        }

        public void Initialize()
        {
            Sleep(50);
            return;
        }

        public void Open()
        {
            Sleep(50);
            return;
        }

        public void ReloadConfiguration()
        {
            Sleep(50);
            return;
        }

        public void SetBuzzerRate(int rate)
        {
            Sleep(50);
            return;
        }

        public void SetBuzzerTime(int time)
        {
            Sleep(50);
            return;
        }

        public void Color(byte r, byte g, byte b)
        {
            Sleep(50);
            return;
        }

        public void Reset() => Sleep(10);

        public void Stop()
        {
            Sleep(10);
        }

        public void SetBuzzerDuration(int duration)
        {
            
        }

        public void SetBuzzerBaud(int baud)
        {
            
        }

        public void HoodSpeed(int speed)
        {
            
        }

        public string GetVersion()
        {
            return "1.0.0";
        }

        public int GetVolume()
        {
            Random random = new Random();
            int volume = random.Next(1,50000);
            return volume;
        }

        public void ReOpen()
        {
            return;
        }

        public void ReInitialize()
        {
            return;
        }

        public void FlashColor(byte r, byte g, byte b, ushort onTime, ushort offTime)
        {
            return;
        }

        public void FlashBuzzer(ushort onTime, ushort offTime)
        {
            return;
        }
    }
}
