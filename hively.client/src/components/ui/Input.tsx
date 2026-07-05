import { forwardRef, type InputHTMLAttributes } from 'react';

type InputProps = InputHTMLAttributes<HTMLInputElement>;

const Input = forwardRef<HTMLInputElement, InputProps>(({ className = '', ...props }, ref) => {
  return (
    <input
      ref={ref}
      className={`w-full bg-bg facet-sm facet-border-strong px-2.5 py-2 text-sm text-text placeholder:text-muted outline-none focus:facet-accent disabled:opacity-50 disabled:cursor-not-allowed ${className}`}
      {...props}
    />
  );
});

Input.displayName = 'Input';

export default Input;
