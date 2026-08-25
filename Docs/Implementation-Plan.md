# Get Together Implementation Plan

## Status

Draft planning document for the v1 product scope defined in PRD.md and aligned to the repository architecture in MAP.md.

## 1. Planning Lens Review

### 1.1 PRD-first review

The PRD tells us the product must feel lightweight and social, not platform-heavy. The user experience is intentionally narrow:

- a private circle of friends
- invite-based onboarding
- event creation in under a minute
- one-click RSVP state changes
- a combined calendar across all circles
- email notifications for creation and reminders

This suggests the product should be led by user journeys and privacy boundaries, not by the inherited joke data model. The core product risks are not technical complexity but domain clarity:

- correct invite-code lifecycle
- robust circle membership isolation
- recurring-event semantics
- RSVP arbitration and reminder permissions
- calendar aggregation without cross-circle leakage

The PRD gives us a clean delivery order:

1. auth and invitation onboarding
2. circles and membership
3. events and RSVPs
4. reminder notifications
5. calendar experience
6. hardening and deployment validation

### 1.2 Repo-architecture-first review

The current repository is a .NET 10 Blazor Server app with a shared data library, EF Core, Azure SQL, Azure App Service deployment, and Playwright tests. That is a good structural base for a private social planner because it already provides:

- strong server-side app hosting
- data access layers with repository boundaries
- SQL-backed persistence and Azure deployment patterns
- existing automated test patterns and browser smoke coverage
- a Bicep + Azure packaging pipeline that can be reused for deployment

The repo-first risk is that the inherited app is still joke-centric. The work is not a greenfield app; it is a focused product conversion from a sample app into a new domain. That means the main effort is not inventing a stack but refactoring the idea and data model while preserving the proven runtime and deployment patterns.

### 1.3 Rubber-duck synthesis

A simple way to see the product clearly is this:

- The app is not a public event platform.
- The app is a private planning surface for real friend groups.
- Identity is external; the app does not own user credentials.
- A circle is the true security boundary.
- An event is a circle-scoped item with membership and RSVP state.
- The calendar is just a filtered aggregation of all events the current member can access.

The correct implementation strategy is therefore: use the repo architecture as the scaffold, but build the domain around privacy, membership, and small-group workflow.

## 2. Combined Implementation Strategy

The combined plan is to keep the existing .NET 10 + Blazor Server + EF Core + Azure SQL foundation, while replacing the joke-specific domain with a privacy-first circle/event model.

The work should happen in dependency order, not in a single large rewrite:

1. rebrand and baseline the inherited application
2. establish auth and invite-code onboarding
3. build circle membership and access boundaries
4. implement events and recurring rules
5. implement RSVP state management and notification triggers
6. build aggregated calendar views
7. complete testing, privacy validation, and deployment checks

## 3. Phase-by-Phase Plan

### Phase 0: Rebrand and foundation reset

Objective: convert the base repo from a Dad-A-Base joke app to a viable Get Together application foundation without disturbing the runtime architecture.

Work items:

- rename remaining joke-oriented namespaces, folder names, and sample data where necessary
- replace static references to jokes with friend-circle/domain terminology in the app shell
- preserve the current architecture, deployment conventions, and test scaffolding
- introduce the initial domain entities and project structure for:
  - User
  - Circle
  - CircleMembership
  - InvitationCode
  - Event
  - EventOccurrence or recurrence metadata
  - RSVP
  - ReminderLog or similar notification tracking
- define a base service boundary for domain logic across the web project

Key file areas:

- src/web/Website/
- src/web/Data/
- src/web/Tests/
- MAP.md and any repo docs that describe the current architecture

Definition of done:

- the repository builds cleanly with the new naming baseline
- the app still runs locally with the same deployment model
- the domain model is in place, even if the UI is still skeletal

### Phase 1: Authentication and invite-based onboarding

Objective: make signup and identity fit the product’s private, invitation-only model.

Work items:

- configure external auth providers: Google, Microsoft Entra ID, and Facebook
- map provider identity to a local User record with email and display name
- add signup flow that requires a valid, unused invitation code
- consume the code on successful first-time registration
- redirect new users directly into the circle tied to their invite
- avoid app-managed passwords entirely

Important implementation constraints:

- all circle membership should be enforced at the service/repository level, not only in the UI
- signup should not allow a user to bypass the invite code or join a circle without membership
- the code should be single-use and tied to exactly one circle and one invited person

Key file areas:

- src/web/Website/Program.cs
- src/web/Website/Pages/
- src/web/Website/Shared/
- src/web/Website/Components/
- src/web/Data/

Verification:

- valid invite code creates a member and lands them in the correct circle
- invalid/expired/used code blocks signup
- cross-circle access is prevented by query/service logic and not just by UI hiding

### Phase 2: Circles and membership model

Objective: create the private social structure that follows all circle-scoped data rules.

Work items:

- add Circle entity and CircleMembership model
- support multiple circles per user
- support independent membership, events, and invites per circle
- display a full member roster for members of the circle
- allow any member to generate a one-time invite to another person
- keep the v1 permission model simple: every member has equal power within that circle

Key design decisions:

- circle membership is the security boundary for every event, RSVP, and reminder
- no global event search or public directory
- all queries should be scoped by user membership and selected circle

Verification:

- user can join multiple circles and switch between them
- event list and roster in one circle never leak data from another circle
- all members can see the same roster within the circle

### Phase 3: Event creation and recurring rules

Objective: support the actual product use case: private event planning inside a circle.

Work items:

- allow circle members to create one-off events
- allow circle members to create recurring events
- define the recurrence vocabulary early and keep it intentionally narrow in v1
- support both RSVP modes:
  - per-occurrence
  - series-wide
- persist enough data for recurring views and roster reporting
- allow event creators and members to view the full RSVP roster

Recommended implementation approach:

- keep a base Event table with StartDateTime, EndDateTime, Location, Description, CircleId, and recurrence metadata
- separate recurring-series logic from occurrence-specific behavior to avoid muddying the data model
- only support a simplified recurrence set for v1 unless product owners explicitly demand broader RFC5545 support

Important open decisions to lock before implementation:

- whether the recurrence set is weekly-only, plus monthly/biweekly variants
- whether future occurrences continue if a member leaves the circle
- whether reminder permissions are creator-only or any member

Verification:

- event appears only in its circle
- one-off and recurring events both render properly
- RSVP mode is honored correctly for the selected recurrence style
- non-members cannot access event details or roster

### Phase 4: RSVP workflow and reminder service

Objective: make the actual “accept / maybe / decline” user flow trustworthy and visible.

Work items:

- model RSVP states: Accept, Decline, Maybe
- bind RSVP to either event or occurrence depending on recurrence mode
- allow users to update their RSVP before the event occurs
- count and display attendance states for each event
- provide a clean “unanswered members” view
- trigger manual reminder emails to selected members or everyone
- send creation emails to all circle members when a new event is created

Notification architecture:

- keep SendGrid behind a service abstraction
- treat email as a domain-side concern, not a page concern
- log a reminder action so it can be audited and retried if needed

Verification:

- RSVP updates appear immediately in the roster and totals
- reminder email chooses the correct audience
- event creation email contains detail and direct RSVP link
- users not in the circle never receive emails for its events

### Phase 5: Combined calendar experience

Objective: give each member a simple daily view of everything happening across all circles they belong to.

Work items:

- aggregate events across all circles the user belongs to
- support month-grid and agenda/list views
- support event colors or labels for circle differentiation
- add day drill-down for the month view
- allow event detail navigation from calendar entries

Recommended design:

- the calendar should be read-only aggregation at the app layer
- all event queries should still pass through circle membership authorization
- rendering should clearly distinguish circles without exposing cross-circle content

Verification:

- user sees only their circle events
- month view and list view are both consistent with the same underlying data
- event links take the user to authorized event detail and roster pages

### Phase 6: Testing and quality hardening

Objective: ensure the product meets its private social use case and doesn’t regress on privacy or workflow correctness.

Work items:

- xUnit tests for invitation and membership logic
- xUnit tests for event creation, recurrence handling, and RSVP behavior
- xUnit tests for reminder targeting and audience filtering
- Playwright browser tests for onboarding, event creation, RSVP, and calendar browsing
- privacy validation tests for circle separation and unauthorized access

Suggested test areas:

- valid/invalid invite code handling
- duplicate or expired invitations
- multi-circle user experience
- recurring event per-occurrence behavior
- event roster and attendance counts
- reminder email targeting
- cross-circle navigation restrictions

Verification:

- relevant test suites pass in CI or local validation
- no cross-circle leakage in UI or API flows
- sign-up, create-event, RSVP, and calendar flows are production-ready

### Phase 7: Deployment and release validation

Objective: confirm the app can be deployed and used in the existing Azure architecture without breaking infrastructure expectations.

Work items:

- validate Bicep/app settings still match the repo’s Azure host model
- ensure SendGrid and identity provider settings are externalized
- validate App Service and Azure SQL configuration for the new domain
- perform end-to-end smoke test on the deployed app

Verification:

- app environment loads without committed secrets
- event creation and reminder flows work in the deployed environment
- SQL schema and app settings are consistent with the chosen data source

## 4. Recommended Milestone Sequence

1. Foundation and rebrand
2. Authentication, invite codes, and onboarding
3. Circle membership and access boundaries
4. Event creation and recurring event rules
5. RSVP state and reminder email flow
6. Calendar aggregation and visual grouping
7. Testing, privacy validation, and release readiness

## 5. Key Repository Files to Use

These are the most likely implementation anchors based on the repo structure and product scope:

- PRD.md — product requirements and scope
- MAP.md — repo architecture and implementation map
- src/web/Website/Program.cs — startup and auth configuration
- src/web/Website/Pages/ — pages for signup, circles, events, and calendar
- src/web/Website/Components/ — reusable UI for event cards, roster, and calendar widgets
- src/web/Website/Shared/ — layout, navigation, and common shell components
- src/web/Data/ — domain model, EF Core context, repository abstractions, and service logic
- src/web/Tests/ — xUnit unit tests for service/domain behavior
- playwright/ — browser smoke and workflow coverage
- infra/Bicep/ — Azure deployment configuration
- Docs/ — additional architecture and product documentation

## 6. Key Product Decisions Still Needed Before Building

The PRD lists a few important open questions. These should be answered before implementation lock-in:

- Who can trigger manual reminder emails: any member or only the creator?
- What happens to future occurrences of a recurring event when a member leaves the circle?
- Is there an invitation-code generation limit for abuse prevention?
- Should declined or expired invitation codes remain visible for cleanup?
- What recurrence subset is required for v1: weekly only, weekly + biweekly, monthly, or broader?

These decisions affect domain rules, reminder policy, and event data model details, so they should be resolved before too much implementation work is started.

## 7. Definition of Done for v1

The v1 release is ready when the following are true:

- a new user can receive an invite, sign up, and land in the correct circle
- members can create and view events inside circles
- RSVP states work for one-off and recurring events
- the calendar shows all events across the user’s circles without cross-circle leakage
- all circle data is private to members of that circle
- email notifications are sent for event creation and manual reminder actions
- xUnit and Playwright coverage validate the critical user journeys
- the application builds and deploys using the existing Azure foundation

## 8. Guidance for Implementation Order

The product order should be:

1. onboarding and auth
2. circle membership
3. event creation
4. recurring logic and roster
5. RSVP workflow
6. reminders
7. calendar
8. validation and release

This order keeps the product understandable, private, and testable while preserving the repo’s existing technical architecture.
