CREATE TABLE [Meetings].[CircleMembership](
    [CircleMembershipId] [int] IDENTITY(1,1) NOT NULL,
    [CircleId] [int] NOT NULL,
    [UserId] [int] NOT NULL,
    [Role] [nvarchar](50) NOT NULL,
    [JoinedUtc] [datetime2](7) NOT NULL,
    [LeftUtc] [datetime2](7) NULL,
    CONSTRAINT [PK_CircleMembership] PRIMARY KEY CLUSTERED ([CircleMembershipId] ASC),
    CONSTRAINT [FK_CircleMembership_Circle] FOREIGN KEY ([CircleId]) REFERENCES [Meetings].[Circle] ([CircleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CircleMembership_User] FOREIGN KEY ([UserId]) REFERENCES [Meetings].[User] ([UserId])
)
GO

ALTER TABLE [Meetings].[CircleMembership] ADD CONSTRAINT [DF_CircleMembership_Role] DEFAULT (N'Member') FOR [Role]
GO
ALTER TABLE [Meetings].[CircleMembership] ADD CONSTRAINT [DF_CircleMembership_JoinedUtc] DEFAULT (getutcdate()) FOR [JoinedUtc]
GO

CREATE UNIQUE INDEX [IX_CircleMembership_CircleId_UserId] ON [Meetings].[CircleMembership] ([CircleId], [UserId])
GO