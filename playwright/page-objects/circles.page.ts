import { type Page, type Locator } from '@playwright/test';

export class CirclesPage {
    readonly page: Page;
    readonly heading: Locator;
    readonly intro: Locator;
    readonly switcherAside: Locator;
    readonly circleChoices: Locator;
    readonly circleDetailHeading: Locator;
    readonly leaveButton: Locator;
    readonly leaveConfirmationModal: Locator;
    readonly confirmLeaveButton: Locator;
    readonly keepCircleButton: Locator;
    readonly membersPanel: Locator;
    readonly settingsPanel: Locator;
    readonly invitesPanel: Locator;
    readonly nameInput: Locator;
    readonly descriptionInput: Locator;
    readonly saveSettingsButton: Locator;
    readonly inviteExpirationInput: Locator;
    readonly generateInviteButton: Locator;
    readonly generatedCodeInput: Locator;
    readonly inviteTable: Locator;
    readonly warningAlert: Locator;
    readonly emptyState: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.locator('#circles-heading');
        this.intro = page.locator('.circles-intro');
        this.switcherAside = page.locator('.circle-switcher');
        this.circleChoices = page.locator('a.circle-choice');
        this.circleDetailHeading = page.locator('#circle-detail-heading');
        this.leaveButton = page.locator('button', { hasText: 'Leave circle' });
        this.leaveConfirmationModal = page.locator('.circle-confirmation');
        this.confirmLeaveButton = page.locator('button', { hasText: 'Yes, leave' });
        this.keepCircleButton = page.locator('button', { hasText: 'Keep circle' });
        this.membersPanel = page.locator('.circle-panel', { has: page.locator('#members-heading') });
        this.settingsPanel = page.locator('.circle-panel', { has: page.locator('#settings-heading') });
        this.invitesPanel = page.locator('.circle-panel', { has: page.locator('#invites-heading') });
        this.nameInput = page.locator('#circle-name');
        this.descriptionInput = page.locator('#circle-description');
        this.saveSettingsButton = page.locator('button[type="submit"]', { hasText: 'Save changes' });
        this.inviteExpirationInput = page.locator('#invite-expiration');
        this.generateInviteButton = page.locator('button', { hasText: 'Generate code' });
        this.generatedCodeInput = page.locator('#generated-code');
        this.inviteTable = page.locator('table.invite-table');
        this.warningAlert = page.locator('.circle-alert-warning');
        this.emptyState = page.locator('.circle-empty');
    }

    async goto(): Promise<void> {
        await this.page.goto('/circles');
    }
}
