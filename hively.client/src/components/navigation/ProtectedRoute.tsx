import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { Header } from '../layout';

interface ProtectedRouteProps {
  children: ReactNode;
  /** Restrict to Admin-role users; Viewer accounts get bounced to /topics. */
  adminOnly?: boolean;
}

export default function ProtectedRoute({ children, adminOnly = false }: ProtectedRouteProps) {
  const { isAuthenticated, isAdmin, loading } = useAuth();

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center bg-bg text-muted">
        <span className="font-mono text-sm">loading…</span>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (adminOnly && !isAdmin) {
    return <Navigate to="/topics" replace />;
  }

  return (
    <div className="flex h-screen flex-col overflow-hidden bg-bg text-text">
      <Header />
      <main className="flex-1 min-h-0">{children}</main>
    </div>
  );
}
