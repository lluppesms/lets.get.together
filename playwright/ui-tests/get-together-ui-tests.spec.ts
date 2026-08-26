import { test, expect } from '../fixtures/test-fixtures';

test.describe('Get Together End-to-End User Journey Tests', () => {

    test('1. Circle Management User Journey', async ({ circlesPage, page }) => {
        await circlesPage.goto();
        const isLogin = page.url().toLowerCase().includes('/login');
        if (!isLogin) {
            await expect(page).toHaveTitle(/Circles | Get Together/i);
            await expect(circlesPage.heading).toBeVisible();
            await expect(circlesPage.heading).toHaveText('Circles');
            await expect(circlesPage.intro).toContainText('Choose a circle to see its people and invitations');

            const hasLayout = await page.locator('.circle-layout').isVisible().catch(() => false);
            if (hasLayout) {
                // Circle Switcher
                await expect(circlesPage.switcherAside).toBeVisible();
                const choiceCount = await circlesPage.circleChoices.count();
                expect(choiceCount).toBeGreaterThan(0);

                // Circle Details
                await expect(circlesPage.circleDetailHeading).toBeVisible();
                await expect(circlesPage.membersPanel).toBeVisible();
                await expect(circlesPage.settingsPanel).toBeVisible();
                await expect(circlesPage.invitesPanel).toBeVisible();

                // Settings Form
                await expect(circlesPage.nameInput).toBeVisible();
                await expect(circlesPage.descriptionInput).toBeVisible();
                await expect(circlesPage.saveSettingsButton).toBeVisible();

                // Invites Generation
                await expect(circlesPage.inviteExpirationInput).toBeVisible();
                await expect(circlesPage.generateInviteButton).toBeVisible();

                // Leave Circle Confirmation
                await expect(circlesPage.leaveButton).toBeVisible();
                await circlesPage.leaveButton.click();
                await expect(circlesPage.leaveConfirmationModal).toBeVisible();
                await circlesPage.keepCircleButton.click();
                await expect(circlesPage.leaveConfirmationModal).not.toBeVisible();
            } else {
                // If SQL is disconnected, warning alert must be displayed safely
                await expect(circlesPage.warningAlert).toBeVisible();
                await expect(circlesPage.warningAlert).toContainText('Circle data is not available in this environment');
            }
        }
    });

    test('2. Event Creation & Recurrence Viewing User Journey', async ({ eventsPage, page }) => {
        await eventsPage.goto();
        const isLogin = page.url().toLowerCase().includes('/login');
        if (!isLogin) {
            await expect(page).toHaveTitle(/Events | Get Together/i);
            await expect(eventsPage.heading).toHaveText('Upcoming Events');

            const canCreate = await eventsPage.createEventButton.isVisible().catch(() => false);
            if (canCreate) {
                // Toggle create form open
                await eventsPage.createEventButton.click();
                await expect(eventsPage.createFormContainer).toBeVisible();

                // Fill event basic info
                await eventsPage.eventTitleInput.fill('Weekly Pickleball Session');
                await eventsPage.eventLocationInput.fill('Community Park Court 1');
                await eventsPage.eventDescriptionInput.fill('Bring your paddle and water bottle.');

                // Enable recurrence
                await eventsPage.recurringCheckbox.check();
                await expect(eventsPage.recurrenceRuleSelect).toBeVisible();
                await expect(eventsPage.rsvpModeSelect).toBeVisible();

                // Select Biweekly & PerOccurrence
                await eventsPage.recurrenceRuleSelect.selectOption('Biweekly');
                await eventsPage.rsvpModeSelect.selectOption({ index: 0 }); // PerOccurrence

                // Cancel creation
                await eventsPage.cancelFormButton.click();
                await expect(eventsPage.createFormContainer).not.toBeVisible();
            }

            // Verify event detail layout
            await page.goto('/events/detail/1');
            if (!page.url().toLowerCase().includes('/login')) {
                const backLink = page.locator('a.back-link');
                await expect(backLink).toBeVisible();
                await expect(backLink).toHaveAttribute('href', '/events');
            }
        }
    });

    test('3. Calendar Navigation & Month/Agenda Views Journey', async ({ calendarPage, page }) => {
        await calendarPage.goto();
        const isLogin = page.url().toLowerCase().includes('/login');
        if (!isLogin) {
            await expect(page).toHaveTitle(/Calendar | Get Together/i);
            await expect(calendarPage.heading).toHaveText('Calendar');

            // View toggle
            await expect(calendarPage.monthViewButton).toBeVisible();
            await expect(calendarPage.agendaViewButton).toBeVisible();

            const hasControls = await page.locator('.calendar-controls-bar').isVisible().catch(() => false);
            if (hasControls) {
                // Controls bar
                await expect(calendarPage.prevMonthButton).toBeVisible();
                await expect(calendarPage.todayButton).toBeVisible();
                await expect(calendarPage.nextMonthButton).toBeVisible();
                await expect(calendarPage.monthDisplay).toBeVisible();
                await expect(calendarPage.circleFilterSelect).toBeVisible();

                // Navigation check
                const currentMonth = await calendarPage.monthDisplay.innerText();
                await calendarPage.nextMonthButton.click();
                expect(await calendarPage.monthDisplay.innerText()).not.toEqual(currentMonth);
                await calendarPage.todayButton.click();
                expect(await calendarPage.monthDisplay.innerText()).toEqual(currentMonth);

                // Month Grid
                const hasGrid = await calendarPage.monthGrid.isVisible().catch(() => false);
                if (hasGrid) {
                    const headers = page.locator('.grid-header-cell');
                    await expect(headers).toHaveCount(7);
                }

                // Agenda View
                await calendarPage.agendaViewButton.click();
                await expect(calendarPage.agendaContainer).toBeVisible();

                // Return to Month View
                await calendarPage.monthViewButton.click();
                await expect(calendarPage.monthGrid).toBeVisible();
            } else {
                await expect(calendarPage.warningAlert).toBeVisible();
                await expect(calendarPage.warningAlert).toContainText('Calendar data is not available in this environment');
            }
        }
    });
});
