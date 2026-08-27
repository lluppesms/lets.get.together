CREATE TABLE [Meetings].[UserIdentity](
    [UserIdentityId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [Provider] [int] NOT NULL,
    [Issuer] [nvarchar](500) NOT NULL,
    [Subject] [nvarchar](500) NOT NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_UserIdentity] PRIMARY KEY CLUSTERED ([UserIdentityId] ASC),
    CONSTRAINT [FK_UserIdentity_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId]) ON DELETE CASCADE
)
GO

ALTER TABLE [Meetings].[UserIdentity] ADD CONSTRAINT [DF_UserIdentity_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

CREATE UNIQUE INDEX [UX_UserIdentity_Provider_Issuer_Subject] ON [Meetings].[UserIdentity] ([Provider], [Issuer], [Subject])
GO