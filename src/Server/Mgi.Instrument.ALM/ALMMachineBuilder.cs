using Castle.DynamicProxy;
using Mgi.ALM.ZLims;
using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Device.Real;
using Mgi.Instrument.ALM.Device.Simulated;
using Mgi.Instrument.ALM.Services;
using Mgi.Instrument.ALM.Util;
using System;
using System.Collections.Generic;
using Unity;

namespace Mgi.Instrument.ALM
{
    public class ALMMachineBuilder
    {
        private IUnityContainer Container { get; }
        private IWorkflowManager WorkflowManager { get; set; } = new WorkflowManager();
        private IList<IBackgroundService> BackgroundServices { get; }
            = new List<IBackgroundService>();
        private DeviceCollection Devices { get; }
            = new DeviceCollection();
        readonly ProxyGenerator proxyGenerator;
        public ALMMachineBuilder() : this(new UnityContainer())
        {
        }
        public ALMMachineBuilder(IUnityContainer container)
        {
            Container = container;
            proxyGenerator = new ProxyGenerator();
            container.RegisterInstance<IProxyGenerator>(proxyGenerator);
            container.RegisterInstance(Devices);
            container.RegisterInstance(BackgroundServices);
            container.RegisterSingleton<IAutoLidMachine, AutoLidMachine>();
        }
        private IConfigProvider configProvider;
        public IConfigProvider ConfigProvider
        {
            get
            {
                if (configProvider == null)
                {
                    configProvider = proxyGenerator.CreateInterfaceProxyWithTarget<IConfigProvider>(
                        new JsonFileConfigProvider(), new GeneralInterceptor(Log4Manager.GetLogger("ConfigProvider")));
                }
                Container.RegisterInstance(configProvider);
                return configProvider;
            }
        }
        public ALMMachineBuilder UseConfigProvider<T>(T provider) where T : IConfigProvider
        {
            configProvider = proxyGenerator.CreateInterfaceProxyWithTarget<IConfigProvider>(
                        provider, new GeneralInterceptor(Log4Manager.GetLogger("ConfigProvider")));
            Container.RegisterInstance(configProvider);
            return this;
        }

        public ALMMachineBuilder UseWorkflowManager<T>(T workflowManager) where T : IWorkflowManager
        {
            WorkflowManager = workflowManager;
            Container.RegisterInstance<IWorkflowManager>(workflowManager);
            return this;
        }
        public ALMMachineBuilder UseScriptEngine()
        {
            var scriptEngine = proxyGenerator.CreateInterfaceProxyWithTarget<IScriptEngine>(
                         new ALMScriptEngine(),
                         new GeneralInterceptor(Log4Manager.GetLogger("ScriptEngine")));
            Container.RegisterInstance(scriptEngine);
            return this;
        }
        public ALMMachineBuilder UseLidUnCover(bool simulated)
        {
            var logger = Log4Manager.GetLogger("ALMLidUnCover");
            IALMLidUnCover lidUnCover = null;
            if (simulated)
            {
                lidUnCover = proxyGenerator.CreateInterfaceProxyWithTarget<IALMLidUnCover>(
                          new SimulatedLidUnCover(ConfigProvider, WorkflowManager),
                          new DeviceCommandInterceptor("SimulatedLidUnCover", WorkflowManager, logger));
            }
            else
            {
                lidUnCover = proxyGenerator.CreateInterfaceProxyWithTarget<IALMLidUnCover>(
                     new ALMLidUnCover(ConfigProvider, WorkflowManager),
                     new DeviceCommandInterceptor("ALMLidUnCover", WorkflowManager, logger));
            }
            Devices.Add(lidUnCover);
            Container.RegisterInstance(lidUnCover);
            return this;
        }
        public ALMMachineBuilder UseRobot(bool simulated)
        {
            var logger = Log4Manager.GetLogger("ALMRobot");
            IALMRobot robot = null;
            if (simulated)
            {
                robot = proxyGenerator.CreateInterfaceProxyWithTarget<IALMRobot>(
                     new SimulatedALMRobot(ConfigProvider, WorkflowManager),
                     new DeviceCommandInterceptor("SimulatedALMRobot", WorkflowManager, logger));
            }
            else
            {
                robot = proxyGenerator.CreateInterfaceProxyWithTarget<IALMRobot>(
                    new ALMRobot(ConfigProvider, WorkflowManager),
                    new DeviceCommandInterceptor("ALMRobot", WorkflowManager, logger));
            }
            Devices.Add(robot);
            Container.RegisterInstance(robot);
            return this;
        }
        public ALMMachineBuilder UsePipettes(bool simulated)
        {
            var logger = Log4Manager.GetLogger("ALMPipettes");
            IALMPipettes pipettes = null;
            if (simulated)
            {
                pipettes = proxyGenerator.CreateInterfaceProxyWithTarget<IALMPipettes>(
                    new SimulatedALMPipettes(ConfigProvider, WorkflowManager),
                    new DeviceCommandInterceptor("SimulatedALMPipettes", WorkflowManager, logger));
            }
            else
            {
                pipettes = proxyGenerator.CreateInterfaceProxyWithTarget<IALMPipettes>(
                    new ALMPipettes(ConfigProvider, WorkflowManager),
                    new DeviceCommandInterceptor("ALMPipettes", WorkflowManager, logger));
            }
            Devices.Add(pipettes);
            Container.RegisterInstance(pipettes);
            return this;
        }
        public ALMMachineBuilder UseIOBoard(bool simulated)
        {
            var logger = Log4Manager.GetLogger("IOBoard");
            IALMIOBoard ioBoard = null;
            if (simulated)
            {
                ioBoard = proxyGenerator.CreateInterfaceProxyWithTarget<IALMIOBoard>(
                    new SimulatedIOBoard(ConfigProvider, WorkflowManager),
                    new DeviceCommandInterceptor("SimulatedIOBoard", WorkflowManager, logger));
            }
            else
            {
                ioBoard = proxyGenerator.CreateInterfaceProxyWithTarget<IALMIOBoard>(
                    new ALMIOBoard(ConfigProvider, WorkflowManager),
                    new DeviceCommandInterceptor("IOBoard", WorkflowManager, logger));
            }
            Devices.Add(ioBoard);
            Container.RegisterInstance(ioBoard);
            return this;
        }
        public ALMMachineBuilder UseRestAPI(int port)
        {
            var apiService = new RestAPIService(Container, port);
            Container.RegisterInstance(apiService);
            BackgroundServices.Add(apiService);
            return this;
        }
        public ALMMachineBuilder UseZLims(bool simulated)
        {
            var config = ConfigProvider.GetZLimsConfig();
            IZLimsMessageService zlimsService;
            if (simulated)
            {
                zlimsService = new SimulatedZLimsService();
            }
            else
            {
                zlimsService = new AlmZLimsMessageService(config);
            }
            Container.RegisterInstance(zlimsService);
            BackgroundServices.Add(zlimsService);
            return this;
        }
        public IAutoLidMachine Build()
        {
            return Container.Resolve<IAutoLidMachine>();
        }
    }
}
