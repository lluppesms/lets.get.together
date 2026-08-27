import { test, expect } from '../fixtures/test-fixtures';
import { InviteSignupPage } from '../page-objects/invite-signup.page';
import { AccountSecurityPage } from '../page-objects/account-security.page';

test.describe('Invite and account security contracts', () => {
    test('captures invitation code and recipient email before requesting verification', async ({ page }) => {
        const signupPage = new InviteSignupPage(page);
        await signupPage.goto();

        await expect(signupPage.heading).toBeVisible();
        await expect(signupPage.invitationCodeInput).toBeVisible();
        await expect(signupPage.invitationEmailInput).toBeVisible();
        await signupPage.requestVerification('valid-invite-code', 'invitee@example.com');

        await expect(signupPage.statusMessage).toContainText(/unavailable|Unable to verify/i);
        await expect(page).toHaveURL(/\/invite|\/signup/);
        await expect(page).not.toHaveURL(/valid-invite-code|invitee%40example\.com/);
    });

    test('does not offer an undelivered invitation verification code', async ({ page }) => {
        const signupPage = new InviteSignupPage(page);
        await signupPage.goto();
        await page.evaluate(() => {
            sessionStorage.setItem('onboarding.invitationCode', 'valid-invite-code');
            sessionStorage.setItem('onboarding.recipientEmail', 'invitee@example.com');
        });

        await page.goto('/invite/verify');

        await expect(signupPage.emailDeliveryUnavailableMessage).toContainText('no code was delivered');
        await expect(page.getByTestId('invitation-verification-code')).toHaveCount(0);
        await expect(page).not.toHaveURL(/valid-invite-code|invitee%40example\.com/);
    });

    test('directs an anonymous visitor to sign in before showing account records', async ({ page }) => {
        const accountSecurityPage = new AccountSecurityPage(page);
        await accountSecurityPage.goto();

        await expect(accountSecurityPage.heading).toBeVisible();
        await expect(accountSecurityPage.signInHeading).toBeVisible();
        await expect(accountSecurityPage.loginLink).toHaveAttribute('href', '/login');
    });
});