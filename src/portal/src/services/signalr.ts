import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
  HttpTransportType,
} from "@microsoft/signalr";
import { getToken } from "@/lib/auth";

const HUB_URL =
  (process.env.NEXT_PUBLIC_API_URL || "") + "/hubs/notifications";

let connection: HubConnection | null = null;

export function getConnection(): HubConnection {
  if (connection) return connection;

  connection = new HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => getToken() || "",
      transport: HttpTransportType.WebSockets,
      skipNegotiation: true,
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (retryContext) => {
        if (retryContext.elapsedMilliseconds < 60000) {
          return Math.random() * 10000;
        }
        return 30000;
      },
    })
    .configureLogging(LogLevel.Warning)
    .build();

  return connection;
}

export async function startConnection(): Promise<void> {
  const conn = getConnection();
  if (conn.state === "Disconnected") {
    await conn.start();
  }
}

export async function stopConnection(): Promise<void> {
  if (connection && connection.state !== "Disconnected") {
    await connection.stop();
  }
}

export function onEvent<T>(
  eventName: string,
  callback: (data: T) => void
): void {
  const conn = getConnection();
  conn.on(eventName, callback);
}

export function offEvent(eventName: string): void {
  const conn = getConnection();
  conn.off(eventName);
}
