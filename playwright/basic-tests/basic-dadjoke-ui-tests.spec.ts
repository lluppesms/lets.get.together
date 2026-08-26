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
