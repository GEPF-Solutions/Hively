using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class Topic
{
    public Guid Id { get; set; }

    public string Path { get; set; } = null!;

    public bool Tracked { get; set; }

    public Guid? ProducerId { get; set; }

    public Guid? SchemaId { get; set; }

    public string? LastPayload { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public bool Retained { get; set; }

    public int ViolationCount { get; set; }

    public DateTime? LastClearedAt { get; set; }

    public string ActivityHistogram { get; set; } = null!;

    public DateTime? RetiredAt { get; set; }

    public Guid? MergedIntoTopicId { get; set; }

    public virtual ICollection<Topic> InverseMergedIntoTopic { get; set; } = new List<Topic>();

    public virtual Topic? MergedIntoTopic { get; set; }

    public virtual Producer? Producer { get; set; }

    public virtual Schema? Schema { get; set; }

    public virtual ICollection<Consumer> Consumers { get; set; } = new List<Consumer>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
