namespace Mgi.Robot.Cantroller.Can
{
    public static class VciCanFrameExtension
    {
        /// <summary>
        /// 在Vci帧中提取协议体
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static byte[] ExtractBody(this VciCanFrame frame)
        {
            return frame.Data.Clone() as byte[];
        }
    }
}
