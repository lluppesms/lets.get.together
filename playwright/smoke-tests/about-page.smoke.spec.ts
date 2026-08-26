import { test, expect } from '../fixtures/test-fixtures';

test.describe('About Page Smoke Tests', () => {

    test.beforeEach(async ({ aboutPage }) => {
        await aboutPage.goto();
    });

    test('page loads with correct title', async ({ page }) => {
        await expect(page).toHaveTitle(/About/i);
    });

    test('about heading is visible', async ({ aboutPage }) => {
        await expect(aboutPage.title).toBeVisible();
    });

    test('contains Get Together subtitle', async ({ aboutPage }) => {
        await expect(aboutPage.subtitle).toContainText('Simple, private event planning');
    });

    test('description paragraph is visible', async ({ aboutPage }) => {
        await expect(aboutPage.description).toBeVisible();
        await expect(aboutPage.description).toContainText('informal gatherings');
    });

    test('about container is present', async ({ aboutPage }) => {
        await expect(aboutPage.aboutContainer).toBeVisible();
    });
});
