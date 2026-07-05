import { useEffect, type ReactNode } from 'react';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  footer?: ReactNode;
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl';
}

const maxWidthClasses: Record<NonNullable<ModalProps['maxWidth']>, string> = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-lg',
  xl: 'max-w-xl',
};

export default function Modal({ isOpen, onClose, title, children, footer, maxWidth = 'md' }: ModalProps) {
  useEffect(() => {
    if (!isOpen) return;

    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleEscape);
    document.body.style.overflow = 'hidden';

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = '';
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4" onClick={onClose}>
      <div
        className={`relative bg-panel facet-lg facet-border-strong shadow-2xl ${maxWidthClasses[maxWidth]} w-full max-h-[85vh] flex flex-col`}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between px-5 pt-4 pb-3">
          <h3 className="text-base font-semibold text-text">{title}</h3>
          <button
            onClick={onClose}
            aria-label="Close"
            className="text-muted hover:text-text text-lg leading-none cursor-pointer bg-transparent border-none"
          >
            ×
          </button>
        </div>

        <div className="flex-1 min-h-0 overflow-y-auto px-5">{children}</div>

        {footer && <div className="flex justify-end gap-2 px-5 py-4">{footer}</div>}
        {!footer && <div className="pb-4" />}
      </div>
    </div>
  );
}
