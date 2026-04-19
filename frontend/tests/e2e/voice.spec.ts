import { test, expect } from '@playwright/test';

const PROJECT_ID = process.env.TEST_PROJECT_ID || '00000000-0000-0000-0000-000000000010';
const EPISODE_ID = process.env.TEST_EPISODE_ID || '00000000-0000-0000-0000-000000000011';

test.describe('Voice Studio', () => {
  test('GivenNoCharacters_WhenPageLoads_ThenShowsEmptyState', async ({ page }) => {
    // Arrange: mock the GET voices endpoint to return empty array
    await page.route('**/api/v1/episodes/*/voices', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      } else {
        await route.continue();
      }
    });

    // Act
    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/voice`);

    // Assert
    await expect(page.getByText('No Characters Assigned')).toBeVisible();
    await expect(page.getByText('Add characters to this episode first')).toBeVisible();
  });

  test('GivenVoiceAssignments_WhenPageLoads_ThenRendersCharacterRows', async ({ page }) => {
    // Arrange
    await page.route('**/api/v1/episodes/*/voices', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              id: 'va-1',
              episodeId: EPISODE_ID,
              characterId: 'char-1',
              characterName: 'Alice',
              voiceName: 'Nova',
              language: 'en-US',
              voiceCloneUrl: null,
              updatedAt: new Date().toISOString(),
            },
            {
              id: 'va-2',
              episodeId: EPISODE_ID,
              characterId: 'char-2',
              characterName: 'Bob',
              voiceName: 'Echo',
              language: 'es-ES',
              voiceCloneUrl: null,
              updatedAt: new Date().toISOString(),
            },
          ]),
        });
      } else {
        await route.continue();
      }
    });

    // Mock subscription to non-Studio tier
    await page.route('**/api/v1/billing/subscription', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'sub-1',
          planName: 'Pro',
          status: 'Active',
          episodesUsedThisMonth: 2,
          episodesPerMonth: 20,
          cancelAtPeriodEnd: false,
          stripeCustomerId: 'cus_test',
        }),
      });
    });

    // Act
    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/voice`);

    // Assert — header
    await expect(page.getByRole('heading', { name: 'Voice Studio' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Save All' })).toBeVisible();

    // Assert — character rows
    await expect(page.getByText('Alice')).toBeVisible();
    await expect(page.getByText('Bob')).toBeVisible();
    await expect(page.getByText('Nova')).toBeVisible();
    await expect(page.getByText('Echo')).toBeVisible();

    // Assert — preview buttons
    const previewButtons = page.getByRole('button', { name: 'Preview' });
    await expect(previewButtons).toHaveCount(2);

    // Assert — voice clone section shows tier gate
    await expect(page.getByText('Studio Tier Required')).toBeVisible();
  });

  test('GivenVoicePreviewClicked_WhenAPIResponds_ThenShowsAudioPlayer', async ({ page }) => {
    // Arrange: set up voice assignments
    await page.route('**/api/v1/episodes/*/voices', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              id: 'va-1',
              episodeId: EPISODE_ID,
              characterId: 'char-1',
              characterName: 'Alice',
              voiceName: 'Alloy',
              language: 'en-US',
              voiceCloneUrl: null,
              updatedAt: new Date().toISOString(),
            },
          ]),
        });
      } else {
        await route.continue();
      }
    });

    // Mock TTS preview endpoint
    await page.route('**/api/v1/voices/preview', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          audioUrl: 'data:audio/mpeg;base64,SUQzBAAAAAAAI1RTU0UAAAAPAAADTGF2',
          expiresAt: new Date(Date.now() + 60000).toISOString(),
        }),
      });
    });

    // Act
    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/voice`);
    await page.getByRole('button', { name: 'Preview' }).click();

    // Assert — play button should appear after preview loads
    await expect(page.getByRole('button', { name: 'Play preview' })).toBeVisible();
  });
});
