using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class AntennaType
{
    public short Id { get; set; }

    public string AntennaName { get; set; } = null!;

    public string Manufacturer { get; set; } = null!;

    public string? Polarization { get; set; }

    public string? AntennaFamily { get; set; }

    public short? Height { get; set; }

    public virtual ICollection<AntennaTypePort> AntennaTypePort { get; set; } = new List<AntennaTypePort>();
}
