import type { ButtonHTMLAttributes } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary';
  size?: 'sm' | 'md';
  isLoading?: boolean;
}

/**
 * Exactly two button types across the whole app — no separate "ghost"/"danger"
 * look. A destructive action is a secondary button with a red text/border
 * override via `className`, not a third variant.
 */
const variantClasses: Record<NonNullable<ButtonProps['variant']>, string> = {
  primary: 'btn-primary-shadow bg-gold text-[oklch(0.16_0.02_92)] font-semibold hover:opacity-90 disabled:opacity-50',
  secondary:
    'bg-white/[0.06] border border-border-strong text-text/90 shadow-[0_1px_2px_oklch(0_0_0/0.25)] hover:bg-white/10 hover:border-muted disabled:opacity-50',
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
