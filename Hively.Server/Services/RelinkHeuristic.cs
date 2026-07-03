using Hively.Server.DbModel;

namespace Hively.Server.Services
{
    /// <summary>
    /// Pure function finding a stale, tracked topic that looks like the "old
    /// address" of a newly-seen untracked topic after a physical relocation.
    /// Ported from the prototype's findRelinkCandidate — a suggestion only, never
    /// applied automatically (see <see cref="Abstractions.ITopicService.AcceptRelinkAsync"/>).
    /// </summary>
    public static class RelinkHeuristic
    {
        /// <summary>
        /// Match on the last N path segments (device-internal segments that
        /// survive a relocation of everything upstream of them).
        /// </summary>
        public const int TailSegmentCount = 6;

        /// <summary>
        /// Minutes of silence before a tracked topic is considered a candidate
        /// "old address" rather than just a quiet-but-current topic.
        /// </summary>
        public const int StaleThresholdMinutes = 720; // 12h

        public static Topic? FindRelinkCandidate(Topic target, IEnumerable<Topic> topics, DateTime now)
        {
            var targetTail = Tail(target.Path);

            return topics.FirstOrDefault(candidate =>
                candidate.Tracked &&
                candidate.Id != target.Id &&
                candidate.Path != target.Path &&
                Tail(candidate.Path) == targetTail &&
                candidate.LastSeenAt is { } lastSeenAt &&
                (now - lastSeenAt).TotalMinutes > StaleThresholdMinutes);
        }

        private static string Tail(string path)
        {
            var segments = path.Split('/');
            return string.Join('/', segments.Skip(Math.Max(0, segments.Length - TailSegmentCount)));
        }
    }
}
