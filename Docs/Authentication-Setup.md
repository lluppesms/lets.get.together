# Authentication Setup

This guide configures the external sign-in providers used by Get Together:

- Google OAuth 2.0
- Microsoft Entra ID (formerly Azure AD)

The application signs users into its own cookie after a provider completes authentication. It preserves the provider, issuer, and subject as the account identity. Do not use an email address as the identity key.

## Before You Begin

You need:

- The deployed HTTPS host name, if configuring a deployed environment.
- Permission to create OAuth credentials in Google Cloud.
- Permission to register applications in the Microsoft Entra tenant.
- A secret store for client secrets. Never commit client secrets to this repository.

The application only enables a provider when its required configuration is present. Empty placeholders do not enable sign-in.

## Google Authentication

### 1. Create or select a Google Cloud project

1. Open the [Google Cloud Console](https://console.cloud.google.com/).
2. Create a project or select the project that will own Get Together.
3. Open **APIs & Services > OAuth consent screen**.
4. Select **External** for users outside your organization, or **Internal** when every user belongs to your Google Workspace organization.
5. Enter the application name, support email, and developer contact email.
6. Add the scopes required by the application. Get Together uses the standard identity information needed by the Google authentication handler; do not add unrelated sensitive scopes.
7. If the consent screen is in testing, add each test Google account under **Test users**.
8. Complete the consent-screen configuration. Google may require verification before an external application can be used broadly.

### 2. Create the OAuth client

1. Open **APIs & Services > Credentials**.
2. Select **Create credentials > OAuth client ID**.
3. Choose **Web application**.
4. Give the client a recognizable name, such as `Get Together Web - dev`.
5. Under **Authorized JavaScript origins**, add the site origin when required by the Google console, for example:

   ```text
   https://get-together.example.com
   ```

6. Under **Authorized redirect URIs**, add the exact callback URL:

   ```text
   https://get-together.example.com/signin-google
   ```

7. For local development, add the exact HTTPS callback URL used by the local app, including its port:

   ```text
   https://localhost:5001/signin-google
   ```

   Use the port printed by `dotnet run` or `dotnet watch` if it differs from `5001`.

8. Select **Create** and copy the client ID. Treat the client secret as a password; store it only in a secret store.

Google redirect URIs must match exactly, including scheme, host, port, path, and trailing slash behavior. Do not use a wildcard or a callback URL containing query-string state.

### 3. Configure Get Together

The application expects these keys:

```text
Authentication:Google:ClientId
Authentication:Google:ClientSecret
Authentication:Google:CallbackPath=/signin-google
```

For local development, use .NET User Secrets from the repository root or web project directory:

```powershell
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
```

Environment variables use double underscores:

```powershell
$env:Authentication__Google__ClientId = "your-client-id"
$env:Authentication__Google__ClientSecret = "your-client-secret"
```

The callback path is already `/signin-google` in `applicationSettings.json`; change it only if the Google Cloud redirect URI and application configuration are changed together.

For Azure App Service or Container Apps, configure the values through the platform's application settings or the deployment secret mechanism:

```text
Authentication__Google__ClientId=<Google client ID>
Authentication__Google__ClientSecret=<Google client secret>
```

The repository's deployment flow uses the environment-scoped `GOOGLE_CLIENT_ID` variable and `GOOGLE_CLIENT_SECRET` secret. Keep the secret out of Bicep parameter files, workflow output, and source control.

### 4. Test Google sign-in

1. Start the application with both Google settings present.
2. Open `/login`.
3. Select **Continue with Google**.
4. Complete Google consent and sign-in.
5. Confirm the browser returns to the application and the authenticated user can open `/circles`.

A new user still needs to complete the application's invitation-based onboarding at `/invite` before a user record and membership are created. Google authentication identifies the person; it does not bypass invitation, email verification, or circle-membership rules.

If Google is not shown on `/login`, verify that both the client ID and client secret are non-empty in the running environment. If Google reports a redirect URI mismatch, compare the URL in the error with the exact URI registered in Google Cloud.

## Microsoft Entra ID

### 1. Register the application

1. Open the [Microsoft Entra admin center](https://entra.microsoft.com/).
2. Go to **Applications > App registrations > New registration**.
3. Enter a name, such as `Get Together Web - dev`.
4. Select the supported account type that matches the intended audience:
   - **Accounts in this organizational directory only** for one tenant.
   - **Accounts in any organizational directory** for multi-tenant organizational access.
   - Include personal Microsoft accounts only when that audience is required.
5. Leave the redirect URI blank during creation, or choose **Web** and enter the deployed callback URI:

   ```text
   https://get-together.example.com/signin-oidc
   ```

6. Select **Register**.
7. Copy the **Application (client) ID** and **Directory (tenant) ID** from the Overview page.

For local development, add a separate Web redirect URI matching the local HTTPS URL:

```text
https://localhost:5001/signin-oidc
```

Use the actual local port when it differs. Keep development and production redirect URIs distinct.

### 2. Create a client secret

1. Open the registration's **Certificates & secrets** page.
2. Select **New client secret**.
3. Add a description and choose the shortest lifetime that fits the operational requirements.
4. Select **Add**.
5. Copy the secret **Value** immediately. It is not the secret ID, and the value is shown only once.

Store the secret in User Secrets, Azure Key Vault, or the deployment platform's secret settings. Rotate it before it expires.

### 3. Configure API permissions and branding

1. Open **API permissions** and confirm the Microsoft Graph delegated `User.Read` permission required for basic sign-in identity data.
2. Grant admin consent only when required by the tenant's consent policy.
3. Review **Branding & properties** and provide the home page, privacy statement, and terms of service URLs when the tenant or consent experience requires them.
4. Under **Authentication**, confirm the platform is **Web** and that every allowed environment has its own exact `/signin-oidc` redirect URI.

Do not add permissions that the application does not use. The application resolves identity from the validated issuer and subject claims, not from a mutable email claim.

### 4. Configure Get Together

The application expects these keys:

```text
AzureAD:TenantId
AzureAD:Instance=https://login.microsoftonline.com/
AzureAD:Domain
AzureAD:ClientId
AzureAD:ClientSecret
AzureAD:CallbackPath=/signin-oidc
```

For local development:

```powershell
dotnet user-secrets set "AzureAD:TenantId" "your-tenant-id"
dotnet user-secrets set "AzureAD:ClientId" "your-application-client-id"
dotnet user-secrets set "AzureAD:ClientSecret" "your-client-secret"
dotnet user-secrets set "AzureAD:Domain" "yourtenant.onmicrosoft.com"
```

Environment variables use double underscores:

```powershell
$env:AzureAD__TenantId = "your-tenant-id"
$env:AzureAD__ClientId = "your-application-client-id"
$env:AzureAD__ClientSecret = "your-client-secret"
$env:AzureAD__Domain = "yourtenant.onmicrosoft.com"
```

For Azure, configure the equivalent application settings through the deployment secret mechanism. Do not place `AzureAD:ClientSecret` in checked-in JSON, Bicep parameter files, or workflow logs.

The callback path is already `/signin-oidc` in `applicationSettings.json`. The effective redirect URI is therefore:

```text
https://<host>/signin-oidc
```

### 5. Test Microsoft sign-in

1. Start the application with `AzureAD:TenantId` and `AzureAD:ClientId` configured, plus the client secret when required by the deployment.
2. Open `/login`.
3. Select **Continue with Microsoft**.
4. Complete sign-in and consent.
5. Confirm the browser returns to the application and the authenticated user can open `/circles`.

A new user still follows the invitation and verification onboarding flow. Entra sign-in does not automatically create an application user from an email address.

If Microsoft is not shown on `/login`, verify that `AzureAD:TenantId` is non-empty in the running environment. For `AADSTS50011`, compare the redirect URI in the error with the exact `/signin-oidc` URI registered on the app registration.

## Provider and Environment Checklist

For each environment, verify:

- The site is using HTTPS.
- The provider callback URI exactly matches the deployed host and configured callback path.
- The provider is enabled only when all required settings are present.
- Client secrets are stored outside source control and are available to the running process.
- OAuth consent and test-user restrictions allow the account being tested.
- `/login` shows the intended provider button.
- A successful callback returns to the application instead of displaying a provider error.
- A first-time user is directed through `/invite` and email verification before account creation.
- An existing user can resolve the same provider-qualified identity on `/circles`.

## Related Configuration

- `src/web/Website/Program.cs` configures the Google and Microsoft authentication handlers.
- `src/web/Website/applicationSettings.json` contains non-secret callback-path defaults and empty credential placeholders.
- `Docs/Deployment_QuickRef.md` describes deployment environment variables and Azure secret handling.
