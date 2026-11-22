using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

public partial class Vendor
{
    public byte Id { get; set; }

    public string VendorName { get; set; } = null!;
}
