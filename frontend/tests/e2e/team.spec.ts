import { test, expect } from '@playwright/test';

test.describe('Team Management Flow', () => {
  test('Given team creation, When form is submitted, Then new team should be listed', async ({ page }) => {
    await page.goto('/settings/team');
    await page.fill('[data-testid="team-name-input"]', 'Test Team');
    await page.click('[data-testid="create-team-button"]');

    await expect(page.locator('[data-testid="team-list"]').first()).toContainText('Test Team');
  });

  test('Given member invitation, When email is submitted, Then invite should be sent', async ({ page }) => {
    await page.goto('/settings/team');
    await page.fill('[data-testid="invite-email-input"]', 'invitee@example.com');
    await page.click('[data-testid="send-invite-button"]');

    await expect(page.locator('[data-testid="invite-success-message"]')).toContainText('Invite sent successfully');
  });

  test('Given accepted invite token, When clicked, Then user should join team', async ({ page }) => {
    const inviteToken = 'validInviteToken123';
    await page.goto(`/accept-invite?token=${inviteToken}`);

    await expect(page.locator('[data-testid="team-dashboard"]')).toContainText('Welcome to the team');
  });
});