namespace Hively.Server.Services
{
    /// <summary>
    /// Pure function matching an MQTT-style topic filter ('+' single-level, '#'
    /// multi-level wildcard) against a topic path. Ported from the prototype's
    /// matchTopic — shared by Match matching and (eventually) wildcard search.
    /// </summary>
    public static class TopicPatternMatcher
    {
        public static bool MatchTopic(string pattern, string topicPath)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            var patternSegments = pattern.Split('/');
            var topicSegments = topicPath.Split('/');

            for (var i = 0; i < patternSegments.Length; i++)
            {
                if (patternSegments[i] == "#")
                {
                    return true;
                }

                if (i >= topicSegments.Length)
                {
                    return false;
                }

                if (patternSegments[i] == "+")
                {
                    continue;
                }

                if (patternSegments[i] != topicSegments[i])
                {
                    return false;
                }
            }

            return patternSegments.Length == topicSegments.Length;
        }
    }
}
