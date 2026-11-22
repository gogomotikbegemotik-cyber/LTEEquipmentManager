using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class Standart
{
    public byte Id { get; set; }

    public string StandartName { get; set; } = null!;

    public string? StandartName2 { get; set; }

    public virtual ICollection<AntennaTypePortStandart> AntennaTypePortStandart { get; set; } = new List<AntennaTypePortStandart>();

    public virtual ICollection<RruTypePortStandart> RruTypePortStandart { get; set; } = new List<RruTypePortStandart>();
}
