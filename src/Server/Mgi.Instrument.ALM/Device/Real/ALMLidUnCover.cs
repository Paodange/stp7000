using log4net;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Device.Real
{
    internal class ALMLidUnCover : AbstractLidUncover, IALMLidUnCover
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMLidUnCover");
        public ALMLidUnCover(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log, false)
        {

        }
        protected override void InitialComponents()
        {
            var config = ConfigProvider.GetLidUnCoverConfig();
            WorkUnits.Add(LidUncoverUnits.A, new LidUncoverUnit()
            {
                LidUncover = LidUncoverUnits.A,
                Z = Axises["DZ1"],
                R1 = Axises["R1"],
                R2 = Axises["R2"],
                C = Axises["C1"],
                T = Axises["T1"],
                E = Axises["E1"],
                BarcodeA = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateBarcodeInstance(config.BarcodeConfigs["A1"]),
                    new DeviceCommandInterceptor("Barcode(A1)", WorkflowManager, log)),
                BarcodeB = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateBarcodeInstance(config.BarcodeConfigs["A2"]),
                    new DeviceCommandInterceptor("Barcode(A2)", WorkflowManager, log)),
                GripperA = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateGripperInstance("GripperA1", config.GripperConfigs["A1"]),
                    new DeviceCommandInterceptor("Gripper(A1)", WorkflowManager, log)),
                GripperB = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateGripperInstance("GripperA2", config.GripperConfigs["A2"]),
                    new DeviceCommandInterceptor("Gripper(A2)", WorkflowManager, log))
            });

            WorkUnits.Add(LidUncoverUnits.B, new LidUncoverUnit()
            {
                LidUncover = LidUncoverUnits.B,
                Z = Axises["DZ2"],
                R1 = Axises["R3"],
                R2 = Axises["R4"],
                C = Axises["C2"],
                T = Axises["T2"],
                E = Axises["E2"],
                BarcodeA = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateBarcodeInstance(config.BarcodeConfigs["B1"]),
                    new DeviceCommandInterceptor("Barcode(B1)", WorkflowManager, log)),
                BarcodeB = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateBarcodeInstance(config.BarcodeConfigs["B2"]),
                    new DeviceCommandInterceptor("Barcode(B2)", WorkflowManager, log)),
                GripperA = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateGripperInstance("Gripper(B1)", config.GripperConfigs["B1"]),
                    new DeviceCommandInterceptor("Gripper(B1)", WorkflowManager, log)),
                GripperB = ProxyGenerator.CreateInterfaceProxyWithTarget(
                    CreateGripperInstance("Gripper(B2)", config.GripperConfigs["B2"]),
                    new DeviceCommandInterceptor("Gripper(B2)", WorkflowManager, log))
            });
        }
    }
}