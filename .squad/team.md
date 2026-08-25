# Squad Team

> Get Together

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Mal | Lead | `.squad/agents/mal/charter.md` | 🏗️ Active |
| Kaylee | Frontend Dev | `.squad/agents/kaylee/charter.md` | ⚛️ Active |
| Simon | Backend Dev | `.squad/agents/simon/charter.md` | 🔧 Active |
| River | Tester | `.squad/agents/river/charter.md` | 🧪 Active |
| Wash | DevOps / Infra | `.squad/agents/wash/charter.md` | ⚙️ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Active |
| Ralph | Work Monitor | `.squad/agents/ralph/charter.md` | 🔄 Active |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

### Capabilities

**🟢 Good fit — auto-route when enabled:**
- Bug fixes with clear reproduction steps
- Test coverage and lint/format fixes
- Dependency updates and documentation fixes
- Small isolated features with clear specifications

**🟡 Needs review — route to @copilot but flag for squad member PR review:**
- Medium features with clear acceptance criteria
- Refactoring with existing test coverage
- API endpoint additions following established patterns

**🔴 Not suitable — route to squad member instead:**
- Architecture decisions and system design
- Multi-system integration requiring coordination
- Ambiguous requirements or security-critical changes

## Project Context

- **Project:** Get Together
- **Repository:** `lluppesms/lets.get.together`
- **Owner:** Lyle MS Luppes
- **Stack:** .NET 10 Blazor Server, EF Core/Azure SQL, Bicep, Azure App Service, SendGrid, GitHub Actions/Azure DevOps, xUnit, Playwright
- **Purpose:** Private invite-only circles where friends create events and RSVP Accept / Decline / Maybe.
- **Source of truth:** `prd.md` is the current product requirements document and is still pending owner approval.
- **Created:** 2026-08-25
