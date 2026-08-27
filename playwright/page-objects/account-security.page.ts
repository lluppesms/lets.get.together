import { type Locator, type Page } from '@playwright/test';

export class AccountSecurityPage {
    readonly page: Page;
    readonly heading: Locator;
    readonly linkedIdentitiesHeading: Locator;
    readonly emailAliasesHeading: Locator;
    readonly signInHeading: Locator;
    readonly loginLink: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.getByRole('heading', { name: /Security and notifications/i });
        this.linkedIdentitiesHeading = page.getByRole('heading', { name: /Linked identities/i });
        this.emailAliasesHeading = page.getByRole('heading', { name: /Email aliases/i });
        this.signInHeading = page.getByRole('heading', { name: /Sign in to manage your account/i });
        this.loginLink = page.getByRole('link', { name: 'Log in' });
    }

    async goto(): Promise<void> {
        await this.page.goto('/account/security');
    }
}