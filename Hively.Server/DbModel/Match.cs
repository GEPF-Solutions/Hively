using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class Match
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string Pattern { get; set; } = null!;

    public Guid? TopicId { get; set; }

    public Guid? ProducerId { get; set; }

    public bool ExcludeProducer { get; set; }

    public Guid? SchemaId { get; set; }

    public bool ExcludeSchema { get; set; }

    public bool AutoApply { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<MatchConsumerAction> MatchConsumerActions { get; set; } = new List<MatchConsumerAction>();

    public virtual ICollection<MatchTagAction> MatchTagActions { get; set; } = new List<MatchTagAction>();

    public virtual Producer? Producer { get; set; }

    public virtual Schema? Schema { get; set; }

    public virtual Topic? Topic { get; set; }
}
