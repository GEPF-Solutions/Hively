using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class Rule
{
    public Guid Id { get; set; }

    public string Pattern { get; set; } = null!;

    public Guid? ProducerId { get; set; }

    public virtual Producer? Producer { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
