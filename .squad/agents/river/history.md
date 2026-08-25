# River — History

## Project Context

- **Project:** Get Together
- **Owner:** Lyle MS Luppes
- **Stack:** xUnit for .NET services/repositories and Playwright for browser smoke/E2E coverage
- **Requirements:** `prd.md`

## Learnings

Squad initialized with Firefly casting on 2026-08-25.

- 2026-08-25: Added focused EF Core InMemory coverage for invitation redemption/rejection, circle privacy denial, and existing external-identity resolution in `GetTogetherAccess_Tests.cs`. Provider-aware identity mapping and full onboarding cannot be covered yet because the current model/repository surface has no provider discriminator or onboarding route/service. No invite/signup Playwright surface or local SQL test fixture exists.
- 2026-08-25: Added Phase 2 EF Core InMemory coverage for multi-circle listing, active-member roster filtering, cross-circle read/write denial, unlimited active-member invite generation, all invitation statuses, revocation, and soft-leave access removal. RSVP deletion on leave remains unverified because the repository currently only soft-deletes membership; no authenticated circle Playwright surface or local SQL fixture exists.

### 2026-08-25T20:41:56Z — Cross-agent status

The locked Phase 1 handoff calls for signup browser coverage, but River's inbox reports no invite, signup, or authenticated-circle Playwright surface and no local SQL fixture. Existing focused repository tests cover current invitation, privacy, and external-identity behavior; provider-aware identity and full onboarding remain untestable at the present API surface.
