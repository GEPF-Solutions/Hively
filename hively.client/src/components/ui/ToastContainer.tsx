import { useToast } from '../../contexts/ToastContext';
import type { ToastVariant } from '../../types';

const variantClasses: Record<ToastVariant, string> = {
  success: 'border-cyan/40 text-cyan',
  error: 'border-red/40 text-red',
  info: 'border-border-strong text-text',
};

export default function ToastContainer() {
  const { toasts, removeToast } = useToast();

  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-4 right-4 z-[100] flex flex-col gap-2">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          onClick={() => removeToast(toast.id)}
          className={`cursor-pointer rounded-md border bg-panel px-4 py-2.5 text-sm shadow-lg ${variantClasses[toast.variant]}`}
        >
          {toast.message}
        </div>
      ))}
    </div>
  );
}
