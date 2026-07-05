namespace Hively.Server.Services
{
    /// <summary>
    /// Pure function ranking an MQTT topic filter's specificity — used by
    /// <see cref="MatchResolver"/> to decide which of several applicable Matches
    /// wins for a given field/value. Ported from the prototype's ruleSpecificity.
    /// </summary>
    public static class PatternSpecificity
    {
        /// <summary>
        /// More literal (non-wildcard) segments = more specific; '#' counts as 0,
        /// '+' as 0.5, a literal segment as 1.
        /// </summary>
        public static double Calculate(string pattern)
        {
            return pattern.Split('/').Sum(segment => segment switch
            {
                "#" => 0.0,
                "+" => 0.5,
                _ => 1.0
            });
        }
    }
}
