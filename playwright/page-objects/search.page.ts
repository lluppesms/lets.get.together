import { type Page, type Locator } from '@playwright/test';

export class SearchPage {
    readonly page: Page;
    readonly heading: Locator;
    readonly searchInput: Locator;
    readonly circleDropdown: Locator;
    readonly searchButton: Locator;
    readonly searchResults: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.getByRole('heading', { name: /Search Events/i });
        this.searchInput = page.getByRole('textbox', { name: 'Search Term' });
        this.circleDropdown = page.locator('.mud-select input.mud-select-input');
        this.searchButton = page.locator('#btnSearch');
        this.searchResults = page.locator('.search-results');
    }

    async goto(): Promise<void> {
        await this.page.goto('/search');
        // Wait for Blazor/SignalR to hydrate
        await this.page.waitForSelector('#btnSearch', { state: 'visible' });
    }

    async searchFor(term: string): Promise<void> {
        await this.searchInput.click();
        await this.searchInput.fill(term);
        await this.searchButton.click();
        await this.page.waitForTimeout(1000);
    }
}
