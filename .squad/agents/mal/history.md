# Mal — History

## Project Context

- **Project:** Get Together
- **Owner:** Lyle MS Luppes
- **Stack:** .NET 10 Blazor Server, EF Core/Azure SQL, Bicep, Azure App Service, SendGrid, xUnit, Playwright
- **Requirements:** `prd.md`

## Learnings

Squad initialized with Firefly casting on 2026-08-25.

### 2026-08-25 — Project Structure Setup

**Inherited state**: Domain models (Circle, Event, RSVP, User, InvitationCode, CircleMembership, ReminderLog) already existed in `src/web/Data/Models/` and were registered in `DadABaseDbContext`. Placeholder Blazor pages for `/circles`, `/events`, `/calendar` existed. NavMenu included those links. **No repository interfaces or implementations existed** for the Get Together domain.

**Work done**:
1. Created repository interfaces in `src/web/Data/Repositories/`:
   - `ICircleRepository`, `IEventRepository`, `IInvitationCodeRepository`, `IRsvpRepository`, `IUserRepository`
2. Created SQL implementations alongside each interface:
   - `CircleSQLRepository`, `EventSQLRepository`, `InvitationCodeSQLRepository`, `RsvpSQLRepository`, `UserSQLRepository`
   - All enforce circle-member access guard (`LeftUtc == null`) before returning or mutating data.
3. Created notification service:
   - `src/web/Website/Services/Interfaces/INotificationService.cs`
   - `src/web/Website/Services/SendGridNotificationService.cs` (stub; logs only)
4. Registered all new services in `Program.cs` (repos under SQL mode guard; notification service unconditionally).
5. Updated `src/web/Website/globalUsings.cs` to add `DadABase.Web.Services.Interfaces`.
6. Updated `MAP.md` routing guide and data layer section.
7. Wrote open questions and architecture decisions to `.squad/decisions/inbox/mal-project-structure.md`.

**Build**: 0 errors, 18 pre-existing warnings in JokeSQLRepository (not new).
**Tests**: All 38 existing xUnit tests pass after changes.

**Key paths**:
- Domain repos: `src/web/Data/Repositories/ICircleRepository.cs` et al.
- Notification: `src/web/Website/Services/Interfaces/INotificationService.cs`
- DI registration: `src/web/Website/Program.cs` (search "Get Together domain repositories")
- Open questions: `.squad/decisions/inbox/mal-project-structure.md`

**Namespace**: Still `DadABase.*` — rename is a separate deferred task (PRD still Draft).

---

### 2026-08-25T14-37-59 — Decision Merge (Scribe)

📌 **Team update (2026-08-25T14:37:59):** All 5 architectural decisions from project structure setup (repository location, JSON fallback policy, notification service registration, namespace deferral, placeholder pages) have been merged into canonical `.squad/decisions.md`. Session logs created at `.squad/log/2026-08-25T14-37-59-structure-setup.md` and `.squad/orchestration-log/2026-08-25T14-37-59-mal.md`. 8 open questions documented for team resolution (OQ-1 through OQ-8).

### 2026-08-26 — Phase 1 Execution Handoff

Converted the eight locked team decisions into a file-level Phase 1 handoff on `task/initial-version`. The ownership boundary is Simon for auth and identity data, Wash for provider configuration, Kaylee for onboarding UI, River for Playwright signup coverage, with recurrence fields tracked as a schema prerequisite and full recurrence behavior deferred to Phase 3.

### 2026-08-25T20:41:56Z — Cross-agent status

Scribe recorded the Phase 1 handoff outcome. Kaylee reported backend contract gaps, River reported missing signup/browser surfaces and provider-aware identity seams, and Wash reported that provider deployment wiring is blocked until runtime auth registration exists. Simon's requested blocker inbox record was absent.

### 2026-08-25 — Phase 2 Execution Handoff

Locked the Phase 2 circles and membership boundary to the existing repository contracts and DadABase namespaces. Any active member may generate uncapped invitation codes, status visibility is based on existing invitation fields, and `CircleMembership.Role` remains a stored no-op; the focused xUnit repository tests remain the privacy backstop where an authenticated SQL-backed Playwright fixture is unavailable.

📌 Team update (2026-08-25T21-08-16Z): Kaylee's Phase 2 circles UI consumes the existing circle, invitation, and user repository contracts, passes persisted user IDs for circle-scoped calls, and exposes an accessible unavailable state when SQL-only registrations are absent.

---

### 2026-08-26 — Phase 3 Execution Handoff

Coordinated Phase 3: Event Creation & Recurrence Rules. Key locked decisions: recurrence fields (`IsRecurring`, `RsvpMode`, `RecurrenceRule`) already exist on the `Event` entity and SQL schema authority (`Dad.Event`). Recurrence rule vocabulary includes Weekly, Biweekly, Monthly; all events are circle-scoped (`CircleId`); any active circle member (`LeftUtc == null`) can view, create, edit, or cancel events in their circle. Produced `.squad/decisions/inbox/mal-phase3-execution-handoff.md` detailing scope, files to touch, ownership boundaries (Simon for backend/helpers, Kaylee for Blazor UI/events page, River for xUnit & Playwright tests, Wash for CI/CD deploy verification), acceptance checks, and risk mitigations. No feature code implemented.

---

### 2026-08-26 — Phase 5 Execution Handoff

Coordinated Phase 5: Calendar Aggregation & Views UI. Key locked decisions embodied: Calendar Aggregation across active user circles (`LeftUtc == null`), interactive Month & Agenda views, month navigation controls, multi-circle filtering, responsive day detail drawer/modal, and chronological date-grouped event listing. All 99 xUnit tests passing with 0 build errors.

📌 Team update (2026-08-26T23-30-00Z): Phase 5 Calendar Aggregation & Views UI fully completed and verified. 99 xUnit tests passing with zero build errors.

---

### 2026-08-26 — Phase 6 Execution Handoff & Legacy Joke Cleanup Directive

Captured critical user directive for legacy joke domain purge (`"when this is complete there should be no references to 'Dad Jokes', 'Dadabase', 'Jokes', 'JokeCategory', etc. that should be all gone"`). Strategy set: phased feature delivery (Phases 1-5 done) followed by Phase 6 testing/privacy hardening, and final legacy joke codebase purge and namespace/project rename (`DadABase` -> `GetTogether`) as final release gate.

Coordinated Phase 6 execution directives:
- **River (Tester)**: Expand Playwright E2E suites and cross-circle privacy verification.
- **Simon (Backend)**: Plan complete removal of `JokeSQLRepository`, `JokeJsonRepository`, `IJokeRepository`, `Joke`, `JokeCategory`, and `Dad` schema joke objects, plus `DadABase.*` -> `GetTogether.*` namespace rename strategy.
- **Kaylee (Frontend)**: Audit Blazor components and layouts for leftover joke references or UI text.
- **Wash (DevOps)**: Audit Bicep templates, app settings, Dockerfiles, and CI/CD pipelines for legacy naming.

Wrote decision inbox entries `.squad/decisions/inbox/mal-legacy-joke-cleanup-directive.md` and `.squad/decisions/inbox/mal-phase6-execution-handoff.md`.

---

### 2026-08-26 — Phase 6 Testing, Privacy Validation & Release Hardening Complete

📌 Team update (2026-08-26T23-59-00Z): Phase 6 is fully implemented, verified, and complete. All 99 xUnit tests passing with 0 build errors. Playwright E2E browser testing suite expanded with Page Object Models (`CirclesPage`, `EventsPage`, `CalendarPage`) and smoke/E2E test specs. Cross-circle privacy boundaries verified. Bicep infrastructure templates validated with `az bicep build` clean. Legacy joke domain purge and solution-wide rebrand (`DadABase.*` -> `GetTogether.*`) plan approved for execution in final release gate. Decision inbox merged and cleared.

---

### 2026-08-26 — Final Release Gate: Legacy Joke Domain Purge & Solution-Wide Rebrand Handoff

Coordinated the execution handoff for the Final Release Gate: Legacy Joke Domain Purge and Solution-Wide Rebrand (`DadABase` -> `GetTogether`).

Key decisions and team assignments recorded in `.squad/decisions/inbox/mal-legacy-purge-rebrand-handoff.md`:
- **Scope**: Total purge of legacy joke models (`Joke.cs`, `JokeCategory.cs`), repositories (`IJokeRepository`, `JokeSQLRepository`, `JokeJsonRepository`), controllers (`JokeController`), SQL views/tables (`CreateJokeView.sql`), seed files, and legacy joke test suites/specs.
- **Project & Namespace Renames**: `DadABase.Data` -> `GetTogether.Data`, `DadABase.Web` -> `GetTogether.Web`, `DadABase.Tests` -> `GetTogether.Tests`, `DadABaseDbContext` -> `GetTogetherDbContext`, and `dadabase.net10.web.sln` -> `gettogether.web.sln`.
- **Assignments**:
  - **Simon**: Backend joke model/repository/controller removal, SQL database project cleanup, `DadABaseDbContext` -> `GetTogetherDbContext` rename, `.csproj`/`.sln` renames, and backend namespace updates.
  - **Kaylee**: Blazor components, pages, `_Imports.razor`, layouts, and UI `using` namespace updates.
  - **River**: Unit test cleanup/purge, `GetTogether.Tests` project/namespace updates, Playwright spec cleanup, and `dotnet build` / `dotnet test` validation.
  - **Wash**: Bicep modules, `azure.yaml`, Dockerfiles, `appsettings.json`, and GitHub/AzDO pipelines rebrand updates.


