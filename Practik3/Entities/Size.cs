using System;
using System.Collections.Generic;

namespace Practik3.Entities;

public partial class Size
{
    public int Id { get; set; }

    public string? SizeLevel { get; set; }

    public int? SizeLevelNumber { get; set; }

    public virtual ICollection<ClothSize> ClothSizes { get; set; } = new List<ClothSize>();
}
