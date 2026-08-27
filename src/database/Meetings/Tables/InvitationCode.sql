CREATE TABLE [Meetings].[InvitationCode](
    [InvitationCodeId] [int] IDENTITY(1,1) NOT NULL,
    [CircleId] [int] NOT NULL,
    [Code] [nvarchar](64) NOT NULL,
    [CreatedByUserId] [int] NOT NULL,
    [RecipientEmailAddress] [nvarchar](320) NULL,
    [NormalizedRecipientEmailAddress] [nvarchar](320) NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    [ExpiresUtc] [datetime2](7) NULL,
    [RedeemedByUserId] [int] NULL,
    [RedeemedUtc] [datetime2](7) NULL,
    [RevokedUtc] [datetime2](7) NULL,
    CONSTRAINT [PK_InvitationCode] PRIMARY KEY CLUSTERED ([InvitationCodeId] ASC),
    CONSTRAINT [FK_InvitationCode_Circle] FOREIGN KEY ([CircleId]) REFERENCES [Meetings].[Circle] ([CircleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_InvitationCode_User_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Meetings].[User] ([UserId]),
    CONSTRAINT [FK_InvitationCode_User_RedeemedBy] FOREIGN KEY ([RedeemedByUserId]) REFERENCES [Meetings].[User] ([UserId]) ON DELETE SET NULL
)
GO

ALTER TABLE [Meetings].[InvitationCode] ADD CONSTRAINT [DF_InvitationCode_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

CREATE UNIQUE INDEX [IX_InvitationCode_Code] ON [Meetings].[InvitationCode] ([Code])
GO
CREATE INDEX [IX_InvitationCode_CircleId_CreatedUtc] ON [Meetings].[InvitationCode] ([CircleId], [CreatedUtc])
GO