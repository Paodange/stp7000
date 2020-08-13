namespace Mgi.Instrument.ALM
{
    /// <summary>
    /// 样本存储器
    /// </summary>
    public class ALMHotel : MicroPlate
    {
        public string TargetDeepPlateName { get; set; }
        public ALMHotel(string name) : this(name, 8, 12)
        {

        }
        public ALMHotel(string name, int rows, int columns) : base(name, rows, columns)
        {
        }

        public override void SetMaterial(PlatePosition pos, PlateStorageUnit material)
        {
            Positions.AddOrUpdate(pos, material, (k, v) => material);
        }
    }
}
