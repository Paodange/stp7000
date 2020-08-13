namespace Mgi.Epson.Robot.T3
{
    public class EpsonRobotConfig
    {
        public string EpsonIP { get; set; }

        public int EpsonPort { get; set; }

        public int EthernetPort { get; set; }

        public int RcvTimeOut { get; set; }

        public int Speed { get; set; }
        public int Accel { get; set; } = 80;

        /// <summary>
        /// 极限转矩
        /// </summary>
        public int LimitTorque { get; set; }

        public bool ServiceSimulated { get; set; }
    }
}
