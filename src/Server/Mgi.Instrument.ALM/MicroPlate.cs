using Mgi.ALM.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mgi.Instrument.ALM
{
    public class MicroPlate
    {
        private readonly object syncLock = new object();
        /// <summary>
        /// 条码
        /// </summary>
        public string Barcode { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 行间距
        /// </summary>
        public virtual int RowSpan { get; }
        /// <summary>
        /// 列间距
        /// </summary>
        public virtual int ColumnSpan { get; }
        /// <summary>
        /// 行数
        /// </summary>
        public int RowCount { get; private set; }
        /// <summary>
        /// 列数
        /// </summary>
        public int ColumnCount { get; private set; }
        /// <summary>
        /// 位置
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public ConcurrentDictionary<PlatePosition, PlateStorageUnit> Positions { get; private set; }

        public List<PlateStorageUnit> StorageUnits { get; set; }
        public MicroPlate(string name) : this(name, 8, 12)
        {

        }
        public MicroPlate(string name, int rows, int columns)
        {
            Name = name;
            RowCount = rows;
            ColumnCount = columns;
            Positions = new ConcurrentDictionary<PlatePosition, PlateStorageUnit>();
            InitializePositions();
        }
        private MicroPlate(MicroPlateInfo plateInfo)
            : this(plateInfo.Name, plateInfo.RowCount, plateInfo.ColumnCount)
        {
            Barcode = plateInfo.Barcode;
            if (plateInfo.StorageUnits != null && plateInfo.StorageUnits.Count > 0)
            {
                foreach (var p in Positions)
                {
                    var unit = plateInfo.StorageUnits.FirstOrDefault(x => x.IsUsed == true && x.Position == p.Key);
                    if (unit != null)
                    {
                        Positions[p.Key] = unit;
                    }
                }
            }
        }

        public bool IsEmpty(int row, int column)
        {
            return IsEmpty(new PlatePosition(row, column));
        }
        public bool IsEmpty(PlatePosition pos)
        {
            CheckPlatePosition(pos);
            return !Positions.TryGetValue(pos, out var value) || value == null;
        }
        public virtual void SetMaterial(int row, int column, PlateStorageUnit material)
        {
            SetMaterial(new PlatePosition(row, column), material);
        }
        public virtual void SetMaterial(PlatePosition pos, PlateStorageUnit material)
        {
            Positions.AddOrUpdate(pos, material, (k, v) => material);
            SaveToFile();
        }
        public PlateStorageUnit GetMaterial(int row, int column)
        {
            return GetMaterial(new PlatePosition(row, column));
        }
        public PlateStorageUnit GetMaterial(PlatePosition pos)
        {
            CheckPlatePosition(pos);
            return Positions[pos];
        }

        public void Clear()
        {
            foreach (var p in Positions.Keys)
            {
                Positions[p] = null;
            }
        }
        private void CheckPlatePosition(PlatePosition pos)
        {
            if (pos.Column < 1 || pos.Row < 1 || pos.Column > ColumnCount || pos.Row > RowCount)
            {
                throw new ArgumentOutOfRangeException($"PlatePosition:{pos} not in the plate");
            }
        }
        private void InitializePositions()
        {
            var dic = new ConcurrentDictionary<PlatePosition, PlateStorageUnit>();
            for (int row = 1; row <= RowCount; row++)
            {
                for (int column = 1; column <= ColumnCount; column++)
                {
                    dic.TryAdd(new PlatePosition(row, column), new PlateStorageUnit()
                    {
                        Position = new PlatePosition()
                        {
                            Column = column,
                            Row = row
                        },
                        Barcode = "",
                        IsUsed = false
                    });
                }
            }
            Positions = new ConcurrentDictionary<PlatePosition, PlateStorageUnit>(dic.OrderBy(x => x.Key.Row).ThenBy(x => x.Key.Column));
        }

        public PlatePosition? GetNextEmptyPosition()
        {
            lock (syncLock)
            {
                for (int row = 1; row <= RowCount; row++)
                {
                    for (int column = 1; column <= ColumnCount; column++)
                    {
                        var pos = new PlatePosition(row, column);
                        if (Positions[pos] == null || Positions[pos].IsUsed == false)
                        {
                            Positions[pos] = new PlateStorageUnit()
                            {
                                Barcode = "",
                                Position = pos,
                                IsUsed = null
                            };
                            return pos;
                        }
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 获取可用的孔数量 
        /// </summary>
        /// <returns></returns>
        public int GetEmptyCount()
        {
            lock (syncLock)
            {
                return Positions.Count(x => x.Value == null || x.Value?.IsUsed == false);
            }
        }
        /// <summary>
        /// 唯一地址转位置（行，列）
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public PlatePosition AddressToPosition(int address)
        {
            if (address > RowCount * ColumnCount)
            {
                throw new Exception($"Address is bigger than allowed, Allow:{RowCount * ColumnCount},Actual:{address}");
            }
            var mod = address % ColumnCount;
            var row = mod == 0 ? (address / ColumnCount) : ((address / ColumnCount) + 1);
            var column = mod == 0 ? ColumnCount : mod;
            return new PlatePosition(row, column);
        }
        /// <summary>
        /// 位置转唯一地址
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int PositionToAddress(PlatePosition position)
        {
            return (position.Row - 1) * ColumnCount + position.Column;
        }

        /// <summary>
        /// 锁定后，有可能因为停止，导致没有执行相应的动作，这样实际上这些位置还是可用的  需要重置会可用的状态
        /// 比如 一开始为一个样本分配了吸头，未到扎吸头这一步，就停止了流程，导致未扎吸头，这样因为一开始锁定
        /// 导致后续这个位置还是不可用，实际是可用的   需要重置一下
        /// </summary>
        public void ResetUsed()
        {
            Positions.Where(x => x.Value.IsUsed == null).ToList().ForEach(x => x.Value.IsUsed = false);
        }

        private readonly object saveFileLock = new object();
        public void SaveToFile()
        {
            Task.Run(() =>
            {
                lock (saveFileLock)
                {
                    try
                    {
                        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Material");
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        string path = Path.Combine(dir, Name + ".json");
                        var list = Positions.Select(x => x.Value).ToList();
                        var obj = new MicroPlateInfo
                        {
                            Barcode = this.Barcode,
                            Name = this.Name,
                            ColumnCount = this.ColumnCount,
                            RowCount = this.RowCount,
                            StorageUnits = list
                        };
                        var json = JsonUtil.Serialize(obj, true);
                        File.WriteAllText(path, json, Encoding.UTF8);
                    }
                    catch
                    {
                        // 
                    }
                }
            });
        }

        public static MicroPlate LoadFromFile(string plateName)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Material");
            string path = Path.Combine(dir, plateName + ".json");
            if (!File.Exists(path))
            {
                var p = new MicroPlate(plateName);
                p.SaveToFile();
                return p;
            }
            var json = File.ReadAllText(path, Encoding.UTF8);
            var plateInfo = JsonUtil.Deserialize<MicroPlateInfo>(json);
            return new MicroPlate(plateInfo);
        }
    }

    /// <summary>
    /// 孔板存储单元
    /// </summary>
    public class PlateStorageUnit : INotifyPropertyChanged
    {
        private bool? isUsed = null;
        private bool barcodeReadFail = false;
        private string barcode;
        public PlatePosition Position { get; set; }
        public bool? IsUsed
        {
            get
            {
                return isUsed;
            }
            set
            {
                if (isUsed != value)
                {
                    isUsed = value;
                    OnPropertyChanged(nameof(IsUsed));
                }
            }
        }
        public string Barcode
        {
            get
            {
                return barcode;
            }
            set
            {
                if (barcode != value)
                {
                    barcode = value;
                    OnPropertyChanged(nameof(Barcode));
                }
            }
        }
        public bool BarcodeReadFail
        {
            get
            {
                return barcodeReadFail;
            }
            set
            {
                if (value != barcodeReadFail)
                {
                    barcodeReadFail = value;
                    OnPropertyChanged(nameof(BarcodeReadFail));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public struct PlatePosition : IEquatable<PlatePosition>, IComparable<PlatePosition>
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public PlatePosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString()
        {
            return ((char)(64 + Row)).ToString() + Column;
        }

        public bool Equals(PlatePosition other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public int CompareTo(PlatePosition other)
        {
            if (Column < other.Column)
            {
                return -1;
            }
            else if (Column > other.Column)
            {
                return 1;
            }
            else
            {
                return Row.CompareTo(other.Row);
            }
        }
        public static bool operator ==(PlatePosition left, PlatePosition right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PlatePosition left, PlatePosition right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return (Row, Column).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PlatePosition))
            {
                return false;
            }
            return Equals((PlatePosition)obj);
        }
    }

    public class MicroPlateInfo
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<PlateStorageUnit> StorageUnits { get; set; }
    }
}
