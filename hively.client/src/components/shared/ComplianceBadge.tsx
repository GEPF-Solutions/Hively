import Badge from '../ui/Badge';
import type { Topic } from '../../types';

export default function ComplianceBadge({ topic }: { topic: Topic }) {
  if (topic.compliant === null) {
    return <Badge tone="neutral">No schema</Badge>;
  }
  if (topic.compliant) {
    return <Badge tone="cyan">Pass</Badge>;
  }
  return <Badge tone="red">{topic.violationCount} fail</Badge>;
}
