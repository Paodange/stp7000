using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Mgi.ALM.IOBoard;
using Mgi.Instrument.ALM.Attr;
using Mgi.Instrument.ALM.Util;

namespace Mgi.Instrument.ALM.Device
{
    [IgnoreWorkflowStatus]
    internal abstract class AbstractIOBoard : AbstractDevice, IALMIOBoard
    {
        private volatile bool monitorStarted = false;
        public override ALMDeviceType DeviceType => ALMDeviceType.IOBoard;
        protected IOnboardHardware Onboard { get; private set; }
        protected IWorkflowManager WorkflowManager { get; }
        protected ILog Log { get; }
        public bool Simulated { get; }
        public AbstractIOBoard(IConfigProvider configProvider, IWorkflowManager workflowManager, ILog log, bool simulated)
            : base(configProvider)
        {
            WorkflowManager = workflowManager;
            Log = log;
            Simulated = simulated;
            InitializeOrder = 0;
            InitialComponents();
        }
        protected virtual void InitialComponents()
        {
            if (Simulated)
            {
                Onboard = ProxyGenerator.CreateInterfaceProxyWithTarget<IOnboardHardware>(new SimulatedBoard(),
                     new DeviceCommandInterceptor($"SimulatedIOBoard", WorkflowManager, Log));
            }
            else
            {
                Onboard = ProxyGenerator.CreateInterfaceProxyWithTarget<IOnboardHardware>(new Sp200Onboard(ConfigProvider.GetIOBoardConfig().PortConfig),
                     new DeviceCommandInterceptor($"IOBoard", WorkflowManager, Log));
            }
        }
        public override void Close()
        {
            monitorStarted = false;
            Thread.Sleep(1000);
            Onboard.Close();
        }
        private bool opened = false;
        private bool initialized = false;
        public override void Initialize()
        {
            if (!opened)
            {
                Onboard.Open();
                opened = true;
            }
            if (!initialized)
            {
                Onboard.Initialize();
                initialized = true;
            }
            Idle();
            monitorStarted = true;
            DoorState lastState = DoorState.Unknow;
            Task.Run(() =>
            {
                while (monitorStarted)
                {
                    var state = GetFrontDoorState();
                    if (state != lastState)
                    {
                        OnDoorStateChanged(new DoorStateChangedEventArgs(state));
                        lastState = state;
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        public bool IfLinghtOn { get; private set; }
        public bool IsLaminarHoodOn { get; private set; }
        public bool IsDoorLocked { get; private set; }
        public bool IsSterilampOn { get; private set; }
        public bool IsBuzzerOn { get; private set; }

        #region 
        private enum SwitchStates : byte
        {
            SWITCH_CLOSE = 0x00,
            SWITCH_OPEN = 0x01,
        };
        private enum CommandCode : byte
        {
            LAMINAR_HOOD = 0x01,
            SAFETY_LOCK = 0x02,
            FLOODLIGHT = 0x03,
            STERILAMP = 0x04,
            LED_LIGHT = 0x05,
            FRONT_DOOR_SWITCH = 0x06,
            PCR_ELECTRIC_SOURCE = 0x07,
            BUZZER = 0x08,
        };

        #endregion

        /// <summary>
        /// 打开层流罩
        /// </summary>
        public void OpenLaminarHood()
        {
            Onboard.GpioOut((uint)CommandCode.LAMINAR_HOOD, (uint)SwitchStates.SWITCH_OPEN);
            IsLaminarHoodOn = true;
        }

        /// <summary>
        /// 关闭层流罩
        /// </summary>
        public void CloseLaminarHood()
        {
            Onboard.GpioOut((uint)CommandCode.LAMINAR_HOOD, (uint)SwitchStates.SWITCH_CLOSE);
            IsLaminarHoodOn = false;
        }

        /// <summary>
        /// 设置下层流罩速度
        /// </summary>
        /// <param name="speed"></param>
        public void HoodSpeed(int speed)
        {
            Onboard.HoodSpeed(speed);
        }

        public string GetVersion()
        {
            return Onboard.GetVersion();
        }

        public int GetVolume()
        {
            return Onboard.GetVolume();
        }

        /// <summary>
        /// 打开安全锁
        /// </summary>
        public void OpenSafetyLock()
        {
            Onboard.GpioOut((uint)CommandCode.SAFETY_LOCK, (uint)SwitchStates.SWITCH_OPEN);
            IsDoorLocked = false;
        }

        /// <summary>
        /// 关闭安全锁
        /// </summary>
        public void Lock()
        {
            Onboard.GpioOut((uint)CommandCode.SAFETY_LOCK, (uint)SwitchStates.SWITCH_CLOSE);
            IsDoorLocked = true;
        }

        /// <summary>
        /// 打开照明灯
        /// </summary>
        public void OpenFloodLight()
        {
            Onboard.GpioOut((uint)CommandCode.FLOODLIGHT, (uint)SwitchStates.SWITCH_OPEN);
            IfLinghtOn = true;
        }

        /// <summary>
        /// 关闭照明灯
        /// </summary>
        public void CloseFloodLight()
        {
            Onboard.GpioOut((uint)CommandCode.FLOODLIGHT, (uint)SwitchStates.SWITCH_CLOSE);
            IfLinghtOn = false;
        }

        /// <summary>
        /// 打开灭菌灯
        /// </summary>
        public void OpenSterilamp()
        {
            Onboard.GpioOut((uint)CommandCode.STERILAMP, (uint)SwitchStates.SWITCH_OPEN);
            IsSterilampOn = true;
        }

        /// <summary>
        /// 关闭灭菌灯
        /// </summary>
        public void CloseSterilamp()
        {
            Onboard.GpioOut((uint)CommandCode.STERILAMP, (uint)SwitchStates.SWITCH_CLOSE);
            IsSterilampOn = false;
        }

        /// <summary>
        /// 获取前面门的开关状态
        /// </summary>
        /// <returns></returns>
        public virtual DoorState GetFrontDoorState()
        {
            return Onboard.GpioIn((uint)CommandCode.FRONT_DOOR_SWITCH) == 1 ? DoorState.Closed : DoorState.Open;
        }

        /// <summary>
        /// 打开蜂鸣器
        /// </summary>
        public void OpenBuzzer()
        {
            Onboard.FlashBuzzer(2000, 0);
            IsBuzzerOn = true;
        }

        /// <summary>
        /// 关闭蜂鸣器
        /// </summary>
        public void CloseBuzzer()
        {
            Onboard.FlashBuzzer(0, 1000);
            IsBuzzerOn = false;
        }

        /// <summary>
        /// 待机 紫外灯关闭 蜂鸣器关闭 灯带置为蓝色  照明灯打开 安全锁打开
        /// </summary>
        public void Idle()
        {
            CloseSterilamp();
            CloseBuzzer();
            OpenLaminarHood();
            OpenFloodLight();
            OpenSafetyLock();
            SetLedColor(LedColor.Green);
        }

        /// <summary>
        /// IO板错误发生 1.紫外灯关闭 2.灯带置为红色 3.蜂鸣器打开 （策略-透明） 5.照明灯打开 6.	安全锁打开
        /// </summary>
        public void Error()
        {
            CloseSterilamp();
            OpenFloodLight();
            OpenSafetyLock();
            SetLedColor(LedColor.Red);
            OpenBuzzer();
        }

        /// <summary>
        /// IO板警告发生
        /// </summary>
        public void Warn()
        {
            CloseSterilamp();
            OpenFloodLight();
            OpenSafetyLock();
            SetLedColor(LedColor.Yellow);
            OpenBuzzer();
        }

        public void Run()
        {
            Lock();
            CloseSterilamp();
            CloseBuzzer();
            OpenLaminarHood();
            OpenFloodLight();
            SetLedColor(LedColor.Green);
        }

        public void SetLedColor(LedColor color)
        {
            switch (color)
            {
                case LedColor.Green:
                    Onboard.Color(0, 128, 0);
                    break;
                case LedColor.Yellow:
                    Onboard.Color(255, 255, 0);
                    break;
                case LedColor.Red:
                    Onboard.Color(15, 0, 0);
                    break;
                default:
                    break;
            }
        }

        public event EventHandler<DoorStateChangedEventArgs> DoorStateChanged;

        protected virtual void OnDoorStateChanged(DoorStateChangedEventArgs e)
        {
            DoorStateChanged?.Invoke(this, e);
        }
    }
}
