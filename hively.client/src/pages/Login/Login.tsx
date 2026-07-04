import { useEffect } from 'react';
import { Navigate, useSearchParams } from 'react-router-dom';
import HiveLogo from '../../components/layout/HiveLogo';
import Button from '../../components/ui/Button';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';
import { useAuthProviders } from '../../hooks/data/useAuthProviders';
import { GoogleIcon, MicrosoftIcon } from './ProviderIcons';
import HiveWatermark from './HiveWatermark';

export default function Login() {
  const { isAuthenticated, loading, loginWithGoogle, loginWithEntra } = useAuth();
  const toast = useToast();
  const [searchParams, setSearchParams] = useSearchParams();
  const providers = useAuthProviders();

  useEffect(() => {
    if (searchParams.get('error') === 'access_denied') {
      toast.error("That account isn't allowed to sign in to Hively.");
      setSearchParams({}, { replace: true });
    }
  }, [searchParams, toast, setSearchParams]);

  if (!loading && isAuthenticated) {
    return <Navigate to="/topics" replace />;
  }

  return (
    <div className="relative flex h-screen items-center justify-center overflow-hidden bg-bg text-text">
      <HiveWatermark />
      <div className="relative w-full max-w-sm rounded-xl border border-border bg-panel p-8 text-center">
        <div className="mb-8 flex items-center justify-center gap-2.5">
          <HiveLogo size={36} />
          <span className="font-brand text-xl font-semibold tracking-wide text-text/90">HIVELY</span>
        </div>
        <div className="flex flex-col gap-2.5">
          {providers?.google && (
            <Button variant="secondary" className="flex items-center justify-center gap-2.5" onClick={() => loginWithGoogle()}>
              <GoogleIcon />
              Continue with Google
            </Button>
          )}
          {providers?.entra && (
            <Button variant="secondary" className="flex items-center justify-center gap-2.5" onClick={() => loginWithEntra()}>
              <MicrosoftIcon />
              Continue with Microsoft Entra ID
            </Button>
          )}
          {providers && !providers.google && !providers.entra && (
            <p className="text-sm text-muted">No sign-in method is configured.</p>
          )}
        </div>
      </div>
    </div>
  );
}
