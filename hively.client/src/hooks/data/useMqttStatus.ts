import { useEffect, useState } from 'react';
import { mqttStatusService } from '../../services/mqttStatusService';
import { useSignalR } from '../../contexts/SignalRContext';
import type { MqttStatus } from '../../types';

export function useMqttStatus() {
  const [status, setStatus] = useState<MqttStatus | null>(null);
  const { topicHubConnection } = useSignalR();

  useEffect(() => {
    mqttStatusService.getStatus().then(setStatus).catch(() => setStatus(null));
  }, []);

  useEffect(() => {
    if (!topicHubConnection) return;

    topicHubConnection.on('BrokerStatusChanged', setStatus);
    return () => {
      topicHubConnection.off('BrokerStatusChanged', setStatus);
    };
  }, [topicHubConnection]);

  return status;
}
