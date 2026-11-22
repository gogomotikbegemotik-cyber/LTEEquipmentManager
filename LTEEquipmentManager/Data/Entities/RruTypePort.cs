using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class RruTypePort
{
    public int Id { get; set; }

    public byte RruTypeId { get; set; }

    public short PortNumber { get; set; }

    public string PortName { get; set; } = null!;

    public int PortTypeId { get; set; }

    public bool? Tx { get; set; }

    public bool? Rx { get; set; }

    public float? MaxPowerWatt { get; set; }

    public virtual PortType PortType { get; set; } = null!;

    public virtual RruType RruType { get; set; } = null!;

    public virtual ICollection<RruTypePortStandart> RruTypePortStandart { get; set; } = new List<RruTypePortStandart>();
}
