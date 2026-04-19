import { test, expect } from '@playwright/test';

const PROJECT_ID = process.env.TEST_PROJECT_ID || '00000000-0000-0000-0000-000000000010';
const EPISODE_ID = process.env.TEST_EPISODE_ID || '00000000-0000-0000-0000-000000000011';

const TEAM_ID = '00000000-0000-0000-0000-000000000001';

function makeEstimate(backend: 'Kling' | 'Local', shotCount = 3) {
  const unit = backend === 'Kling' ? 0.056 : 0;
  return {
    episodeId: EPISODE_ID,
    backend,
    shotCount,
    unitCostUsd: unit,
    totalCostUsd: unit * shotCount,
    breakdown: Array.from({ length: shotCount }, (_, i) => ({
      sceneNumber: 1,
      shotIndex: i,
      storyboardShotId: `shot-${i}`,
      unitCostUsd: unit,
    })),
  };
}

async function mockTeam(page: import('@playwright/test').Page) {
  await page.route('**/api/teams/me', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ id: TEAM_ID, name: 'Acme' }),
    }),
  );
}

test.describe('Animation Studio', () => {
  test('GivenNoClips_WhenPageLoads_ThenShowsCostEstimateCard', async ({ page }) => {
    await mockTeam(page);

    await page.route('**/api/v1/episodes/*/animation/estimate**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(makeEstimate('Kling', 3)),
      }),
    );

    await page.route('**/api/v1/episodes/*/animation', async (route) => {
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

    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/animation`);

    await expect(page.getByRole('heading', { name: 'Animation Studio' })).toBeVisible();
    await expect(page.getByText('Render cost estimate')).toBeVisible();
    await expect(page.getByText('Kling (Cloud)')).toBeVisible();
    await expect(page.getByText('Local (On-prem)')).toBeVisible();
    await expect(page.getByText(/\$0\.168/)).toBeVisible(); // 3 × 0.056
    await expect(page.getByRole('button', { name: /Approve & render/ })).toBeEnabled();
  });

  test('GivenBackendToggled_WhenLocalSelected_ThenTotalIsZero', async ({ page }) => {
    await mockTeam(page);

    await page.route('**/api/v1/episodes/*/animation/estimate**', (route) => {
      const url = new URL(route.request().url());
      const backend = (url.searchParams.get('backend') as 'Kling' | 'Local') ?? 'Kling';
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(makeEstimate(backend, 3)),
      });
    });

    await page.route('**/api/v1/episodes/*/animation', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      }),
    );

    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/animation`);

    await page.getByRole('radio', { name: /Local/ }).click();

    await expect(page.getByText(/\$0\.00/).first()).toBeVisible();
  });

  test('GivenApprovalDialog_WhenConfirmed_ThenPostsApproval', async ({ page }) => {
    await mockTeam(page);

    await page.route('**/api/v1/episodes/*/animation/estimate**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(makeEstimate('Kling', 2)),
      }),
    );

    let approvalPosted = false;
    await page.route('**/api/v1/episodes/*/animation', async (route) => {
      const method = route.request().method();
      if (method === 'POST') {
        approvalPosted = true;
        await route.fulfill({
          status: 202,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'job-1',
            episodeId: EPISODE_ID,
            backend: 'Kling',
            estimatedCostUsd: 0.112,
            actualCostUsd: null,
            approvedByUserId: 'user-1',
            approvedAt: new Date().toISOString(),
            status: 'Approved',
            createdAt: new Date().toISOString(),
          }),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
    });

    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/animation`);

    await page.getByRole('button', { name: /Approve & render/ }).click();
    await expect(
      page.getByRole('heading', { name: 'Approve animation render?' }),
    ).toBeVisible();
    await page.getByRole('button', { name: /Approve & render 2 shots/ }).click();

    await expect.poll(() => approvalPosted).toBeTruthy();
  });

  test('GivenReadyClips_WhenPageLoads_ThenRendersClipGrid', async ({ page }) => {
    await mockTeam(page);

    await page.route('**/api/v1/episodes/*/animation/estimate**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(makeEstimate('Kling', 2)),
      }),
    );

    await page.route('**/api/v1/episodes/*/animation', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            {
              id: 'clip-1',
              episodeId: EPISODE_ID,
              sceneNumber: 1,
              shotIndex: 0,
              storyboardShotId: 'shot-0',
              clipUrl: 'clips/ep/clip-1.mp4',
              durationSeconds: 4.2,
              status: 'Ready',
              createdAt: new Date().toISOString(),
            },
            {
              id: 'clip-2',
              episodeId: EPISODE_ID,
              sceneNumber: 1,
              shotIndex: 1,
              storyboardShotId: 'shot-1',
              clipUrl: null,
              durationSeconds: null,
              status: 'Rendering',
              createdAt: new Date().toISOString(),
            },
          ]),
        });
      } else {
        await route.continue();
      }
    });

    await page.route('**/api/v1/episodes/*/animation/clips/*', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          clipId: 'clip-1',
          url: 'https://cdn.local/clips/ep/clip-1.mp4?se=abc',
          expiresAt: new Date(Date.now() + 60000).toISOString(),
        }),
      }),
    );

    await page.goto(`/projects/${PROJECT_ID}/episodes/${EPISODE_ID}/animation`);

    await expect(page.getByRole('heading', { name: /Clips/ })).toBeVisible();
    await expect(page.getByText('S1·0')).toBeVisible();
    await expect(page.getByText('S1·1')).toBeVisible();
    await expect(page.getByText('Ready').first()).toBeVisible();
    await expect(page.getByText('Rendering').first()).toBeVisible();
  });
});
