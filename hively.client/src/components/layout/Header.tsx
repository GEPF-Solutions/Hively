import { Link } from 'react-router-dom';
import HiveLogo from './HiveLogo';
import BrokerStatusPill from './BrokerStatusPill';
import NavLink from '../navigation/NavLink';
import Button from '../ui/Button';
import Badge from '../ui/Badge';
import { ManageMenu } from '../../pages/Manage';
import { useAuth } from '../../contexts/AuthContext';

export default function Header() {
  const { user, isAdmin, logout } = useAuth();

  return (
    <header className="flex h-[52px] min-h-[52px] items-center gap-3.5 border-b border-border bg-header px-5">
      <Link to="/topics" className="flex items-center gap-2.5">
        <HiveLogo />
        <span className="font-brand text-base font-semibold tracking-wide text-text/90">HIVELY</span>
      </Link>

      <div className="h-4 w-px bg-border" />

      <BrokerStatusPill />

      <div className="ml-1.5 flex h-full items-center gap-4">
        <NavLink href="/topics">Topics</NavLink>
        <NavLink href="/graph">Graph</NavLink>
      </div>

      <div className="flex-1" />

      {isAdmin && <ManageMenu />}

      {user && (
        <div className="flex items-center gap-3">
          <span className="text-sm text-text/80">{user.email}</span>
          <Badge tone={user.role === 'Admin' ? 'plain' : 'neutral'}>{user.role}</Badge>
          <Button variant="secondary" size="sm" onClick={() => logout()}>
            Sign out
          </Button>
        </div>
      )}
    </header>
  );
}
