using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Instrument.ALM.Attr
{
    /// <summary>
    /// 指示一个方法不需要被拦截
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class NonInterceptorAttribute : Attribute
    {
    }
}
