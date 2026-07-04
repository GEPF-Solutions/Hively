import { Link } from 'react-router-dom';
import HiveLogo from './HiveLogo';
import NavLink from '../navigation/NavLink';
import Button from '../ui/Button';
import Badge from '../ui/Badge';
import { ManageMenu } from '../../pages/Manage';
import { useAuth } from '../../contexts/AuthContext';
import { useSignalR } from '../../contexts/SignalRContext';

export default function Header() {
  const { user, isAdmin, logout } = useAuth();
  const { topicHubConnection } = useSignalR();
  const isLive = topicHubConnection !== null;

  return (
    <header className="flex h-[52px] min-h-[52px] items-center gap-3.5 border-b border-border bg-header px-5">
      <Link to="/topics" className="flex items-center gap-2.5">
        <HiveLogo />
        <span className="font-brand text-base font-semibold tracking-wide text-text/90">HIVELY</span>
      </Link>

      <div className="h-4 w-px bg-border" />

      <div className="flex items-center gap-1.5">
        <span
          className="h-1.5 w-1.5 rounded-full"
          style={{ background: isLive ? 'oklch(0.72 0.16 150)' : 'oklch(0.55 0.012 254)' }}
        />
        <span className="font-mono text-[11.5px] text-muted">{isLive ? 'live' : 'connecting…'}</span>
      </div>

      <div className="ml-1.5 flex gap-0.5 rounded-lg border border-border bg-bg p-0.5">
        <NavLink href="/topics">Topics</NavLink>
        <NavLink href="/graph">Graph</NavLink>
      </div>

      <div className="flex-1" />

      {isAdmin && <ManageMenu />}

      {user && (
        <div className="flex items-center gap-3">
          <span className="text-sm text-text/80">{user.email}</span>
          <Badge tone={user.role === 'Admin' ? 'cyan' : 'neutral'}>{user.role}</Badge>
          <Button variant="secondary" size="sm" onClick={() => logout()}>
            Sign out
          </Button>
        </div>
      )}
    </header>
  );
}
