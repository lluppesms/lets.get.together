# � Let's Get Together!

> *Because "Reply-All: I'm in!" is not an event management system.*

![Status: Under Construction](https://img.shields.io/badge/Status-Under%20Construction-yellow?style=for-the-badge&logo=hammer)
![Vibe](https://img.shields.io/badge/Vibe-Low%20Effort%2C%20High%20Fun-purple?style=for-the-badge)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Borrowed%20With%20Love-blue?style=for-the-badge)

---

## 🎯 What Is This?

**Get Together** is a small, friendly app for organizing informal gatherings with actual friends — pickleball on Wednesday, movie night, the standing Friday-morning coffee run, a Habitat for Humanity shift. You create a **circle**, invite people you actually know, post a **thing to do**, and everyone taps *Accept*, *Decline*, or *Maybe*.

No ticketing. No public event feed. No payments. No algorithm deciding who sees your pickleball game. Just you, your friends, and a calendar.

**See [PRD.md](./PRD.md) for the full product requirements**, and [PRODUCT.md](./PRODUCT.md) / [MAP.md](./MAP.md) for the living project docs.

---

## 🤔 Why Does This Exist?

Because organizing five friends for pickleball shouldn't require a group text with 40 replies, three "wait what time again?" messages, and someone showing up on the wrong day.

**This project is for:**
- 🏓 Actually getting your circle of friends together, on purpose, on time
- 📅 Seeing every circle's events in one calendar instead of six group chats
- ✅ Knowing at a glance who's in, who's out, and who's ghosting
- 🏗️ Reusing a solid, already-battle-tested .NET/Azure foundation instead of starting from a blank folder

> **Heads up:** this repository was bootstrapped from a sibling demo project (a dad-joke app, of all things) purely to inherit its working .NET 10 / Azure / CI-CD scaffolding. The rebrand — new names, new namespaces, new data model — is the first real chunk of work, tracked in the PRD.

---

## 🚀 What's Being Built (and What It's Built On)

| Piece | Description | Status |
|------------|-------------|--------|
| 🔥 **.NET 10 Blazor App** | The web app where circles, events, and RSVPs will live | ![In Progress](https://img.shields.io/badge/-In%20Progress-yellow) |
| 👥 **Circles & Invitations** | Private friend groups, joined only via single-use invite codes | ![Planned](https://img.shields.io/badge/-Planned-lightgrey) |
| 📅 **Events & RSVPs** | One-off and recurring events with Accept/Decline/Maybe responses | ![Planned](https://img.shields.io/badge/-Planned-lightgrey) |
| 📧 **Email Notifications** | Event-created emails and manually-triggered reminders via SendGrid | ![Planned](https://img.shields.io/badge/-Planned-lightgrey) |
| 🗓️ **Calendar Views** | Month grid + agenda list of everything coming up across your circles | ![Planned](https://img.shields.io/badge/-Planned-lightgrey) |
| 🗃️ **SQL Automation** | Schema + data deployed via SQL DACPAC (inherited, being re-modeled) | ![Schema Migration](https://img.shields.io/badge/-Schema%20Migration-lightgrey) |
| 🏗️ **Bicep IaC** | Full Azure resource deployment, inherited as-is | ![100% Declarative](https://img.shields.io/badge/-100%25%20Declarative-informational) |
| 🔄 **Azure DevOps Pipelines** | Full CI/CD pipelines built with reusable templates | ![Modular](https://img.shields.io/badge/-Modular%20Templates-orange) |
| 🐙 **GitHub Actions** | Because we support *all* the CI/CD platforms | ![Multi-Platform](https://img.shields.io/badge/-Multi--Platform-blueviolet) |
| 🔍 **Code Scanning** | Security scanning, because friend data deserves protection | ![Secure](https://img.shields.io/badge/-Secure-red) |
| 🎭 **Playwright Testing** | Automated browser tests that actually click buttons | ![End-to-End](https://img.shields.io/badge/-End--to--End-9cf) |
| ✅ **Unit Testing** | With code coverage, because untested RSVPs are worse than no RSVPs | ![High Coverage](https://img.shields.io/badge/-High%20Coverage-brightgreen) |
| 🪝 **Pre-Commit Hooks** | Auto-format C# and scan for secrets before every commit | ![Developer Experience](https://img.shields.io/badge/-Developer%20Experience-teal) |

> See [PRD.md](./PRD.md) for the full requirements behind each of these.

---

## 🌱 Planned Features

### 🌐 The Blazor Web App
- 🔑 **Sign in with Google, Microsoft, or Facebook** — no new password to remember
- 🎟️ **Invite-only signup** — a friend's single-use code gets you in
- 👥 **Circles** — private friend groups; belong to as many as you like
- 📅 **One-off and recurring events** — "Friday movie night" or "Pickleball every Saturday"
- ✅ **Accept / Decline / Maybe RSVPs** — know who's actually showing up
- 🗓️ **Combined calendar** — every circle's events, one view, month or agenda style

### 🏗️ Infrastructure as Code
- 🎯 **Bicep templates** that deploy entire environments with one command
- 🔐 **Managed Identity** support - no passwords in config files!
- 📊 **Application Insights** - because we need to monitor whether anyone actually RSVP'd
- 🗄️ **Azure SQL** - real storage for circles, events, and RSVPs

### 🗃️ SQL Database as Code
- 📝 **Complete schema defined in code** - tables, views, stored procedures, all versioned
- 📦 **DACPAC deployment** - database changes deployed via automated pipelines
- 🔄 **CI/CD integration** - schema changes flow through the same pipeline as application code
- ✅ **Schema validation** - ensures database integrity before deployment

### 🎭 Playwright Automated Testing
- 🤖 **End-to-end UI testing** - automated browser tests validate the entire user experience
- 🔄 **Pipeline integration** - tests run automatically after deployment
- 🌐 **Multi-browser support** - verify functionality across different browsers
- 📊 **API testing included** - validate both UI and backend endpoints
- ✅ **Post-deployment validation** - ensure an RSVP actually sticks

---

## 🎬 Quick Start (local test run)

```bash
# Clone the repo
git clone https://github.com/lluppesms/lets.get.together.git

# Install npm dependencies (also activates pre-commit hooks)
npm install

# Navigate to the web project
cd src/web/Website

# Run the Blazor app
dotnet run

# Open browser and go organize something!
```

---

## 🛠️ Developer Setup (Pre-Commit Hooks)

This repo uses [Husky](https://typicode.github.io/husky/) to run pre-commit checks automatically before every `git commit`:

- 🔍 **Secret scanning** via [gitleaks](https://github.com/gitleaks/gitleaks) — blocks commits containing API keys, tokens, or credentials
- 🎨 **Auto-formatting** via `dotnet format` — fixes C# whitespace/style violations and re-stages the corrected files

### One-time setup

1. **Install gitleaks** (required for secret scanning):
   - Windows: `winget install gitleaks`
   - macOS: `brew install gitleaks`

2. **Run `npm install`** — this activates the Husky hooks automatically via the `prepare` script.

That's it! The hooks run on every commit from then on. See [CONTRIBUTING.md](./CONTRIBUTING.md) for full details.

---

## 🔧 Developer Setup

This repo uses [Husky](https://typicode.github.io/husky/) pre-commit hooks to automatically format C# code and scan for secrets before every commit. Two one-time setup steps are required:

### 1. Install gitleaks

The secret scanner must be installed separately on each developer machine:

| Platform | Command |
|----------|---------|
| 🪟 **Windows** | `winget install gitleaks` |
| 🍎 **macOS** | `brew install gitleaks` |
| 🐧 **Linux** | See [gitleaks releases](https://github.com/gitleaks/gitleaks/releases) |

> If `gitleaks` is not found, the hook will skip secret scanning with a warning — it won't block your commit.

### 2. Activate Husky hooks

```bash
npm install
```

That's it! The `prepare` script in `package.json` activates the hooks automatically. From this point on, every `git commit` will:

1. 🔍 **Scan for secrets** using gitleaks (skipped gracefully if not installed)
2. 🎨 **Auto-format staged `.cs` files** using `dotnet format` across all solution files
3. ✅ **Re-stage any auto-formatted files** so they're included in your commit

See [CONTRIBUTING.md](./CONTRIBUTING.md) for full contributor guidelines.

---

## 🚀 Deployment Options

Choose your adventure:

| Method | Documentation | Difficulty |
|--------|---------------|------------|
| 🔄 **Azure DevOps** | [Pipeline Guide](./.azdo/pipelines/readme.md) | ⭐⭐⭐ |
| 🐙 **GitHub Actions** | [Actions Guide](./.github/workflows-readme.md) | ⭐⭐⭐ |
| ⌨️ **AZD CLI** | [AZD Guide](./.azure/readme.md) | ⭐⭐ |

[![azd Compatible](/Docs/images/AZD_Compatible.png)](/.azure/readme.md)

---

## 🔄 CI/CD Pipelines and Actions Showcase

### Azure DevOps Pipelines
Our Azure DevOps pipelines, doing the unglamorous work so you don't have to:

| Pipeline | Purpose |
|----------|---------|
| 🏗️ `deploy-bicep` | Create all Azure resources |
| 🌐 `build-deploy-webapp` | Build, test, and deploy the Blazor app to standard Azure App Service |
| 📦 `build-deploy-containerapp` | Build, test, and deploy the Blazor app to Azure Container Apps |
| ⚡ `build-deploy-function` | Ship the serverless bits |
| 🗃️ `build-deploy-dacpac` | Deploy SQL schema and seed data |
| 🔍 `scan-code` | Security scanning (serious stuff) |
| 🎭 `smoke-test-webapp` | Make sure the app is actually working (automated) |

### GitHub Actions
Same great taste, GitHub flavor:

| Workflow | Badges |
|----------|--------|
| Deploy Infrastructure | [![deploy-bicep](https://github.com/lluppesms/dadabase.demo/actions/workflows/1-deploy-bicep.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/1-deploy-bicep.yml) |
| Build & Deploy Web App (App Service) | [![bicep-build-deploy-webapp](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.1-bicep-build-deploy-webapp.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.1-bicep-build-deploy-webapp.yml) |
| Build & Deploy Web App (Container App) | [![bicep-build-deploy-containerapp](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.2-bicep-build-deploy-containerapp.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.2-bicep-build-deploy-containerapp.yml) |
| Deploy DACPAC | [![build-deploy-dacpac](https://github.com/lluppesms/dadabase.demo/actions/workflows/4-build-deploy-dacpac.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/4-build-deploy-dacpac.yml) |
| Code Scanning | [![scan-code](https://github.com/lluppesms/dadabase.demo/actions/workflows/7-scan-code.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/7-scan-code.yml) |

---

## 🏛️ The Grand Architecture

```
📁 Get Together Repository (currently mid-rebrand from a borrowed scaffold)
├── 🌐 src/web/           → .NET 10 Blazor App (circles, events, RSVPs)
├── ⚡ src/function/       → Azure Function (serverless API bits)
├── 💻 src/console/        → Console App
├── 📊 src/sql.database/   → SQL Database Project (DACPAC central)
├── 🏗️ infra/Bicep/        → Infrastructure as Code (Bicep flexing)
├── 🔄 .azdo/pipelines/    → Azure DevOps CI/CD (YAML wizardry)
├── 🐙 .github/workflows/  → GitHub Actions (also YAML wizardry)
└── 🎭 playwright/         → Automated testing (robot event critics)
```

---

## 🧪 Testing Philosophy

> *"An RSVP without tests is just a guess."* - Ancient DevOps Proverb

- ✅ **Unit Tests** with xUnit and Coverlet for code coverage
- 🎭 **Playwright Tests** for end-to-end UI validation
- 📊 **Test results and Code coverage** integrated directly into CI/CD pipelines - because metrics matter

---

## 📚 Documentation

| Topic | Link |
|-------|------|
| 📖 Coding Standards | [Coding_Standards.md](./Docs/Coding_Standards.md) |
| 🏗️ Infrastructure as Code | [Bicep Instructions](./.github/instructions/bicep-instructions.md) |
| 🗃️ SQL DACPAC Deployment | [SQL/DACPAC Instructions](./.github/instructions/sql-database-dacpac-instructions.md) |
| 🔄 Azure DevOps Pipelines | [AzDO Pipeline Instructions](./.github/instructions/azure-devops-pipeline-instructions.md) |
| 🐙 GitHub Actions | [GitHub Actions Instructions](./.github/instructions/github-actions-instructions.md) |
| 🤝 Contributing & Dev Setup | [CONTRIBUTING.md](./CONTRIBUTING.md) |
| 🤝 Contributing Guide | [CONTRIBUTING.md](./CONTRIBUTING.md) |

---

## 🤝 Contributing

Found a bug? Want to add a feature? Know a friend group that needs this?

Pull requests are welcome! Check out [CONTRIBUTING.md](./CONTRIBUTING.md) for developer setup (including pre-commit hooks), code style, and PR guidelines.

See [CONTRIBUTING.md](./CONTRIBUTING.md) for developer setup and contribution guidelines.

---

## 📜 License

[MIT](./LICENSE) - Because organizing your friends should be free for everyone.

---

*Made with 💚 for anyone tired of 40-message group threads*

**Remember: Good code and good gatherings both require timing.**

</div>
