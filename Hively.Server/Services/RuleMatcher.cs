using Hively.Server.DbModel;

namespace Hively.Server.Services
{
    /// <summary>
    /// Pure functions for ranking Rules against a topic path. Ported from the
    /// prototype's ruleSpecificity/findMatchingRules.
    /// </summary>
    public static class RuleMatcher
    {
        /// <summary>
        /// More literal (non-wildcard) segments = more specific; '#' counts as 0,
        /// '+' as 0.5, a literal segment as 1.
        /// </summary>
        public static double RuleSpecificity(string pattern)
        {
            return pattern.Split('/').Sum(segment => segment switch
            {
                "#" => 0.0,
                "+" => 0.5,
                _ => 1.0
            });
        }

        /// <summary>
        /// Rules whose pattern matches the topic path, most specific first.
        /// </summary>
        public static List<Rule> FindMatchingRules(string topicPath, IEnumerable<Rule> rules)
        {
            return rules
                .Where(r => TopicPatternMatcher.MatchTopic(r.Pattern, topicPath))
                .OrderByDescending(r => RuleSpecificity(r.Pattern))
                .ToList();
        }
    }
}
