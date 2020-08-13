namespace Mgi.Barcode.Leuze
{
    /// <summary>
    /// 条码枪接口
    /// </summary>
    public interface ILeuzeBarcode
    {
        /// <summary>
        /// 查询版本
        /// </summary>
        /// <returns></returns>
        string GetVersion();
        /// <summary>
        /// 单次扫码
        /// </summary>
        /// <returns></returns>
        string SingleTrigger();
        /// <summary>
        /// 连续扫码
        /// </summary>
        string ConsequentTrigger();
        /// <summary>
        /// 开启连续扫码 并立即返回
        /// </summary>
        void BeginConsequentTrigger();
        /// <summary>
        /// 关闭连续扫码 并返回扫码结果
        /// </summary>
        /// <returns></returns>
        string EndConsequentTrigger();



        void Open();
        void Close();

    }
}
