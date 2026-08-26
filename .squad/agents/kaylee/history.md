# Kaylee — History

## Project Context

- **Project:** Get Together
- **Owner:** Lyle MS Luppes
- **Stack:** .NET 10 Blazor Server with responsive web UI, EF Core/Azure SQL, Playwright
- **Requirements:** `prd.md`

## Learnings

Squad initialized with Firefly casting on 2026-08-25.

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
