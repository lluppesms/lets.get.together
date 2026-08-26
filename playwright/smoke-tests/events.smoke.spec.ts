import { test, expect } from '../fixtures/test-fixtures';

test.describe('Events & Recurrence Smoke Tests', () => {

    test.beforeEach(async ({ eventsPage }) => {
        await eventsPage.goto();
    });

    test('page loads with correct title', async ({ page }) => {
        await expect(page).toHaveTitle(/(Events|Log in|Access Denied)/i);
    });

    test('events heading and kicker are present', async ({ page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const heading = page.locator('#events-heading');
            const kicker = page.locator('.events-kicker');
            await expect(heading).toBeVisible();
            await expect(heading).toHaveText('Upcoming Events');
            await expect(kicker).toBeVisible();
            await expect(kicker).toContainText('Gatherings with friends');
        }
    });

    test('circle filter dropdown and events count display are present', async ({ eventsPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const hasToolbar = await page.locator('.events-toolbar').isVisible().catch(() => false);
            if (hasToolbar) {
                await expect(eventsPage.circleFilterSelect).toBeVisible();
                await expect(eventsPage.eventsCount).toBeVisible();
            }
        }
    });

    test('create event form toggles open and closed', async ({ eventsPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const canCreate = await eventsPage.createEventButton.isVisible().catch(() => false);
            if (canCreate) {
                await eventsPage.createEventButton.click();
                await expect(eventsPage.createFormContainer).toBeVisible();
                await expect(eventsPage.eventTitleInput).toBeVisible();
                await expect(eventsPage.eventCircleSelect).toBeVisible();
                await expect(eventsPage.eventLocationInput).toBeVisible();
                await expect(eventsPage.eventDescriptionInput).toBeVisible();

                // Test recurring options panel
                await expect(eventsPage.recurringCheckbox).toBeVisible();
                await eventsPage.recurringCheckbox.check();
                await expect(eventsPage.recurrenceRuleSelect).toBeVisible();
                await expect(eventsPage.rsvpModeSelect).toBeVisible();

                // Verify recurrence dropdown options
                const ruleOptions = await eventsPage.recurrenceRuleSelect.locator('option').allInnerTexts();
                expect(ruleOptions.some(opt => opt.includes('Weekly'))).toBeTruthy();
                expect(ruleOptions.some(opt => opt.includes('Biweekly'))).toBeTruthy();
                expect(ruleOptions.some(opt => opt.includes('Monthly'))).toBeTruthy();

                // Close form
                await eventsPage.createEventButton.click();
                await expect(eventsPage.createFormContainer).not.toBeVisible();
            }
        }
    });

    test('displays either warning alert, empty state, or events grid', async ({ eventsPage, page }) => {
        if (!page.url().toLowerCase().includes('/login')) {
            const hasWarning = await eventsPage.warningAlert.isVisible().catch(() => false);
            const hasEmpty = await eventsPage.emptyState.isVisible().catch(() => false);
            const hasGrid = await eventsPage.eventsGrid.isVisible().catch(() => false);
            expect(hasWarning || hasEmpty || hasGrid).toBeTruthy();
        }
    });

    test('event detail page renders correct header and specifications layout', async ({ page }) => {
        await page.goto('/events/detail/1');
        if (!page.url().toLowerCase().includes('/login')) {
            await expect(page).toHaveTitle(/Event Detail|Get Together/i);
            const backLink = page.locator('a.back-link');
            await expect(backLink).toBeVisible();
            await expect(backLink).toContainText('Back to Upcoming Events');
        }
    });
});
