import { toast } from "sonner";
import { getSession } from "next-auth/react";
import { MOCK_DATA_ENABLED } from "./config";
import { getMockResponse } from "./mock-data/mock-interceptor";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5001";
const IS_DEV = process.env.NODE_ENV === "development";

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  if (MOCK_DATA_ENABLED) {
    const mock = getMockResponse<T>(path, options);
    if (mock !== undefined) return mock;
  }

  const url = `${API_BASE_URL}${path}`;

  // In development, send NO Authorization header so the backend DevAuthHandler
  // auto-authenticates the request with the seeded dev identity.
  // DevAuthHandler returns NoResult() whenever an Authorization header is present,
  // which would fall through to JWT validation and produce a 401.
  let authHeader: Record<string, string> = {};
  if (typeof window !== "undefined" && !IS_DEV) {
    const session = await getSession();
    if (session) {
      authHeader = { Authorization: `Bearer ${(session as any).accessToken}` };
    }
  }

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...authHeader,
    ...(options.headers as Record<string, string>),
  };

  try {
    const response = await fetch(url, { ...options, headers });

    if (!response.ok) {
      throw new Error(await response.text());
    }

    return response.json();
  } catch (error) {
    toast.error(error instanceof Error ? error.message : "An error occurred.");
    throw error;
  }
}