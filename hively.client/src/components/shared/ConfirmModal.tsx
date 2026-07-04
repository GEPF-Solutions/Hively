import Modal from '../ui/Modal';
import Button from '../ui/Button';

interface ConfirmModalProps {
  title: string;
  message: string;
  confirmLabel?: string;
  confirming?: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

/** Generic destructive-action confirmation — used for deletes that are too consequential for a bare "×". */
export default function ConfirmModal({
  title,
  message,
  confirmLabel = 'Delete',
  confirming = false,
  onConfirm,
  onClose,
}: ConfirmModalProps) {
  return (
    <Modal
      isOpen
      onClose={onClose}
      title={title}
      maxWidth="sm"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="danger" onClick={onConfirm} isLoading={confirming}>
            {confirmLabel}
          </Button>
        </>
      }
    >
      <p className="text-sm leading-relaxed text-text/80">{message}</p>
    </Modal>
  );
}
