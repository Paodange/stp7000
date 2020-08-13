using log4net;
using Mgi.Epson.Robot.T3;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Config;
using Mgi.Instrument.ALM.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mgi.Instrument.ALM.Device
{
    internal abstract class AbstractRobot : AbstractDevice
    {
        static readonly object syncLock = new object();
        public override ALMDeviceType DeviceType => ALMDeviceType.Robot;


        bool gripperInitialized = false;
        bool robotInitialized = false;
        bool opened = false;
        Dictionary<RobotLocation, LocationMap> locationDict;
        readonly IConfigProvider _configProvider;
        protected IWorkflowManager WorkflowManager { get; }
        public IZimmaGripper Gripper { get; protected set; }
        public IEpsonRobot EpsonRobot { get; protected set; }
        public AbstractRobot(IConfigProvider configProvider, IWorkflowManager workflowManager, ILog log)
            : base(configProvider)
        {
            _configProvider = configProvider;
            WorkflowManager = workflowManager;
            InitializeOrder = 10;
            locationDict = _configProvider.GetRobotLocations().ToDictionary(x => x.Location);
            InitialComponents();
        }

        protected abstract void InitialComponents();
        protected override void OnConfigChanged(ConfigChangedEventArgs e)
        {
            if (e.ConfigType == ConfigType.All || e.ConfigType == ConfigType.RobotLocation)
            {
                locationDict = _configProvider.GetRobotLocations().ToDictionary(x => x.Location);
            }
        }

        public override void Close()
        {
            EpsonRobot.Close();
            Gripper.Close();
        }

        public override void Initialize()
        {
            if (!gripperInitialized)
            {
                Gripper.Initialize();
                gripperInitialized = true;
            }
            if (!opened)
            {
                EpsonRobot.Open();
                opened = true;
            }
            if (!robotInitialized)
            {
                EpsonRobot.Initialize();
                robotInitialized = true;
            }
        }
        public void Open()
        {
            if (opened) return;
            EpsonRobot.Open();
            opened = true;
        }
        public void Home()
        {
            EpsonRobot.Reset();
        }
        public void Grasp(string tubeId, RobotLocation location, int row = 0, int column = 0)
        {
            lock (syncLock)
            {
                GraspOnly(tubeId, location, row, column);
                GraspGoBack(location, row, column);
            }
        }
        public void GraspOnly(string tubeId, RobotLocation location, int row = 0, int column = 0)
        {
            var config = GetTubeConfig(tubeId);
            lock (syncLock)
            {
                CheckLocation(location, row, column);
                var loc = locationDict[location];
                Gripper.ReleaseGripper();
                if (loc.UsePallet)
                {
                    for (int i = 0; i < loc.GraspGoPaths.Count; i++)
                    {
                        GotoPath(loc.GraspGoPaths[i], 0, 0, loc.GraspGoPaths[i].ZOffset);
                    }
                    var pallet = GetRobotPallet(location, row, column);
                    var xoffset = RequireXOffset(location) ? -config.TubeCenterOffset : 0;
                    GotoPallet(RobotAction.Jump, pallet.PalletNo, pallet.Row, pallet.Column, xoffset, 0, loc.ZOffset + config.RobotGraspZOffset);
                    TightenGripper(tubeId);
                }
                else
                {
                    var last = loc.GraspGoPaths.LastOrDefault();
                    for (int i = 0; i < loc.GraspGoPaths.Count - 1; i++)
                    {
                        GotoPath(loc.GraspGoPaths[i], 0, 0, loc.GraspGoPaths[i].ZOffset);
                    }
                    var xoffset = RequireXOffset(location) ? -config.TubeCenterOffset : 0;
                    GotoPath(last, xoffset, 0, last.ZOffset + config.RobotGraspZOffset);
                    TightenGripper(tubeId);
                }
            }
        }

        public void Loosen(string tubeId, RobotLocation location, int row = 0, int column = 0)
        {
            lock (syncLock)
            {
                LoosenOnly(tubeId, location, row, column);
                LoosenGoBack(location, row, column);
            }
        }

        public void LoosenOnly(string tubeId, RobotLocation location, int row = 0, int column = 0)
        {
            var config = GetTubeConfig(tubeId);
            lock (syncLock)
            {
                CheckLocation(location, row, column);
                var loc = locationDict[location];
                if (loc.UsePallet)
                {
                    for (int i = 0; i < loc.LoosenGoPaths.Count; i++)
                    {
                        GotoPath(loc.LoosenGoPaths[i], 0, 0, loc.LoosenGoPaths[i].ZOffset);
                    }
                    var pallet = GetRobotPallet(location, row, column);
                    var xoffset = RequireXOffset(location) ? -config.TubeCenterOffset : 0;
                    GotoPallet(RobotAction.Jump, pallet.PalletNo, pallet.Row, pallet.Column, xoffset, 0, loc.ZOffset + config.RobotGraspZOffset);
                    Gripper.ReleaseGripper();
                }
                else
                {
                    var last = loc.LoosenGoPaths.LastOrDefault();
                    for (int i = 0; i < loc.LoosenGoPaths.Count - 1; i++)
                    {
                        GotoPath(loc.LoosenGoPaths[i], 0, 0, loc.LoosenGoPaths[i].ZOffset);
                    }
                    var xoffset = RequireXOffset(location) ? -config.TubeCenterOffset : 0;
                    GotoPath(last, xoffset, 0, last.ZOffset + config.RobotGraspZOffset);
                    Gripper.ReleaseGripper();
                }
                Thread.Sleep(100);
            }
        }
        public void GraspGoBack(RobotLocation location, int row = 0, int column = 0)
        {
            lock (syncLock)
            {
                CheckLocation(location, row, column);
                var loc = locationDict[location];
                for (int i = 0; i < loc.GraspBackPaths.Count; i++)
                {
                    GotoPath(loc.GraspBackPaths[i], 0, 0, loc.GraspBackPaths[i].ZOffset);
                }
            }
        }
        public void LoosenGoBack(RobotLocation location, int row = 0, int column = 0)
        {
            lock (syncLock)
            {
                CheckLocation(location, row, column);
                var loc = locationDict[location];
                for (int i = 0; i < loc.LoosenBackPaths.Count; i++)
                {
                    GotoPath(loc.LoosenBackPaths[i], 0, 0, loc.LoosenBackPaths[i].ZOffset);
                }
            }
        }
        private void CheckLocation(RobotLocation location, int row, int column)
        {
            if (!locationDict.ContainsKey(location))
            {
                throw new Exception($"Location:{location} not found");
            }
            if (location == RobotLocation.HotelA || location == RobotLocation.HotelB)
            {
                if (row <= 0 || row > 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(row), $"Row:{row} of location:{location} is out of range");
                }
                if (column <= 0 || column > 12)
                {
                    throw new ArgumentOutOfRangeException(nameof(column), $"Column:{column} of location:{location} is out of range");
                }
            }
        }

        public int TightenGripper(string tubeId, bool throwIfOverTolerance = true)
        {
            var config = GetTubeConfig(tubeId);
            return Gripper.TightenGripper(config.RobotTightenForce, (ushort)config.RobotGripperPosition, config.RobotGripperPositionTolerance, throwIfOverTolerance);
        }

        public int ReleaseGripper()
        {
            return Gripper.ReleaseGripper();
        }

        private void GotoPallet(RobotAction action, int palletNo, int row, int column, double xOffset = 0.0, double yOffset = 0.0, double zOffset = 0.0)
        {
            switch (action)
            {
                case RobotAction.Jump:
                    EpsonRobot.JumpPalletNo(palletNo, row, column, xOffset, yOffset, zOffset);
                    break;
                case RobotAction.Go:
                    EpsonRobot.GoPalletNo(palletNo, row, column, xOffset, yOffset, zOffset);
                    break;
                case RobotAction.Move:
                    EpsonRobot.GoPalletNo(palletNo, row, column, xOffset, yOffset, zOffset);
                    break;
                case RobotAction.JumpZ0:
                case RobotAction.Arc:
                case RobotAction.Arc3:
                default:
                    throw new Exception($"Unsupport action:{action}");
            }
        }

        private void GotoPath(RobotPath path, double xOffset = 0, double yOffset = 0, double zOffset = 0)
        {
            switch (path.Action)
            {
                case RobotAction.Jump:
                    EpsonRobot.Jump(path.GetActualPos(), xOffset, yOffset, zOffset);
                    break;
                case RobotAction.Go:
                    EpsonRobot.Go(path.GetActualPos(), xOffset, yOffset, zOffset);
                    break;
                case RobotAction.Move:
                    EpsonRobot.Move(path.GetActualPos(), xOffset, yOffset, zOffset);
                    break;
                case RobotAction.JumpZ0:
                case RobotAction.Arc:
                case RobotAction.Arc3:
                default:
                    throw new Exception($"Unsupport action:{path.Action}");
            }
        }

        /// <summary>
        /// Hotel 行列号 到机械臂阵列号，行列号的转换
        /// </summary>
        /// <param name="location"></param>
        /// <param name="hotelRow"></param>
        /// <param name="hotelColumn"></param>
        /// <returns></returns>
        private RobotPallet GetRobotPallet(RobotLocation location, int hotelRow, int hotelColumn)
        {
            if (hotelRow < 1 || hotelRow > 8)
            {
                throw new ArgumentOutOfRangeException($"Row:{hotelRow} out of range:[1,8]");
            }
            if (hotelColumn < 1 || hotelColumn > 12)
            {
                throw new ArgumentOutOfRangeException($"Column:{hotelColumn} out of range:[1,12]");
            }
            if (location == RobotLocation.HotelA)
            {
                if (hotelRow <= 4)
                {
                    return new RobotPallet()
                    {
                        PalletNo = 1,
                        Row = hotelColumn,
                        Column = hotelRow
                    };
                }
                else
                {
                    return new RobotPallet()
                    {
                        PalletNo = 2,
                        Row = hotelColumn,
                        Column = hotelRow - 4
                    };
                }
            }
            else if (location == RobotLocation.HotelB)
            {
                if (hotelColumn <= 5)
                {
                    if (hotelRow <= 4)
                    {
                        return new RobotPallet()
                        {
                            PalletNo = 3,
                            Row = hotelColumn,
                            Column = hotelRow
                        };
                    }
                    else
                    {
                        return new RobotPallet()
                        {
                            PalletNo = 4,
                            Row = hotelColumn,
                            Column = hotelRow - 4
                        };
                    }
                }
                else
                {
                    return new RobotPallet()
                    {
                        PalletNo = 5,
                        Row = hotelColumn - 5,
                        Column = hotelRow
                    };
                }
            }
            throw new Exception($"Undefined robot pallet position:{location},row:{hotelRow},column:{hotelColumn}");
        }

        private bool RequireXOffset(RobotLocation location)
        {
            return location != RobotLocation.HotelA && location != RobotLocation.HotelB;
        }
    }
    public class RobotPallet
    {
        public int PalletNo { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"PalletNo:{PalletNo},Row:{Row},Column:{Column}";
        }
    }
}
