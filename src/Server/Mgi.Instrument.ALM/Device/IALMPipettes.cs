using System.Collections.Generic;
using Mgi.Instrument.ALM.Attr;

namespace Mgi.Instrument.ALM.Device
{
    /// <summary>
    /// 移液器接口
    /// </summary>
    public interface IALMPipettes : IALMDevice
    {
        void HomeAll();
        /// <summary>
        /// 单轴复位
        /// </summary>
        /// <param name="name"></param>
        /// <param name="channel"></param>
        void HomeAxis(PipetteChannels channel, string name);
        void HomeAxis(string name);
        /// <summary>
        /// 通道集合
        /// </summary>
        Dictionary<PipetteChannels, PipetteChannel> Channels { get; }
        /// <summary>
        /// 单通道吸液
        /// </summary>
        /// <param name="tubeId"></param>
        /// <param name="channel">吸液通道</param>
        /// <param name="position"></param>
        /// <param name="volume">体积</param>
        void Aspirate(string tubeId, PipetteChannels channel, PipettePositions position, double volume = 200);
        void AspirateBoth(string tubeId, PipettePositions position, double volume = 200);
        void LoadTip(PipetteChannels channel, PipettePositions position);
        void LoadTips(PipettePositions position);
        void UnAspirate(PipetteChannels channel, PipettePositions position);
        void UnAspirateBoth(PipettePositions position);
        void UnLoadTip(PipetteChannels channel);
        void UnloadTips();
        /// <summary>
        /// 移动到指定位置  两通道相邻
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        [ErrorOperation(ErrorOperation.Abort | ErrorOperation.Retry)]
        void MoveTo(PipettePositions pos, int row, int column);
        /// <summary>
        /// 移动到指定位置  两通道分别在不同的列， 要求 column2>column1
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="row"></param>
        /// <param name="column1"></param>
        /// <param name="column2"></param>
        [ErrorOperation(ErrorOperation.Abort | ErrorOperation.Retry)]
        void MoveTo(PipettePositions pos, int row, int column1, int column2);
        [ErrorOperation(ErrorOperation.Abort | ErrorOperation.Retry)]
        void MoveTo(string pos, int row, int column);
    }
}
