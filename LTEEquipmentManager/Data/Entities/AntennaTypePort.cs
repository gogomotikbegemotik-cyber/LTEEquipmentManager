using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class AntennaTypePort
{
    public int Id { get; set; }

    public short AntennaTypeId { get; set; }

    public short PortNumber { get; set; }

    public string PortName { get; set; } = null!;

    public int PortTypeId { get; set; }

    public byte? TiltGroup { get; set; }

    public decimal? MinTilt { get; set; }

    public decimal? MaxTilt { get; set; }

    public decimal? TiltStep { get; set; }

    public short? Polarization { get; set; }

    public short? AzimuthOffsetDeg { get; set; }

    public bool Combiner { get; set; }

    public virtual AntennaType AntennaType { get; set; } = null!;

    public virtual ICollection<AntennaTypePortStandart> AntennaTypePortStandart { get; set; } = new List<AntennaTypePortStandart>();

    public virtual PortType PortType { get; set; } = null!;
}
