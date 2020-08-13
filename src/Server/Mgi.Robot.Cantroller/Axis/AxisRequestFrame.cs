using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Mgi.Robot.Cantroller.Axis
{
    /// <summary>
    /// 轴/电机的请求帧 * CAN专用。其他类型需要加上校验
    /// </summary>
    /// <remarks>
    /// 4.1  Binary command format
    /// Every command has a  mnemonic and  a binary representation.  When  commands are sent from  a host to  a 
    /// module, the binary format has to be used.Every command consists of a one-byte command field, a one-byte
    /// type  field,  a one-byte motor/bank field  and a  four-byte value  field.So the  binary representation  of a
    /// command always has seven bytes.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Ansi)]
    public struct AxisRequestFrame
    {
        public byte RequestId;
        public byte TargetAddress;     
        public byte InstructionNo;
        public byte Type;
        public byte MotorOrBand;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Data;
    }
    
}
