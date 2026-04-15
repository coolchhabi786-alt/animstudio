import { test, expect } from '@playwright/test';

test('GivenValidTeamData_WhenCreatingTeam_ThenAddsTeamSuccessfully', async ({ page }) => {
  // Arrange
  await page.goto('/settings/team');

  // Act
  await page.fill('input[name="teamName"]', 'New Team Name');
  await page.click('button:has-text("Create Team")');

  // Assert
  await expect(page.locator('.team-name')).toHaveText('New Team Name');
});

test('GivenValidInviteToken_WhenInviteAccepted_ThenUserJoinsTeam', async ({ page }) => {
  // Arrange
  const inviteToken = process.env.TEST_TEAM_INVITE_TOKEN || 'dummy-token';
  await page.goto(`/accept-invite?token=${inviteToken}`);

  // Act
  await page.click('button:has-text("Accept")');

  // Assert
  await expect(page.locator('.team-members')).toContainText('You');
});

test('GivenExistingTeam_WhenTeamPageLoaded_ThenDisplaysTeamDetails', async ({ page }) => {
  // Arrange
  await page.goto('/settings/team');

  // Assert
  await expect(page.locator('.team-name')).toBeVisible();
  await expect(page.locator('.team-members')).toBeVisible();
});