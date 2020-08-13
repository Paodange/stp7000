using System.Reflection;
using System.Text;

namespace Mgi.Epson.Robot.T3.Protocol
{
    public class EpsonRequest
    {
        private static readonly Encoding MESSAGE_ENCODING = Encoding.ASCII;
        /// <summary>
        /// 消息头
        /// </summary>
        private const string MESSAGE_HEAD = "#head";
        /// <summary>
        /// 消息结束符
        /// </summary>
        private const string MESSAGE_TAIL = "#tail";

        public FunctionType Type { get; set; }

        public virtual byte[] GetBytes()
        {
            string message = MESSAGE_HEAD + "[1]" + Type.ToString() + ":";
            foreach (var p in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                message += p.GetValue(this)?.ToString() + ",";
            }
            if (message.EndsWith(","))
            {
                message = message.Substring(0, message.Length - 1);
            }
            message += MESSAGE_TAIL;
            return MESSAGE_ENCODING.GetBytes(message);
        }
    }
}
