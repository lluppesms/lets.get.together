import { type Page, type Locator } from '@playwright/test';

export class HomePage {
    readonly page: Page;
    readonly heading: Locator;
    readonly leadText: Locator;
    readonly heroCard: Locator;
    readonly manageCirclesButton: Locator;
    readonly viewEventsButton: Locator;
    readonly openCalendarButton: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.getByRole('heading', { name: /Get Together/i, level: 1 });
        this.leadText = page.locator('.lead');
        this.heroCard = page.locator('.hero-card');
        this.manageCirclesButton = page.getByRole('link', { name: 'Manage Circles' });
        this.viewEventsButton = page.getByRole('link', { name: 'View Events' });
        this.openCalendarButton = page.getByRole('link', { name: 'Open Calendar' });
    }

    async goto(): Promise<void> {
        await this.page.goto('/');
    }
}
