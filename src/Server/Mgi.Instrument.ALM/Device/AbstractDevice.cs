using Castle.DynamicProxy;
using Mgi.Instrument.ALM.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mgi.Instrument.ALM.Device
{
    internal abstract class AbstractDevice : IALMDevice
    {
        private Dictionary<string, ALMTubeConfig> tubeConfigs;
        protected ProxyGenerator ProxyGenerator { get; } = new ProxyGenerator();

        protected IConfigProvider ConfigProvider { get; }
        public AbstractDevice(IConfigProvider configProvider)
        {
            ConfigProvider = configProvider;
            LoadTubeConfigs();
            ConfigProvider.ConfigChanged += ConfigProvider_ConfigChanged;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public abstract ALMDeviceType DeviceType { get; }
        public DeviceStatus Status { get; protected set; }
        public int InitializeOrder { get; set; } = 10;
        public abstract void Close();
        public abstract void Initialize();
        public ALMTubeConfig GetTubeConfig(string tubeId)
        {
            if (!tubeConfigs.ContainsKey(tubeId))
            {
                throw new Exception($"Tube with Id:{tubeId} not exists");
            }
            return tubeConfigs[tubeId];
        }

        protected virtual void OnConfigChanged(ConfigChangedEventArgs e)
        {

        }
        private void ConfigProvider_ConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            if (e.ConfigType == ConfigType.TubeConfig || e.ConfigType == ConfigType.All)
            {
                LoadTubeConfigs();
            }
            OnConfigChanged(e);
        }
        private void LoadTubeConfigs()
        {
            tubeConfigs = ConfigProvider.GetTubeConfigs().ToDictionary(x => x.Id);
        }
    }
}
