using System.Collections.Generic;
using System.Linq;

namespace Mgi.Robot.Cantroller.Can
{
    /// <summary>
    /// CAN error info 的格式化
    /// </summary>
    public static class VciErrorExtension
    {
        public static IReadOnlyDictionary<uint, string> ErrorDict { get; private set; }

        static VciErrorExtension()
        {
            ErrorDict = new Dictionary<uint, string>()
            {
                { 0x0100, "设备已经打开" },
                { 0x0200, "打开设备错误" },
                { 0x0400, "设备没有打开" },
                { 0x0800, "缓冲区溢出" },
                { 0x1000, "此设备不存在" },
                { 0x2000, "装载动态库失败" },
                { 0x4000, "表示为执行命令失败错误" },
                { 0x8000, "内存不足" },
                { 0x0001, "CAN控制器内部FIFO溢出" },
                { 0x0002, "CAN 控制器错误报警" },
                { 0x0004, "CAN 控制器消极错误" },
                { 0x0008, "CAN 控制器仲裁丢失" },
                { 0x0010, "CAN 控制器总线错误" }
            };
        }

        public static string ToUserFriendly(this VciError err)
        {
            var target = ErrorDict.FirstOrDefault(kv => kv.Key == err.ErrCode);
            return string.Format("{0},{1}", target.Key, target.Value);
        }
    }
}
