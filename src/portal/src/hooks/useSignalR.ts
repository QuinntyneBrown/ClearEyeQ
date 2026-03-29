"use client";

import { useEffect, useRef, useCallback } from "react";
import {
  startConnection,
  stopConnection,
  onEvent,
  offEvent,
  getConnection,
} from "@/services/signalr";

interface UseSignalROptions {
  enabled?: boolean;
}

export function useSignalR(options: UseSignalROptions = {}) {
  const { enabled = true } = options;
  const isConnected = useRef(false);

  useEffect(() => {
    if (!enabled) return;

    startConnection()
      .then(() => {
        isConnected.current = true;
      })
      .catch((err) => {
        console.error("SignalR connection failed:", err);
      });

    return () => {
      stopConnection();
      isConnected.current = false;
    };
  }, [enabled]);

  const on = useCallback(<T>(eventName: string, callback: (data: T) => void) => {
    onEvent(eventName, callback);
    return () => offEvent(eventName);
  }, []);

  const invoke = useCallback(async (methodName: string, ...args: unknown[]) => {
    const conn = getConnection();
    if (conn.state === "Connected") {
      await conn.invoke(methodName, ...args);
    }
  }, []);

  return {
    on,
    invoke,
    isConnected: isConnected.current,
  };
}
