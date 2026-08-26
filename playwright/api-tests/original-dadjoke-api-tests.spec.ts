const { test, expect, request } = require('@playwright/test');

test.describe('Get Together API Tests', () => {
  test('should get config endpoint', async ( { baseURL } ) => {
      console.log('Test: Call Get Together API');
      console.log('Using Base URL: ' + baseURL);
      const apiContext = await request.newContext({ 
        baseURL: baseURL,
        ignoreHTTPSErrors: true 
      });
      const response = await apiContext.get("/api/config");  
      expect(response.ok()).toBeTruthy();
      expect(response.status()).toBe(200);
  });
});
