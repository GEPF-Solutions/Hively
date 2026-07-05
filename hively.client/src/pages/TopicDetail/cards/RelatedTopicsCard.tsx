import { useNavigate } from 'react-router-dom';
import { segmentsOf } from '../../../utils/topicPath';
import type { Topic } from '../../../types';

export default function RelatedTopicsCard({ topic, allTopics }: { topic: Topic; allTopics: Topic[] }) {
  const navigate = useNavigate();
  const segments = segmentsOf(topic.path);
  const parentKey = segments.slice(0, -1).join('/');

  const related = allTopics.filter(
    (t) => t.id !== topic.id && segmentsOf(t.path).length === segments.length && segmentsOf(t.path).slice(0, -1).join('/') === parentKey,
  );

  return (
    <div className="hv-card col-span-full p-4">
      <div className="mb-2.5 text-[11px] font-semibold uppercase tracking-wide text-muted">
        Related topics · same parent
      </div>
      <div className="flex flex-wrap gap-2">
        {related.map((r) => (
          <button
            key={r.id}
            onClick={() => navigate(`/topics/${r.id}`)}
            className="rounded-md border border-border bg-bg px-2.5 py-1.5 font-mono text-xs text-text/80"
          >
            {segmentsOf(r.path).at(-1)}
          </button>
        ))}
        {related.length === 0 && <div className="text-[12.5px] italic text-muted">no siblings under this cell</div>}
      </div>
    </div>
  );
}
