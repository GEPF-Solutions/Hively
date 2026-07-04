import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Input from '../../../components/ui/Input';
import { topicService } from '../../../services/topicService';
import { useToast } from '../../../contexts/ToastContext';
import type { Topic } from '../../../types';

interface AddTopicModalProps {
  onClose: () => void;
  onCreated: (topic: Topic) => void;
}

/**
 * Manually catalogues a topic path before the broker has ever seen traffic
 * for it — creates the same kind of untracked stub MQTT ingestion would
 * create on first sighting (see TopicController.InsertTopicAsync), then
 * hands off to ConfigureTopicModal to assign producer/consumers/schema/tags.
 */
export default function AddTopicModal({ onClose, onCreated }: AddTopicModalProps) {
  const toast = useToast();
  const [path, setPath] = useState('');
  const [saving, setSaving] = useState(false);

  async function handleCreate() {
    const trimmed = path.trim();
    if (!trimmed) return;
    setSaving(true);
    try {
      const created = await topicService.insertTopic({ path: trimmed, tracked: false });
      onCreated(created);
      onClose();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to add topic');
    } finally {
      setSaving(false);
    }
  }

  return (
    <Modal
      isOpen
      onClose={onClose}
      title="Add Topic"
      maxWidth="sm"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleCreate} isLoading={saving}>
            Create &amp; configure
          </Button>
        </>
      }
    >
      <div className="mb-1.5 text-[11.5px] font-semibold text-muted">Full topic path</div>
      <Input
        value={path}
        onChange={(e) => setPath(e.target.value)}
        placeholder="enterprise/region/country/site/area/line/cell/equipment/module/subsystem/sensor/measurement"
        className="font-mono"
        autoFocus
      />
      <p className="mt-2.5 text-xs leading-relaxed text-muted">
        Catalogue a topic before the broker has published to it yet — useful for planning a new machine's namespace
        ahead of commissioning. It's created as an untracked stub, same as one MQTT ingestion would create on first
        sighting; you'll configure its producer/consumers/schema/tags next.
      </p>
    </Modal>
  );
}
