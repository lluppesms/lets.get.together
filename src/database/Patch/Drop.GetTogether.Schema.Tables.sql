/*
Database rollback script for application tables
Drops all tables in the [Meetings] schema (in dependency-safe order) and then drops the schema itself.

WARNING: This is destructive and irreversible. All data in these tables will be lost.
*/

-- USE Meetup
-- GO

-- ------------------------------------------------------------------------------------------------------------------------
PRINT N'Dropping tables in schema [Meetings]...';
GO

IF OBJECT_ID(N'[Meetings].[ReminderLog]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[ReminderLog];
GO

IF OBJECT_ID(N'[Meetings].[RSVP]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[RSVP];
GO

IF OBJECT_ID(N'[Meetings].[EmailVerificationToken]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[EmailVerificationToken];
GO

IF OBJECT_ID(N'[Meetings].[UserIdentity]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[UserIdentity];
GO

IF OBJECT_ID(N'[Meetings].[UserEmailAlias]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[UserEmailAlias];
GO

IF OBJECT_ID(N'[Meetings].[InvitationCode]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[InvitationCode];
GO

IF OBJECT_ID(N'[Meetings].[CircleMembership]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[CircleMembership];
GO

IF OBJECT_ID(N'[Meetings].[Event]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[Event];
GO

IF OBJECT_ID(N'[Meetings].[Circle]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[Circle];
GO

IF OBJECT_ID(N'[Meetings].[User]', N'U') IS NOT NULL
    DROP TABLE [Meetings].[User];
GO

-- ------------------------------------------------------------------------------------------------------------------------
PRINT N'Dropping Schema [Meetings]...';
GO

IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Meetings')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'Meetings')
    BEGIN
        PRINT N'Cannot drop schema [Meetings] - tables still exist in the schema.';
    END
    ELSE
    BEGIN
        DROP SCHEMA [Meetings];
    END
END
GO

-- ------------------------------------------------------------------------------------------------------------------------
PRINT 'Schema rollback completed successfully.'
