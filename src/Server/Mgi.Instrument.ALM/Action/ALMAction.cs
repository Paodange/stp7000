using Mgi.Instrument.ALM.Device;
using Mgi.Instrument.ALM.Util;
using Mgi.Robot.Cantroller;
using System.Threading;

namespace Mgi.Instrument.ALM.Action
{
    internal abstract class ALMAction
    {
        public string RawCommand { get; }
        public ActionType ActionType { get; set; }

        public ALMAction(string rawCommand)
        {
            RawCommand = rawCommand;
            ResolveRawCommand();
        }

        protected abstract void ResolveRawCommand();
        public abstract void Execute(CanBasedDevice device);

    }

    internal class AxisMoveAction : ALMAction
    {
        public string AxisName { get; set; }
        public MoveType MoveType { get; set; }
        public int Pulse { get; set; }
        public bool SoftLimitCheck { get; set; }
        public bool CheckDeviation { get; set; }
        // Move PZ1,
        public AxisMoveAction(string rawCommand) : base(rawCommand)
        {

        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }

        public override void Execute(CanBasedDevice device)
        {
            if (SoftLimitCheck)
            {
                device.Axises[AxisName].MoveWithCheck(Pulse, MoveType);
            }
            else
            {
                device.Axises[AxisName].Move(Pulse, MoveType);
            }
        }
    }

    internal class AxisMoveBeginAction : ALMAction
    {
        public bool SoftLimitCheck { get; set; }
        public string AxisName { get; set; }
        public MoveType MoveType { get; set; }
        public int Pulse { get; set; }

        public AxisMoveBeginAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            device.Axises[AxisName].MoveBegin(Pulse, MoveType);
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class AxisMoveEndAction : ALMAction
    {
        public string AxisName { get; set; }
        public bool CheckDeviation { get; set; }
        public AxisMoveEndAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            if (CheckDeviation)
            {
                device.Axises[AxisName].MoveEndWithCheck();
            }
            else
            {
                device.Axises[AxisName].MoveEnd();
            }
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class AxisMoveFriendlyAction : ALMAction
    {
        public bool SoftLimitCheck { get; set; }
        public string AxisName { get; set; }
        public MoveType MoveType { get; set; }
        public int Unit { get; set; }
        public bool CheckDeviation { get; set; }
        public AxisMoveFriendlyAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            if (SoftLimitCheck)
            {
                device.Axises[AxisName].MoveFriendlyWithCheck(Unit, MoveType);
            }
            else
            {
                device.Axises[AxisName].MoveFriendly(Unit, MoveType);
            }
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class AxisMoveFriendlyBeginAction : AxisMoveFriendlyAction
    {
        public AxisMoveFriendlyBeginAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            if (SoftLimitCheck)
            {
                device.Axises[AxisName].MoveFriendlyBeginWithCheck(Unit, MoveType);
            }
            else
            {
                device.Axises[AxisName].MoveFriendlyBegin(Unit, MoveType);
            }
        }
    }

    internal class SetRateAction : ALMAction
    {
        public string AxisName { get; set; }
        public int Rate { get; set; }
        public SetRateAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            device.Axises[AxisName].SetRate(Rate);
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class SetToDefaultRateAction : ALMAction
    {
        public string AxisName { get; set; }
        public SetToDefaultRateAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            device.Axises[AxisName].SetToDefaultRate();
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class SettingAction : ALMAction
    {
        public string AxisName { get; set; }
        public byte Type { get; set; }
        public int Value { get; set; }
        public SettingAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            device.Axises[AxisName].Setting(Type, Value);
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class SettingToDefaultAction : ALMAction
    {
        public string AxisName { get; set; }
        public byte Type { get; set; }
        public SettingToDefaultAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            device.Axises[AxisName].SettingToDefault(Type);
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class ThreadSleepAction : ALMAction
    {
        public int Milliseconds { get; set; }
        public ThreadSleepAction(string rawCommand) : base(rawCommand)
        {

        }
        public override void Execute(CanBasedDevice device)
        {
            Thread.Sleep(Milliseconds);
        }

        protected override void ResolveRawCommand()
        {
            throw new System.NotImplementedException();
        }
    }

    internal enum ActionType
    {
        Move,
        MoveBegin,
        MoveMoveEnd,
        MoveFriendly,
        MoveFriendlyBegin,
        SetRate,
        SetToDefaultRate,
        Setting,
        SettingToDefault,
        ThreadSleep
    }
}
