import { useState } from 'react';
import Modal from '../../../components/ui/Modal';
import Button from '../../../components/ui/Button';
import Badge from '../../../components/ui/Badge';
import Input from '../../../components/ui/Input';
import { ManageList } from '../../../components/shared';
import { useUsers } from '../../../hooks/data/useUsers';
import { useAuth } from '../../../contexts/AuthContext';
import { userService } from '../../../services/userService';
import { useToast } from '../../../contexts/ToastContext';
import type { UserRole } from '../../../types';

type RoleFilter = UserRole | null;

export default function ManageUsersPanel({ onClose }: { onClose: () => void }) {
  const { users, refetch } = useUsers();
  const { user: currentUser } = useAuth();
  const toast = useToast();
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState<RoleFilter>(null);

  const filtered = users
    .filter((u) => !roleFilter || u.role === roleFilter)
    .filter((u) => u.email.toLowerCase().includes(search.trim().toLowerCase()));

  async function handleSetRole(userId: string, role: UserRole) {
    try {
      await userService.updateRole(userId, role);
      refetch();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to update role');
    }
  }

  return (
    <Modal isOpen onClose={onClose} title="Manage Users" maxWidth="sm" footer={<Button onClick={onClose}>Done</Button>}>
      <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="filter by email…" className="mb-2.5" />

      <div className="mb-3 flex gap-1.5">
        {([null, 'Admin', 'Viewer'] as RoleFilter[]).map((role) => (
          <button
            key={role ?? 'all'}
            onClick={() => setRoleFilter(role)}
            className={`rounded-md border px-2.5 py-1 text-[11px] font-medium ${
              roleFilter === role ? 'border-brand/50 text-brand' : 'border-border-strong text-muted'
            }`}
          >
            {role ?? 'all'}
          </button>
        ))}
      </div>

      <ManageList>
        {filtered.map((user) => {
          const isSelf = user.email === currentUser?.email;
          return (
            <div key={user.id} className="flex items-center gap-2.5 rounded-md bg-bg px-2.5 py-2">
              <div className="min-w-0 flex-1">
                <div className="truncate text-[12.5px] text-text">{user.email}</div>
                <div className="mt-0.5 text-[10.5px] text-muted/70">joined {new Date(user.createdAt).toLocaleDateString()}</div>
              </div>
              <Badge tone={user.role === 'Admin' ? 'plain' : 'neutral'}>{user.role}</Badge>
              {isSelf ? (
                <span className="text-[10.5px] italic text-muted/70">you</span>
              ) : (
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => handleSetRole(user.id, user.role === 'Admin' ? 'Viewer' : 'Admin')}
                >
                  {user.role === 'Admin' ? 'Demote' : 'Promote'}
                </Button>
              )}
            </div>
          );
        })}
        {filtered.length === 0 && <div className="px-1 py-1 text-[12.5px] italic text-muted">No users match.</div>}
      </ManageList>

      <p className="text-xs leading-relaxed text-muted">
        You can't change your own role here — ask another Admin if you need to step down.
      </p>
    </Modal>
  );
}
