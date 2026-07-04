import type { ReactNode } from 'react';
import { Link, useLocation } from 'react-router-dom';

interface NavLinkProps {
  href: string;
  children: ReactNode;
}

/** Pill-style tab link (Topics / Graph switcher in the header). */
export default function NavLink({ href, children }: NavLinkProps) {
  const { pathname } = useLocation();
  const isActive = pathname === href || pathname.startsWith(`${href}/`);

  return (
    <Link
      to={href}
      className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
        isActive ? 'bg-cyan text-[oklch(0.15_0.02_200)]' : 'text-muted hover:text-text'
      }`}
    >
      {children}
    </Link>
  );
}
