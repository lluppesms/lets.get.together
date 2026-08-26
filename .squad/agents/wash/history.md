### 2026-08-26 — App Service Always-On Session Record

- The `alwaysOn` Bicep parameter, `#{ALWAYS_ON}#` bicepparam token, and environment-scoped GitHub secret convention were completed and documented.
- `az bicep build --file infra/Bicep/main.bicep` succeeded with only pre-existing warnings; no team decision was required.
# Wash — History

## Project Context

- **Project:** Get Together

## Learnings

📌 Team update (2026-08-26T00:00:00Z): The SQL schema is now `Meetings` (renamed from `Dad`) — decided by Simon. Table scripts moved to `src/database/Meetings/`, EF Core model `Schema` attributes updated to match. Affects deployment scripts and any UI/data assumptions referencing the old `Dad` schema name.
- **Owner:** Lyle MS Luppes
- **Stack:** Azure Bicep, Azure App Service, Azure SQL, GitHub Actions/Azure DevOps
- **Requirements:** `prd.md`

## Learnings

Squad initialized with Firefly casting on 2026-08-25.

## 2026-08-25

- Added empty checked-in configuration placeholders for `AzureAD`, `Authentication:Google`, and `Authentication:Facebook` in the web application template.
- Documented environment and Key Vault naming (`__` and `--`) in `Docs/Deployment_QuickRef.md` and `MAP.md`.
- Recorded the current blocker: Bicep and web startup wire Entra ID only; Google/Facebook provisioning must wait for runtime provider registration.

### 2026-08-25T20:41:56Z — Cross-agent status

The locked decision ledger requires Entra ID, Google, and Facebook in v1. Wash's inbox confirms safe empty provider placeholders exist, while Bicep and startup still wire Entra only; Google/Facebook deployment settings must wait for runtime provider registration and credentials remain externalized.

## 2026-08-26

- **Phase 6 Infra & Config Audit Completed:**
  - Audited `infra/Bicep/`, `azure.yaml`, `src/web/Website/applicationSettings.json`, `Dockerfile` (`src/web/Dockerfile`), `.github/workflows/`, and `.azdo/pipelines/` for legacy naming (`dadabase`, `dadjoke`, `Dad`).
  - Created missing Bicep sub-modules (`modules/container/containerregistry.bicep`, `containerappenvironment.bicep`, `containerapp.bicep`, `modules/functions/functionresources.bicep`, and `modules/function/functionflex.bicep`) matching parameter contracts in `main.bicep`.
  - Validated Bicep compilation via `az bicep build` for both `infra/Bicep/main.bicep` and `infra/azd-main.bicep` with 0 compilation errors.
  - Verified .NET 10 solution build (`DadABase.Web.csproj`) and xUnit test suite (`DadABase.Tests.csproj`), achieving 99/99 test passes (0 failures).
  - Formulated the Infrastructure Parameter & Renaming Migration Plan for the upcoming legacy joke purge milestone (rebranding `dadabase` -> `gettogether`).

---

### 2026-08-26 — Phase 6/7 Infrastructure & Configuration Rebrand Complete

- **Azure CLI (`azure.yaml`)**: Updated project name from `dadabase-blazor-azd` to `gettogether-blazor-azd`.
- **Bicep Templates (`infra/Bicep/`)**:
  - Updated default `sqlDatabaseName` in `infra/Bicep/main.bicep` from `'dadabase'` to `'gettogether'`.
  - Updated local deployment script `infra/Bicep/Run_Deploy_Locally.ps1` to use resource group `rg_gettogether_full-dev`, `lll-gettogether-full`, `lll-gettogether-web-demo`, and `lflgettogetherdevstorefunc`.
  - Updated example script comments in `New-AppRegistrationManagedIdentityAssertion.ps1`.
  - Confirmed `az bicep build` completes with 0 errors across `infra/Bicep/main.bicep` and `infra/azd-main.bicep`.
- **Application Settings (`src/web/Website/applicationSettings.json`)**:
  - Updated `AppDescription` to `"Get Together - An ASP.NET Core web app for simple, private event planning for real friends"`.
  - Updated `NOTSET-USE-JSON-DefaultConnection` and `DefaultConnection` placeholders to `GetTogether`.
- **Dockerfiles (`src/web/Dockerfile` & `src/web/Website/Dockerfile.generated`)**:
  - Updated project references to `Data/GetTogether.Data.csproj` and `Website/GetTogether.Web.csproj`.
  - Updated `ENTRYPOINT` to `GetTogether.Web.dll` and image tag examples to `gettogether-web:latest`.
- **CI/CD Pipelines (`.github/workflows/` and `.azdo/pipelines/`)**:
  - Updated `.github/config/projects.yml` to map `web` project name to `GetTogether.Web` and test project to `GetTogether.Tests`.
  - Updated `.github/workflows/5-run-sql-script.yml` database copy default to `GetTogetherV02` and resource group to `rg-gettogether-dev`.
  - Updated `.azdo/pipelines/vars/var-common.yml` to `rg-gettogether-azdo`, `Get Together`, `gettogether.web.sln`, `GetTogether.Web`, and `GetTogether.Tests`.
  - Updated `.azdo/pipelines/vars/var-source-location-app.yml` to `src/web`, `gettogether.web`, `Website`, `GetTogether.Web`, `Tests`, and `GetTogether.Tests`.
  - Updated variable group references across 12 AzDO pipeline YAML files from `Dadabase.Demo` to `GetTogether.Demo` and updated container image name to `gettogether-web`.
  - Updated SBOM generator package parameters to `GetTogether` and repository link to `https://github.com/lluppesms/lets.get.together`.
- **Verification**: `az bicep build` succeeds (0 errors), `dotnet build src/web/gettogether.web.sln` succeeds (0 errors), and `dotnet test src/web/Tests/GetTogether.Tests.csproj` passes all 74 xUnit tests.

### 2026-08-26 — App Service Always-On Configuration

- Externalized the App Service `siteConfig.alwaysOn` setting through the `alwaysOn` boolean parameters in `infra/Bicep/main.bicep` and `modules/webapp/website.bicep`.
- Added the `#{ALWAYS_ON}#` token to `main.bicepparam`; GitHub Actions token replacement receives the environment-scoped `ALWAYS_ON` secret without a workflow contract change.
- Documented the secret setup in `.github/CreateGitHubSecrets.md` and validated `az bicep build --file infra/Bicep/main.bicep` with no errors.
