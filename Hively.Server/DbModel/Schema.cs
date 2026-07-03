using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class Schema
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Definition { get; set; } = null!;

    public string Version { get; set; } = null!;

    public virtual ICollection<SchemaVersion> SchemaVersions { get; set; } = new List<SchemaVersion>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
