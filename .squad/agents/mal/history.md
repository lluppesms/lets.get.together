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


