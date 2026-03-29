import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
  HttpTransportType,
} from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore';

const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL || 'http://localhost:5100';

function createHubConnection(hubPath: string): HubConnection {
  const connection = new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}${hubPath}`, {
      accessTokenFactory: () => useAuthStore.getState().token || '',
      transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (retryContext) => {
        const delays = [0, 1000, 2000, 5000, 10000, 30000];
        const index = Math.min(retryContext.previousRetryCount, delays.length - 1);
        return delays[index];
      },
    })
    .configureLogging(__DEV__ ? LogLevel.Information : LogLevel.Warning)
    .build();

  return connection;
}

let scanHubConnection: HubConnection | null = null;
let notificationsHubConnection: HubConnection | null = null;

export async function getScanHubConnection(): Promise<HubConnection> {
  if (!scanHubConnection) {
    scanHubConnection = createHubConnection('/hubs/scan');
  }
  if (scanHubConnection.state === 'Disconnected') {
    await scanHubConnection.start();
  }
  return scanHubConnection;
}

export async function getNotificationsHubConnection(): Promise<HubConnection> {
  if (!notificationsHubConnection) {
    notificationsHubConnection = createHubConnection('/hubs/notifications');
  }
  if (notificationsHubConnection.state === 'Disconnected') {
    await notificationsHubConnection.start();
  }
  return notificationsHubConnection;
}

export async function disconnectScanHub(): Promise<void> {
  if (scanHubConnection && scanHubConnection.state !== 'Disconnected') {
    await scanHubConnection.stop();
  }
  scanHubConnection = null;
}

export async function disconnectNotificationsHub(): Promise<void> {
  if (notificationsHubConnection && notificationsHubConnection.state !== 'Disconnected') {
    await notificationsHubConnection.stop();
  }
  notificationsHubConnection = null;
}

export async function disconnectAll(): Promise<void> {
  await Promise.all([disconnectScanHub(), disconnectNotificationsHub()]);
}
