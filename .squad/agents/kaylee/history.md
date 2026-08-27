# Kaylee — History

## Project Context

- **Project:** Get Together

## Learnings

### 2026-08-27 - Home/About hero and shared header spacing

- Home and About use the exact deployed `/images/Hero-Image.jpg` path with scoped responsive styles and useful alt text.
- `Shared/MainLayout.razor.css` owns app-bar gap, logo dimensions, and aspect-ratio-preserving image sizing; the deployed logo path is `/images/favicon.png`.

### 2026-08-27 - Circle creation flow

- `Pages/Circles.razor` now exposes the existing `ICircleRepository.CreateCircleAsync` contract through an inline, validated create-circle form that is available for both empty and populated circle lists.
- Successful creation navigates the member directly to the new circle; SQL-unavailable environments retain the existing accessible warning state.

📌 Team update (2026-08-26T00:00:00Z): The SQL schema is now `Meetings` (renamed from `Dad`) — decided by Simon. Table scripts moved to `src/database/Meetings/`, EF Core model `Schema` attributes updated to match. Affects deployment scripts and any UI/data assumptions referencing the old `Dad` schema name.
- **Owner:** Lyle MS Luppes
- **Stack:** .NET 10 Blazor Server with responsive web UI, EF Core/Azure SQL, Playwright
- **Requirements:** `prd.md`

## Learnings

Squad initialized with Firefly casting on 2026-08-25.

- The authorized Admin page resolves the persisted application user from the authenticated `ClaimTypes.NameIdentifier` with optional `IUserRepository` resolution, so non-SQL startup and a missing user record remain non-throwing.

- Phase 1 auth UI lives in `src/web/Website/Pages/Login.razor`, `Signup.razor`, and `SignupCallback.razor`, with matching scoped CSS files.
- `Shared/LoginDisplay.razor` is the global anonymous entry point; route it to `/login` so provider readiness is decided in one place.
- The app's existing auth backend is Entra-only and conditional on `AzureAD:TenantId`; provider UI must keep Google/Facebook disabled until backend challenge handlers exist.
- Build validation for the web project is `process: build`; the new auth UI builds cleanly with no warnings after removing nullable annotations from a nullable-disabled Razor project.

### 2026-08-25 - Phase 2 circles UI

- `Pages/Circles.razor` now owns the authenticated circle list/detail flow and uses `AuthenticationStateProvider` plus `IUserRepository.FindByExternalIdAsync` to resolve the persisted user before calling circle or invite repositories.
- `ICircleRepository` and `IInvitationCodeRepository` provide the complete Phase 2 contract, including member-loaded detail, settings update, self-leave, invite history, and revocation. The page derives invite statuses locally from redemption, expiry, and revocation fields.
- Get Together repositories are conditional SQL registrations; resolving them through `IServiceProvider` lets the page render an accessible unavailable message in JSON mode instead of breaking page activation.

### 2026-08-25T20:41:56Z — Cross-agent status

The locked decision ledger confirms Entra ID, Google, and Facebook are in v1. Kaylee's inbox contract records that only Entra is currently actionable and that Google/Facebook buttons must remain disabled until backend challenge, callback, and identity-persistence contracts are defined.

### 2026-08-25 - Phase 2 circles UI continuation

- The existing circles page already covered the supported Phase 2 contracts: circle switching/detail, active member roster, settings, leaving, and invite lifecycle history/statuses.
- Removed the misleading no-op generated-code click handler, added accessible success feedback for settings and invite revocation, and cleared transient detail state when switching circles.
- No new circle contract gap was found; SQL-only unavailable states remain explicit and accessible.

### 2026-08-26 - Phase 3 UI for Events & Recurrence

- Built `Components/EventCreate.razor` and `Components/EventCreate.razor.css` to handle event creation with Title, Circle selection, Location, Description, Start/End Date & Time, IsRecurring toggle, RecurrenceRule (Weekly, Biweekly, Monthly), and RsvpMode (PerOccurrence vs Series).
- Updated `Pages/Events.razor` and created `Pages/Events.razor.css` for viewing upcoming events by selected circle or across all user circles with a circle filter dropdown, toggleable event creation form, quick RSVP action buttons directly on event cards, and count badges.
- Built `Pages/EventDetail.razor` and `Pages/EventDetail.razor.css` for viewing full event specifications, location, formatted times, recurrence schedule info, interactive user RSVP with notes, categorized member RSVP roster (Accepted, Maybe, Declined, Undecided), and event cancellation.
- All Razor pages follow the repository resolution pattern via `IServiceProvider`, render accessible warning alerts in JSON/non-SQL mode, and use single quotes for Blazor `@onclick` string literals. Verified build and all 82 xUnit tests passing.

### 2026-08-26 - Phase 4 UI for RSVP Workflow & Reminders

- Enhanced `Pages/Events.razor` and `Pages/Events.razor.css` with instant optimistic RSVP status updates, per-card busy tracking (`busyEventIds`), visual feedback badges (`cardFeedback`), and inline optional note handling with dedicated "Save Note" action.
- Enhanced `Pages/EventDetail.razor` and `Pages/EventDetail.razor.css` with one-tap interactive RSVP selection (Accept/Maybe/Decline), optimistic roster updates, status feedback banner, and integrated `<RsvpRoster>` component.
- Embedded `<RsvpRoster>` component (`Components/RsvpRoster.razor`) displaying grouped attendee lists (Accepted, Maybe, Declined, Awaiting Response) with member avatars, initials, display names, email addresses, optional notes, search filtering, and count badges.
- Integrated "Send Reminder" workflow in `Pages/EventDetail.razor` with an accessible modal dialog displaying target unanswered member count (`undecidedMembers.Count`), reminder target details, SendGrid integration via `INotificationService.SendReminderEmailAsync`, and member permission checks (per OQ-1).
- Kept UI fully responsive and accessible with single-quote `@onclick` handlers, `aria-pressed`, `aria-modal`, and theme variable styling (`var(--card-bg-light)`, `var(--card-bg-dark)`). Verified 0 build errors in `DadABase.Web` and all 90 xUnit tests passing.

### 2026-08-26 - Phase 5 UI for Calendar Aggregation & Views

- Replaced Phase 0 placeholder at `Pages/Calendar.razor` and created `Pages/Calendar.razor.css` with view switcher (Month View vs. Agenda View), Month navigation controls (Prev/Next month buttons, Today jump button, Current Month/Year header), and Circle filter dropdown ("All My Circles" or specific circle).
- Implemented `Components/MonthCalendar.razor` and `Components/MonthCalendar.razor.css` rendering a 7-column calendar grid for the selected month, day cells with current/padding month states and today marker, event chips color-coded by circle ID using 6 distinct light/dark theme variants (`circle-theme-0` through `circle-theme-5`), and day cell click drill-down modal/drawer displaying day event details, formatted times, location, RSVP status, and link to `/events/detail/{EventId}`.
- Implemented `Components/AgendaView.razor` and `Components/AgendaView.razor.css` displaying chronological event lists grouped by date header, with circle color badges, start/end time, location, RSVP status badge, attendance count summaries, and detail links.
- Verified responsive layout for mobile and desktop, light/dark mode theme variable compliance, 0 build errors in `DadABase.Web`, and clean execution of all xUnit test suites.

### 2026-08-26 - Phase 6 UI Audit & Cleanup

- Audited all Blazor pages (`Pages/`), components (`Components/`), shared layouts (`Shared/`), and static files (`wwwroot/`, CSS) in `src/web/Website/`.
- Purged stale legacy joke Blazor pages (`Pages/JokeDetail.razor`, `Pages/JokeEditor.razor`, `Pages/Random.razor`, `Pages/Export.razor`, and code-behinds/CSS files) and legacy components (`Components/JokeDisplayComponent.razor`, code-behind, and CSS).
- Verified `Shared/NavMenu.razor` and `Shared/MainLayout.razor` contain 100% Get Together branding with navigation links to Home (`/`), Search (`/search`), Circles (`/circles`), Events (`/events`), Calendar (`/calendar`), About (`/about`), and Admin (`/admin`), and zero legacy joke links or categories.
- Updated `Pages/Search.razor` and `Pages/Search.razor.cs` to filter Get Together events and circles matching domain model properties (`e.Title`, `e.Details`, `circle.Name`, `evt.StartsUtc`).
- Updated `applicationSettings.json` AppDescription to `"An ASP.NET Core web app for simple, private event planning for real friends"` and cleaned up legacy test comments in `Pages/Admin.razor.cs`.
- Verified 0 build errors in `DadABase.Web.csproj` and 100% pass rate across all 99 xUnit tests in `DadABase.Tests.csproj`.

### 2026-08-26 - Phase 6/7 UI Rebrand Completion

- Verified and updated `@using GetTogether.Data`, `@using GetTogether.Data.Models`, `@using GetTogether.Data.Repositories`, and `@using GetTogether.Web` in `src/web/Website/_Imports.razor`.
- Explicitly verified and added `@namespace GetTogether.Web.Pages`, `@namespace GetTogether.Web.Components`, `@namespace GetTogether.Web.Shared`, and `@namespace GetTogether.Web` across all Blazor pages (`About.razor`, `Admin.razor`), components (`MessageBubbleComponent.razor`), layouts (`LoginDisplay.razor`, `MainLayout.razor`, `NavMenu.razor`), and root (`App.razor`).
- Audited all UI components, layouts, pages, and CSS styles to verify zero references remain to legacy "Dad Jokes", "Dadabase", "Jokes", or joke domain items. Refactored legacy CSS classes in `Search.razor.css` (`.joke-*` -> `.event-*`) and `wwwroot/css/site.css` (`.JokeCard`/`.JokeText`/`.JokeCategory`/`.joke-image` -> `.EventCard`/`.EventText`/`.EventCategory`/`.event-image`).
- Verified 0 build errors for `GetTogether.Web.csproj` and `gettogether.web.sln` and 100% pass rate (74/74 tests) in `GetTogether.Tests.csproj`.
