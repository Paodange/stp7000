using Mgi.Barcode.Leuze;
using Mgi.Gripper.Zimma;
using Mgi.Instrument.ALM.Axis;
using Mgi.Robot.Cantroller;

namespace Mgi.Instrument.ALM.Device
{
    public class LidUncoverUnit
    {
        public LidUncoverUnits LidUncover { get; set; }
        /// <summary>
        /// Z
        /// </summary>
        public IALMAxis Z { get; set; }
        /// <summary>
        /// 旋转1
        /// </summary>
        public IALMAxis R1 { get; set; }
        /// <summary>
        /// 旋转2
        /// </summary>
        public IALMAxis R2 { get; set; }
        /// <summary>
        /// 传送带1
        /// </summary>
        public IALMAxis T { get; set; }
        /// <summary>
        /// 传送带固定1
        /// </summary>
        public IALMAxis C { get; set; }

        /// <summary>
        /// E轴
        /// </summary>
        public IALMAxis E { get; set; }

        public ILeuzeBarcode BarcodeA { get; set; }
        public ILeuzeBarcode BarcodeB { get; set; }
        public IZimmaGripper GripperA { get; set; }
        public IZimmaGripper GripperB { get; set; }

        public void HomeAll()
        {
            Z.GoHome();
            R1.HomeBegin();
            R2.HomeBegin();
            R1.HomeEnd();
            R2.HomeEnd();

            T.HomeBegin();
            C.HomeBegin();
            E.HomeBegin();
            T.HomeEnd();
            C.HomeEnd();
            E.HomeEnd();
        }
    }
}
