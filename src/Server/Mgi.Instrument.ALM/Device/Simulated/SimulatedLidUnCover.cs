using log4net;
using Mgi.Barcode.Leuze;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Device.Simulated
{
    internal class SimulatedLidUnCover : AbstractLidUncover, IALMLidUnCover
    {
        static readonly ILog log = Log4Manager.GetLogger("ALMLidUnCover");

        public SimulatedLidUnCover(IConfigProvider configProvider, IWorkflowManager workflowManager)
            : base(configProvider, workflowManager, log, true)
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
                BarcodeA = ProxyGenerator.CreateInterfaceProxyWithTarget<ILeuzeBarcode>(
                              new SimulatedLeuzeBarcode(),
                              new DeviceCommandInterceptor($"A_BarcodeA", WorkflowManager, log)),
                BarcodeB = ProxyGenerator.CreateInterfaceProxyWithTarget<ILeuzeBarcode>(
                              new SimulatedLeuzeBarcode(),
                              new DeviceCommandInterceptor("A_BarcodeB", WorkflowManager, log)),
                GripperA = ProxyGenerator.CreateInterfaceProxyWithTarget<IZimmaGripper>(
                              new SimulatedModbusGripper(),
                              new DeviceCommandInterceptor("A_GripperA", WorkflowManager, log)),
                GripperB = ProxyGenerator.CreateInterfaceProxyWithTarget<IZimmaGripper>(
                              new SimulatedModbusGripper(),
                              new DeviceCommandInterceptor("A_GripperB", WorkflowManager, log))
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
                BarcodeA = ProxyGenerator.CreateInterfaceProxyWithTarget<ILeuzeBarcode>(
                    new SimulatedLeuzeBarcode(),
                    new DeviceCommandInterceptor("B_BarcodeA", WorkflowManager, log)),
                BarcodeB = ProxyGenerator.CreateInterfaceProxyWithTarget<ILeuzeBarcode>(
                    new SimulatedLeuzeBarcode(),
                    new DeviceCommandInterceptor("B_BarcodeB", WorkflowManager, log)),
                GripperA = ProxyGenerator.CreateInterfaceProxyWithTarget<IZimmaGripper>(
                    new SimulatedModbusGripper(),
                    new DeviceCommandInterceptor("B_GripperA", WorkflowManager, log)),
                GripperB = ProxyGenerator.CreateInterfaceProxyWithTarget<IZimmaGripper>(
                    new SimulatedModbusGripper(),
                    new DeviceCommandInterceptor("B_GripperB", WorkflowManager, log))
            });
        }

        public override void AssertSamplePosState(LidUncoverUnits lidUncover, SliderSamplePos pos, SliderSamplePosState state)
        {

        }

        public override bool AssertSamplePosStateBool(LidUncoverUnits lidUncover, SliderSamplePos pos, SliderSamplePosState state)
        {
            return true;
        }
    }
}
