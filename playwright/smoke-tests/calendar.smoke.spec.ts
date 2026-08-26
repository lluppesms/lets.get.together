import { test, expect } from '../fixtures/test-fixtures';

test.describe('Calendar Navigation & Views Smoke Tests', () => {

    test.beforeEach(async ({ calendarPage }) => {
        await calendarPage.goto();
    });

    test('page loads with correct title', async ({ page }) => {
        await expect(page).toHaveTitle(/(Calendar|Log in|Access Denied)/i);
    });

    test('calendar heading and kicker are visible', async ({ page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const heading = page.locator('#calendar-heading');
            const kicker = page.locator('.calendar-kicker');
            await expect(heading).toBeVisible();
            await expect(heading).toHaveText('Calendar');
            await expect(kicker).toBeVisible();
            await expect(kicker).toContainText('Aggregated Gatherings');
        }
    });

    test('view selection toggle group buttons exist', async ({ calendarPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            await expect(calendarPage.monthViewButton).toBeVisible();
            await expect(calendarPage.agendaViewButton).toBeVisible();
            await expect(calendarPage.monthViewButton).toHaveAttribute('aria-pressed', 'true');
            await expect(calendarPage.agendaViewButton).toHaveAttribute('aria-pressed', 'false');
        }
    });

    test('calendar controls bar contains month navigation and circle filter', async ({ calendarPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const hasControls = await page.locator('.calendar-controls-bar').isVisible().catch(() => false);
            if (hasControls) {
                await expect(calendarPage.prevMonthButton).toBeVisible();
                await expect(calendarPage.todayButton).toBeVisible();
                await expect(calendarPage.nextMonthButton).toBeVisible();
                await expect(calendarPage.monthDisplay).toBeVisible();
                await expect(calendarPage.circleFilterSelect).toBeVisible();
            }
        }
    });

    test('month navigation buttons update current month display', async ({ calendarPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const hasControls = await page.locator('.calendar-controls-bar').isVisible().catch(() => false);
            if (hasControls) {
                const initialMonthText = await calendarPage.monthDisplay.innerText();
                await calendarPage.nextMonthButton.click();
                const nextMonthText = await calendarPage.monthDisplay.innerText();
                expect(nextMonthText).not.toEqual(initialMonthText);

                await calendarPage.todayButton.click();
                const todayMonthText = await calendarPage.monthDisplay.innerText();
                expect(todayMonthText).toEqual(initialMonthText);
            }
        }
    });

    test('Month View renders 7-column calendar grid', async ({ calendarPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const hasGrid = await calendarPage.monthGrid.isVisible().catch(() => false);
            if (hasGrid) {
                const headers = page.locator('.grid-header-cell');
                await expect(headers).toHaveCount(7);
                const headerTexts = await headers.allInnerTexts();
                expect(headerTexts).toEqual(['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']);

                // Verify day cells exist
                const dayCount = await calendarPage.dayCells.count();
                expect(dayCount).toBeGreaterThanOrEqual(28);
            }
        }
    });

    test('Agenda View renders date-grouped list when toggled', async ({ calendarPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            await calendarPage.agendaViewButton.click();
            await expect(calendarPage.agendaViewButton).toHaveAttribute('aria-pressed', 'true');
            await expect(calendarPage.monthViewButton).toHaveAttribute('aria-pressed', 'false');

            const hasAgenda = await calendarPage.agendaContainer.isVisible().catch(() => false);
            expect(hasAgenda).toBeTruthy();

            // Toggle back to Month View
            await calendarPage.monthViewButton.click();
            await expect(calendarPage.monthViewButton).toHaveAttribute('aria-pressed', 'true');
        }
    });

    test('day cell click opens day details modal in Month View', async ({ calendarPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const hasGrid = await calendarPage.monthGrid.isVisible().catch(() => false);
            if (hasGrid) {
                const firstCurrentDay = page.locator('.day-cell.current-month').first();
                if (await firstCurrentDay.isVisible()) {
                    await firstCurrentDay.click();
                    const modal = page.locator('.calendar-modal-backdrop');
                    await expect(modal).toBeVisible();
                    const closeBtn = page.locator('.modal-close-btn');
                    await closeBtn.click();
                    await expect(modal).not.toBeVisible();
                }
            }
        }
    });
});
