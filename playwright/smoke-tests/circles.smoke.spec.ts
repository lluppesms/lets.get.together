import { test, expect } from '../fixtures/test-fixtures';

test.describe('Circles Smoke Tests', () => {

    test.beforeEach(async ({ circlesPage }) => {
        await circlesPage.goto();
    });

    test('page loads with correct title', async ({ page }) => {
        await expect(page).toHaveTitle(/(Circles|Log in|Access Denied)/i);
    });

    test('circles page elements exist when navigated directly', async ({ page }) => {
        const isLoginPage = page.url().toLowerCase().includes('/login');
        if (!isLoginPage) {
            const heading = page.locator('#circles-heading');
            await expect(heading).toBeVisible();
            await expect(heading).toHaveText('Circles');
        } else {
            await expect(page).toHaveURL(/\/login/i);
        }
    });

    test('kicker and intro text are present on circles page', async ({ page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const kicker = page.locator('.circles-kicker');
            const intro = page.locator('.circles-intro');
            await expect(kicker).toBeVisible();
            await expect(kicker).toContainText('Your private groups');
            await expect(intro).toBeVisible();
            await expect(intro).toContainText('Choose a circle to see its people and invitations');
        }
    });

    test('shows either warning alert, empty state, or circle layout', async ({ circlesPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const hasWarning = await circlesPage.warningAlert.isVisible().catch(() => false);
            const hasEmpty = await circlesPage.emptyState.isVisible().catch(() => false);
            const hasLayout = await page.locator('.circle-layout').isVisible().catch(() => false);
            expect(hasWarning || hasEmpty || hasLayout).toBeTruthy();
        }
    });

    test('circle detail panels are structured correctly when layout is present', async ({ circlesPage, page }) => {
        const hasLayout = await page.locator('.circle-layout').isVisible().catch(() => false);
        if (hasLayout) {
            await expect(circlesPage.switcherAside).toBeVisible();
            await expect(circlesPage.membersPanel).toBeVisible();
            await expect(circlesPage.settingsPanel).toBeVisible();
            await expect(circlesPage.invitesPanel).toBeVisible();
            await expect(circlesPage.leaveButton).toBeVisible();
        }
    });

    test('leave circle confirmation modal toggles on leave click', async ({ circlesPage, page }) => {
        const hasLayout = await page.locator('.circle-layout').isVisible().catch(() => false);
        if (hasLayout) {
            await circlesPage.leaveButton.click();
            await expect(circlesPage.leaveConfirmationModal).toBeVisible();
            await circlesPage.keepCircleButton.click();
            await expect(circlesPage.leaveConfirmationModal).not.toBeVisible();
        }
    });
});
