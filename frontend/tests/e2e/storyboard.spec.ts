import { test, expect } from '@playwright/test';

const PROJECT_ID = process.env.TEST_PROJECT_ID || '00000000-0000-0000-0000-000000000010';
const EPISODE_ID = process.env.TEST_EPISODE_ID || '00000000-0000-0000-0000-000000000011';

test.describe('Storyboard Studio', () => {
  test('GivenNoStoryboard_WhenPageLoads_ThenShowsEmptyStateAndCTA', async ({ page }) => {
    // Arrange: mock the GET storyboard endpoint to return 404 (no storyboard yet)
    await page.route('**/api/v1/episodes/*/storyboard', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 404,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'No storyboard', code: 'NO_STORYBOARD' }),
        });
      } else {
        await route.continue();
      }
    });

    // Act
    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/storyboard`);

    // Assert
    await expect(page.getByRole('heading', { name: 'Storyboard Studio' })).toBeVisible();
    await expect(page.getByText('No storyboard yet')).toBeVisible();
  });

  test('GivenStoryboardExists_WhenPageLoads_ThenRendersSceneGroups', async ({ page }) => {
    // Arrange
    await page.route('**/api/v1/episodes/*/storyboard', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'sb-1',
            episodeId: EPISODE_ID,
            screenplayTitle: 'My Screenplay',
            directorNotes: null,
            shots: [
              {
                id: 'shot-1',
                storyboardId: 'sb-1',
                sceneNumber: 1,
                shotIndex: 1,
                description: 'Wide establishing shot of the cafe.',
                imageUrl: 'https://placehold.co/640x360/png',
                styleOverride: null,
                regenerationCount: 0,
                updatedAt: new Date().toISOString(),
              },
              {
                id: 'shot-2',
                storyboardId: 'sb-1',
                sceneNumber: 1,
                shotIndex: 2,
                description: 'Close-up on protagonist sipping coffee.',
                imageUrl: 'https://placehold.co/640x360/png',
                styleOverride: 'noir film grain',
                regenerationCount: 4,
                updatedAt: new Date().toISOString(),
              },
            ],
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          }),
        });
      } else {
        await route.continue();
      }
    });

    // Act
    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/storyboard`);

    // Assert
    await expect(page.getByRole('heading', { name: 'Scene 1' })).toBeVisible();
    await expect(page.getByText('2 shots')).toBeVisible();
    // Regen count badge only shows when > 3
    await expect(page.getByText('4×')).toBeVisible();
    // Style override label on shot 2
    await expect(page.getByText('Style: noir film grain')).toBeVisible();
  });
});
