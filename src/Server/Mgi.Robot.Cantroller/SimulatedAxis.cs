using System;

using Mgi.Robot.Cantroller.Axis;
using Mgi.Robot.Cantroller.Can;

namespace Mgi.Robot.Cantroller
{
    public class SimulatedAxis : ISp200Axis
    {
        static Random _random = new Random(1024);
        private readonly ICanController _can;
        private readonly AxisConfig _config;
        //private readonly ILog _logger;
        private int currentPosition = 0;
        public string Name => $"Simulated_{_config.Name}";

        public byte No => 127;

        public int SoftMinLimit => _config.SoftLimitMin;

        public int SoftMaxLimit => _config.SoftLimitMax;

        public bool IfCheckDeviation => _config.IfCheckDeviation;

        public int AllowedDeviation => _config.AllowedDeviation;

        private void LogMsg(string txt) => Console.WriteLine("12321");//_logger.DebugFormat("Simulated {0}: {1}", Name, txt);

        public static int Next => _random.Next(1000);

        public SimulatedAxis(AxisConfig config, ICanController can)
        {
            _config = config;
            _can = can;
        }

        public void HomeBegin()
        {
            LogMsg("Home begin");
        }

        public void HomeEnd()
        {
            LogMsg("Home end");
            currentPosition = 0;
        }

        public void Move(int pulse, MoveType type)
        {
            LogMsg($"Move {pulse},{type}");
            if (type == MoveType.REL)
            {
                currentPosition += pulse;
            }
            else
            {
                currentPosition = pulse;
            }
        }

        public void MoveBegin(int pulse, MoveType type)
        {
            LogMsg($"MoveBegin {pulse},{type}");
            if (type == MoveType.REL)
            {
                currentPosition += pulse;
            }
            else
            {
                currentPosition = pulse;
            }
        }

        public int ReadActualPosition() => currentPosition;

        public int ReadEncoderPosition() => currentPosition;

        public int ReadTargetPosision() => currentPosition;

        public void SetRate(int pulse) => LogMsg($"Set rate {pulse}");
        public void SetToDefaultRate()
        {
            LogMsg($"Set rate default");
        }

        public void SettingToDefault(byte type)
        {
            LogMsg($"Setting:{type}=default");
        }
        public void Setting(byte type, int value) => LogMsg($"Setting:{type}={value}");

        public void StopAsync() => LogMsg("Stop");

        public void WriteActualPosition(int value) => LogMsg($"W a p {value}");

        public void WriteEncoderPosition(int value) => LogMsg($"W e p {value}");

        public void WriteTargetPosition(int value) => LogMsg($"W t p {value}");

        public int Getting(byte type) => 1;

        public void MoveEnd() => LogMsg("Move end");

        public int CurrentPostion() => currentPosition;

        public void RotateRight(uint velocity) => LogMsg($"Rotate right {velocity}");

        public void Current(uint current) => LogMsg($"Current {current}");

        public uint Current() => (uint)currentPosition;

        public void GoHome()
        {
            LogMsg("Go home");
            currentPosition = 0;
        }

        public void UseConfigSetting() => LogMsg("UseConfigSetting");

        public void Sio(byte port, uint value)
        {
            LogMsg($"Sio: p={port}, v={value}");
        }

        public bool IfStopped() => true;

        public void WaitforStopping()
        {
            // nothing in simulation model
        }

        public uint Gio(byte port)
        {
            return 2;
        }

        public int Gapio(int ioType)
        {
            return 1;
        }

        public uint Gap(byte type, byte moto)
        {
            return 1;

        }
    }
}
