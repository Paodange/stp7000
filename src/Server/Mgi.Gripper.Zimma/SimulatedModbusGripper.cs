using System.Threading;

namespace Mgi.Gripper.Zimma
{
    public class SimulatedModbusGripper : IZimmaGripper
    {
        public string Name { get; set; }

        public void BeginRelease()
        {
            Thread.Sleep(10);
        }

        public void BeginTighten(byte gripForce, ushort teachPosition)
        {
            Thread.Sleep(10);
        }

        public void Close()
        {
            Thread.Sleep(10);
        }

        public int EndRelease()
        {
            return 0;
        }


        public int EndTighten(ushort teachPosition, byte positionTolerance, bool throwIfEmptyGrasp)
        {
            return teachPosition;
        }

        public void Initialize()
        {
            Thread.Sleep(10);
        }

        public int ReleaseGripper()
        {
            BeginRelease();
            return EndRelease();
        }

        public int TightenGripper(byte gripForce, ushort teachPosition, byte positionTolerance, bool throwIfEmtpyGrasp)
        {
            BeginTighten(gripForce, teachPosition);
            return EndTighten(teachPosition, positionTolerance, throwIfEmtpyGrasp);
        }
    }
}
