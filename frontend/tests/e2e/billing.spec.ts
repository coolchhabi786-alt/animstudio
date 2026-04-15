import { test, expect } from '@playwright/test';

test('GivenExistingPlan_WhenBillingPageIsLoaded_ThenDisplaysPlanDetails', async ({ page }) => {
  // Arrange
  await page.goto('/billing');

  // Assert
  await expect(page.locator('h2')).toHaveText(/Current Plan/);
  await expect(page.locator('.plan-name')).toBeVisible();
  await expect(page.locator('.usage-progress-bar')).toBeVisible();
});

test('GivenRedirectToCheckout_WhenUpgradeIsAttempted_ThenNavigatesToStripeCheckout', async ({ page }) => {
  // Arrange
  await page.goto('/billing');

  // Act
  await page.click("button:has-text('Upgrade')");

  // Assert
  await expect(page).toHaveURL(/stripe/i);
});

test('GivenRedirectToPortal_WhenManageBillingIsClicked_ThenNavigatesToStripePortal', async ({ page }) => {
  // Arrange
  await page.goto('/billing');

  // Act
  await page.click("button:has-text('Manage Billing')");

  // Assert
  await expect(page).toHaveURL(/billingportal/i);
});