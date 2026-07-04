import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { createTopicHubConnection } from '../services/signalr';
import { useAuth } from './AuthContext';

interface SignalRContextType {
  topicHubConnection: HubConnection | null;
}

const SignalRContext = createContext<SignalRContextType>({ topicHubConnection: null });

/**
 * Owns one shared TopicHub connection for the whole app, started once the
 * user is authenticated (the hub is [Authorize]-gated) and stopped on
 * sign-out. Consumers (e.g. useTopics) attach/detach their own `.on()`
 * handlers to the connection returned here.
 */
export function SignalRProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const connectionRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    if (!isAuthenticated) {
      connectionRef.current?.stop();
      connectionRef.current = null;
      setConnection(null);
      return;
    }

    const hubConnection = createTopicHubConnection();
    connectionRef.current = hubConnection;
    hubConnection
      .start()
      .then(() => setConnection(hubConnection))
      .catch((err) => console.error('TopicHub connection failed:', err));

    return () => {
      hubConnection.stop();
      if (connectionRef.current === hubConnection) connectionRef.current = null;
    };
  }, [isAuthenticated]);

  return <SignalRContext.Provider value={{ topicHubConnection: connection }}>{children}</SignalRContext.Provider>;
}

export function useSignalR() {
  return useContext(SignalRContext);
}
