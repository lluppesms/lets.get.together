import { test, expect } from '../fixtures/test-fixtures';

test.describe('Home Page Smoke Tests', () => {

    test.beforeEach(async ({ homePage }) => {
        await homePage.goto();
    });

    test('page loads with correct title', async ({ page }) => {
        await expect(page).toHaveTitle(/Get Together/i);
    });

    test('heading is visible', async ({ homePage }) => {
        await expect(homePage.heading).toBeVisible();
    });

    test('lead description is visible', async ({ homePage }) => {
        await expect(homePage.leadText).toBeVisible();
        await expect(homePage.leadText).toContainText('Simple, private event planning');
    });

    test('hero card is present', async ({ homePage }) => {
        await expect(homePage.heroCard).toBeVisible();
    });

    test('feature action buttons are present', async ({ homePage }) => {
        await expect(homePage.manageCirclesButton).toBeVisible();
        await expect(homePage.viewEventsButton).toBeVisible();
        await expect(homePage.openCalendarButton).toBeVisible();
    });
});
