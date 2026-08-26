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

### 2026-08-25T21:00:00Z — Phase 2 focused test follow-up

The reported `--no-build --no-restore` failure was not reproducible: both focused circle membership tests passed, the build-backed focused run passed, and the full web test project passed 54/54. No production or test defect was identified; stale or transient test-run state remains the likely explanation.

### 2026-08-26 — Phase 3 Event & Recurrence Expansion Tests

- Added `RecurrenceExpander` utility in `src/web/Data/Helpers/RecurrenceExpander.cs` for expanding Weekly, Biweekly, and Monthly event recurrence rules.
- Created `src/web/Tests/RepositoryTests/EventRepository_Tests.cs` covering Phase 3 xUnit scenarios:
  1. Event CRUD operations (`CreateEventAsync`, `GetEventAsync`, `GetEventsForCircleAsync`, `GetUpcomingEventsForUserAsync`, `UpdateEventAsync`, `CancelEventAsync`).
  2. Circle-member access guards denying non-member reads, creates, updates, and cancellations with `InvalidOperationException` or `null`/empty returns.
  3. Recurrence expansion logic for `Weekly` (7-day intervals), `Biweekly` (14-day / `INTERVAL=2`), `Monthly` (calendar month offsets), non-recurring events, and boundary limits (`untilUtc`, `maxCount`).
- All 18 narrow tests in `EventRepository_Tests` passed cleanly (total web test suite: 82/82 passing).

### 2026-08-26 — Phase 4 RSVP Workflow, Reminder Targeting & Member Leave Tests

- Extended `IRsvpRepository` and `RsvpSQLRepository` with `GetUnansweredMembersAsync(eventId, requestingUserId)` to query active circle members without an RSVP entry while enforcing circle-membership security guards.
- Fixed UI build issues in `RsvpRoster.razor` and `EventDetail.razor` (`User.EmailAddress` property mapping, Razor quote syntax, `OpenReminderModal` handler).
- Created `src/web/Tests/RepositoryTests/RsvpRepository_Tests.cs` covering Phase 4 xUnit scenarios:
  1. RSVP state transitions (`Accept` -> `Decline` -> `Maybe` -> `Accept`) with notes, timestamp updates, and attendance count aggregations (`Accept`, `Maybe`, `Decline`). Rejection of invalid status strings (`ArgumentException`).
  2. Cross-circle RSVP denial: verifying non-members cannot RSVP (`InvalidOperationException`), view RSVP lists (`GetRsvpsForEventAsync` returns empty), or query unanswered members (`GetUnansweredMembersAsync` returns empty).
  3. Reminder targeting: verifying `GetUnansweredMembersAsync` returns active members who haven't responded, excludes members who have responded, enforces circle membership guards, and integrates with `SendGridNotificationService.SendReminderAsync`.
  4. Member leave cleanup: verifying `CircleRepository.RemoveMemberAsync` soft-deletes membership (`LeftUtc`), purges all RSVPs for that member in that circle from the database, retains remaining members' RSVPs, and updates unanswered lists.
- All 6 narrow tests in `RsvpRepository_Tests` passed cleanly (total web test suite: 88/88 passing).
