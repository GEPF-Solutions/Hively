import { forwardRef, type SelectHTMLAttributes } from 'react';

type SelectProps = SelectHTMLAttributes<HTMLSelectElement>;

const Select = forwardRef<HTMLSelectElement, SelectProps>(({ className = '', children, ...props }, ref) => {
  return (
    <select
      ref={ref}
      className={`w-full bg-bg facet-sm facet-border-strong px-2.5 py-2 text-sm text-text outline-none focus:facet-accent disabled:opacity-50 disabled:cursor-not-allowed ${className}`}
      {...props}
    >
      {children}
    </select>
  );
});

Select.displayName = 'Select';

export default Select;
