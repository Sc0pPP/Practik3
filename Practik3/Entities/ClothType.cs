using System;
using System.Collections.Generic;

namespace Practik3.Entities;

public partial class ClothType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Cloth> Cloths { get; set; } = new List<Cloth>();
}
