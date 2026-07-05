import { useEffect, useState } from 'react';
import { Navigate, useSearchParams } from 'react-router-dom';
import HiveLogo from '../../components/layout/HiveLogo';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';
import { useAuthProviders } from '../../hooks/data/useAuthProviders';
import { GoogleIcon, MicrosoftIcon } from './ProviderIcons';
import HiveWatermark from './HiveWatermark';

export default function Login() {
  const { isAuthenticated, loading, loginWithGoogle, loginWithEntra, loginWithBasic } = useAuth();
  const toast = useToast();
  const [searchParams, setSearchParams] = useSearchParams();
  const providers = useAuthProviders();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [signingIn, setSigningIn] = useState(false);

  useEffect(() => {
    if (searchParams.get('error') === 'access_denied') {
      toast.error("That account isn't allowed to sign in to Hively.");
      setSearchParams({}, { replace: true });
    }
  }, [searchParams, toast, setSearchParams]);

  if (!loading && isAuthenticated) {
    return <Navigate to="/topics" replace />;
  }

  async function handleBasicLogin(e: React.FormEvent) {
    e.preventDefault();
    setSigningIn(true);
    try {
      await loginWithBasic(username, password);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to sign in');
      setSigningIn(false);
    }
  }

  const hasOAuthProvider = providers?.google.enabled || providers?.entra.enabled;

  return (
    <div className="relative flex h-screen items-center justify-center overflow-hidden bg-bg text-text">
      <HiveWatermark />
      <div className="relative w-full max-w-sm rounded-xl border border-border bg-panel p-8 text-center">
        <div className="mb-8 flex items-center justify-center gap-2.5">
          <HiveLogo size={36} />
          <span className="font-brand text-xl font-semibold tracking-wide text-text/90">HIVELY</span>
        </div>
        <div className="flex flex-col gap-2.5">
          {providers?.google.enabled && (
            <Button
              variant="secondary"
              className="flex items-center justify-center gap-2.5"
              onClick={() => loginWithGoogle()}
              disabled={!providers.google.configured}
              title={providers.google.configured ? undefined : 'Enabled but not configured — missing ClientId/ClientSecret'}
            >
              <GoogleIcon />
              Continue with Google
            </Button>
          )}
          {providers?.entra.enabled && (
            <Button
              variant="secondary"
              className="flex items-center justify-center gap-2.5"
              onClick={() => loginWithEntra()}
              disabled={!providers.entra.configured}
              title={providers.entra.configured ? undefined : 'Enabled but not configured — missing ClientId/ClientSecret/TenantId'}
            >
              <MicrosoftIcon />
              Continue with Microsoft Entra ID
            </Button>
          )}
          {providers && !providers.google.enabled && !providers.entra.enabled && !providers.basic && (
            <p className="text-sm text-muted">No sign-in method is configured.</p>
          )}
        </div>

        {providers?.basic && (
          <>
            {hasOAuthProvider && (
              <div className="my-4 flex items-center gap-2.5 text-[11px] text-muted">
                <div className="h-px flex-1 bg-border" />
                or
                <div className="h-px flex-1 bg-border" />
              </div>
            )}
            <form onSubmit={handleBasicLogin} className="flex flex-col gap-2.5">
              <Input
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="username"
                autoComplete="username"
                disabled={signingIn}
              />
              <Input
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="password"
                type="password"
                autoComplete="current-password"
                disabled={signingIn}
              />
              <Button type="submit" variant="primary" isLoading={signingIn} disabled={!username || !password}>
                Sign in
              </Button>
            </form>
          </>
        )}
      </div>
    </div>
  );
}
