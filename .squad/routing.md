# Work Routing

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Scope, architecture, priorities, code review | Mal | PRD decomposition, design decisions, reviewer gates |
| Blazor UI and responsive UX | Kaylee | Pages, components, calendar, RSVP interactions |
| .NET services, data, and APIs | Simon | EF Core, Azure SQL, auth, invites, circles, events, SendGrid |
| Tests and quality | River | xUnit, Playwright, acceptance criteria, regression checks |
| Azure infrastructure and delivery | Wash | Bicep, App Service, CI/CD, configuration, deployment |
| Session logging and decisions | Scribe | Automatic after substantial work |
| Work queue monitoring | Ralph | Issue and PR scans, follow-up routing |
| Autonomous small scoped work | @copilot | Only when capability profile is a good fit; auto-assign disabled |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage and assign a `squad:{member}` label | Mal |
| `squad:{member}` | Pick up and complete the issue | Named member |

## Rules

1. Eagerly start independent implementation, test, documentation, and infrastructure work.
2. Scribe runs after substantial work and never blocks the conversation.
3. Quick facts are answered directly by the coordinator.
4. Route cross-domain work to Mal for coordination and review.
