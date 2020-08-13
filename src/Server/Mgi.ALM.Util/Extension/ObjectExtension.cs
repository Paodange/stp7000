using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Mgi.ALM.Util.Extension
{
    public static class ObjectExtension
    {
        private static readonly char[] _lib = new char[] {'A','B','C','D','E','F','G','H','I','J','K','L','M',
                                                          'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                                                          '1','2','3','4','5','6','7','8','9','a','b','c','d',
                                                          'e','f','g','h','i','j','k','m','n','q','r','t'};
        private static Random _rand = new Random();
        /// <summary>
        /// 在指定的最大时间(max)内，查看condiitions是否全部满足。如果全部满足，则返回true。
        /// 调用此函数的线程将会阻塞，睡眠interval的时间间隔，轮询conditons是否满足。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="conditions"></param>
        /// <param name="max"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static bool WaitForEnd(this object obj, ref bool[] conditions, TimeSpan max, TimeSpan interval)
        {
            var duration = TimeSpan.FromSeconds(0);
            while (conditions.Count(b => b == true) != conditions.Length)
            {
                System.Threading.Thread.Sleep(interval);
                duration += interval;
                if (duration > max)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool WaitForEnd(this object obj, ref bool condition, TimeSpan max, TimeSpan interval)
        {
            var duration = TimeSpan.FromSeconds(0);
            while (!condition)
            {
                System.Threading.Thread.Sleep(interval);
                duration += interval;
                if (duration > max)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 在指定的最大时间(max)内，查看condiitions是否全部满足。如果全部满足，则返回true。
        /// 调用此函数的线程将会阻塞，睡眠interval的时间间隔，轮询conditons是否满足。
        /// 在轮询过程中，将会调用action操作。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="conditions"></param>
        /// <param name="max"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool WaitForEnd(this object obj, ref bool[] conditions,
                                        TimeSpan max,
                                        TimeSpan interval,
                                        Action action)
        {
            var duration = TimeSpan.FromSeconds(0);
            while (conditions.Count(b => b == true) != conditions.Length)
            {
                System.Threading.Thread.Sleep(interval);
                action.Invoke();
                duration += interval;
                if (duration > max)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 等待condition结果为true，才返回。否则一直等待，直到等待时间超过max。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="max"></param>
        /// <param name="interval"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static bool WaitForEnd(this object obj, TimeSpan max, TimeSpan interval,
                                        Func<bool> condition)
        {
            var duration = TimeSpan.FromSeconds(0);
            while (!condition.Invoke())
            {
                System.Threading.Thread.Sleep(interval);
                duration += interval;
                if (duration > max)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool WaitForEnd<T>(this T obj, Func<T, bool> condition, int maxMillSeconds, int intervalMillSeconds)
        {
            var beginTime = DateTime.Now;
            while (!condition(obj))
            {
                Thread.Sleep(intervalMillSeconds);
                if ((DateTime.Now - beginTime).TotalMilliseconds > maxMillSeconds)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 将对象转换为JSON格式串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonString(this object obj)
        {
            if (obj == null) return string.Empty;
            if (obj is IEnumerable em && !em.GetEnumerator().MoveNext())
            {
                return "[]";
            }
            var setting = new JsonSerializerSettings()
            {
                Error = (o, e) =>
                {
                    e.ErrorContext.Handled = true;
                },
                ContractResolver = new CustomJsonResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(obj, setting);
        }

        public static byte[] BinarySerialize(this object obj)
        {
            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj); ;
                return ms.ToArray();
            }
        }

        public static T FromJsonString<T>(this T obj, string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        public static StringBuilder ToReflectStrint(this object obj)
        {
            var buffer = new StringBuilder(1024);
            if (obj is string)
            {
                buffer.Append(obj);
                return buffer;
            }
            else
            {
                obj.GetType()
                    .GetRuntimeProperties()
                    .ToList()
                    .ForEach(p => buffer.AppendLine($"{p.Name}={p.GetValue(obj)}"));
            }
            return buffer;
        }

        /// <summary>
        /// 返回随机长度的船
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(this object obj, int length)
        {
            var builder = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                builder.Append(_lib[_rand.Next(0, _lib.Length)]);
            return builder.ToString();
        }

        public static int RandomInt(this object obj, int min, int max)
        {
            return _rand.Next(min, max);
        }

        public static Func<int> RandInt(this object obj, int min, int max)
        {
            return () => _rand.Next(min, max);
        }

        public static double RandomDouble(this object obj, int min, int max)
        {
            var target = 0.0;
            do
            {
                target = _rand.NextDouble() * max;
            } while (target < min);
            return target;
        }

        /// <summary>
        /// 一般用来重载Override ToString
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static string ToVisibleString(this object obj, int buff)
        {
            var buffer = new StringBuilder(buff);
            obj.GetType()
                    .GetRuntimeProperties()
                    .ToList()
                    .ForEach(p => buffer.AppendFormat("{0}:{1}\t", p.Name, p.GetValue(obj)));

            return buffer.ToString();
        }

        /// <summary>
        /// Md5(string)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Md5Hash(this string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null) return null;
            if (obj is IEnumerable) throw new NotSupportedException("-0xFF[530264]0xEE-ToDictionary does not support array or collections");
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(obj));
        }

        public static string ToJson(this object obj)
        {
            return JsonUtil.Serialize(obj);
        }
    }

    class CustomJsonResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = instance =>
            {
                if (member is PropertyInfo p)
                {
                    if (instance is Exception && p.Name == "Data")
                    {
                        return false;
                    }
                    try
                    {
                        p.GetValue(instance, null);
                        return true;
                    }
                    catch
                    {
                    }
                }
                return false;
            };

            return property;
        }
    }
}
