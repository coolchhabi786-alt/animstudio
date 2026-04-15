export function generateCorrelationId(): string {
  return crypto.randomUUID();
}

export function attachCorrelationId(headers: Headers): Headers {
  headers.set("X-Correlation-ID", generateCorrelationId());
  return headers;
}