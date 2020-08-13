using System;
using System.Runtime.InteropServices;

namespace Mgi.Robot.Cantroller.Can
{
    #region Constant

    public enum EnumInterfaceType
    {
        //接口卡类型定义
        VCI_PCI5121 = 1,
        VCI_PCI9810 = 2,
        VCI_USBCAN1 = 3,
        VCI_USBCAN2 = 4,
        VCI_USBCAN2A = 4,
        VCI_PCI9820 = 5,
        VCI_CAN232 = 6,
        VCI_PCI5110 = 7,
        VCI_CANLITE = 8,
        VCI_ISA9620 = 9,
        VCI_ISA5420 = 10,
        VCI_PC104CAN = 11,
        VCI_CANETUDP = 12,
        VCI_CANETE = 12,
        VCI_DNP9810 = 13,
        VCI_PCI9840 = 14,
        VCI_PC104CAN2 = 15,
        VCI_PCI9820I = 16,
        VCI_CANETTCP = 17,
        VCI_PEC9920 = 18,
        VCI_PCIE_9220 = 18,
        VCI_PCI5010U = 19,
        VCI_USBCAN_E_U = 20,
        VCI_USBCAN_2E_U = 21,
        VCI_PCI5020U = 22,
        VCI_EG20T_CAN = 23,
        VCI_PCIE9221 = 24,
        VCI_WIFICAN_TCP = 25,
        VCI_WIFICAN_UDP = 26,
        VCI_PCIe9120 = 27,
        VCI_PCIe9110 = 28,
        VCI_PCIe9140 = 29
    }


    public enum EnumErrorCode
    {
        //CAN错误码
        ERR_CAN_OVERFLOW = 0x0001,	//CAN控制器内部FIFO溢出

        ERR_CAN_ERRALARM = 0x0002,	//CAN控制器错误报警

        ERR_CAN_PASSIVE = 0x0004,	//CAN控制器消极错误

        ERR_CAN_LOSE = 0x0008,	//CAN控制器仲裁丢失

        ERR_CAN_BUSERR = 0x0010,	//CAN控制器总线错误
        ERR_CAN_BUSOFF = 0x0020,    //总线关闭错误
        //通用错误码
        ERR_DEVICEOPENED = 0x0100,	//设备已经打开

        ERR_DEVICEOPEN = 0x0200,	//打开设备错误

        ERR_DEVICENOTOPEN = 0x0400,	//设备没有打开

        ERR_BUFFEROVERFLOW = 0x0800,	//缓冲区溢出

        ERR_DEVICENOTEXIST = 0x1000,	//此设备不存在

        ERR_LOADKERNELDLL = 0x2000,	//装载动态库失败
        ERR_CMDFAILED = 0x4000,	//执行命令失败错误码

        ERR_BUFFERCREATE = 0x8000	//内存不足
    }

    public enum EnumFunctionReturn
    {
        STATUS_OK = 1,
        STATUS_ERR = 0
        //CMD_DESIP = 0,
        // CMD_DESPORT			1
        // CMD_CHGDESIPANDPORT		2
        // CMD_SRCPORT			2		
        // CMD_TCP_TYPE		4					//tcp 工作方式，服务器:1 或是客户端:0
        // TCP_CLIENT			0
        // TCP_SERVER			1
        ////服务器方式下有效
        // CMD_CLIENT_COUNT    5					//连接上的客户端计数
        // CMD_CLIENT			6					//连接上的客户端
        // CMD_DISCONN_CLINET  7					//断开一个连接
        // CMD_SET_RECONNECT_TIME 8			//使能自动重连
    }
    #endregion

    #region Struct mapping

    //1.ZLGCAN系列接口卡信息的数据类型。
    [StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VciBoardInfo  //VCI_BOARD_INFO
    {
        ushort hw_Version;
        ushort fw_Version;
        ushort dr_Version;
        ushort in_Version;
        ushort irq_Num;
        byte can_Num;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        string str_Serial_Num;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        string str_hw_Type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        ushort Reserved;
    }

    /// <summary>
    /// .定义CAN信息帧的数据类型。
    /// </summary>
    [StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VciCanFrame//_VCI_CAN_OBJ
    {
        public uint ID;
        public uint TimeStamp;
        public byte TimeFlag;
        public byte SendType;
        public byte RemoteFlag;//是否是远程帧
        public byte ExternFlag;//是否是扩展帧
        public byte DataLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Reserved;
    }

    /// <summary>
    /// 定义CAN控制器状态的数据类型。
    /// </summary>
    [StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VciCanStatus//_VCI_CAN_STATUS
    {
        char ErrInterrupt;
        char regMode;
        char regStatus;
        char regALCapture;
        char regECCapture;
        char regEWLimit;
        char regRECounter;
        char regTECounter;
        uint Reserved;
    }

    /// <summary>
    /// 定义错误信息的数据类型。
    /// </summary>
    [StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VciError//_VCI_ERR_INFO
    {
        public uint ErrCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Passive_ErrData;
        public byte ArLost_ErrData;
    }


    /// <summary>
    /// 定义初始化CAN的数据类型
    /// </summary>
    [StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VciInitConfig//VCI_INIT_CONFIG
    {
        public uint AccCode;
        public uint AccMask;
        public uint Reserved;
        public byte Filter;
        public byte Timing0;
        public byte Timing1;
        public byte Mode;
    }



    /// <summary>
    /// new add struct for filter
    /// </summary>
    [StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VciFilterRecord//_VCI_FILTER_RECORD
    {
        uint ExtFrame; //是否为扩展帧
        uint Start;
        uint End;
    }


    /// <summary>
    /// 定时自动发送帧结构 
    /// </summary>
    [StructLayout(layoutKind: LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VciAutoSendFrame//_VCI_AUTO_SEND_OBJ
    {
        byte Enable;//使能本条报文.  0：禁能   1：使能
        byte Index;  //报文编号.   最大支持32条报文
        uint Interval;//定时发送时间。1ms为单位

        /// <summary>
        /// 报文 (VCI_CAN_OBJ obj;)
        /// </summary>
        VciCanFrame obj;
    }
    //VCI_AUTO_SEND_OBJ,* PVCI_AUTO_SEND_OBJ;

    #endregion    

    #region function mapping

    public static class CanController
    {
        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_OpenDevice", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint OpenDevice(uint DeviceType, uint DeviceInd, uint Reserved);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_CloseDevice", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint CloseDevice(uint DeviceType, uint DeviceInd);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_InitCAN", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint InitCAN(uint DeviceType, uint DeviceInd, uint CANInd, ref VciInitConfig pInitConfig);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_ReadBoardInfo", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint ReadBoardInfo(uint DeviceType, uint DeviceInd, ref VciBoardInfo pInfo);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_ReadErrInfo", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint ReadErrInfo(uint DeviceType, uint DeviceInd, uint CANInd, ref VciError pErrInfo);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_ReadCANStatus", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint ReadCANStatus(uint DeviceType, uint DeviceInd, uint CANInd, ref VciCanStatus pCANStatus);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_GetReference", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint GetReference(uint DeviceType, uint DeviceInd, uint CANInd, uint RefType, out IntPtr pData);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_SetReference", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint SetReference(uint DeviceType, uint DeviceInd, uint CANInd, uint RefType, out IntPtr pData);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_GetReceiveNum", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint GetReceiveNum(uint DeviceType, uint DeviceInd, uint CANInd);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_ClearBuffer", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint ClearBuffer(uint DeviceType, uint DeviceInd, uint CANInd);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_StartCAN", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint StartCAN(uint DeviceType, uint DeviceInd, uint CANInd);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_StartCAN", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static uint ResetCAN(uint DeviceType, uint DeviceInd, uint CANInd);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_Transmit", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static ulong Transmit(uint DeviceType, uint DeviceInd, uint CANInd, ref VciCanFrame[] pSend, ulong Len);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_Transmit", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VciCanFrame pSend, UInt32 Len);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_Receive", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static ulong Receive(uint DeviceType, uint DeviceInd, uint CANInd, ref VciCanFrame[] pReceive, UInt32 Len, int WaitTime = -1);

        [DllImport("3lib/ControlCAN.dll", EntryPoint = "VCI_Receive", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public extern static ulong Receive(uint DeviceType, uint DeviceInd, uint CANInd, IntPtr pReceive, UInt32 Len, int WaitTime = -1);
    }

    #endregion  
}
