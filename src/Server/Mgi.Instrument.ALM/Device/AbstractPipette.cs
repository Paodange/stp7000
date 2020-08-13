using log4net;
using Mgi.Instrument.ALM.Axis;
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
    internal abstract class AbstractPipette : CanBasedDevice, IALMPipettes
    {
        /// <summary>
        /// Z轴安全位置 脉冲
        /// </summary>
        private const int Z_SAFE_POSITION = 100;
        /// <summary>
        /// 两个通道在Y轴上的间距(毫米)
        /// </summary>
        private const int DEFAULT_Y_DISTANCE = 9;
        public Dictionary<string, PipettePosition> Positions { get; private set; }

        /// <summary>
        /// 每个位置对应的物料类型 固定
        /// </summary>
        private readonly Dictionary<PipettePositions, string> defaultMaterialMapping
            = new Dictionary<PipettePositions, string>()
        {
            { PipettePositions.POS1, "Tips"},
            { PipettePositions.POS2, "Tips"},
            { PipettePositions.POS3, "DeepPlate"},
            { PipettePositions.POS4, "DeepPlate"},
            { PipettePositions.POS5, "Sample"},
            { PipettePositions.POS6, "Sample"},
            { PipettePositions.POS7, "Trash" } //退吸头位置
        };
        /// <summary>
        /// 物料的参数配置
        /// </summary>
        private readonly Dictionary<string, MaterialConfig> materialConfigs
            = new Dictionary<string, MaterialConfig>()
        {
            { "Tips",       new MaterialConfig(){ Name = "Tips",        RowCount = 8, ColumnCount = 12, RowSpan = 9, ColumnSpan = 9 } },
            { "DeepPlate",  new MaterialConfig(){ Name = "DeepPlate",   RowCount = 8, ColumnCount = 12, RowSpan = 9, ColumnSpan = 9 } },
            { "Sample",     new MaterialConfig(){ Name = "Sample",      RowCount = 1, ColumnCount = 2,  RowSpan = 0, ColumnSpan = 80 } },
            { "Trash",      new MaterialConfig(){ Name = "Trash",       RowCount = 1, ColumnCount = 2,  RowSpan = 0, ColumnSpan = 20 } },
        };
        private ALMPipettesConfig pipettesConfig;
        public Dictionary<PipetteChannels, PipetteChannel> Channels { get; } = new Dictionary<PipetteChannels, PipetteChannel>();
        public override ALMDeviceType DeviceType => ALMDeviceType.Pipettes;

        public AbstractPipette(IConfigProvider configProvider, IWorkflowManager workflowManager, ILog log, bool simulated)
         : base(simulated, configProvider.GetPipettesConfig().RobotConfigs, configProvider, log, workflowManager)
        {
            InitializeOrder = 0;
            var config = ConfigProvider.GetPipettesConfig();
            CheckConfig();
            Channels.Add(PipetteChannels.B, new PipetteChannel()
            {
                Channel = PipetteChannels.B,
                P = Axises["P1"],
                X = Axises["X"],
                Y = Axises["Y1"],
                Z = Axises["PZ1"]
            });
            Channels.Add(PipetteChannels.A, new PipetteChannel()
            {
                Channel = PipetteChannels.A,
                P = Axises["P2"],
                X = Axises["X"],
                Y = Axises["Y2"],
                Z = Axises["PZ2"]
            });
            pipettesConfig = config;
            Positions = config.Positions.ToDictionary(x => x.Name);
        }

        protected override void OnConfigChanged(ConfigChangedEventArgs e)
        {
            if (e.ConfigType == ConfigType.All || e.ConfigType == ConfigType.Pipette)
            {
                var config = ConfigProvider.GetPipettesConfig();
                pipettesConfig = config;
                Positions = config.Positions.ToDictionary(x => x.Name);
            }
        }

        public override void HomeAll()
        {
            var channelA = Channels[PipetteChannels.A];
            var channelB = Channels[PipetteChannels.B];
            channelA.Z.HomeBegin();
            channelB.Z.HomeBegin();
            channelA.Z.HomeEnd();
            channelB.Z.HomeEnd();

            if (pipettesConfig.ZCompensate != 0)
            {
                channelB.Z.MoveFriendlyWithCheck(pipettesConfig.ZCompensate, MoveType.ABS);
                channelB.Z.WriteActualPosition(0);
                channelB.Z.WriteEncoderPosition(0);
            }

            channelA.X.GoHome();  // X轴 共用一个电机  不需要再对B通道X轴 home操作

            //Y2 释放使能
            channelA.Y.Setting(7, 0);
            channelB.Y.GoHome();

            //恢复使能
            channelA.Y.SettingToDefault(7);
            channelA.Y.GoHome();
            //channelB.Y.MoveFriendly(DEFAULT_Y_DISTANCE, MoveType.ABS);
            //channelB.Y.WriteActualPosition(0);
            //channelB.Y.GoHome();
            // 退吸头
            MoveTo(PipettePositions.POS7, 1, 1);
            UnloadTips();

            channelA.P.HomeBegin();
            channelB.P.HomeBegin();
            channelA.P.HomeEnd();
            channelB.P.HomeEnd();
        }

        public void HomeAxis(PipetteChannels channel, string name)
        {
            var axis = GetPipetteAxis(channel, name);
            axis.GoHome();
            if (channel == PipetteChannels.B && "Z".Equals(name) && pipettesConfig.ZCompensate != 0)
            {
                axis.MoveFriendlyWithCheck(pipettesConfig.ZCompensate, MoveType.REL);
                axis.WriteActualPosition(0);
                axis.WriteEncoderPosition(0);
            }
        }

        public void HomeAxis(string name)
        {
            foreach (var channel in Channels)
            {
                var axis = GetPipetteAxis(channel.Key, name);
                axis.HomeBegin();
            }

            foreach (var channel in Channels)
            {
                var axis = GetPipetteAxis(channel.Key, name);
                axis.HomeEnd();
            }
            if ("Z".Equals(name) && pipettesConfig.ZCompensate != 0)
            {
                Channels[PipetteChannels.B].Z.MoveFriendlyWithCheck(pipettesConfig.ZCompensate, MoveType.REL);
                Channels[PipetteChannels.B].Z.WriteActualPosition(0);
                Channels[PipetteChannels.B].Z.WriteEncoderPosition(0);
            }
        }
        /// <summary>
        /// 多通道同时运动
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public void MoveTo(PipettePositions pos, int row, int column)
        {
            BeforeMove();
            var channelA = Channels[PipetteChannels.A];
            var channelB = Channels[PipetteChannels.B];
            // X轴
            channelA.X.MoveWithCheck(GetXPulse(pos, row), MoveType.ABS);

            // Y轴
            // 判断哪个通道先运动
            var firstMoveChannel = GetFirstMoveChannel(pos, column);
            if (firstMoveChannel == PipetteChannels.A)
            {
                channelA.Y.MoveWithCheck(GetYPulse(PipetteChannels.A, pos, column), MoveType.ABS);
                channelB.Y.MoveWithCheck(GetYPulse(PipetteChannels.B, pos, column), MoveType.ABS);
            }
            else
            {
                channelB.Y.MoveWithCheck(GetYPulse(PipetteChannels.B, pos, column), MoveType.ABS);
                channelA.Y.MoveWithCheck(GetYPulse(PipetteChannels.A, pos, column), MoveType.ABS);
            }
        }

        public void MoveTo(string pos, int row, int column)
        {
            if (!Enum.TryParse<PipettePositions>(pos, out var value))
            {
                throw new Exception($"POS：{pos} not defined");
            }
            MoveTo(value, row, column);
        }

        public void MoveTo(PipettePositions pos, int row, int column1, int column2)
        {
            if (column1 >= column2)
            {
                throw new Exception($"Cannot move to row:{row},column1:{column1},column2:{column2}");
            }
            BeforeMove();
            var channelA = Channels[PipetteChannels.A];
            var channelB = Channels[PipetteChannels.B];
            // X轴
            channelA.X.MoveWithCheck(GetXPulse(pos, row), MoveType.ABS);

            // Y轴
            // 判断哪个通道先运动
            var firstMoveChannel = GetFirstMoveChannel(pos, column2);
            if (firstMoveChannel == PipetteChannels.A)
            {
                channelA.Y.MoveWithCheck(GetYPulse(PipetteChannels.A, pos, column1), MoveType.ABS);
                channelB.Y.MoveWithCheck(GetYPulse(PipetteChannels.B, pos, column2 - 1), MoveType.ABS);
            }
            else
            {
                channelB.Y.MoveWithCheck(GetYPulse(PipetteChannels.B, pos, column2 - 1), MoveType.ABS);
                channelA.Y.MoveWithCheck(GetYPulse(PipetteChannels.A, pos, column1), MoveType.ABS);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="position"></param>
        public void LoadTip(PipetteChannels channel, PipettePositions position)
        {
            var c = Channels[channel];
            var pos = Positions[position.ToString()];
            c.Z.MoveWithCheck(pos.Z - c.Z.MMToPulse(pipettesConfig.LoadTipConfig.ZFirstMoveOffset), MoveType.ABS);
            c.Z.Setting(5, pipettesConfig.LoadTipConfig.LoadTipAccel);
            c.Z.SetRate(pipettesConfig.LoadTipConfig.LoadTipSpeed);

            c.Z.MoveBeginWithCheck(pos.Z + c.Z.MMToPulse(pipettesConfig.LoadTipConfig.LoadTipOffset), MoveType.ABS);
            c.Z.LoadTipMoveEndWithCheck(pipettesConfig.LoadTipConfig.MinAccuracy, pipettesConfig.LoadTipConfig.MaxAccuracy);
            c.Z.SettingToDefault(5);
            c.Z.SetToDefaultRate();

            c.Z.MoveWithCheck(Z_SAFE_POSITION, MoveType.ABS);
        }

        public void LoadTips(PipettePositions position)
        {
            var a = Channels[PipetteChannels.A];
            var b = Channels[PipetteChannels.B];
            var pos = Positions[position.ToString()];
            a.Z.SettingToDefault(5);
            b.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SetToDefaultRate();

            a.Z.MoveBeginWithCheck(pos.Z - a.Z.MMToPulse(pipettesConfig.LoadTipConfig.ZFirstMoveOffset), MoveType.ABS);
            b.Z.MoveBeginWithCheck(pos.Z - b.Z.MMToPulse(pipettesConfig.LoadTipConfig.ZFirstMoveOffset), MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();

            a.Z.Setting(5, pipettesConfig.LoadTipConfig.LoadTipAccel);
            b.Z.Setting(5, pipettesConfig.LoadTipConfig.LoadTipAccel);
            a.Z.SetRate(pipettesConfig.LoadTipConfig.LoadTipSpeed);
            b.Z.SetRate(pipettesConfig.LoadTipConfig.LoadTipSpeed);

            a.Z.MoveBeginWithCheck(pos.Z + a.Z.MMToPulse(pipettesConfig.LoadTipConfig.LoadTipOffset), MoveType.ABS);
            a.Z.LoadTipMoveEndWithCheck(pipettesConfig.LoadTipConfig.MinAccuracy, pipettesConfig.LoadTipConfig.MaxAccuracy);
            b.Z.MoveBeginWithCheck(pos.Z + b.Z.MMToPulse(pipettesConfig.LoadTipConfig.LoadTipOffset), MoveType.ABS);
            b.Z.LoadTipMoveEndWithCheck(pipettesConfig.LoadTipConfig.MinAccuracy, pipettesConfig.LoadTipConfig.MaxAccuracy);
            a.Z.SettingToDefault(5);
            b.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SetToDefaultRate();
            a.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            b.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();
        }

        public void UnLoadTip(PipetteChannels channel)
        {
            var c = Channels[channel];
            var pos = Positions[PipettePositions.POS7.ToString()];
            c.Z.SettingToDefault(5);
            c.Z.SetToDefaultRate();
            c.Z.MoveWithCheck(pos.Z, MoveType.ABS);
            c.X.SetRate(50);
            c.X.MoveWithCheck(pos.X + c.X.MMToPulse(pipettesConfig.UnloadTipConfig.UnloadTipXOffset), MoveType.ABS);
            c.X.SetToDefaultRate();
            c.Z.Setting(5, pipettesConfig.UnloadTipConfig.UnloadTipZUpAccel);
            c.Z.SetRate(pipettesConfig.UnloadTipConfig.UnloadTipZUpSpeed);
            c.Z.MoveWithCheck(pos.Z - c.Z.MMToPulse(pipettesConfig.UnloadTipConfig.UnloadTipZUpDistance), MoveType.ABS);
            c.Z.SettingToDefault(5);
            c.Z.SetToDefaultRate();
            c.Z.MoveWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            c.X.MoveWithCheck(pos.X, MoveType.ABS);
        }

        public void UnloadTips()
        {
            var a = Channels[PipetteChannels.A];
            var b = Channels[PipetteChannels.B];
            var pos = Positions[PipettePositions.POS7.ToString()];
            a.Z.SettingToDefault(5);
            b.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SetToDefaultRate();
            a.Z.MoveBeginWithCheck(pos.Z, MoveType.ABS);
            b.Z.MoveBeginWithCheck(pos.Z, MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();

            a.X.SetRate(50);
            a.X.MoveBeginWithCheck(pos.X + a.X.MMToPulse(pipettesConfig.UnloadTipConfig.UnloadTipXOffset), MoveType.ABS);
            a.X.MoveEndWithCheck();
            a.X.SetToDefaultRate();

            a.Z.Setting(5, pipettesConfig.UnloadTipConfig.UnloadTipZUpAccel);
            a.Z.SetRate(pipettesConfig.UnloadTipConfig.UnloadTipZUpSpeed);
            b.Z.Setting(5, pipettesConfig.UnloadTipConfig.UnloadTipZUpAccel);
            b.Z.SetRate(pipettesConfig.UnloadTipConfig.UnloadTipZUpSpeed);
            a.Z.MoveBeginWithCheck(pos.Z - a.Z.MMToPulse(pipettesConfig.UnloadTipConfig.UnloadTipZUpDistance), MoveType.ABS);
            b.Z.MoveBeginWithCheck(pos.Z - b.Z.MMToPulse(pipettesConfig.UnloadTipConfig.UnloadTipZUpDistance), MoveType.ABS);
            //a.Z.MoveFriendlyBegin(-10, MoveType.REL);
            //b.Z.MoveFriendlyBegin(-10, MoveType.REL);
            a.Z.MoveEndWithCheck();
            a.Z.MoveEndWithCheck();

            a.Z.SettingToDefault(5);
            b.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SetToDefaultRate();

            a.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            b.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();
            a.X.MoveWithCheck(pos.X, MoveType.ABS);
        }

        public void Aspirate(string tubeId, PipetteChannels channel, PipettePositions position, double volume = 200)
        {
            var c = Channels[channel];
            var pos = Positions[position.ToString()];
            var tubeConfig = GetTubeConfig(tubeId);
            if (tubeConfig.TubeCenterOffset != 0)
            {
                c.X.MoveWithCheck(pos.X - c.X.MMToPulse(tubeConfig.TubeCenterOffset), MoveType.ABS);
            }

            c.P.Setting(5, pipettesConfig.AspirateConfig.AspirateAccel);
            c.P.SetRate(pipettesConfig.AspirateConfig.AspirateSpeed);
            if (pipettesConfig.AspirateConfig.FrontAspirateVolume != 0)
            {
                c.P.MoveWithCheck(GetActualPulseWithCompensate(pipettesConfig.AspirateConfig.FrontAspirateVolume), MoveType.ABS);
            }
            c.Z.SettingToDefault(5);
            c.Z.SetToDefaultRate();
            if (ConfigProvider.GetMachineConfig().UncoverFailHandleMode == 1)
            {
                c.Z.MoveWithCheck(pos.Z - c.Z.MMToPulse(tubeConfig.AspirateFirstOffset), MoveType.ABS);
            }
            else
            {
                try
                {
                    c.Z.MoveBeginWithCheck(pos.Z - c.Z.MMToPulse(tubeConfig.AspirateFirstOffset), MoveType.ABS);
                    c.Z.MoveEndWithCheckNonInterceptor();
                }
                catch (AccuracyCheckFailException e)
                {
                    c.Z.GoHome();
                    c.Z.FixedPosRegister();
                    throw new AspirateAccuracyCheckFailException($"Asiprate fail due to {c.Z.Name} accuracy check fail", channel, e);
                }
            }

            c.Z.Setting(5, pipettesConfig.AspirateConfig.ZSecondAccel);
            c.Z.SetRate(pipettesConfig.AspirateConfig.ZSecondSpeed);
            if (ConfigProvider.GetMachineConfig().UncoverFailHandleMode == 1)
            {
                c.Z.MoveWithCheck(pos.Z - c.Z.MMToPulse(tubeConfig.AspirateHeight), MoveType.ABS);
            }
            else
            {
                try
                {
                    c.Z.MoveBeginWithCheck(pos.Z - c.Z.MMToPulse(tubeConfig.AspirateHeight), MoveType.ABS);
                    c.Z.MoveEndWithCheckNonInterceptor();
                }
                catch (AccuracyCheckFailException e)
                {
                    c.Z.GoHome();
                    c.Z.FixedPosRegister();
                    throw new AspirateAccuracyCheckFailException($"Asiprate fail due to {c.Z.Name} accuracy check fail", channel, e);
                }
            }

            c.P.MoveWithCheck(GetActualPulseWithCompensate(volume + pipettesConfig.AspirateConfig.FrontAspirateVolume), MoveType.ABS);

            Thread.Sleep(pipettesConfig.AspirateConfig.AspirateDelay);
            c.Z.SettingToDefault(5);
            c.Z.SetToDefaultRate();
            c.Z.MoveWithCheck(0, MoveType.ABS);
            if (pipettesConfig.AspirateConfig.BackAspirateVolume != 0)
            {
                c.P.MoveWithCheck(GetActualPulseWithCompensate(volume
                    + pipettesConfig.AspirateConfig.FrontAspirateVolume
                    + pipettesConfig.AspirateConfig.BackAspirateVolume), MoveType.ABS);
            }
        }

        public void AspirateBoth(string tubeId, PipettePositions position, double volume = 200)
        {
            var tubeConfig = GetTubeConfig(tubeId);
            int pulse = 0;
            var a = Channels[PipetteChannels.A];
            var b = Channels[PipetteChannels.B];
            var pos = Positions[position.ToString()];
            a.P.Setting(5, pipettesConfig.AspirateConfig.AspirateAccel);
            a.P.SetRate(pipettesConfig.AspirateConfig.AspirateSpeed);
            b.P.Setting(5, pipettesConfig.AspirateConfig.AspirateAccel);
            b.P.SetRate(pipettesConfig.AspirateConfig.AspirateSpeed);
            if (tubeConfig.TubeCenterOffset != 0)
            {
                a.X.MoveWithCheck(pos.X - a.X.MMToPulse(tubeConfig.TubeCenterOffset), MoveType.ABS);
            }

            if (pipettesConfig.AspirateConfig.FrontAspirateVolume != 0)
            {
                pulse = GetActualPulseWithCompensate(pipettesConfig.AspirateConfig.FrontAspirateVolume);
                a.P.MoveBeginWithCheck(pulse, MoveType.ABS);
                b.P.MoveBeginWithCheck(pulse, MoveType.ABS);
                a.P.MoveEndWithCheck();
                b.P.MoveEndWithCheck();
            }
            a.Z.SettingToDefault(5);
            b.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SetToDefaultRate();
            a.Z.MoveBeginWithCheck(pos.Z - a.Z.MMToPulse(tubeConfig.AspirateFirstOffset), MoveType.ABS);
            b.Z.MoveBeginWithCheck(pos.Z - b.Z.MMToPulse(tubeConfig.AspirateFirstOffset), MoveType.ABS);

            var errHandleMode = ConfigProvider.GetMachineConfig().UncoverFailHandleMode;
            if (errHandleMode == 1)
            {
                a.Z.MoveEndWithCheck();
                b.Z.MoveEndWithCheck();
            }
            else
            {
                AspirateZMoveEndWithCheck(tubeId, position, volume);
            }


            a.Z.Setting(5, pipettesConfig.AspirateConfig.ZSecondAccel);
            b.Z.Setting(5, pipettesConfig.AspirateConfig.ZSecondAccel);

            a.Z.SetRate(pipettesConfig.AspirateConfig.ZSecondSpeed);
            b.Z.SetRate(pipettesConfig.AspirateConfig.ZSecondSpeed);

            a.Z.MoveBeginWithCheck(pos.Z - a.Z.MMToPulse(tubeConfig.AspirateHeight), MoveType.ABS);
            b.Z.MoveBeginWithCheck(pos.Z - b.Z.MMToPulse(tubeConfig.AspirateHeight), MoveType.ABS);
            if (errHandleMode == 1)
            {
                a.Z.MoveEndWithCheck();
                b.Z.MoveEndWithCheck();
            }
            else
            {
                AspirateZMoveEndWithCheck(tubeId, position, volume);
            }

            //c.Z.MoveWithCheck(170073, MoveType.ABS);
            //c.Z.MoveWithCheck(170073, MoveType.ABS);
            pulse = GetActualPulseWithCompensate(volume + pipettesConfig.AspirateConfig.FrontAspirateVolume);

            a.P.MoveWithCheck(pulse, MoveType.ABS);
            b.P.MoveWithCheck(pulse, MoveType.ABS);
            a.P.MoveEndWithCheck();
            b.P.MoveEndWithCheck();

            Thread.Sleep(pipettesConfig.AspirateConfig.AspirateDelay);

            a.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SettingToDefault(5);
            b.Z.SetToDefaultRate();

            a.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            b.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();
            if (pipettesConfig.AspirateConfig.BackAspirateVolume != 0)
            {
                pulse = GetActualPulseWithCompensate(volume + pipettesConfig.AspirateConfig.FrontAspirateVolume + pipettesConfig.AspirateConfig.BackAspirateVolume);
                a.P.MoveBeginWithCheck(pulse, MoveType.ABS);
                b.P.MoveBeginWithCheck(pulse, MoveType.ABS);
                a.P.MoveEndWithCheck();
                b.P.MoveEndWithCheck();
            }
        }

        public void UnAspirate(PipetteChannels channel, PipettePositions position)
        {
            var c = Channels[channel];
            var pos = Positions[position.ToString()];
            c.Z.SettingToDefault(5);
            c.Z.SetToDefaultRate();
            c.Z.MoveWithCheck(pos.Z - c.Z.MMToPulse(pipettesConfig.UnAspirateConfig.UnAspirateHeight), MoveType.ABS);

            c.P.Setting(5, pipettesConfig.UnAspirateConfig.UnAspirateAccel);
            c.P.SetRate(pipettesConfig.UnAspirateConfig.UnAspirateSpeed);
            c.P.MoveWithCheck(0, MoveType.ABS);

            Thread.Sleep(pipettesConfig.UnAspirateConfig.UnAspirateDelay);

            c.Z.Setting(5, pipettesConfig.UnAspirateConfig.ZFirstUpAccel);
            c.Z.SetRate(pipettesConfig.UnAspirateConfig.ZFirstUpSpeed);
            c.Z.MoveWithCheck(pos.Z - c.Z.MMToPulse(pipettesConfig.UnAspirateConfig.ZFirstUpOffset), MoveType.ABS);

            c.Z.SettingToDefault(5);
            c.Z.SetToDefaultRate();
            c.Z.MoveWithCheck(Z_SAFE_POSITION, MoveType.ABS);
        }

        public void UnAspirateBoth(PipettePositions position)
        {
            var a = Channels[PipetteChannels.A];
            var b = Channels[PipetteChannels.B];
            var pos = Positions[position.ToString()];
            a.Z.SettingToDefault(5);
            b.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SetToDefaultRate();
            a.Z.MoveBeginWithCheck(pos.Z - a.Z.MMToPulse(pipettesConfig.UnAspirateConfig.UnAspirateHeight), MoveType.ABS);
            b.Z.MoveBeginWithCheck(pos.Z - b.Z.MMToPulse(pipettesConfig.UnAspirateConfig.UnAspirateHeight), MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();

            a.P.Setting(5, pipettesConfig.UnAspirateConfig.UnAspirateAccel);
            b.P.Setting(5, pipettesConfig.UnAspirateConfig.UnAspirateAccel);

            a.P.SetRate(pipettesConfig.UnAspirateConfig.UnAspirateSpeed);
            b.P.SetRate(pipettesConfig.UnAspirateConfig.UnAspirateSpeed);

            a.P.MoveBeginWithCheck(0, MoveType.ABS);
            b.P.MoveBeginWithCheck(0, MoveType.ABS);
            a.P.MoveEndWithCheck();
            b.P.MoveEndWithCheck();

            Thread.Sleep(pipettesConfig.UnAspirateConfig.UnAspirateDelay);

            a.Z.Setting(5, pipettesConfig.UnAspirateConfig.ZFirstUpAccel);
            b.Z.Setting(5, pipettesConfig.UnAspirateConfig.ZFirstUpAccel);
            a.Z.SetRate(pipettesConfig.UnAspirateConfig.ZFirstUpSpeed);
            b.Z.SetRate(pipettesConfig.UnAspirateConfig.ZFirstUpSpeed);
            a.Z.MoveBeginWithCheck(pos.Z - a.Z.MMToPulse(pipettesConfig.UnAspirateConfig.ZFirstUpOffset), MoveType.ABS);
            b.Z.MoveBeginWithCheck(pos.Z - b.Z.MMToPulse(pipettesConfig.UnAspirateConfig.ZFirstUpOffset), MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();

            a.Z.SettingToDefault(5);
            b.Z.SettingToDefault(5);
            a.Z.SetToDefaultRate();
            b.Z.SetToDefaultRate();
            a.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            b.Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            a.Z.MoveEndWithCheck();
            b.Z.MoveEndWithCheck();
        }
        /// <summary>
        /// 检查配置是否正确
        /// </summary>
        protected override void CheckConfig()
        {
            Contract.Assert(Axises != null);
            Contract.Assert(Axises.ContainsKey("P1"));
            Contract.Assert(Axises.ContainsKey("P2"));
            Contract.Assert(Axises.ContainsKey("Y1"));
            Contract.Assert(Axises.ContainsKey("Y2"));
            Contract.Assert(Axises.ContainsKey("PZ1"));
            Contract.Assert(Axises.ContainsKey("PZ2"));
            Contract.Assert(Axises.ContainsKey("X"));
        }

        private PipetteChannels GetFirstMoveChannel(PipettePositions pos, int channelBColumn)
        {
            var channelBOffset = Channels[PipetteChannels.B].Y.ReadActualPosition();
            // 目标位置偏移比当前位置大   向外运动  通道A先动  
            if (GetYPulse(PipetteChannels.B, pos, channelBColumn) > channelBOffset)
            {
                return PipetteChannels.A;
            }
            else
            {
                // 向里运动
                return PipetteChannels.B;
            }
        }

        private void BeforeMove()
        {
            Channels[PipetteChannels.A].Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            Channels[PipetteChannels.B].Z.MoveBeginWithCheck(Z_SAFE_POSITION, MoveType.ABS);
            Channels[PipetteChannels.A].Z.MoveEndWithCheck();
            Channels[PipetteChannels.B].Z.MoveEndWithCheck();
        }

        /// <summary>
        /// 计算移动到目标位置时 X轴需要绝对移动的脉冲数
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private int GetXPulse(PipettePositions pos, int row)
        {
            var position = Positions[pos.ToString()];
            var pulse = position.X;
            if (row > 1)
            {
                var config = materialConfigs[defaultMaterialMapping[pos]];
                pulse += Channels[PipetteChannels.A].X.MMToPulse((row - 1) * config.RowSpan);
            }
            return pulse;
        }

        /// <summary>
        /// 计算移动到目标位置时 Y轴需要绝对移动的脉冲数
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="pos"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int GetYPulse(PipetteChannels channel, PipettePositions pos, int column)
        {
            var position = Positions[pos.ToString()];
            var pulse = position.Y;
            var config = materialConfigs[defaultMaterialMapping[pos]];

            // 位置是根据B学习的  A通道要计算偏移
            if (channel == PipetteChannels.B)
            {
                pulse -= Channels[PipetteChannels.A].Y.MMToPulse((column) * config.ColumnSpan - DEFAULT_Y_DISTANCE - pipettesConfig.YCompensate);
            }
            else
            {
                pulse -= Channels[PipetteChannels.A].Y.MMToPulse((column - 1) * config.ColumnSpan);
            }
            return pulse;
        }
        private IALMAxis GetPipetteAxis(PipetteChannels c, string axisName)
        {
            var channel = Channels[c];
            var p = channel.GetType().GetProperty(axisName);
            if (p == null)
            {
                throw new Exception($"Unknow axis:{axisName} for channel:{c}");
            }
            return p.GetValue(channel) as IALMAxis;
        }
        private IALMAxis GetPipetteAxis(string channel, string axisName)
        {
            if (!Enum.TryParse<PipetteChannels>(channel, out var e))
            {
                throw new Exception($"Unknow channel:{channel}");
            }
            return GetPipetteAxis(e, axisName);
        }

        private int GetActualPulseWithCompensate(double volume)
        {
            // y = ax+b; 吸液补偿
            var cc = GetCompensateConfig(volume);
            var pulse = Axises["P1"].MMToPulse(volume);
            return (int)(cc.CompensateA * pulse + cc.CompensateB);
        }

        private CompensateConfig GetCompensateConfig(double volume)
        {
            var c = pipettesConfig.CompensateConfigs.FirstOrDefault(x => volume >= x.MinVolume && volume < x.MaxVolume);
            if (c == null)
            {
                return new CompensateConfig() { MinVolume = 0, MaxVolume = 1000, CompensateA = 1, CompensateB = 0 };
            }
            return c;
        }

        private void AspirateZMoveEndWithCheck(string tubeId, PipettePositions position, double volume)
        {
            var a = Channels[PipetteChannels.A];
            var b = Channels[PipetteChannels.B];
            var exceptions = new Dictionary<PipetteChannels, AccuracyCheckFailException>();
            try
            {
                a.Z.MoveEndWithCheckNonInterceptor();
            }
            catch (AccuracyCheckFailException e)
            {
                exceptions.Add(PipetteChannels.A, e);
            }
            try
            {
                b.Z.MoveEndWithCheckNonInterceptor();
            }
            catch (AccuracyCheckFailException e)
            {
                exceptions.Add(PipetteChannels.B, e);
            }
            if (exceptions.Count == 2) //两个都没拔盖成功  
            {
                foreach (var e in exceptions)
                {
                    e.Value.Axis.HomeBegin();
                }
                foreach (var e in exceptions)
                {
                    e.Value.Axis.HomeEnd();
                }
                foreach (var e in exceptions)
                {
                    e.Value.Axis.FixedPosRegister();
                }
                throw new AspirateAccuracyCheckFailException($"Asiprate fail due to both PZ accuracy check fail", PipetteChannels.Both, exceptions.Values.ToArray());
            }
            else if (exceptions.Count == 1)
            {
                var item = exceptions.ElementAt(0);
                var c = item.Key == PipetteChannels.A ? PipetteChannels.B : PipetteChannels.A;
                item.Value.Axis.GoHome();
                item.Value.Axis.FixedPosRegister();
                Aspirate(tubeId, c, position, volume);  //另一个通道继续吸液
                throw new AspirateAccuracyCheckFailException($"Asiprate fail due to {item.Value.Axis.Name} accuracy check fail", item.Key, exceptions.Values.ToArray());
            }
        }
    }
}
