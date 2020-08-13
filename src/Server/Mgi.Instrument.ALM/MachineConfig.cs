using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mgi.Instrument.ALM
{
    public class MachineConfig
    {
        public bool LidUncoverSimulated { get; set; }
        public bool RobotSimulated { get; set; }
        public bool PipettesSimulated { get; set; }
        public bool IOBoardSimulated { get; set; } = false;
        public bool UseWebApi { get; set; }
        public int WebApiPort { get; set; } = 8800;
        /// <summary>
        /// 流程类型  1. 试管位置与深孔板位置无关系， 2. 试管位置与深孔板位置一一对应
        /// </summary>
        public int WorkflowType { get; set; } = 1;  // 1

        /// <summary>
        /// 开盖失败 吸液丢步异常处理方式   1. 人工处理  2. 自动处理(不吸液  机械臂抓回原位置 界面上红色标记)
        /// </summary>
        public int UncoverFailHandleMode { get; set; } = 2;

        public string LogLevel { get; set; } = "Info";
        public bool UseZLims { get; set; } = false;
    }

    public class LastRunInfo
    {
        public DateTime CreateTime { get; set; }
        public bool Finished { get; set; } = true;

        public Dictionary<string, string> TubeIds { get; set; }
        public Dictionary<string, int> Volumes { get; set; }
        public Dictionary<string, string> DeepPlateBarcodes { get; set; }
        public Dictionary<string, string> TipsBarcodes { get; set; }
    }
}
