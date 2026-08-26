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

### 2026-08-26 — Phase 3 backend for Events & Recurrence

- Updated `IEventRepository` and `EventSQLRepository` to implement `GetEventsByCircleAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, and `DeleteAsync` with active circle membership guards (`LeftUtc == null`), preserving backward compatibility aliases.
- Implemented `EventOccurrence`, `IRecurrenceService`, and `RecurrenceService` in `DadABase.Data.Services` supporting Weekly (specify day of week), Biweekly (every 2 weeks), and Monthly (day of month) recurrence expansion within a date window.
- Registered `IRecurrenceService` in `Program.cs` for DI.
- Added comprehensive xUnit tests in `EventRepository_Tests.cs` (CRUD, circle privacy isolation, former member guards) and `RecurrenceService_Tests.cs` (Weekly, Biweekly, Monthly, date windows, cancelled events, non-recurring events). All 82 tests pass.
