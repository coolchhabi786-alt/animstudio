import { test, expect } from '@playwright/test';

const fakeEmail = `user-${Date.now()}@example.com`;
const fakePassword = 'SecurePassword123';

// Test login flow

test.describe('Auth Flow', () => {
  test('Given valid credentials, When logging in, Then user should see dashboard', async ({ page }) => {
    await page.goto('/login');
    await page.fill('[data-testid="email"]', process.env.TEST_USER_EMAIL || fakeEmail);
    await page.fill('[data-testid="password"]', process.env.TEST_USER_PASSWORD || fakePassword);
    await page.click('[data-testid="login-button"]');

    expect(await page.url()).toContain('/dashboard');
  });

  test('Given invalid credentials, When logging in, Then error message should display', async ({ page }) => {
    await page.goto('/login');
    await page.fill('[data-testid="email"]', 'fakeuser@example.com');
    await page.fill('[data-testid="password"]', 'notarealpassword');
    await page.click('[data-testid="login-button"]');

    expect(await page.textContent('[data-testid="error-message"]')).toContain('Invalid credentials');
  });

  test('Given user signup, When form fills correct', () =>default & signup/new redirection?