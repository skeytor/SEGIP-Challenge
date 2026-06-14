const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080";

type HttpMethod = "GET" | "POST" | "PATCH" | "DELETE";

async function request<T>(
  endpoint: string,
  method: HttpMethod = "GET",
  body?: unknown,
): Promise<T> {
  const response = await fetch(`${BASE_URL}${endpoint}`, {
    method,
    headers: {
      "Content-Type": "application/json",
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
    cache: "no-store",
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    throw new ApiError(response.status, error?.detail ?? response.statusText);
  }

  return response.json() as Promise<T>;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export const api = {
  get: <T>(endpoint: string) => request<T>(endpoint, "GET"),

  post: <T>(endpoint: string, body: unknown) =>
    request<T>(endpoint, "POST", body),

  patch: <T>(endpoint: string, body?: unknown) =>
    request<T>(endpoint, "PATCH", body),
};
