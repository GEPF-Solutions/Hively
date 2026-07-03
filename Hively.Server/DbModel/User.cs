using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserIdentity> UserIdentities { get; set; } = new List<UserIdentity>();
}
