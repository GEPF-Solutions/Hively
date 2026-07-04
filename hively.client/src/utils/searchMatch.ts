// Ported from Design/MQTT Data Catalog.dc.html — client-side topic search
// (see Design/README.md, "Key Behaviors" #5). Backend has its own copy of the
// MQTT-wildcard matcher (Services/TopicPatternMatcher.cs) for Rule matching;
// this is the same algorithm, kept separately since it runs client-side.

function wildcardToRegex(pattern: string): RegExp {
  const escaped = pattern.replace(/[.+^${}()|[\]\\]/g, '\\$&');
  const withGlob = escaped.replace(/\*/g, '.*').replace(/\?/g, '.');
  return new RegExp(withGlob, 'i');
}

/** MQTT-style '+'/'#' wildcard match between a filter pattern and a topic path. */
export function matchTopicPattern(pattern: string, topicPath: string): boolean {
  if (!pattern) return false;
  const p = pattern.split('/');
  const t = topicPath.split('/');
  for (let i = 0; i < p.length; i++) {
    if (p[i] === '#') return true;
    if (i >= t.length) return false;
    if (p[i] === '+') continue;
    if (p[i] !== t[i]) return false;
  }
  return p.length === t.length;
}

/**
 * Substring search by default; auto-detects MQTT ('+'/'#') or glob ('*'/'?')
 * wildcards and matches against the path only in that case.
 */
export function matchesSearch(query: string, path: string, haystackExtra: string): boolean {
  const q = query.trim();
  if (!q) return true;

  const hasWildcard = /[*?+#]/.test(q);
  if (hasWildcard) {
    if (q.includes('+') || q.includes('#')) return matchTopicPattern(q, path);
    return wildcardToRegex(q).test(path);
  }

  const haystack = `${path} ${haystackExtra}`.toLowerCase();
  return haystack.includes(q.toLowerCase());
}
