const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

async function apiFetch<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(options?.headers || {}),
    },
    credentials: 'include',
  });

  if (!response.ok) {
    throw new Error(
      `Error ${response.status}: ${response.statusText} (${await response.text()})`
    );
  }

  return response.json();
}

export { apiFetch };