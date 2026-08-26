import { test, expect } from '../fixtures/test-fixtures';

test.describe('Search Page Smoke Tests', () => {

    test.beforeEach(async ({ searchPage }) => {
        await searchPage.goto();
    });

    test('page loads with correct heading', async ({ searchPage }) => {
        await expect(searchPage.heading).toBeVisible();
    });

    test('search text input is visible', async ({ searchPage }) => {
        await expect(searchPage.searchInput).toBeVisible();
    });

    test('search button is visible', async ({ searchPage }) => {
        await expect(searchPage.searchButton).toBeVisible();
    });

    test('can trigger search', async ({ searchPage }) => {
        await searchPage.searchFor('pickleball');
        await expect(searchPage.searchButton).toBeVisible();
    });
});
