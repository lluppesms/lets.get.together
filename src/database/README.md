---
title: Get Together SQL Database Project
description: SQL database project structure, deployment, and schema notes for Get Together
author: Get Together maintainers
ms.date: 2026-05-14
ms.topic: reference
keywords:
  - get-together
  - sqlproj
  - dacpac
estimated_reading_time: 7
---

## Get Together SQL Database Project

This is a SQL Server Database Project for the Get Together application. It defines the `Meetings` schema used to store circles, events, and RSVPs. The schema/folder name is `Meetings`.

## Project Structure

```text
database/
├── Meetings/
│   ├── Tables/
│   │   ├── User.sql              # App users (linked to external auth identity)
│   │   ├── Circle.sql            # Circles (private groups)
│   │   ├── CircleMembership.sql  # User membership within a circle
│   │   ├── InvitationCode.sql    # Invite codes used to join a circle
│   │   ├── Event.sql             # Circle events (single or recurring)
│   │   ├── RSVP.sql              # User RSVPs to events
│   │   └── ReminderLog.sql       # Log of reminder notifications sent
│   ├── Pre.Deployment.sql        # Drops legacy dbo objects for offline migration
│   └── Post.Deployment.sql       # Post-deployment script
├── Schemas/
│   └── Meetings.sql              # Meetings schema definition
├── Patch/                         # Patch scripts for updates
└── GetTogether.Sql.Database.sqlproj          # SQL Server Database Project file
```

## Database Schema

### Tables

**User**
- App user linked to an external authentication identity
- Fields: UserId, ExternalId, DisplayName, EmailAddress, IsActive, CreatedUtc

**Circle**
- A private group that owns events and members
- Fields: CircleId, Name, Description, CreatedByUserId, CreatedUtc, IsArchived

**CircleMembership**
- Links a User to a Circle with a role (e.g. Member, Owner)
- Fields: CircleMembershipId, CircleId, UserId, Role, JoinedUtc, LeftUtc

**InvitationCode**
- A code used to invite a new member into a Circle
- Fields: InvitationCodeId, CircleId, Code, CreatedByUserId, CreatedUtc, ExpiresUtc, RedeemedByUserId, RedeemedUtc, RevokedUtc

**Event**
- A single or recurring event belonging to a Circle
- Fields: EventId, CircleId, Title, Details, StartsUtc, EndsUtc, IsRecurring, RsvpMode, RecurrenceRule, CreatedByUserId, CreatedUtc, CancelledUtc

**RSVP**
- A user's RSVP response to an Event
- Fields: RsvpId, EventId, CircleId, UserId, Status, Notes, OccurrenceDate, RespondedUtc

**ReminderLog**
- Log of reminder notifications sent for an Event
- Fields: ReminderLogId, EventId, UserId, Channel, SentUtc, DeliveryState, ProviderMessageId

## Building the Project

### Using Visual Studio

1. Open the solution in Visual Studio with SQL Server Data Tools (SSDT)
2. Build the sql.database project
3. This will create a DACPAC file in the bin folder

### Using MSBuild

```bash
msbuild GetTogether.Sql.Database.sqlproj /p:Configuration=Release
```

### Using SQL Server Data Tools Build

```bash
# Navigate to project directory
cd src/database

# Build the project
SqlPackage.exe /Action:Build /SourceFile:GetTogether.Sql.Database.sqlproj
```

## Deploying the Database

### Option 1: Using SqlPackage CLI

```bash
SqlPackage.exe /Action:Publish \
  /SourceFile:bin/Release/GetTogether.Sql.Database.dacpac \
  /TargetConnectionString:"Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=LetsGetTogether;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;" \
  /p:ScriptDatabaseOptions=False
```

For a fresh environment, publish the DACPAC to an empty database. The pre-deploy
script is idempotent and does nothing when the legacy `dbo` LetsGetTogether objects are
not present. Load starter data only if the environment should begin with sample circles/events.

### Option 2: Using Visual Studio SSDT

1. Right-click the project in Solution Explorer
2. Select "Publish"
3. Configure your target database connection
4. Click "Publish"

### Option 3: Using Azure DevOps Pipeline

The project includes pipeline templates for automated deployment:
- Build DACPAC artifact
- Deploy to Azure SQL Database
- Support for multiple environments (DEV, QA, PROD)

See the `.azdo/pipelines/` folder for deployment pipeline examples.

## Populating with Data

After deploying the database schema, populate it with sample data:

1. Navigate to the `Patch/` folder
2. Run `InsertDefaultData.sql` to populate the database with sample users, circles, memberships, invitation codes, events, and RSVPs

## Bicep Infrastructure

The database infrastructure is defined in Bicep templates at `/infra/Bicep/`:

- `sqlserver.bicep` - Azure SQL Server and Database configuration
- `main.bicep` - Main deployment orchestration

Deploy infrastructure before deploying schema:

```bash
az deployment group create \
  --resource-group rg-LetsGetTogether-dev \
  --template-file infra/Bicep/main.bicep \
  --parameters sqlDatabaseName=LetsGetTogether
```

## Reusability

This database project is designed to be reusable across different applications:

- **Web Application**: Used by the main web app in `src/web`
- **Function App**: Can be used by Azure Functions in `src/function`  
- **Console App**: Can be used by console applications in `src/console`
- **MCP Services**: Can be used by Model Context Protocol services in `src/mcp`

## Connection Strings

Example connection strings for different environments:

**Azure SQL Database**

``` sql
data source=tcp:<databaseServerName>.database.windows.net,1433;Database=YourBase;Authentication=Active Directory Default;Connection Timeout=120;
```

**LocalDB (Development)**

``` sql
Server=(localdb)\\mssqllocaldb;Database=LetsGetTogether;Trusted_Connection=True;MultipleActiveResultSets=true
```

## Entity Framework Integration

This database schema is designed to work with Entity Framework Core. The corresponding C# models are located in:
- `src/web/Data/Models/`

The models include:
- `User.cs`
- `Circle.cs`
- `CircleMembership.cs`
- `InvitationCode.cs`
- `Event.cs`
- `RSVP.cs`
- `ReminderLog.cs`

## Patch Scripts

The `Patch/` folder is for database update scripts that need to be run manually or as part of a specific deployment:
- Use naming convention: `Patch-YYYYMMDD.sql`
- These are not automatically executed during DACPAC deployment
- Useful for data migrations, one-time updates, or breaking changes

## Version History

- **v2.0** - Renamed schema/folder to `Meetings`; schema now models Users, Circles, CircleMemberships, InvitationCodes, Events, RSVPs, and ReminderLog
- **v1.0** - Initial database schema inherited from the LetsGetTogether joke app (Joke, JokeCategory, JokeRating tables)
- Foreign key relationships with cascade rules
- Default constraints for audit fields

## Support

For issues or questions about the database schema, please refer to:
- Main repository documentation at `/Docs/sql/README.md`
- Bicep infrastructure templates at `/infra/Bicep/`
