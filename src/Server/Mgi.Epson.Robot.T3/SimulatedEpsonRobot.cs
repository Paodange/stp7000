using System;
using System.Collections.Generic;
using System.Threading;

namespace Mgi.Epson.Robot.T3
{
    public class SimulatedEpsonRobot : IEpsonRobot
    {
        public void Open()
        {
            Thread.Sleep(20);
            return;
        }

        public void ReOpen()
        {
            Open();
        }
        public void Close()
        {
            Thread.Sleep(20);
            return;
        }

        public void Initialize()
        {
            Thread.Sleep(20);
            return;
        }
        public void ReInitialize()
        {
            Initialize();
        }

        public void Stop()
        {
            Thread.Sleep(20);
            return;
        }

        public void MotorOff()
        {
            Thread.Sleep(20);
            return;
        }

        public void MotorOn()
        {
            Thread.Sleep(20);
            return;
        }

        public void PowerHigh()
        {
            Thread.Sleep(20);
            return;
        }

        public void PowerLow()
        {
            Thread.Sleep(20);
            return;
        }

        public void Reset()
        {
            Thread.Sleep(20);
            return;
        }

        public void Grasp(int graspType)
        {
            Thread.Sleep(20);
            return;
        }

        public void Loosen()
        {
            Thread.Sleep(20);
            return;
        }

        public void Go(int pointNum, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0)
        {
            Thread.Sleep(20);
            return;
        }

        public void Jump(int pointNum, double xOffset = 0, double yOffset = 0, double zOffset = 0)
        {
            Thread.Sleep(20);
            return;
        }

        public void SetSpeed(int maxSpeedPercent)
        {
            Thread.Sleep(20);
            return;
        }

        public void SetAccel(int maxAccelPercent, int maxDecelPercent)
        {
            Thread.Sleep(20);
            return;
        }

        public void Pause()
        {
            Thread.Sleep(20);
            return;
        }

        public void Resume()
        {
            Thread.Sleep(20);
            return;
        }

        //public void GripperSetup()
        //{
        //    Thread.Sleep(20);
        //    return;
        //}

        public void GripperDrive(ushort driveSet)
        {
            Thread.Sleep(20);
            return;
        }

        public void BGo(double x, double y, double z, double u)
        {
            Thread.Sleep(20);
            return;
        }

        public void ResetCmd()
        {
            Thread.Sleep(20);
            return;
        }

        public Dictionary<string, double> Current()
        {
            Random random = new Random();
            Dictionary<string, double> pos = new Dictionary<string, double> { { "X", random.Next(0, 1000) }, { "Y", random.Next(0, 1000) }, { "Z", random.Next(0, 1000) },
                    { "U", random.Next(0, 1000) }, { "V", random.Next(0, 1000) }, { "W", random.Next(0, 1000) } };
            return pos;
        }

        public Dictionary<string, double> GetPos(int pointNum)
        {
            Random random = new Random();
            Dictionary<string, double> pos = new Dictionary<string, double> { { "X", random.Next(0, 1000) }, { "Y", random.Next(0, 1000) }, { "Z", random.Next(0, 1000) },
                    { "U", random.Next(0, 1000) }, { "V", random.Next(0, 1000) }, { "W", random.Next(0, 1000) } };
            return pos;
        }


        public Dictionary<string, double> GetPalletPos(int palletNo, int colrowNo)
        {
            Random random = new Random();
            Dictionary<string, double> pos = new Dictionary<string, double> { { "X", random.Next(0, 1000) }, { "Y", random.Next(0, 1000) }, { "Z", random.Next(0, 1000) },
                    { "U", random.Next(0, 1000) }, { "V", random.Next(0, 1000) }, { "W", random.Next(0, 1000) } };
            return pos;
        }

        public int GetIO(int io)
        {
            return 1;
        }

        public void Move(int pointNum, double xOffset = 0, double yOffset = 0, double zOffset = 0.0)
        {
            Thread.Sleep(20);
            return;
        }

        public void LimitTorque(int percent)
        {
            return;
        }

        public void MoveSpeed(int speed)
        {
            return;
        }

        public void MoveAccel(int accel)
        {
            return;
        }

        public void GoPalletNo(int palletNo, int row, int column, double xOffset = 0, double yOffset = 0, double zOffset = 0)
        {

        }

        public void JumpPalletNo(int palletNo, int row, int column, double xOffset = 0, double yOffset = 0, double zOffset = 0)
        {
     
        }
    }
}
