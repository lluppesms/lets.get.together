import { type Page, type Locator } from '@playwright/test';

export class CalendarPage {
    readonly page: Page;
    readonly heading: Locator;
    readonly monthViewButton: Locator;
    readonly agendaViewButton: Locator;
    readonly prevMonthButton: Locator;
    readonly todayButton: Locator;
    readonly nextMonthButton: Locator;
    readonly monthDisplay: Locator;
    readonly circleFilterSelect: Locator;
    readonly monthGrid: Locator;
    readonly dayCells: Locator;
    readonly eventChips: Locator;
    readonly modalBackdrop: Locator;
    readonly agendaContainer: Locator;
    readonly agendaGroups: Locator;
    readonly agendaCards: Locator;
    readonly warningAlert: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.locator('#calendar-heading');
        this.monthViewButton = page.locator('button', { hasText: 'Month View' });
        this.agendaViewButton = page.locator('button', { hasText: 'Agenda View' });
        this.prevMonthButton = page.locator('button[aria-label="Previous Month"]');
        this.todayButton = page.locator('button', { hasText: 'Today' });
        this.nextMonthButton = page.locator('button[aria-label="Next Month"]');
        this.monthDisplay = page.locator('.current-month-display');
        this.circleFilterSelect = page.locator('#circle-filter-select');
        this.monthGrid = page.locator('.calendar-grid');
        this.dayCells = page.locator('.day-cell');
        this.eventChips = page.locator('.event-chip');
        this.modalBackdrop = page.locator('.calendar-modal-backdrop');
        this.agendaContainer = page.locator('.agenda-view-container');
        this.agendaGroups = page.locator('.agenda-group');
        this.agendaCards = page.locator('article.agenda-card');
        this.warningAlert = page.locator('.calendar-alert-warning');
    }

    async goto(): Promise<void> {
        await this.page.goto('/calendar');
    }
}
