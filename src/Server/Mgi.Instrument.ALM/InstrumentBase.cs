using System;
using System.Collections.Generic;
using System.Linq;
using Mgi.Instrument.ALM.Device;

namespace Mgi.Instrument.ALM
{
    public abstract class InstrumentBase
    {
        public event EventHandler<MethodExceptionEventArgs> MethodExecuteError;
        public DeviceCollection Devices { get; }

        public IScriptEngine ScriptEngine { get; }

        public InstrumentBase(DeviceCollection devices, IScriptEngine scriptEngine)
        {
            Devices = devices;
            ScriptEngine = scriptEngine;
        }

        public abstract void PowerOff();

        public abstract void PowerOn();

        public virtual void RunScript(string scriptFilePath)
        {
            ScriptEngine.ExecuteByFile(scriptFilePath);
        }


        public IEnumerable<T> FindDevicesByType<T>(ALMDeviceType deviceType) where T : class, IALMDevice
        {
            return Devices.FindDevicesByType<T>(deviceType);
        }

        public T FindFirst<T>(ALMDeviceType deviceType, bool throwIfNotFound = true) where T : class, IALMDevice
        {
            return Devices.FindFirst<T>(deviceType, throwIfNotFound);
        }

        public T FindDeviceById<T>(string deviceId, bool throwIfNotFound = true) where T : class, IALMDevice
        {
            return Devices.FindDeviceById<T>(deviceId, throwIfNotFound);
        }

        public T FindDeviceByName<T>(string deviceName, bool throwIfNotFound = true) where T : class, IALMDevice
        {
            return Devices.FindDeviceByName<T>(deviceName, throwIfNotFound);
        }

        public IALMDevice FindDeviceByName(string deviceName, bool throwIfNotFound = true)
        {
            return Devices.FindDeviceByName(deviceName, throwIfNotFound);
        }

        public IEnumerable<IALMDevice> FindDevicesByType(ALMDeviceType dxDeviceType)
        {
            return Devices.FindDevicesByType(dxDeviceType);
        }

        public IALMDevice FindFirst(ALMDeviceType deviceType, bool throwIfNotFound = true)
        {
            return Devices.FindFirst(deviceType, throwIfNotFound);
        }

        public IALMDevice FindDeviceById(string deviceId, bool throwIfNotFound = true)
        {
            return Devices.FindDeviceById(deviceId, throwIfNotFound);
        }

        protected virtual void OnMethodException(object sender, MethodExceptionEventArgs e)
        {
            MethodExecuteError?.Invoke(sender, e);
        }
    }
    public class DeviceCollection : List<IALMDevice>
    {
        public IEnumerable<T> FindDevicesByType<T>(ALMDeviceType deviceType) where T : class, IALMDevice
        {
            return this.Where(x => x.DeviceType == deviceType).Select(x => (T)x);
        }

        public T FindFirst<T>(ALMDeviceType deviceType, bool throwIfNotFound = true) where T : class, IALMDevice
        {
            var device = this.FirstOrDefault(x => x.DeviceType == deviceType);
            if (device != null)
            {
                if (device is T t)
                {
                    return t;
                }
                throw new Exception($"Device,id:{device.Id},name:{device.Name} is not {typeof(T)}, actually {device.GetType()}");
            }
            if (throwIfNotFound)
            {
                throw new Exception($"Device with Type:[{deviceType.ToString()}] is not exists");
            }
            else
            {
                return null;
            }
        }

        public T FindDeviceById<T>(string deviceId, bool throwIfNotFound = true) where T : class, IALMDevice
        {
            var device = this.FirstOrDefault(x => x.Id == deviceId);
            if (device != null)
            {
                if (device is T t)
                {
                    return t;
                }
                throw new Exception($"Device,id:{deviceId},name:{device.Name} is not {typeof(T)}, actually {device.GetType()}");
            }
            if (throwIfNotFound)
            {
                throw new Exception($"Device with id:[{deviceId}] is not exists");
            }
            else
            {
                return null;
            }
        }

        public T FindDeviceByName<T>(string deviceName, bool throwIfNotFound = true) where T : class, IALMDevice
        {
            var device = this.FirstOrDefault(x => x.Name == deviceName);
            if (device != null)
            {
                if (device is T t)
                {
                    return t;
                }
                throw new Exception($"Device,id:{device.Id},name:{device.Name} is not {typeof(T)}, actually {device.GetType()}");
            }
            if (throwIfNotFound)
            {
                throw new Exception($"Device with name:[{deviceName}] is not exists");
            }
            else
            {
                return null;
            }
        }

        public IALMDevice FindDeviceByName(string deviceName, bool throwIfNotFound = true)
        {
            var device = this.FirstOrDefault(x => x.Name == deviceName);
            if (device != null)
            {
                return device;
            }
            if (throwIfNotFound)
            {
                throw new Exception($"Device with name:[{deviceName}] is not exists");
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<IALMDevice> FindDevicesByType(ALMDeviceType dxDeviceType)
        {
            return this.Where(x => x.DeviceType == dxDeviceType);
        }

        public IALMDevice FindFirst(ALMDeviceType deviceType, bool throwIfNotFound = true)
        {
            var device = this.FirstOrDefault(x => x.DeviceType == deviceType);
            if (device != null)
            {
                return device;
            }
            if (throwIfNotFound)
            {
                throw new Exception($"Device with Type:[{deviceType.ToString()}] is not exists");
            }
            else
            {
                return null;
            }
        }

        public IALMDevice FindDeviceById(string deviceId, bool throwIfNotFound = true)
        {
            var device = this.FirstOrDefault(x => x.Id == deviceId);
            if (device != null)
            {
                return device;
            }
            if (throwIfNotFound)
            {
                throw new Exception($"Device with id:[{deviceId}] is not exists");
            }
            else
            {
                return null;
            }
        }
    }
}
