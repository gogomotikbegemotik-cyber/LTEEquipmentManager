using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class AntennaTypePortStandart
{
    public int Id { get; set; }

    public int AntennaTypePortId { get; set; }

    public byte StandartId { get; set; }

    public decimal? Gain { get; set; }

    public decimal? Hhpbw { get; set; }

    public decimal? Vhpbw { get; set; }

    public virtual AntennaTypePort AntennaTypePort { get; set; } = null!;

    public virtual Standart Standart { get; set; } = null!;
}
