using System;
using System.Collections.Generic;

namespace Practik3.Entities;

public partial class UserType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
