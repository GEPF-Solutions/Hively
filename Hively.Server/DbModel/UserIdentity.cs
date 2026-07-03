using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class UserIdentity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AuthProvider { get; set; } = null!;

    public string ExternalSubject { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
