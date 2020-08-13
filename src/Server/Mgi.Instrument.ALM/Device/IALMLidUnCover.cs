using System.Collections.Generic;
using Mgi.Instrument.ALM.Attr;

namespace Mgi.Instrument.ALM.Device
{
    /// <summary>
    /// 拔盖器接口
    /// </summary>
    public interface IALMLidUnCover : IALMDevice
    {
        /// <summary>
        /// 拔盖工作单元集合
        /// </summary>
        Dictionary<LidUncoverUnits, LidUncoverUnit> WorkUnits { get; }
        /// <summary>
        /// 拔盖 并扫码 返回扫到的条码  一个通道可以配两个条码枪，同时扫两个
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="lidUncover"></param>
        /// <param name="gripper"></param>
        /// <returns></returns>
        IEnumerable<string> UnCoverAndScan(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A, LidUncoverGripper gripper = LidUncoverGripper.Both);

        /// <summary>
        /// 盖回去
        /// </summary>
        /// <param name="tubeId">试管id</param>
        /// <param name="lidUncover">工作单元</param>
        void Cover(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A);

        /// <summary>
        /// 导轨移动
        /// </summary>
        /// <param name="lidUncover"></param>
        /// <param name="position"></param>
        [ErrorOperation(ErrorOperation.Abort | ErrorOperation.Retry)]
        void SliderMoveTo(LidUncoverUnits lidUncover, SliderPositionEnum position);

        /// <summary>
        ///  夹紧
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="lidUncover"></param>
        [ErrorOperation(ErrorOperation.Abort | ErrorOperation.Retry)]
        void TightenCAxis(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A);

        /// <summary>
        /// 松开
        /// </summary>
        [ErrorOperation(ErrorOperation.Abort | ErrorOperation.Retry)]
        void ReleaseCAxis(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A, LidUncoverCReleaseLevel level = LidUncoverCReleaseLevel.ForGrasp);

        /// <summary>
        /// 断言指定滑块的指定试管位置的状态为空或者占用 断言失败 则抛出异常
        /// </summary>
        /// <param name="lidUncover"></param>
        /// <param name="pos"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void AssertSamplePosState(LidUncoverUnits lidUncover, SliderSamplePos pos, SliderSamplePosState state);

        /// <summary>
        /// 断言指定滑块的指定试管位置的状态为空或者占用 断言成功返回true 断言失败返回false
        /// </summary>
        /// <param name="lidUncover"></param>
        /// <param name="pos"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        bool AssertSamplePosStateBool(LidUncoverUnits lidUncover, SliderSamplePos pos, SliderSamplePosState state);

        /// <summary>
        /// R轴Home 后转90度 
        /// </summary>
        void ResetRAxis(LidUncoverUnits lidUncover);

        /// <summary>
        /// 夹紧所有夹爪 在开盖子之前先夹紧再松开  为了提高速度，提出松开夹爪步骤用于外部流程并行
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="lidUncover"></param>
        void TightenGrippers(string tubeId, LidUncoverUnits lidUncover = LidUncoverUnits.A);
    }
}
