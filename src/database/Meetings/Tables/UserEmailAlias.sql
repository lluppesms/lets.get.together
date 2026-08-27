CREATE TABLE [Meetings].[UserEmailAlias](
    [UserEmailAliasId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [EmailAddress] [nvarchar](320) NOT NULL,
    [NormalizedEmailAddress] [nvarchar](320) NOT NULL,
    [IsVerified] [bit] NOT NULL,
    [VerifiedUtc] [datetime2](7) NULL,
    [IsPrimary] [bit] NOT NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_UserEmailAlias] PRIMARY KEY CLUSTERED ([UserEmailAliasId] ASC),
    CONSTRAINT [FK_UserEmailAlias_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId]) ON DELETE CASCADE
)
GO

ALTER TABLE [Meetings].[UserEmailAlias] ADD CONSTRAINT [DF_UserEmailAlias_IsVerified] DEFAULT ((0)) FOR [IsVerified]
GO
ALTER TABLE [Meetings].[UserEmailAlias] ADD CONSTRAINT [DF_UserEmailAlias_IsPrimary] DEFAULT ((0)) FOR [IsPrimary]
GO
ALTER TABLE [Meetings].[UserEmailAlias] ADD CONSTRAINT [DF_UserEmailAlias_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

CREATE UNIQUE INDEX [UX_UserEmailAlias_NormalizedEmailAddress_Verified] ON [Meetings].[UserEmailAlias] ([NormalizedEmailAddress]) WHERE [IsVerified] = 1
GO
CREATE UNIQUE INDEX [UX_UserEmailAlias_UserId_Primary] ON [Meetings].[UserEmailAlias] ([UserId]) WHERE [IsPrimary] = 1
GO