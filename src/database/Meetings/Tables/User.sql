CREATE TABLE [Dad].[User](
    [UserId] [int] IDENTITY(1,1) NOT NULL,
    [ExternalId] [nvarchar](200) NOT NULL,
    [DisplayName] [nvarchar](200) NOT NULL,
    [EmailAddress] [nvarchar](320) NOT NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserId] ASC)
)
GO

ALTER TABLE [Dad].[User] ADD CONSTRAINT [DF_User_IsActive] DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [Dad].[User] ADD CONSTRAINT [DF_User_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

CREATE UNIQUE INDEX [IX_User_ExternalId] ON [Dad].[User] ([ExternalId])
GO
CREATE UNIQUE INDEX [IX_User_EmailAddress] ON [Dad].[User] ([EmailAddress])
GO