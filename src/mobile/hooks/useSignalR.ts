import { useEffect, useRef, useCallback } from 'react';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { getScanHubConnection, getNotificationsHubConnection } from '@/services/signalr';

type HubType = 'scan' | 'notifications';

interface UseSignalROptions {
  hub: HubType;
  autoConnect?: boolean;
}

export function useSignalR({ hub, autoConnect = true }: UseSignalROptions) {
  const connectionRef = useRef<HubConnection | null>(null);
  const listenersRef = useRef<Map<string, (...args: unknown[]) => void>>(new Map());

  const getConnection = useCallback(async (): Promise<HubConnection> => {
    if (hub === 'scan') {
      return getScanHubConnection();
    }
    return getNotificationsHubConnection();
  }, [hub]);

  const connect = useCallback(async () => {
    try {
      const connection = await getConnection();
      connectionRef.current = connection;

      listenersRef.current.forEach((callback, event) => {
        connection.on(event, callback);
      });

      return connection;
    } catch (error) {
      console.error(`[SignalR] Failed to connect to ${hub} hub:`, error);
      throw error;
    }
  }, [getConnection, hub]);

  const disconnect = useCallback(async () => {
    if (connectionRef.current && connectionRef.current.state !== HubConnectionState.Disconnected) {
      listenersRef.current.forEach((callback, event) => {
        connectionRef.current?.off(event, callback);
      });
      await connectionRef.current.stop();
      connectionRef.current = null;
    }
  }, []);

  const on = useCallback((event: string, callback: (...args: unknown[]) => void) => {
    listenersRef.current.set(event, callback);
    if (connectionRef.current && connectionRef.current.state === HubConnectionState.Connected) {
      connectionRef.current.on(event, callback);
    }

    return () => {
      listenersRef.current.delete(event);
      connectionRef.current?.off(event, callback);
    };
  }, []);

  const invoke = useCallback(async (method: string, ...args: unknown[]) => {
    if (!connectionRef.current || connectionRef.current.state !== HubConnectionState.Connected) {
      await connect();
    }
    return connectionRef.current?.invoke(method, ...args);
  }, [connect]);

  useEffect(() => {
    if (autoConnect) {
      connect();
    }
    return () => {
      disconnect();
    };
  }, [autoConnect, connect, disconnect]);

  return {
    connection: connectionRef.current,
    connect,
    disconnect,
    on,
    invoke,
  };
}
