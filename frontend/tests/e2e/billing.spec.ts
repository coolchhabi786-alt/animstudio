import { test, expect } from '@playwright/test';

test.describe('Billing Flow', () => {
  test('Given billing page, When plans render, Then user should see all active plans', async ({ page }) => {
    await page.goto('/settings/billing');
    const plans = await page.locator('[data-testid="plan-card"]');

    await expect(plans).toHaveCount(3); // Assume 3 plans as example
    await expect(plans.first()).toContainText('$10 / month');
  });

  test('Given checkout request, When redirecting, Then Stripe checkout should appear', async ({ page }) => {
    await page.goto('/settings/billing');
    await page.click('[data-testid="checkout-button"]');

    await expect(page.url()).toContain('https://checkout.stripe.com');
  });

  test('Given billing portal redirect, When redirect triggered, Then Stripe portal appears', async ({ page }) => {
    await page.goto('/settings/billing');
    await page.click('[data-testid="portal-button"]');

    await expect(page.url()).toContain('https://billing.stripe.com');
  });
});