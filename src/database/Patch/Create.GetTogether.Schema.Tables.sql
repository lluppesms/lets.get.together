/*
Deployment script for LuppesShared
*/

USE LuppesShared;
GO

PRINT N'Creating Schema [Meetings]...';
GO

CREATE SCHEMA [Meetings]
    AUTHORIZATION [dbo];
GO

PRINT N'Creating Table [Meetings].[ReminderLog]...';
GO
CREATE TABLE [Meetings].[ReminderLog] (
    [ReminderLogId]     INT            IDENTITY (1, 1) NOT NULL,
    [EventId]           INT            NOT NULL,
    [UserId]            INT            NOT NULL,
    [Channel]           NVARCHAR (30)  NOT NULL,
    [SentUtc]           DATETIME2 (7)  NOT NULL,
    [DeliveryState]     NVARCHAR (30)  NOT NULL,
    [ProviderMessageId] NVARCHAR (120) NULL,
    CONSTRAINT [PK_ReminderLog] PRIMARY KEY CLUSTERED ([ReminderLogId] ASC)
);


GO
PRINT N'Creating Index [Meetings].[ReminderLog].[IX_ReminderLog_EventId_UserId_SentUtc]...';


GO
CREATE NONCLUSTERED INDEX [IX_ReminderLog_EventId_UserId_SentUtc]
    ON [Meetings].[ReminderLog]([EventId] ASC, [UserId] ASC, [SentUtc] ASC);


GO
PRINT N'Creating Table [Meetings].[RSVP]...';


GO
CREATE TABLE [Meetings].[RSVP] (
    [RsvpId]         INT             IDENTITY (1, 1) NOT NULL,
    [EventId]        INT             NOT NULL,
    [CircleId]       INT             NOT NULL,
    [UserId]         INT             NOT NULL,
    [Status]         NVARCHAR (32)   NOT NULL,
    [Notes]          NVARCHAR (1000) NULL,
    [OccurrenceDate] DATETIME2 (7)   NULL,
    [RespondedUtc]   DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_RSVP] PRIMARY KEY CLUSTERED ([RsvpId] ASC)
);


GO
PRINT N'Creating Index [Meetings].[RSVP].[IX_RSVP_EventId_UserId]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RSVP_EventId_UserId]
    ON [Meetings].[RSVP]([EventId] ASC, [UserId] ASC);


GO
PRINT N'Creating Table [Meetings].[Event]...';


GO
CREATE TABLE [Meetings].[Event] (
    [EventId]         INT             IDENTITY (1, 1) NOT NULL,
    [CircleId]        INT             NOT NULL,
    [Title]           NVARCHAR (200)  NOT NULL,
    [Details]         NVARCHAR (2000) NULL,
    [StartsUtc]       DATETIME2 (7)   NOT NULL,
    [EndsUtc]         DATETIME2 (7)   NULL,
    [IsRecurring]     BIT             NOT NULL,
    [RsvpMode]        INT             NOT NULL,
    [RecurrenceRule]  NVARCHAR (200)  NULL,
    [CreatedByUserId] INT             NOT NULL,
    [CreatedUtc]      DATETIME2 (7)   NOT NULL,
    [CancelledUtc]    DATETIME2 (7)   NULL,
    CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED ([EventId] ASC),
    CONSTRAINT [AK_Event_EventId_CircleId] UNIQUE NONCLUSTERED ([EventId] ASC, [CircleId] ASC)
);


GO
PRINT N'Creating Index [Meetings].[Event].[IX_Event_CircleId_StartsUtc]...';


GO
CREATE NONCLUSTERED INDEX [IX_Event_CircleId_StartsUtc]
    ON [Meetings].[Event]([CircleId] ASC, [StartsUtc] ASC);


GO
PRINT N'Creating Table [Meetings].[InvitationCode]...';


GO
CREATE TABLE [Meetings].[InvitationCode] (
    [InvitationCodeId] INT           IDENTITY (1, 1) NOT NULL,
    [CircleId]         INT           NOT NULL,
    [Code]             NVARCHAR (64) NOT NULL,
    [CreatedByUserId]  INT           NOT NULL,
    [CreatedUtc]       DATETIME2 (7) NOT NULL,
    [ExpiresUtc]       DATETIME2 (7) NULL,
    [RedeemedByUserId] INT           NULL,
    [RedeemedUtc]      DATETIME2 (7) NULL,
    [RevokedUtc]       DATETIME2 (7) NULL,
    CONSTRAINT [PK_InvitationCode] PRIMARY KEY CLUSTERED ([InvitationCodeId] ASC)
);


GO
PRINT N'Creating Index [Meetings].[InvitationCode].[IX_InvitationCode_Code]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_InvitationCode_Code]
    ON [Meetings].[InvitationCode]([Code] ASC);


GO
PRINT N'Creating Index [Meetings].[InvitationCode].[IX_InvitationCode_CircleId_CreatedUtc]...';


GO
CREATE NONCLUSTERED INDEX [IX_InvitationCode_CircleId_CreatedUtc]
    ON [Meetings].[InvitationCode]([CircleId] ASC, [CreatedUtc] ASC);


GO
PRINT N'Creating Table [Meetings].[CircleMembership]...';


GO
CREATE TABLE [Meetings].[CircleMembership] (
    [CircleMembershipId] INT           IDENTITY (1, 1) NOT NULL,
    [CircleId]           INT           NOT NULL,
    [UserId]             INT           NOT NULL,
    [Role]               NVARCHAR (50) NOT NULL,
    [JoinedUtc]          DATETIME2 (7) NOT NULL,
    [LeftUtc]            DATETIME2 (7) NULL,
    CONSTRAINT [PK_CircleMembership] PRIMARY KEY CLUSTERED ([CircleMembershipId] ASC)
);


GO
PRINT N'Creating Index [Meetings].[CircleMembership].[IX_CircleMembership_CircleId_UserId]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_CircleMembership_CircleId_UserId]
    ON [Meetings].[CircleMembership]([CircleId] ASC, [UserId] ASC);


GO
PRINT N'Creating Table [Meetings].[Circle]...';


GO
CREATE TABLE [Meetings].[Circle] (
    [CircleId]        INT             IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (200)  NOT NULL,
    [Description]     NVARCHAR (1000) NULL,
    [CreatedByUserId] INT             NOT NULL,
    [CreatedUtc]      DATETIME2 (7)   NOT NULL,
    [IsArchived]      BIT             NOT NULL,
    CONSTRAINT [PK_Circle] PRIMARY KEY CLUSTERED ([CircleId] ASC)
);


GO
PRINT N'Creating Table [Meetings].[User]...';


GO
CREATE TABLE [Meetings].[User] (
    [UserId]       INT            IDENTITY (1, 1) NOT NULL,
    [ExternalId]   NVARCHAR (200) NOT NULL,
    [DisplayName]  NVARCHAR (200) NOT NULL,
    [EmailAddress] NVARCHAR (320) NOT NULL,
    [IsActive]     BIT            NOT NULL,
    [CreatedUtc]   DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserId] ASC)
);


GO
PRINT N'Creating Index [Meetings].[User].[IX_User_ExternalId]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_ExternalId]
    ON [Meetings].[User]([ExternalId] ASC);


GO
PRINT N'Creating Index [Meetings].[User].[IX_User_EmailAddress]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_EmailAddress]
    ON [Meetings].[User]([EmailAddress] ASC);


GO
PRINT N'Creating Default Constraint [Meetings].[DF_ReminderLog_Channel]...';


GO
ALTER TABLE [Meetings].[ReminderLog]
    ADD CONSTRAINT [DF_ReminderLog_Channel] DEFAULT (N'InApp') FOR [Channel];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_ReminderLog_SentUtc]...';


GO
ALTER TABLE [Meetings].[ReminderLog]
    ADD CONSTRAINT [DF_ReminderLog_SentUtc] DEFAULT (getutcdate()) FOR [SentUtc];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_ReminderLog_DeliveryState]...';


GO
ALTER TABLE [Meetings].[ReminderLog]
    ADD CONSTRAINT [DF_ReminderLog_DeliveryState] DEFAULT (N'Queued') FOR [DeliveryState];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_RSVP_Status]...';


GO
ALTER TABLE [Meetings].[RSVP]
    ADD CONSTRAINT [DF_RSVP_Status] DEFAULT (N'Pending') FOR [Status];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_RSVP_RespondedUtc]...';


GO
ALTER TABLE [Meetings].[RSVP]
    ADD CONSTRAINT [DF_RSVP_RespondedUtc] DEFAULT (getutcdate()) FOR [RespondedUtc];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_Event_IsRecurring]...';


GO
ALTER TABLE [Meetings].[Event]
    ADD CONSTRAINT [DF_Event_IsRecurring] DEFAULT ((0)) FOR [IsRecurring];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_Event_RsvpMode]...';


GO
ALTER TABLE [Meetings].[Event]
    ADD CONSTRAINT [DF_Event_RsvpMode] DEFAULT ((0)) FOR [RsvpMode];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_Event_CreatedUtc]...';


GO
ALTER TABLE [Meetings].[Event]
    ADD CONSTRAINT [DF_Event_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_InvitationCode_CreatedUtc]...';


GO
ALTER TABLE [Meetings].[InvitationCode]
    ADD CONSTRAINT [DF_InvitationCode_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_CircleMembership_Role]...';


GO
ALTER TABLE [Meetings].[CircleMembership]
    ADD CONSTRAINT [DF_CircleMembership_Role] DEFAULT (N'Member') FOR [Role];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_CircleMembership_JoinedUtc]...';


GO
ALTER TABLE [Meetings].[CircleMembership]
    ADD CONSTRAINT [DF_CircleMembership_JoinedUtc] DEFAULT (getutcdate()) FOR [JoinedUtc];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_Circle_CreatedUtc]...';


GO
ALTER TABLE [Meetings].[Circle]
    ADD CONSTRAINT [DF_Circle_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_Circle_IsArchived]...';


GO
ALTER TABLE [Meetings].[Circle]
    ADD CONSTRAINT [DF_Circle_IsArchived] DEFAULT ((0)) FOR [IsArchived];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_User_IsActive]...';


GO
ALTER TABLE [Meetings].[User]
    ADD CONSTRAINT [DF_User_IsActive] DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint [Meetings].[DF_User_CreatedUtc]...';


GO
ALTER TABLE [Meetings].[User]
    ADD CONSTRAINT [DF_User_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc];


GO
PRINT N'Creating Foreign Key [Meetings].[FK_ReminderLog_Event]...';


GO
ALTER TABLE [Meetings].[ReminderLog] WITH NOCHECK
    ADD CONSTRAINT [FK_ReminderLog_Event] FOREIGN KEY ([EventId]) REFERENCES [Meetings].[Event] ([EventId]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [Meetings].[FK_ReminderLog_User]...';


GO
ALTER TABLE [Meetings].[ReminderLog] WITH NOCHECK
    ADD CONSTRAINT [FK_ReminderLog_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId]);


GO
PRINT N'Creating Foreign Key [Meetings].[FK_RSVP_Event]...';


GO
ALTER TABLE [Meetings].[RSVP] WITH NOCHECK
    ADD CONSTRAINT [FK_RSVP_Event] FOREIGN KEY ([EventId], [CircleId]) REFERENCES [Meetings].[Event] ([EventId], [CircleId]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [Meetings].[FK_RSVP_User]...';


GO
ALTER TABLE [Meetings].[RSVP] WITH NOCHECK
    ADD CONSTRAINT [FK_RSVP_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId]);


GO
PRINT N'Creating Foreign Key [Meetings].[FK_RSVP_CircleMembership]...';


GO
ALTER TABLE [Meetings].[RSVP] WITH NOCHECK
    ADD CONSTRAINT [FK_RSVP_CircleMembership] FOREIGN KEY ([CircleId], [UserId]) REFERENCES [Meetings].[CircleMembership] ([CircleId], [UserId]);


GO
PRINT N'Creating Foreign Key [Meetings].[FK_Event_Circle]...';


GO
ALTER TABLE [Meetings].[Event] WITH NOCHECK
    ADD CONSTRAINT [FK_Event_Circle] FOREIGN KEY ([CircleId]) REFERENCES [Meetings].[Circle] ([CircleId]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [Meetings].[FK_Event_User_CreatedBy]...';


GO
ALTER TABLE [Meetings].[Event] WITH NOCHECK
    ADD CONSTRAINT [FK_Event_User_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Meetings].[User] ([UserId]);


GO
PRINT N'Creating Foreign Key [Meetings].[FK_InvitationCode_Circle]...';


GO
ALTER TABLE [Meetings].[InvitationCode] WITH NOCHECK
    ADD CONSTRAINT [FK_InvitationCode_Circle] FOREIGN KEY ([CircleId]) REFERENCES [Meetings].[Circle] ([CircleId]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [Meetings].[FK_InvitationCode_User_CreatedBy]...';


GO
ALTER TABLE [Meetings].[InvitationCode] WITH NOCHECK
    ADD CONSTRAINT [FK_InvitationCode_User_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Meetings].[User] ([UserId]);


GO
PRINT N'Creating Foreign Key [Meetings].[FK_InvitationCode_User_RedeemedBy]...';


GO
ALTER TABLE [Meetings].[InvitationCode] WITH NOCHECK
    ADD CONSTRAINT [FK_InvitationCode_User_RedeemedBy] FOREIGN KEY ([RedeemedByUserId]) REFERENCES [Meetings].[User] ([UserId]) ON DELETE SET NULL;


GO
PRINT N'Creating Foreign Key [Meetings].[FK_CircleMembership_Circle]...';


GO
ALTER TABLE [Meetings].[CircleMembership] WITH NOCHECK
    ADD CONSTRAINT [FK_CircleMembership_Circle] FOREIGN KEY ([CircleId]) REFERENCES [Meetings].[Circle] ([CircleId]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [Meetings].[FK_CircleMembership_User]...';


GO
ALTER TABLE [Meetings].[CircleMembership] WITH NOCHECK
    ADD CONSTRAINT [FK_CircleMembership_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId]);


GO
PRINT N'Creating Foreign Key [Meetings].[FK_Circle_User_CreatedByUserId]...';


GO
ALTER TABLE [Meetings].[Circle] WITH NOCHECK
    ADD CONSTRAINT [FK_Circle_User_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Meetings].[User] ([UserId]);


GO
/*
Post-Deployment Script for LetsGetTogether SQL Database
This script is run after the database schema deployment completes.
Use SQLCMD syntax to reference external scripts or variables.
*/

PRINT 'Post-deployment script started.'
PRINT 'Database: LetsGetTogether'
PRINT 'Schema deployment completed successfully.'

-- Note: To populate the database with sample data, use the InsertDefaultData.sql script
-- located in the Docs/sql folder of the repository. This script can be run manually
-- after deployment or integrated into your deployment pipeline.

PRINT 'To populate with sample data, run the InsertDefaultData.sql script from Docs/sql folder.'
PRINT 'Post-deployment script completed.'

PRINT 'To grant rights to your CICD pipeline to use the DACPAC to create the schema:'
PRINT '  Creating user [your_cicd_pipeline_sp] from external provider...';
PRINT '  ALTER ROLE db_owner ADD MEMBER [your_cicd_pipeline_sp];'

PRINT 'To grant rights to your application to use database:'
PRINT '  Creating user [your_managed_identity] from external provider...';
PRINT '  GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Meetings] TO [your_managed_identity];'
PRINT '  GRANT EXECUTE ON SCHEMA::[Meetings] TO [your_managed_identity];'

GO

GO
PRINT N'Checking existing data against newly created constraints';


GO
USE LuppesShared;
GO

ALTER TABLE [Meetings].[ReminderLog] WITH CHECK CHECK CONSTRAINT [FK_ReminderLog_Event];

ALTER TABLE [Meetings].[ReminderLog] WITH CHECK CHECK CONSTRAINT [FK_ReminderLog_User];

ALTER TABLE [Meetings].[RSVP] WITH CHECK CHECK CONSTRAINT [FK_RSVP_Event];

ALTER TABLE [Meetings].[RSVP] WITH CHECK CHECK CONSTRAINT [FK_RSVP_User];

ALTER TABLE [Meetings].[RSVP] WITH CHECK CHECK CONSTRAINT [FK_RSVP_CircleMembership];

ALTER TABLE [Meetings].[Event] WITH CHECK CHECK CONSTRAINT [FK_Event_Circle];

ALTER TABLE [Meetings].[Event] WITH CHECK CHECK CONSTRAINT [FK_Event_User_CreatedBy];

ALTER TABLE [Meetings].[InvitationCode] WITH CHECK CHECK CONSTRAINT [FK_InvitationCode_Circle];

ALTER TABLE [Meetings].[InvitationCode] WITH CHECK CHECK CONSTRAINT [FK_InvitationCode_User_CreatedBy];

ALTER TABLE [Meetings].[InvitationCode] WITH CHECK CHECK CONSTRAINT [FK_InvitationCode_User_RedeemedBy];

ALTER TABLE [Meetings].[CircleMembership] WITH CHECK CHECK CONSTRAINT [FK_CircleMembership_Circle];

ALTER TABLE [Meetings].[CircleMembership] WITH CHECK CHECK CONSTRAINT [FK_CircleMembership_User];

ALTER TABLE [Meetings].[Circle] WITH CHECK CHECK CONSTRAINT [FK_Circle_User_CreatedByUserId];

GO

PRINT N'Update complete.';
GO
