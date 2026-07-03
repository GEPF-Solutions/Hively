namespace Hively.Server.Dto
{
    /// <summary>
    /// A tracked-but-silent topic that looks like the "old address" of a newly-seen
    /// untracked topic after a physical relocation (see RelinkHeuristic). A suggestion
    /// the admin confirms via TopicService.AcceptRelinkAsync — never applied automatically.
    /// </summary>
    public class RelinkCandidateDto
    {
        public TopicDto Topic { get; set; } = null!;

        /// <summary>Minutes since the candidate's LastSeenAt — the "silent Nh/d ago" copy in the UI.</summary>
        public int SilentForMinutes { get; set; }
    }
}
