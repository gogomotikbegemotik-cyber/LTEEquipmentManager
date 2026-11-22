using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class RruTypePortStandart
{
    public int Id { get; set; }

    public int RruTypePortId { get; set; }

    public byte StandartId { get; set; }

    public virtual RruTypePort RruTypePort { get; set; } = null!;

    public virtual Standart Standart { get; set; } = null!;
}
