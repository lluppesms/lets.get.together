import { test, expect } from '../fixtures/test-fixtures';

test.describe('Login Page Smoke Tests', () => {
    test('Google sign-in entry is disabled when unavailable or challenges through its dedicated endpoint', async ({ page }) => {
        await page.goto('/login');

        const googleSignInLink = page.getByRole('link', { name: 'Continue with Google' });
        const googleSignInButton = page.getByRole('button', { name: /Continue with Google/ });

        if (await googleSignInLink.count()) {
            await expect(googleSignInLink).toHaveAttribute('href', '/login/google');
            return;
        }

        await expect(googleSignInButton).toBeDisabled();
        await expect(googleSignInButton).toHaveText(/Not configured/);
    });
});