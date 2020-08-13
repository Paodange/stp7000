using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Mgi.Robot.Cantroller.Axis
{
    /// <summary>
    /// 轴/电机的响应帧 * CAN专用
    /// When using CAN bus, the first byte (reply address) and the last byte (checksum) are left out.
    /// </summary>
    /// <remarks>
    /// 1  Reply address
    /// 1  Module address
    /// 1  Status(e.g. 100 means no error)
    /// 1  Command number
    /// 4  Value(MSB first!)
    /// 1  Checksum
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct AxisResponseFrame
    {
        public const byte FrameSize = 8;
        public byte RequestId;
        public byte TargetAddress;
        public byte FrameId;
        public byte Status;
        public byte InstructionNo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Data;
    }



    public static class AxisResponseFrameExtension
    {
        public readonly static
            IReadOnlyDictionary<int, Tuple<string, string>> ResponseCode = new Dictionary<int, Tuple<string, string>>
        {
            //Code  Meaning
            { 100,  new Tuple<string, string>(@"Code", @"Successfully executed, no error") },
            { 101,  new Tuple<string, string>(@"Code", @"Command loaded into TMCL") },
            //
            { 1,  new Tuple<string, string>(@"program EEPROM", @"Wrong checksum") },
            { 2,  new Tuple<string, string>(@"program EEPROM", @"Invalid command") },
            { 3,  new Tuple<string, string>(@"program EEPROM", @"Wrong type") },
            { 4,  new Tuple<string, string>(@"program EEPROM", @"Invalid value") },
            { 5,  new Tuple<string, string>(@"program EEPROM", @"Configuration EEPROM locked") },
            { 6,  new Tuple<string, string>(@"program EEPROM", @"Command not available") },
        };
    }

    public static class AxisResponseFrameFactory
    {
        /// <summary>
        /// 使用指定内容填充AxisResponseFrame
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static AxisResponseFrame CreateFromContent(byte[] content)
        {
            if (content.Length != AxisResponseFrame.FrameSize)
                throw new ArgumentException($"-0xFF[1314656]0xEE-content.Length != {AxisResponseFrame.FrameSize}");

            return new AxisResponseFrame()
            {
                TargetAddress = 0x00,
                FrameId = content[0],
                Status = content[1],
                InstructionNo = content[2],
                Data = new byte[] { content[3], content[4], content[5], content[6] },
                RequestId = content[7]
            };
        }
    }
}
