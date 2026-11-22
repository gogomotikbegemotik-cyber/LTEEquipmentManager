using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class PortType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Connector { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public byte Channels { get; set; }

    public virtual ICollection<AntennaTypePort> AntennaTypePort { get; set; } = new List<AntennaTypePort>();

    public virtual ICollection<RruTypePort> RruTypePort { get; set; } = new List<RruTypePort>();
}
