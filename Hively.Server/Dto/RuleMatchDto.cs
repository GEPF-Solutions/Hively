namespace Hively.Server.Dto
{
    /// <summary>
    /// A rule that matches an untracked topic's path, ranked by specificity —
    /// used by the Configure modal to let the admin pick when multiple rules match.
    /// </summary>
    public class RuleMatchDto
    {
        public RuleDto Rule { get; set; } = null!;

        /// <summary>Higher = more specific. See RuleMatcher.RuleSpecificity.</summary>
        public double Specificity { get; set; }

        /// <summary>True for the single most-specific match — pre-labeled "recommended" in the UI.</summary>
        public bool Recommended { get; set; }
    }
}
