import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';

// Load environment variables from .env
dotenv.config();

const baseURL = process.env.E2E_BASE_URL || 'http://localhost:3000';

export default defineConfig({
  testDir: './e2e',
  timeout: 30 * 1000,
  expect: {
    timeout: 5000,
  },
  fullyParallel: true,
  retries: process.env.CI ? 2 : 0,
  projects: [
    {
      name: 'Chromium',
      use: { ...devices['Desktop Chrome'], baseURL },
    },
    {
      name: 'Firefox',
      use: { ...devices['Desktop Firefox'], baseURL },
    },
    {
      name: 'Webkit',
      use: { ...devices['Desktop Safari'], baseURL },
    },
  ],
  reporter: 'html',
});