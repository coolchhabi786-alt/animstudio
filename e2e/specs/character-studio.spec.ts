import { test, expect, Page } from "@playwright/test";

/**
 * E2E tests for the Character Studio (Phase 4).
 *
 * These tests run against the full stack (Next.js + .NET API + SQL DB).
 * Auth state is pre-seeded by global.setup.ts — dev auth bypass is used
 * so no Entra credentials are needed in CI.
 *
 * Prerequisites:
 *   - `docker compose up` from the repo root (sets up SQL + API + frontend)
 *   - OR run API + frontend locally and set PLAYWRIGHT_BASE_URL accordingly
 */

const CHARACTER_NAME = `E2E Cat ${Date.now()}`;
const TEST_PROJECT_URL = "/projects/e2e-test-project";

test.describe("Character Studio", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(`${TEST_PROJECT_URL}/characters`);
    await expect(page.getByRole("main")).toBeVisible();
  });

  // ── TC-CS-01: Page renders ────────────────────────────────────────────────

  test("TC-CS-01 page renders without errors", async ({ page }) => {
    await expect(page.getByRole("heading", { name: "Create Character" })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Team Characters" })).toBeVisible();
  });

  // ── TC-CS-02: Create character → 202 accepted ─────────────────────────────

  test("TC-CS-02 creates a character and shows Draft card", async ({ page }) => {
    await fillAndSubmitForm(page, CHARACTER_NAME);

    // Toast should appear
    await expect(page.getByText(/queued for training/i)).toBeVisible({ timeout: 5_000 });

    // A card with the character name should appear in the gallery
    await expect(
      page.getByRole("heading", { name: CHARACTER_NAME })
    ).toBeVisible({ timeout: 10_000 });
  });

  // ── TC-CS-03: Training badge shows Draft initially ────────────────────────

  test("TC-CS-03 new character card shows Draft badge", async ({ page }) => {
    await fillAndSubmitForm(page, `${CHARACTER_NAME}-badge`);

    const card = page.locator(`[aria-label*="${CHARACTER_NAME}-badge"]`).first();
    await expect(card).toBeVisible({ timeout: 10_000 });
    await expect(
      page.locator(`text=Draft`).first()
    ).toBeVisible({ timeout: 5_000 });
  });

  // ── TC-CS-04: Name field validation ──────────────────────────────────────

  test("TC-CS-04 shows validation error for empty name", async ({ page }) => {
    const submitBtn = page.getByRole("button", { name: /create character/i });
    await submitBtn.click();

    await expect(
      page.getByText(/name is required/i)
    ).toBeVisible({ timeout: 3_000 });
  });

  // ── TC-CS-05: Delete character ────────────────────────────────────────────

  test("TC-CS-05 deletes a character after confirming", async ({ page }) => {
    const nameToDelete = `${CHARACTER_NAME}-del`;
    await fillAndSubmitForm(page, nameToDelete);
    await page.waitForSelector(`text=${nameToDelete}`);

    // Hover card to reveal delete button, then click
    const card = page.locator(".group").filter({ hasText: nameToDelete }).first();
    await card.hover();

    page.on("dialog", (dialog) => dialog.accept());
    await card.getByRole("button", { name: /delete/i }).click();

    // Card should disappear
    await expect(
      page.getByRole("heading", { name: nameToDelete })
    ).not.toBeVisible({ timeout: 5_000 });
  });

  // ── TC-CS-06: Cost estimate visible ──────────────────────────────────────

  test("TC-CS-06 shows credit cost estimate in form", async ({ page }) => {
    await expect(
      page.getByText(/training this character costs/i)
    ).toBeVisible();
    await expect(page.getByText(/50 credits/i)).toBeVisible();
  });

  // ── TC-CS-07: BOLA — cannot access another team's characters ─────────────
  // This test relies on the API returning 403/404 for cross-team access.
  // Verified at the API layer via security review; E2E confirms the UI handles it.

  test("TC-CS-07 API returns 403 for cross-team character fetch", async ({
    page,
    request,
  }) => {
    // Attempt to fetch a known other-team character UUID
    const otherTeamCharacterId = "00000000-0000-0000-0000-000000000001";
    const response = await request.get(
      `/api/v1/characters/${otherTeamCharacterId}`
    );
    expect(response.status()).toEqual(403);
  });
});

// ── Helpers ──────────────────────────────────────────────────────────────────

async function fillAndSubmitForm(page: Page, name: string) {
  await page.getByLabel("Name").fill(name);
  await page
    .getByRole("button", { name: /create character/i })
    .click();
}
