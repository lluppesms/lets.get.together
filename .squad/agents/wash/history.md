# Wash — History

## Project Context

- **Project:** Get Together
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
