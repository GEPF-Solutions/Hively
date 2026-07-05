using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class MatchConsumerAction
{
    public Guid MatchId { get; set; }

    public Guid ConsumerId { get; set; }

    public bool IsExclude { get; set; }

    public virtual Consumer Consumer { get; set; } = null!;

    public virtual Match Match { get; set; } = null!;
}
