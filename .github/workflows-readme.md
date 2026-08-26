---
title: Set up GitHub Actions
description: Repository workflow setup notes for deploying Get Together resources and database updates
author: Get Together maintainers
ms.date: 2026-05-14
ms.topic: how-to
keywords:
- github actions
- azure
- get-together
estimated_reading_time: 6
---

## Set up GitHub Actions

The deployment workflows use GitHub environments. Define shared values at the repository level only when they are safe to reuse; prefer environment-level variables and secrets for environment-specific values.

---

## Workflow Definitions

- **[1-deploy-bicep.yml](./workflows/1-deploy-bicep.yml):** Deploys or previews `infra/Bicep/main.bicep`; supports `webapp`, `containerapp`, `functionapp`, or `all`.
- **[2-bicep-build-deploy-webapp.yml](./workflows/2-bicep-build-deploy-webapp.yml):** Optionally deploys Bicep, builds and deploys the Web App, and optionally scans and smoke-tests it.
- **[4-build-deploy-dacpac.yml](./workflows/4-build-deploy-dacpac.yml):** Builds and optionally deploys the SQL DACPAC and default data.
- **[5-run-sql-script.yml](./workflows/5-run-sql-script.yml):** Runs a selected SQL patch, default-data script, or database copy operation.
- **[6-pr-scan-build.yml](./workflows/6-pr-scan-build.yml):** Scans and builds the Web App for pull requests or manual runs; it does not deploy.
- **[7-scan-code.yml](./workflows/7-scan-code.yml):** Runs the scheduled or manual MS DevSecOps, CodeQL, and SBOM scans.
- **[8-smoke-test-webapp.yml](./workflows/8-smoke-test-webapp.yml):** Runs the Playwright UI smoke suite against the selected environment.
- **[azure-dev.yml](./workflows/azure-dev.yml):** Runs `azd provision` and `azd deploy` using the repository's `azure.yaml` App Service definition and client-secret login.

Files named `template-*.yml` are reusable workflows called by these entry workflows. The `squad-*.yml` and `sync-squad-labels.yml` files support repository automation and are not Azure application deployment workflows.

---

## Sequence of Workflows

For a new SQL-backed environment, deploy infrastructure first, grant the CI/CD principal permission to publish the DACPAC and the application identity permission to use the `Meetings` schema, then run the DACPAC workflow. The SQL script workflow can load the default data or apply a patch afterward.

---

## Azure Credentials

Before deployment, set these environment-scoped secrets (repository-scoped values also work when shared across environments):

See the **[CreateGitHubSecrets.md](./CreateGitHubSecrets.md)** file for info on how to create the a service principal and set up the Federated Credentials.

Once the credentials are set up, you can customize and run the following commands, or you can set these secrets up manually by going to the Settings -> Secrets -> Actions -> Secrets.

You can set these up at the Repository Level...

```bash
gh secret set --env <ENV-NAME> AZURE_CLIENT_ID -b <GUID-application-client-id>
gh secret set --env <ENV-NAME> AZURE_TENANT_ID -b <GUID-Entra-tenant>
gh secret set --env <ENV-NAME> AZURE_SUBSCRIPTION_ID -b <subscription-id>
```

but it's probably better to set up one set of credentials for each Environment:

```bash
gh secret set --env <ENV-NAME> AZURE_CLIENT_ID -b <GUID-application-client-id>
gh secret set --env <ENV-NAME> AZURE_TENANT_ID -b <GUID-Entra-tenant>
gh secret set --env <ENV-NAME> AZURE_SUBSCRIPTION_ID -b <subscription-id>
```

The `azure-dev.yml` workflow is separate from the OIDC workflows and currently expects `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`, `AZURE_SUBSCRIPTION_ID`, `AZURE_TENANT_ID`, `AZURE_ENV_NAME`, and `RESOURCE_GROUP_LOCATION` as secrets. Use it only when that client-secret login is intentionally configured.

```bash
gh secret set ADMIN_IP_ADDRESS 192.168.1.1
gh secret set ADMIN_PRINCIPAL_ID <yourGuid>
```

---

## Database Security

In order for the workflows to be able to update the database, you must grant rights to the CICD service principal and the application managed identity in the database.

Grant the CI/CD service principal rights to publish the DACPAC schema:

```sql
CREATE USER [yourServicePrincipalName] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [yourServicePrincipalName];
```

Grant the application identity rights to read and write data (not change the schema):

```sql
CREATE USER [yourAppManagedIdentityName] FROM EXTERNAL PROVIDER;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Meetings] TO [yourAppManagedIdentityName];
GRANT EXECUTE ON SCHEMA::[Meetings] TO [yourAppManagedIdentityName];
```

Once these rights are in place, before the application can run successfully, then you can deploy the SQL database schema and data using the [DACPAC deployment workflow](./workflows/4-build-deploy-dacpac.yml) and the [SQL script workflow](./workflows/5-run-sql-script.yml). In the SQL Script workflow, choose the [InsertDefaultData.sql](../src/database/Patch/InsertDefaultData.sql) script to populate the database with some starter data.

---

## Bicep Configuration Values

There are other values used by the Bicep templates to configure the resource names that are deployed. Make sure the App_Name variable is unique to your deployment. It will be used as the basis for the application name and for all the other Azure resources, some of which must be globally unique.

See **[CreateGitHubSecrets.md](./CreateGitHubSecrets.md)** for the current variable and secret inventory, including optional existing-resource and AI settings.

---

## References

- [Deploying ARM Templates with GitHub Actions](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-github-actions)
- [Manage Federated Identity Credential in Entra Id](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-create-trust?pivots=identity-wif-apps-methods-azp) (MS Learn)
- [Immutable subject claims for GitHub Actions OIDC tokens](https://github.blog/changelog/2026-04-23-immutable-subject-claims-for-github-actions-oidc-tokens/) (GitHub Changelog Announcement - April 2026)
- [Migrate GitHub Actions federated credentials to immutable subjects](https://learn.microsoft.com/en-us/entra/workload-id/workload-identities-github-immutable-subjects) (MS Learn)
- [GitHub Secrets CLI](https://cli.github.com/manual/gh_secret_set)
- [GitHub Variables CLI](https://cli.github.com/manual/gh_variable_set)

---

[Return to Home Page](../README.md)
