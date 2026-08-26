import { test, expect } from '@playwright/test';

test('find Get Together home page', async ({ page, baseURL }) => {
  console.log('Test: Open Get Together website');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/');
  await expect(page).toHaveTitle(/Get Together/);
});

test('find Get Together search page', async ({ page, baseURL }) => {
  console.log('Test: Find Search page');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/');
  await page.getByRole('link', { name: 'Search' }).click();
  await expect(page.getByRole('heading', { name: 'Search Events' })).toBeVisible();
});

test('search for events', async ({ page, baseURL }) => {
  console.log('Test: Search for Events');
  console.log('Using Base URL: ' + baseURL);
  console.log('process.env.CI: ' + process.env.CI);
  console.log('process.env.TEST_ENVIRONMENT: ' + process.env.TEST_ENVIRONMENT);
  await page.goto('/search');
  await page.waitForSelector('#btnSearch', { state: 'visible' });
  await page.getByRole('textbox', { name: 'Search Term' }).click();
  await page.getByRole('textbox', { name: 'Search Term' }).fill('pickleball');
  await page.locator('#btnSearch').click();
  await page.waitForTimeout(1000);
  await expect(page.locator('#btnSearch')).toBeVisible();
});

