import { type Locator, type Page } from '@playwright/test';

export class InviteSignupPage {
    readonly page: Page;
    readonly heading: Locator;
    readonly invitationCodeInput: Locator;
    readonly invitationEmailInput: Locator;
    readonly continueButton: Locator;
    readonly statusMessage: Locator;
    readonly emailDeliveryUnavailableMessage: Locator;
    readonly loginLink: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.getByRole('heading', { name: 'Verify your invitation.', level: 1 });
        this.invitationCodeInput = page.getByLabel('Invitation code');
        this.invitationEmailInput = page.getByLabel('Invitation email');
        this.continueButton = page.getByRole('button', { name: 'Continue to verification' });
        this.statusMessage = page.getByRole('alert');
        this.emailDeliveryUnavailableMessage = page.getByTestId('invitation-email-delivery-unavailable');
        this.loginLink = page.getByRole('link', { name: 'Log in' });
    }

    async goto(): Promise<void> {
        await this.page.goto('/signup');
    }

    async requestVerification(invitationCode: string, invitationEmail: string): Promise<void> {
        await this.invitationCodeInput.fill(invitationCode);
        await this.invitationEmailInput.fill(invitationEmail);
        await this.continueButton.click();
    }
}