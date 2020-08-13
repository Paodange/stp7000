using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mgi.ALM.Util.Extension
{
    /// <summary>
    /// 字符串的扩展函数
    /// </summary>
    public static class StringExtension
    {
        public static T ToDynamic<T>(this string json, T t)
        {
            try
            {
                return JsonConvert.DeserializeAnonymousType(json, t);
            }
            catch(Exception)
            {
                return default(T);
            }
            
        }

        public static T ToDynamic<T>(this IDictionary<string,object> dic, T t)
        {
            try
            {                
                return JsonConvert.DeserializeAnonymousType(dic.ToJsonString(), t);
            }
            catch (Exception)
            {
                return default(T);
            }

        }

        public static T JsonToObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static double ToDouble(this string str)
        {
            var d = 0.0;
            return double.TryParse(str, out d) ? d : -1;
        }

        public static int ToInt(this string str)
        {
            var d = 0;
            return int.TryParse(str, out d) ? d : -1;
        }

        public static string ToBase64(this string str)
        {
            return Convert.ToBase64String(Encoding.Default.GetBytes(str));
        }

    }
}
