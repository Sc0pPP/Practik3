using System;
using System.Collections.Generic;

namespace Practik3.Entities;

public partial class Clothorder
{
    public int ClothOrderId { get; set; }

    public int? OrderId { get; set; }

    public int? ClothId { get; set; }

    public virtual Cloth? Cloth { get; set; }

    public virtual Order? Order { get; set; }
}
