using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Instrument.ALM.Config
{
    /// <summary>
    /// 试管参数配置
    /// </summary>
    public class ALMTubeConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        ///  高度 (毫米)
        /// </summary>
        public double Height { get; set; }
        /// <summary>
        /// 直径 试管直径(毫米)
        /// </summary>
        public double TubeDiameter { get; set; }
        /// <summary>
        /// 盖子直径(毫米)
        /// </summary>
        public double LidDiameter { get; set; }
        /// <summary>
        /// 试管中心距偏移(毫米)
        /// </summary>
        public double TubeCenterOffset { get; set; }
        /// <summary>
        /// c轴夹紧偏移(毫米)
        /// </summary>
        public double CGripperOffset { get; set; }
        /// <summary>
        /// 开盖子Z下降的高度(毫米)
        /// </summary>
        public double UncoverZOffset { get; set; }
        /// <summary>
        /// 开盖的旋转圈数
        /// </summary>
        public double UncoverFirstCycles { get; set; }

        /// <summary>
        /// 开盖子第一次旋转R轴速度
        /// </summary>
        public int UncoverFirstSpeed { get; set; }

        /// <summary>
        /// 开盖子第二次旋转R轴速度
        /// </summary>
        public int UncoverSecondSpeed { get; set; }
        /// <summary>
        /// 开盖子第一次旋转R轴加速度
        /// </summary>
        public int UncoverFirstAccel { get; set; }
        /// <summary>
        /// 开盖子第二次旋转R轴加速度
        /// </summary>
        public int UncoverSecondAccel { get; set; }
        /// <summary>
        /// 开盖子第二次旋转圈数  配合 UncoverZMoveDistance  Z上抬的距离
        /// </summary>
        public double UncoverSecondCycles { get; set; }
        /// <summary>
        /// 开盖第二步Z轴上提距离(毫米)
        /// </summary>
        public double UncoverZMoveDistance { get; set; }

        /// <summary>
        /// 盖盖子第一次旋转圈数
        /// </summary>
        public double CoverFirstCycles { get; set; }

        /// <summary>
        /// 盖盖子第二次旋转圈数
        /// </summary>
        public double CoverSecondCycles { get; set; }

        /// <summary>
        /// 盖盖子第一次旋转R轴速度
        /// </summary>
        public int CoverFirstSpeed { get; set; }

        /// <summary>
        /// 盖盖子第二次旋转R轴速度
        /// </summary>
        public int CoverSecondSpeed { get; set; }
        /// <summary>
        /// 盖盖子第一次旋转R轴加速度
        /// </summary>
        public int CoverFirstAccel { get; set; }
        /// <summary>
        /// 盖盖子第二次旋转R轴加速度
        /// </summary>
        public int CoverSecondAccel { get; set; }

        /// <summary>
        /// 试管条码位置 扫描条码的绝对距离(毫米)
        /// </summary>
        public double TubeBarcodePosition { get; set; }
        /// <summary>
        /// 液面高度（毫米）
        /// </summary>
        public double LiquidHeight { get; set; }
        /// <summary>
        /// 机械臂抓取Z轴偏移
        /// </summary>
        public double RobotGraspZOffset { get; set; }
        /// <summary>
        /// 试管底部到第二段速度起始点的距离 （毫米）  第一段走的脉冲为 学习位置-此偏移
        /// </summary>
        public double AspirateFirstOffset { get; set; }
        /// <summary>
        /// 吸液高度 （毫米） 试管底部到吸液的距离
        /// </summary>
        public double AspirateHeight { get; set; }
        /// <summary>
        /// 拔盖器夹爪位置
        /// </summary>
        public int LidUncoverGripperPosition { get; set; }
        /// <summary>
        /// 拔盖器 夹爪允许位置偏离阈值
        /// </summary>
        public byte LidUncoverGripperPositionTolerance { get; set; } = 255;
        /// <summary>
        /// 拔盖器夹紧力度  1-4
        /// </summary>
        public byte LidUncoverTightenForce { get; set; }
        /// <summary>
        /// 机械臂夹爪位置
        /// </summary>
        public int RobotGripperPosition { get; set; } = 1300;
        /// <summary>
        /// 机械臂夹紧力度  1-4
        /// </summary>
        public byte RobotTightenForce { get; set; }
        /// <summary>
        ///  机械臂 夹爪允许位置偏离阈值
        /// </summary>
        public byte RobotGripperPositionTolerance { get; set; } = 255;
    }
}
