using Hively.Server.DbModel;

namespace Hively.Server.Services
{
    /// <summary>
    /// Pure function resolving a topic's producer/schema/consumers/tags from every
    /// Match whose pattern covers its path — no manual "which match wins" step.
    /// Single-value fields (producer, schema) take the most-specific applicable
    /// match that has an opinion (set or exclude) on that field. Multi-value fields
    /// (consumers, tags) resolve per distinct id: the most-specific applicable match
    /// that mentions that particular id decides whether it's included, independent
    /// of what any other match sets/excludes — so two matches only ever compete when
    /// they name the exact same id.
    /// </summary>
    public static class MatchResolver
    {
        public class ResolvedAssignment
        {
            public Guid? ProducerId { get; set; }

            public Guid? SchemaId { get; set; }

            public List<Guid> ConsumerIds { get; set; } = new();

            public List<string> TagIds { get; set; } = new();
        }

        /// <summary>
        /// Resolves the full assignment for a single topic path against every Match
        /// (branch-authored, pattern-authored, or a topic's own private override —
        /// all just rows in the same table).
        /// </summary>
        public static ResolvedAssignment Resolve(string topicPath, IEnumerable<Match> matches)
        {
            var applicable = matches
                .Where(m => TopicPatternMatcher.MatchTopic(m.Pattern, topicPath))
                .ToList();

            return new ResolvedAssignment
            {
                ProducerId = ResolveSingleValue(applicable, m => m.ProducerId, m => m.ExcludeProducer),
                SchemaId = ResolveSingleValue(applicable, m => m.SchemaId, m => m.ExcludeSchema),
                ConsumerIds = ResolveMultiValue(
                    applicable,
                    m => m.MatchConsumerActions.Select(a => (a.ConsumerId, a.IsExclude))),
                TagIds = ResolveMultiValue(
                    applicable,
                    m => m.MatchTagActions.Select(a => (a.TagId, a.IsExclude)))
            };
        }

        private static Guid? ResolveSingleValue(
            List<Match> applicable,
            Func<Match, Guid?> getSetValue,
            Func<Match, bool> getExclude)
        {
            var winner = applicable
                .Where(m => getSetValue(m) != null || getExclude(m))
                .OrderByDescending(m => PatternSpecificity.Calculate(m.Pattern))
                .ThenByDescending(m => m.UpdatedAt)
                .FirstOrDefault();

            if (winner == null)
            {
                return null;
            }

            return getExclude(winner) ? null : getSetValue(winner);
        }

        private static List<TValue> ResolveMultiValue<TValue>(
            List<Match> applicable,
            Func<Match, IEnumerable<(TValue Value, bool IsExclude)>> getActions) where TValue : notnull
        {
            var mentionsByValue = new Dictionary<TValue, List<(Match Match, bool IsExclude)>>();
            foreach (var match in applicable)
            {
                foreach (var (value, isExclude) in getActions(match))
                {
                    if (!mentionsByValue.TryGetValue(value, out var mentions))
                    {
                        mentions = new List<(Match, bool)>();
                        mentionsByValue[value] = mentions;
                    }

                    mentions.Add((match, isExclude));
                }
            }

            var result = new List<TValue>();
            foreach (var (value, mentions) in mentionsByValue)
            {
                var winner = mentions
                    .OrderByDescending(m => PatternSpecificity.Calculate(m.Match.Pattern))
                    .ThenByDescending(m => m.Match.UpdatedAt)
                    .First();

                if (!winner.IsExclude)
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }
}
