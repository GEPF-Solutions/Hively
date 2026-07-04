import type { ButtonHTMLAttributes } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
  size?: 'sm' | 'md';
  isLoading?: boolean;
}

const variantClasses: Record<NonNullable<ButtonProps['variant']>, string> = {
  primary: 'bg-cyan text-[oklch(0.15_0.02_200)] font-semibold hover:opacity-90 disabled:opacity-50',
  secondary:
    'bg-transparent border border-border-strong text-text/90 hover:border-muted disabled:opacity-50',
  ghost: 'bg-transparent text-muted hover:text-text disabled:opacity-50',
  danger: 'bg-transparent border border-red/50 text-red hover:bg-red/10 disabled:opacity-50',
};

const sizeClasses: Record<NonNullable<ButtonProps['size']>, string> = {
  sm: 'px-3 py-1.5 text-xs',
  md: 'px-3.5 py-2 text-sm',
};

export default function Button({
  variant = 'secondary',
  size = 'md',
  className = '',
  isLoading = false,
  disabled,
  children,
  ...props
}: ButtonProps) {
  return (
    <button
      className={`rounded-md transition-colors cursor-pointer disabled:cursor-not-allowed ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? 'Working…' : children}
    </button>
  );
}
