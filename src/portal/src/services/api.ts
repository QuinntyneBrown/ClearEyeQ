import { getToken } from "@/lib/auth";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || "";

export interface ApiError {
  status: number;
  message: string;
  details?: Record<string, string[]>;
}

export class ApiRequestError extends Error {
  public readonly status: number;
  public readonly details?: Record<string, string[]>;

  constructor(error: ApiError) {
    super(error.message);
    this.name = "ApiRequestError";
    this.status = error.status;
    this.details = error.details;
  }
}

interface RequestOptions extends Omit<RequestInit, "body"> {
  body?: unknown;
  tenantId?: string;
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let errorBody: Record<string, unknown> = {};
    try {
      errorBody = await response.json();
    } catch {
      // response body is not JSON
    }

    throw new ApiRequestError({
      status: response.status,
      message:
        (errorBody.message as string) ||
        (errorBody.title as string) ||
        `Request failed with status ${response.status}`,
      details: errorBody.errors as Record<string, string[]> | undefined,
    });
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function api<T>(
  endpoint: string,
  options: RequestOptions = {}
): Promise<T> {
  const { body, tenantId, headers: customHeaders, ...restOptions } = options;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(customHeaders as Record<string, string>),
  };

  const token = getToken();
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  if (tenantId) {
    headers["X-Tenant-Id"] = tenantId;
  }

  const config: RequestInit = {
    ...restOptions,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  };

  const url = `${BASE_URL}${endpoint}`;
  const response = await fetch(url, config);
  return handleResponse<T>(response);
}

export const apiGet = <T>(endpoint: string, options?: RequestOptions) =>
  api<T>(endpoint, { ...options, method: "GET" });

export const apiPost = <T>(
  endpoint: string,
  body?: unknown,
  options?: RequestOptions
) => api<T>(endpoint, { ...options, method: "POST", body });

export const apiPut = <T>(
  endpoint: string,
  body?: unknown,
  options?: RequestOptions
) => api<T>(endpoint, { ...options, method: "PUT", body });

export const apiPatch = <T>(
  endpoint: string,
  body?: unknown,
  options?: RequestOptions
) => api<T>(endpoint, { ...options, method: "PATCH", body });

export const apiDelete = <T>(endpoint: string, options?: RequestOptions) =>
  api<T>(endpoint, { ...options, method: "DELETE" });
