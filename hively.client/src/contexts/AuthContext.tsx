import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';
import { authService } from '../services/authService';
import type { CurrentUser } from '../types';

interface AuthContextType {
  user: CurrentUser | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  loading: boolean;
  loginWithGoogle: (returnUrl?: string) => void;
  loginWithEntra: (returnUrl?: string) => void;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    authService.getCurrentUser().then((currentUser) => {
      setUser(currentUser);
      setLoading(false);
    });
  }, []);

  // Full-page redirect — the browser has to leave the SPA to complete the
  // Google/Entra OIDC handshake, so this can't be a fetch() call.
  const loginWithGoogle = useCallback((returnUrl = '/') => {
    window.location.href = authService.googleLoginUrl(returnUrl);
  }, []);

  const loginWithEntra = useCallback((returnUrl = '/') => {
    window.location.href = authService.entraLoginUrl(returnUrl);
  }, []);

  const logout = useCallback(async () => {
    await authService.logout();
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: user !== null,
        isAdmin: user?.role === 'Admin',
        loading,
        loginWithGoogle,
        loginWithEntra,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
