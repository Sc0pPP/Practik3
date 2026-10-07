using System;
using System.Collections.Generic;

namespace Practik3.Entities;

public partial class Order
{
    public int Id { get; set; }

    public DateOnly? DateTime { get; set; }

    public string? Addres { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Clothorder> Clothorders { get; set; } = new List<Clothorder>();

    public virtual User User { get; set; } = null!;
}
