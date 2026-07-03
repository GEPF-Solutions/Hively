using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class Consumer
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
