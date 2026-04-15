import { test, expect } from '@playwright/test';

test('GivenValidCredentials_WhenLoginIsAttempted_ThenRedirectsToDashboard', async ({ page }) => {
  // Arrange
  await page.goto('/login');

  // Act
  await page.fill('input[name="email"]', process.env.TEST_USER_EMAIL || 'test@example.com');
  await page.fill('input[name="password"]', process.env.TEST_USER_PASSWORD || 'password123');
  await page.click('button[type="submit"]');

  // Assert
  await expect(page).toHaveURL('/dashboard');
  await expect(page.locator('h1')).toHaveText('Welcome');
});

test('GivenValidInviteToken_WhenSignupIsAttempted_ThenCreatesAccount', async ({ page }) => {
  // Arrange
  const inviteToken = process.env.TEST_INVITE_TOKEN || 'dummy-token';
  await page.goto(`/signup?invite=${inviteToken}`);

  // Act
  await page.fill('input[name="email"]', 'newuser@example.com');
  await page.fill('input[name="password"]', 'securepassword');
  await page.click('button[type="submit"]');

  // Assert
  await expect(page).toHaveURL('/dashboard');
  await expect(page.locator('h1')).toHaveText('Welcome');
});