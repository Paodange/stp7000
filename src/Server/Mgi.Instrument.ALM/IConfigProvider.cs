using Mgi.ALM.ZLims;
using Mgi.Epson.Robot.T3;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mgi.Instrument.ALM
{
    public interface IConfigProvider
    {
        ModbusGripperConfig GetGripperConfig();
        ALMLidUnCoverConfig GetLidUnCoverConfig();
        ALMPipettesConfig GetPipettesConfig();
        EpsonRobotConfig GetRobotConfig();
        List<LocationMap> GetRobotLocations();
        List<ALMTubeConfig> GetTubeConfigs();
        ALMIOBoardConfig GetIOBoardConfig();
        MachineConfig GetMachineConfig();
        LastRunInfo GetLastRunInfo();
        void SaveGripperConfig();
        void SaveLidUnCoverConfig();
        void SavePipettesConfig();
        void SaveRobotConfig();
        void SaveRobotLocations();
        void SaveTubeConfigs();

        void ReloadAll();

        void SaveLastRunInfo();
        event ConfigChangedEventHandler ConfigChanged;

        ZLimsConfig GetZLimsConfig();
    }

    internal class JsonFileConfigProvider : IConfigProvider
    {

        static readonly string CONFIG_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        static readonly string GRIPPER_CONFIG_FILE = Path.Combine(CONFIG_DIR, "Gripper.json");
        static readonly string LIDUNCOVER_CONFIG_FILE = Path.Combine(CONFIG_DIR, "LidUnCover.json");
        static readonly string PIPETTES_CONFIG_FILE = Path.Combine(CONFIG_DIR, "Pipettes.json");
        static readonly string ROBOT_CONFIG_FILE = Path.Combine(CONFIG_DIR, "EpsonRobot.json");
        static readonly string ROBOT_LOCATION_FILE = Path.Combine(CONFIG_DIR, "Locations.json");
        static readonly string TUBE_CONFIG_FILE = Path.Combine(CONFIG_DIR, "Tubes.json");
        static readonly string IOBoard_CONFIG_FILE = Path.Combine(CONFIG_DIR, "IOBoard.json");
        static readonly string MACHINE_CONFIG_FILE = Path.Combine(CONFIG_DIR, "Global.json");
        static readonly string LAST_RUN_INFO_FILE = Path.Combine(CONFIG_DIR, "LastRunInfo.json");
        static readonly string ZLIMS_CONFIG_FILE = Path.Combine(CONFIG_DIR, "ZLims.json");
        private static readonly IDictionary<string, object> configs = new Dictionary<string, object>();
        private static readonly Encoding encoding = Encoding.UTF8;
        public JsonFileConfigProvider()
        {
            if (!Directory.Exists(CONFIG_DIR))
            {
                Directory.CreateDirectory(CONFIG_DIR);
            }
        }

        public event ConfigChangedEventHandler ConfigChanged;

        public ModbusGripperConfig GetGripperConfig()
        {
            return GetConfig<ModbusGripperConfig>(GRIPPER_CONFIG_FILE, false);
        }

        public ALMLidUnCoverConfig GetLidUnCoverConfig()
        {
            return GetConfig<ALMLidUnCoverConfig>(LIDUNCOVER_CONFIG_FILE, false);
        }

        public ALMPipettesConfig GetPipettesConfig()
        {
            return GetConfig<ALMPipettesConfig>(PIPETTES_CONFIG_FILE, false);
        }

        public EpsonRobotConfig GetRobotConfig()
        {
            return GetConfig<EpsonRobotConfig>(ROBOT_CONFIG_FILE, false);
        }

        public List<LocationMap> GetRobotLocations()
        {
            return GetConfig<List<LocationMap>>(ROBOT_LOCATION_FILE, false);
        }

        public List<ALMTubeConfig> GetTubeConfigs()
        {
            return GetConfig<List<ALMTubeConfig>>(TUBE_CONFIG_FILE, false);
        }
        public ALMIOBoardConfig GetIOBoardConfig()
        {
            return GetConfig<ALMIOBoardConfig>(IOBoard_CONFIG_FILE, false);
        }
        public MachineConfig GetMachineConfig()
        {
            return GetConfig<MachineConfig>(MACHINE_CONFIG_FILE, false);
        }

        public LastRunInfo GetLastRunInfo()
        {
            return GetConfig<LastRunInfo>(LAST_RUN_INFO_FILE, false);
        }

        public ZLimsConfig GetZLimsConfig()
        {
            return GetConfig<ZLimsConfig>(ZLIMS_CONFIG_FILE, false);
        }
        public void SaveGripperConfig()
        {
            if (configs.ContainsKey(GRIPPER_CONFIG_FILE))
            {
                WriteToFile(GRIPPER_CONFIG_FILE, configs[GRIPPER_CONFIG_FILE]);
                OnConfigChanged(new ConfigChangedEventArgs(ConfigType.RobotGripper));
            }
        }

        public void SaveLidUnCoverConfig()
        {
            if (configs.ContainsKey(LIDUNCOVER_CONFIG_FILE))
            {
                WriteToFile(LIDUNCOVER_CONFIG_FILE, configs[LIDUNCOVER_CONFIG_FILE]);
                OnConfigChanged(new ConfigChangedEventArgs(ConfigType.LiUncover));
            }
        }

        public void SavePipettesConfig()
        {
            if (configs.ContainsKey(PIPETTES_CONFIG_FILE))
            {
                WriteToFile(PIPETTES_CONFIG_FILE, configs[PIPETTES_CONFIG_FILE]);
                OnConfigChanged(new ConfigChangedEventArgs(ConfigType.Pipette));
            }
        }

        public void SaveRobotConfig()
        {
            if (configs.ContainsKey(ROBOT_CONFIG_FILE))
            {
                WriteToFile(ROBOT_CONFIG_FILE, configs[ROBOT_CONFIG_FILE]);
                OnConfigChanged(new ConfigChangedEventArgs(ConfigType.RobotConfig));
            }
        }

        public void SaveRobotLocations()
        {
            if (configs.ContainsKey(ROBOT_LOCATION_FILE))
            {
                WriteToFile(ROBOT_LOCATION_FILE, configs[ROBOT_LOCATION_FILE]);
                OnConfigChanged(new ConfigChangedEventArgs(ConfigType.RobotLocation));
            }
        }

        public void SaveTubeConfigs()
        {
            if (configs.ContainsKey(TUBE_CONFIG_FILE))
            {
                WriteToFile(TUBE_CONFIG_FILE, configs[TUBE_CONFIG_FILE]);
                OnConfigChanged(new ConfigChangedEventArgs(ConfigType.TubeConfig));
            }
        }
        public void SaveLastRunInfo()
        {
            if (configs.ContainsKey(LAST_RUN_INFO_FILE))
            {
                WriteToFile(LAST_RUN_INFO_FILE, configs[LAST_RUN_INFO_FILE]);
            }
        }
        private T GetConfig<T>(string file, bool reload) where T : class, new()
        {
            lock (configs)
            {
                if (!reload && configs.ContainsKey(file))
                {
                    return configs[file] as T;
                }
                var c = ReadJsonAs<T>(file);
                configs[file] = c;
                return c;
            }
        }

        private T ReadJsonAs<T>(string file) where T : class, new()
        {
            if (!File.Exists(file))
            {
                var t = new T();
                WriteToFile(file, t);
                return t;
            }
            var json = File.ReadAllText(file);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        private void WriteToFile<T>(string file, T data) where T : class, new()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(file, json, encoding);
        }

        private void OnConfigChanged(ConfigChangedEventArgs e)
        {
            ConfigChanged?.Invoke(this, e);
        }

        public void ReloadAll()
        {
            configs.Clear();
            OnConfigChanged(new ConfigChangedEventArgs(ConfigType.All));
        }


    }

    public delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs e);

    public class ConfigChangedEventArgs : EventArgs
    {
        public ConfigType ConfigType { get; }
        public ConfigChangedEventArgs(ConfigType t)
        {
            ConfigType = t;
        }
    }

    public enum ConfigType
    {
        All,
        LiUncover,
        Pipette,
        RobotLocation,
        TubeConfig,
        RobotConfig,
        RobotGripper
    }
}
