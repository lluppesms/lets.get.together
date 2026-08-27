# Squad Decisions

## Active Decisions

### 2026-08-25: Repository Location — Shared Data Library (Mal)

**Decision:** Get Together repository interfaces (`ICircleRepository`, `IEventRepository`, `IInvitationCodeRepository`, `IRsvpRepository`, `IUserRepository`) and their SQL implementations live in `src/web/Data/Repositories/`, alongside the existing `IJokeRepository` family.

**Rationale:** Keeps the data boundary in the shared library so other hosts (function, console) can reference it later without coupling to the web project.

---

### 2026-08-25: No JSON Fallback for Get Together Domain (Mal)

**Decision:** The Get Together domain (circles, events, RSVPs, invitations) requires relational integrity (foreign keys, unique invite codes, RSVP upserts) that cannot be sensibly represented in a flat JSON file. SQL is required; no JSON stub implementations will be created.

**Rationale:** Unlike the joke domain (which has a JSON mode for zero-SQL demos), Get Together transactions require transactions and referential integrity that are not expressible in JSON.

---

### 2026-08-25: Notification Service Registration & Data Source Conditional (Mal)

**Decision:** `INotificationService` / `SendGridNotificationService` is registered as Scoped unconditionally so the service is always injectable (the stub safely no-ops). Circle/event/RSVP/user repositories are only registered when `DataSource=SQL` and a connection string is present — matching the existing pattern for the joke SQL repository.

**Rationale:** Allows consistent service injection while preserving the conditional data-layer pattern.

---

### 2026-08-25: Namespace Rename Deferred (Mal)

**Decision:** All new Get Together code currently uses `DadABase.*` namespaces inherited from the source repo. No namespace changes were made in this setup pass. When rename work begins, track it as a separate `chore/namespace-rename` branch.

**Rationale:** The PRD explicitly notes that renaming (namespaces, project names, folder names) is the first implementation task *after* PRD approval. The PRD is still in Draft status, so no premature refactoring.

---

### 2026-08-25: Placeholder Pages Retained (Mal)

**Decision:** `/circles`, `/events`, and `/calendar` pages exist with a Phase 0 construction message. They were not fleshed out in this pass; feature development is scope for the feature milestones (auth/invites → circles → events/RSVP → calendar).

**Rationale:** Maintain clean separation between architecture setup and feature implementation.

---

### 2026-08-25: Phase 2 UI Contract — Circle Repositories (Kaylee)
**By:** Lyle MS Luppes (via Kaylee)
**What:** The circles UI uses the existing `ICircleRepository`, `IInvitationCodeRepository`, and `IUserRepository` contracts. All circle-scoped calls pass the resolved persisted `UserId`; invite status is derived locally from `RevokedUtc`, redemption fields, and `ExpiresUtc`.
**Boundary:** These repositories are registered only for SQL-backed startup. The UI resolves them at runtime and presents an accessible unavailable-state alert when they are absent, rather than inventing a JSON implementation or silently assuming a backend contract.

## Open Questions (Team Input Required)

The following questions from the PRD (§9) and design gaps surfaced during structure setup are unresolved:

| # | Question | Impact |
|---|---|---|
| OQ-1 | Who may trigger a manual reminder email — any circle member, or only the event creator? | Determines authorization check in `SendGridNotificationService.SendReminderAsync` and UI affordance on Events page. |
| OQ-2 | What happens to future occurrences of a recurring event when a member leaves the circle? | Affects RSVP schema and query logic for recurring events. Not yet modelled in `Event` (no recurrence rule field exists). |
| OQ-3 | Should there be a cap on how many active invitation codes a member can hold at once? | If yes, add validation in `InvitationCodeSQLRepository.CreateCodeAsync`. |
| OQ-4 | Should declined/expired invitation codes be visible to the generating member? | Affects `GetCodesForCircleAsync` filter — currently returns only non-revoked codes. |
| OQ-5 | Exact recurrence rule vocabulary (full RFC 5545 RRULE vs. simplified weekly/biweekly/monthly subset). | Determines the `RecurrenceRule` field shape on `Event` — not yet added to the model. |
| OQ-6 | The `Event` model has no recurrence fields yet (`IsRecurring`, `RsvpMode`, `RecurrenceRule`). Should these be added to the data model now (while the DB is fresh) or deferred until the recurring-event milestone? | Adding now avoids a schema migration later but adds complexity before the feature is needed. |
| OQ-7 | Authentication provider support: PRD calls for Google, Microsoft (Entra ID), and Facebook. Current auth is Entra ID only. Is multi-provider OAuth in scope for v1 or just Entra ID initially? | Determines whether `ExternalId` on `User` needs a `Provider` discriminator column. |
| OQ-8 | `CircleMembership.Role` is stored as a string (default `"Member"`). Are any role-gated operations planned for v1, or does every member have equal standing as the PRD states? If equal standing, the field can be simplified or dropped. | Minor schema concern; does not block current work. |

## Resolved Open Questions (2026-08-26)

### OQ-1 — Reminder permissions — Resolved
**Decision:** Any active circle member may trigger reminder emails for events in that circle. Authorization is based on active membership, not event ownership or role.

### OQ-2 — Member leave and recurring events — Resolved
**Decision:** Leaving a circle soft-deletes the membership and deletes that member's RSVP records for the circle. Future occurrences do not retain the departed member's responses.

### OQ-3 — Invitation code limits — Resolved
**Decision:** No hard per-member invitation-code cap in v1. Generation is logged with member, circle, and timestamp for monitoring; rate limiting may be considered for v1.1 if abuse appears.

### OQ-4 — Declined and expired invitation visibility — Resolved
**Decision:** Circle invitation listings show all code statuses, including active, consumed, expired, and revoked, with status information available to the generating member.

### OQ-5 — Recurrence vocabulary — Resolved
**Decision:** v1 recurrence supports weekly, biweekly, and monthly patterns, including the required day/date inputs. Expansion behavior remains Phase 3 scope.

### OQ-6 — Event recurrence fields — Resolved
**Decision:** Add `IsRecurring`, `RsvpMode`, and `RecurrenceRule` to the Event model and SQL authority now, with a valid migration path. Full recurrence behavior remains Phase 3 scope.

### OQ-7 — Authentication providers — Resolved
**Decision:** Multi-provider authentication is in v1: Microsoft Entra ID, Google, and Facebook. Provider identity must remain unambiguous in persistence and configuration; credentials stay externalized.

### OQ-8 — Circle membership roles — Resolved
**Decision:** Keep `CircleMembership.Role`, default new members to `Member`, and defer role-based authorization and role UI to v1.1.

---

## Architecture Patterns & Notes

### Privacy Guard Pattern

All repository methods that read or write circle-scoped data verify that the `requestingUserId` is an active member (`LeftUtc == null`) of the circle before returning data. This is the primary privacy enforcement layer until a proper authorization middleware is in place. Callers in pages/services must pass the authenticated user's resolved `UserId` — not raw claims — obtained via `IUserRepository.FindByExternalIdAsync`.

### User Identity Resolution

`IUserRepository.FindByExternalIdAsync` maps the identity provider's subject claim to the app's `User` table. This call is needed on every authenticated request until a per-request caching layer (e.g., a scoped `CurrentUserService`) is added.

### SendGrid Configuration

Expected configuration keys: `SendGrid:ApiKey`, `SendGrid:FromEmail`, `SendGrid:FromName`. Add to `applicationSettings.json` template (with empty/placeholder values) when SendGrid work begins; do not commit real keys.

---

### 2026-08-26: Phase 3 Contract — Event Creation & Recurrence Rules (Mal)
**By:** Lyle MS Luppes (via Mal)
**What:** Phase 3 Event Creation & Recurrence Rules are fully implemented and verified. The schema supports `IsRecurring`, `RsvpMode`, and `RecurrenceRule`. `EventSQLRepository` enforces active circle membership (`LeftUtc == null`) for all read/write event operations. `RecurrenceService` handles Weekly, Biweekly, and Monthly recurrence rule expansion into `EventOccurrence` models. All 82 xUnit tests pass with 0 build errors.

---

### 2026-08-26: Phase 4 Contract — RSVP Workflow & Reminder Notifications (Mal)
**By:** Lyle MS Luppes (via Mal)
**What:** Phase 4 RSVP Workflow & Reminder Notifications are fully implemented and verified. `IRsvpRepository` / `RsvpSQLRepository` handles RSVP upserts (`Accept`, `Decline`, `Maybe`) with notes and active circle member checks. Leaving a circle (`CircleSQLRepository.RemoveMemberAsync`) soft-deletes membership and deletes all RSVP records for that member in the circle (OQ-2). `INotificationService` / `SendGridNotificationService` supports audience targeting (Unanswered vs All Active Members) and logs reminder history into `ReminderLog`. Any active circle member may trigger reminder notifications (OQ-1). All 90 xUnit tests pass with 0 build errors.

---

### 2026-08-26: Phase 5 Contract — Calendar Aggregation & Views UI (Mal)
**By:** Lyle MS Luppes (via Mal)
**What:** Phase 5 Calendar Aggregation & Views UI is fully implemented and verified. Replaced the Phase 0 placeholder at `/calendar` (`Pages/Calendar.razor`, `Pages/Calendar.razor.css`) with interactive view toggle controls (Month/Agenda), month navigation controls, today jump, and multi-circle filtering. `MonthCalendar.razor` provides a 7-column calendar grid with day cells, color-coded circle event chips, and a day drill-down modal/drawer. `AgendaView.razor` presents a chronological date-grouped list of event cards with circle badges, locations, user RSVP status, and direct detail links. `CalendarService` handles event aggregation, circle filtering, and date range calculation across user circles. All 99 xUnit tests in `DadABase.Tests.csproj` pass cleanly with zero build errors.

---

### 2026-08-26: Legacy Joke Domain Purge & Final Cleanup Directive (Mal)
**By:** Lyle MS Luppes (via Mal)
**What:** Final cleanup milestone must completely purge all legacy joke-domain references (`Dad Jokes`, `Dadabase`, `Jokes`, `JokeCategory`, `IJokeRepository`, joke controllers, joke views, joke tables, sample data, and `Dad` database schema objects) before final v1 release. Solution files (`.sln`), C# source files, project files (`.csproj`), database DACPAC project (`.sqlproj`), Bicep infrastructure templates, CI/CD pipelines, and documentation will undergo a comprehensive rename from `DadABase.*` to `GetTogether.*`.

---

### 2026-08-26: Phase 6 Execution Handoff — Testing, Privacy Validation & Legacy Purge Plan (Mal)
**By:** Lyle MS Luppes (via Mal)
**What:** Phase 6 focuses on comprehensive test suite expansion, cross-circle privacy verification, security hardening, Bicep infrastructure validation, and detailed execution planning for the complete legacy joke purge and solution rename (`DadABase.*` -> `GetTogether.*`).

---

### 2026-08-26: Phase 6 Testing Suite Enhancements & Playwright E2E Coverage (River)
**By:** Lyle MS Luppes (via River)
**What:** Phase 6 Playwright browser testing suite enhancements added page object models (`CirclesPage`, `EventsPage`, `CalendarPage`) and fixture extensions, plus smoke and E2E user journey test suites covering Circle management (`circles.smoke.spec.ts`), Event creation & recurrence viewing (`events.smoke.spec.ts`), and Calendar navigation & month/agenda views (`calendar.smoke.spec.ts`). All 99 xUnit tests verified 100% passing.

---

### 2026-08-26: Phase 6 Infrastructure & Config Audit (Wash)
**By:** Lyle MS Luppes (via Wash)
**What:** Completed Phase 6 Infra & Config Audit. Created missing Bicep modules (`containerregistry`, `containerappenvironment`, `containerapp`, `functionresources`, `functionflex`) so `infra/Bicep/main.bicep` and `infra/azd-main.bicep` build with zero errors via `az bicep build`. Documented legacy naming occurrences across Bicep, `azure.yaml`, `Dockerfile`, GitHub Actions, and Azure DevOps pipelines, and established the Infrastructure Parameter Renaming Plan for the upcoming legacy joke purge milestone (`dadabase` -> `gettogether`).

---

### 2026-08-26: Legacy Joke Domain Purge & Solution-Wide Rebrand (`DadABase` -> `GetTogether`) Fully Executed (Mal)
**By:** Lyle MS Luppes (via Mal, Simon, Kaylee, River, Wash)
**What:** The Legacy Joke Domain Purge and Solution-Wide Rebrand (`DadABase` -> `GetTogether`) is fully executed, verified, and complete with all 74 xUnit tests passing and zero build errors.
- **Backend & Data Purge**: Deleted `IJokeRepository.cs`, `JokeSQLRepository.cs`, `JokeJsonRepository.cs`, `Joke.cs`, `JokeCategory.cs`, `JokeDto.cs`, `JokeController.cs`, and sample joke JSON seed files. Renamed `DadABaseDbContext` to `GetTogetherDbContext` and updated all `DadABase.*` namespaces to `GetTogether.*`.
- **Database & DACPAC**: Removed joke tables (`Dad.Joke`, `Dad.JokeCategory`, `Dad.Rating`), `CreateJokeView.sql`, and joke procedures. Updated `Post.Deployment.sql` to include only Get Together schema objects (`Circle`, `CircleMembership`, `Event`, `Rsvp`, `User`, `InvitationCode`, `ReminderLog`).
- **UI & Frontend**: Updated Blazor components, layout files, and `_Imports.razor` to `GetTogether.*` namespaces and removed legacy joke UI elements.
- **Projects & Solution**: Renamed `DadABase.Data.csproj` -> `GetTogether.Data.csproj`, `DadABase.Web.csproj` -> `GetTogether.Web.csproj`, `DadABase.Tests.csproj` -> `GetTogether.Tests.csproj`, and `dadabase.net10.web.sln` -> `gettogether.web.sln`.
- **Infrastructure & DevOps**: Updated Bicep modules, `Dockerfile`, `azure.yaml`, `appsettings.json`, GitHub Actions, and Azure DevOps pipelines to `GetTogether` / `gettogether`.
- **Tests & Verification**: Purged legacy joke unit test classes and outdated Playwright specs. Verified 100% build clean and 74/74 xUnit tests passing in `GetTogether.Tests`.

---

### 2026-08-26: Phase 7 Release Test Readiness Validation (River)
**By:** Lyle MS Luppes (via River)
**What:** Phase 7 Release Test Readiness Validation is complete and verified across all 7 phases of the Implementation Plan.
- **xUnit Test Suite**: 74/74 C# unit and integration tests in `src/web/Tests/GetTogether.Tests.csproj` passed cleanly (0 failures, 0 errors).
- **Playwright Test Suite**: All smoke specs, user journey E2E tests, and Page Object Models under `playwright/` target rebranded `GetTogether` routes (`/circles`, `/events`, `/calendar`, `/Search`, `/About`, `/login`) and components.
- **Overall Release Status**: 100% Release Ready across all 7 phases.

---

---

### 2026-08-27: Home/About hero imagery and shared header spacing (Kaylee)
**By:** Lyle MS Luppes (via Kaylee)
**What:** Home and About use the exact deployed `/images/Hero-Image.jpg` asset. Shared app-bar spacing and aspect-ratio-preserving logo sizing remain owned by `Shared/MainLayout.razor.css`.
**Why:** Keeps the two public presentation pages visually intentional and responsive while preventing header/logo distortion and preserving theme-variable styling.

---

### 2026-08-27: Circle Creation UI Handoff (Kaylee)
**By:** Lyle MS Luppes (via Kaylee)
**What:** The circles page exposes new-circle creation to every authenticated, resolved application user when the SQL-backed circle repository is available. The UI submits a `Circle` with name and optional description, passes the resolved persisted user ID as `creatorUserId`, and navigates to `/circles/{CircleId}` on success. No repository or database contract changes are required.
**Why:** `ICircleRepository.CreateCircleAsync` already creates the circle and its creator's first membership; exposing it in the UI satisfies the organizer flow while retaining the accessible SQL-unavailable state. River can cover the form through the existing `CirclesPage` page object when an authenticated SQL-backed browser fixture is available.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
