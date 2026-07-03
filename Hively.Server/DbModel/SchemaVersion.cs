using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class SchemaVersion
{
    public Guid Id { get; set; }

    public Guid SchemaId { get; set; }

    public string Version { get; set; } = null!;

    public string Definition { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Schema Schema { get; set; } = null!;
}
