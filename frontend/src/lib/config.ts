/** Set NEXT_PUBLIC_MOCK_DATA=true in .env.local to intercept API calls with local mock data. */
export const MOCK_DATA_ENABLED =
  process.env.NEXT_PUBLIC_MOCK_DATA === "true"
