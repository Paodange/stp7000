using log4net;
using Mgi.Barcode.Leuze;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Config;
using Mgi.Instrument.ALM.Util;
using Mgi.Robot.Cantroller;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Mgi.Instrument.ALM.Device
{
    internal abstract class AbstractLidUncover : CanBasedDevice, IALMLidUnCover
    {
        List<SliderPosition> SliderPositions { get; set; }
        public Dictionary<LidUncoverUnits, LidUncoverUnit> WorkUnits { get; } = new Dictionary<LidUncoverUnits, LidUncoverUnit>();
        ALMLidUnCoverConfig lidUnCoverConfig;

        public double CReleaseOffset { get; private set; }
        public double CTightenOffset { get; private set; }
        public AbstractLidUncover(IConfigProvider configProvider, IWorkflowManager workflowManager, ILog log, bool simulated)
            : base(simulated, configProvider.GetLidUnCoverConfig().RobotConfigs, configProvider, log, workflowManager)
        {
            InitializeOrder = 20;
            var config = ConfigProvider.GetLidUnCoverConfig();
            lidUnCoverConfig = config;
            CReleaseOffset = config.CReleaseOffset;
            CTightenOffset = config.CTightenOffset;
            SliderPositions = config.SliderPositions;
            InitialComponents();
        }

        protected abstract void InitialComponents();

        public override ALMDeviceType DeviceType => ALMDeviceType.LidUnCover;
        protected override void CheckConfig()
        {
            Contract.Assert(Axises != null);
        }

        protected override void OnConfigChanged(ConfigChangedEventArgs e)
        {
            if (e.ConfigType == ConfigType.All || e.ConfigType == ConfigType.LiUncover)
            {
                var config = ConfigProvider.GetLidUnCoverConfig();
                lidUnCoverConfig = config;
                CReleaseOffset = config.CReleaseOffset;
                CTightenOffset = config.CTightenOffset;
                SliderPositions = config.SliderPositions;
            }
        }
        public override void Initialize()
        {
            WorkUnits[LidUncoverUnits.A].BarcodeA.Open();
            WorkUnits[LidUncoverUnits.A].BarcodeB.Open();
            WorkUnits[LidUncoverUnits.B].BarcodeA.Open();
            WorkUnits[LidUncoverUnits.B].BarcodeB.Open();

            WorkUnits[LidUncoverUnits.A].GripperA.Initialize();
            WorkUnits[LidUncoverUnits.A].GripperB.Initialize();
            WorkUnits[LidUncoverUnits.B].GripperA.Initialize();
            WorkUnits[LidUncoverUnits.B].GripperB.Initialize();
            base.Initialize();
        }
        public override void HomeAll()
        {
            WorkUnits[LidUncoverUnits.A].GripperA.BeginRelease();
            WorkUnits[LidUncoverUnits.A].GripperB.BeginRelease();
            WorkUnits[LidUncoverUnits.B].GripperA.BeginRelease();
            WorkUnits[LidUncoverUnits.B].GripperB.BeginRelease();
            WorkUnits[LidUncoverUnits.A].GripperA.EndRelease();
            WorkUnits[LidUncoverUnits.A].GripperB.EndRelease();
            WorkUnits[LidUncoverUnits.B].GripperA.EndRelease();
            WorkUnits[LidUncoverUnits.B].GripperB.EndRelease();

            WorkUnits[LidUncoverUnits.A].Z.HomeBegin();
            WorkUnits[LidUncoverUnits.B].Z.HomeBegin();
            WorkUnits[LidUncoverUnits.A].Z.HomeEnd();
            WorkUnits[LidUncoverUnits.B].Z.HomeEnd();

            WorkUnits[LidUncoverUnits.A].R1.HomeBegin();
            WorkUnits[LidUncoverUnits.B].R1.HomeBegin();
            WorkUnits[LidUncoverUnits.A].R2.HomeBegin();
            WorkUnits[LidUncoverUnits.B].R2.HomeBegin();
            WorkUnits[LidUncoverUnits.A].R1.HomeEnd();
            WorkUnits[LidUncoverUnits.B].R1.HomeEnd();
            WorkUnits[LidUncoverUnits.A].R2.HomeEnd();
            WorkUnits[LidUncoverUnits.B].R2.HomeEnd();

            WorkUnits[LidUncoverUnits.A].R1.MoveFriendlyBegin(90, MoveType.REL);
            WorkUnits[LidUncoverUnits.B].R1.MoveFriendlyBegin(90, MoveType.REL);
            WorkUnits[LidUncoverUnits.A].R2.MoveFriendlyBegin(90, MoveType.REL);
            WorkUnits[LidUncoverUnits.B].R2.MoveFriendlyBegin(90, MoveType.REL);
            WorkUnits[LidUncoverUnits.A].R1.MoveEnd();
            WorkUnits[LidUncoverUnits.B].R1.MoveEnd();
            WorkUnits[LidUncoverUnits.A].R2.MoveEnd();
            WorkUnits[LidUncoverUnits.B].R2.MoveEnd();

            WorkUnits[LidUncoverUnits.A].C.HomeBegin();
            WorkUnits[LidUncoverUnits.B].C.HomeBegin();
            WorkUnits[LidUncoverUnits.A].C.HomeEnd();
            WorkUnits[LidUncoverUnits.B].C.HomeEnd();

            WorkUnits[LidUncoverUnits.A].T.HomeBegin();
            WorkUnits[LidUncoverUnits.B].T.HomeBegin();
            WorkUnits[LidUncoverUnits.A].E.HomeBegin();
            WorkUnits[LidUncoverUnits.B].E.HomeBegin();
            WorkUnits[LidUncoverUnits.A].T.HomeEnd();
            WorkUnits[LidUncoverUnits.B].T.HomeEnd();
            WorkUnits[LidUncoverUnits.A].E.HomeEnd();
            WorkUnits[LidUncoverUnits.B].E.HomeEnd();
        }

        public void SliderMoveTo(LidUncoverUnits lidUncover, SliderPositionEnum position)
        {
            var unit = WorkUnits[lidUncover];
            var p = SliderPositions.FirstOrDefault(x => x.LidUncover == lidUncover && x.Position == position);
            if (p == null)
            {
                throw new Exception($"Position pulse of work unit:{lidUncover},position:{position} not defined");
            }
            unit.T.MoveWithCheck(p.AbsPulse, MoveType.ABS);
        }
        public void TightenGrippers(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A)
        {
            var config = GetTubeConfig(tubeId);
            var unit = WorkUnits[lidUncover];
            unit.GripperA.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            unit.GripperB.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            unit.GripperA.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, false);
            unit.GripperB.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, false);
        }
        public IEnumerable<string> UnCoverAndScan(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A, LidUncoverGripper gripper = LidUncoverGripper.Both)
        {
            var config = GetTubeConfig(tubeId);
            var lidBottomOfZ = GetLidBottomOfZ(lidUncover);
            var eBasePosition = GetEBasePosition(lidUncover);
            bool aThrowIfEmptyGrasp = gripper == LidUncoverGripper.Both || gripper == LidUncoverGripper.A;
            bool bThrowIfEmptyGrasp = gripper == LidUncoverGripper.Both || gripper == LidUncoverGripper.B;
            string[] barcodes = new string[2] { "", "" };
            var unit = WorkUnits[lidUncover];

            // 夹爪先张开
            unit.GripperA.BeginRelease();
            unit.GripperB.BeginRelease();
            unit.GripperA.EndRelease();
            unit.GripperB.EndRelease();
            ReleaseCAxis(tubeId, lidUncover, LidUncoverCReleaseLevel.ForGrasp);
            unit.E.MoveWithCheck(eBasePosition + unit.E.MMToPulse(config.TubeCenterOffset), MoveType.ABS);

            unit.Z.SettingToDefault(5);
            unit.Z.SetToDefaultRate();
            unit.Z.MoveWithCheck(lidBottomOfZ - unit.Z.MMToPulse(config.UncoverZOffset), MoveType.ABS);

            unit.GripperA.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            unit.GripperB.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            unit.GripperA.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, aThrowIfEmptyGrasp);
            unit.GripperB.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, bThrowIfEmptyGrasp);

            if (lidUnCoverConfig.ScannerEnable)
            {
                ReleaseCAxis(tubeId, lidUncover, LidUncoverCReleaseLevel.ForLoosen);
                unit.Z.MoveWithCheck(unit.Z.MMToPulse(config.TubeBarcodePosition), MoveType.ABS);  //上升到扫码位置

                unit.R1.Setting(5, lidUnCoverConfig.UncoverConfig.ScanAccel);
                unit.R2.Setting(5, lidUnCoverConfig.UncoverConfig.ScanAccel);
                unit.R1.SetRate(lidUnCoverConfig.UncoverConfig.ScanSpeed);
                unit.R2.SetRate(lidUnCoverConfig.UncoverConfig.ScanSpeed);

                bool scanA = aThrowIfEmptyGrasp;
                bool scanB = bThrowIfEmptyGrasp;
                for (int i = 0; i < 2; i++)  //扫码时，对于未扫到条码的试管，再执行一次扫码
                {
                    if (scanA) //位置1有试管
                    {
                        unit.BarcodeA.BeginConsequentTrigger();
                    }
                    if (scanB)
                    {
                        unit.BarcodeB.BeginConsequentTrigger();
                    }
                    Thread.Sleep(100);  //旋转之前打开扫码枪并等待100ms  美超提出

                    unit.R1.MoveFriendlyBegin(360 * lidUnCoverConfig.UncoverConfig.ScanCycles, MoveType.REL);
                    unit.R2.MoveFriendlyBegin(360 * lidUnCoverConfig.UncoverConfig.ScanCycles, MoveType.REL);
                    unit.R1.MoveEnd();  // R轴不检查丢步
                    unit.R2.MoveEnd();  // R轴不检查丢步 

                    Thread.Sleep(100); //转完之后 延迟100毫秒再关闭   美超提出
                    if (scanA)
                    {
                        barcodes[0] = unit.BarcodeA.EndConsequentTrigger();
                        scanA = string.IsNullOrWhiteSpace(barcodes[0]);
                    }
                    if (scanB)
                    {
                        barcodes[1] = unit.BarcodeB.EndConsequentTrigger();
                        scanB = string.IsNullOrWhiteSpace(barcodes[1]);
                    }
                    if (!scanA && !scanB)
                    {
                        break;
                    }
                }
            }

            unit.Z.MoveWithCheck(lidBottomOfZ - unit.Z.MMToPulse(config.UncoverZOffset), MoveType.ABS);

            TightenCAxis(tubeId, lidUncover);

            //unit.GripperA.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            //unit.GripperB.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            //unit.GripperA.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, aThrowIfEmptyGrasp);
            //unit.GripperB.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, bThrowIfEmptyGrasp);

            unit.R1.Setting(5, config.UncoverFirstAccel);
            unit.R2.Setting(5, config.UncoverFirstAccel);
            unit.R1.SetRate(config.UncoverFirstSpeed);
            unit.R2.SetRate(config.UncoverFirstSpeed);
            unit.R1.MoveFriendlyBegin(360 * config.UncoverFirstCycles, MoveType.REL);
            unit.R2.MoveFriendlyBegin(360 * config.UncoverFirstCycles, MoveType.REL);
            unit.R1.MoveEnd(); // R轴不检查丢步
            unit.R2.MoveEnd();

            unit.R1.Setting(5, config.UncoverSecondAccel);
            unit.R2.Setting(5, config.UncoverSecondAccel);
            unit.R1.SetRate(config.UncoverSecondSpeed);
            unit.R2.SetRate(config.UncoverSecondSpeed);
            unit.Z.Setting(5, lidUnCoverConfig.UncoverConfig.UncoverUpSecondAccel);
            unit.Z.SetRate(lidUnCoverConfig.UncoverConfig.UncoverUpSecondSpeed);
            unit.R1.MoveFriendlyBegin(360 * config.UncoverSecondCycles, MoveType.REL);
            unit.R2.MoveFriendlyBegin(360 * config.UncoverSecondCycles, MoveType.REL);
            unit.Z.MoveBegin(-unit.Z.MMToPulse(config.UncoverZMoveDistance), MoveType.REL);
            unit.R1.MoveEnd();   // R轴不检查丢步
            unit.R2.MoveEnd();
            unit.Z.MoveEndWithCheck();

            //unit.GripperA.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            //unit.GripperB.BeginTighten(config.LidUncoverTightenForce, (ushort)config.LidUncoverGripperPosition);
            //int aPos2 = unit.GripperA.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, aThrowIfEmptyGrasp);
            //int bPos2 = unit.GripperB.EndTighten((ushort)config.LidUncoverGripperPosition, config.LidUncoverGripperPositionTolerance, bThrowIfEmptyGrasp);

            //if (Math.Abs(aPos2 - aPos1) > config.LidUncoverGripperPositionTolerance)
            //{
            //    throw new Exception($"Unit:{lidUncover},Gripper:A, lid not uncovered");
            //}
            //if (Math.Abs(bPos2 - bPos1) > config.LidUncoverGripperPositionTolerance)
            //{
            //    throw new Exception($"Unit:{lidUncover},Gripper:B, lid not uncovered");
            //}
            unit.Z.SettingToDefault(5);
            unit.Z.SetToDefaultRate();
            unit.Z.MoveWithCheck(lidUnCoverConfig.ZSafePositions[lidUncover], MoveType.ABS);
            return barcodes;
        }

        /// <summary>
        /// 盖盖子
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="lidUncover"></param>
        public void Cover(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A)
        {
            var config = GetTubeConfig(tubeId);
            var lidBottomOfZ = GetLidBottomOfZ(lidUncover);
            var eBasePosition = GetEBasePosition(lidUncover);
            var unit = WorkUnits[lidUncover];
            TightenCAxis(tubeId, lidUncover);
            unit.E.MoveWithCheck(eBasePosition + unit.E.MMToPulse(config.TubeCenterOffset), MoveType.ABS);
            var pulse = lidBottomOfZ - unit.Z.MMToPulse(config.UncoverZOffset) + unit.Z.MMToPulse(lidUnCoverConfig.CoverConfig.CoverDownOffset);
            unit.Z.SettingToDefault(5);
            unit.Z.SetToDefaultRate();
            unit.Z.MoveWithCheck(pulse, MoveType.ABS);

            unit.R1.Setting(5, config.CoverFirstAccel);
            unit.R2.Setting(5, config.CoverFirstAccel);
            unit.R1.SetRate(config.CoverFirstSpeed);
            unit.R2.SetRate(config.CoverFirstSpeed);
            unit.R1.MoveFriendlyBegin(-360 * config.CoverFirstCycles, MoveType.REL);
            unit.R2.MoveFriendlyBegin(-360 * config.CoverFirstCycles, MoveType.REL);
            unit.R1.MoveEnd();
            unit.R2.MoveEnd();

            unit.R1.Setting(5, config.CoverSecondAccel);
            unit.R2.Setting(5, config.CoverSecondAccel); //第二次旋转加速度
            unit.R1.SetRate(config.CoverSecondSpeed);
            unit.R2.SetRate(config.CoverSecondSpeed); // 第二次旋转速度
            unit.R1.Setting(6, lidUnCoverConfig.CoverConfig.CoverSecondCurrent);
            unit.R2.Setting(6, lidUnCoverConfig.CoverConfig.CoverSecondCurrent);
            unit.R1.MoveFriendlyBegin(-360 * config.CoverSecondCycles, MoveType.REL);
            unit.R2.MoveFriendlyBegin(-360 * config.CoverSecondCycles, MoveType.REL);
            unit.R1.MoveEnd();
            unit.R2.MoveEnd();

            unit.R1.SettingToDefault(6);
            unit.R2.SettingToDefault(6);

            unit.GripperA.BeginRelease();
            unit.GripperB.BeginRelease();
            unit.GripperA.EndRelease();
            unit.GripperB.EndRelease();
            unit.Z.Move(lidUnCoverConfig.ZSafePositions[lidUncover], MoveType.ABS);
        }
        /// <summary>
        /// 检查配置是否正确
        /// </summary>

        public void TightenCAxis(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A)
        {
            var config = GetTubeConfig(tubeId);
            var unit = WorkUnits[lidUncover];
            unit.C.MoveFriendlyWithCheck(CTightenOffset + config.CGripperOffset, MoveType.ABS);
        }

        public void ReleaseCAxis(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A, LidUncoverCReleaseLevel level = LidUncoverCReleaseLevel.ForGrasp)
        {
            var config = GetTubeConfig(tubeId);
            var unit = WorkUnits[lidUncover];
            if (level == LidUncoverCReleaseLevel.ForLoosen)
            {
                unit.C.MoveFriendlyWithCheck(CReleaseOffset + config.CGripperOffset - lidUnCoverConfig.CReleaseForLoosenOffset, MoveType.ABS);
            }
            else
            {
                unit.C.MoveFriendlyWithCheck(CReleaseOffset + config.CGripperOffset, MoveType.ABS);
            }
        }
        public void ResetRAxis(LidUncoverUnits lidUncover)
        {
            var unit = WorkUnits[lidUncover];

            unit.Z.SettingToDefault(5);
            unit.Z.SetToDefaultRate();
            unit.Z.Move(lidUnCoverConfig.ZSafePositions[lidUncover], MoveType.ABS);

            unit.R1.SettingToDefault(5);
            unit.R1.SettingToDefault(6);
            unit.R1.SetToDefaultRate();
            unit.R2.SettingToDefault(5);
            unit.R2.SettingToDefault(6);
            unit.R2.SetToDefaultRate();

            unit.R1.HomeBegin();
            unit.R2.HomeBegin();
            unit.R1.HomeEnd();
            unit.R2.HomeEnd();

            unit.R1.MoveFriendlyBegin(90, MoveType.REL);
            unit.R2.MoveFriendlyBegin(90, MoveType.REL);
            unit.R1.MoveEnd();
            unit.R2.MoveEnd();
        }
        public virtual void AssertSamplePosState(LidUncoverUnits lidUncover, SliderSamplePos pos, SliderSamplePosState state)
        {
            if (lidUnCoverConfig.SliderSensorEnable)
            {
                var axis = WorkUnits[lidUncover].C;
                var actual = (SliderSamplePosState)axis.Gap(10, (byte)pos);
                if (actual != state)
                {
                    throw new SliderPosStateException($"Work unit:{lidUncover} require sample position:{pos} {state},  actual: {actual}");
                }
            }
        }

        public virtual bool AssertSamplePosStateBool(LidUncoverUnits lidUncover, SliderSamplePos pos, SliderSamplePosState state)
        {
            var axis = WorkUnits[lidUncover].C;
            var actual = (SliderSamplePosState)axis.Gap(10, (byte)pos);
            return actual == state;
        }

        protected virtual ILeuzeBarcode CreateBarcodeInstance(SerialPortConfig config)
        {
            if (config.Simulated)
            {
                return new SimulatedLeuzeBarcode();
            }
            if (config.Type == 1)
            {
                return new LeuzeCR100Barcode(config);
            }
            else if (config.Type == 2)
            {
                return new LeuzeDCR55QRCode(config);
            }
            else
            {
                return new ConexBarcode(config);
            }
        }

        protected virtual IZimmaGripper CreateGripperInstance(string name, ModbusGripperConfig config)
        {
            if (config.Simulated)
            {
                return new SimulatedModbusGripper() { Name = name };

            }
            else
            {
                return new ModbusGripper(config) { Name = name };
            }
        }

        private int GetLidBottomOfZ(LidUncoverUnits workUnit)
        {
            if (!lidUnCoverConfig.LidBottomOfZs.ContainsKey(workUnit))
            {
                throw new Exception($"Work Unit:{workUnit} LidBottom of Z not set");
            }
            return lidUnCoverConfig.LidBottomOfZs[workUnit];
        }
        private int GetEBasePosition(LidUncoverUnits workUnit)
        {
            if (!lidUnCoverConfig.EBasePositions.ContainsKey(workUnit))
            {
                throw new Exception($"Work Unit:{workUnit} LidBottom of Z not set");
            }
            return lidUnCoverConfig.EBasePositions[workUnit];
        }
    }
}
