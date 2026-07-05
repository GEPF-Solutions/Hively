using System;
using System.Collections.Generic;

namespace Hively.Server.DbModel;

public partial class MatchTagAction
{
    public Guid MatchId { get; set; }

    public string TagId { get; set; } = null!;

    public bool IsExclude { get; set; }

    public virtual Match Match { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
