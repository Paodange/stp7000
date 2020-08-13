using System;
using System.Linq;

namespace Mgi.Epson.Robot.T3.Extentions
{
    public static class ObjectExtention
    {
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
    }
}
