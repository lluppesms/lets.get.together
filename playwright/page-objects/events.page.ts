import { type Page, type Locator } from '@playwright/test';

export class EventsPage {
    readonly page: Page;
    readonly heading: Locator;
    readonly createEventButton: Locator;
    readonly circleFilterSelect: Locator;
    readonly eventsCount: Locator;
    readonly eventsGrid: Locator;
    readonly eventCards: Locator;
    readonly emptyState: Locator;
    readonly createFormContainer: Locator;
    readonly eventTitleInput: Locator;
    readonly eventCircleSelect: Locator;
    readonly eventLocationInput: Locator;
    readonly eventDescriptionInput: Locator;
    readonly startDateInput: Locator;
    readonly startTimeInput: Locator;
    readonly recurringCheckbox: Locator;
    readonly recurrenceRuleSelect: Locator;
    readonly rsvpModeSelect: Locator;
    readonly submitEventButton: Locator;
    readonly cancelFormButton: Locator;
    readonly warningAlert: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.locator('#events-heading');
        this.createEventButton = page.locator('button', { hasText: /\+ Create Event|Close Form/i });
        this.circleFilterSelect = page.locator('#circle-filter');
        this.eventsCount = page.locator('.events-count');
        this.eventsGrid = page.locator('.events-grid');
        this.eventCards = page.locator('article.event-card');
        this.emptyState = page.locator('.events-empty');
        this.createFormContainer = page.locator('.event-create-container');
        this.eventTitleInput = page.locator('#event-title');
        this.eventCircleSelect = page.locator('#event-circle');
        this.eventLocationInput = page.locator('#event-location');
        this.eventDescriptionInput = page.locator('#event-description');
        this.startDateInput = page.locator('#event-start-date');
        this.startTimeInput = page.locator('#event-start-time');
        this.recurringCheckbox = page.locator('#event-recurring');
        this.recurrenceRuleSelect = page.locator('#event-recurrence-rule');
        this.rsvpModeSelect = page.locator('#event-rsvp-mode');
        this.submitEventButton = page.locator('button[type="submit"]', { hasText: /Create Event/i });
        this.cancelFormButton = page.locator('button', { hasText: 'Cancel' });
        this.warningAlert = page.locator('.events-alert-warning');
    }

    async goto(): Promise<void> {
        await this.page.goto('/events');
    }
}
