import { test, expect } from '../fixtures/test-fixtures';

test.describe('API Endpoint Smoke Tests', () => {

    test('GET /api/config returns 200', async ({ request }) => {
        const response = await request.get('/api/config');
        expect(response.status()).toBe(200);
        expect(response.ok()).toBeTruthy();
    });
});
