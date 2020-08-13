using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Robot.Cantroller.Axis
{
    static class BoolExtendsion
    {
        /// <summary>
        /// 如果值为false，则抛出轴操作异常
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <exception cref="AxisOperateException">如果当前值为false</exception>
        public static void ThrowIfNotChecked(this bool value, string message)
        {
            if (!value)
                throw new AxisOperateException($"-0xFF[1316657]0xEE-{message}");
        }
    }
}
