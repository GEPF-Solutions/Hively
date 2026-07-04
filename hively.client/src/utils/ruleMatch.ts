import { matchTopicPattern } from './searchMatch';
import type { Rule, RuleMatch } from '../types';

// Client-side mirror of Services/RuleMatcher.cs (also ported to the prototype's
// ruleSpecificity/findMatchingRules) — used so the topic list can show
// "⚡ Apply rule" / "N rules match" without an API round-trip per row.
export function ruleSpecificity(pattern: string): number {
  return pattern.split('/').reduce((n, seg) => {
    if (seg === '#') return n;
    if (seg === '+') return n + 0.5;
    return n + 1;
  }, 0);
}

export function findMatchingRules(topicPath: string, rules: Rule[]): RuleMatch[] {
  const matches = rules
    .filter((r) => matchTopicPattern(r.pattern, topicPath))
    .sort((a, b) => ruleSpecificity(b.pattern) - ruleSpecificity(a.pattern))
    .map((rule) => ({ rule, specificity: ruleSpecificity(rule.pattern), recommended: false }));

  if (matches.length > 0) matches[0].recommended = true;
  return matches;
}
