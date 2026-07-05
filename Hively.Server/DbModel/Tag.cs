using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class Tag
{
    public string Id { get; set; } = null!;

    public string Label { get; set; } = null!;

    public int? Hue { get; set; }

    public virtual ICollection<MatchTagAction> MatchTagActions { get; set; } = new List<MatchTagAction>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
