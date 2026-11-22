using System;
using System.Collections.Generic;

namespace LTEEquipmentManager.Data.Entities;

using System.Collections.Generic;

public partial class RruType
{
    // номер RRU в базе
    public byte Id { get; set; }

    // номер производителя из справочника
    public byte IdVendor { get; set; }

    // номер типа размещения
    public byte IdPlacementType { get; set; }

    // название модели RRU
    public string Name { get; set; } = null!;

    // тип устройства
    public string Type { get; set; } = null!;

    // количество TRX
    public byte TrxCount { get; set; }

    // количество CU
    public byte CuCount { get; set; }

    // максимальная мощность устройства
    public float MaxPowerWatt { get; set; }

    // комментарий, можно оставить пустым
    public string? Comment { get; set; }

    // тут хранятся связанные с RRU порты
    public virtual ICollection<RruTypePort> RruTypePort { get; set; }
        = new List<RruTypePort>();
}

