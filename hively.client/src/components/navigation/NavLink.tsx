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
      className={`border-b-2 pb-1.5 text-xs font-medium transition-colors ${
        isActive ? 'border-gold text-gold font-semibold' : 'border-transparent text-muted hover:text-text'
      }`}
    >
      {children}
    </Link>
  );
}
