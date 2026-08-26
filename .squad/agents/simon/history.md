# Simon — History

## Project Context

- **Project:** Get Together
- **Owner:** Lyle MS Luppes
- **Stack:** .NET 10, ASP.NET Core/Blazor Server, EF Core/Azure SQL, external identity providers, SendGrid
- **Requirements:** `prd.md`

## Learnings

Squad initialized with Firefly casting on 2026-08-25.

- Phase 1 backend seams were already wired through EF Core and DI, but the SQL database project lacked the Get Together table definitions. Added the seven tables and matching constraints/indexes, including the Event alternate key required by RSVP relationships.
- Added `Event.IsRecurring`, `Event.RsvpMode` (default `PerOccurrence`), and nullable `Event.RecurrenceRule`; kept the existing repository APIs unchanged.
- Invitation-code listing now returns all statuses for active circle members, and redemption rejects an already-active member. Six focused xUnit tests pass; web and SQL project builds pass.

### 2026-08-25T20:41:56Z — Cross-agent status

The locked Phase 1 decisions require multi-provider authentication and adding Event recurrence fields now. The requested `simon-phase1-blocker.md` inbox record was absent, so no Simon-specific blocker or validation outcome was recorded beyond the supplied handoff requirements.

### 2026-08-25 — Phase 2 circle repositories

- Completed circle membership operations with active-member authorization, inactive-membership reactivation, active-only roster projections, and RSVP cleanup on removal.
- Invitation code generation remains uncapped, logs the generator/circle/timestamp, and circle listings preserve every status for active members.
- Full web tests pass (54) and the website build passes.

### 2026-08-25 — Phase 2 continuation

- Invitation redemption now reactivates a former member's existing membership row, preserving the unique circle/user membership constraint while retaining rejection of active duplicates.
- Focused invitation redemption and circle repository tests pass; existing repository and web build warnings remain unrelated to this change.

### 2026-08-26 — Phase 4 backend for RSVP Workflow & Reminder Notifications

- Updated `IRsvpRepository` and `RsvpSQLRepository` to implement `GetRsvpAsync`, `GetRsvpsByEventAsync`, `GetRsvpsByOccurrenceAsync`, `UpsertRsvpAsync` (with optional `occurrenceDate` for occurrence-specific responses and idempotent Accept/Decline/Maybe updates), and `GetUnansweredMembersAsync` (with optional `occurrenceDate`). Enforced active circle membership (`LeftUtc == null`) across all RSVP operations.
- Updated `INotificationService` and `SendGridNotificationService` to implement `SendEventCreationEmailAsync` and `SendReminderEmailAsync` (in addition to `SendEventCreatedAsync` and `SendReminderAsync`). Enforced active circle membership on reminder triggers (OQ-1) and target audiences, and persisted all sent reminder events into the `ReminderLog` table via `DadABaseDbContext`.
- Updated `CircleSQLRepository.RemoveMemberAsync` (OQ-2) to purge a departing member's RSVP records for that circle upon soft-deleting membership.
- Enhanced xUnit tests in `RsvpRepository_Tests.cs` covering RSVP state transitions, attendance count aggregations, occurrence-level RSVPs, reminder audience targeting, `ReminderLog` DB logging, and member-leave RSVP cleanup. All 90 xUnit tests pass with 0 build errors.

### 2026-08-26 — Phase 5 backend for Calendar Aggregation & Views

- Implemented `ICalendarAggregationService`, `CalendarAggregationService`, and `CalendarEventItem` in `src/web/Data/Services/`.
- `GetCalendarEventsForUserAsync` fetches all active circles the user belongs to (`_circleRepository.GetCirclesForUserAsync`), retrieves non-cancelled events for those circles, expands recurring and one-off events into occurrences within the window via `IRecurrenceService`, attaches user RSVP status (`UserRsvpStatus`, `UserRsvpNotes`), and attaches circle metadata (`CircleId`, `CircleName`, `CircleColorIndex`, `CircleColor`).
- Strict circle privacy is enforced by filtering active circle memberships (`LeftUtc == null`). Events from circles where a user is a former member are completely excluded.
- Registered `ICalendarAggregationService` / `CalendarAggregationService` in `src/web/Website/Program.cs` under the SQL data source service composition block.
- Added comprehensive xUnit unit tests in `src/web/Tests/ServicesTests/CalendarAggregationService_Tests.cs` testing multi-circle aggregation, date window bounds, recurrence expansion, RSVP attachment, and former-member exclusion. All 95 xUnit tests pass with 0 build errors.

### 2026-08-26 — Phase 6/7 Backend Decoupling Audit & Legacy Cleanup Preparation

- Conducted comprehensive inspection of Get Together SQL repositories (`CircleSQLRepository`, `EventSQLRepository`, `InvitationCodeSQLRepository`, `RsvpSQLRepository`, `UserSQLRepository`). Confirmed **zero coupling or dependencies** to legacy joke tables or joke repositories.
- Cataloged all legacy joke entities (`Joke`, `JokeCategory`, `JokeJokeCategory`, `JokeRating`, `JsonJoke`), joke repositories (`IJokeRepository`, `JokeSQLRepository`, `JokeJsonRepository`), API controllers (`JokeController`, `CategoryController`, `JokeImageController`), database objects (`Dad.Joke*` tables, `vw_Jokes` view, `usp_Joke*` sprocs), and seed assets (`Jokes.json`, AI prompt text files).
- Formulated file-by-file removal checklist and complete namespace migration plan (`DadABase.*` -> `GetTogether.*`, `DadABaseDbContext` -> `GetTogetherDbContext`).
- Executed `dotnet test src/web/Tests/DadABase.Tests.csproj` and `dotnet build src/web/dadabase.net10.web.sln` — verified all 99 xUnit tests pass cleanly with zero build errors. Fully prepared for execution upon directive.

### 2026-08-26 — Final Release Gate: Backend Legacy Purge & Namespace/Project Rebrand (`DadABase` -> `GetTogether`)

- **Purged Legacy Joke Files**: Deleted legacy joke domain models (`Joke.cs`, `JokeCategory.cs`, `JokeJokeCategory.cs`, `JokeRating.cs`, `JsonJoke.cs`), repositories (`IJokeRepository.cs`, `JokeSQLRepository.cs`, `JokeJsonRepository.cs`), API controllers (`JokeController.cs`, `CategoryController.cs`, `JokeImageController.cs`), background/view models (`JokeImageQueueService.cs`, `IJokeImageQueue.cs`, `ProjectEntities.cs`, `JokeBasic.cs`, `JokeList.cs`), seed/prompt files (`Jokes.json`, prompt text files), SQL database objects (`Joke.sql`, `JokeCategory.sql`, `JokeJokeCategory.sql`, `JokeRating.sql`, `vw_Jokes.sql`, `usp_*.sql`), and legacy joke Blazor/Export pages/components.
- **DbContext & Registrations**: Renamed `DadABaseDbContext.cs` -> `GetTogetherDbContext.cs` and class `GetTogetherDbContext`. Stripped all legacy joke `DbSet` properties, `JokeJokeCategory` composite key, and rating precision model configs. Updated `Program.cs` to remove `IJokeRepository`, `JokeJsonRepository`, `Jokes.json` fallback logic, and `IJokeImageQueue` background service registrations. Updated `BaseAPIController.cs` to remove legacy AutoMapper joke mappings. Cleaned `ApplicationDbContext.cs` removing joke DbSets.
- **Project & Solution Rebrand**: Renamed `DadABase.Data.csproj` -> `GetTogether.Data.csproj`, `DadABase.Web.csproj` -> `GetTogether.Web.csproj`, `DadABase.Tests.csproj` -> `GetTogether.Tests.csproj`, and solution `dadabase.net10.web.sln` -> `gettogether.web.sln`. Updated project references and solution file definitions.
- **Namespace Migration**: Rebranded namespaces across all C# and Razor files from `DadABase.Data`, `DadABase.Web`, `DadABase.API`, `DadABase.Tests`, `DadABase.Helpers` to `GetTogether.Data`, `GetTogether.Web`, `GetTogether.API`, `GetTogether.Tests`, `GetTogether.Helpers`.
- **Validation**: Executed `dotnet build src/web/gettogether.web.sln` — 0 build errors across all projects (`GetTogether.Data`, `GetTogether.Web`, `GetTogether.Tests`). Executed `dotnet test src/web/Tests/GetTogether.Tests.csproj` — all 74 unit tests passed cleanly with 100% pass rate.

### 2026-08-26 — Phase 7 Deployment & Release Validation (Backend & DB)

- **SQL DACPAC Build**: Executed `dotnet build src/database/GetTogether.Sql.Database.sln`. Verified clean build and artifact generation (`src/database/bin/Debug/GetTogether.Sql.Database.dacpac`).
- **Schema & DACPAC Matching**: Updated `src/database/Dad/Tables/RSVP.sql` to include `[OccurrenceDate] [datetime2](7) NULL`, ensuring 100% alignment between `GetTogetherDbContext` model definitions (`Circle`, `CircleMembership`, `Event`, `RSVP`, `User`, `InvitationCode`, `ReminderLog`) and DACPAC table scripts.
- **Program.cs Service Registrations**: Verified startup registrations for SQL data source mode (`GetTogetherDbContext`, `ApplicationDbContext`), scoped domain repositories (`CircleSQLRepository`, `EventSQLRepository`, `InvitationCodeSQLRepository`, `RsvpSQLRepository`, `UserSQLRepository`), core services (`RecurrenceService`, `CalendarAggregationService`), and notification service (`SendGridNotificationService`).
- **Build & Test Verification**: `GetTogether.Sql.Database.sln` builds cleanly in 6.1s. `gettogether.web.sln` builds with 0 errors. All 74 unit tests in `GetTogether.Tests.csproj` pass with 100% success rate.
