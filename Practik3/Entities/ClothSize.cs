using System;
using System.Collections.Generic;

namespace Practik3.Entities;

public partial class ClothSize
{
    public int Id { get; set; }

    public int? ClothId { get; set; }

    public int? SizeId { get; set; }

    public int? CountInStock { get; set; }

    public virtual Cloth? Cloth { get; set; }

    public virtual Size? Size { get; set; }
}
