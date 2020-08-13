using ModbusTCP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Mgi.Gripper.Zimma
{
    public class ModbusGripper : IZimmaGripper
    {
        const int RELEASE_TIMEOUT = 5 * 1000;
        // 相同ip和端口时  使用同一个master对象
        readonly static Dictionary<IPPortNode, Master> masters
            = new Dictionary<IPPortNode, Master>();
        private volatile bool initialized = false;
        private static readonly object syncObj = new object();
        private Master master;
        private const ushort WRITE_ID = 8;
        private const ushort READ_ID = 3;
        private const byte UNIT = 0;
        public ModbusGripperConfig Config { get; }
        public string Name { get; set; } = "";
        public string IP
        {
            get
            {
                return Config.IP;
            }
        }
        public ushort Port
        {
            get
            {
                return Config.Port;
            }
        }
        public ushort WriteChannelId
        {
            get
            {
                return Config.WriteChannelId;
            }
        }
        public ushort ReadChannelId
        {
            get
            {
                return Config.ReadChannelId;
            }
        }

        public ModbusGripper(ModbusGripperConfig config)
        {
            Config = config;
        }

        public void Initialize()
        {
            lock (syncObj)
            {
                if (initialized) return;
                var node = new IPPortNode() { IP = IP, Port = Port };
                if (master == null)
                {
                    if (masters.ContainsKey(node))
                    {
                        master = masters[node];
                    }
                    else
                    {
                        master = new Master(IP, Port);
                        masters.Add(node, master);
                    }
                }
                HandShake(1, 0, true);
                try
                {
                    ReleaseGripper(); //block后重置错误第一次松开夹爪时  会报0x307错误  再松开一次就正常
                }
                catch (GripperException ex) when (ex.ErrorCode == 0x307)
                {
                    ReleaseGripper();
                }
                initialized = true;
                Task.Run(() =>
                {
                    while (initialized)
                    {
                        ReadResponse();
                        Thread.Sleep(5000);
                    }
                });
            }
        }

        public void Close()
        {
            if (master != null)
            {
                master.disconnect();
                master.Dispose();
            }
            initialized = false;
        }
        public int ReleaseGripper()
        {
            BeginRelease();
            return EndRelease();
        }

        public int TightenGripper(byte gripForce, ushort teachPosition, byte positionTolerance, bool throwIfEmtpyGrasp)
        {
            BeginTighten(gripForce, teachPosition);
            return EndTighten(teachPosition, positionTolerance, throwIfEmtpyGrasp);
        }

        public void BeginTighten(byte gripForce, ushort teachPosition)
        {
            var req = HandShake(gripForce, teachPosition);
            req.ControlWord = 512;
            SendRequest(req);
        }

        public int EndTighten(ushort teachPosition, byte positionTolerance, bool throwIfEmptyGrasp)
        {
            DateTime d = DateTime.Now;
            while (true)
            {
                var response = ReadAndCheckResponse();
                if ((DateTime.Now - d).TotalMilliseconds > RELEASE_TIMEOUT)
                {
                    throw new GripperException(0x10001, $"{Name} Tighten gripper timeout");
                }

                if (response.ActualPosition > teachPosition + positionTolerance)
                {
                    if (Config.ToleranceCheck && throwIfEmptyGrasp)
                    {
                        throw new EmptyGraspException($"{Name} Over PositionTolerance, Actual Position:{response.ActualPosition},TeachPosition:{teachPosition},PositionTolerance:{positionTolerance}");
                    }
                    if (Config.GraspDelay > 0)
                    {
                        Thread.Sleep(Config.GraspDelay);
                    }
                    response = ReadAndCheckResponse();
                    return response.ActualPosition;
                }
                if (response.TeachPosition || response.WorkPosition)
                {
                    if (Config.GraspDelay > 0)
                    {
                        Thread.Sleep(Config.GraspDelay);
                    }
                    response = ReadAndCheckResponse();
                    return response.ActualPosition;
                }
                Thread.Sleep(10);
            }
            //int lastPosition = -10000;
            //DateTime d = DateTime.Now;
            //var response = ReadAndCheckResponse();
            //while (response.ActualPosition > lastPosition)
            //{
            //    if ((DateTime.Now - d).TotalMilliseconds > RELEASE_TIMEOUT)
            //    {
            //        throw new GripperException(0x10001, "Tighten gripper timeout");
            //    }
            //    lastPosition = response.ActualPosition;
            //    Thread.Sleep(100);
            //    response = ReadAndCheckResponse();
            //}
            //if (response.ActualPosition > teachPosition + positionTolerance && throwIfEmptyGrasp)
            //{
            //    throw new EmptyGraspException($"Grasped nothing,Actual Position:{response.ActualPosition},TeachPosition:{teachPosition},PositionTolerance:{positionTolerance}");
            //}
            //return response.ActualPosition;
        }

        public void BeginRelease()
        {
            var req = HandShake(4, 0);
            req.ControlWord = 256;
            SendRequest(req);
        }

        public int EndRelease()
        {
            DateTime d = DateTime.Now;
            var response = ReadAndCheckResponse();
            while (!response.BasePosition && !response.TeachPosition)
            {
                if ((DateTime.Now - d).TotalMilliseconds > RELEASE_TIMEOUT)
                {
                    throw new GripperException(0x10001, $"{Name} Release gripper timeout");
                }
                Thread.Sleep(10);
                response = ReadAndCheckResponse();
            }
            return response.ActualPosition;
        }

        private GripperRequest HandShake(byte gripForce, ushort teachPosition, bool initialize = false)
        {
            var req = new GripperRequest()
            {
                ControlWord = 1,
                DeviceMode = 100,
                GripForce = gripForce,
                PositionTolerance = 255,
                WorkPieceNo = 0,
                TeachPosition = teachPosition
            };
            var resp = ReadResponse();
            if (resp.GripperPCLActive)
            {
                req.ControlWord = 1;
                SendRequest(req);
            }
            req.ControlWord = 0;
            SendRequest(req);
            req.ControlWord = 0x8000;  //reset error
            SendRequest(req);
            req.ControlWord = 4;
            SendRequest(req);
            req.ControlWord = 8;
            SendRequest(req);
            resp = ReadResponse();
            CheckResponse(resp);
            return req;
        }
        //        WorkpieceNo := 3; (* Recipe is to be stored as the third workpiece recipe*)
        //PositionTolerance := 50;
        //GripForce := 3;
        //TeachPosition := 500;
        private void SetGripperParam(byte workPieceNo, byte positionTolerance, byte gripForce, ushort teachPosition)
        {
            var req = new GripperRequest()
            {
                ControlWord = 1,
                WorkPieceNo = workPieceNo,
                PositionTolerance = positionTolerance,
                GripForce = gripForce,
                DeviceMode = 100,
                TeachPosition = teachPosition
            };
            SendRequest(req);
            var resp = ReadResponse();
            if (resp.DataTransferOK)
            {
                SendRequest(new GripperRequest() { ControlWord = 0 });
                resp = ReadResponse();
            }
            if (!resp.DataTransferOK)
            {
                SendRequest(new GripperRequest() { ControlWord = 2 });
                resp = ReadResponse();
            }
            if (resp.DataTransferOK)
            {
                SendRequest(new GripperRequest() { ControlWord = 0 });
                resp = ReadResponse();
            }
        }
        private string GetErrorMessage(ushort errorCode)
        {
            var message = "Unknow Error";
            if (errorDict.ContainsKey(errorCode))
            {
                message = errorDict[errorCode];
            }
            return $"Error Code:{ToHex(errorCode)}, Message:{message}";
        }
        private string ToHex(ushort errorCode)
        {
            return $"0x{errorCode:X2}";
        }
        private readonly Dictionary<ushort, string> errorDict = new Dictionary<ushort, string>()
        {
            { 0x00, "Success" },
            { 0x100, "Actuator supply is not present or is too low." },
            { 0x101, "Max permitted temperature exceeded." },
            { 0x102, "Temperature below min. permitted temperature." },
            { 0x300, "The configured \"ControlWord\" is implausible." },
            { 0x301, "The configured \"TeachPosition\" is outside the permitted range." },
            { 0x302, "The configured gripping force is outside the permitted range." },
            { 0x304, "The configured tolerance value is outside the permitted range." },
            { 0x305, "The device has an incorrect reference position." },
            { 0x306, "The configured \"DeviceMode\" is implausible." },
            { 0x307, "Movement order cannot be executed." },
            { 0x308, "\"WorkpieceNo.\" not available." },
            { 0x309, "\"TeachPosition\" not transmitted" },
            { 0x030D, "\"GripForce\" not transmitted" },
            { 0x030F, "\"TeachTolerance\" not transmitted" },
            { 0x0310, "\"DeviceMode\" not taken over" },
            { 0x0311, "\"WorkpieceNo.\" not transmitted" },
            { 0x0312, "Initial \"Handshake\" missing" },
            { 0x400, "Gripper is blocked" },
            { 0x406, "System error" }
        };
        private void SendRequest(GripperRequest request)
        {
            master.WriteMultipleRegister(WRITE_ID, UNIT, WriteChannelId, request.GetBytes());
            Thread.Sleep(50);
        }
        private GripperResponse ReadAndCheckResponse()
        {
            // 有错误
            var resp = ReadResponse();
            CheckResponse(resp);
            return resp;
        }
        private GripperResponse ReadResponse()
        {
            byte[] values = null;
            master.ReadHoldingRegister(READ_ID, UNIT, ReadChannelId, 3, ref values);
            return GripperResponse.FromBytes(values);
        }
        private void CheckResponse(GripperResponse resp)
        {
            if (resp.HasError || resp.Diagnosis != 0)
            {
                throw new GripperException(resp.Diagnosis, $"{Name} {GetErrorMessage(resp.Diagnosis)}");
            }
        }
    }

    public class GripperRequest
    {
        public ushort ControlWord { get; set; }
        public byte DeviceMode { get; set; }
        public byte WorkPieceNo { get; set; }
        public ushort TeachPosition { get; set; }
        /// <summary>
        /// 抓取力度
        /// </summary>
        public byte GripForce { get; set; }
        /// <summary>
        /// 报错阈值
        /// </summary>
        public byte PositionTolerance { get; set; }

        public GripperRequest()
        {
            ControlWord = 256;
            DeviceMode = 100;
            WorkPieceNo = 0;
            TeachPosition = 0;
            GripForce = 1;
            PositionTolerance = 255;
        }

        public byte[] GetBytes()
        {
            var data = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(BitUtil.SwapUInt16(ControlWord)), 0, data, 0, 2);
            data[2] = DeviceMode;
            data[3] = WorkPieceNo;
            Buffer.BlockCopy(BitConverter.GetBytes(BitUtil.SwapUInt16(TeachPosition)), 0, data, 4, 2);
            data[6] = GripForce;
            data[7] = PositionTolerance;
            return data;
        }
    }

    public class GripperResponse
    {
        public ushort StatusWord { get; set; }

        public ushort Diagnosis { get; set; }

        public ushort ActualPosition { get; set; }

        public bool HasError
        {
            get
            {
                return ((StatusWord >> 15) & 1) == 1;
            }
        }

        public bool ControlWord200
        {
            get
            {
                return ((StatusWord >> 14) & 1) == 1;
            }
        }

        public bool ControlWord100
        {
            get
            {
                return ((StatusWord >> 13) & 1) == 1;
            }
        }

        public bool DataTransferOK
        {
            get
            {
                return ((StatusWord >> 12) & 1) == 1;
            }
        }

        public bool IsUndefinedPosition
        {
            get
            {
                return ((StatusWord >> 11) & 1) == 1;
            }
        }

        public bool WorkPosition
        {
            get
            {
                return ((StatusWord >> 10) & 1) == 1;
            }
        }

        public bool TeachPosition
        {
            get
            {
                return ((StatusWord >> 9) & 1) == 1;
            }
        }

        public bool BasePosition
        {
            get
            {
                return ((StatusWord >> 8) & 1) == 1;
            }
        }

        public bool GripperPCLActive
        {
            get
            {
                return ((StatusWord >> 6) & 1) == 1;
            }
        }


        public static GripperResponse FromBytes(byte[] values)
        {
            if (values == null || values.Length < 6)
            {
                throw new Exception("Invalid data for Response Packet");
            }
            var p = new GripperResponse()
            {
                StatusWord = BitUtil.SwapUInt16(BitConverter.ToUInt16(values, 0)),
                Diagnosis = BitUtil.SwapUInt16(BitConverter.ToUInt16(values, 2)),
                ActualPosition = BitUtil.SwapUInt16(BitConverter.ToUInt16(values, 4)),
            };
            return p;
        }
    }

    /// <summary>
    ///  IP和端口节点
    /// </summary>
    public class IPPortNode
    {
        public string IP { get; set; }
        public ushort Port { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is IPPortNode other))
            {
                return false;
            }
            return IP == other.IP && Port == other.Port;
        }
        public override string ToString()
        {
            return $"IP:{IP},Port:{Port}";
        }

        public override int GetHashCode()
        {
            return (IP, Port).GetHashCode();
        }
    }
    public class BitUtil
    {
        public static ushort SwapUInt16(ushort inValue)
        {
            return (ushort)(((inValue & 0xff00) >> 8) |
                     ((inValue & 0x00ff) << 8));
        }
    }
}