using System.Text;

namespace Mgi.Robot.Cantroller.Axis
{

    static class AxisExtension
    {


        /// <summary>
        /// 返回 请求轴，请求帧、向应帧的格式化信息.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="request"></param>
        /// <param name="resposne"></param>
        /// <returns></returns>
        public static string FormatInformation(this ISp200Axis axis, AxisRequestFrame request,
                                                        AxisResponseFrame resposne)
        {
            var buff = new StringBuilder(128);
            return buff.AppendFormat("Axis:{0} ", axis.Name)
                       .AppendFormat("{0} ", request.ToReadableString())
                       .Append(resposne.ToReadableString())
                       .ToString();
        }

        /// <summary>
        /// 返回 请求轴，请求帧的格式化信息.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="request"></param>
        /// <param name="resposne"></param>
        /// <returns></returns>
        public static string FormatInformation(this ISp200Axis axis, AxisRequestFrame request)
        {
            var buff = new StringBuilder(128);
            return buff.AppendFormat("Axis:{0} ", axis.No)
                       .AppendFormat("{0} ", request.ToReadableString())
                       .ToString();
        }

        ///// <summary>
        ///// 按照毫米每秒设置速度。（*只接受毫米每秒)
        ///// [阻塞操作]
        ///// </summary>
        ///// <param name="speed"></param>
        //public static void SetSpeedFriendly(this ISp200Axis axis, double speed, double stroke)
        //{
        //    var microSteps = axis.Getting(140);
        //    var pulseDiv = axis.Getting(154);
        //    var pps =
        //    var rate = speed.SpeedToAxisRate(stroke, pulseDiv, microSteps);
        //    axis.SetRate(rate);
        //}

        //public static void SetDosingFriendly(this ISp200Axis axis, double speed, double stroke)
        //{
        //    var microSteps = axis.Getting(140);
        //    var pulseDiv = axis.Getting(154);
        //    var rate = speed.VolRateToAxisRate(stroke, pulseDiv, microSteps);
        //    axis.SetRate(rate);
        //}
    }
}
