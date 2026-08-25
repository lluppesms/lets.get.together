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

---

## Architecture Patterns & Notes

### Privacy Guard Pattern

All repository methods that read or write circle-scoped data verify that the `requestingUserId` is an active member (`LeftUtc == null`) of the circle before returning data. This is the primary privacy enforcement layer until a proper authorization middleware is in place. Callers in pages/services must pass the authenticated user's resolved `UserId` — not raw claims — obtained via `IUserRepository.FindByExternalIdAsync`.

### User Identity Resolution

`IUserRepository.FindByExternalIdAsync` maps the identity provider's subject claim to the app's `User` table. This call is needed on every authenticated request until a per-request caching layer (e.g., a scoped `CurrentUserService`) is added.

### SendGrid Configuration

Expected configuration keys: `SendGrid:ApiKey`, `SendGrid:FromEmail`, `SendGrid:FromName`. Add to `applicationSettings.json` template (with empty/placeholder values) when SendGrid work begins; do not commit real keys.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
