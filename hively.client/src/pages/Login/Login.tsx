import { Navigate } from 'react-router-dom';
import HiveLogo from '../../components/layout/HiveLogo';
import Button from '../../components/ui/Button';
import { useAuth } from '../../contexts/AuthContext';

export default function Login() {
  const { isAuthenticated, loading, loginWithGoogle, loginWithEntra } = useAuth();

  if (!loading && isAuthenticated) {
    return <Navigate to="/topics" replace />;
  }

  return (
    <div className="flex h-screen items-center justify-center bg-bg text-text">
      <div className="w-full max-w-sm rounded-xl border border-border bg-panel p-8 text-center">
        <div className="mb-6 flex items-center justify-center gap-2.5">
          <HiveLogo size={36} />
          <span className="font-brand text-xl font-semibold tracking-wide text-text/90">HIVELY</span>
        </div>
        <p className="mb-6 text-sm text-muted">Sign in to browse the UNS data catalog.</p>
        <div className="flex flex-col gap-2.5">
          <Button variant="primary" onClick={() => loginWithGoogle()}>
            Continue with Google
          </Button>
          <Button variant="secondary" onClick={() => loginWithEntra()}>
            Continue with Microsoft Entra ID
          </Button>
        </div>
      </div>
    </div>
  );
}
