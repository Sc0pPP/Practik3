using System;
using System.Collections.Generic;

namespace Practik3.Entities;

public partial class Cloth
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public int? CountBuys { get; set; }

    public int? TypeId { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ClothSize> ClothSizes { get; set; } = new List<ClothSize>();

    public virtual ICollection<Clothorder> Clothorders { get; set; } = new List<Clothorder>();

    public virtual ClothType? Type { get; set; }
}
